using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Protocol.Code;
using Protocol.Dto;

namespace FairiesPoker
{
    public partial class Lobby : Form
    {
        private NetManager netManager;
        private int currentChatChannel = ChatTypes.WORLD;
        private List<ChatMessage> worldMessages = new List<ChatMessage>();
        private List<ChatMessage> roomMessages = new List<ChatMessage>();
        private List<ChatMessage> privateMessages = new List<ChatMessage>();
        private Dictionary<int, List<ChatMessage>> privateChats = new Dictionary<int, List<ChatMessage>>();
        private List<RoomDto> roomList = new List<RoomDto>();
        private RoomDto currentRoom = null;
        private bool isInRoom = false;
        private List<int> readyPlayers = new List<int>();

        // 私聊相关
        private int privateChatTargetUserId = 0;
        private string privateChatTargetUserName = "";
        private List<UserDto> onlineUsersList = new List<UserDto>();
        private List<UserDto> privateHistoryUsersList = new List<UserDto>();

        // 历史消息相关
        private long worldEarliestTimestamp = 0;
        private long privateEarliestTimestamp = 0;
        private bool isLoadingHistory = false;
        private bool hasMoreWorldHistory = true;
        private bool hasMorePrivateHistory = true;

        // Emoji面板
        private Panel emojiPanel;
        private bool emojiPanelVisible = false;

        // 在线用户选择面板
        private Form onlineUsersDialog;

        // UI资源
        private UI ui = new UI();

        private Point mouseOff;
        private bool leftFlag;

        public Lobby(NetManager netMgr)
        {
            InitializeComponent();
            netManager = netMgr;

            // 设置双缓冲
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            // 订阅事件
            Models.OnChatMessage += OnChatMessageReceived;
            Models.OnChatHistory += OnChatHistoryReceived;
            Models.OnOnlineUsers += OnOnlineUsersReceived;
            Models.OnPrivateUsers += OnPrivateUsersReceived;
            Models.OnRoomUpdate += OnRoomUpdateReceived;
            Models.OnRoomListUpdate += OnRoomListUpdateReceived;
            Models.OnMatchUpdate += OnMatchUpdateReceived;
            Models.OnAvatarLoaded += OnAvatarLoadedReceived;
            Models.OnGameStart += OnGameStartReceived;

            // 初始化Emoji面板
            InitEmojiPanel();

            // 初始化聊天列表滚动事件（用于加载历史）
            lstChatMessages.SelectedIndexChanged += LstChatMessages_SelectedIndexChanged;
        }

        private void Lobby_Load(object sender, EventArgs e)
        {
            // 加载UI资源
            config con = new config();
            ui.setUI(con.UI);
            this.BackgroundImage = ui.Background;
            this.BackgroundImageLayout = ImageLayout.Stretch;

            // 设置Panel透明度
            try
            {
                SetPanelTransparency();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"设置透明度失败: {ex.Message}");
            }

            // 为主要Panel绑定拖拽事件
            BindDragEventsToPanels();

            // 显示玩家信息
            UpdatePlayerInfo();

            // 请求房间列表
            RequestRoomList();

            // 请求今日聊天消息（解决登录时推送消息可能丢失的时序问题）
            RequestTodayMessages();

            // 开始网络更新
            timerNetwork.Start();

            // 初始淡入效果
            timerFadeIn.Start();
        }

        /// <summary>
        /// 设置Panel透明度，使背景图片可见
        /// </summary>
        private void SetPanelTransparency()
        {
            // 设置Panel背景为透明（让窗体背景图片显示出来）
            // 然后在Panel的Paint事件中绘制半透明覆盖层
            SetupTransparentPanel(panelTop);
            SetupTransparentPanel(panelLeft);
            SetupTransparentPanel(panelRight);
            SetupTransparentPanel(panelRoom);
            SetupTransparentPanel(panelChatChannels);
            SetupTransparentPanel(panelChatInput);
            SetupTransparentPanel(panelPlayerSelf);
            SetupTransparentPanel(panelPlayerLeft);
            SetupTransparentPanel(panelPlayerRight);

            // 设置ListBox透明背景
            if (lstRooms != null)
            {
                lstRooms.EmptyText = "暂无房间";
            }
            if (lstChatMessages != null)
            {
                lstChatMessages.EmptyText = "暂无消息";
            }

            // 设置文本框背景
            if (txtChatInput != null) txtChatInput.BackColor = Color.FromArgb(220, 50, 55, 65);
        }

        /// <summary>
        /// 设置Panel为透明背景，并添加半透明绘制
        /// </summary>
        private void SetupTransparentPanel(Panel panel)
        {
            if (panel == null) return;

            // 启用双缓冲
            try
            {
                typeof(Panel).InvokeMember("DoubleBuffered",
                    System.Reflection.BindingFlags.SetProperty |
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic,
                    null, panel, new object[] { true });
            }
            catch { }

            // 设置背景透明
            panel.BackColor = Color.Transparent;

            // 设置子控件背景透明（Label等）
            foreach (Control child in panel.Controls)
            {
                if (child is Label || child is PictureBox)
                {
                    child.BackColor = Color.Transparent;
                }
            }

            // 移除旧的Paint事件处理器，避免重复
            panel.Paint -= Panel_Paint;
            panel.Paint += Panel_Paint;
        }

        /// <summary>
        /// Panel绘制半透明背景
        /// </summary>
        private void Panel_Paint(object sender, PaintEventArgs e)
        {
            Panel panel = sender as Panel;
            if (panel == null) return;

            // 绘制半透明背景（Alpha=120，约50%透明度）
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(120, 30, 35, 45)))
            {
                e.Graphics.FillRectangle(brush, panel.ClientRectangle);
            }
        }

        /// <summary>
        /// 为主要Panel绑定拖拽事件，使窗口可以通过拖拽Panel移动
        /// </summary>
        private void BindDragEventsToPanels()
        {
            // 为主要Panel及其子控件添加拖拽事件
            BindDragEventsRecursive(panelTop);
            BindDragEventsRecursive(panelLeft);
            BindDragEventsRecursive(panelRight);
            BindDragEventsRecursive(panelRoom);
            BindDragEventsRecursive(panelChatChannels);
            // 排除按钮和输入框，它们需要响应点击事件
        }

        /// <summary>
        /// 递归为控件及其子控件绑定拖拽事件
        /// </summary>
        private void BindDragEventsRecursive(Control control)
        {
            // 排除按钮、文本框、列表框等需要响应点击的控件
            if (control is Button || control is TextBox || control is ListBox ||
                control is ComboBox || control is CheckBox || control is RadioButton)
            {
                return;
            }

            // 为当前控件绑定拖拽事件
            control.MouseDown += Lobby_MouseDown;
            control.MouseMove += Lobby_MouseMove;
            control.MouseUp += Lobby_MouseUp;

            // 递归处理子控件
            foreach (Control child in control.Controls)
            {
                BindDragEventsRecursive(child);
            }
        }

        #region 玩家信息

        private void UpdatePlayerInfo()
        {
            var user = Models.GameModel.UserDto;
            if (user != null)
            {
                lblPlayerName.Text = user.Name;
                lblPlayerLevel.Text = $"Lv.{user.Lv}";
                lblPlayerBeans.Text = user.Been.ToString();
                lblPlayerWins.Text = $"{user.WinCount}胜";
                lblPlayerLosses.Text = $"{user.LoseCount}负";

                // 加载头像
                LoadPlayerAvatar(user.AvatarUrl);
            }
        }

        private void LoadPlayerAvatar(string avatarUrl)
        {
            if (string.IsNullOrEmpty(avatarUrl))
            {
                picPlayerAvatar.BackgroundImage = Properties.Resources.Pla;
                return;
            }

            // 先检查缓存
            var cachedAvatar = AvatarHandler.GetCachedAvatar(avatarUrl);
            if (cachedAvatar != null)
            {
                picPlayerAvatar.BackgroundImage = new Bitmap(cachedAvatar);
                return;
            }

            // 没有缓存，显示默认头像并请求下载
            picPlayerAvatar.BackgroundImage = Properties.Resources.Pla;
            AvatarHandler.RequestDownloadAvatar(avatarUrl);
        }

        private void OnAvatarLoadedReceived(string avatarUrl)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string>(OnAvatarLoadedReceived), avatarUrl);
                return;
            }

            // 检查是否是当前用户的头像
            var user = Models.GameModel.UserDto;
            if (user != null && user.AvatarUrl == avatarUrl)
            {
                var cachedAvatar = AvatarHandler.GetCachedAvatar(avatarUrl);
                if (cachedAvatar != null)
                {
                    picPlayerAvatar.BackgroundImage = new Bitmap(cachedAvatar);
                }
            }

            // 更新房间内玩家头像
            UpdateRoomPanel();
        }

        #endregion

        #region 聊天功能

        private void btnChannelWorld_Click(object sender, EventArgs e)
        {
            SwitchChatChannel(ChatTypes.WORLD);
        }

        private void btnChannelRoom_Click(object sender, EventArgs e)
        {
            SwitchChatChannel(ChatTypes.ROOM);
        }

        private void btnChannelPrivate_Click(object sender, EventArgs e)
        {
            // 私聊频道：如果没有选择私聊对象，弹出用户选择
            if (privateChatTargetUserId == 0)
            {
                ShowOnlineUsersDialog();
            }
            else
            {
                SwitchChatChannel(ChatTypes.PRIVATE);
            }
        }

        private void SwitchChatChannel(int channel)
        {
            currentChatChannel = channel;

            // 更新按钮样式
            btnChannelWorld.BackColor = channel == ChatTypes.WORLD ? Color.FromArgb(100, 150, 200) : Color.FromArgb(60, 60, 80);
            btnChannelRoom.BackColor = channel == ChatTypes.ROOM ? Color.FromArgb(100, 150, 200) : Color.FromArgb(60, 60, 80);
            btnChannelPrivate.BackColor = channel == ChatTypes.PRIVATE ? Color.FromArgb(100, 150, 200) : Color.FromArgb(60, 60, 80);

            // 更新私聊按钮文字
            if (channel == ChatTypes.PRIVATE && privateChatTargetUserId > 0)
            {
                btnChannelPrivate.Text = $"私聊({privateChatTargetUserName})";
            }
            else
            {
                btnChannelPrivate.Text = "私聊";
            }

            // 显示对应频道的消息
            RefreshChatMessages();
        }

        private void RefreshChatMessages()
        {
            lstChatMessages.Items.Clear();

            List<ChatMessage> messages;
            switch (currentChatChannel)
            {
                case ChatTypes.WORLD:
                    messages = worldMessages;
                    break;
                case ChatTypes.ROOM:
                    messages = roomMessages;
                    break;
                case ChatTypes.PRIVATE:
                    messages = privateMessages;
                    break;
                default:
                    messages = worldMessages;
                    break;
            }

            foreach (var msg in messages)
            {
                var time = DateTimeOffset.FromUnixTimeMilliseconds(msg.Timestamp).ToLocalTime().ToString("HH:mm:ss");
                var displayText = $"[{time}] {msg.UserName}: {msg.Text}";
                lstChatMessages.Items.Add(displayText);
            }

            // 滚动到底部
            if (lstChatMessages.Items.Count > 0)
            {
                lstChatMessages.TopIndex = lstChatMessages.Items.Count - 1;
            }
        }

        /// <summary>
        /// 预添加历史消息到列表顶部
        /// </summary>
        private void PrependChatMessages(List<ChatMessage> historyMessages)
        {
            if (historyMessages == null || historyMessages.Count == 0) return;

            List<ChatMessage> messages;
            switch (currentChatChannel)
            {
                case ChatTypes.WORLD:
                    messages = worldMessages;
                    break;
                case ChatTypes.PRIVATE:
                    messages = privateMessages;
                    break;
                default:
                    return; // 房间聊天不支持历史
            }

            // 在顶部插入历史消息（保持时间顺序：从旧到新）
            for (int i = 0; i < historyMessages.Count; i++)
            {
                messages.Insert(i, historyMessages[i]);
            }

            // 限制消息数量
            while (messages.Count > 200)
            {
                messages.RemoveAt(messages.Count - 1);
            }

            // 刷新显示，保持滚动位置
            int oldCount = lstChatMessages.Items.Count;
            RefreshChatMessages();
            lstChatMessages.TopIndex = historyMessages.Count; // 滚动到之前的第一条消息
        }

        /// <summary>
        /// 聊天列表滚动事件 - 检测是否滚动到顶部以加载历史
        /// </summary>
        private void LstChatMessages_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isLoadingHistory) return;

            // 当选中第一条消息时，尝试加载历史
            if (lstChatMessages.SelectedIndex == 0 && lstChatMessages.Items.Count > 0)
            {
                LoadChatHistory();
            }
        }

        /// <summary>
        /// 加载聊天历史
        /// </summary>
        private void LoadChatHistory()
        {
            // 房间聊天不支持历史
            if (currentChatChannel == ChatTypes.ROOM) return;

            // 全服聊天需要检查是否还有更多历史
            if (currentChatChannel == ChatTypes.WORLD && !hasMoreWorldHistory) return;

            // 私聊需要检查是否还有更多历史
            if (currentChatChannel == ChatTypes.PRIVATE && !hasMorePrivateHistory) return;

            // 私聊需要选择目标用户
            if (currentChatChannel == ChatTypes.PRIVATE && privateChatTargetUserId == 0) return;

            if (!netManager.IsConnected) return;

            isLoadingHistory = true;

            // 获取最早消息的时间戳
            long beforeTimestamp = 0;
            List<ChatMessage> messages = null;

            switch (currentChatChannel)
            {
                case ChatTypes.WORLD:
                    messages = worldMessages;
                    break;
                case ChatTypes.PRIVATE:
                    messages = privateMessages;
                    break;
            }

            if (messages != null && messages.Count > 0)
            {
                beforeTimestamp = messages[0].Timestamp;
            }

            if (beforeTimestamp == 0)
            {
                beforeTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }

            // 发送历史请求
            var requestDto = new HistoryRequestDto(currentChatChannel, privateChatTargetUserId, beforeTimestamp, 20);
            var msg = new SocketMsg(OpCode.CHAT, ChatCode.GET_HISTORY_CREQ, requestDto);
            netManager.Execute(0, msg);
        }

        /// <summary>
        /// 请求今日聊天消息
        /// </summary>
        private void RequestTodayMessages()
        {
            if (!netManager.IsConnected) return;

            // 请求今日全服消息（使用0作为beforeTimestamp表示获取最新消息）
            var requestDto = new HistoryRequestDto(ChatTypes.WORLD, 0, 0, 100);
            var msg = new SocketMsg(OpCode.CHAT, ChatCode.GET_HISTORY_CREQ, requestDto);
            netManager.Execute(0, msg);
        }

        private void btnSendChat_Click(object sender, EventArgs e)
        {
            SendChatMessage();
        }

        private void txtChatInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendChatMessage();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void SendChatMessage()
        {
            var text = txtChatInput.Text.Trim();
            if (string.IsNullOrEmpty(text)) return;

            if (!netManager.IsConnected)
            {
                MessageBox.Show("未连接到服务器", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var user = Models.GameModel.UserDto;
            if (user == null) return;

            // 私聊需要选择目标用户
            if (currentChatChannel == ChatTypes.PRIVATE && privateChatTargetUserId == 0)
            {
                MessageBox.Show("请先选择私聊对象", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ShowOnlineUsersDialog();
                return;
            }

            var chatDto = new ChatDto(user.Id, user.Name, text, currentChatChannel, privateChatTargetUserId);
            var msg = new SocketMsg(OpCode.CHAT, ChatCode.SEND_CREQ, chatDto);
            netManager.Execute(0, msg);

            txtChatInput.Clear();
        }

        private void OnChatMessageReceived(ChatDto chatDto)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<ChatDto>(OnChatMessageReceived), chatDto);
                return;
            }

            var message = new ChatMessage
            {
                UserId = chatDto.UserId,
                UserName = chatDto.UserName ?? $"玩家{chatDto.UserId}",
                Text = chatDto.Text,
                ChatType = chatDto.ChatType,
                Timestamp = chatDto.Timestamp
            };

            switch (chatDto.ChatType)
            {
                case ChatTypes.WORLD:
                    worldMessages.Add(message);
                    if (worldMessages.Count > 200) worldMessages.RemoveAt(0);
                    break;
                case ChatTypes.ROOM:
                    roomMessages.Add(message);
                    if (roomMessages.Count > 50) roomMessages.RemoveAt(0);
                    break;
                case ChatTypes.PRIVATE:
                    // 只显示与当前私聊对象相关的消息
                    var myId = Models.GameModel.UserDto?.Id ?? 0;

                    // 收到私聊消息时，自动设置私聊对象以便快速回复
                    if (chatDto.UserId != myId && chatDto.UserId != privateChatTargetUserId)
                    {
                        // 收到新用户的私聊消息，自动切换到该用户
                        privateChatTargetUserId = chatDto.UserId;
                        privateChatTargetUserName = chatDto.UserName ?? $"玩家{chatDto.UserId}";
                        // 更新私聊按钮显示
                        btnChannelPrivate.Text = $"私聊({privateChatTargetUserName})";
                        // 添加到私聊历史用户列表（如果不存在）
                        if (!privateHistoryUsersList.Any(u => u.Id == chatDto.UserId))
                        {
                            privateHistoryUsersList.Add(new UserDto(chatDto.UserId, chatDto.UserName ?? $"玩家{chatDto.UserId}", 0, 0, 0, 0, 1, 0));
                        }
                    }
                    else if (chatDto.TargetUserId != myId && chatDto.TargetUserId != privateChatTargetUserId && chatDto.TargetUserId > 0)
                    {
                        // 自己发送的消息，确保目标用户在历史列表中
                        if (!privateHistoryUsersList.Any(u => u.Id == chatDto.TargetUserId))
                        {
                            // 从在线列表查找用户信息
                            var targetUser = onlineUsersList.FirstOrDefault(u => u.Id == chatDto.TargetUserId);
                            if (targetUser != null)
                            {
                                privateHistoryUsersList.Add(targetUser);
                            }
                            else
                            {
                                privateHistoryUsersList.Add(new UserDto(chatDto.TargetUserId, privateChatTargetUserName, 0, 0, 0, 0, 1, 0));
                            }
                        }
                    }

                    if (chatDto.UserId == privateChatTargetUserId || chatDto.TargetUserId == privateChatTargetUserId ||
                        (privateChatTargetUserId == 0 && (chatDto.UserId == myId || chatDto.TargetUserId == myId)))
                    {
                        privateMessages.Add(message);
                        if (privateMessages.Count > 100) privateMessages.RemoveAt(0);
                    }
                    break;
            }

            if (chatDto.ChatType == currentChatChannel)
            {
                RefreshChatMessages();
            }
        }

        /// <summary>
        /// 收到聊天历史消息
        /// </summary>
        private void OnChatHistoryReceived(List<ChatDto> messages, bool isAppend)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<List<ChatDto>, bool>(OnChatHistoryReceived), messages, isAppend);
                return;
            }

            isLoadingHistory = false;

            if (messages == null || messages.Count == 0)
            {
                // 没有更多历史
                if (currentChatChannel == ChatTypes.WORLD) hasMoreWorldHistory = false;
                if (currentChatChannel == ChatTypes.PRIVATE) hasMorePrivateHistory = false;
                return;
            }

            // 转换为ChatMessage列表
            var chatMessages = new List<ChatMessage>();
            foreach (var dto in messages)
            {
                chatMessages.Add(new ChatMessage
                {
                    UserId = dto.UserId,
                    UserName = dto.UserName ?? $"玩家{dto.UserId}",
                    Text = dto.Text,
                    ChatType = dto.ChatType,
                    Timestamp = dto.Timestamp
                });
            }

            if (isAppend)
            {
                // 追加到顶部（历史加载）- 只处理当前频道的历史
                PrependChatMessages(chatMessages);
            }
            else
            {
                // 今日消息推送 - 根据消息的ChatType分发到对应列表
                foreach (var msg in chatMessages)
                {
                    switch (msg.ChatType)
                    {
                        case ChatTypes.WORLD:
                            if (!worldMessages.Any(m => m.Timestamp == msg.Timestamp && m.UserId == msg.UserId))
                            {
                                worldMessages.Add(msg);
                            }
                            break;
                        case ChatTypes.PRIVATE:
                            if (!privateMessages.Any(m => m.Timestamp == msg.Timestamp && m.UserId == msg.UserId))
                            {
                                privateMessages.Add(msg);
                            }
                            break;
                    }
                }
                // 排序
                worldMessages.Sort((a, b) => a.Timestamp.CompareTo(b.Timestamp));
                privateMessages.Sort((a, b) => a.Timestamp.CompareTo(b.Timestamp));
                // 限制消息数量
                while (worldMessages.Count > 200) worldMessages.RemoveAt(0);
                while (privateMessages.Count > 100) privateMessages.RemoveAt(0);
                // 刷新显示
                RefreshChatMessages();
            }
        }

        /// <summary>
        /// 收到在线用户列表
        /// </summary>
        private void OnOnlineUsersReceived(List<UserDto> users)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<List<UserDto>>(OnOnlineUsersReceived), users);
                return;
            }

            onlineUsersList = users;
            // 同时请求私聊历史用户列表
            var msg = new SocketMsg(OpCode.CHAT, ChatCode.GET_PRIVATE_USERS_CREQ, null);
            netManager.Execute(0, msg);
        }

        /// <summary>
        /// 收到私聊历史用户列表
        /// </summary>
        private void OnPrivateUsersReceived(List<UserDto> users)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<List<UserDto>>(OnPrivateUsersReceived), users);
                return;
            }

            privateHistoryUsersList = users;
            // 收到两个列表后显示选择对话框
            ShowOnlineUsersDialog();
        }

        /// <summary>
        /// 显示在线用户选择对话框（合并在线用户和私聊历史用户）
        /// </summary>
        private void ShowOnlineUsersDialog()
        {
            if (onlineUsersDialog != null && onlineUsersDialog.Visible)
            {
                onlineUsersDialog.Focus();
                return;
            }

            // 请求在线用户列表和私聊历史用户列表
            if (onlineUsersList.Count == 0 && privateHistoryUsersList.Count == 0)
            {
                var msg = new SocketMsg(OpCode.USER, UserCode.GET_ONLINE_USERS_CREQ, null);
                netManager.Execute(0, msg);
                return;
            }

            // 创建在线用户选择对话框
            onlineUsersDialog = new Form
            {
                Text = "选择私聊对象",
                Size = new Size(300, 450),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.FromArgb(40, 40, 50)
            };

            var lstUsers = new ListBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(50, 50, 60),
                ForeColor = Color.White,
                Font = new Font("微软雅黑", 10F),
                BorderStyle = BorderStyle.None
            };

            var myId = Models.GameModel.UserDto?.Id ?? 0;
            var combinedUsers = new List<UserDto>();
            var addedUserIds = new HashSet<int>();

            // 先添加在线用户（标记为在线）
            foreach (var user in onlineUsersList)
            {
                if (user.Id != myId && !addedUserIds.Contains(user.Id))
                {
                    combinedUsers.Add(user);
                    addedUserIds.Add(user.Id);
                    lstUsers.Items.Add($"[在线] Lv.{user.Lv} {user.Name}");
                }
            }

            // 再添加私聊历史用户（去重，标记是否在线）
            foreach (var user in privateHistoryUsersList)
            {
                if (user.Id != myId && !addedUserIds.Contains(user.Id))
                {
                    combinedUsers.Add(user);
                    addedUserIds.Add(user.Id);
                    // 检查是否在线
                    bool isOnline = onlineUsersList.Any(u => u.Id == user.Id);
                    var statusTag = isOnline ? "[在线]" : "[离线]";
                    lstUsers.Items.Add($"{statusTag} Lv.{user.Lv} {user.Name}");
                }
            }

            lstUsers.Tag = combinedUsers;

            if (lstUsers.Items.Count == 0)
            {
                lstUsers.Items.Add("暂无其他用户");
            }

            lstUsers.DoubleClick += (s, e) =>
            {
                if (lstUsers.SelectedIndex >= 0 && lstUsers.Tag != null)
                {
                    var users = lstUsers.Tag as List<UserDto>;
                    if (users != null && lstUsers.SelectedIndex < users.Count)
                    {
                        var selectedUser = users[lstUsers.SelectedIndex];
                        privateChatTargetUserId = selectedUser.Id;
                        privateChatTargetUserName = selectedUser.Name;
                        onlineUsersDialog.Close();
                        SwitchChatChannel(ChatTypes.PRIVATE);
                    }
                }
            };

            onlineUsersDialog.Controls.Add(lstUsers);
            onlineUsersDialog.ShowDialog(this);
        }

        #endregion

        #region Emoji功能

        private void InitEmojiPanel()
        {
            emojiPanel = new Panel();
            emojiPanel.Size = new Size(280, 150);
            emojiPanel.BackColor = Color.FromArgb(50, 50, 60);
            emojiPanel.BorderStyle = BorderStyle.FixedSingle;
            emojiPanel.Visible = false;

            var emojis = new string[] {
                "😀", "😃", "😄", "😁", "😆", "😅", "🤣", "😂",
                "🙂", "😊", "😇", "🥰", "😍", "🤩", "😘", "😗",
                "😚", "😋", "😛", "😜", "🤪", "😝", "🤑", "🤗",
                "🤭", "🤫", "🤔", "🤐", "🤨", "😐", "😑", "😶",
                "😏", "😒", "🙄", "😬", "🤥", "😌", "😔", "😪",
                "🤤", "😴", "😷", "🤒", "🤕", "🤢", "🤮", "🤧",
                "🥵", "🥶", "🥴", "😵", "🤯", "🤠", "🥳", "😎",
                "🤓", "🧐", "😕", "😟", "🙁", "☹️", "😮", "😯"
            };

            int x = 5, y = 5;
            foreach (var emoji in emojis)
            {
                var btn = new Button();
                btn.Text = emoji;
                btn.Size = new Size(30, 30);
                btn.Location = new Point(x, y);
                btn.FlatStyle = FlatStyle.Flat;
                btn.BackColor = Color.Transparent;
                btn.ForeColor = Color.White;
                btn.Font = new Font("Segoe UI Emoji", 12);
                btn.Click += (s, e) =>
                {
                    txtChatInput.Text += emoji;
                    txtChatInput.Focus();
                    txtChatInput.SelectionStart = txtChatInput.Text.Length;
                };
                emojiPanel.Controls.Add(btn);

                x += 33;
                if (x > 260)
                {
                    x = 5;
                    y += 33;
                }
            }

            this.Controls.Add(emojiPanel);
        }

        private void btnEmoji_Click(object sender, EventArgs e)
        {
            emojiPanelVisible = !emojiPanelVisible;
            emojiPanel.Visible = emojiPanelVisible;

            // 将按钮位置转换为窗体坐标，然后计算面板位置
            var btnScreenPos = btnEmoji.Parent.PointToScreen(btnEmoji.Location);
            var formPos = this.PointToClient(btnScreenPos);
            emojiPanel.Location = new Point(formPos.X - emojiPanel.Width + btnEmoji.Width, formPos.Y - emojiPanel.Height - 5);
            emojiPanel.BringToFront();
        }

        #endregion

        #region 房间功能

        private void RequestRoomList()
        {
            if (netManager == null || !netManager.IsConnected) return;

            var msg = new SocketMsg(OpCode.MATCH, RoomCode.GET_ROOMS_CREQ, null);
            netManager.Execute(0, msg);
        }

        private void OnRoomListUpdateReceived(List<RoomDto> rooms)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<List<RoomDto>>(OnRoomListUpdateReceived), rooms);
                return;
            }

            roomList = rooms;
            RefreshRoomList();
        }

        private void RefreshRoomList()
        {
            lstRooms.Items.Clear();

            foreach (var room in roomList)
            {
                var status = room.Status == RoomStatus.WAITING ? "等待中" : "游戏中";
                var item = $"{room.RoomName} | {room.PlayerCount}/3 | {status}";
                lstRooms.Items.Add(item);
            }
        }

        private void btnCreateRoom_Click(object sender, EventArgs e)
        {
            if (!netManager.IsConnected)
            {
                MessageBox.Show("未连接到服务器", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var roomName = $"房间{new Random().Next(1000, 9999)}";
            if (Models.GameModel.UserDto != null)
            {
                roomName = $"{Models.GameModel.UserDto.Name}的房间";
            }

            var createDto = new RoomDto
            {
                RoomName = roomName,
                HostId = Models.GameModel.UserDto?.Id ?? 0,
                MaxPlayers = 3
            };

            var msg = new SocketMsg(OpCode.MATCH, RoomCode.CREATE_CREQ, createDto);
            netManager.Execute(0, msg);
        }

        private void btnJoinRoom_Click(object sender, EventArgs e)
        {
            if (lstRooms.SelectedIndex < 0)
            {
                MessageBox.Show("请选择一个房间", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!netManager.IsConnected)
            {
                MessageBox.Show("未连接到服务器", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedRoom = roomList[lstRooms.SelectedIndex];
            if (selectedRoom.Status == RoomStatus.PLAYING)
            {
                MessageBox.Show("该房间正在进行游戏", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var msg = new SocketMsg(OpCode.MATCH, RoomCode.JOIN_CREQ, selectedRoom.RoomId);
            netManager.Execute(0, msg);
        }

        private void btnQuickMatch_Click(object sender, EventArgs e)
        {
            if (!netManager.IsConnected)
            {
                MessageBox.Show("未连接到服务器", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var msg = new SocketMsg(OpCode.MATCH, MatchCode.ENTER_CREQ, null);
            netManager.Execute(0, msg);

            lblMatchStatus.Text = "正在匹配...";
            lblMatchStatus.Visible = true;
        }

        private void OnRoomUpdateReceived(RoomDto room)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<RoomDto>(OnRoomUpdateReceived), room);
                return;
            }

            currentRoom = room;
            isInRoom = true;
            UpdateRoomPanel();
        }

        private void OnMatchUpdateReceived(MatchRoomDto matchRoom)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<MatchRoomDto>(OnMatchUpdateReceived), matchRoom);
                return;
            }

            // 检查当前玩家是否仍在房间中
            var myId = Models.GameModel.UserDto?.Id ?? 0;
            if (matchRoom == null || !matchRoom.UIdUserDict.ContainsKey(myId))
            {
                // 当前玩家不在房间中，隐藏房间面板
                Models.GameModel.MatchRoomDto = null;
                panelRoom.Visible = false;
                isInRoom = false;
                currentRoom = null;
                lblMatchStatus.Visible = false;
                RequestRoomList();
                return;
            }

            Models.GameModel.MatchRoomDto = matchRoom;

            if (matchRoom.UIdList.Count >= 3)
            {
                // 3人齐了，等待房主开始游戏
            }

            UpdateRoomPanel();
        }

        private void UpdateRoomPanel()
        {
            if (currentRoom == null && Models.GameModel.MatchRoomDto == null)
            {
                panelRoom.Visible = false;
                isInRoom = false;
                return;
            }

            panelRoom.Visible = true;
            isInRoom = true;

            var matchRoom = Models.GameModel.MatchRoomDto;
            if (matchRoom != null)
            {
                // 显示房间名称
                lblRoomTitle.Text = string.IsNullOrEmpty(matchRoom.RoomName) ? "游戏房间" : matchRoom.RoomName;

                // 显示房间内玩家
                var myId = Models.GameModel.UserDto?.Id ?? 0;
                bool isHost = (matchRoom.HostId == myId);
                bool isFull = matchRoom.UIdList.Count >= 3;
                bool allReady = matchRoom.ReadyUIdList.Count >= 3;
                bool isQuickMatch = matchRoom.IsQuickMatch;

                // 快速匹配房间显示"匹配中..."
                if (isQuickMatch)
                {
                    lblRoomTitle.Text = "快速匹配中...";
                }

                // 自己
                if (matchRoom.UIdUserDict.ContainsKey(myId))
                {
                    var me = matchRoom.UIdUserDict[myId];
                    SetPlayerAvatar(picSelfAvatar, me.AvatarUrl);
                    bool isSelfHost = (matchRoom.HostId == myId);
                    lblSelfName.Text = isSelfHost ? $"👑 {me.Name}" : me.Name;
                    // 快速匹配房间显示"等待匹配"
                    if (isQuickMatch)
                    {
                        lblSelfStatus.Text = "等待匹配";
                        lblSelfStatus.ForeColor = Color.Yellow;
                    }
                    else
                    {
                        lblSelfStatus.Text = matchRoom.ReadyUIdList.Contains(myId) ? "已准备" : "等待中";
                        lblSelfStatus.ForeColor = matchRoom.ReadyUIdList.Contains(myId) ? Color.LimeGreen : Color.Gray;
                    }
                    lblSelfLevel.Text = $"Lv.{me.Lv}";
                }

                // 左边玩家
                if (matchRoom.LeftId > 0 && matchRoom.UIdUserDict.ContainsKey(matchRoom.LeftId))
                {
                    var left = matchRoom.UIdUserDict[matchRoom.LeftId];
                    SetPlayerAvatar(picLeftAvatar, left.AvatarUrl);
                    bool isLeftHost = (matchRoom.HostId == matchRoom.LeftId);
                    lblLeftName.Text = isLeftHost ? $"👑 {left.Name}" : left.Name;
                    // 快速匹配房间显示"等待匹配"
                    if (isQuickMatch)
                    {
                        lblLeftStatus.Text = "等待匹配";
                        lblLeftStatus.ForeColor = Color.Yellow;
                    }
                    else
                    {
                        lblLeftStatus.Text = matchRoom.ReadyUIdList.Contains(matchRoom.LeftId) ? "已准备" : "等待中";
                        lblLeftStatus.ForeColor = matchRoom.ReadyUIdList.Contains(matchRoom.LeftId) ? Color.LimeGreen : Color.Gray;
                    }
                    lblLeftLevel.Text = $"Lv.{left.Lv}";
                }
                else
                {
                    picLeftAvatar.BackgroundImage = null;
                    lblLeftName.Text = "等待加入...";
                    lblLeftStatus.Text = "";
                    lblLeftLevel.Text = "";
                }

                // 右边玩家
                if (matchRoom.RightId > 0 && matchRoom.UIdUserDict.ContainsKey(matchRoom.RightId))
                {
                    var right = matchRoom.UIdUserDict[matchRoom.RightId];
                    SetPlayerAvatar(picRightAvatar, right.AvatarUrl);
                    bool isRightHost = (matchRoom.HostId == matchRoom.RightId);
                    lblRightName.Text = isRightHost ? $"👑 {right.Name}" : right.Name;
                    // 快速匹配房间显示"等待匹配"
                    if (isQuickMatch)
                    {
                        lblRightStatus.Text = "等待匹配";
                        lblRightStatus.ForeColor = Color.Yellow;
                    }
                    else
                    {
                        lblRightStatus.Text = matchRoom.ReadyUIdList.Contains(matchRoom.RightId) ? "已准备" : "等待中";
                        lblRightStatus.ForeColor = matchRoom.ReadyUIdList.Contains(matchRoom.RightId) ? Color.LimeGreen : Color.Gray;
                    }
                    lblRightLevel.Text = $"Lv.{right.Lv}";
                }
                else
                {
                    picRightAvatar.BackgroundImage = null;
                    lblRightName.Text = "等待加入...";
                    lblRightStatus.Text = "";
                    lblRightLevel.Text = "";
                }

                // 更新准备按钮（快速匹配房间隐藏）
                if (isQuickMatch)
                {
                    btnReady.Visible = false;
                    btnStartGame.Visible = false;
                }
                else
                {
                    bool isReady = matchRoom.ReadyUIdList.Contains(myId);
                    btnReady.Visible = true;
                    btnReady.Text = isReady ? "取消准备" : "准备";
                    btnReady.Enabled = true;
                    btnReady.BackColor = isReady ? Color.FromArgb(150, 100, 60) : Color.FromArgb(80, 150, 100);

                    // 显示/隐藏开始游戏按钮（只有房主可见）
                    btnStartGame.Visible = isHost;
                    if (isHost)
                    {
                        btnStartGame.Enabled = isFull && allReady;
                        if (!isFull)
                        {
                            btnStartGame.Text = "等待玩家...";
                        }
                        else if (!allReady)
                        {
                            btnStartGame.Text = "等待准备...";
                        }
                        else
                        {
                            btnStartGame.Text = "开始游戏";
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 设置玩家头像
        /// </summary>
        private void SetPlayerAvatar(PictureBox pictureBox, string avatarUrl)
        {
            if (string.IsNullOrEmpty(avatarUrl))
            {
                pictureBox.BackgroundImage = Properties.Resources.Pla;
                return;
            }

            var cachedAvatar = AvatarHandler.GetCachedAvatar(avatarUrl);
            if (cachedAvatar != null)
            {
                pictureBox.BackgroundImage = new Bitmap(cachedAvatar);
            }
            else
            {
                pictureBox.BackgroundImage = Properties.Resources.Pla;
                AvatarHandler.RequestDownloadAvatar(avatarUrl);
            }
        }

        private void btnReady_Click(object sender, EventArgs e)
        {
            if (!netManager.IsConnected) return;

            var msg = new SocketMsg(OpCode.MATCH, MatchCode.READY_CREQ, null);
            netManager.Execute(0, msg);
        }

        private void btnStartGame_Click(object sender, EventArgs e)
        {
            if (!netManager.IsConnected) return;

            var matchRoom = Models.GameModel.MatchRoomDto;
            if (matchRoom == null) return;

            var myId = Models.GameModel.UserDto?.Id ?? 0;
            if (matchRoom.HostId != myId) return;

            // 只有房主可以开始游戏，发送开始游戏请求
            if (matchRoom.UIdList.Count >= 3 && matchRoom.ReadyUIdList.Count >= 3)
            {
                var msg = new SocketMsg(OpCode.MATCH, MatchCode.START_CREQ, null);
                netManager.Execute(0, msg);
            }
        }

        private void btnLeaveRoom_Click(object sender, EventArgs e)
        {
            if (!netManager.IsConnected) return;

            var msg = new SocketMsg(OpCode.MATCH, MatchCode.LEAVE_CREQ, null);
            netManager.Execute(0, msg);

            currentRoom = null;
            isInRoom = false;
            Models.GameModel.MatchRoomDto = null;
            panelRoom.Visible = false;
            lblMatchStatus.Visible = false;

            // 刷新房间列表
            RequestRoomList();
        }

        private void StartGame()
        {
            // 停止大厅的网络更新定时器（游戏界面会自己处理网络消息）
            timerNetwork.Stop();

            // 隐藏当前窗口
            this.Hide();

            // 打开游戏窗口
            DdzMian gameForm = new DdzMian(true);
            gameForm.FormClosed += (s, e) =>
            {
                // 游戏结束后重新启动大厅的网络更新
                timerNetwork.Start();
                this.Show();
                RequestRoomList();
                UpdatePlayerInfo();
            };
            gameForm.Show();
        }

        /// <summary>
        /// 游戏开始事件处理
        /// </summary>
        private void OnGameStartReceived()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(OnGameStartReceived));
                return;
            }

            StartGame();
        }

        #endregion

        #region 窗口事件

        private void btnSettings_Click(object sender, EventArgs e)
        {
            Settings settings = new Settings();
            settings.ShowDialog();
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("确定要退出登录吗？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                // 取消订阅事件
                Models.OnChatMessage -= OnChatMessageReceived;
                Models.OnChatHistory -= OnChatHistoryReceived;
                Models.OnOnlineUsers -= OnOnlineUsersReceived;
                Models.OnPrivateUsers -= OnPrivateUsersReceived;
                Models.OnRoomUpdate -= OnRoomUpdateReceived;
                Models.OnRoomListUpdate -= OnRoomListUpdateReceived;
                Models.OnMatchUpdate -= OnMatchUpdateReceived;
                Models.OnAvatarLoaded -= OnAvatarLoadedReceived;
                Models.OnGameStart -= OnGameStartReceived;

                // 通知服务器下线并安全断开连接
                if (netManager.IsConnected)
                {
                    var logoutMsg = new SocketMsg(OpCode.ACCOUNT, AccountCode.LOGOUT_CREQ, null);
                    netManager.SafeDisconnect(logoutMsg);
                }

                // 返回登录界面
                Login login = new Login();
                login.Show();
                this.Close();
            }
        }

        private void timerNetwork_Tick(object sender, EventArgs e)
        {
            netManager.Update();
        }

        private void timerFadeIn_Tick(object sender, EventArgs e)
        {
            if (Opacity < 1)
            {
                Opacity += 0.05;
            }
            else
            {
                timerFadeIn.Stop();
            }
        }

        private void timerRefresh_Tick(object sender, EventArgs e)
        {
            // 定期刷新房间列表
            if (!isInRoom)
            {
                RequestRoomList();
            }
        }

        // 窗口拖动
        private void Lobby_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // 获取鼠标在屏幕上的位置
                Point mousePos = Control.MousePosition;
                // 转换为窗体坐标
                Point formPos = this.PointToClient(mousePos);
                mouseOff = new Point(-formPos.X, -formPos.Y);
                leftFlag = true;
            }
        }

        private void Lobby_MouseMove(object sender, MouseEventArgs e)
        {
            if (leftFlag)
            {
                Point mouseSet = Control.MousePosition;
                mouseSet.Offset(mouseOff.X, mouseOff.Y);
                Location = mouseSet;
            }
        }

        private void Lobby_MouseUp(object sender, MouseEventArgs e)
        {
            if (leftFlag)
            {
                leftFlag = false;
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            Models.OnChatMessage -= OnChatMessageReceived;
            Models.OnChatHistory -= OnChatHistoryReceived;
            Models.OnOnlineUsers -= OnOnlineUsersReceived;
            Models.OnPrivateUsers -= OnPrivateUsersReceived;
            Models.OnRoomUpdate -= OnRoomUpdateReceived;
            Models.OnRoomListUpdate -= OnRoomListUpdateReceived;
            Models.OnMatchUpdate -= OnMatchUpdateReceived;
            Models.OnAvatarLoaded -= OnAvatarLoadedReceived;
            Models.OnGameStart -= OnGameStartReceived;

            // 如果是通过关闭窗口退出的，安全断开连接
            if (netManager != null && netManager.IsConnected)
            {
                var logoutMsg = new SocketMsg(OpCode.ACCOUNT, AccountCode.LOGOUT_CREQ, null);
                netManager.SafeDisconnect(logoutMsg);
            }

            base.OnFormClosed(e);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // 如果用户点击了关闭按钮但没有通过退出登录按钮
            if (e.CloseReason == CloseReason.UserClosing && netManager != null && netManager.IsConnected)
            {
                var result = MessageBox.Show("确定要退出游戏吗？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }
            base.OnFormClosing(e);
        }

        #endregion
    }

    /// <summary>
    /// 聊天消息模型
    /// </summary>
    public class ChatMessage
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Text { get; set; }
        public int ChatType { get; set; }
        public long Timestamp { get; set; }
    }
}