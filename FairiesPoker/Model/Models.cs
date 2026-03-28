using Protocol.Dto;
using Protocol.Dto.Fight;
using System.Collections.Generic;

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

/// <summary>
/// 获取手牌回调
/// </summary>
/// <param name="cardList">手牌列表</param>
public delegate void GetCardsCallback(List<CardDto> cardList);

/// <summary>
/// 转换抢地主回调
/// </summary>
/// <param name="userId">下一个抢地主的玩家ID</param>
public delegate void TurnGrabCallback(int userId);

/// <summary>
/// 抢地主结果回调
/// </summary>
/// <param name="grabDto">抢地主数据</param>
public delegate void GrabLandlordCallback(GrabDto grabDto);

/// <summary>
/// 转换出牌回调
/// </summary>
/// <param name="userId">下一个出牌的玩家ID</param>
public delegate void TurnDealCallback(int userId);

/// <summary>
/// 出牌广播回调
/// </summary>
/// <param name="dealDto">出牌数据</param>
public delegate void DealBroadcastCallback(DealDto dealDto);

/// <summary>
/// 出牌响应回调
/// </summary>
/// <param name="result">结果（0成功，-1失败）</param>
public delegate void DealResponseCallback(int result);

/// <summary>
/// 不出响应回调
/// </summary>
/// <param name="result">结果</param>
public delegate void PassResponseCallback(int result);

/// <summary>
/// 游戏结束回调
/// </summary>
/// <param name="overDto">结束数据</param>
public delegate void GameOverCallback(OverDto overDto);

/// <summary>
/// 倍数变化回调
/// </summary>
/// <param name="multiple">倍数</param>
public delegate void MultipleChangeCallback(int multiple);

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

    /// <summary>
    /// 获取手牌回调
    /// </summary>
    public static event GetCardsCallback OnGetCards;

    /// <summary>
    /// 转换抢地主回调
    /// </summary>
    public static event TurnGrabCallback OnTurnGrab;

    /// <summary>
    /// 抢地主结果回调
    /// </summary>
    public static event GrabLandlordCallback OnGrabLandlord;

    /// <summary>
    /// 转换出牌回调
    /// </summary>
    public static event TurnDealCallback OnTurnDeal;

    /// <summary>
    /// 出牌广播回调
    /// </summary>
    public static event DealBroadcastCallback OnDealBroadcast;

    /// <summary>
    /// 出牌响应回调
    /// </summary>
    public static event DealResponseCallback OnDealResponse;

    /// <summary>
    /// 不出响应回调
    /// </summary>
    public static event PassResponseCallback OnPassResponse;

    /// <summary>
    /// 游戏结束回调
    /// </summary>
    public static event GameOverCallback OnGameOver;

    /// <summary>
    /// 倍数变化回调
    /// </summary>
    public static event MultipleChangeCallback OnMultipleChange;

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

    /// <summary>
    /// 触发获取手牌事件
    /// </summary>
    public static void TriggerGetCards(List<CardDto> cardList)
    {
        OnGetCards?.Invoke(cardList);
    }

    /// <summary>
    /// 触发转换抢地主事件
    /// </summary>
    public static void TriggerTurnGrab(int userId)
    {
        OnTurnGrab?.Invoke(userId);
    }

    /// <summary>
    /// 触发抢地主结果事件
    /// </summary>
    public static void TriggerGrabLandlord(GrabDto grabDto)
    {
        OnGrabLandlord?.Invoke(grabDto);
    }

    /// <summary>
    /// 触发转换出牌事件
    /// </summary>
    public static void TriggerTurnDeal(int userId)
    {
        OnTurnDeal?.Invoke(userId);
    }

    /// <summary>
    /// 触发出牌广播事件
    /// </summary>
    public static void TriggerDealBroadcast(DealDto dealDto)
    {
        OnDealBroadcast?.Invoke(dealDto);
    }

    /// <summary>
    /// 触发出牌响应事件
    /// </summary>
    public static void TriggerDealResponse(int result)
    {
        OnDealResponse?.Invoke(result);
    }

    /// <summary>
    /// 触发不出响应事件
    /// </summary>
    public static void TriggerPassResponse(int result)
    {
        OnPassResponse?.Invoke(result);
    }

    /// <summary>
    /// 触发游戏结束事件
    /// </summary>
    public static void TriggerGameOver(OverDto overDto)
    {
        OnGameOver?.Invoke(overDto);
    }

    /// <summary>
    /// 触发倍数变化事件
    /// </summary>
    public static void TriggerMultipleChange(int multiple)
    {
        OnMultipleChange?.Invoke(multiple);
    }
}