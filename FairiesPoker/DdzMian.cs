using System;
using System.Collections;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Threading;
using System.Windows.Forms;
using Protocol.Code;
using Protocol.Dto;
using Protocol.Dto.Fight;
using Protocol.Constant;
using System.Collections.Generic;
public delegate void Myweituo1(int status,bool visible);
namespace FairiesPoker
{
    public partial class DdzMian : Form
    {
        /// <summary>
        /// 面向对象编程中型项目首次实践（斗地主单机版）
        /// </summary>
        #region 所有属性
        Point mouseOff;
        public win win;
        private UI ui = new UI();
        private config con = new config();
        private Pai[] pai;
        private Juese juese1;
        private Juese juese2;
        private Juese juese3;
        private Chupai chupai;
        private Jiepai jiepai;
        private GameThreadManager tm_faPai;
        private GameThreadManager tm_daPai;
        private PictureBox[] paiImage;
        private ComputerChuPai Cchupai;
        private SoundPlayer SoundLoss;
        private SoundPlayer SoundWin;
        private SoundPlayer SoundClick;
        private SoundPlayer SoundGive;
        private Myweituo1 weituo1;
        private ArrayList saveList;
        private int buChuPai = 0;
        private int chuPaiWeiZhi;
        private int tishi = 0;
        private bool bl_isDiZhu = false;
        private bool noDiZhu = false;
        private bool bl_isFirst = false;
        private bool bl_chuPaiOver;
        private bool leftFlag;
        private bool online = false;

        // 多人模式新增字段
        private NetManager netManager;
        private List<CardDto> myCardList; // 多人模式手牌
        private int landlordId = -1; // 地主玩家ID
        private int currentTurnUserId = -1; // 当前出牌玩家ID
        private DealDto lastDealDto; // 上一次出牌信息
        private bool isMyTurn = false; // 是否轮到我出牌
        private bool needFollow = false; // 是否需要接牌（不是首出）
        private int lastPaiType = 0; // 上一次出牌的牌型
        private int turnTimeoutSeconds = 20; // 出牌超时时间
        private int remainingSeconds = 20; // 剩余秒数
        private int otherPlayerRemainingSeconds = 20; // 其他玩家剩余秒数
        private List<CardDto> tableCards; // 底牌（三张）
        private List<PictureBox> myCardPictureBoxes; // 多人模式手牌PictureBox列表
        private List<PictureBox> dealCardImages; // 出牌显示用的PictureBox列表
        private List<int> selectedCardIndices; // 选中的牌索引
        private PictureBox picLeftAvatar; // 左边玩家头像
        private PictureBox picRightAvatar; // 右边玩家头像
        private PictureBox picSelfAvatar; // 自己头像
        private List<PictureBox> leftPlayerCardBacks; // 左边玩家牌背
        private List<PictureBox> rightPlayerCardBacks; // 右边玩家牌背
        // 滑动选牌相关字段
        private bool isSlidingSelection = false; // 是否正在滑动选牌
        private int slideStartIndex = -1; // 滑动起始牌索引
        private int slideEndIndex = -1; // 滑动结束牌索引
        private bool slideSelectMode = true; // 滑动选牌模式（true=选中，false=取消选中）
        #endregion
        #region 窗体设置
        public DdzMian(bool online)
        {
            this.online = online;
            InitializeComponent();
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            ui.setUI(con.UI); this.BackgroundImage = ui.Background;
            Opacity = 0;timer1.Start();

            if (online)
            {
                // 多人模式初始化
                netManager = NetManager.Instance;
                myCardList = new List<CardDto>();
                tableCards = new List<CardDto>();

                // 订阅游戏事件
                Models.OnGetCards += OnGetCardsReceived;
                Models.OnTurnGrab += OnTurnGrabReceived;
                Models.OnGrabLandlord += OnGrabLandlordReceived;
                Models.OnTurnDeal += OnTurnDealReceived;
                Models.OnDealBroadcast += OnDealBroadcastReceived;
                Models.OnDealResponse += OnDealResponseReceived;
                Models.OnPassResponse += OnPassResponseReceived;
                Models.OnGameOver += OnGameOverReceived;
                Models.OnMultipleChange += OnMultipleChangeReceived;
            }
        }
        private void DdzMian_Load(object sender, EventArgs e)
        {
            int i = 1; CheckForIllegalCrossThreadCalls = false;
            for (i = 1; i <=8; i++)
            {
                Control[] a = this.Controls.Find("button" + i.ToString(), true);
                ((Button)(a[0])).BackgroundImage = ui.Button;
                ((Button)(a[0])).MouseDown += DdzMian_MouseDown1;
                ((Button)(a[0])).MouseUp += DdzMian_MouseUp1;
            }
            string resourcePath = Application.StartupPath + "\\Resources\\";
            SoundLoss = new SoundPlayer(resourcePath + "5538.wav");
            SoundWin = new SoundPlayer(resourcePath + "5553.wav");
            SoundClick = new SoundPlayer(resourcePath + "click.wav");
            SoundGive = new SoundPlayer(resourcePath + "give.wav");
            audioPlayer = new AudioPlayer();
            this.Focus();

            if (online)
            {
                // 多人模式：隐藏开始按钮，显示玩家信息，等待服务器发牌
                button1.Visible = false;
                groupBox1.Visible = true;
                groupBox2.Visible = true;
                groupBox3.Visible = true;

                // 创建头像PictureBox
                CreateAvatarPictureBoxes();

                // 设置玩家名称和头像
                var matchRoom = Models.GameModel.MatchRoomDto;
                if (matchRoom != null)
                {
                    int myId = Models.GameModel.UserDto.Id;
                    // 自己
                    groupBox2.Text = Models.GameModel.UserDto.Name;
                    SetAvatarImage(picSelfAvatar, Models.GameModel.UserDto.AvatarUrl);

                    // 左边玩家
                    if (matchRoom.LeftId > 0 && matchRoom.UIdUserDict.ContainsKey(matchRoom.LeftId))
                    {
                        groupBox1.Text = matchRoom.UIdUserDict[matchRoom.LeftId].Name;
                        SetAvatarImage(picLeftAvatar, matchRoom.UIdUserDict[matchRoom.LeftId].AvatarUrl);
                    }
                    // 右边玩家
                    if (matchRoom.RightId > 0 && matchRoom.UIdUserDict.ContainsKey(matchRoom.RightId))
                    {
                        groupBox3.Text = matchRoom.UIdUserDict[matchRoom.RightId].Name;
                        SetAvatarImage(picRightAvatar, matchRoom.UIdUserDict[matchRoom.RightId].AvatarUrl);
                    }
                }

                // 启动网络更新定时器
                timerNetwork.Interval = 50;
                timerNetwork.Tick += timerNetwork_Tick;
                timerNetwork.Start();
            }
        }

        /// <summary>
        /// 创建玩家头像PictureBox
        /// </summary>
        private void CreateAvatarPictureBoxes()
        {
            // 左边玩家头像
            picLeftAvatar = new PictureBox();
            picLeftAvatar.Size = new Size(50, 50);
            picLeftAvatar.Location = new Point(90, 20);
            picLeftAvatar.SizeMode = PictureBoxSizeMode.StretchImage;
            picLeftAvatar.BackgroundImage = Properties.Resources.Pla;
            picLeftAvatar.BackgroundImageLayout = ImageLayout.Stretch;
            groupBox1.Controls.Add(picLeftAvatar);

            // 自己头像
            picSelfAvatar = new PictureBox();
            picSelfAvatar.Size = new Size(50, 50);
            picSelfAvatar.Location = new Point(90, 20);
            picSelfAvatar.SizeMode = PictureBoxSizeMode.StretchImage;
            picSelfAvatar.BackgroundImage = Properties.Resources.Pla;
            picSelfAvatar.BackgroundImageLayout = ImageLayout.Stretch;
            groupBox2.Controls.Add(picSelfAvatar);

            // 右边玩家头像
            picRightAvatar = new PictureBox();
            picRightAvatar.Size = new Size(50, 50);
            picRightAvatar.Location = new Point(90, 20);
            picRightAvatar.SizeMode = PictureBoxSizeMode.StretchImage;
            picRightAvatar.BackgroundImage = Properties.Resources.Pla;
            picRightAvatar.BackgroundImageLayout = ImageLayout.Stretch;
            groupBox3.Controls.Add(picRightAvatar);
        }

        /// <summary>
        /// 设置头像图片
        /// </summary>
        private void SetAvatarImage(PictureBox pictureBox, string avatarUrl)
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

        // 多人模式网络更新定时器
        private System.Windows.Forms.Timer timerNetwork = new System.Windows.Forms.Timer();
        private void timerNetwork_Tick(object sender, EventArgs e)
        {
            if (netManager != null)
            {
                netManager.Update();
            }
        }
        #endregion
        #region NEW出3个角色
        private void newPlayer()
        {
            juese1 = new Juese();
            juese2 = new Juese();
            juese3 = new Juese();
            juese1.WeiZhi = 1;
            juese2.WeiZhi = 2;
            juese3.WeiZhi = 3;
        }
        #endregion
        #region 开局初始化
        private void load()
        {
            pai = new Pai[54];
            paiImage = new PictureBox[54];
            weituo1 = new Myweituo1(buttonset);
            tm_faPai = new GameThreadManager();
            tm_daPai = new GameThreadManager();
            chupai = new Chupai();
            jiepai = new Jiepai();
            saveList = new ArrayList();
            Cchupai = new ComputerChuPai();
            newPlayer();
            newpai();
            suiji1();
            newPaiImage();
            pai_paixu(juese1);
            pai_paixu(juese2);
            pai_paixu(juese3);
            addJuesePai(juese1);
            addJuesePai(juese2);
            addJuesePai(juese3);
        }
        private void newpai ()
        {
            Pai dw = new Pai("", 17);
            Pai xw = new Pai("", 16);
            pai[0] = dw;pai[1] = xw;
            Pai[,] pai1 = new Pai[4,13];
            int k = 0;
            for (int i = 0; i < 4; i++)
            {
                int j = 0;
                string huasetemp="";
                switch (i)
                {
                    case 0:huasetemp = "heitao";
                        break;
                    case 1:huasetemp = "hongtao";
                        break;
                    case 2:huasetemp = "meihua";
                        break;
                    case 3:huasetemp = "fangkuai";
                        break;
                }
                for (j=0; j < 13; j++)
                {
                    pai1[i, j] = new Pai(huasetemp,j+3);
                    pai[k + j + 2] = pai1[i, j];
                }
                k = k + j;
            }
        }
        private void suiji1()
        {
            Random rd = new Random();
            for (int i = 0; i < pai.Length; i++)
            {
                int k = rd.Next(54);
                if (i == 0)
                {
                    pai[i].Index = k;
                }
                for (int j = 0; j < i; j++)
                {
                    if (pai[j].Index == k)
                    {
                        i--;
                        break;
                    }
                    else if (j == i - 1)
                    {
                        pai[i].Index = k;
                    }
                }
            }
        }
        private void newPaiImage()
        {
            int a = 810;
            for (int i = 0; i < paiImage.Length; i++)
            {
                paiImage[pai[i].Index] = new PictureBox();
                paiImage[pai[i].Index].SetBounds(a, 170, 150, 225);
                paiImage[pai[i].Index].BackgroundImage = Properties.Resources.牌背3;
                paiImage[pai[i].Index].BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
                this.Controls.Add(this.paiImage[pai[i].Index]);
                if (i < 51)
                {
                    if (i % 3 == 0) juese1.ImagePaiSub.Add(pai[i].Index);
                    else if ((i + 2) % 3 == 0)
                    {
                        int cardIndex = juese2.ImagePaiSub.Count; // 当前牌的位置索引
                        juese2.ImagePaiSub.Add(pai[i].Index);
                        paiImage[pai[i].Index].Tag = cardIndex; // 设置位置索引作为Tag
                        paiImage[pai[i].Index].Click += new System.EventHandler(paiImage_Click);
                        // 添加滑动选牌事件
                        paiImage[pai[i].Index].MouseDown += new MouseEventHandler(PaiImage_MouseDown);
                        paiImage[pai[i].Index].MouseMove += new MouseEventHandler(PaiImage_MouseMove);
                        paiImage[pai[i].Index].MouseUp += new MouseEventHandler(PaiImage_MouseUp);
                    }
                    else juese3.ImagePaiSub.Add(pai[i].Index);
                }
                a -= 10;
            }
        }
        #endregion
        #region 排序--牌
        private void pai_paixu(Juese juese)
        {
            for (int i = 0; i < juese.ImagePaiSub.Count; i++)
            {
                for (int j = i; j < juese.ImagePaiSub.Count; j++)
                {
                    if (pai[(int)juese.ImagePaiSub[i]].Size < pai[(int)juese.ImagePaiSub[j]].Size)
                    {
                        int temp = (int)juese.ImagePaiSub[i];
                        juese.ImagePaiSub[i] = juese.ImagePaiSub[j];
                        juese.ImagePaiSub[j] = temp;
                    }
                }
            }
        }
        #endregion
        #region 排序--牌图形
        private void image_paixu(Juese juese, int j)
        {
            if (juese.ShengYuPai.Count != 0)
            {
                if (juese.WeiZhi == 2)
                {
                    pai_paixu(juese);
                    int a = 0;
                    for (int i = juese.ImagePaiSub.Count - 1; i >= 0; i--)
                    {
                        paiImage[(int)juese.ImagePaiSub[a]].BringToFront();
                        paiImage[(int)juese.ImagePaiSub[i]].Left = j;
                        a++; j -= 30;
                    }
                    pai_paixu(juese);
                    // 更新Tags以反映排序后的位置
                    for (int i = 0; i < juese.ImagePaiSub.Count; i++)
                    {
                        paiImage[(int)juese.ImagePaiSub[i]].Tag = i;
                    }
                }
                else
                {
                    int k = 220; int a = 0;
                    for (int i = 0; i < juese.ImagePaiSub.Count; i++)
                    {
                        paiImage[(int)juese.ImagePaiSub[i]].Top = k;
                        paiImage[(int)juese.ImagePaiSub[a]].BringToFront();a++;
                    }
                }
            }
        }
        #endregion
        #region 添加值
        private void addJuesePai(Juese juese)
        {
            for (int i = 0; i < juese.ImagePaiSub.Count; i++)
            {
                juese.ShengYuPai.Add(pai[(int)juese.ImagePaiSub[i]].Size);
            }
        }
        #endregion
        #region 发牌
        private void fapai ()
        {
            int[] a = new int[] {325,483,440,6 };
            for (int i = 0; i < 51; i++)
            {
                paiImage[pai[i].Index].BringToFront();
                paiImage[pai[i].Index].SetBounds(1110, 220, 150, 225);
                Thread.Sleep(80);i++;
                paiImage[pai[i].Index].BringToFront();
                paiImage[pai[i].Index].SetBounds(a[0],a[1],150,225);
                paiImage[pai[i].Index].BackgroundImage = pai[pai[i].Index].Image;
                Thread.Sleep(80);i++;a[0] += 30;
                paiImage[pai[i].Index].BringToFront();
                paiImage[pai[i].Index].SetBounds(20,220,150,225);
                Thread.Sleep(80);
            }
            for (int i = 51; i < 54; i++)
            {
                paiImage[pai[i].Index].BringToFront();
                paiImage[pai[i].Index].SetBounds(a[2],a[3],120,180);
                a[2] += 140;
            }
            image_paixu(juese2, 805);
            image_paixu(juese1, 805);
            image_paixu(juese3, 805);
            Random rd = new Random();
            int num = rd.Next(3) + 1;
            Thread.Sleep(500);
            int switchDiZhu = 0;
            bool bl1 = false; bool bl2 = false; bool bl3 = false; int count = 0;
            if (con.BackMusic)
            {
                string mpath = "";
                switch (ui.uiselect)
                {
                    case 1:mpath = Path.UI_TB.ToString();
                        break;
                    case 2:mpath = Path.UI_LT.ToString();
                        break;
                    case 3:mpath = Path.UI_FR.ToString();
                        break;
                    case 4:mpath = Path.UI_SW.ToString();
                        break;
                    case 5:mpath = Path.UI_PF.ToString();
                        break;
                    case 6:mpath = Path.UI_LN.ToString();
                        break;
                    case 7:mpath = Path.UI_LN.ToString();
                        break;
                }
                audioPlayer.Play(Application.StartupPath + "\\" + mpath + "\\background.mp3", true);
            }
            do
            {
                switch (num)
                {
                    case 1:
                        if (bl1 == false)
                        {
                            bl1 = true; count++; num++;
                            if (isJiaoDiZhu(juese3))
                            {
                                count = 3; juese3.Dizhu = true; switchDiZhu = 3;
                                label1.Text = "叫地主"; Thread.Sleep(1500);
                            }
                            else
                            { label1.Text = "不  叫"; Thread.Sleep(1500); }
                        }
                        break;
                    case 2:
                        if (bl2 == false)
                        {
                            bl2 = true; count++; num++;
                            if (isJiaoDiZhu(juese1))
                            {
                                count = 3; juese1.Dizhu = true; switchDiZhu = 1;
                                label2.Text = "叫地主"; Thread.Sleep(1500);
                            }
                            else { label2.Text = "不  叫"; Thread.Sleep(1500); }
                        }
                        break;
                    case 3:
                        if (bl3 == false)
                        {
                            bl3 = true; count++; num = 1;
                            this.button1.Invoke(weituo1,1,true);
                            tm_faPai.Pause();
                            tm_faPai.WaitOne();
                            if (tm_faPai.ShouldStop()) return;
                            if (bl_isDiZhu)
                            {
                                count = 3; juese2.Dizhu = true; switchDiZhu = 2;
                                label3.Text = "叫地主"; Thread.Sleep(500);
                            }
                            else { label3.Text = "不  叫"; Thread.Sleep(1500); }
                        }
                        break;
                }
            } while (count != 3);
            Thread.Sleep(500);
            label1.Text = "";label2.Text = "";label3.Text = "";
            this.button1.Invoke(weituo1,3,false);
            if (switchDiZhu !=0)
            {
                pic1.SetBounds(pic1.Left, pic1.Top, 120, 180);
                pic2.SetBounds(pic2.Left, pic2.Top, 120, 180);
                pic3.SetBounds(pic3.Left, pic3.Top, 120, 180);
                pic1.BackgroundImage = pai[pai[51].Index].Image;
                pic2.BackgroundImage = pai[pai[52].Index].Image;
                pic3.BackgroundImage = pai[pai[53].Index].Image;
                if (switchDiZhu==1)
                {
                    paizhi(juese2,juese3);
                    kouDiPai(juese1);name();xianshi();
                    pic_dz.SetBounds(1190, 144, 60, 60);
                    pic_dz.Visible = true; pic_dz.BringToFront();
                    pic_dz.Image = Properties.Resources.hook;
                }
                else if (switchDiZhu==2)
                {
                    paizhi(juese1, juese3);
                    kouDiPai(juese2);name();xianshi();
                    pic_dz.SetBounds(1090, 645, 60, 60);
                    pic_dz.Visible = true; pic_dz.BringToFront();
                    pic_dz.Image = Properties.Resources.hook;
                }
                else if (switchDiZhu==3)
                {
                    paizhi(juese1, juese2);
                    kouDiPai(juese3);name();xianshi();
                    pic_dz.SetBounds(100, 463, 60, 60);
                    pic_dz.Visible = true; pic_dz.BringToFront();
                    pic_dz.Image = Properties.Resources.hook;
                }
            }
            else
            {
                MessageBox.Show("没有人选择地主，本局结束！","Warning",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                noDiZhu = true;
            }
            return;
        }
        #endregion
        #region 按钮设置
        private void buttonset (int status,bool visible)
        {
            if (status==1)
            {
                button2.Visible = visible;
                button3.Visible = visible;
                button2.Text = "叫地主";
                button3.Text = "不叫";
            }
            else if (status==2)
            {
                button2.Left = 547;
                button4.Visible = visible;
                button6.Visible = visible;
                button2.Visible = visible;
                button3.Visible = visible;
                button4.Text = "重选";
                button2.Text = "出牌";
                button3.Text = "不出";
                button6.Text = "提示";
            }
            else if (status==3)
            {
                button2.Left = 547;
                button4.Visible = visible;
                button6.Visible = visible;
                button2.Visible = visible;
                button3.Visible = visible;
                button4.Text = button2.Text = button3.Text = button6.Text = "";
            }
            else if (status==4)
            {
                button2.Left = 595;
                button2.Visible = visible;
                button2.Text = "出牌";
            }
        }
        #endregion
        #region 电脑叫地主
        private bool isJiaoDiZhu (Juese juese)
        {
            int quanzhi = 0;
            foreach (int size in juese.ShengYuPai)
            {
                switch (size)
                {
                    case 14:quanzhi += 1;
                        break;
                    case 15:quanzhi += 2;
                        break;
                    case 16:quanzhi += 3;
                        break;
                    case 17:quanzhi += 4;
                        break;
                }
            }
            if (quanzhi >= 7) return true;
            else return false;
        }
        #endregion
        #region 打牌主流程
        private void daPai ()
        {
            tm_faPai.Start(new ThreadStart(fapai));
            tm_faPai.Join();
            if (noDiZhu)
            {
                noDiZhu = false; chongZhi();
                return;
            }
            int num = 0;
            if (juese1.Dizhu)
            {
                num = 2; Thread.Sleep(2000); computerChuPai(juese1);soundFx(1); shengyupai();
            }
            else if (juese2.Dizhu)
            {
                num = 1; this.button1.Invoke(weituo1,4, true);
                tm_daPai.Pause();
                tm_daPai.WaitOne();
                if (tm_daPai.ShouldStop()) return;
                shengyupai();
            }
            else if (juese3.Dizhu)
            {
                num = 3; Thread.Sleep(2000); computerChuPai(juese3);soundFx(1); shengyupai();
            }
            bl_isFirst = true;
            do
            {
                #region 角色一出牌或接牌
                if (num == 1)
                {
                    num++; Thread.Sleep(1000);
                    if (buChuPai == 2)
                    {
                        computerChuPai(juese1); buChuPai = 0;
                    }
                    else computerJiePai(juese1);
                    this.label3.Text = "";
                    shengyupai();
                }
                if (bl_chuPaiOver)
                {
                    ShowPai(juese3, jiepai.arrayToArgs(juese3.ShengYuPai));
                    juese3.ShengYuPai.Add(0);
                    break;
                }
                #endregion
                #region 角色三出牌或接牌
                if (num == 2)
                {
                    num++;Thread.Sleep(1000);
                    if (buChuPai == 2)
                    {
                        computerChuPai(juese3); buChuPai = 0;
                    }
                    else computerJiePai(juese3);
                    this.label2.Text = "";
                    shengyupai();
                }
                if (bl_chuPaiOver)
                {
                    ShowPai(juese1, jiepai.arrayToArgs(juese1.ShengYuPai));
                    juese1.ShengYuPai.Add(0);
                    break;
                }
                #endregion
                #region 角色二出牌或接牌(当前玩家)
                if (num == 3)
                {
                    num = 1;
                    if (buChuPai == 2) this.button1.Invoke(weituo1, 4, true);
                    else this.button1.Invoke(weituo1, 2, true);
                    tm_daPai.Pause();
                    tm_daPai.WaitOne();
                    if (tm_daPai.ShouldStop()) return;
                    this.label1.Text = "";
                    shengyupai();
                }
                if (bl_chuPaiOver)
                {
                    ShowPai(juese1, jiepai.arrayToArgs(juese1.ShengYuPai));
                    ShowPai(juese3, jiepai.arrayToArgs(juese3.ShengYuPai));
                    juese1.ShengYuPai.Add(0); juese3.ShengYuPai.Add(0);
                    break;
                }
                #endregion
            } while (true);
            #region 出牌结束
            if (juese2.ShengYuPai.Count == 0) soundFx(2);
            else if (juese1.Dizhu && juese1.ShengYuPai.Count != 0 || juese3.Dizhu && juese3.ShengYuPai.Count != 0) soundFx(2);
            else soundFx(3);
            chongZhi();
            return;
            #endregion
        }
        #endregion
        #region 电脑出牌
        private void computerChuPai(Juese juese)
        {
            yichu();

            // 初始化AI（每次出牌前确保AI知道当前游戏状态）
            int landlordPos = 0;
            bool isCurrentLandlord = juese.Dizhu;
            if (juese1.Dizhu) landlordPos = 1;
            else if (juese2.Dizhu) landlordPos = 2;
            else if (juese3.Dizhu) landlordPos = 3;

            Cchupai.InitializeGame(juese.WeiZhi, isCurrentLandlord, landlordPos);

            ArrayList list = Cchupai.chuPai(juese.ShengYuPai);
            System.Diagnostics.Debug.WriteLine($"computerChuPai: list={(list == null ? "null" : list.Count.ToString())}");
            if (list != null && chupai.isRight(list))
            {
                System.Diagnostics.Debug.WriteLine($"computerChuPai: chupai.PaiType={chupai.PaiType}");
                juese1.ShangShouPai.Clear(); juese2.ShangShouPai.Clear(); juese3.ShangShouPai.Clear();
                movePai(juese, jiepai.arrayToArgs(list));soundFx(1);
            }
            else MessageBox.Show("程序出问题啦! 请与作者联系! QQ 842590128","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
        }
        #endregion
        #region 牌移动
        private void movePai(Juese juese, int[] whatPai)
        {
            juese.ShangShouPai.Clear();
            chupai.format(whatPai);
            int j = 0; chuPaiWeiZhi = 640 - (whatPai.Length * 30 + 120) / 2;
            for (int i = 0; i < juese.ImagePaiSub.Count; i++)
            {
                if (pai[(int)juese.ImagePaiSub[i]].Size == whatPai[j])
                {
                    paiImage[(int)juese.ImagePaiSub[i]].BringToFront();
                    switch (juese.WeiZhi)
                    {
                        case 1:
                            juese3.ShangShouPai.Add(pai[(int)juese.ImagePaiSub[i]].Size);
                            paiImage[(int)juese.ImagePaiSub[i]].SetBounds(chuPaiWeiZhi, 193, 150, 225);
                            paiImage[(int)juese.ImagePaiSub[i]].BackgroundImage = pai[(int)juese.ImagePaiSub[i]].Image;
                            break;
                        case 2:
                            juese1.ShangShouPai.Add(pai[(int)juese.ImagePaiSub[i]].Size);
                            paiImage[(int)juese.ImagePaiSub[i]].SetBounds(chuPaiWeiZhi, 193, 150, 225);
                            break;
                        case 3:
                            juese2.ShangShouPai.Add(pai[(int)juese.ImagePaiSub[i]].Size);
                            paiImage[(int)juese.ImagePaiSub[i]].SetBounds(chuPaiWeiZhi, 193, 150, 225);
                            paiImage[(int)juese.ImagePaiSub[i]].BackgroundImage = pai[(int)juese.ImagePaiSub[i]].Image;
                            break;
                    }
                    juese.YiChuPai.Add((int)juese.ImagePaiSub[i]);
                    juese.ShengYuPai.Remove(pai[(int)juese.ImagePaiSub[i]].Size);
                    juese.ImagePaiSub.RemoveAt(i);
                    chuPaiWeiZhi += 30 ; j++; i--;
                    if (j == whatPai.Length) break;
                }
            }
            if (juese.ShengYuPai.Count == 0) bl_chuPaiOver = true;
            int temp = 640 + (juese.ShengYuPai.Count * 30 + 120) / 2 - 150;
            image_paixu(juese, temp);
        }
        #endregion
        #region 电脑接牌
        private void computerJiePai(Juese juese)
        {
            chuPaiWeiZhi = 640 - (juese.ShangShouPai.Count * 30 + 120) / 2;

            // 调试信息：检查关键参数
            System.Diagnostics.Debug.WriteLine($"电脑接牌: PaiType={chupai.PaiType}, ShangShouPai.Count={juese.ShangShouPai?.Count ?? -1}, ShengYuPai.Count={juese.ShengYuPai?.Count ?? -1}");

            // 检查ShangShouPai是否为空
            if (juese.ShangShouPai == null || juese.ShangShouPai.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("ShangShouPai为空，无法接牌");
                if (juese == juese1)
                {
                    this.label2.Text = "不出"; soundFx(0);
                }
                else if (juese == juese2)
                {
                    this.label3.Text = "不出"; soundFx(0);
                }
                else
                {
                    this.label1.Text = "不出"; soundFx(0);
                }
                buChuPai++;
                return;
            }

            // 检查PaiType是否有效
            if (chupai.PaiType <= 0)
            {
                System.Diagnostics.Debug.WriteLine($"PaiType无效: {chupai.PaiType}");
                if (juese == juese1)
                {
                    this.label2.Text = "不出"; soundFx(0);
                    juese3.ShangShouPai = (ArrayList)juese.ShangShouPai.Clone();
                }
                else if (juese == juese2)
                {
                    this.label3.Text = "不出"; soundFx(0);
                    juese1.ShangShouPai = (ArrayList)juese.ShangShouPai.Clone();
                }
                else
                {
                    this.label1.Text = "不出"; soundFx(0);
                    juese2.ShangShouPai = (ArrayList)juese.ShangShouPai.Clone();
                }
                buChuPai++; juese.ShangShouPai.Clear();
                return;
            }

            // 直接使用原来的提示接牌逻辑（AI版本，不显示提示）
            ArrayList possibleMoves = jiepai.isRight(chupai.PaiType, juese.ShangShouPai, juese.ShengYuPai);
            System.Diagnostics.Debug.WriteLine($"jiepai.isRight返回: {(possibleMoves == null ? "null" : possibleMoves.Count.ToString())}");

            bool bl = tiShiJiePai(possibleMoves, juese, false);
            if (bl == false)
            {
                if (juese == juese1)
                {
                    this.label2.Text = "不出";soundFx(0);
                    juese3.ShangShouPai = (ArrayList)juese.ShangShouPai.Clone();
                }
                else if (juese == juese2)
                {
                    this.label3.Text = "不出";soundFx(0);
                    juese1.ShangShouPai = (ArrayList)juese.ShangShouPai.Clone();
                }
                else
                {
                    this.label1.Text = "不出";soundFx(0);
                    juese2.ShangShouPai = (ArrayList)juese.ShangShouPai.Clone();
                }
                buChuPai++; juese.ShangShouPai.Clear();
            }
            else
            {
                buChuPai = 0;
                soundFx(1);
            }
        }
        #endregion
        #region 隐藏已出牌
        private void yiChuPai(Juese juese)
        {
            for (int i = 0; i < juese.YiChuPai.Count; i++)
            {
                if (paiImage[(int)juese.YiChuPai[i]].Visible == true)
                {
                    paiImage[(int)juese.YiChuPai[i]].Visible = false;
                }
            }
        }
        #endregion
        #region 提示接牌
        private bool tiShiJiePai(ArrayList list, Juese juese, bool bl_tishi)
        {
            System.Diagnostics.Debug.WriteLine($"tiShiJiePai: PaiType={chupai.PaiType}, list={(list == null ? "null" : list.Count.ToString())}");

            if (chupai.PaiType == (int)Guize.天炸) return false;
            #region 单张
            else if (chupai.PaiType == (int)Guize.一张)
            {
                System.Diagnostics.Debug.WriteLine("处理单张...");
                if (list != null)
                {
                    int[] jie = null;
                    if (((ArrayList)list[0]).Count != 0) jie = jiepai.mArrayToArgs((ArrayList)list[0]);
                    else if (((ArrayList)list[1]).Count != 0) jie = jiepai.mArrayToArgs((ArrayList)list[1]);
                    else if (((ArrayList)list[2]).Count != 0) jie = jiepai.mArrayToArgs((ArrayList)list[2]);
                    else if (((ArrayList)list[3]).Count != 0) jie = jiepai.mArrayToArgs((ArrayList)list[3]);

                    System.Diagnostics.Debug.WriteLine($"单张jie: {(jie == null ? "null" : string.Join(",", jie))}");
                    if (jie != null)
                    {
                        if (tishi == jie.Length) tishi = 0;
                        int[] _jie = new int[] { jie[tishi] };
                        if (bl_tishi) tiShiBottun(_jie);
                        else { yichu(); movePai(juese, _jie); }
                        return true;
                    }
                }
            }
            #endregion
            #region 对子
            else if (chupai.PaiType == (int)Guize.对子)
            {
                System.Diagnostics.Debug.WriteLine("处理对子...");
                if (list != null)
                {
                    int[] jie = null;
                    if (((ArrayList)list[0]).Count != 0) jie = jiepai.mArrayToArgs((ArrayList)list[0]);
                    else if (((ArrayList)list[1]).Count != 0) jie = jiepai.mArrayToArgs((ArrayList)list[1]);
                    else if (((ArrayList)list[2]).Count != 0) jie = jiepai.mArrayToArgs((ArrayList)list[2]);

                    System.Diagnostics.Debug.WriteLine($"对子jie: {(jie == null ? "null" : string.Join(",", jie))}");
                    if (jie != null)
                    {
                        if (tishi == jie.Length) tishi = 0;
                        int[] _jie = new int[] { jie[tishi], jie[tishi] };
                        if (bl_tishi) tiShiBottun(_jie);
                        else { yichu(); movePai(juese, _jie); }
                        return true;
                    }
                }
            }
            #endregion
            #region 三张
            else if (chupai.PaiType == (int)Guize.三不带)
            {
                System.Diagnostics.Debug.WriteLine("处理三张...");
                if (list != null)
                {
                    int[] jie = null;
                    if (((ArrayList)list[0]).Count != 0) jie = jiepai.mArrayToArgs((ArrayList)list[0]);
                    else if (((ArrayList)list[1]).Count != 0) jie = jiepai.mArrayToArgs((ArrayList)list[1]);

                    System.Diagnostics.Debug.WriteLine($"三张jie: {(jie == null ? "null" : string.Join(",", jie))}");
                    if (jie != null)
                    {
                        if (tishi == jie.Length) tishi = 0;
                        int[] _jie = new int[] { jie[tishi], jie[tishi], jie[tishi] };
                        if (bl_tishi) tiShiBottun(_jie);
                        else { yichu(); movePai(juese, _jie); }
                        return true;
                    }
                }
            }
            #endregion
            #region 炸弹
            else if (chupai.PaiType == (int)Guize.炸弹)
            {
                System.Diagnostics.Debug.WriteLine("处理炸弹...");
                if (list != null && list.Count != 0)
                {
                    int[] jie = jiepai.mArrayToArgs(list);
                    System.Diagnostics.Debug.WriteLine($"炸弹jie: {(jie == null ? "null" : string.Join(",", jie))}");
                    if (tishi == jie.Length) tishi = 0;
                    int[] _jie = new int[] { jie[tishi], jie[tishi], jie[tishi], jie[tishi] };
                    if (bl_tishi) tiShiBottun(_jie);
                    else { yichu(); movePai(juese, _jie); }
                    return true;
                }
            }
            #endregion
            #region 三带一,三带二,顺子,连对,飞机不带
            else if (chupai.PaiType > 4 && chupai.PaiType < 13)
            {
                if (list != null && list.Count != 0)
                {
                    if (tishi == list.Count) tishi = 0;
                    int[] jie = jiepai.mArrayToArgs((ArrayList)list[tishi]);
                    if (bl_tishi) tiShiBottun(jie);
                    else { yichu(); movePai(juese, jie); }
                    return true;
                }
            }
            #endregion
            #region 四带二,四带两对,飞机带,三飞机带,四飞机带
            else if (chupai.PaiType > 12 && chupai.PaiType < 20)
            {
                if (list != null)
                {
                    int[] jie = jiepai.mArrayToArgs(list);
                    if (bl_tishi) tiShiBottun(jie);
                    else { yichu(); movePai(juese, jie); }
                    return true;
                }
            }
            #endregion
            #region 如果同类型牌要不起，就判断是否有炸弹
            System.Diagnostics.Debug.WriteLine($"检查炸弹: PaiType={chupai.PaiType}");
            if (chupai.PaiType != (int)Guize.炸弹)
            {
                list = jiepai.findZhadan(juese.ShengYuPai);
                int[] jie = jiepai.mArrayToArgs(list);
                System.Diagnostics.Debug.WriteLine($"炸弹搜索结果: {(jie == null ? "null" : string.Join(",", jie))}");
                if (jie != null)
                {
                    if (tishi == jie.Length) tishi = 0;
                    int[] _jie = new int[] { jie[tishi], jie[tishi], jie[tishi], jie[tishi] };
                    if (bl_tishi) tiShiBottun(_jie);
                    else
                    {
                        chupai.PaiType = (int)Guize.炸弹;
                        yichu(); movePai(juese, _jie);
                    }
                    return true;
                }
            }
            list = jiepai.findTianzha(juese.ShengYuPai);
            int[] huoJian = jiepai.mArrayToArgs(list);
            System.Diagnostics.Debug.WriteLine($"王炸搜索结果: {(huoJian == null ? "null" : string.Join(",", huoJian))}");
            if (huoJian != null)
            {
                if (bl_tishi) tiShiBottun(huoJian);
                else
                {
                    yichu(); movePai(juese, huoJian);
                    chupai.PaiType = (int)Guize.天炸;
                }
                return true;
            }
            #endregion
            System.Diagnostics.Debug.WriteLine("无可用牌，返回false");
            return false;
        }
        #endregion
        #region 提示按钮
        private void tiShiBottun(int[] args)
        {
                for (int i = 0; i < juese2.ImagePaiSub.Count; i++)
                {
                    paiImage[(int)juese2.ImagePaiSub[i]].Top = 483;
                }
                chupai.format(args);
                int num = 0;
                for (int i = 0; i < args.Length; i++)
                {
                    for (int j = num; j < juese2.ImagePaiSub.Count; j++)
                    {
                        if (pai[(int)juese2.ImagePaiSub[j]].Size == args[i])
                        {
                            paiImage[(int)juese2.ImagePaiSub[j]].Top = 453;
                            num = j + 1; break;
                        }
                    }
                }
        }
        #endregion
        #region 重置所有对象
        private void chongZhi()
        {
            audioPlayer?.Stop();
            if (bl_chuPaiOver)
            {
                win = new win(result(),new string[] {juese1.Name,juese2.Name,juese3.Name });
                win.ShowDialog();
            }
            for (int i = 0; i < paiImage.Length; i++)
            {
                paiImage[i].Visible = false;
            }
            foreach (PictureBox pai in paiImage)
            {
                pai.Dispose();
            }
            GC.Collect();
            this.pic1.BackgroundImage = null;
            this.pic2.BackgroundImage = null;
            this.pic3.BackgroundImage = null;
            label1.Text = "";
            label2.Text = "";
            label3.Text = "";
            bl_isDiZhu = false;
            bl_isFirst = false;
            bl_chuPaiOver = false;
            pic_dz.Visible = false;
            buChuPai = 0;
            pic1.Height = 131;
            pic2.Height = 131;
            pic3.Height = 131;
            groupBox1.Visible = false;
            groupBox2.Visible = false;
            groupBox3.Visible = false;
            label6.Text = "剩余牌：";
            label5.Text = "剩余牌：";
            label4.Text = "剩余牌：";
            this.button1.Visible = true;
        }
        #endregion
        #region 部分操作和结果计算
        private void kouDiPai(Juese juese)
        {
            for (int i = 51; i < 54; i++)
            {
                juese.ImagePaiSub.Add(pai[i].Index);
                juese.ShengYuPai.Add(pai[pai[i].Index].Size);
                switch (juese.WeiZhi)
                {
                    case 1:
                        paiImage[pai[i].Index].SetBounds(20, 220, 150, 225);
                        break;
                    case 2:
                        paiImage[pai[i].Index].Top = 483;
                        paiImage[pai[i].Index].BackgroundImage = pai[pai[i].Index].Image;
                        paiImage[pai[i].Index].Width = 150; paiImage[pai[i].Index].Height = 225;
                        paiImage[pai[i].Index].Click += new System.EventHandler(paiImage_Click);
                        // 添加滑动选牌事件
                        paiImage[pai[i].Index].MouseDown += new MouseEventHandler(PaiImage_MouseDown);
                        paiImage[pai[i].Index].MouseMove += new MouseEventHandler(PaiImage_MouseMove);
                        paiImage[pai[i].Index].MouseUp += new MouseEventHandler(PaiImage_MouseUp);
                        break;
                    case 3:
                        paiImage[pai[i].Index].SetBounds(1110, 220, 150, 225);
                        break;
                }
            }
            pai_paixu(juese); image_paixu(juese, 850);
            // image_paixu已更新Tags，无需额外操作
            shengyupai();
        }
        private void yichu()
        {
            yiChuPai(juese1);
            yiChuPai(juese2);
            yiChuPai(juese3);
        }
        private void shengyupai ()
        {
            label6.Text = "剩余牌：" + juese1.ShengYuPai.Count;
            label5.Text = "剩余牌：" + juese2.ShengYuPai.Count;
            label4.Text = "剩余牌：" + juese3.ShengYuPai.Count;
        }
        private void soundFx (int a)
        {
            if (con.SoundFX)
            {
                switch (a)
                {
                    case 0:SoundClick.Play();
                        break;
                    case 1:SoundGive.Play();
                        break;
                    case 2:SoundWin.Play();
                        break;
                    case 3:SoundLoss.Play();
                        break;
                }
            }
        }
        private bool[] result()
        {
            bool[] result = new bool[3];
            if (juese2.Dizhu&&juese2.ShengYuPai.Count==0)
            {
                result[1] = true;
                return result;
            }
            else if (juese2.ShengYuPai.Count!=0&&juese2.Dizhu)
            {
                result[0] = true;
                result[2] = true;
                return result;
            }
            if (juese1.Dizhu&&juese1.ShengYuPai.Count==0)
            {
                result[0] = true;
                return result;
            }
            else if (juese1.ShengYuPai.Count!=0&&juese1.Dizhu)
            {
                result[1] = true;
                result[2] = true;
                return result;
            }
            if (juese3.Dizhu && juese3.ShengYuPai.Count == 0)
            {
                result[2] = true;
                return result;
            }
            else if (juese3.ShengYuPai.Count != 0&&juese3.Dizhu)
            {
                result[0] = true;
                result[1] = true;
                return result;
            }
            else return result;
        }
        private void paizhi (Juese farmer1,Juese farmer2)
        {
            int tmp1 = 0;
            int tmp2 = 0;
            foreach (int item in farmer1.ShengYuPai)
            {
                tmp1 += item;
            }
            foreach (int item in farmer2.ShengYuPai)
            {
                tmp2 += item;
            }
            if (tmp1>tmp2)
            {
                farmer1.Bigger = true;
            }
            else
            {
                farmer2.Bigger = true;
            }
        }
        private void name ()
        {
            if (juese1.Dizhu)
            {
                juese1.Name = dzname();
                if (juese2.Bigger)
                {
                    juese2.Name = whichname(true);
                    juese3.Name = whichname(false);
                }
                else
                {
                    juese2.Name = whichname(false);
                    juese3.Name = whichname(true);
                }
            }
            else if (juese2.Dizhu)
            {
                juese2.Name = dzname();
                if (juese1.Bigger)
                {
                    juese1.Name = whichname(true);
                    juese3.Name = whichname(false);
                }
                else
                {
                    juese1.Name = whichname(false);
                    juese3.Name = whichname(true);
                }
            }
            else if (juese3.Dizhu)
            {
                juese3.Name = dzname();
                if (juese1.Bigger)
                {
                    juese1.Name = whichname(true);
                    juese2.Name = whichname(false);
                }
                else
                {
                    juese1.Name = whichname(false);
                    juese2.Name = whichname(true);
                }
            }
        }
        private string whichname (bool bigger)
        {
            string name = "";
            if (bigger)
            {
                switch (ui.uiselect)
                {
                    case 1:name = "Other Fairies";
                        break;
                    case 2:name = "Terrence";
                        break;
                    case 3:name = "Lizzy";
                        break;
                    case 4:name = "Tinker Bell";
                        break;
                    case 5:name = "Zarina";
                        break;
                    case 6:name = "Neverbeast";
                        break;
                    case 7:name = "Rosetta";
                        break;
                }
            }
            else
            {
                switch (ui.uiselect)
                {
                    case 1:name = "Tinker Bell";
                        break;
                    case 2:name = "Blaze";
                        break;
                    case 3:name = "Tinker Bell";
                        break;
                    case 4:name = "Periwinkle";
                        break;
                    case 5:name = "Tinker Bell";
                        break;
                    case 6:name = "Fawn";
                        break;
                    case 7:name = "Chloe";
                        break;
                }
            }
            return name;
        }
        private string dzname()
        {
            string name = "";
            switch (ui.uiselect)
            {
                case 1:
                    name = "Vidia";
                    break;
                case 2:
                    name = "Tinker Bell";
                    break;
                case 3:
                    name = "Lizzy's Father";
                    break;
                case 4:
                    name = "Lord Milori";
                    break;
                case 5:
                    name = "James Hook";
                    break;
                case 6:
                    name = "Nyx";
                    break;
                case 7:
                    name = "Rumble";
                    break;
            }
            return name;
        }
        private void xianshi()
        {
            groupBox3.Text = juese1.Name;
            groupBox2.Text = juese2.Name;
            groupBox1.Text = juese3.Name;
        }
        private void ThreadStop ()
        {
            tm_faPai?.Cancel();
            tm_daPai?.Cancel();
        }
        private void ShowPai (Juese juese, int[] whatPai)
        {
            chupai.format(whatPai);
            int j = 0;int k = 0; int line = 220;int XianShiWeiZhi = SPLocation(juese.WeiZhi);
            for (int i = 0; i < juese.ImagePaiSub.Count; i++)
            {
                if (pai[(int)juese.ImagePaiSub[i]].Size == whatPai[j])
                {
                    paiImage[(int)juese.ImagePaiSub[i]].BringToFront();
                    if(juese.WeiZhi==1)
                    {
                            juese3.ShangShouPai.Add(pai[(int)juese.ImagePaiSub[i]].Size);
                            paiImage[(int)juese.ImagePaiSub[i]].SetBounds(XianShiWeiZhi, line, 150, 225);
                            paiImage[(int)juese.ImagePaiSub[i]].BackgroundImage = pai[(int)juese.ImagePaiSub[i]].Image;
                    }
                    else
                    {
                        juese1.ShangShouPai.Add(pai[(int)juese.ImagePaiSub[i]].Size);
                        paiImage[(int)juese.ImagePaiSub[i]].SetBounds(XianShiWeiZhi, line, 150, 225);
                        paiImage[(int)juese.ImagePaiSub[i]].BackgroundImage = pai[(int)juese.ImagePaiSub[i]].Image;
                    }
                    juese.YiChuPai.Add((int)juese.ImagePaiSub[i]);
                    juese.ShengYuPai.Remove(pai[(int)juese.ImagePaiSub[i]].Size);
                    juese.ImagePaiSub.RemoveAt(i);
                    XianShiWeiZhi += 30; j++; i--;k++;
                    if (k == 6)
                    {
                        line += 30;
                        XianShiWeiZhi = SPLocation(juese.WeiZhi);
                    }
                    if (j == whatPai.Length) break;
                }
            }
        }
        private int SPLocation (int a)
        {
            switch (a)
            {
                case 1:return 930;
                case 3:return 20;
            }
            return 0;
        }
        private void CloseWindow()
        {
            for (int i = 0; i < 20; i++)
            {
                Opacity -= 0.05;
                Thread.Sleep(50);
            }
        }
        #endregion
        #region 所有事件
        private void button1_Click(object sender, EventArgs e)
        {
            button1.Visible = false;
            groupBox1.Visible = true;
            groupBox2.Visible = true;
            groupBox3.Visible = true;
            load();
            soundFx(0);
            tm_daPai.Start(new ThreadStart(daPai));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            soundFx(0);
            if (button2.Text=="叫地主")
            {
                bl_isDiZhu = true;
                buttonset(3, false);
                tm_faPai.Resume();
            }
            else if (button2.Text == "抢地主")
            {
                // 多人模式抢地主
                SendGrabRequest(true);
                button2.Visible = false;
                button3.Visible = false;
            }
            else if (button2.Text=="出牌")
            {
                if (online)
                {
                    // 多人模式出牌
                    OnlineDealCards();
                }
                else
                {
                    // 单人模式出牌
                    SinglePlayerDealCards();
                }
            }
        }

        /// <summary>
        /// 多人模式出牌
        /// </summary>
        private void OnlineDealCards()
        {
            if (selectedCardIndices.Count == 0)
            {
                MessageBox.Show("请选择要出的牌!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            List<CardDto> selectedCards = new List<CardDto>();
            List<int> selectedWeights = new List<int>();

            // 获取选中的牌
            foreach (int idx in selectedCardIndices)
            {
                if (idx >= 0 && idx < myCardList.Count)
                {
                    var card = myCardList[idx];
                    selectedCards.Add(new CardDto(card.Name, card.Color, card.Weight));
                    selectedWeights.Add(card.Weight);
                }
            }

            // 检查牌型是否正确
            ArrayList selectedPaiList = new ArrayList(selectedWeights.ToArray());
            if (!chupai.isRight(selectedPaiList))
            {
                MessageBox.Show("您出的牌不符合规则!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 如果需要接牌，检查是否能管住上家的牌
            if (needFollow && lastDealDto != null)
            {
                ArrayList lastPaiList = new ArrayList();
                foreach (var card in lastDealDto.SelectCardList)
                {
                    lastPaiList.Add(card.Weight);
                }

                bool canFollow = jiepai.isRight(lastPaiList, selectedPaiList, lastPaiType);
                if (!canFollow)
                {
                    MessageBox.Show("您出的牌管不上上家的牌!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            // 发送出牌请求
            StopTurnTimer();
            HideDealButtons();
            SendDealRequest(selectedCards);
        }

        /// <summary>
        /// 单人模式出牌
        /// </summary>
        private void SinglePlayerDealCards()
        {
            chuPaiWeiZhi = 640;int j = 0;
            for (int i = 0; i < juese2.ImagePaiSub.Count; i++)
            {
                if (paiImage[(int)juese2.ImagePaiSub[i]].Top == 453)
                {
                    saveList.Add(pai[(int)juese2.ImagePaiSub[i]].Size); j++;
                }
            }
            chuPaiWeiZhi -= (j * 30 + 120)/2;
            int paiType = chupai.PaiType;
            System.Diagnostics.Debug.WriteLine($"玩家出牌: 原PaiType={paiType}, ShangShouPai.Count={juese2.ShangShouPai?.Count ?? -1}");
            if (saveList.Count != 0)
            {
                if (chupai.isRight(saveList))
                {
                    System.Diagnostics.Debug.WriteLine($"玩家出牌验证通过: 新PaiType={chupai.PaiType}");
                    if (buChuPai != 2 && bl_isFirst)
                    {
                        if (jiepai.isRight(juese2.ShangShouPai, saveList, paiType))
                        {
                            yichu();soundFx(1);
                            this.button2.Invoke (weituo1,3,false);
                            juese1.ShangShouPai.Clear(); movePai(juese2, jiepai.arrayToArgs(saveList)); buChuPai = 0;
                            System.Diagnostics.Debug.WriteLine($"玩家出牌完成: PaiType={chupai.PaiType}, juese1.ShangShouPai.Count={juese1.ShangShouPai?.Count ?? -1}");
                            tm_daPai.Resume();
                        }
                        else
                        {
                            if (chupai.PaiType == paiType) MessageBox.Show("您出的牌小于上手的牌!");
                            else MessageBox.Show("您出的牌型不符!");
                            chupai.PaiType = paiType;
                        }
                    }
                    else
                    {
                        yichu();soundFx(1);
                        this.button2.Invoke(weituo1, 3, false);
                        juese1.ShangShouPai.Clear(); juese2.ShangShouPai.Clear(); juese3.ShangShouPai.Clear();
                        movePai(juese2, jiepai.arrayToArgs(saveList)); buChuPai = 0;
                        System.Diagnostics.Debug.WriteLine($"玩家出牌完成(首出): PaiType={chupai.PaiType}, juese1.ShangShouPai.Count={juese1.ShangShouPai?.Count ?? -1}");
                        tm_daPai.Resume();
                    }
                }
                else
                {
                    chupai.PaiType = paiType;
                    MessageBox.Show("您出的牌不符合规则!");
                }
                saveList.Clear();
                for (int i = 0; i < juese2.ImagePaiSub.Count; i++)
                {
                    paiImage[(int)juese2.ImagePaiSub[i]].Top = 483;
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            soundFx(0);
            if (button3.Text=="不叫")
            {
                buttonset(3, false);
                tm_faPai.Resume();
            }
            else if (button3.Text == "不抢")
            {
                // 多人模式不抢地主
                SendGrabRequest(false);
                button2.Visible = false;
                button3.Visible = false;
            }
            else if (button3.Text=="不出")
            {
                if (online)
                {
                    // 多人模式不出
                    StopTurnTimer();
                    HideDealButtons();
                    SendPassRequest();
                }
                else
                {
                    // 单人模式不出
                    for (int i = 0; i < juese2.ImagePaiSub.Count; i++)
                    {
                        paiImage[(int)juese2.ImagePaiSub[i]].Top = 483;
                    }
                    buChuPai++; tishi = 0;
                    buttonset(3,false);
                    juese1.ShangShouPai.Clear();
                    juese1.ShangShouPai = (ArrayList)juese2.ShangShouPai.Clone();
                    juese2.ShangShouPai.Clear();
                    this.label3.Text = "不出";
                    tm_daPai.Resume();
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            soundFx(0);
            for (int i = 0; i < juese2.ImagePaiSub.Count; i++)
            {
                paiImage[(int)juese2.ImagePaiSub[i]].Top = 483;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            soundFx(0);

            if (online)
            {
                // 多人模式提示
                OnlineHintCards();
            }
            else
            {
                // 单人模式提示
                bool bl = tiShiJiePai(jiepai.isRight(chupai.PaiType, juese2.ShangShouPai, juese2.ShengYuPai), juese2, true);
                if (bl == false) button3_Click(sender, e);
                else tishi++;
            }
        }

        /// <summary>
        /// 多人模式提示出牌
        /// </summary>
        private void OnlineHintCards()
        {
            // 先重置所有牌位置
            selectedCardIndices.Clear();
            foreach (var pb in myCardPictureBoxes)
            {
                pb.Top = 483;
            }

            // 获取当前手牌权值列表
            ArrayList myWeights = new ArrayList(myCardList.Select(c => c.Weight).ToArray());

            if (needFollow && lastDealDto != null)
            {
                // 需要接牌 - 使用单机模式的提示逻辑
                ArrayList lastPaiList = new ArrayList();
                foreach (var card in lastDealDto.SelectCardList)
                {
                    lastPaiList.Add(card.Weight);
                }

                // 设置chupai.PaiType以便jiepai使用
                chupai.PaiType = lastPaiType;

                // 获取可能的出牌
                ArrayList possibleMoves = jiepai.isRight(lastPaiType, lastPaiList, myWeights);

                bool found = OnlineTiShiJiePai(possibleMoves, lastPaiType);
                if (!found)
                {
                    // 无法接牌，提示不出
                    MessageBox.Show("没有能管住上家的牌，请选择不出！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                // 首出，尝试找最小的单张牌
                // 如果有牌，提示最小的单张
                if (myCardList.Count > 0)
                {
                    // 找最小的牌
                    int minWeight = myCardList.Min(c => c.Weight);
                    OnlineShowHint(new int[] { minWeight });
                    chupai.PaiType = (int)Guize.一张;
                }
            }
        }

        /// <summary>
        /// 多人模式提示接牌（参考单机模式逻辑）
        /// </summary>
        private bool OnlineTiShiJiePai(ArrayList list, int paiType)
        {
            // 王炸无法接
            if (paiType == (int)Guize.天炸) return false;

            #region 单张
            else if (paiType == (int)Guize.一张)
            {
                if (list != null && list.Count >= 4)
                {
                    int[] jie = null;
                    if (((ArrayList)list[0]).Count != 0) jie = jiepai.mArrayToArgs((ArrayList)list[0]);
                    else if (((ArrayList)list[1]).Count != 0) jie = jiepai.mArrayToArgs((ArrayList)list[1]);
                    else if (((ArrayList)list[2]).Count != 0) jie = jiepai.mArrayToArgs((ArrayList)list[2]);
                    else if (((ArrayList)list[3]).Count != 0) jie = jiepai.mArrayToArgs((ArrayList)list[3]);

                    if (jie != null)
                    {
                        if (tishi >= jie.Length) tishi = 0;
                        int[] _jie = new int[] { jie[tishi] };
                        OnlineShowHint(_jie);
                        tishi++;
                        chupai.PaiType = (int)Guize.一张;
                        return true;
                    }
                }
            }
            #endregion
            #region 对子
            else if (paiType == (int)Guize.对子)
            {
                if (list != null && list.Count >= 3)
                {
                    int[] jie = null;
                    if (((ArrayList)list[0]).Count != 0) jie = jiepai.mArrayToArgs((ArrayList)list[0]);
                    else if (((ArrayList)list[1]).Count != 0) jie = jiepai.mArrayToArgs((ArrayList)list[1]);
                    else if (((ArrayList)list[2]).Count != 0) jie = jiepai.mArrayToArgs((ArrayList)list[2]);

                    if (jie != null)
                    {
                        if (tishi >= jie.Length) tishi = 0;
                        int[] _jie = new int[] { jie[tishi], jie[tishi] };
                        OnlineShowHint(_jie);
                        tishi++;
                        chupai.PaiType = (int)Guize.对子;
                        return true;
                    }
                }
            }
            #endregion
            #region 三张
            else if (paiType == (int)Guize.三不带)
            {
                if (list != null && list.Count >= 2)
                {
                    int[] jie = null;
                    if (((ArrayList)list[0]).Count != 0) jie = jiepai.mArrayToArgs((ArrayList)list[0]);
                    else if (((ArrayList)list[1]).Count != 0) jie = jiepai.mArrayToArgs((ArrayList)list[1]);

                    if (jie != null)
                    {
                        if (tishi >= jie.Length) tishi = 0;
                        int[] _jie = new int[] { jie[tishi], jie[tishi], jie[tishi] };
                        OnlineShowHint(_jie);
                        tishi++;
                        chupai.PaiType = (int)Guize.三不带;
                        return true;
                    }
                }
            }
            #endregion
            #region 炸弹
            else if (paiType == (int)Guize.炸弹)
            {
                if (list != null && list.Count != 0)
                {
                    int[] jie = jiepai.mArrayToArgs(list);
                    if (jie != null)
                    {
                        if (tishi >= jie.Length) tishi = 0;
                        int[] _jie = new int[] { jie[tishi], jie[tishi], jie[tishi], jie[tishi] };
                        OnlineShowHint(_jie);
                        tishi++;
                        chupai.PaiType = (int)Guize.炸弹;
                        return true;
                    }
                }
            }
            #endregion
            #region 三带一,三带二,顺子,连对,飞机不带
            else if (paiType > 4 && paiType < 13)
            {
                if (list != null && list.Count != 0)
                {
                    if (tishi >= list.Count) tishi = 0;
                    int[] jie = jiepai.mArrayToArgs((ArrayList)list[tishi]);
                    OnlineShowHint(jie);
                    tishi++;
                    chupai.PaiType = paiType;
                    return true;
                }
            }
            #endregion
            #region 四带二,四带两对,飞机带,三飞机带,四飞机带
            else if (paiType > 12 && paiType < 20)
            {
                if (list != null)
                {
                    int[] jie = jiepai.mArrayToArgs(list);
                    if (jie != null && jie.Length > 0)
                    {
                        OnlineShowHint(jie);
                        chupai.PaiType = paiType;
                        return true;
                    }
                }
            }
            #endregion

            #region 如果同类型牌要不起，就判断是否有炸弹
            if (paiType != (int)Guize.炸弹)
            {
                ArrayList bombList = jiepai.findZhadan(juese2.ShengYuPai);
                int[] bombJie = jiepai.mArrayToArgs(bombList);
                if (bombJie != null && bombJie.Length > 0)
                {
                    if (tishi >= bombJie.Length) tishi = 0;
                    int[] _jie = new int[] { bombJie[tishi], bombJie[tishi], bombJie[tishi], bombJie[tishi] };
                    OnlineShowHint(_jie);
                    tishi++;
                    chupai.PaiType = (int)Guize.炸弹;
                    return true;
                }
            }
            #endregion

            #region 检查王炸
            ArrayList rocketList = jiepai.findTianzha(juese2.ShengYuPai);
            int[] rocket = jiepai.mArrayToArgs(rocketList);
            if (rocket != null && rocket.Length == 2)
            {
                OnlineShowHint(rocket);
                chupai.PaiType = (int)Guize.天炸;
                return true;
            }
            #endregion

            return false;
        }

        /// <summary>
        /// 多人模式显示提示牌
        /// </summary>
        private void OnlineShowHint(int[] weights)
        {
            selectedCardIndices.Clear();
            foreach (var pb in myCardPictureBoxes)
            {
                pb.Top = 483;
            }

            List<int> weightsToSelect = new List<int>(weights);
            for (int i = 0; i < myCardList.Count && weightsToSelect.Count > 0; i++)
            {
                if (weightsToSelect.Contains(myCardList[i].Weight))
                {
                    weightsToSelect.Remove(myCardList[i].Weight);
                    myCardPictureBoxes[i].Top = 453;
                    selectedCardIndices.Add(i);
                }
            }
        }

        private void paiImage_Click(object sender, EventArgs e)
        {
            if (((PictureBox)sender).Top == 483) ((PictureBox)sender).Top = 453;
            else ((PictureBox)sender).Top = 483;
        }

        #region 滑动选牌功能（单机模式）
        /// <summary>
        /// 单机模式牌图片鼠标按下事件（开始滑动选牌）
        /// </summary>
        private void PaiImage_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            PictureBox pb = sender as PictureBox;
            if (pb == null) return;

            // 开始滑动选牌，阻止窗体拖动
            isSlidingSelection = true;
            leftFlag = false; // 禁止窗体拖动
            int index = (int)pb.Tag;
            slideStartIndex = index;
            slideEndIndex = index;
            // 记录按下时的状态，用于判断是否需要切换
            slideSelectMode = (pb.Top == 483);

            // 捕获鼠标，确保即使鼠标离开控件也能接收鼠标事件
            pb.Capture = true;
        }

        /// <summary>
        /// 单机模式牌图片鼠标移动事件（处理滑动选牌）
        /// </summary>
        private void PaiImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isSlidingSelection) return;

            // 获取鼠标在窗体中的位置
            Point formPoint = this.PointToClient(Control.MousePosition);
            int currentIndex = FindCardIndexAtPositionSingle(formPoint);

            if (currentIndex >= 0 && currentIndex != slideEndIndex)
            {
                slideEndIndex = currentIndex;
                UpdateSlideSelectionPreview();
            }
        }

        /// <summary>
        /// 单机模式牌图片鼠标释放事件
        /// </summary>
        private void PaiImage_MouseUp(object sender, MouseEventArgs e)
        {
            if (!isSlidingSelection) return;

            // 释放鼠标捕获
            if (sender is PictureBox pb)
            {
                pb.Capture = false;
            }

            // 判断是否有滑动
            bool hasSlided = (slideStartIndex != slideEndIndex);

            if (hasSlided)
            {
                // 滑动选牌：更新所有滑动过的牌的状态
                ApplySlideSelection();
            }
            // 单次点击不做处理，让Click事件处理

            isSlidingSelection = false;
            slideStartIndex = -1;
            slideEndIndex = -1;
        }

        /// <summary>
        /// 更新滑动选牌预览（单机模式）
        /// </summary>
        private void UpdateSlideSelectionPreview()
        {
            int minIndex = Math.Min(slideStartIndex, slideEndIndex);
            int maxIndex = Math.Max(slideStartIndex, slideEndIndex);

            // 使用juese2.ImagePaiSub直接获取牌图片索引（Tag对应ImagePaiSub中的位置）
            for (int i = minIndex; i <= maxIndex; i++)
            {
                if (i >= 0 && i < juese2.ImagePaiSub.Count)
                {
                    int paiIndex = (int)juese2.ImagePaiSub[i];
                    if (paiIndex >= 0 && paiIndex < paiImage.Length && paiImage[paiIndex] != null)
                    {
                        if (slideSelectMode)
                            paiImage[paiIndex].Top = 453; // 选中状态
                        else
                            paiImage[paiIndex].Top = 483; // 未选中状态
                    }
                }
            }
        }
        #endregion

        private void DdzMian_MouseDown(object sender, MouseEventArgs e)
        {
            this.Focus();
            if (e.Button == MouseButtons.Left && !isSlidingSelection)
            {
                // 检查鼠标是否在牌区域内，如果是则不启动窗口移动
                Point mousePos = e.Location;
                bool isInCardArea = IsMouseInCardArea(mousePos);

                if (!isInCardArea)
                {
                    mouseOff = new Point(-e.X, -e.Y);
                    leftFlag = true;
                }
            }
        }

        /// <summary>
        /// 检查鼠标是否在牌区域内
        /// </summary>
        /// <summary>
        /// 检查鼠标是否在牌区域内（检测下半区，减少误触窗口移动）
        /// </summary>
        private bool IsMouseInCardArea(Point point)
        {
            // 将按钮以下的下半区都纳入检测范围
            // 手牌区域在底部，按钮大约在Y=200左右的位置
            // 设置检测线为Y=250，此线以下的区域都视为牌区域
            int detectionLineY = 250;

            // 如果鼠标在检测线以下，视为在牌区域
            if (point.Y >= detectionLineY)
            {
                return true;
            }

            return false;
        }

        private void DdzMian_MouseMove(object sender, MouseEventArgs e)
        {
            // 处理滑动选牌
            if (isSlidingSelection)
            {
                // 获取鼠标在窗体中的位置
                Point formPoint = this.PointToClient(Control.MousePosition);
                int currentIndex = -1;

                if (online)
                {
                    // 联机模式：遍历手牌PictureBox找到鼠标所在的牌
                    currentIndex = FindCardIndexAtPosition(formPoint, myCardPictureBoxes);
                }
                else
                {
                    // 单机模式：遍历玩家手牌找到鼠标所在的牌
                    currentIndex = FindCardIndexAtPositionSingle(formPoint);
                }

                if (currentIndex >= 0 && currentIndex != slideEndIndex)
                {
                    slideEndIndex = currentIndex;
                    if (online)
                        UpdateOnlineSlideSelectionPreview();
                    else
                        UpdateSlideSelectionPreview();
                }
                return;
            }

            // 处理窗体拖动
            if (leftFlag)
            {
                Point mouseSet = Control.MousePosition;
                mouseSet.Offset(mouseOff.X, mouseOff.Y);
                Location = mouseSet;
            }
        }

        /// <summary>
        /// 查找鼠标位置对应的牌索引（联机模式）
        /// </summary>
        private int FindCardIndexAtPosition(Point point, List<PictureBox> cardList)
        {
            // 牌是重叠的，从右到左排列（索引0在最右边）
            // 每张牌只露出左边30像素，最后一张牌完全可见
            // 需要从右到左检测（从索引0开始）

            for (int i = 0; i < cardList.Count; i++)
            {
                var pb = cardList[i];
                if (pb != null && pb.Visible)
                {
                    // 计算可见区域
                    int visibleWidth;
                    if (i == cardList.Count - 1)
                    {
                        // 最后一张牌（最左边）完全可见
                        visibleWidth = pb.Width;
                    }
                    else
                    {
                        // 其他牌只露出左边30像素
                        visibleWidth = 30;
                    }

                    // 创建可见区域的矩形
                    Rectangle visibleBounds = new Rectangle(pb.Left, pb.Top, visibleWidth, pb.Height);
                    if (visibleBounds.Contains(point))
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        /// <summary>
        /// 查找鼠标位置对应的牌索引（单机模式）
        /// </summary>
        private int FindCardIndexAtPositionSingle(Point point)
        {
            // 牌是重叠的，从右到左排列（索引0在最右边）
            // 每张牌只露出左边30像素，最后一张牌完全可见
            // 需要从右到左检测（从索引0开始）

            for (int i = 0; i < juese2.ImagePaiSub.Count; i++)
            {
                int paiIndex = (int)juese2.ImagePaiSub[i];
                if (paiIndex >= 0 && paiIndex < paiImage.Length && paiImage[paiIndex] != null)
                {
                    var pb = paiImage[paiIndex];

                    // 计算可见区域（考虑牌的当前位置，可能是选中状态Top=453或未选中状态Top=483）
                    int visibleWidth;
                    if (i == juese2.ImagePaiSub.Count - 1)
                    {
                        // 最后一张牌（最左边）完全可见
                        visibleWidth = pb.Width;
                    }
                    else
                    {
                        // 其他牌只露出左边30像素
                        visibleWidth = 30;
                    }

                    // 使用牌的实际位置创建可见区域的矩形
                    Rectangle visibleBounds = new Rectangle(pb.Left, pb.Top, visibleWidth, pb.Height);
                    if (visibleBounds.Contains(point))
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        private void DdzMian_MouseUp(object sender, MouseEventArgs e)
        {
            // 处理滑动选牌结束
            if (isSlidingSelection)
            {
                // 判断是否有滑动（slideStartIndex != slideEndIndex）
                bool hasSlided = (slideStartIndex != slideEndIndex);

                if (hasSlided)
                {
                    // 滑动选牌：更新所有滑动过的牌的状态
                    if (online)
                        ApplyOnlineSlideSelection();
                    else
                        ApplySlideSelection();
                }
                // 单次点击不做处理，让Click事件处理

                isSlidingSelection = false;
                slideStartIndex = -1;
                slideEndIndex = -1;
                return;
            }

            if (leftFlag)
            {
                leftFlag = false;
            }
        }

        /// <summary>
        /// 应用滑动选牌结果（单机模式）
        /// </summary>
        private void ApplySlideSelection()
        {
            int minIndex = Math.Min(slideStartIndex, slideEndIndex);
            int maxIndex = Math.Max(slideStartIndex, slideEndIndex);

            for (int i = minIndex; i <= maxIndex; i++)
            {
                if (i >= 0 && i < juese2.ImagePaiSub.Count)
                {
                    int paiIndex = (int)juese2.ImagePaiSub[i];
                    if (paiIndex >= 0 && paiIndex < paiImage.Length && paiImage[paiIndex] != null)
                    {
                        if (slideSelectMode)
                            paiImage[paiIndex].Top = 453; // 选中状态
                        else
                            paiImage[paiIndex].Top = 483; // 未选中状态
                    }
                }
            }
        }

        /// <summary>
        /// 应用滑动选牌结果（联机模式）
        /// </summary>
        private void ApplyOnlineSlideSelection()
        {
            int minIndex = Math.Min(slideStartIndex, slideEndIndex);
            int maxIndex = Math.Max(slideStartIndex, slideEndIndex);

            for (int i = minIndex; i <= maxIndex; i++)
            {
                if (i >= 0 && i < myCardPictureBoxes.Count)
                {
                    var pb = myCardPictureBoxes[i];
                    if (slideSelectMode)
                    {
                        pb.Top = 453; // 选中状态
                        if (!selectedCardIndices.Contains(i))
                        {
                            selectedCardIndices.Add(i);
                        }
                    }
                    else
                    {
                        pb.Top = 483; // 未选中状态
                        selectedCardIndices.Remove(i);
                    }
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            soundFx(0);
            if (MessageBox.Show("确定要退出吗?", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                ThreadStop();CloseWindow();
                Application.Exit();
            }

        }

        private void button7_Click(object sender, EventArgs e)
        {
            soundFx(0);
            if (MessageBox.Show("确定要返回上一级吗?", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (online)
                {
                    // 多人模式返回大厅
                    StopTurnTimer();
                    timerNetwork.Stop();
                    ReturnToLobby();
                }
                else
                {
                    // 单人模式返回主菜单
                    Main m = new Main();
                    ThreadStop();
                    CloseWindow();
                    m.Show();
                    this.Close();
                    GC.Collect();
                }
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            soundFx(0);
            this.WindowState = FormWindowState.Minimized;
        }

        private void DdzMian_MouseUp1(object sender, MouseEventArgs e)
        {
            ((Button)sender).BackgroundImage = ui.Button;
        }

        private void DdzMian_MouseDown1(object sender, MouseEventArgs e)
        {
            ((Button)sender).BackgroundImage = ui.Buttonpress;
        }
        private void DdzMian_Shown(object sender, EventArgs e)
        {
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Opacity += 0.05;
            if (Opacity==100)
            {
                timer1.Stop();
            }
        }

        #region 多人模式网络事件处理

        /// <summary>
        /// 获取手牌
        /// </summary>
        private void OnGetCardsReceived(List<CardDto> cardList)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<List<CardDto>>(OnGetCardsReceived), cardList);
                return;
            }

            myCardList = cardList;
            // 初始化手牌显示
            InitOnlineCards();
        }

        /// <summary>
        /// 初始化多人模式手牌显示
        /// </summary>
        private void InitOnlineCards()
        {
            chupai = new Chupai();
            jiepai = new Jiepai();
            saveList = new ArrayList();
            myCardPictureBoxes = new List<PictureBox>();
            dealCardImages = new List<PictureBox>();
            selectedCardIndices = new List<int>();
            leftPlayerCardBacks = new List<PictureBox>();
            rightPlayerCardBacks = new List<PictureBox>();
            newPlayer();

            // 按权值降序排序手牌（与单机模式一致）
            myCardList = myCardList.OrderByDescending(c => c.Weight).ToList();

            // 初始化手牌图片（使用与单机模式一致的居中逻辑）
            // 单机模式公式: 640 + (牌数 * 30 + 120) / 2 - 150
            // 对于17张牌: 640 + (510 + 120) / 2 - 150 = 805
            int startX = 640 + (myCardList.Count * 30 + 120) / 2 - 150;
            for (int i = 0; i < myCardList.Count; i++)
            {
                var card = myCardList[i];
                PictureBox pb = new PictureBox();
                // 位置：从右到左，每张牌递减30（与单机模式一致）
                pb.SetBounds(startX - i * 30, 483, 150, 225);
                pb.BackgroundImage = GetCardImage(card.Weight, card.Color);
                pb.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
                pb.Tag = i; // 存储牌索引
                pb.Click += new System.EventHandler(OnlinePaiImage_Click);
                // 添加滑动选牌事件
                pb.MouseDown += new MouseEventHandler(OnlinePaiImage_MouseDown);
                pb.MouseMove += new MouseEventHandler(OnlinePaiImage_MouseMove);
                pb.MouseUp += new MouseEventHandler(OnlinePaiImage_MouseUp);
                this.Controls.Add(pb);
                myCardPictureBoxes.Add(pb);

                // 同步更新角色数据
                juese2.ImagePaiSub.Add(i);
                juese2.ShengYuPai.Add(card.Weight);
            }

            // 初始化其他玩家的牌背显示（17张牌）
            InitOtherPlayersCardBacks();

            shengyupai();
        }

        /// <summary>
        /// 初始化其他玩家的牌背显示
        /// </summary>
        private void InitOtherPlayersCardBacks()
        {
            // 左边玩家的牌背（17张）
            int leftX = 20;
            int leftY = 220;
            for (int i = 0; i < 17; i++)
            {
                PictureBox pb = new PictureBox();
                pb.SetBounds(leftX, leftY + (i < 6 ? i * 30 : (i < 12 ? (i - 6) * 30 : (i - 12) * 30)), 150, 225);
                pb.BackgroundImage = Properties.Resources.牌背3;
                pb.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
                pb.Tag = -1; // 标识为其他玩家的牌
                this.Controls.Add(pb);
                pb.BringToFront();
                leftPlayerCardBacks.Add(pb);

                // 更新juese1数据
                juese1.ShengYuPai.Add(0);
            }

            // 重新排列左边玩家的牌背
            int cardIndex = 0;
            for (int layer = 0; layer < 3; layer++)
            {
                for (int i = 0; i < 6 && cardIndex < leftPlayerCardBacks.Count; i++)
                {
                    var pb = leftPlayerCardBacks[cardIndex];
                    pb.SetBounds(leftX, leftY + layer * 10, 150, 225);
                    cardIndex++;
                }
            }

            // 右边玩家的牌背（17张）
            int rightX = 1110;
            int rightY = 220;
            for (int i = 0; i < 17; i++)
            {
                PictureBox pb = new PictureBox();
                pb.SetBounds(rightX, rightY + (i < 6 ? i * 30 : (i < 12 ? (i - 6) * 30 : (i - 12) * 30)), 150, 225);
                pb.BackgroundImage = Properties.Resources.牌背3;
                pb.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
                pb.Tag = -1; // 标识为其他玩家的牌
                this.Controls.Add(pb);
                pb.BringToFront();
                rightPlayerCardBacks.Add(pb);

                // 更新juese3数据
                juese3.ShengYuPai.Add(0);
            }

            // 重新排列右边玩家的牌背
            cardIndex = 0;
            for (int layer = 0; layer < 3; layer++)
            {
                for (int i = 0; i < 6 && cardIndex < rightPlayerCardBacks.Count; i++)
                {
                    var pb = rightPlayerCardBacks[cardIndex];
                    pb.SetBounds(rightX, rightY + layer * 10, 150, 225);
                    cardIndex++;
                }
            }
        }

        /// <summary>
        /// 更新其他玩家的牌背数量
        /// </summary>
        private void UpdateOtherPlayerCardBacks(int leftCount, int rightCount)
        {
            // 更新左边玩家牌背数量
            while (leftPlayerCardBacks.Count > leftCount)
            {
                var pb = leftPlayerCardBacks[leftPlayerCardBacks.Count - 1];
                pb.Visible = false;
                this.Controls.Remove(pb);
                pb.Dispose();
                leftPlayerCardBacks.RemoveAt(leftPlayerCardBacks.Count - 1);
            }

            // 更新右边玩家牌背数量
            while (rightPlayerCardBacks.Count > rightCount)
            {
                var pb = rightPlayerCardBacks[rightPlayerCardBacks.Count - 1];
                pb.Visible = false;
                this.Controls.Remove(pb);
                pb.Dispose();
                rightPlayerCardBacks.RemoveAt(rightPlayerCardBacks.Count - 1);
            }
        }

        /// <summary>
        /// 根据权值和花色获取牌图片
        /// </summary>
        private Image GetCardImage(int weight, int color)
        {
            try
            {
                // 获取UI设置
                string uiFolder = "5"; // 默认
                try
                {
                    uiFolder = new config().UI.ToString();
                }
                catch { }

                // 服务器花色映射: 0=Hearts, 1=Diamonds, 2=Clubs, 3=Spades
                // 客户端花色映射: heitao, hongtao, meihua, fangkuai
                string[] colorNames = { "hongtao", "fangkuai", "meihua", "heitao" };

                string fileName;
                if (weight == 16)
                {
                    // 小王
                    fileName = Application.StartupPath + @"\Pokers\" + uiFolder + "\\16.png";
                }
                else if (weight == 17)
                {
                    // 大王
                    fileName = Application.StartupPath + @"\Pokers\" + uiFolder + "\\17.png";
                }
                else if (weight >= 3 && weight <= 15 && color >= 0 && color <= 3)
                {
                    // 普通牌
                    fileName = Application.StartupPath + @"\Pokers\" + uiFolder + "\\" + colorNames[color] + weight + ".png";
                }
                else
                {
                    return Properties.Resources.牌背3;
                }

                if (System.IO.File.Exists(fileName))
                {
                    return Image.FromFile(fileName);
                }
            }
            catch { }

            return Properties.Resources.牌背3;
        }

        /// <summary>
        /// 获取牌资源名称（已废弃，保留兼容）
        /// </summary>
        private string GetCardResourceName(int weight, int color)
        {
            // 权值 16=小王, 17=大王
            if (weight == 16) return "小王";
            if (weight == 17) return "大王";

            // 普通牌：花色 + 数字
            // color: 0=Hearts(红桃), 1=Diamonds(方块), 2=Clubs(梅花), 3=Spades(黑桃)
            string[] colorNames = { "红桃", "方块", "梅花", "黑桃" };
            string[] numberNames = { "", "", "", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A", "2" };

            if (weight >= 3 && weight <= 15 && color >= 0 && color <= 3)
            {
                return colorNames[color] + numberNames[weight];
            }

            return "牌背3";
        }

        /// <summary>
        /// 多人模式牌点击事件
        /// </summary>
        private void OnlinePaiImage_Click(object sender, EventArgs e)
        {
            if (!isMyTurn) return;
            PictureBox pb = sender as PictureBox;
            if (pb == null) return;

            int cardIndex = (int)pb.Tag;
            if (pb.Top == 483)
            {
                pb.Top = 453;
                if (!selectedCardIndices.Contains(cardIndex))
                {
                    selectedCardIndices.Add(cardIndex);
                }
            }
            else
            {
                pb.Top = 483;
                selectedCardIndices.Remove(cardIndex);
            }
        }

        #region 滑动选牌功能（联机模式）
        /// <summary>
        /// 联机模式牌图片鼠标按下事件（开始滑动选牌）
        /// </summary>
        private void OnlinePaiImage_MouseDown(object sender, MouseEventArgs e)
        {
            if (!isMyTurn) return;
            if (e.Button != MouseButtons.Left) return;
            PictureBox pb = sender as PictureBox;
            if (pb == null) return;

            // 开始滑动选牌，阻止窗体拖动
            isSlidingSelection = true;
            leftFlag = false; // 禁止窗体拖动
            int index = (int)pb.Tag;
            slideStartIndex = index;
            slideEndIndex = index;
            // 记录按下时的状态，用于判断是否需要切换
            slideSelectMode = (pb.Top == 483);

            // 捕获鼠标，确保即使鼠标离开控件也能接收鼠标事件
            pb.Capture = true;
        }

        /// <summary>
        /// 联机模式牌图片鼠标移动事件（处理滑动选牌）
        /// </summary>
        private void OnlinePaiImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isSlidingSelection || !isMyTurn) return;

            // 获取鼠标在窗体中的位置
            Point formPoint = this.PointToClient(Control.MousePosition);
            int currentIndex = FindCardIndexAtPosition(formPoint, myCardPictureBoxes);

            if (currentIndex >= 0 && currentIndex != slideEndIndex)
            {
                slideEndIndex = currentIndex;
                UpdateOnlineSlideSelectionPreview();
            }
        }

        /// <summary>
        /// 联机模式牌图片鼠标释放事件
        /// </summary>
        private void OnlinePaiImage_MouseUp(object sender, MouseEventArgs e)
        {
            if (!isSlidingSelection) return;

            // 释放鼠标捕获
            if (sender is PictureBox pb)
            {
                pb.Capture = false;
            }

            // 判断是否有滑动
            bool hasSlided = (slideStartIndex != slideEndIndex);

            if (hasSlided)
            {
                // 滑动选牌：更新所有滑动过的牌的状态
                ApplyOnlineSlideSelection();
            }
            // 单次点击不做处理，让Click事件处理

            isSlidingSelection = false;
            slideStartIndex = -1;
            slideEndIndex = -1;
        }

        /// <summary>
        /// 更新滑动选牌预览（联机模式）
        /// </summary>
        private void UpdateOnlineSlideSelectionPreview()
        {
            int minIndex = Math.Min(slideStartIndex, slideEndIndex);
            int maxIndex = Math.Max(slideStartIndex, slideEndIndex);

            for (int i = minIndex; i <= maxIndex; i++)
            {
                if (i >= 0 && i < myCardPictureBoxes.Count)
                {
                    var pb = myCardPictureBoxes[i];
                    if (slideSelectMode)
                    {
                        pb.Top = 453; // 选中状态
                        if (!selectedCardIndices.Contains(i))
                        {
                            selectedCardIndices.Add(i);
                        }
                    }
                    else
                    {
                        pb.Top = 483; // 未选中状态
                        selectedCardIndices.Remove(i);
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// 转换抢地主
        /// </summary>
        private void OnTurnGrabReceived(int userId)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<int>(OnTurnGrabReceived), userId);
                return;
            }

            if (userId == Models.GameModel.UserDto.Id)
            {
                // 显示抢地主按钮
                button2.Visible = true;
                button3.Visible = true;
                button2.Text = "抢地主";
                button3.Text = "不抢";

                // 开始自己的倒计时
                StartTurnTimer();
            }
            else
            {
                // 显示其他玩家正在抢地主（显示倒计时）
                StartOtherPlayerTimer(userId);
            }
        }

        /// <summary>
        /// 抢地主结果
        /// </summary>
        private void OnGrabLandlordReceived(GrabDto grabDto)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<GrabDto>(OnGrabLandlordReceived), grabDto);
                return;
            }

            // 停止计时器
            StopTurnTimer();
            HideDealButtons();

            landlordId = grabDto.UserId;

            // 播放背景音乐
            if (con.BackMusic)
            {
                string mpath = "";
                switch (ui.uiselect)
                {
                    case 1: mpath = Path.UI_TB.ToString(); break;
                    case 2: mpath = Path.UI_LT.ToString(); break;
                    case 3: mpath = Path.UI_FR.ToString(); break;
                    case 4: mpath = Path.UI_SW.ToString(); break;
                    case 5: mpath = Path.UI_PF.ToString(); break;
                    case 6: mpath = Path.UI_LN.ToString(); break;
                    case 7: mpath = Path.UI_LN.ToString(); break;
                }
                audioPlayer.Play(Application.StartupPath + "\\" + mpath + "\\background.mp3", true);
            }

            // 显示底牌
            pic1.BackgroundImage = GetPaiImage(grabDto.TableCardList[0]);
            pic2.BackgroundImage = GetPaiImage(grabDto.TableCardList[1]);
            pic3.BackgroundImage = GetPaiImage(grabDto.TableCardList[2]);

            tableCards = grabDto.TableCardList;

            // 更新地主标识（用文字标识，不显示图标）
            if (landlordId == Models.GameModel.UserDto.Id)
            {
                // 自己是地主
                string name = Models.GameModel.UserDto.Name;
                groupBox2.Text = "【地主】" + name;
                juese2.Dizhu = true;

                // 给地主添加底牌
                AddTableCardsToHand();
            }
            else if (landlordId == Models.GameModel.MatchRoomDto.LeftId)
            {
                // 左边玩家是地主
                string name = Models.GameModel.MatchRoomDto.UIdUserDict[landlordId].Name;
                groupBox1.Text = "【地主】" + name;
                juese1.Dizhu = true;
            }
            else if (landlordId == Models.GameModel.MatchRoomDto.RightId)
            {
                // 右边玩家是地主
                string name = Models.GameModel.MatchRoomDto.UIdUserDict[landlordId].Name;
                groupBox3.Text = "【地主】" + name;
                juese3.Dizhu = true;
            }
        }

        /// <summary>
        /// 获取牌图片
        /// </summary>
        private Image GetPaiImage(CardDto cardDto)
        {
            return GetCardImage(cardDto.Weight, cardDto.Color);
        }

        /// <summary>
        /// 给地主添加底牌
        /// </summary>
        private void AddTableCardsToHand()
        {
            // 将底牌添加到手牌列表
            foreach (var card in tableCards)
            {
                myCardList.Add(new CardDto(card.Name, card.Color, card.Weight));
            }

            // 重新排序手牌
            myCardList = myCardList.OrderByDescending(c => c.Weight).ToList();

            // 清除旧的PictureBox
            foreach (var pb in myCardPictureBoxes)
            {
                pb.Visible = false;
                this.Controls.Remove(pb);
                pb.Dispose();
            }
            myCardPictureBoxes.Clear();
            juese2.ImagePaiSub.Clear();
            juese2.ShengYuPai.Clear();

            // 重新创建所有手牌的PictureBox（使用与单机模式一致的居中逻辑）
            // 单机模式公式: 640 + (牌数 * 30 + 120) / 2 - 150
            // 对于20张牌: 640 + (600 + 120) / 2 - 150 = 850
            int startX = 640 + (myCardList.Count * 30 + 120) / 2 - 150;
            for (int i = 0; i < myCardList.Count; i++)
            {
                var card = myCardList[i];
                PictureBox pb = new PictureBox();
                pb.SetBounds(startX - i * 30, 483, 150, 225);
                pb.BackgroundImage = GetCardImage(card.Weight, card.Color);
                pb.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
                pb.Tag = i;
                pb.Click += new System.EventHandler(OnlinePaiImage_Click);
                // 添加滑动选牌事件
                pb.MouseDown += new MouseEventHandler(OnlinePaiImage_MouseDown);
                pb.MouseMove += new MouseEventHandler(OnlinePaiImage_MouseMove);
                pb.MouseUp += new MouseEventHandler(OnlinePaiImage_MouseUp);
                this.Controls.Add(pb);
                myCardPictureBoxes.Add(pb);
                juese2.ImagePaiSub.Add(i);
                juese2.ShengYuPai.Add(card.Weight);
            }

            selectedCardIndices.Clear();
            shengyupai();
        }

        /// <summary>
        /// 转换出牌
        /// </summary>
        private void OnTurnDealReceived(int userId)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<int>(OnTurnDealReceived), userId);
                return;
            }

            currentTurnUserId = userId;

            if (userId == Models.GameModel.UserDto.Id)
            {
                // 轮到我出牌
                isMyTurn = true;
                StartTurnTimer();

                if (lastDealDto == null || lastDealDto.UserId == userId)
                {
                    // 首出
                    needFollow = false;
                    button2.Left = 595;
                    button2.Visible = true;
                    button3.Visible = false;
                    button4.Visible = false;
                    button6.Visible = true;
                    button2.Text = "出牌";
                    button6.Text = "提示";
                }
                else
                {
                    // 接牌
                    needFollow = true;
                    button2.Left = 547;
                    button2.Visible = true;
                    button3.Visible = true;
                    button4.Visible = true;
                    button6.Visible = true;
                    button2.Text = "出牌";
                    button3.Text = "不出";
                    button4.Text = "重选";
                    button6.Text = "提示";
                }
            }
            else
            {
                // 其他玩家出牌
                isMyTurn = false;
                StopTurnTimer();
                HideDealButtons();

                // 开始其他玩家的倒计时显示
                StartOtherPlayerTimer(userId);
            }
        }

        /// <summary>
        /// 出牌广播
        /// </summary>
        private void OnDealBroadcastReceived(DealDto dealDto)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<DealDto>(OnDealBroadcastReceived), dealDto);
                return;
            }

            // 停止计时器
            timerTurnTimeout.Stop();

            lastDealDto = dealDto;
            lastPaiType = dealDto.Type;

            // 清除之前的出牌显示（多人模式使用独立方法）
            if (online)
            {
                ClearDealCardImages();
            }
            else
            {
                yichu();
            }

            // 显示出牌
            ShowOnlineDeal(dealDto);

            // 更新剩余牌数
            if (dealDto.UserId == Models.GameModel.UserDto.Id)
            {
                // 移除自己出的牌
                RemoveMyCards(dealDto.SelectCardList);
            }
            else
            {
                // 显示其他玩家剩余牌数
                UpdateOtherPlayerCardCount(dealDto.UserId, dealDto.RemainCardList?.Count ?? 0);
            }

            // 清除思考状态
            if (dealDto.UserId == Models.GameModel.MatchRoomDto.LeftId)
            {
                label1.Text = "";
            }
            else if (dealDto.UserId == Models.GameModel.MatchRoomDto.RightId)
            {
                label2.Text = "";
            }
            else if (dealDto.UserId == Models.GameModel.UserDto.Id)
            {
                label3.Text = "";
            }

            shengyupai();
        }

        /// <summary>
        /// 显示多人模式出牌
        /// </summary>
        private void ShowOnlineDeal(DealDto dealDto)
        {
            // 清除之前的出牌显示
            ClearDealCardImages();

            chuPaiWeiZhi = 640 - (dealDto.Length * 30 + 120) / 2;

            foreach (var card in dealDto.SelectCardList)
            {
                // 创建新的PictureBox显示出牌
                PictureBox pb = new PictureBox();
                pb.SetBounds(chuPaiWeiZhi, 193, 150, 225);

                // 获取牌图片
                Image cardImage = GetCardImageByWeight(card.Weight, card.Color);
                pb.BackgroundImage = cardImage;
                pb.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;

                this.Controls.Add(pb);
                pb.BringToFront();
                dealCardImages.Add(pb);

                chuPaiWeiZhi += 30;
            }

            chupai.PaiType = dealDto.Type;
            soundFx(1);
        }

        /// <summary>
        /// 清除出牌显示
        /// </summary>
        private void ClearDealCardImages()
        {
            foreach (var pb in dealCardImages)
            {
                pb.Visible = false;
                this.Controls.Remove(pb);
                pb.Dispose();
            }
            dealCardImages.Clear();
        }

        /// <summary>
        /// 根据权值和花色获取牌图片
        /// </summary>
        private Image GetCardImageByWeight(int weight, int color)
        {
            return GetCardImage(weight, color);
        }

        /// <summary>
        /// 移除自己出的牌
        /// </summary>
        private void RemoveMyCards(List<CardDto> cardList)
        {
            // 创建要移除的牌的权重列表
            List<int> weightsToRemove = cardList.Select(c => c.Weight).ToList();

            // 从后往前遍历，避免索引问题
            for (int i = myCardList.Count - 1; i >= 0; i--)
            {
                if (weightsToRemove.Contains(myCardList[i].Weight))
                {
                    weightsToRemove.Remove(myCardList[i].Weight);

                    // 移除对应的PictureBox
                    if (i < myCardPictureBoxes.Count)
                    {
                        myCardPictureBoxes[i].Visible = false;
                        this.Controls.Remove(myCardPictureBoxes[i]);
                        myCardPictureBoxes[i].Dispose();
                        myCardPictureBoxes.RemoveAt(i);
                    }

                    // 移除牌数据
                    myCardList.RemoveAt(i);
                    juese2.ShengYuPai.RemoveAt(i);
                    juese2.ImagePaiSub.RemoveAt(i);
                }
            }

            // 更新PictureBox的Tag和位置（使用与单机模式一致的居中逻辑）
            // 计算居中起始位置: 640 + (牌数 * 30 + 120) / 2 - 150
            int startX = 640 + (myCardPictureBoxes.Count * 30 + 120) / 2 - 150;
            for (int i = 0; i < myCardPictureBoxes.Count; i++)
            {
                myCardPictureBoxes[i].Tag = i;
                myCardPictureBoxes[i].Left = startX - i * 30;
                myCardPictureBoxes[i].Top = 483;
                myCardPictureBoxes[i].BringToFront();
            }

            selectedCardIndices.Clear();
            shengyupai();
        }

        /// <summary>
        /// 更新其他玩家剩余牌数
        /// </summary>
        private void UpdateOtherPlayerCardCount(int userId, int count)
        {
            if (userId == Models.GameModel.MatchRoomDto.LeftId)
            {
                label4.Text = "剩余牌：" + count;
                juese1.ShengYuPai.Clear();
                for (int i = 0; i < count; i++) juese1.ShengYuPai.Add(0);
                // 更新左边玩家牌背
                UpdateOtherPlayerCardBacks(count, rightPlayerCardBacks.Count);
            }
            else if (userId == Models.GameModel.MatchRoomDto.RightId)
            {
                label6.Text = "剩余牌：" + count;
                juese3.ShengYuPai.Clear();
                for (int i = 0; i < count; i++) juese3.ShengYuPai.Add(0);
                // 更新右边玩家牌背
                UpdateOtherPlayerCardBacks(leftPlayerCardBacks.Count, count);
            }
        }

        /// <summary>
        /// 出牌响应
        /// </summary>
        private void OnDealResponseReceived(int result)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<int>(OnDealResponseReceived), result);
                return;
            }

            if (result == -1)
            {
                // 出牌失败，重新显示按钮
                MessageBox.Show("您出的牌管不上上一个玩家出的牌!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                isMyTurn = true;
                // 重置选中的牌
                ResetSelectedCards();
            }
            else
            {
                // 出牌成功
                StopTurnTimer();
                HideDealButtons();
                label3.Text = "";
            }
        }

        /// <summary>
        /// 重置选中的牌
        /// </summary>
        private void ResetSelectedCards()
        {
            selectedCardIndices.Clear();
            foreach (var pb in myCardPictureBoxes)
            {
                pb.Top = 483;
            }
        }

        /// <summary>
        /// 不出响应
        /// </summary>
        private void OnPassResponseReceived(int result)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<int>(OnPassResponseReceived), result);
                return;
            }

            if (result == -1)
            {
                // 不能不出（如果是首出）
                MessageBox.Show("您必须出牌!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                // 不出成功，重置选中的牌位置
                ResetSelectedCards();
                StopTurnTimer();
                HideDealButtons();
                label3.Text = "不出";
                soundFx(0);
            }
        }

        /// <summary>
        /// 游戏结束
        /// </summary>
        private void OnGameOverReceived(OverDto overDto)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<OverDto>(OnGameOverReceived), overDto);
                return;
            }

            StopTurnTimer();
            HideDealButtons();

            int myId = Models.GameModel.UserDto.Id;
            bool isWin = overDto.WinUIdList.Contains(myId);
            bool isLandlord = (landlordId == myId);

            if (isWin)
            {
                soundFx(2);
            }
            else
            {
                soundFx(3);
            }

            // 构建结果数组
            bool[] result = new bool[3];
            string[] names = new string[3];

            var matchRoom = Models.GameModel.MatchRoomDto;
            if (matchRoom != null)
            {
                // 左边玩家
                if (matchRoom.LeftId > 0 && matchRoom.UIdUserDict.ContainsKey(matchRoom.LeftId))
                {
                    names[0] = matchRoom.UIdUserDict[matchRoom.LeftId].Name;
                    result[0] = overDto.WinUIdList.Contains(matchRoom.LeftId);
                }
                // 自己
                names[1] = Models.GameModel.UserDto.Name;
                result[1] = isWin;
                // 右边玩家
                if (matchRoom.RightId > 0 && matchRoom.UIdUserDict.ContainsKey(matchRoom.RightId))
                {
                    names[2] = matchRoom.UIdUserDict[matchRoom.RightId].Name;
                    result[2] = overDto.WinUIdList.Contains(matchRoom.RightId);
                }
            }

            // 显示结果窗口
            win resultWindow = new win(result, names);
            resultWindow.ShowDialog();

            // 返回大厅
            ReturnToLobby();
        }

        /// <summary>
        /// 倍数变化
        /// </summary>
        private void OnMultipleChangeReceived(int multiple)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<int>(OnMultipleChangeReceived), multiple);
                return;
            }

            // 可以在这里显示倍数
        }

        /// <summary>
        /// 获取玩家名称
        /// </summary>
        private string GetPlayerName(int userId)
        {
            if (Models.GameModel.MatchRoomDto != null &&
                Models.GameModel.MatchRoomDto.UIdUserDict.ContainsKey(userId))
            {
                return Models.GameModel.MatchRoomDto.UIdUserDict[userId].Name;
            }
            return $"玩家{userId}";
        }

        #endregion

        #region 多人模式出牌计时器

        /// <summary>
        /// 开始出牌计时（自己）
        /// </summary>
        private void StartTurnTimer()
        {
            isMyTurn = true;
            remainingSeconds = turnTimeoutSeconds;
            lblTurnTimer.Text = remainingSeconds.ToString();
            lblTurnTimer.Visible = true;
            lblTurnTimer.ForeColor = Color.White;
            timerTurnTimeout.Start();
        }

        /// <summary>
        /// 开始其他玩家出牌计时
        /// </summary>
        private void StartOtherPlayerTimer(int userId)
        {
            isMyTurn = false;
            otherPlayerRemainingSeconds = turnTimeoutSeconds;
            timerTurnTimeout.Start();

            // 立即显示倒计时
            UpdateOtherPlayerTimerDisplay(userId, otherPlayerRemainingSeconds);
        }

        /// <summary>
        /// 更新其他玩家的倒计时显示
        /// </summary>
        private void UpdateOtherPlayerTimerDisplay(int userId, int seconds)
        {
            string timerText = seconds.ToString();

            if (userId == Models.GameModel.MatchRoomDto.LeftId)
            {
                label1.Text = timerText;
                label1.ForeColor = seconds <= 5 ? Color.Red : (seconds <= 10 ? Color.Yellow : Color.White);
            }
            else if (userId == Models.GameModel.MatchRoomDto.RightId)
            {
                label2.Text = timerText;
                label2.ForeColor = seconds <= 5 ? Color.Red : (seconds <= 10 ? Color.Yellow : Color.White);
            }
        }

        /// <summary>
        /// 停止出牌计时
        /// </summary>
        private void StopTurnTimer()
        {
            timerTurnTimeout.Stop();
            lblTurnTimer.Visible = false;
            // 清除其他玩家的倒计时显示
            label1.Text = "";
            label2.Text = "";
            isMyTurn = false;
        }

        /// <summary>
        /// 计时器Tick事件
        /// </summary>
        private void timerTurnTimeout_Tick(object sender, EventArgs e)
        {
            if (isMyTurn)
            {
                // 自己的倒计时
                remainingSeconds--;
                lblTurnTimer.Text = remainingSeconds.ToString();

                // 改变颜色提示
                if (remainingSeconds <= 5)
                {
                    lblTurnTimer.ForeColor = Color.Red;
                }
                else if (remainingSeconds <= 10)
                {
                    lblTurnTimer.ForeColor = Color.Yellow;
                }

                if (remainingSeconds <= 0)
                {
                    // 超时，自动不出
                    StopTurnTimer();
                    AutoPass();
                }
            }
            else
            {
                // 其他玩家的倒计时
                otherPlayerRemainingSeconds--;
                UpdateOtherPlayerTimerDisplay(currentTurnUserId, otherPlayerRemainingSeconds);

                if (otherPlayerRemainingSeconds <= 0)
                {
                    // 其他玩家超时，停止计时器
                    timerTurnTimeout.Stop();
                }
            }
        }

        /// <summary>
        /// 自动不出（超时处理）
        /// </summary>
        private void AutoPass()
        {
            if (needFollow)
            {
                // 发送不出请求
                SendPassRequest();
            }
            else
            {
                // 首出情况下超时，自动出最小的牌
                AutoDealSmallest();
            }
        }

        /// <summary>
        /// 发送不出请求
        /// </summary>
        private void SendPassRequest()
        {
            if (netManager != null && netManager.IsConnected)
            {
                var msg = new SocketMsg(OpCode.FIGHT, FightCode.PASS_CREQ, null);
                netManager.Execute(0, msg);
            }
        }

        /// <summary>
        /// 自动出最小的牌
        /// </summary>
        private void AutoDealSmallest()
        {
            // 选择最小的牌
            List<CardDto> smallestCards = new List<CardDto>();

            // 从手牌中找最小的单牌
            if (myCardList.Count > 0)
            {
                // 排序手牌
                var sortedCards = myCardList.OrderBy(c => c.Weight).ToList();
                smallestCards.Add(sortedCards[0]);

                // 发送出牌请求
                SendDealRequest(smallestCards);
            }
        }

        /// <summary>
        /// 发送出牌请求
        /// </summary>
        private void SendDealRequest(List<CardDto> cards)
        {
            if (netManager != null && netManager.IsConnected)
            {
                var dealDto = new DealDto(cards, Models.GameModel.UserDto.Id);
                var msg = new SocketMsg(OpCode.FIGHT, FightCode.DEAL_CREQ, dealDto);
                netManager.Execute(0, msg);
            }
        }

        /// <summary>
        /// 发送抢地主请求
        /// </summary>
        private void SendGrabRequest(bool grab)
        {
            if (netManager != null && netManager.IsConnected)
            {
                var msg = new SocketMsg(OpCode.FIGHT, FightCode.GRAB_LANDLORD_CREQ, grab);
                netManager.Execute(0, msg);
            }
        }

        /// <summary>
        /// 隐藏出牌按钮
        /// </summary>
        private void HideDealButtons()
        {
            button2.Visible = false;
            button3.Visible = false;
            button4.Visible = false;
            button6.Visible = false;
        }

        /// <summary>
        /// 返回大厅
        /// </summary>
        private void ReturnToLobby()
        {
            // 取消事件订阅
            Models.OnGetCards -= OnGetCardsReceived;
            Models.OnTurnGrab -= OnTurnGrabReceived;
            Models.OnGrabLandlord -= OnGrabLandlordReceived;
            Models.OnTurnDeal -= OnTurnDealReceived;
            Models.OnDealBroadcast -= OnDealBroadcastReceived;
            Models.OnDealResponse -= OnDealResponseReceived;
            Models.OnPassResponse -= OnPassResponseReceived;
            Models.OnGameOver -= OnGameOverReceived;
            Models.OnMultipleChange -= OnMultipleChangeReceived;

            timerNetwork.Stop();
            this.Close();
        }

        #endregion
        #endregion
    }
}