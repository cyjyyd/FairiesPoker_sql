using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Protocol.Code;
using Protocol.Dto;

namespace FairiesPoker.MG.Network.Impl
{

public class AvatarHandler : HandlerBase
{
    /// <summary>
    /// 头像缓存：URL -> Image
    /// </summary>
    private static Dictionary<string, Image> _avatarCache = new Dictionary<string, Image>();

    /// <summary>
    /// 正在下载中的头像URL
    /// </summary>
    private static HashSet<string> _downloadingUrls = new HashSet<string>();

    public override void OnReceive(int subCode, object value)
    {
        switch (subCode)
        {
            case AvatarCode.UPLOAD_SRES:
                HandleUploadResponse(value as AvatarResultDto);
                break;
            case AvatarCode.GET_SRES:
                HandleGetResponse(value as AvatarResultDto);
                break;
            case AvatarCode.DOWNLOAD_SRES:
                HandleDownloadResponse(value as AvatarDto);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 处理上传响应
    /// </summary>
    private void HandleUploadResponse(AvatarResultDto result)
    {
        if (result == null)
            return;

        bool success = result.Result == 0;
        string message = result.Message ?? (success ? "上传成功" : "上传失败");

        Models.TriggerAvatarUploadResult(success, message);
    }

    /// <summary>
    /// 处理获取头像响应
    /// </summary>
    private void HandleGetResponse(AvatarResultDto result)
    {
        if (result == null)
            return;

        // 可以在这里触发事件通知UI更新头像
    }

    /// <summary>
    /// 处理下载头像响应
    /// </summary>
    private void HandleDownloadResponse(AvatarDto dto)
    {
        if (dto == null || string.IsNullOrEmpty(dto.AvatarUrl))
            return;

        lock (_downloadingUrls)
        {
            _downloadingUrls.Remove(dto.AvatarUrl);
        }

        if (dto.ImageData != null && dto.ImageData.Length > 0)
        {
            try
            {
                using (var ms = new MemoryStream(dto.ImageData))
                {
                    var image = Image.FromStream(ms);
                    lock (_avatarCache)
                    {
                        _avatarCache[dto.AvatarUrl] = new Bitmap(image);
                    }
                }

                // 触发头像加载完成事件
                Models.TriggerAvatarLoaded(dto.AvatarUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"解析头像图片失败: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 请求下载头像
    /// </summary>
    public static void RequestDownloadAvatar(string avatarUrl)
    {
        if (string.IsNullOrEmpty(avatarUrl))
            return;

        // 检查缓存
        lock (_avatarCache)
        {
            if (_avatarCache.ContainsKey(avatarUrl))
            {
                Models.TriggerAvatarLoaded(avatarUrl);
                return;
            }
        }

        // 检查是否正在下载
        lock (_downloadingUrls)
        {
            if (_downloadingUrls.Contains(avatarUrl))
                return;
            _downloadingUrls.Add(avatarUrl);
        }

        // 发送下载请求
        var dto = AvatarDto.CreateDownloadRequest(avatarUrl);
        var msg = new SocketMsg(OpCode.AVATAR, AvatarCode.DOWNLOAD_CREQ, dto);
        NetManager.Instance?.Execute(0, msg);
    }

    /// <summary>
    /// 获取缓存的头像图片
    /// </summary>
    public static Image GetCachedAvatar(string avatarUrl)
    {
        if (string.IsNullOrEmpty(avatarUrl))
            return null;

        lock (_avatarCache)
        {
            if (_avatarCache.TryGetValue(avatarUrl, out var image))
                return image;
        }
        return null;
    }

    /// <summary>
    /// 清除头像缓存
    /// </summary>
    public static void ClearCache()
    {
        lock (_avatarCache)
        {
            foreach (var image in _avatarCache.Values)
            {
                image?.Dispose();
            }
            _avatarCache.Clear();
        }
    }
}}
