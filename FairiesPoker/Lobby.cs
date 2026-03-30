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

        // Emoji面板
        private Panel emojiPanel;
        private bool emojiPanelVisible = false;

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
            Models.OnRoomUpdate += OnRoomUpdateReceived;
            Models.OnRoomListUpdate += OnRoomListUpdateReceived;
            Models.OnMatchUpdate += OnMatchUpdateReceived;
            Models.OnAvatarLoaded += OnAvatarLoadedReceived;
            Models.OnGameStart += OnGameStartReceived;

            // 初始化Emoji面板
            InitEmojiPanel();
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
            SwitchChatChannel(ChatTypes.PRIVATE);
        }

        private void SwitchChatChannel(int channel)
        {
            currentChatChannel = channel;

            // 更新按钮样式
            btnChannelWorld.BackColor = channel == ChatTypes.WORLD ? Color.FromArgb(100, 150, 200) : Color.FromArgb(60, 60, 80);
            btnChannelRoom.BackColor = channel == ChatTypes.ROOM ? Color.FromArgb(100, 150, 200) : Color.FromArgb(60, 60, 80);
            btnChannelPrivate.BackColor = channel == ChatTypes.PRIVATE ? Color.FromArgb(100, 150, 200) : Color.FromArgb(60, 60, 80);

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

            var chatDto = new ChatDto(user.Id, user.Name, text, currentChatChannel);
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
                    if (worldMessages.Count > 100) worldMessages.RemoveAt(0);
                    break;
                case ChatTypes.ROOM:
                    roomMessages.Add(message);
                    if (roomMessages.Count > 50) roomMessages.RemoveAt(0);
                    break;
                case ChatTypes.PRIVATE:
                    privateMessages.Add(message);
                    if (privateMessages.Count > 50) privateMessages.RemoveAt(0);
                    break;
            }

            if (chatDto.ChatType == currentChatChannel)
            {
                RefreshChatMessages();
            }
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