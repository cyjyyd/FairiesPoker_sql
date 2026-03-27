using FPServer.Cache;
using FPServer.Database;
using FPServer.Network;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using Protocol.Code;
using Protocol.Dto;

namespace FPServer.Handlers
{
    /// <summary>
    /// 头像处理器 - 处理头像上传
    /// </summary>
    public class AvatarHandler
    {
        private readonly MessageHandler _messageHandler;
        private readonly ILogger<AvatarHandler> _logger;
        private readonly OnlineUserCache _userCache;
        private const string AvatarDir = "avatars";
        private const string PendingDir = "avatars/pending";
        private const int MaxFileSize = 500 * 1024; // 500KB
        private const int OutputSize = 200; // 200x200

        /// <summary>
        /// 是否自动审核通过头像
        /// </summary>
        public bool AutoApprove { get; set; } = true;

        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };

        public AvatarHandler(MessageHandler messageHandler, ILoggerFactory loggerFactory, OnlineUserCache userCache, IConfiguration configuration = null)
        {
            _messageHandler = messageHandler;
            _logger = loggerFactory.CreateLogger<AvatarHandler>();
            _userCache = userCache;

            // 从配置读取自动审核设置
            if (configuration != null)
            {
                AutoApprove = configuration.GetValue<bool>("Avatar:AutoApprove", true);
            }

            // 确保目录存在
            EnsureDirectoriesExist();

            _logger.LogInformation("头像处理器已初始化，自动审核: {AutoApprove}", AutoApprove);
        }

        private void EnsureDirectoriesExist()
        {
            try
            {
                if (!Directory.Exists(AvatarDir))
                    Directory.CreateDirectory(AvatarDir);
                if (!Directory.Exists(PendingDir))
                    Directory.CreateDirectory(PendingDir);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建头像目录失败");
            }
        }

        /// <summary>
        /// 处理头像相关消息
        /// </summary>
        public void Handle(ClientConnection client, int subCode, object value)
        {
            switch (subCode)
            {
                case AvatarCode.UPLOAD_CREQ:
                    HandleUpload(client, value as AvatarDto);
                    break;
                case AvatarCode.GET_CREQ:
                    HandleGet(client, value as AvatarDto);
                    break;
                case AvatarCode.DOWNLOAD_CREQ:
                    HandleDownload(client, value as AvatarDto);
                    break;
                default:
                    _logger.LogWarning("未知头像操作码: {SubCode}", subCode);
                    break;
            }
        }

        /// <summary>
        /// 处理头像上传
        /// </summary>
        private async void HandleUpload(ClientConnection client, AvatarDto dto)
        {
            // 验证用户已登录
            if (client.UserId <= 0)
            {
                SendUploadResponse(client, -1, null, "请先登录");
                return;
            }

            if (dto == null || dto.ImageData == null || dto.ImageData.Length == 0)
            {
                SendUploadResponse(client, -2, null, "图片数据无效");
                return;
            }

            // 验证文件大小
            if (dto.ImageData.Length > MaxFileSize)
            {
                SendUploadResponse(client, -3, null, $"图片大小超过限制({MaxFileSize / 1024}KB)");
                return;
            }

            // 验证文件扩展名
            string extension = Path.GetExtension(dto.FileName ?? "").ToLower();
            if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
            {
                SendUploadResponse(client, -2, null, "不支持的图片格式，仅支持JPG/PNG/GIF");
                return;
            }

            try
            {
                // 生成唯一文件名
                string fileName = $"{client.UserId}_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid():N}{extension}";
                string avatarUrl = $"/avatars/{fileName}";

                if (AutoApprove)
                {
                    // 自动审核模式：直接保存到正式目录
                    string filePath = Path.Combine(AvatarDir, fileName);
                    await File.WriteAllBytesAsync(filePath, dto.ImageData);

                    // 更新用户头像URL
                    await DbHelper.Instance.ExecuteNonQueryAsync(
                        "UPDATE users SET avatar_url = @avatarUrl WHERE id = @userId",
                        new MySqlParameter("@avatarUrl", avatarUrl),
                        new MySqlParameter("@userId", client.UserId));

                    // 记录到审核表（已通过状态）
                    await DbHelper.Instance.ExecuteNonQueryAsync(
                        @"INSERT INTO user_avatars (user_id, file_path, upload_time, is_approved, review_time)
                          VALUES (@userId, @filePath, NOW(), 1, NOW())",
                        new MySqlParameter("@userId", client.UserId),
                        new MySqlParameter("@filePath", fileName));

                    _logger.LogInformation("用户 {UserId} 头像上传并自动审核通过: {FileName}", client.UserId, fileName);
                    SendUploadResponse(client, 0, avatarUrl, "头像上传成功");
                }
                else
                {
                    // 手动审核模式：保存到待审核目录
                    string filePath = Path.Combine(PendingDir, fileName);
                    await File.WriteAllBytesAsync(filePath, dto.ImageData);

                    // 插入审核记录
                    await DbHelper.Instance.ExecuteNonQueryAsync(
                        @"INSERT INTO user_avatars (user_id, file_path, upload_time, is_approved)
                          VALUES (@userId, @filePath, NOW(), 0)",
                        new MySqlParameter("@userId", client.UserId),
                        new MySqlParameter("@filePath", fileName));

                    _logger.LogInformation("用户 {UserId} 上传头像成功，等待审核: {FileName}", client.UserId, fileName);
                    SendUploadResponse(client, 0, null, "头像上传成功，等待审核");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "头像上传失败: {UserId}", client.UserId);
                SendUploadResponse(client, -4, null, "服务器错误，请稍后重试");
            }
        }

        /// <summary>
        /// 处理获取头像请求
        /// </summary>
        private async void HandleGet(ClientConnection client, AvatarDto dto)
        {
            if (dto == null || dto.UserId <= 0)
            {
                return;
            }

            try
            {
                // 查询用户的已审核头像
                var avatarUrl = await DbHelper.Instance.ExecuteScalarAsync(
                    "SELECT avatar_url FROM users WHERE id = @userId",
                    new MySqlParameter("@userId", dto.UserId));

                string url = avatarUrl?.ToString();

                var result = new AvatarResultDto
                {
                    Result = string.IsNullOrEmpty(url) ? -1 : 0,
                    AvatarUrl = url,
                    Message = string.IsNullOrEmpty(url) ? "未设置头像" : null
                };

                var msg = new SocketMsg(OpCode.AVATAR, AvatarCode.GET_SRES, result);
                _messageHandler.Send(client, msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取头像失败: {UserId}", dto.UserId);
            }
        }

        /// <summary>
        /// 处理下载头像请求
        /// </summary>
        private void HandleDownload(ClientConnection client, AvatarDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.AvatarUrl))
            {
                SendDownloadResponse(client, -1, null, "头像URL无效");
                return;
            }

            try
            {
                // 解析URL，格式如 /avatars/filename.jpg
                string avatarUrl = dto.AvatarUrl;
                if (avatarUrl.StartsWith("/"))
                {
                    avatarUrl = avatarUrl.Substring(1);
                }

                string filePath = Path.Combine(Directory.GetCurrentDirectory(), avatarUrl);

                if (!File.Exists(filePath))
                {
                    _logger.LogWarning("头像文件不存在: {FilePath}", filePath);
                    SendDownloadResponse(client, -2, null, "头像文件不存在");
                    return;
                }

                // 读取文件
                byte[] imageData = File.ReadAllBytes(filePath);
                string fileName = Path.GetFileName(filePath);

                var result = new AvatarDto
                {
                    ImageData = imageData,
                    FileName = fileName,
                    AvatarUrl = dto.AvatarUrl
                };

                var msg = new SocketMsg(OpCode.AVATAR, AvatarCode.DOWNLOAD_SRES, result);
                _messageHandler.Send(client, msg);

                _logger.LogDebug("发送头像数据: {FileName}, 大小: {Size}KB", fileName, imageData.Length / 1024);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "下载头像失败: {AvatarUrl}", dto.AvatarUrl);
                SendDownloadResponse(client, -3, null, "服务器错误");
            }
        }

        private void SendDownloadResponse(ClientConnection client, int result, byte[] imageData, string message)
        {
            var resultDto = new AvatarResultDto
            {
                Result = result,
                Message = message
            };

            var msg = new SocketMsg(OpCode.AVATAR, AvatarCode.DOWNLOAD_SRES, resultDto);
            _messageHandler.Send(client, msg);
        }

        private void SendUploadResponse(ClientConnection client, int result, string avatarUrl, string message)
        {
            var resultDto = new AvatarResultDto
            {
                Result = result,
                AvatarUrl = avatarUrl,
                Message = message
            };

            var msg = new SocketMsg(OpCode.AVATAR, AvatarCode.UPLOAD_SRES, resultDto);
            _messageHandler.Send(client, msg);
        }

        /// <summary>
        /// 审核通过头像（供管理员调用）
        /// </summary>
        public async Task<bool> ApproveAvatar(int avatarId, int reviewerId)
        {
            try
            {
                // 查询头像记录
                using var reader = await DbHelper.Instance.ExecuteReaderAsync(
                    "SELECT user_id, file_path FROM user_avatars WHERE id = @id AND is_approved = 0",
                    new MySqlParameter("@id", avatarId));

                if (!await reader.ReadAsync())
                    return false;

                int userId = reader.GetInt32("user_id");
                string filePath = reader.GetString("file_path");
                reader.Close();

                // 移动文件
                string sourcePath = Path.Combine(PendingDir, filePath);
                string destPath = Path.Combine(AvatarDir, filePath);
                if (File.Exists(sourcePath))
                {
                    File.Move(sourcePath, destPath, true);
                }

                // 更新审核状态
                await DbHelper.Instance.ExecuteNonQueryAsync(
                    "UPDATE user_avatars SET is_approved = 1, reviewed_by = @reviewerId, review_time = NOW() WHERE id = @id",
                    new MySqlParameter("@reviewerId", reviewerId),
                    new MySqlParameter("@id", avatarId));

                // 更新用户头像URL
                string avatarUrl = $"/avatars/{filePath}";
                await DbHelper.Instance.ExecuteNonQueryAsync(
                    "UPDATE users SET avatar_url = @avatarUrl WHERE id = @userId",
                    new MySqlParameter("@avatarUrl", avatarUrl),
                    new MySqlParameter("@userId", userId));

                _logger.LogInformation("头像审核通过: AvatarId={AvatarId}, UserId={UserId}", avatarId, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "审核头像失败: {AvatarId}", avatarId);
                return false;
            }
        }

        /// <summary>
        /// 审核拒绝头像（供管理员调用）
        /// </summary>
        public async Task<bool> RejectAvatar(int avatarId, int reviewerId, string note = null)
        {
            try
            {
                // 查询头像记录
                var filePathObj = await DbHelper.Instance.ExecuteScalarAsync(
                    "SELECT file_path FROM user_avatars WHERE id = @id AND is_approved = 0",
                    new MySqlParameter("@id", avatarId));

                if (filePathObj == null)
                    return false;

                string filePath = filePathObj.ToString();
                string fullPath = Path.Combine(PendingDir, filePath);

                // 删除文件
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }

                // 更新审核状态
                await DbHelper.Instance.ExecuteNonQueryAsync(
                    "UPDATE user_avatars SET is_approved = 2, reviewed_by = @reviewerId, review_time = NOW(), review_note = @note WHERE id = @id",
                    new MySqlParameter("@reviewerId", reviewerId),
                    new MySqlParameter("@note", note ?? ""),
                    new MySqlParameter("@id", avatarId));

                _logger.LogInformation("头像审核拒绝: AvatarId={AvatarId}", avatarId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "拒绝头像失败: {AvatarId}", avatarId);
                return false;
            }
        }

        /// <summary>
        /// 获取待审核头像列表
        /// </summary>
        public async Task<List<PendingAvatarDto>> GetPendingAvatarsAsync()
        {
            var list = new List<PendingAvatarDto>();

            try
            {
                using var reader = await DbHelper.Instance.ExecuteReaderAsync(
                    @"SELECT a.id, a.user_id, a.file_path, a.upload_time, u.username
                      FROM user_avatars a
                      LEFT JOIN users u ON a.user_id = u.id
                      WHERE a.is_approved = 0
                      ORDER BY a.upload_time DESC");

                while (await reader.ReadAsync())
                {
                    list.Add(new PendingAvatarDto
                    {
                        Id = reader.GetInt32("id"),
                        UserId = reader.GetInt32("user_id"),
                        FilePath = reader.GetString("file_path"),
                        UploadTime = reader.GetDateTime("upload_time"),
                        Username = reader["username"] == DBNull.Value ? "未知" : reader.GetString("username")
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取待审核头像列表失败");
            }

            return list;
        }

        /// <summary>
        /// 批量审核通过所有待审核头像
        /// </summary>
        public async Task<int> ApproveAllAsync(int reviewerId = 0)
        {
            var pendingList = await GetPendingAvatarsAsync();
            int count = 0;

            foreach (var avatar in pendingList)
            {
                if (await ApproveAvatar(avatar.Id, reviewerId))
                {
                    count++;
                }
            }

            return count;
        }
    }

    /// <summary>
    /// 待审核头像信息
    /// </summary>
    public class PendingAvatarDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string FilePath { get; set; }
        public DateTime UploadTime { get; set; }
    }
}