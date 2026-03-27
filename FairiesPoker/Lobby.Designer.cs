namespace FairiesPoker
{
    partial class Lobby
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            timerNetwork = new System.Windows.Forms.Timer(components);
            timerFadeIn = new System.Windows.Forms.Timer(components);
            timerRefresh = new System.Windows.Forms.Timer(components);
            panelTop = new System.Windows.Forms.Panel();
            btnLogout = new System.Windows.Forms.Button();
            btnSettings = new System.Windows.Forms.Button();
            lblPlayerLosses = new System.Windows.Forms.Label();
            lblPlayerWins = new System.Windows.Forms.Label();
            lblPlayerBeans = new System.Windows.Forms.Label();
            lblPlayerLevel = new System.Windows.Forms.Label();
            lblPlayerName = new System.Windows.Forms.Label();
            picPlayerAvatar = new System.Windows.Forms.PictureBox();
            panelLeft = new System.Windows.Forms.Panel();
            lblMatchStatus = new System.Windows.Forms.Label();
            btnQuickMatch = new System.Windows.Forms.Button();
            btnJoinRoom = new System.Windows.Forms.Button();
            btnCreateRoom = new System.Windows.Forms.Button();
            lstRooms = new System.Windows.Forms.ListBox();
            lblRoomList = new System.Windows.Forms.Label();
            panelRight = new System.Windows.Forms.Panel();
            panelChatInput = new System.Windows.Forms.Panel();
            btnSendChat = new System.Windows.Forms.Button();
            btnEmoji = new System.Windows.Forms.Button();
            txtChatInput = new System.Windows.Forms.TextBox();
            lstChatMessages = new System.Windows.Forms.ListBox();
            panelChatChannels = new System.Windows.Forms.Panel();
            btnChannelPrivate = new System.Windows.Forms.Button();
            btnChannelRoom = new System.Windows.Forms.Button();
            btnChannelWorld = new System.Windows.Forms.Button();
            panelRoom = new System.Windows.Forms.Panel();
            btnStartGame = new System.Windows.Forms.Button();
            lblRoomTitle = new System.Windows.Forms.Label();
            btnLeaveRoom = new System.Windows.Forms.Button();
            btnReady = new System.Windows.Forms.Button();
            panelPlayerRight = new System.Windows.Forms.Panel();
            lblRightLevel = new System.Windows.Forms.Label();
            lblRightStatus = new System.Windows.Forms.Label();
            lblRightName = new System.Windows.Forms.Label();
            picRightAvatar = new System.Windows.Forms.PictureBox();
            panelPlayerLeft = new System.Windows.Forms.Panel();
            lblLeftLevel = new System.Windows.Forms.Label();
            lblLeftStatus = new System.Windows.Forms.Label();
            lblLeftName = new System.Windows.Forms.Label();
            picLeftAvatar = new System.Windows.Forms.PictureBox();
            panelPlayerSelf = new System.Windows.Forms.Panel();
            lblSelfLevel = new System.Windows.Forms.Label();
            lblSelfStatus = new System.Windows.Forms.Label();
            lblSelfName = new System.Windows.Forms.Label();
            picSelfAvatar = new System.Windows.Forms.PictureBox();
            panelTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picPlayerAvatar).BeginInit();
            panelLeft.SuspendLayout();
            panelRight.SuspendLayout();
            panelChatInput.SuspendLayout();
            panelChatChannels.SuspendLayout();
            panelRoom.SuspendLayout();
            panelPlayerRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picRightAvatar).BeginInit();
            panelPlayerLeft.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picLeftAvatar).BeginInit();
            panelPlayerSelf.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picSelfAvatar).BeginInit();
            SuspendLayout();
            // 
            // timerNetwork
            // 
            timerNetwork.Interval = 50;
            timerNetwork.Tick += timerNetwork_Tick;
            // 
            // timerFadeIn
            // 
            timerFadeIn.Interval = 30;
            timerFadeIn.Tick += timerFadeIn_Tick;
            // 
            // timerRefresh
            // 
            timerRefresh.Interval = 5000;
            timerRefresh.Tick += timerRefresh_Tick;
            // 
            // panelTop
            // 
            panelTop.BackColor = System.Drawing.Color.FromArgb(40, 40, 50);
            panelTop.Controls.Add(btnLogout);
            panelTop.Controls.Add(btnSettings);
            panelTop.Controls.Add(lblPlayerLosses);
            panelTop.Controls.Add(lblPlayerWins);
            panelTop.Controls.Add(lblPlayerBeans);
            panelTop.Controls.Add(lblPlayerLevel);
            panelTop.Controls.Add(lblPlayerName);
            panelTop.Controls.Add(picPlayerAvatar);
            panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            panelTop.Location = new System.Drawing.Point(0, 0);
            panelTop.Margin = new System.Windows.Forms.Padding(4);
            panelTop.Name = "panelTop";
            panelTop.Size = new System.Drawing.Size(1500, 120);
            panelTop.TabIndex = 0;
            // 
            // btnLogout
            // 
            btnLogout.BackColor = System.Drawing.Color.FromArgb(150, 60, 60);
            btnLogout.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnLogout.Font = new System.Drawing.Font("微软雅黑", 9F);
            btnLogout.ForeColor = System.Drawing.Color.White;
            btnLogout.Location = new System.Drawing.Point(1365, 30);
            btnLogout.Margin = new System.Windows.Forms.Padding(4);
            btnLogout.Name = "btnLogout";
            btnLogout.Size = new System.Drawing.Size(105, 60);
            btnLogout.TabIndex = 7;
            btnLogout.Text = "退出";
            btnLogout.UseVisualStyleBackColor = false;
            btnLogout.Click += btnLogout_Click;
            // 
            // btnSettings
            // 
            btnSettings.BackColor = System.Drawing.Color.FromArgb(60, 60, 70);
            btnSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnSettings.Font = new System.Drawing.Font("微软雅黑", 9F);
            btnSettings.ForeColor = System.Drawing.Color.White;
            btnSettings.Location = new System.Drawing.Point(1230, 30);
            btnSettings.Margin = new System.Windows.Forms.Padding(4);
            btnSettings.Name = "btnSettings";
            btnSettings.Size = new System.Drawing.Size(105, 60);
            btnSettings.TabIndex = 6;
            btnSettings.Text = "设置";
            btnSettings.UseVisualStyleBackColor = false;
            btnSettings.Click += btnSettings_Click;
            // 
            // lblPlayerLosses
            // 
            lblPlayerLosses.AutoSize = true;
            lblPlayerLosses.Font = new System.Drawing.Font("微软雅黑", 9F);
            lblPlayerLosses.ForeColor = System.Drawing.Color.LightGray;
            lblPlayerLosses.Location = new System.Drawing.Point(375, 63);
            lblPlayerLosses.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblPlayerLosses.Name = "lblPlayerLosses";
            lblPlayerLosses.Size = new System.Drawing.Size(39, 24);
            lblPlayerLosses.TabIndex = 5;
            lblPlayerLosses.Text = "0负";
            // 
            // lblPlayerWins
            // 
            lblPlayerWins.AutoSize = true;
            lblPlayerWins.Font = new System.Drawing.Font("微软雅黑", 9F);
            lblPlayerWins.ForeColor = System.Drawing.Color.LightBlue;
            lblPlayerWins.Location = new System.Drawing.Point(300, 63);
            lblPlayerWins.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblPlayerWins.Name = "lblPlayerWins";
            lblPlayerWins.Size = new System.Drawing.Size(39, 24);
            lblPlayerWins.TabIndex = 4;
            lblPlayerWins.Text = "0胜";
            // 
            // lblPlayerBeans
            // 
            lblPlayerBeans.AutoSize = true;
            lblPlayerBeans.Font = new System.Drawing.Font("微软雅黑", 9F);
            lblPlayerBeans.ForeColor = System.Drawing.Color.LightGreen;
            lblPlayerBeans.Location = new System.Drawing.Point(210, 63);
            lblPlayerBeans.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblPlayerBeans.Name = "lblPlayerBeans";
            lblPlayerBeans.Size = new System.Drawing.Size(48, 24);
            lblPlayerBeans.TabIndex = 3;
            lblPlayerBeans.Text = "豆: 0";
            // 
            // lblPlayerLevel
            // 
            lblPlayerLevel.AutoSize = true;
            lblPlayerLevel.Font = new System.Drawing.Font("微软雅黑", 9F);
            lblPlayerLevel.ForeColor = System.Drawing.Color.Gold;
            lblPlayerLevel.Location = new System.Drawing.Point(135, 63);
            lblPlayerLevel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblPlayerLevel.Name = "lblPlayerLevel";
            lblPlayerLevel.Size = new System.Drawing.Size(43, 24);
            lblPlayerLevel.TabIndex = 2;
            lblPlayerLevel.Text = "Lv.1";
            // 
            // lblPlayerName
            // 
            lblPlayerName.AutoSize = true;
            lblPlayerName.Font = new System.Drawing.Font("微软雅黑", 14F, System.Drawing.FontStyle.Bold);
            lblPlayerName.ForeColor = System.Drawing.Color.White;
            lblPlayerName.Location = new System.Drawing.Point(135, 18);
            lblPlayerName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblPlayerName.Name = "lblPlayerName";
            lblPlayerName.Size = new System.Drawing.Size(101, 37);
            lblPlayerName.TabIndex = 1;
            lblPlayerName.Text = "玩家名";
            // 
            // picPlayerAvatar
            // 
            picPlayerAvatar.BackColor = System.Drawing.Color.Transparent;
            picPlayerAvatar.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            picPlayerAvatar.Location = new System.Drawing.Point(22, 15);
            picPlayerAvatar.Margin = new System.Windows.Forms.Padding(4);
            picPlayerAvatar.Name = "picPlayerAvatar";
            picPlayerAvatar.Size = new System.Drawing.Size(90, 90);
            picPlayerAvatar.TabIndex = 0;
            picPlayerAvatar.TabStop = false;
            // 
            // panelLeft
            // 
            panelLeft.BackColor = System.Drawing.Color.FromArgb(30, 30, 40);
            panelLeft.Controls.Add(lblMatchStatus);
            panelLeft.Controls.Add(btnQuickMatch);
            panelLeft.Controls.Add(btnJoinRoom);
            panelLeft.Controls.Add(btnCreateRoom);
            panelLeft.Controls.Add(lstRooms);
            panelLeft.Controls.Add(lblRoomList);
            panelLeft.Dock = System.Windows.Forms.DockStyle.Left;
            panelLeft.Location = new System.Drawing.Point(0, 120);
            panelLeft.Margin = new System.Windows.Forms.Padding(4);
            panelLeft.Name = "panelLeft";
            panelLeft.Size = new System.Drawing.Size(450, 780);
            panelLeft.TabIndex = 1;
            // 
            // lblMatchStatus
            // 
            lblMatchStatus.AutoSize = true;
            lblMatchStatus.Font = new System.Drawing.Font("微软雅黑", 9F);
            lblMatchStatus.ForeColor = System.Drawing.Color.Yellow;
            lblMatchStatus.Location = new System.Drawing.Point(22, 705);
            lblMatchStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblMatchStatus.Name = "lblMatchStatus";
            lblMatchStatus.Size = new System.Drawing.Size(0, 24);
            lblMatchStatus.TabIndex = 5;
            lblMatchStatus.Visible = false;
            // 
            // btnQuickMatch
            // 
            btnQuickMatch.BackColor = System.Drawing.Color.FromArgb(200, 100, 50);
            btnQuickMatch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnQuickMatch.Font = new System.Drawing.Font("微软雅黑", 11F, System.Drawing.FontStyle.Bold);
            btnQuickMatch.ForeColor = System.Drawing.Color.White;
            btnQuickMatch.Location = new System.Drawing.Point(22, 615);
            btnQuickMatch.Margin = new System.Windows.Forms.Padding(4);
            btnQuickMatch.Name = "btnQuickMatch";
            btnQuickMatch.Size = new System.Drawing.Size(405, 75);
            btnQuickMatch.TabIndex = 4;
            btnQuickMatch.Text = "快速匹配";
            btnQuickMatch.UseVisualStyleBackColor = false;
            btnQuickMatch.Click += btnQuickMatch_Click;
            // 
            // btnJoinRoom
            // 
            btnJoinRoom.BackColor = System.Drawing.Color.FromArgb(80, 150, 100);
            btnJoinRoom.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnJoinRoom.Font = new System.Drawing.Font("微软雅黑", 10F);
            btnJoinRoom.ForeColor = System.Drawing.Color.White;
            btnJoinRoom.Location = new System.Drawing.Point(232, 525);
            btnJoinRoom.Margin = new System.Windows.Forms.Padding(4);
            btnJoinRoom.Name = "btnJoinRoom";
            btnJoinRoom.Size = new System.Drawing.Size(195, 60);
            btnJoinRoom.TabIndex = 3;
            btnJoinRoom.Text = "加入房间";
            btnJoinRoom.UseVisualStyleBackColor = false;
            btnJoinRoom.Click += btnJoinRoom_Click;
            // 
            // btnCreateRoom
            // 
            btnCreateRoom.BackColor = System.Drawing.Color.FromArgb(60, 120, 180);
            btnCreateRoom.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnCreateRoom.Font = new System.Drawing.Font("微软雅黑", 10F);
            btnCreateRoom.ForeColor = System.Drawing.Color.White;
            btnCreateRoom.Location = new System.Drawing.Point(22, 525);
            btnCreateRoom.Margin = new System.Windows.Forms.Padding(4);
            btnCreateRoom.Name = "btnCreateRoom";
            btnCreateRoom.Size = new System.Drawing.Size(195, 60);
            btnCreateRoom.TabIndex = 2;
            btnCreateRoom.Text = "创建房间";
            btnCreateRoom.UseVisualStyleBackColor = false;
            btnCreateRoom.Click += btnCreateRoom_Click;
            // 
            // lstRooms
            // 
            lstRooms.BackColor = System.Drawing.Color.FromArgb(40, 40, 50);
            lstRooms.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            lstRooms.ForeColor = System.Drawing.Color.White;
            lstRooms.FormattingEnabled = true;
            lstRooms.Location = new System.Drawing.Point(22, 75);
            lstRooms.Margin = new System.Windows.Forms.Padding(4);
            lstRooms.Name = "lstRooms";
            lstRooms.Size = new System.Drawing.Size(404, 410);
            lstRooms.TabIndex = 1;
            // 
            // lblRoomList
            // 
            lblRoomList.AutoSize = true;
            lblRoomList.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold);
            lblRoomList.ForeColor = System.Drawing.Color.White;
            lblRoomList.Location = new System.Drawing.Point(22, 22);
            lblRoomList.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblRoomList.Name = "lblRoomList";
            lblRoomList.Size = new System.Drawing.Size(110, 31);
            lblRoomList.TabIndex = 0;
            lblRoomList.Text = "房间列表";
            // 
            // panelRight
            // 
            panelRight.BackColor = System.Drawing.Color.FromArgb(30, 30, 40);
            panelRight.Controls.Add(panelChatInput);
            panelRight.Controls.Add(lstChatMessages);
            panelRight.Controls.Add(panelChatChannels);
            panelRight.Dock = System.Windows.Forms.DockStyle.Right;
            panelRight.Location = new System.Drawing.Point(1050, 120);
            panelRight.Margin = new System.Windows.Forms.Padding(4);
            panelRight.Name = "panelRight";
            panelRight.Size = new System.Drawing.Size(450, 780);
            panelRight.TabIndex = 2;
            // 
            // panelChatInput
            // 
            panelChatInput.BackColor = System.Drawing.Color.FromArgb(50, 50, 60);
            panelChatInput.Controls.Add(btnSendChat);
            panelChatInput.Controls.Add(btnEmoji);
            panelChatInput.Controls.Add(txtChatInput);
            panelChatInput.Dock = System.Windows.Forms.DockStyle.Bottom;
            panelChatInput.Location = new System.Drawing.Point(0, 705);
            panelChatInput.Margin = new System.Windows.Forms.Padding(4);
            panelChatInput.Name = "panelChatInput";
            panelChatInput.Size = new System.Drawing.Size(450, 75);
            panelChatInput.TabIndex = 2;
            // 
            // btnSendChat
            // 
            btnSendChat.BackColor = System.Drawing.Color.FromArgb(60, 120, 180);
            btnSendChat.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnSendChat.Font = new System.Drawing.Font("微软雅黑", 9F);
            btnSendChat.ForeColor = System.Drawing.Color.White;
            btnSendChat.Location = new System.Drawing.Point(368, 12);
            btnSendChat.Margin = new System.Windows.Forms.Padding(4);
            btnSendChat.Name = "btnSendChat";
            btnSendChat.Size = new System.Drawing.Size(75, 48);
            btnSendChat.TabIndex = 2;
            btnSendChat.Text = "发送";
            btnSendChat.UseVisualStyleBackColor = false;
            btnSendChat.Click += btnSendChat_Click;
            // 
            // btnEmoji
            // 
            btnEmoji.BackColor = System.Drawing.Color.FromArgb(70, 70, 80);
            btnEmoji.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnEmoji.Font = new System.Drawing.Font("Segoe UI Emoji", 12F);
            btnEmoji.Location = new System.Drawing.Point(300, 12);
            btnEmoji.Margin = new System.Windows.Forms.Padding(4);
            btnEmoji.Name = "btnEmoji";
            btnEmoji.Size = new System.Drawing.Size(52, 48);
            btnEmoji.TabIndex = 1;
            btnEmoji.Text = "😊";
            btnEmoji.UseVisualStyleBackColor = false;
            btnEmoji.Click += btnEmoji_Click;
            // 
            // txtChatInput
            // 
            txtChatInput.BackColor = System.Drawing.Color.FromArgb(60, 60, 70);
            txtChatInput.BorderStyle = System.Windows.Forms.BorderStyle.None;
            txtChatInput.ForeColor = System.Drawing.Color.White;
            txtChatInput.Location = new System.Drawing.Point(15, 21);
            txtChatInput.Margin = new System.Windows.Forms.Padding(4);
            txtChatInput.Name = "txtChatInput";
            txtChatInput.Size = new System.Drawing.Size(270, 23);
            txtChatInput.TabIndex = 0;
            txtChatInput.KeyDown += txtChatInput_KeyDown;
            // 
            // lstChatMessages
            // 
            lstChatMessages.BackColor = System.Drawing.Color.FromArgb(40, 40, 50);
            lstChatMessages.BorderStyle = System.Windows.Forms.BorderStyle.None;
            lstChatMessages.Dock = System.Windows.Forms.DockStyle.Fill;
            lstChatMessages.ForeColor = System.Drawing.Color.White;
            lstChatMessages.FormattingEnabled = true;
            lstChatMessages.Location = new System.Drawing.Point(0, 60);
            lstChatMessages.Margin = new System.Windows.Forms.Padding(4);
            lstChatMessages.Name = "lstChatMessages";
            lstChatMessages.Size = new System.Drawing.Size(450, 720);
            lstChatMessages.TabIndex = 1;
            // 
            // panelChatChannels
            // 
            panelChatChannels.BackColor = System.Drawing.Color.FromArgb(50, 50, 60);
            panelChatChannels.Controls.Add(btnChannelPrivate);
            panelChatChannels.Controls.Add(btnChannelRoom);
            panelChatChannels.Controls.Add(btnChannelWorld);
            panelChatChannels.Dock = System.Windows.Forms.DockStyle.Top;
            panelChatChannels.Location = new System.Drawing.Point(0, 0);
            panelChatChannels.Margin = new System.Windows.Forms.Padding(4);
            panelChatChannels.Name = "panelChatChannels";
            panelChatChannels.Size = new System.Drawing.Size(450, 60);
            panelChatChannels.TabIndex = 0;
            // 
            // btnChannelPrivate
            // 
            btnChannelPrivate.BackColor = System.Drawing.Color.FromArgb(60, 60, 80);
            btnChannelPrivate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnChannelPrivate.Font = new System.Drawing.Font("微软雅黑", 9F);
            btnChannelPrivate.ForeColor = System.Drawing.Color.White;
            btnChannelPrivate.Location = new System.Drawing.Point(308, 8);
            btnChannelPrivate.Margin = new System.Windows.Forms.Padding(4);
            btnChannelPrivate.Name = "btnChannelPrivate";
            btnChannelPrivate.Size = new System.Drawing.Size(135, 45);
            btnChannelPrivate.TabIndex = 2;
            btnChannelPrivate.Text = "私聊";
            btnChannelPrivate.UseVisualStyleBackColor = false;
            btnChannelPrivate.Click += btnChannelPrivate_Click;
            // 
            // btnChannelRoom
            // 
            btnChannelRoom.BackColor = System.Drawing.Color.FromArgb(60, 60, 80);
            btnChannelRoom.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnChannelRoom.Font = new System.Drawing.Font("微软雅黑", 9F);
            btnChannelRoom.ForeColor = System.Drawing.Color.White;
            btnChannelRoom.Location = new System.Drawing.Point(158, 8);
            btnChannelRoom.Margin = new System.Windows.Forms.Padding(4);
            btnChannelRoom.Name = "btnChannelRoom";
            btnChannelRoom.Size = new System.Drawing.Size(135, 45);
            btnChannelRoom.TabIndex = 1;
            btnChannelRoom.Text = "房间";
            btnChannelRoom.UseVisualStyleBackColor = false;
            btnChannelRoom.Click += btnChannelRoom_Click;
            // 
            // btnChannelWorld
            // 
            btnChannelWorld.BackColor = System.Drawing.Color.FromArgb(100, 150, 200);
            btnChannelWorld.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnChannelWorld.Font = new System.Drawing.Font("微软雅黑", 9F);
            btnChannelWorld.ForeColor = System.Drawing.Color.White;
            btnChannelWorld.Location = new System.Drawing.Point(8, 8);
            btnChannelWorld.Margin = new System.Windows.Forms.Padding(4);
            btnChannelWorld.Name = "btnChannelWorld";
            btnChannelWorld.Size = new System.Drawing.Size(135, 45);
            btnChannelWorld.TabIndex = 0;
            btnChannelWorld.Text = "全服";
            btnChannelWorld.UseVisualStyleBackColor = false;
            btnChannelWorld.Click += btnChannelWorld_Click;
            // 
            // panelRoom
            // 
            panelRoom.BackColor = System.Drawing.Color.FromArgb(35, 35, 45);
            panelRoom.Controls.Add(btnStartGame);
            panelRoom.Controls.Add(lblRoomTitle);
            panelRoom.Controls.Add(btnLeaveRoom);
            panelRoom.Controls.Add(btnReady);
            panelRoom.Controls.Add(panelPlayerRight);
            panelRoom.Controls.Add(panelPlayerLeft);
            panelRoom.Controls.Add(panelPlayerSelf);
            panelRoom.Location = new System.Drawing.Point(465, 120);
            panelRoom.Margin = new System.Windows.Forms.Padding(4);
            panelRoom.Name = "panelRoom";
            panelRoom.Size = new System.Drawing.Size(570, 780);
            panelRoom.TabIndex = 3;
            panelRoom.Visible = false;
            // 
            // btnStartGame
            // 
            btnStartGame.BackColor = System.Drawing.Color.FromArgb(200, 150, 50);
            btnStartGame.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnStartGame.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold);
            btnStartGame.ForeColor = System.Drawing.Color.White;
            btnStartGame.Location = new System.Drawing.Point(150, 615);
            btnStartGame.Margin = new System.Windows.Forms.Padding(4);
            btnStartGame.Name = "btnStartGame";
            btnStartGame.Size = new System.Drawing.Size(270, 68);
            btnStartGame.TabIndex = 5;
            btnStartGame.Text = "开始游戏";
            btnStartGame.UseVisualStyleBackColor = false;
            btnStartGame.Visible = false;
            btnStartGame.Click += btnStartGame_Click;
            // 
            // lblRoomTitle
            // 
            lblRoomTitle.AutoSize = true;
            lblRoomTitle.Font = new System.Drawing.Font("微软雅黑", 11F, System.Drawing.FontStyle.Bold);
            lblRoomTitle.ForeColor = System.Drawing.Color.White;
            lblRoomTitle.Location = new System.Drawing.Point(15, 18);
            lblRoomTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblRoomTitle.Name = "lblRoomTitle";
            lblRoomTitle.Size = new System.Drawing.Size(101, 30);
            lblRoomTitle.TabIndex = 6;
            lblRoomTitle.Text = "房间名称";
            // 
            // btnLeaveRoom
            // 
            btnLeaveRoom.BackColor = System.Drawing.Color.FromArgb(100, 60, 60);
            btnLeaveRoom.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnLeaveRoom.Font = new System.Drawing.Font("微软雅黑", 9F);
            btnLeaveRoom.ForeColor = System.Drawing.Color.White;
            btnLeaveRoom.Location = new System.Drawing.Point(210, 15);
            btnLeaveRoom.Margin = new System.Windows.Forms.Padding(4);
            btnLeaveRoom.Name = "btnLeaveRoom";
            btnLeaveRoom.Size = new System.Drawing.Size(150, 45);
            btnLeaveRoom.TabIndex = 4;
            btnLeaveRoom.Text = "离开房间";
            btnLeaveRoom.UseVisualStyleBackColor = false;
            btnLeaveRoom.Click += btnLeaveRoom_Click;
            // 
            // btnReady
            // 
            btnReady.BackColor = System.Drawing.Color.FromArgb(80, 150, 100);
            btnReady.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnReady.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold);
            btnReady.ForeColor = System.Drawing.Color.White;
            btnReady.Location = new System.Drawing.Point(150, 690);
            btnReady.Margin = new System.Windows.Forms.Padding(4);
            btnReady.Name = "btnReady";
            btnReady.Size = new System.Drawing.Size(270, 68);
            btnReady.TabIndex = 3;
            btnReady.Text = "准备";
            btnReady.UseVisualStyleBackColor = false;
            btnReady.Click += btnReady_Click;
            // 
            // panelPlayerRight
            // 
            panelPlayerRight.BackColor = System.Drawing.Color.FromArgb(50, 50, 60);
            panelPlayerRight.Controls.Add(lblRightLevel);
            panelPlayerRight.Controls.Add(lblRightStatus);
            panelPlayerRight.Controls.Add(lblRightName);
            panelPlayerRight.Controls.Add(picRightAvatar);
            panelPlayerRight.Location = new System.Drawing.Point(375, 106);
            panelPlayerRight.Margin = new System.Windows.Forms.Padding(4);
            panelPlayerRight.Name = "panelPlayerRight";
            panelPlayerRight.Size = new System.Drawing.Size(180, 225);
            panelPlayerRight.TabIndex = 2;
            // 
            // lblRightLevel
            // 
            lblRightLevel.Font = new System.Drawing.Font("微软雅黑", 8F);
            lblRightLevel.ForeColor = System.Drawing.Color.Gold;
            lblRightLevel.Location = new System.Drawing.Point(0, 202);
            lblRightLevel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblRightLevel.Name = "lblRightLevel";
            lblRightLevel.Size = new System.Drawing.Size(180, 22);
            lblRightLevel.TabIndex = 3;
            lblRightLevel.Text = "Lv.1";
            lblRightLevel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblRightStatus
            // 
            lblRightStatus.Font = new System.Drawing.Font("微软雅黑", 8F);
            lblRightStatus.ForeColor = System.Drawing.Color.Gray;
            lblRightStatus.Location = new System.Drawing.Point(0, 176);
            lblRightStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblRightStatus.Name = "lblRightStatus";
            lblRightStatus.Size = new System.Drawing.Size(180, 27);
            lblRightStatus.TabIndex = 2;
            lblRightStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblRightName
            // 
            lblRightName.Font = new System.Drawing.Font("微软雅黑", 9F);
            lblRightName.ForeColor = System.Drawing.Color.White;
            lblRightName.Location = new System.Drawing.Point(0, 146);
            lblRightName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblRightName.Name = "lblRightName";
            lblRightName.Size = new System.Drawing.Size(180, 30);
            lblRightName.TabIndex = 1;
            lblRightName.Text = "等待加入...";
            lblRightName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // picRightAvatar
            // 
            picRightAvatar.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            picRightAvatar.Location = new System.Drawing.Point(30, 15);
            picRightAvatar.Margin = new System.Windows.Forms.Padding(4);
            picRightAvatar.Name = "picRightAvatar";
            picRightAvatar.Size = new System.Drawing.Size(120, 120);
            picRightAvatar.TabIndex = 0;
            picRightAvatar.TabStop = false;
            // 
            // panelPlayerLeft
            // 
            panelPlayerLeft.BackColor = System.Drawing.Color.FromArgb(50, 50, 60);
            panelPlayerLeft.Controls.Add(lblLeftLevel);
            panelPlayerLeft.Controls.Add(lblLeftStatus);
            panelPlayerLeft.Controls.Add(lblLeftName);
            panelPlayerLeft.Controls.Add(picLeftAvatar);
            panelPlayerLeft.Location = new System.Drawing.Point(15, 106);
            panelPlayerLeft.Margin = new System.Windows.Forms.Padding(4);
            panelPlayerLeft.Name = "panelPlayerLeft";
            panelPlayerLeft.Size = new System.Drawing.Size(180, 225);
            panelPlayerLeft.TabIndex = 1;
            // 
            // lblLeftLevel
            // 
            lblLeftLevel.Font = new System.Drawing.Font("微软雅黑", 8F);
            lblLeftLevel.ForeColor = System.Drawing.Color.Gold;
            lblLeftLevel.Location = new System.Drawing.Point(0, 202);
            lblLeftLevel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblLeftLevel.Name = "lblLeftLevel";
            lblLeftLevel.Size = new System.Drawing.Size(180, 22);
            lblLeftLevel.TabIndex = 3;
            lblLeftLevel.Text = "Lv.1";
            lblLeftLevel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblLeftStatus
            // 
            lblLeftStatus.Font = new System.Drawing.Font("微软雅黑", 8F);
            lblLeftStatus.ForeColor = System.Drawing.Color.Gray;
            lblLeftStatus.Location = new System.Drawing.Point(0, 176);
            lblLeftStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblLeftStatus.Name = "lblLeftStatus";
            lblLeftStatus.Size = new System.Drawing.Size(180, 27);
            lblLeftStatus.TabIndex = 2;
            lblLeftStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblLeftName
            // 
            lblLeftName.Font = new System.Drawing.Font("微软雅黑", 9F);
            lblLeftName.ForeColor = System.Drawing.Color.White;
            lblLeftName.Location = new System.Drawing.Point(0, 146);
            lblLeftName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblLeftName.Name = "lblLeftName";
            lblLeftName.Size = new System.Drawing.Size(180, 30);
            lblLeftName.TabIndex = 1;
            lblLeftName.Text = "等待加入...";
            lblLeftName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // picLeftAvatar
            // 
            picLeftAvatar.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            picLeftAvatar.Location = new System.Drawing.Point(30, 15);
            picLeftAvatar.Margin = new System.Windows.Forms.Padding(4);
            picLeftAvatar.Name = "picLeftAvatar";
            picLeftAvatar.Size = new System.Drawing.Size(120, 120);
            picLeftAvatar.TabIndex = 0;
            picLeftAvatar.TabStop = false;
            // 
            // panelPlayerSelf
            // 
            panelPlayerSelf.BackColor = System.Drawing.Color.FromArgb(50, 50, 60);
            panelPlayerSelf.Controls.Add(lblSelfLevel);
            panelPlayerSelf.Controls.Add(lblSelfStatus);
            panelPlayerSelf.Controls.Add(lblSelfName);
            panelPlayerSelf.Controls.Add(picSelfAvatar);
            panelPlayerSelf.Location = new System.Drawing.Point(195, 376);
            panelPlayerSelf.Margin = new System.Windows.Forms.Padding(4);
            panelPlayerSelf.Name = "panelPlayerSelf";
            panelPlayerSelf.Size = new System.Drawing.Size(180, 225);
            panelPlayerSelf.TabIndex = 0;
            // 
            // lblSelfLevel
            // 
            lblSelfLevel.Font = new System.Drawing.Font("微软雅黑", 8F);
            lblSelfLevel.ForeColor = System.Drawing.Color.Gold;
            lblSelfLevel.Location = new System.Drawing.Point(0, 202);
            lblSelfLevel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblSelfLevel.Name = "lblSelfLevel";
            lblSelfLevel.Size = new System.Drawing.Size(180, 22);
            lblSelfLevel.TabIndex = 3;
            lblSelfLevel.Text = "Lv.1";
            lblSelfLevel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblSelfStatus
            // 
            lblSelfStatus.Font = new System.Drawing.Font("微软雅黑", 8F);
            lblSelfStatus.ForeColor = System.Drawing.Color.Gray;
            lblSelfStatus.Location = new System.Drawing.Point(0, 176);
            lblSelfStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblSelfStatus.Name = "lblSelfStatus";
            lblSelfStatus.Size = new System.Drawing.Size(180, 27);
            lblSelfStatus.TabIndex = 2;
            lblSelfStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblSelfName
            // 
            lblSelfName.Font = new System.Drawing.Font("微软雅黑", 9F);
            lblSelfName.ForeColor = System.Drawing.Color.White;
            lblSelfName.Location = new System.Drawing.Point(0, 146);
            lblSelfName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblSelfName.Name = "lblSelfName";
            lblSelfName.Size = new System.Drawing.Size(180, 30);
            lblSelfName.TabIndex = 1;
            lblSelfName.Text = "等待加入...";
            lblSelfName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // picSelfAvatar
            // 
            picSelfAvatar.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            picSelfAvatar.Location = new System.Drawing.Point(30, 15);
            picSelfAvatar.Margin = new System.Windows.Forms.Padding(4);
            picSelfAvatar.Name = "picSelfAvatar";
            picSelfAvatar.Size = new System.Drawing.Size(120, 120);
            picSelfAvatar.TabIndex = 0;
            picSelfAvatar.TabStop = false;
            // 
            // Lobby
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.FromArgb(25, 25, 35);
            ClientSize = new System.Drawing.Size(1500, 900);
            Controls.Add(panelRoom);
            Controls.Add(panelRight);
            Controls.Add(panelLeft);
            Controls.Add(panelTop);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            Margin = new System.Windows.Forms.Padding(4);
            Name = "Lobby";
            Opacity = 0D;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "游戏大厅";
            Load += Lobby_Load;
            MouseDown += Lobby_MouseDown;
            MouseMove += Lobby_MouseMove;
            MouseUp += Lobby_MouseUp;
            panelTop.ResumeLayout(false);
            panelTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picPlayerAvatar).EndInit();
            panelLeft.ResumeLayout(false);
            panelLeft.PerformLayout();
            panelRight.ResumeLayout(false);
            panelChatInput.ResumeLayout(false);
            panelChatInput.PerformLayout();
            panelChatChannels.ResumeLayout(false);
            panelRoom.ResumeLayout(false);
            panelRoom.PerformLayout();
            panelPlayerRight.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picRightAvatar).EndInit();
            panelPlayerLeft.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picLeftAvatar).EndInit();
            panelPlayerSelf.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picSelfAvatar).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Timer timerNetwork;
        private System.Windows.Forms.Timer timerFadeIn;
        private System.Windows.Forms.Timer timerRefresh;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Button btnLogout;
        private System.Windows.Forms.Button btnSettings;
        private System.Windows.Forms.Label lblPlayerLosses;
        private System.Windows.Forms.Label lblPlayerWins;
        private System.Windows.Forms.Label lblPlayerBeans;
        private System.Windows.Forms.Label lblPlayerLevel;
        private System.Windows.Forms.Label lblPlayerName;
        private System.Windows.Forms.PictureBox picPlayerAvatar;
        private System.Windows.Forms.Panel panelLeft;
        private System.Windows.Forms.Label lblMatchStatus;
        private System.Windows.Forms.Button btnQuickMatch;
        private System.Windows.Forms.Button btnJoinRoom;
        private System.Windows.Forms.Button btnCreateRoom;
        private System.Windows.Forms.ListBox lstRooms;
        private System.Windows.Forms.Label lblRoomList;
        private System.Windows.Forms.Panel panelRight;
        private System.Windows.Forms.Panel panelChatInput;
        private System.Windows.Forms.Button btnSendChat;
        private System.Windows.Forms.Button btnEmoji;
        private System.Windows.Forms.TextBox txtChatInput;
        private System.Windows.Forms.ListBox lstChatMessages;
        private System.Windows.Forms.Panel panelChatChannels;
        private System.Windows.Forms.Button btnChannelPrivate;
        private System.Windows.Forms.Button btnChannelRoom;
        private System.Windows.Forms.Button btnChannelWorld;
        private System.Windows.Forms.Panel panelRoom;
        private System.Windows.Forms.Button btnLeaveRoom;
        private System.Windows.Forms.Button btnReady;
        private System.Windows.Forms.Panel panelPlayerRight;
        private System.Windows.Forms.Label lblRightLevel;
        private System.Windows.Forms.Label lblRightStatus;
        private System.Windows.Forms.Label lblRightName;
        private System.Windows.Forms.PictureBox picRightAvatar;
        private System.Windows.Forms.Panel panelPlayerLeft;
        private System.Windows.Forms.Label lblLeftLevel;
        private System.Windows.Forms.Label lblLeftStatus;
        private System.Windows.Forms.Label lblLeftName;
        private System.Windows.Forms.PictureBox picLeftAvatar;
        private System.Windows.Forms.Panel panelPlayerSelf;
        private System.Windows.Forms.Label lblSelfLevel;
        private System.Windows.Forms.Label lblSelfStatus;
        private System.Windows.Forms.Label lblSelfName;
        private System.Windows.Forms.PictureBox picSelfAvatar;
        private System.Windows.Forms.Button btnStartGame;
        private System.Windows.Forms.Label lblRoomTitle;
    }
}