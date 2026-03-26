using System;
using System.Collections;
using System.Drawing;
using System.Media;
using System.Threading;
using System.Windows.Forms;
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
                button1.Visible = false;
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
                        juese2.ImagePaiSub.Add(pai[i].Index);
                        paiImage[pai[i].Index].Click += new System.EventHandler(paiImage_Click);
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
                        break;
                    case 3:
                        paiImage[pai[i].Index].SetBounds(1110, 220, 150, 225);
                        break;
                }
            }
            pai_paixu(juese); image_paixu(juese, 850);
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
            else if (button2.Text=="出牌")
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
        }

        private void button3_Click(object sender, EventArgs e)
        {
            soundFx(0);
            if (button3.Text=="不叫")
            {
                buttonset(3, false);
                tm_faPai.Resume();
            }
            else if (button3.Text=="不出")
            {
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
            bool bl = tiShiJiePai(jiepai.isRight(chupai.PaiType, juese2.ShangShouPai, juese2.ShengYuPai), juese2, true);
            if (bl == false) button3_Click(sender, e);
            else tishi++;
        }

        private void paiImage_Click(object sender, EventArgs e)
        {
            if (((PictureBox)sender).Top == 483) ((PictureBox)sender).Top = 453;
            else ((PictureBox)sender).Top = 483;
        }

        private void DdzMian_MouseDown(object sender, MouseEventArgs e)
        {
            this.Focus();
            if (e.Button == MouseButtons.Left)
            {
                mouseOff = new Point(-e.X, -e.Y);
                leftFlag = true;
            }
        }

        private void DdzMian_MouseMove(object sender, MouseEventArgs e)
        {
            if (leftFlag)
            {
                Point mouseSet = Control.MousePosition;
                mouseSet.Offset(mouseOff.X, mouseOff.Y);
                Location = mouseSet;
            }
        }

        private void DdzMian_MouseUp(object sender, MouseEventArgs e)
        {
            if (leftFlag)
            {
                leftFlag = false;
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
                Main m = new Main();
                ThreadStop();
                CloseWindow();
                m.Show();
                this.Close();
                GC.Collect();
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
        #endregion
    }
}