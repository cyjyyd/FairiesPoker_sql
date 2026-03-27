using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Protocol.Dto;

/// <summary>
/// 注册结果回调
/// </summary>
/// <param name="success">是否成功</param>
public delegate void RegisterResultCallback(bool success);

/// <summary>
/// 头像上传结果回调
/// </summary>
/// <param name="success">是否成功</param>
/// <param name="message">消息</param>
public delegate void AvatarUploadResultCallback(bool success, string message);

/// <summary>
/// 聊天消息回调
/// </summary>
/// <param name="chatDto">聊天数据</param>
public delegate void ChatMessageCallback(ChatDto chatDto);

/// <summary>
/// 房间更新回调
/// </summary>
/// <param name="room">房间数据</param>
public delegate void RoomUpdateCallback(RoomDto room);

/// <summary>
/// 房间列表更新回调
/// </summary>
/// <param name="rooms">房间列表</param>
public delegate void RoomListUpdateCallback(List<RoomDto> rooms);

/// <summary>
/// 匹配更新回调
/// </summary>
/// <param name="matchRoom">匹配房间数据</param>
public delegate void MatchUpdateCallback(MatchRoomDto matchRoom);

/// <summary>
/// 头像加载完成回调
/// </summary>
/// <param name="avatarUrl">头像URL</param>
public delegate void AvatarLoadedCallback(string avatarUrl);

static public class Models
{
    /// <summary>
    /// 游戏数据
    /// </summary>
    public static GameModel GameModel;

    /// <summary>
    /// 注册结果回调
    /// </summary>
    public static event RegisterResultCallback OnRegisterResult;

    /// <summary>
    /// 头像上传结果回调
    /// </summary>
    public static event AvatarUploadResultCallback OnAvatarUploadResult;

    /// <summary>
    /// 聊天消息回调
    /// </summary>
    public static event ChatMessageCallback OnChatMessage;

    /// <summary>
    /// 房间更新回调
    /// </summary>
    public static event RoomUpdateCallback OnRoomUpdate;

    /// <summary>
    /// 房间列表更新回调
    /// </summary>
    public static event RoomListUpdateCallback OnRoomListUpdate;

    /// <summary>
    /// 匹配更新回调
    /// </summary>
    public static event MatchUpdateCallback OnMatchUpdate;

    /// <summary>
    /// 头像加载完成回调
    /// </summary>
    public static event AvatarLoadedCallback OnAvatarLoaded;

    static Models()
    {
        GameModel = new GameModel();
    }

    /// <summary>
    /// 触发注册结果事件
    /// </summary>
    public static void TriggerRegisterResult(bool success)
    {
        OnRegisterResult?.Invoke(success);
    }

    /// <summary>
    /// 触发头像上传结果事件
    /// </summary>
    public static void TriggerAvatarUploadResult(bool success, string message)
    {
        OnAvatarUploadResult?.Invoke(success, message);
    }

    /// <summary>
    /// 触发聊天消息事件
    /// </summary>
    public static void TriggerChatMessage(ChatDto chatDto)
    {
        OnChatMessage?.Invoke(chatDto);
    }

    /// <summary>
    /// 触发房间更新事件
    /// </summary>
    public static void TriggerRoomUpdate(RoomDto room)
    {
        OnRoomUpdate?.Invoke(room);
    }

    /// <summary>
    /// 触发房间列表更新事件
    /// </summary>
    public static void TriggerRoomListUpdate(List<RoomDto> rooms)
    {
        OnRoomListUpdate?.Invoke(rooms);
    }

    /// <summary>
    /// 触发匹配更新事件
    /// </summary>
    public static void TriggerMatchUpdate(MatchRoomDto matchRoom)
    {
        OnMatchUpdate?.Invoke(matchRoom);
    }

    /// <summary>
    /// 触发头像加载完成事件
    /// </summary>
    public static void TriggerAvatarLoaded(string avatarUrl)
    {
        OnAvatarLoaded?.Invoke(avatarUrl);
    }
}