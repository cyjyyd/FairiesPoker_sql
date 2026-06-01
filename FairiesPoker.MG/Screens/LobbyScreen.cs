using FairiesPoker.MG.Core;
using FairiesPoker.MG.Network;
using FairiesPoker.MG.Network.Impl;
using FairiesPoker.MG.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Protocol.Code;
using Protocol.Dto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FairiesPoker.MG.Screens;

/// <summary>
/// 大厅屏幕 - 替代Lobby.cs (Phase 5 完整实现)
/// 玩家信息 + 房间列表/管理 + 聊天系统 + 匹配
/// </summary>
public class LobbyScreen : ScreenBase
{
    // 布局常量
    private const int TopHeight = 80;
    private const int LeftWidth = 300;
    private const int RightWidth = 450;
    private const int CenterX = LeftWidth;
    private const int RightX = 830;
    private const int PanelBottom = 720;
    private const int PanelHeight = 720 - TopHeight;
    private const int PlayerCardWidth = 160;
    private const int PlayerCardHeight = 190;
    private const int PlayerCardY = TopHeight + 85;
    private const int PlayerAvatarSize = 96;
    private const int EmojiColumns = 8;
    private const int EmojiRows = 8;
    private const int EmojiButtonSize = 30;
    private const int EmojiPadding = 2;

    private static readonly Rectangle TopAvatarRect = new(20, 10, 60, 60);
    private static readonly Rectangle LeftPlayerCardRect = new(CenterX + 25, PlayerCardY, PlayerCardWidth, PlayerCardHeight);
    private static readonly Rectangle SelfPlayerCardRect = new(CenterX + 205, PlayerCardY, PlayerCardWidth, PlayerCardHeight);
    private static readonly Rectangle RightPlayerCardRect = new(CenterX + 385, PlayerCardY, PlayerCardWidth, PlayerCardHeight);
    private static readonly Color MutedTextColor = new(190, 198, 210);
    private static readonly Color ReadyTextColor = new(112, 230, 140);

    // UI控件 - 顶部
    private readonly UILabel _playerName = new();
    private readonly UILabel _playerLevel = new();
    private readonly UILabel _playerBeans = new();
    private readonly UILabel _playerWins = new();
    private readonly UILabel _playerLosses = new();
    private readonly UIButton _settingsBtn = new();
    private readonly UIButton _logoutBtn = new();

    // UI控件 - 左侧房间列表
    private readonly UILabel _roomListTitle = new();
    private readonly UIListBox _roomList = new();
    private readonly UIButton _createRoomBtn = new();
    private readonly UIButton _joinRoomBtn = new();
    private readonly UIButton _quickMatchBtn = new();
    private readonly UILabel _matchStatus = new();

    // UI控件 - 右侧聊天
    private readonly UIButton _channelWorld = new();
    private readonly UIButton _channelRoom = new();
    private readonly UIButton _channelPrivate = new();
    private readonly UIListBox _chatList = new();
    private readonly UITextBox _chatInput = new();
    private readonly UILabel _chatInputLabel = new();
    private readonly UIButton _emojiBtn = new();
    private readonly UIButton _sendBtn = new();
    private readonly UILabel _onlineUsersTitle = new();
    private readonly UIListBox _onlineUserList = new();

    // UI控件 - 房间面板
    private readonly UIPanel _roomPanel = new();
    private readonly UILabel _roomTitle = new();
    private readonly UILabel _roomSubtitle = new();
    private readonly UIButton _leaveRoomBtn = new();
    private readonly UIButton _readyBtn = new();
    private readonly UIButton _startGameBtn = new();

    // 玩家信息卡片 (头像+名称+状态)
    private readonly UILabel _leftPlayerName = new();
    private readonly UILabel _leftPlayerStatus = new();
    private readonly UILabel _selfPlayerName = new();
    private readonly UILabel _selfPlayerStatus = new();
    private readonly UILabel _rightPlayerName = new();
    private readonly UILabel _rightPlayerStatus = new();

    private Texture2D? _backgroundTexture;
    private Texture2D? _defaultAvatarTexture;
    private readonly Dictionary<string, Texture2D> _avatarTextures = new();
    private readonly HashSet<string> _requestedAvatarUrls = new();
    private readonly Dictionary<string, Texture2D> _emojiTextures = new();
    private string? _playerAvatarUrl;
    private string? _leftPlayerAvatarUrl;
    private string? _selfPlayerAvatarUrl;
    private string? _rightPlayerAvatarUrl;

    // 数据
    private NetManager _netManager;
    private Point _mousePos;

    private int _currentChannel = ChatTypes.WORLD;
    private int _privateChatTargetUserId;
    private string _privateChatTargetUserName = "";

    private readonly List<string> _worldChatMessages = new();
    private readonly List<string> _roomChatMessages = new();
    private readonly List<string> _privateChatMessages = new();
    private readonly List<UserDto> _onlineUsersList = new();
    private readonly List<UserDto> _privateHistoryUsersList = new();
    private readonly List<UserDto> _privateUserListItems = new();

    private List<RoomDto> _roomListData = new();
    private int _selectedRoomIndex = -1;
    private bool _isInRoom;
    private bool _isReady;
    private bool _isHost;

    private float _refreshTimer;
    private float _networkTimer;
    private bool _showEmojiPicker;
    private Vector2 _emojiPickerPos;

    // Emoji映射 (64个常用emoji)
    private static readonly string[] Emojis = {
        "😀", "😁", "😂", "🤣", "😃", "😄", "😅", "😆",
        "😉", "😊", "😋", "😎", "😍", "🥰", "😘", "😗",
        "😐", "😑", "😶", "😏", "😒", "🙄", "😬", "😮",
        "😯", "😪", "😫", "😴", "😌", "😛", "😜", "🤪",
        "😝", "🤤", "😳", "😧", "😦", "😨", "😰", "😥",
        "😢", "😭", "😱", "😖", "😣", "😞", "😓", "😩",
        "😤", "😡", "🤬", "💀", "💩", "🤡", "👻", "👽",
        "👍", "👎", "👏", "🙏", "🤝", "💪", "✌️", "🤞"
    };

    public LobbyScreen(Game1 game, ScreenManager screenManager)
        : base(game, screenManager)
    {
    }

    public override void Initialize()
    {
        base.Initialize();
        _netManager = NetManager.Instance ?? new NetManager();
        Opacity = 0f;

        InitUI();
        InitRoomPanel();

        // 订阅网络事件
        Models.OnChatMessage += OnChatMessageReceived;
        Models.OnChatHistory += OnChatHistoryReceived;
        Models.OnRoomUpdate += OnRoomUpdateReceived;
        Models.OnRoomListUpdate += OnRoomListUpdateReceived;
        Models.OnMatchUpdate += OnMatchUpdateReceived;
        Models.OnGameStart += OnGameStartReceived;
        Models.OnOnlineUsers += OnOnlineUsersReceived;
        Models.OnPrivateUsers += OnPrivateUsersReceived;
        Models.OnAvatarLoaded += OnAvatarLoadedReceived;

        // 更新玩家信息
        UpdatePlayerInfo();

        // 请求房间列表
        RequestRoomList();
        if (Models.GameModel?.MatchRoomDto != null)
        {
            OnMatchUpdateReceived(Models.GameModel.MatchRoomDto);
        }

        // 请求今日聊天消息
        RequestTodayMessages();
        // 请求在线用户和私聊历史用户
        RequestOnlineUsers();
        RequestPrivateUsers();
    }

    public override void LoadContent()
    {
        base.LoadContent();

        _backgroundTexture = UIResourceManager.LoadResource("main seq.jpg")
            ?? UIResourceManager.LoadResource("ChatHall1.jpg")
            ?? UIResourceManager.LoadResource("backIMG.png");
        _defaultAvatarTexture = UIResourceManager.LoadResource("Pla.jpg");

        var user = Models.GameModel.UserDto;
        if (user != null)
        {
            SetAvatarUrl(ref _playerAvatarUrl, user.AvatarUrl);
        }
    }

    public override void UnloadContent()
    {
        Models.OnChatMessage -= OnChatMessageReceived;
        Models.OnChatHistory -= OnChatHistoryReceived;
        Models.OnRoomUpdate -= OnRoomUpdateReceived;
        Models.OnRoomListUpdate -= OnRoomListUpdateReceived;
        Models.OnMatchUpdate -= OnMatchUpdateReceived;
        Models.OnGameStart -= OnGameStartReceived;
        Models.OnOnlineUsers -= OnOnlineUsersReceived;
        Models.OnPrivateUsers -= OnPrivateUsersReceived;
        Models.OnAvatarLoaded -= OnAvatarLoadedReceived;

        foreach (var texture in _avatarTextures.Values)
            texture.Dispose();
        _avatarTextures.Clear();

        foreach (var texture in _emojiTextures.Values)
            texture.Dispose();
        _emojiTextures.Clear();
    }

    private void InitUI()
    {
        // === 顶部信息 ===
        _playerName.Position = new Vector2(100, 15);
        _playerName.Text = "玩家名称";
        _playerName.TextColor = Color.White;
        _playerName.Size = new Vector2(200, 25);

        _playerLevel.Position = new Vector2(100, 42);
        _playerLevel.Text = "Lv.1";
        _playerLevel.TextColor = Color.Yellow;
        _playerLevel.Size = new Vector2(80, 25);

        _playerBeans.Position = new Vector2(300, 15);
        _playerBeans.Text = "金币: 0";
        _playerBeans.TextColor = Color.LightGreen;
        _playerBeans.Size = new Vector2(150, 25);

        _playerWins.Position = new Vector2(300, 42);
        _playerWins.Text = "胜: 0";
        _playerWins.TextColor = Color.LightBlue;
        _playerWins.Size = new Vector2(80, 25);

        _playerLosses.Position = new Vector2(400, 42);
        _playerLosses.Text = "负: 0";
        _playerLosses.TextColor = Color.LightPink;
        _playerLosses.Size = new Vector2(80, 25);

        _settingsBtn.Position = new Vector2(1100, 20);
        _settingsBtn.Size = new Vector2(80, 35);
        _settingsBtn.Text = "设置";
        _settingsBtn.TextColor = Color.White;
        _settingsBtn.OnClick = OnSettings;

        _logoutBtn.Position = new Vector2(1190, 20);
        _logoutBtn.Size = new Vector2(80, 35);
        _logoutBtn.Text = "退出";
        _logoutBtn.TextColor = Color.LightCoral;
        _logoutBtn.OnClick = OnLogout;

        // === 左侧房间列表 ===
        _roomListTitle.Position = new Vector2(10, 90);
        _roomListTitle.Text = "房间列表";
        _roomListTitle.TextColor = Color.White;
        _roomListTitle.Size = new Vector2(100, 25);

        _roomList.Position = new Vector2(10, 120);
        _roomList.Size = new Vector2(280, 300);
        _roomList.Font = FontManager.Default;
        _roomList.ItemHeight = 30;
        _roomList.BackgroundColor = new Color(22, 26, 36, 180);
        _roomList.TextColor = new Color(242, 246, 255);
        _roomList.SelectedBackgroundColor = new Color(30, 105, 128, 220);
        _roomList.SelectedTextColor = Color.White;
        _roomList.ScrollBarColor = new Color(180, 205, 225, 190);
        _roomList.OnItemSelected = OnRoomSelected;

        _createRoomBtn.Position = new Vector2(10, 435);
        _createRoomBtn.Size = new Vector2(135, 35);
        _createRoomBtn.Text = "创建房间";
        _createRoomBtn.TextColor = Color.White;
        _createRoomBtn.OnClick = OnCreateRoom;

        _joinRoomBtn.Position = new Vector2(155, 435);
        _joinRoomBtn.Size = new Vector2(135, 35);
        _joinRoomBtn.Text = "加入房间";
        _joinRoomBtn.TextColor = Color.White;
        _joinRoomBtn.OnClick = OnJoinRoom;

        _quickMatchBtn.Position = new Vector2(10, 485);
        _quickMatchBtn.Size = new Vector2(280, 55);
        _quickMatchBtn.Text = "快速匹配";
        _quickMatchBtn.TextColor = Color.White;
        _quickMatchBtn.OnClick = OnQuickMatch;

        _matchStatus.Position = new Vector2(10, 550);
        _matchStatus.Text = "";
        _matchStatus.TextColor = Color.Yellow;
        _matchStatus.Size = new Vector2(280, 25);

        // === 右侧聊天 ===
        int chatY = 90;
        _channelWorld.Position = new Vector2(RightX + 10, chatY);
        _channelWorld.Size = new Vector2(130, 30);
        _channelWorld.Text = "世界频道";
        _channelWorld.TextColor = Color.White;
        _channelWorld.OnClick = () => SwitchChannel(ChatTypes.WORLD);

        _channelRoom.Position = new Vector2(RightX + 150, chatY);
        _channelRoom.Size = new Vector2(130, 30);
        _channelRoom.Text = "房间频道";
        _channelRoom.TextColor = Color.White;
        _channelRoom.OnClick = () => SwitchChannel(ChatTypes.ROOM);

        _channelPrivate.Position = new Vector2(RightX + 290, chatY);
        _channelPrivate.Size = new Vector2(130, 30);
        _channelPrivate.Text = "私聊";
        _channelPrivate.TextColor = Color.White;
        _channelPrivate.OnClick = () => SwitchChannel(ChatTypes.PRIVATE);

        _chatList.Position = new Vector2(RightX + 10, chatY + 40);
        _chatList.Size = new Vector2(RightWidth - 20, 355);
        _chatList.Font = FontManager.Default;
        _chatList.ItemHeight = 22;
        _chatList.BackgroundColor = new Color(22, 26, 36, 172);
        _chatList.TextColor = new Color(242, 246, 255);
        _chatList.SelectedBackgroundColor = new Color(55, 70, 88, 190);
        _chatList.SelectedTextColor = Color.White;
        _chatList.ScrollBarColor = new Color(180, 205, 225, 170);

        _onlineUsersTitle.Position = new Vector2(RightX + 10, chatY + 405);
        _onlineUsersTitle.Text = "在线用户 / 私聊对象";
        _onlineUsersTitle.TextColor = new Color(235, 241, 255);
        _onlineUsersTitle.Size = new Vector2(RightWidth - 20, 22);

        _onlineUserList.Position = new Vector2(RightX + 10, chatY + 430);
        _onlineUserList.Size = new Vector2(RightWidth - 20, 130);
        _onlineUserList.Font = FontManager.Default;
        _onlineUserList.ItemHeight = 24;
        _onlineUserList.BackgroundColor = new Color(22, 26, 36, 172);
        _onlineUserList.TextColor = new Color(242, 246, 255);
        _onlineUserList.SelectedBackgroundColor = new Color(30, 105, 128, 220);
        _onlineUserList.SelectedTextColor = Color.White;
        _onlineUserList.ScrollBarColor = new Color(180, 205, 225, 170);
        _onlineUserList.OnItemSelected = OnPrivateUserSelected;

        _chatInput.Position = new Vector2(RightX + 10, PanelBottom - 55);
        _chatInput.Size = new Vector2(RightWidth - 138, 30);
        _chatInput.Placeholder = "";
        _chatInput.TextColor = Color.White;
        _chatInput.PlaceholderColor = new Color(220, 226, 238);
        _chatInput.BackgroundColor = new Color(20, 24, 32, 225);
        _chatInput.BorderColor = new Color(185, 205, 225, 170);
        _chatInput.PreferIme = true;

        _chatInputLabel.Position = new Vector2(RightX + 14, PanelBottom - 50);
        _chatInputLabel.Text = "输入消息...";
        _chatInputLabel.TextColor = new Color(220, 226, 238);
        _chatInputLabel.Size = new Vector2(100, 20);

        _emojiBtn.Position = new Vector2(RightX + RightWidth - 118, PanelBottom - 55);
        _emojiBtn.Size = new Vector2(60, 30);
        _emojiBtn.Text = "表情";
        _emojiBtn.TextColor = Color.White;
        _emojiBtn.OnClick = OnEmoji;

        _sendBtn.Position = new Vector2(RightX + RightWidth - 50, PanelBottom - 55);
        _sendBtn.Size = new Vector2(40, 30);
        _sendBtn.Text = "发送";
        _sendBtn.TextColor = Color.White;
        _sendBtn.OnClick = OnSendChat;

        UpdateChannelButtons();
    }

    private void InitRoomPanel()
    {
        _roomPanel.BackgroundColor = new Color(30, 35, 45, 145);
        _roomPanel.Position = new Vector2(CenterX, TopHeight);
        _roomPanel.Size = new Vector2(RightX - CenterX, PanelHeight);
        _roomPanel.Visible = false;

        _roomTitle.Position = new Vector2(CenterX + 20, TopHeight + 15);
        _roomTitle.Text = "房间";
        _roomTitle.TextColor = Color.White;
        _roomTitle.Size = new Vector2(200, 25);

        _roomSubtitle.Position = new Vector2(CenterX + 20, TopHeight + 42);
        _roomSubtitle.Text = "";
        _roomSubtitle.TextColor = new Color(205, 214, 228);
        _roomSubtitle.Size = new Vector2(200, 25);

        _leaveRoomBtn.Position = new Vector2(CenterX + 400, TopHeight + 15);
        _leaveRoomBtn.Size = new Vector2(80, 30);
        _leaveRoomBtn.Text = "离开";
        _leaveRoomBtn.TextColor = Color.LightCoral;
        _leaveRoomBtn.OnClick = OnLeaveRoom;

        // 玩家信息卡片 - 左
        _leftPlayerName.Position = new Vector2(LeftPlayerCardRect.X, LeftPlayerCardRect.Y + 122);
        _leftPlayerName.Text = "等待加入...";
        _leftPlayerName.TextColor = MutedTextColor;
        _leftPlayerName.Size = new Vector2(PlayerCardWidth, 25);
        _leftPlayerName.TextAlignment = UILabel.AlignmentType.Center;

        _leftPlayerStatus.Position = new Vector2(LeftPlayerCardRect.X, LeftPlayerCardRect.Y + 150);
        _leftPlayerStatus.Text = "";
        _leftPlayerStatus.TextColor = MutedTextColor;
        _leftPlayerStatus.Size = new Vector2(PlayerCardWidth, 20);
        _leftPlayerStatus.TextAlignment = UILabel.AlignmentType.Center;

        // 玩家信息卡片 - 中(自己)
        _selfPlayerName.Position = new Vector2(SelfPlayerCardRect.X, SelfPlayerCardRect.Y + 122);
        _selfPlayerName.Text = "我";
        _selfPlayerName.TextColor = Color.White;
        _selfPlayerName.Size = new Vector2(PlayerCardWidth, 25);
        _selfPlayerName.TextAlignment = UILabel.AlignmentType.Center;

        _selfPlayerStatus.Position = new Vector2(SelfPlayerCardRect.X, SelfPlayerCardRect.Y + 150);
        _selfPlayerStatus.Text = "未准备";
        _selfPlayerStatus.TextColor = MutedTextColor;
        _selfPlayerStatus.Size = new Vector2(PlayerCardWidth, 20);
        _selfPlayerStatus.TextAlignment = UILabel.AlignmentType.Center;

        // 玩家信息卡片 - 右
        _rightPlayerName.Position = new Vector2(RightPlayerCardRect.X, RightPlayerCardRect.Y + 122);
        _rightPlayerName.Text = "等待加入...";
        _rightPlayerName.TextColor = MutedTextColor;
        _rightPlayerName.Size = new Vector2(PlayerCardWidth, 25);
        _rightPlayerName.TextAlignment = UILabel.AlignmentType.Center;

        _rightPlayerStatus.Position = new Vector2(RightPlayerCardRect.X, RightPlayerCardRect.Y + 150);
        _rightPlayerStatus.Text = "";
        _rightPlayerStatus.TextColor = MutedTextColor;
        _rightPlayerStatus.Size = new Vector2(PlayerCardWidth, 20);
        _rightPlayerStatus.TextAlignment = UILabel.AlignmentType.Center;

        _readyBtn.Position = new Vector2(CenterX + 220, TopHeight + 280);
        _readyBtn.Size = new Vector2(160, 45);
        _readyBtn.Text = "准备";
        _readyBtn.TextColor = Color.White;
        _readyBtn.OnClick = OnReady;

        _startGameBtn.Position = new Vector2(CenterX + 220, TopHeight + 340);
        _startGameBtn.Size = new Vector2(160, 45);
        _startGameBtn.Text = "开始游戏";
        _startGameBtn.TextColor = Color.LightGreen;
        _startGameBtn.OnClick = OnStartGame;
        _startGameBtn.Visible = false;
    }

    private void OnRoomSelected(int index)
    {
        _selectedRoomIndex = index;
    }

    private void SwitchChannel(int channel)
    {
        _currentChannel = channel;
        UpdateChannelButtons();
        RefreshChatList();

        // 私聊时检查是否已选择目标
        if (channel == ChatTypes.PRIVATE && _privateChatTargetUserId == 0)
        {
            RequestOnlineUsers();
            AddSystemChatMessage("请在下方在线用户列表选择私聊对象", ChatTypes.PRIVATE);
        }
    }

    private void UpdateChannelButtons()
    {
        UpdatePrivateChannelText();
        _channelWorld.TextColor = _currentChannel == ChatTypes.WORLD ? Color.Yellow : Color.White;
        _channelRoom.TextColor = _currentChannel == ChatTypes.ROOM ? Color.Yellow : Color.White;
        _channelPrivate.TextColor = _currentChannel == ChatTypes.PRIVATE ? Color.Yellow : Color.White;
    }

    private void UpdatePrivateChannelText()
    {
        _channelPrivate.Text = _privateChatTargetUserId > 0
            ? $"私聊({CompactName(_privateChatTargetUserName, 5)})"
            : "私聊";
    }

    private static string CompactName(string name, int maxTextElements)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "";

        var elements = System.Globalization.StringInfo.ParseCombiningCharacters(name);
        if (elements.Length <= maxTextElements)
            return name;

        int cutIndex = elements[maxTextElements];
        return name.Substring(0, cutIndex) + "...";
    }

    private void RefreshChatList()
    {
        _chatList.Clear();
        var messages = _currentChannel switch
        {
            ChatTypes.WORLD => _worldChatMessages,
            ChatTypes.ROOM => _roomChatMessages,
            ChatTypes.PRIVATE => _privateChatMessages,
            _ => _worldChatMessages
        };
        foreach (var msg in messages)
            _chatList.AddItem(msg);
        _chatList.SelectedIndex = _chatList.Items.Count > 0 ? _chatList.Items.Count - 1 : -1;
        _chatList.ScrollOffset = Math.Max(0, _chatList.Items.Count - (int)(_chatList.Size.Y / _chatList.ItemHeight));
    }

    private void AddChatMessage(ChatDto dto)
    {
        string userName = dto.UserName ?? $"玩家{dto.UserId}";
        string time = FormatTimestamp(dto.Timestamp);
        string displayMsg = $"[{time}] {userName}: {dto.Text}";

        switch (dto.ChatType)
        {
            case ChatTypes.WORLD:
                _worldChatMessages.Add(displayMsg);
                if (_worldChatMessages.Count > 200) _worldChatMessages.RemoveAt(0);
                break;
            case ChatTypes.ROOM:
                _roomChatMessages.Add(displayMsg);
                if (_roomChatMessages.Count > 100) _roomChatMessages.RemoveAt(0);
                break;
            case ChatTypes.PRIVATE:
                int myId = Models.GameModel.UserDto?.Id ?? 0;
                // 私聊消息: 只保存与当前目标相关的
                if (dto.UserId == _privateChatTargetUserId || dto.TargetUserId == _privateChatTargetUserId ||
                    dto.UserId == myId || dto.TargetUserId == myId)
                {
                    // 收到私聊消息时自动切换目标
                    if (dto.UserId != myId && dto.UserId != _privateChatTargetUserId)
                    {
                        SetPrivateTarget(new UserDto(dto.UserId, dto.UserName ?? $"玩家{dto.UserId}", 0, 0, 0, 0, 1, 0),
                            switchToPrivate: false,
                            clearMessages: true,
                            requestHistory: false);
                    }
                    else if (dto.TargetUserId != myId && dto.TargetUserId > 0)
                    {
                        AddPrivateHistoryUser(dto.TargetUserId, _privateChatTargetUserName);
                    }
                    _privateChatMessages.Add(displayMsg);
                    if (_privateChatMessages.Count > 200) _privateChatMessages.RemoveAt(0);
                }
                break;
        }

        // 刷新当前频道
        if (_currentChannel == dto.ChatType)
            RefreshChatList();
    }

    private void AddSystemChatMessage(string text, int? channel = null)
    {
        int targetChannel = channel ?? _currentChannel;
        string msg = $"[系统] {text}";
        var target = targetChannel switch
        {
            ChatTypes.WORLD => _worldChatMessages,
            ChatTypes.PRIVATE => _privateChatMessages,
            _ => _roomChatMessages
        };

        if (target.Count == 0 || target[^1] != msg)
            target.Add(msg);

        if (_currentChannel == targetChannel)
            RefreshChatList();
    }

    private static string FormatTimestamp(long timestamp)
    {
        try
        {
            var dt = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).LocalDateTime;
            return dt.ToString("HH:mm");
        }
        catch
        {
            return "";
        }
    }

    // === 网络事件处理 ===

    private void OnChatMessageReceived(ChatDto dto)
    {
        AddChatMessage(dto);
    }

    private void OnChatHistoryReceived(List<ChatDto> messages, bool isAppend)
    {
        if (messages == null || messages.Count == 0) return;

        var newMessages = new List<string>();
        foreach (var dto in messages)
        {
            string userName = dto.UserName ?? $"玩家{dto.UserId}";
            string time = FormatTimestamp(dto.Timestamp);
            newMessages.Add($"[{time}] {userName}: {dto.Text}");
        }

        if (isAppend)
        {
            // 历史消息追加到顶部
            if (_currentChannel == ChatTypes.WORLD)
            {
                _worldChatMessages.InsertRange(0, newMessages);
                RefreshChatList();
            }
        }
        else
        {
            // 新消息(今日消息)直接替换
            var target = _currentChannel switch
            {
                ChatTypes.WORLD => _worldChatMessages,
                ChatTypes.ROOM => _roomChatMessages,
                ChatTypes.PRIVATE => _privateChatMessages,
                _ => _worldChatMessages
            };
            target.Clear();
            target.AddRange(newMessages);
            RefreshChatList();
        }
    }

    private void OnRoomUpdateReceived(RoomDto room)
    {
        // 房间信息更新 (进入房间后)
        _isInRoom = true;
        _roomPanel.Visible = true;
        _roomTitle.Text = room.RoomName ?? "房间";
        _roomSubtitle.Text = $"房主: {GetHostName(room)} | {room.PlayerCount}/3";

        UpdateRoomPlayers(room);
    }

    private void OnRoomListUpdateReceived(List<RoomDto> rooms)
    {
        _roomListData = rooms ?? new List<RoomDto>();
        _roomList.Clear();
        for (int i = 0; i < _roomListData.Count; i++)
        {
            var r = _roomListData[i];
            string status = r.Status == RoomStatus.WAITING ? "等待中" : "游戏中";
            _roomList.AddItem($"{r.RoomName} ({r.PlayerCount}/3) - {status}");
        }

        SyncCurrentRoomWithRoomList(_roomListData);
    }

    private void OnMatchUpdateReceived(MatchRoomDto matchRoom)
    {
        if (matchRoom == null)
        {
            ClearCurrentRoom();
            return;
        }

        Models.GameModel.MatchRoomDto = matchRoom;
        _isInRoom = true;
        _isHost = matchRoom.HostId == Models.GameModel.UserDto?.Id;

        // 更新房间信息
        _roomPanel.Visible = true;
        _roomTitle.Text = matchRoom.RoomName ?? "匹配房间";
        _roomSubtitle.Text = $"房间号: {matchRoom.RoomId} | {matchRoom.UIdList.Count}/3";

        // 更新玩家
        int myId = Models.GameModel.UserDto?.Id ?? 0;
        matchRoom.ResetPosition(myId);

        // 左侧玩家
        if (matchRoom.LeftId > 0 && matchRoom.UIdUserDict.TryGetValue(matchRoom.LeftId, out var leftUser))
        {
            _leftPlayerName.Text = leftUser.Name;
            _leftPlayerName.TextColor = Color.White;
            bool ready = matchRoom.IsReady(matchRoom.LeftId);
            bool disconnected = matchRoom.IsDisconnected(matchRoom.LeftId);
            _leftPlayerStatus.Text = disconnected ? "已断线" : (ready ? "已准备" : "未准备");
            _leftPlayerStatus.TextColor = disconnected ? Color.LightCoral : (ready ? ReadyTextColor : MutedTextColor);
            SetAvatarUrl(ref _leftPlayerAvatarUrl, leftUser.AvatarUrl);
        }
        else
        {
            _leftPlayerName.Text = "等待加入...";
            _leftPlayerName.TextColor = MutedTextColor;
            _leftPlayerStatus.Text = "";
            _leftPlayerAvatarUrl = null;
        }

        // 自己
        var selfUser = matchRoom.UIdUserDict.TryGetValue(myId, out var me)
            ? me
            : Models.GameModel.UserDto;
        _selfPlayerName.Text = selfUser?.Name ?? "我";
        _isReady = matchRoom.IsReady(myId);
        bool selfDisconnected = matchRoom.IsDisconnected(myId);
        _selfPlayerStatus.Text = selfDisconnected ? "已断线" : (_isReady ? "已准备" : "未准备");
        _selfPlayerStatus.TextColor = selfDisconnected ? Color.LightCoral : (_isReady ? ReadyTextColor : MutedTextColor);
        _readyBtn.Text = _isReady ? "取消准备" : "准备";
        SetAvatarUrl(ref _selfPlayerAvatarUrl, selfUser?.AvatarUrl);

        // 右侧玩家
        if (matchRoom.RightId > 0 && matchRoom.UIdUserDict.TryGetValue(matchRoom.RightId, out var rightUser))
        {
            _rightPlayerName.Text = rightUser.Name;
            _rightPlayerName.TextColor = Color.White;
            bool ready = matchRoom.IsReady(matchRoom.RightId);
            bool disconnected = matchRoom.IsDisconnected(matchRoom.RightId);
            _rightPlayerStatus.Text = disconnected ? "已断线" : (ready ? "已准备" : "未准备");
            _rightPlayerStatus.TextColor = disconnected ? Color.LightCoral : (ready ? ReadyTextColor : MutedTextColor);
            SetAvatarUrl(ref _rightPlayerAvatarUrl, rightUser.AvatarUrl);
        }
        else
        {
            _rightPlayerName.Text = "等待加入...";
            _rightPlayerName.TextColor = MutedTextColor;
            _rightPlayerStatus.Text = "";
            _rightPlayerAvatarUrl = null;
        }

        // 房主显示开始按钮；服务端要求满3人且全部准备才会接受 START_CREQ。
        bool isFull = matchRoom.UIdList.Count >= 3;
        bool allReady = isFull && matchRoom.UIdList.All(id => matchRoom.ReadyUIdList.Contains(id));
        _startGameBtn.Visible = _isHost && !matchRoom.IsQuickMatch;
        _startGameBtn.Enabled = allReady;
        _startGameBtn.Text = !isFull ? "等待玩家..." : (allReady ? "开始游戏" : "等待准备...");
    }

    private void SyncCurrentRoomWithRoomList(List<RoomDto> rooms)
    {
        var matchRoom = Models.GameModel?.MatchRoomDto;
        if (!_isInRoom || matchRoom == null || matchRoom.IsQuickMatch || string.IsNullOrEmpty(matchRoom.RoomId))
            return;

        if (!rooms.Any(room => room.RoomId == matchRoom.RoomId))
        {
            ClearCurrentRoom("房间已不存在");
        }
    }

    private void ClearCurrentRoom(string statusText = "")
    {
        _isInRoom = false;
        _isReady = false;
        _isHost = false;
        _roomPanel.Visible = false;
        _startGameBtn.Visible = false;
        _startGameBtn.Enabled = false;
        if (Models.GameModel != null)
            Models.GameModel.MatchRoomDto = null;

        _matchStatus.Text = statusText;
        _matchStatus.TextColor = string.IsNullOrEmpty(statusText) ? Color.White : Color.Yellow;
        ClearRoomAvatarUrls();
    }

    private void OnGameStartReceived()
    {
        // 游戏开始，进入游戏屏幕
        ScreenManager.Replace(new GameScreen(Game, ScreenManager, true));
    }

    private void OnOnlineUsersReceived(List<UserDto> users)
    {
        _onlineUsersList.Clear();
        if (users != null)
            _onlineUsersList.AddRange(users);

        RefreshPrivateUserList();
        RequestPrivateUsers();
    }

    private void OnPrivateUsersReceived(List<UserDto> users)
    {
        _privateHistoryUsersList.Clear();
        if (users != null)
            _privateHistoryUsersList.AddRange(users);

        RefreshPrivateUserList();
    }

    private void RefreshPrivateUserList()
    {
        _privateUserListItems.Clear();
        _onlineUserList.Clear();

        int myId = Models.GameModel.UserDto?.Id ?? 0;
        var added = new HashSet<int>();
        int selectedIndex = -1;

        foreach (var user in _onlineUsersList.Where(u => u != null && u.Id != myId).OrderBy(u => u.Name))
        {
            if (!added.Add(user.Id))
                continue;

            AddPrivateUserListItem(user, online: true, ref selectedIndex);
        }

        foreach (var user in _privateHistoryUsersList.Where(u => u != null && u.Id != myId).OrderBy(u => u.Name))
        {
            if (!added.Add(user.Id))
                continue;

            bool online = _onlineUsersList.Any(u => u.Id == user.Id);
            AddPrivateUserListItem(user, online, ref selectedIndex);
        }

        if (_onlineUserList.Items.Count == 0)
        {
            _onlineUserList.AddItem("暂无可私聊用户");
            _onlineUserList.SelectedIndex = -1;
            return;
        }

        _onlineUserList.SelectedIndex = selectedIndex;
        if (selectedIndex >= 0)
        {
            int visibleItems = Math.Max(1, (int)(_onlineUserList.Size.Y / _onlineUserList.ItemHeight));
            if (selectedIndex < _onlineUserList.ScrollOffset)
                _onlineUserList.ScrollOffset = selectedIndex;
            else if (selectedIndex >= _onlineUserList.ScrollOffset + visibleItems)
                _onlineUserList.ScrollOffset = Math.Max(0, selectedIndex - visibleItems + 1);
        }
    }

    private void AddPrivateUserListItem(UserDto user, bool online, ref int selectedIndex)
    {
        if (user.Id == _privateChatTargetUserId)
            selectedIndex = _privateUserListItems.Count;

        _privateUserListItems.Add(user);
        string selectedPrefix = user.Id == _privateChatTargetUserId ? "→ " : "";
        string status = online ? "[在线]" : "[离线]";
        _onlineUserList.AddItem($"{selectedPrefix}{status} Lv.{user.Lv} {user.Name}");
    }

    private void OnPrivateUserSelected(int index)
    {
        if (index < 0 || index >= _privateUserListItems.Count)
            return;

        SetPrivateTarget(_privateUserListItems[index],
            switchToPrivate: true,
            clearMessages: true,
            requestHistory: true);
    }

    private void SetPrivateTarget(UserDto user, bool switchToPrivate, bool clearMessages, bool requestHistory)
    {
        if (user == null || user.Id <= 0 || user.Id == Models.GameModel.UserDto?.Id)
            return;

        bool targetChanged = _privateChatTargetUserId != user.Id;
        _privateChatTargetUserId = user.Id;
        _privateChatTargetUserName = string.IsNullOrWhiteSpace(user.Name) ? $"玩家{user.Id}" : user.Name;
        AddPrivateHistoryUser(user.Id, _privateChatTargetUserName);
        UpdatePrivateChannelText();
        RefreshPrivateUserList();

        if (clearMessages && targetChanged)
            _privateChatMessages.Clear();

        if (switchToPrivate)
            SwitchChannel(ChatTypes.PRIVATE);
        else if (_currentChannel == ChatTypes.PRIVATE)
            RefreshChatList();

        if (requestHistory)
            RequestPrivateHistory(user.Id);
    }

    private void AddPrivateHistoryUser(int userId, string userName)
    {
        if (userId <= 0 || userId == Models.GameModel.UserDto?.Id)
            return;

        if (_privateHistoryUsersList.Any(u => u.Id == userId))
            return;

        var onlineUser = _onlineUsersList.FirstOrDefault(u => u.Id == userId);
        _privateHistoryUsersList.Add(onlineUser ?? new UserDto(userId, string.IsNullOrWhiteSpace(userName) ? $"玩家{userId}" : userName, 0, 0, 0, 0, 1, 0));
    }

    private static string GetHostName(RoomDto room)
    {
        if (room.Players != null && room.Players.Count > 0)
        {
            var host = room.Players.FirstOrDefault(p => p.Id == room.HostId);
            return host?.Name ?? "未知";
        }
        return "未知";
    }

    private void UpdateRoomPlayers(RoomDto room)
    {
        if (room.Players != null)
        {
            for (int i = 0; i < room.Players.Count && i < 3; i++)
            {
                var player = room.Players[i];
                int myId = Models.GameModel.UserDto?.Id ?? 0;
                if (player.Id == myId)
                {
                    _selfPlayerName.Text = player.Name;
                    _selfPlayerStatus.Text = room.Status == RoomStatus.WAITING ? "等待中" : "游戏中";
                    SetAvatarUrl(ref _selfPlayerAvatarUrl, player.AvatarUrl);
                }
                else if (i == 0)
                {
                    _leftPlayerName.Text = player.Name;
                    _leftPlayerStatus.Text = "等待中";
                    SetAvatarUrl(ref _leftPlayerAvatarUrl, player.AvatarUrl);
                }
                else
                {
                    _rightPlayerName.Text = player.Name;
                    _rightPlayerStatus.Text = "等待中";
                    SetAvatarUrl(ref _rightPlayerAvatarUrl, player.AvatarUrl);
                }
            }
        }
    }

    // === 按钮事件 ===

    private void UpdatePlayerInfo()
    {
        var user = Models.GameModel.UserDto;
        if (user == null) return;

        _playerName.Text = user.Name;
        _playerLevel.Text = $"Lv.{user.Lv}";
        _playerBeans.Text = $"金币: {user.Been}";
        _playerWins.Text = $"胜: {user.WinCount}";
        _playerLosses.Text = $"负: {user.LoseCount}";
        SetAvatarUrl(ref _playerAvatarUrl, user.AvatarUrl);
    }

    private void OnSettings()
    {
        // TODO: 打开设置屏幕
    }

    private void OnLogout()
    {
        _netManager.Disconnect();
        ScreenManager.Pop();
    }

    private void RequestRoomList()
    {
        var msg = new SocketMsg(OpCode.MATCH, RoomCode.GET_ROOMS_CREQ, null);
        _netManager.Execute(0, msg);
    }

    private void RequestTodayMessages()
    {
        var requestDto = new HistoryRequestDto(ChatTypes.WORLD, limit: 50);
        var msg = new SocketMsg(OpCode.CHAT, ChatCode.GET_HISTORY_CREQ, requestDto);
        _netManager.Execute(0, msg);
    }

    private void RequestOnlineUsers()
    {
        if (!_netManager.IsConnected)
            return;

        var msg = new SocketMsg(OpCode.USER, UserCode.GET_ONLINE_USERS_CREQ, null);
        _netManager.Execute(0, msg);
    }

    private void RequestPrivateUsers()
    {
        if (!_netManager.IsConnected)
            return;

        var msg = new SocketMsg(OpCode.CHAT, ChatCode.GET_PRIVATE_USERS_CREQ, null);
        _netManager.Execute(0, msg);
    }

    private void RequestPrivateHistory(int targetUserId)
    {
        if (!_netManager.IsConnected || targetUserId <= 0)
            return;

        var requestDto = new HistoryRequestDto(ChatTypes.PRIVATE, targetUserId, 0, 50);
        var msg = new SocketMsg(OpCode.CHAT, ChatCode.GET_HISTORY_CREQ, requestDto);
        _netManager.Execute(0, msg);
    }

    private void OnCreateRoom()
    {
        var user = Models.GameModel.UserDto;
        if (user == null) return;

        var createDto = new RoomDto
        {
            RoomName = $"{user.Name}的房间",
            HostId = user.Id,
            MaxPlayers = 3
        };

        var msg = new SocketMsg(OpCode.MATCH, RoomCode.CREATE_CREQ, createDto);
        _netManager.Execute(0, msg);

        _matchStatus.Text = "创建房间中...";
        _matchStatus.TextColor = Color.Yellow;
    }

    private void OnJoinRoom()
    {
        if (_selectedRoomIndex < 0 || _selectedRoomIndex >= _roomListData.Count)
        {
            _matchStatus.Text = "请先选择一个房间";
            _matchStatus.TextColor = Color.Red;
            return;
        }

        var room = _roomListData[_selectedRoomIndex];
        if (room.Status == RoomStatus.PLAYING)
        {
            _matchStatus.Text = "该房间正在游戏中";
            _matchStatus.TextColor = Color.Red;
            return;
        }

        var msg = new SocketMsg(OpCode.MATCH, RoomCode.JOIN_CREQ, room.RoomId);
        _netManager.Execute(0, msg);

        _matchStatus.Text = $"加入房间: {room.RoomName}";
        _matchStatus.TextColor = Color.Yellow;
    }

    private void OnQuickMatch()
    {
        var msg = new SocketMsg(OpCode.MATCH, MatchCode.ENTER_CREQ, null);
        _netManager.Execute(0, msg);

        _matchStatus.Text = "匹配中...";
        _matchStatus.TextColor = Color.Yellow;
    }

    private void OnLeaveRoom()
    {
        var matchRoom = Models.GameModel?.MatchRoomDto;
        int leaveCode = matchRoom?.IsQuickMatch == true ? MatchCode.LEAVE_CREQ : RoomCode.LEAVE_CREQ;
        var msg = new SocketMsg(OpCode.MATCH, leaveCode, null);
        _netManager.Execute(0, msg);

        ClearCurrentRoom();
    }

    private void OnReady()
    {
        var msg = new SocketMsg(OpCode.MATCH, MatchCode.READY_CREQ, null);
        _netManager.Execute(0, msg);
    }

    private void OnStartGame()
    {
        if (!_isHost) return;
        var matchRoom = Models.GameModel.MatchRoomDto;
        if (matchRoom == null) return;

        bool isFull = matchRoom.UIdList.Count >= 3;
        bool allReady = isFull && matchRoom.UIdList.All(id => matchRoom.ReadyUIdList.Contains(id));
        if (!allReady)
        {
            _matchStatus.Text = isFull ? "请等待所有玩家准备" : "请等待房间满员";
            _matchStatus.TextColor = Color.Yellow;
            return;
        }

        var msg = new SocketMsg(OpCode.MATCH, MatchCode.START_CREQ, null);
        _netManager.Execute(0, msg);
    }

    private void OnSendChat()
    {
        var text = _chatInput.Text.Trim();
        if (string.IsNullOrEmpty(text)) return;

        var user = Models.GameModel.UserDto;
        if (user == null) return;

        if (!_netManager.IsConnected)
        {
            AddSystemChatMessage("未连接到服务器");
            return;
        }

        // 私聊需要选择目标
        if (_currentChannel == ChatTypes.PRIVATE && _privateChatTargetUserId == 0)
        {
            AddSystemChatMessage("请先选择私聊对象");
            return;
        }

        var chatDto = new ChatDto(user.Id, user.Name, text, _currentChannel, _privateChatTargetUserId);
        var msg = new SocketMsg(OpCode.CHAT, ChatCode.SEND_CREQ, chatDto);
        _netManager.Execute(0, msg);

        _chatInput.Text = "";
    }

    private void OnEmoji()
    {
        _showEmojiPicker = !_showEmojiPicker;
        if (_showEmojiPicker)
        {
            int pickerWidth = EmojiColumns * (EmojiButtonSize + EmojiPadding);
            int pickerHeight = EmojiRows * (EmojiButtonSize + EmojiPadding) + 10;
            _emojiPickerPos = new Vector2(
                _emojiBtn.Position.X + _emojiBtn.Size.X - pickerWidth,
                _emojiBtn.Position.Y - pickerHeight - 5);
        }
    }

    public override void Update(GameTime gameTime)
    {
        FadeIn(0.03);

        _networkTimer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
        _refreshTimer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

        // 网络消息处理
        if (_networkTimer >= 50)
        {
            _networkTimer = 0;
            _netManager.Update();
        }

        // 房间列表定时刷新 (5秒)
        if (_refreshTimer >= 5000)
        {
            _refreshTimer = 0;
            if (!_isInRoom)
                RequestRoomList();
            RequestOnlineUsers();
        }
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        var tint = Color.White * Opacity;

        if (_backgroundTexture != null)
        {
            spriteBatch.Draw(_backgroundTexture, new Rectangle(0, 0, 1280, 720), tint);
        }
        else
        {
            spriteBatch.Draw(CreateWhitePixel(),
                new Rectangle(0, 0, 1280, 720),
                new Color(24, 26, 34) * Opacity);
        }

        // 顶部背景
        spriteBatch.Draw(CreateWhitePixel(),
            new Rectangle(0, 0, 1280, TopHeight),
            new Color(22, 25, 34, 175) * Opacity);

        // 左侧背景
        spriteBatch.Draw(CreateWhitePixel(),
            new Rectangle(0, TopHeight, LeftWidth, PanelHeight),
            new Color(24, 28, 38, 145) * Opacity);

        // 右侧背景
        spriteBatch.Draw(CreateWhitePixel(),
            new Rectangle(RightX, TopHeight, RightWidth, PanelHeight),
            new Color(24, 28, 38, 145) * Opacity);

        // 中间背景
        spriteBatch.Draw(CreateWhitePixel(),
            new Rectangle(CenterX, TopHeight, RightX - CenterX, PanelHeight),
            new Color(18, 22, 30, 95) * Opacity);

        // 绘制UI
        DrawAvatar(spriteBatch, _playerAvatarUrl, TopAvatarRect, true, Opacity);
        _playerName.Draw(spriteBatch);
        _playerLevel.Draw(spriteBatch);
        _playerBeans.Draw(spriteBatch);
        _playerWins.Draw(spriteBatch);
        _playerLosses.Draw(spriteBatch);
        _settingsBtn.Draw(spriteBatch);
        _logoutBtn.Draw(spriteBatch);

        _roomListTitle.Draw(spriteBatch);
        _roomList.Draw(spriteBatch);
        _createRoomBtn.Draw(spriteBatch);
        _joinRoomBtn.Draw(spriteBatch);
        _quickMatchBtn.Draw(spriteBatch);
        _matchStatus.Draw(spriteBatch);

        _channelWorld.Draw(spriteBatch);
        _channelRoom.Draw(spriteBatch);
        _channelPrivate.Draw(spriteBatch);
        _chatList.Draw(spriteBatch);
        _onlineUsersTitle.Draw(spriteBatch);
        _onlineUserList.Draw(spriteBatch);
        _chatInput.Draw(spriteBatch);
        _chatInputLabel.Draw(spriteBatch);
        _emojiBtn.Draw(spriteBatch);
        _sendBtn.Draw(spriteBatch);

        // 房间面板
        if (_roomPanel.Visible)
        {
            _roomPanel.Draw(spriteBatch);
            DrawPlayerSlot(spriteBatch, LeftPlayerCardRect, _leftPlayerAvatarUrl, IsPlayerSlotOccupied(_leftPlayerName.Text), Opacity);
            DrawPlayerSlot(spriteBatch, SelfPlayerCardRect, _selfPlayerAvatarUrl, true, Opacity);
            DrawPlayerSlot(spriteBatch, RightPlayerCardRect, _rightPlayerAvatarUrl, IsPlayerSlotOccupied(_rightPlayerName.Text), Opacity);
            _roomTitle.Draw(spriteBatch);
            _roomSubtitle.Draw(spriteBatch);
            _leaveRoomBtn.Draw(spriteBatch);
            _leftPlayerName.Draw(spriteBatch);
            _leftPlayerStatus.Draw(spriteBatch);
            _selfPlayerName.Draw(spriteBatch);
            _selfPlayerStatus.Draw(spriteBatch);
            _rightPlayerName.Draw(spriteBatch);
            _rightPlayerStatus.Draw(spriteBatch);
            _readyBtn.Draw(spriteBatch);
            _startGameBtn.Draw(spriteBatch);
        }

        // Emoji选择器
        if (_showEmojiPicker)
            DrawEmojiPicker(spriteBatch, Opacity);

        // 提示文字
        if (!_isInRoom && _roomList.Items.Count == 0)
        {
            FontManager.DrawString(spriteBatch, FontManager.Default, "暂无房间，请创建或快速匹配",
                new Vector2(CenterX + 150, 350), MutedTextColor * Opacity);
        }
    }

    private void DrawEmojiPicker(SpriteBatch sb, float opacity)
    {
        int pickerWidth = EmojiColumns * (EmojiButtonSize + EmojiPadding);
        int pickerHeight = EmojiRows * (EmojiButtonSize + EmojiPadding) + 10;

        // 背景
        sb.Draw(CreateWhitePixel(),
            new Rectangle((int)_emojiPickerPos.X, (int)_emojiPickerPos.Y, pickerWidth, pickerHeight),
            new Color(40, 45, 55, 235) * opacity);

        // Emoji按钮
        for (int i = 0; i < Emojis.Length; i++)
        {
            int col = i % EmojiColumns;
            int row = i / EmojiColumns;
            int x = (int)_emojiPickerPos.X + col * (EmojiButtonSize + EmojiPadding) + 5;
            int y = (int)_emojiPickerPos.Y + row * (EmojiButtonSize + EmojiPadding) + 5;

            var emojiRect = new Rectangle(x, y, EmojiButtonSize, EmojiButtonSize);
            bool hover = emojiRect.Contains(_mousePos);

            if (hover)
                sb.Draw(CreateWhitePixel(), emojiRect, new Color(60, 65, 75, 200) * opacity);

            var emojiTexture = GetEmojiTexture(Emojis[i]);
            if (emojiTexture != null)
            {
                sb.Draw(emojiTexture, new Rectangle(x + 3, y + 3, 24, 24), Color.White * opacity);
            }
            else
            {
                FontManager.DrawString(sb, FontManager.Default ?? CreateFallbackFont(),
                    "?", new Vector2(x + 10, y + 4), Color.White * opacity);
            }
        }
    }

    private void DrawPlayerSlot(SpriteBatch sb, Rectangle cardRect, string? avatarUrl, bool occupied, float opacity)
    {
        sb.Draw(CreateWhitePixel(), cardRect, new Color(26, 30, 40, 125) * opacity);
        DrawBorder(sb, cardRect, new Color(170, 180, 205, 70) * opacity);

        if (!occupied)
            return;

        var avatarRect = new Rectangle(
            cardRect.X + (cardRect.Width - PlayerAvatarSize) / 2,
            cardRect.Y + 14,
            PlayerAvatarSize,
            PlayerAvatarSize);
        DrawAvatar(sb, avatarUrl, avatarRect, true, opacity);
    }

    private void DrawAvatar(SpriteBatch sb, string? avatarUrl, Rectangle rect, bool showDefault, float opacity)
    {
        var texture = GetAvatarTexture(avatarUrl);
        if (texture == null && showDefault)
            texture = _defaultAvatarTexture;

        if (texture != null)
        {
            sb.Draw(texture, rect, Color.White * opacity);
            DrawBorder(sb, rect, new Color(230, 235, 245, 120) * opacity);
            return;
        }

        if (showDefault)
        {
            sb.Draw(CreateWhitePixel(), rect, new Color(55, 60, 72, 185) * opacity);
            DrawBorder(sb, rect, new Color(230, 235, 245, 80) * opacity);
        }
    }

    private void DrawBorder(SpriteBatch sb, Rectangle rect, Color color)
    {
        var whitePixel = CreateWhitePixel();
        sb.Draw(whitePixel, new Rectangle(rect.X, rect.Y, rect.Width, 1), color);
        sb.Draw(whitePixel, new Rectangle(rect.X, rect.Y, 1, rect.Height), color);
        sb.Draw(whitePixel, new Rectangle(rect.X, rect.Bottom - 1, rect.Width, 1), color);
        sb.Draw(whitePixel, new Rectangle(rect.Right - 1, rect.Y, 1, rect.Height), color);
    }

    private Texture2D? GetAvatarTexture(string? avatarUrl)
    {
        if (string.IsNullOrEmpty(avatarUrl))
            return _defaultAvatarTexture;

        if (_avatarTextures.TryGetValue(avatarUrl, out var texture))
            return texture;

        var cachedAvatar = AvatarHandler.GetCachedAvatar(avatarUrl);
        if (cachedAvatar != null)
        {
            texture = CreateTextureFromImage(cachedAvatar);
            if (texture != null)
            {
                _avatarTextures[avatarUrl] = texture;
                return texture;
            }
        }

        if (_requestedAvatarUrls.Add(avatarUrl))
            AvatarHandler.RequestDownloadAvatar(avatarUrl);

        return _defaultAvatarTexture;
    }

    private void SetAvatarUrl(ref string? currentUrl, string? avatarUrl)
    {
        currentUrl = avatarUrl;
        if (!string.IsNullOrEmpty(avatarUrl))
            _ = GetAvatarTexture(avatarUrl);
    }

    private void ClearRoomAvatarUrls()
    {
        _leftPlayerAvatarUrl = null;
        _selfPlayerAvatarUrl = null;
        _rightPlayerAvatarUrl = null;
    }

    private void OnAvatarLoadedReceived(string avatarUrl)
    {
        if (string.IsNullOrEmpty(avatarUrl))
            return;

        _requestedAvatarUrls.Remove(avatarUrl);
        if (_avatarTextures.ContainsKey(avatarUrl))
            return;

        var cachedAvatar = AvatarHandler.GetCachedAvatar(avatarUrl);
        if (cachedAvatar == null)
            return;

        var texture = CreateTextureFromImage(cachedAvatar);
        if (texture != null)
            _avatarTextures[avatarUrl] = texture;
    }

    private Texture2D? CreateTextureFromImage(System.Drawing.Image image)
    {
        try
        {
            using var ms = new MemoryStream();
            image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Position = 0;
            return Texture2D.FromStream(Game.GraphicsDevice, ms);
        }
        catch
        {
            return null;
        }
    }

    private Texture2D? GetEmojiTexture(string emoji)
    {
        if (_emojiTextures.TryGetValue(emoji, out var cached))
            return cached;

        try
        {
            using var bitmap = new System.Drawing.Bitmap(EmojiButtonSize, EmojiButtonSize, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using var graphics = System.Drawing.Graphics.FromImage(bitmap);
            graphics.Clear(System.Drawing.Color.Transparent);
            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            using var font = new System.Drawing.Font("Segoe UI Emoji", 23f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            using var brush = new System.Drawing.SolidBrush(System.Drawing.Color.White);
            graphics.DrawString(emoji, font, brush, new System.Drawing.PointF(-3f, -1f));

            using var ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Position = 0;
            var texture = Texture2D.FromStream(Game.GraphicsDevice, ms);
            _emojiTextures[emoji] = texture;
            return texture;
        }
        catch
        {
            return null;
        }
    }

    private static bool IsPlayerSlotOccupied(string text)
    {
        return !string.IsNullOrWhiteSpace(text) && text != "等待加入...";
    }

    public override void HandleInput(InputManager input)
    {
        _mousePos = new Point((int)input.MousePosition.X, (int)input.MousePosition.Y);

        // 更新UI控件输入
        _chatInput.Update(input);
        _settingsBtn.Update(input);
        _logoutBtn.Update(input);
        _createRoomBtn.Update(input);
        _joinRoomBtn.Update(input);
        _quickMatchBtn.Update(input);
        _channelWorld.Update(input);
        _channelRoom.Update(input);
        _channelPrivate.Update(input);
        _emojiBtn.Update(input);
        _sendBtn.Update(input);
        _roomList.Update(input);
        _onlineUserList.Update(input);

        if (_roomPanel.Visible)
        {
            _leaveRoomBtn.Update(input);
            _readyBtn.Update(input);
            _startGameBtn.Update(input);
        }

        // Emoji选择器点击
        if (_showEmojiPicker)
        {
            for (int i = 0; i < Emojis.Length; i++)
            {
                int col = i % EmojiColumns;
                int row = i / EmojiColumns;
                int x = (int)_emojiPickerPos.X + col * (EmojiButtonSize + EmojiPadding) + 5;
                int y = (int)_emojiPickerPos.Y + row * (EmojiButtonSize + EmojiPadding) + 5;

                var emojiRect = new Rectangle(x, y, EmojiButtonSize, EmojiButtonSize);
                if (input.LeftMouseReleased && emojiRect.Contains(_mousePos))
                {
                    _chatInput.Text += Emojis[i];
                    _showEmojiPicker = false;
                    break;
                }
            }

            // 点击选择器外部关闭
            if (input.LeftMouseReleased)
            {
                int pickerWidth = EmojiColumns * (EmojiButtonSize + EmojiPadding);
                int pickerHeight = EmojiRows * (EmojiButtonSize + EmojiPadding) + 10;
                var pickerRect = new Rectangle((int)_emojiPickerPos.X, (int)_emojiPickerPos.Y, pickerWidth, pickerHeight);
                if (!pickerRect.Contains(_mousePos) && _emojiBtn.Bounds.Contains(_mousePos) == false)
                {
                    _showEmojiPicker = false;
                }
            }
        }

        // ESC 返回
        if (input.KeyPressed(Keys.Escape))
        {
            if (_showEmojiPicker)
            {
                _showEmojiPicker = false;
            }
            else if (_isInRoom)
            {
                OnLeaveRoom();
            }
            else
            {
                OnLogout();
            }
        }

        // Enter 发送消息
        if (input.KeyPressed(Keys.Enter) && !input.HasTextInputCharacters && !string.IsNullOrEmpty(_chatInput.Text))
        {
            OnSendChat();
        }

        // 输入框可见性
        if (!string.IsNullOrEmpty(_chatInput.Text))
            _chatInputLabel.Visible = false;
        else
            _chatInputLabel.Visible = true;
    }

    private static Texture2D _whitePixel;
    private Texture2D CreateWhitePixel()
    {
        if (_whitePixel != null) return _whitePixel;
        _whitePixel = new Texture2D(Game1.Instance.GraphicsDevice, 1, 1);
        _whitePixel.SetData(new[] { Color.White });
        return _whitePixel;
    }

    private SpriteFont CreateFallbackFont()
    {
        if (FontManager.Default != null) return FontManager.Default;
        // 如果FontManager没有默认字体，创建一个最小的占位字体
        return FontManager.Default;
    }
}
