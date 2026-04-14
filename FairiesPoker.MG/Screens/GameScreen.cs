using FairiesPoker;
using FairiesPoker.MG.Core;
using FairiesPoker.MG.Network;
using FairiesPoker.MG.Renderers;
using FairiesPoker.MG.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Protocol.Code;
using Protocol.Dto;
using Protocol.Dto.Fight;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace FairiesPoker.MG.Screens;

/// <summary>
/// 游戏状态枚举 - 替代GameThreadManager的线程状态
/// </summary>
public enum GameState
{
    WAITING,       // 等待开始
    DEALING,       // 发牌中
    GRABBING,      // 叫地主阶段
    MY_TURN,       // 轮到我出牌
    AI_TURN,       // AI出牌中
    FINISHED       // 游戏结束
}

/// <summary>
/// 游戏屏幕 - 一比一复刻原DdzMian.cs单机模式逻辑
/// </summary>
public class GameScreen : ScreenBase
{
    private readonly bool _isOnline;
    private GameState _state = GameState.WAITING;

    // === 游戏逻辑对象(完全复用原逻辑) ===
    private Pai[] _pai;
    private Juese _juese1; // 左玩家(AI) - WeiZhi=1
    private Juese _juese2; // 自己 - WeiZhi=2
    private Juese _juese3; // 右玩家(AI) - WeiZhi=3
    private Chupai _chupai;
    private Jiepai _jiepai;
    private ComputerChuPai _computerChuPai;

    // === 渲染器 ===
    private Texture2D? _bgTexture;
    private Texture2D? _cardBackTexture;

    // 手牌相关
    private Vector2[] _handPositions;
    private bool[] _handSelected;
    private CardSelectionHandler _selection;

    // 已出的牌(当前桌面显示的牌)
    private List<(string huase, int size)> _myPlayedCards;
    private List<(string huase, int size)> _leftPlayedCards;
    private List<(string huase, int size)> _rightPlayedCards;

    // 底牌
    private List<(string huase, int size)> _tableCards;
    private bool _tableCardsRevealed;

    // 剩余牌数
    private int _leftCardCount;
    private int _rightCardCount;

    // 玩家名
    private string _leftName = "Computer1";
    private string _selfName = "Player";
    private string _rightName = "Computer2";

    // === 按钮 ===
    private readonly UIButton _btnStart = new();
    private readonly UIButton _btnAction = new();  // 叫地主/出牌
    private readonly UIButton _btnPass = new();    // 不叫/不出
    private readonly UIButton _btnHint = new();    // 提示
    private readonly UIButton _btnReselect = new(); // 重选
    private readonly UIButton _btnBack = new();
    private readonly UIButton _btnMinimize = new();
    private readonly UIButton _btnClose = new();

    // === 标签（映射原项目：label1=右AI, label2=左AI, label3=玩家）===
    private readonly UILabel _lblRightStatus = new();   // 对应原label1（右AI状态）
    private readonly UILabel _lblLeftStatus = new();    // 对应原label2（左AI状态）
    private readonly UILabel _lblMyStatus = new();      // 对应原label3（玩家状态）
    private readonly UILabel _lblLeftRemain = new();
    private readonly UILabel _lblRightRemain = new();
    private readonly UILabel _lblStatus = new();

    // === 状态变量（完全对应原项目）===
    private bool _bl_isFirst;      // 是否首出（对应原bl_isFirst）
    private bool _bl_isDiZhu;      // 是否地主（对应原bl_isDiZhu）
    private int _buChuPai;         // 不出计数（对应原buChuPai）
    private int _chuPaiWeiZhi;     // 出牌位置（对应原chuPaiWeiZhi，用于显示位置）
    private int _tishi;            // 提示计数（对应原tishi）
    private bool _noDiZhu;         // 无人叫地主（对应原noDiZhu）
    private bool _bl_chuPaiOver;   // 出牌结束（对应原bl_chuPaiOver）

    // === 轮转计数器（对应原项目的num）===
    // num=1 → juese1(左AI) → num++
    // num=2 → juese3(右AI) → num++ （注意：num=2对应juese3！）
    // num=3 → juese2(玩家) → num=1
    private int _turnNum;

    // === 发牌动画 ===
    private float _dealAccumulator;
    private const float DealInterval = 80f; // ms per card
    private int _dealtCount;
    private int _totalDealCards = 54;

    // === AI出牌计时 ===
    private float _aiAccumulator;
    private const float AiDelay = 1000f; // ms (与原项目Thread.Sleep(1000)一致)

    // === 回合计时器 ===
    private float _turnTimerAccumulator;
    private int _remainingSeconds = 20;
    private bool _turnTimerActive;

    // === 输入 ===
    private bool _isSelecting;
    private Point _mousePos;
    private bool _isDragging;
    private Vector2 _dragOffset;

    // === 网络相关(联机模式) ===
    private NetManager? _netManager;
    private List<CardDto>? _onlineCards;
    private int _landlordId = -1;
    private int _currentTurnUserId = -1;
    private bool _isMyTurnOnline;
    private DealDto? _lastDealDto;
    private int _lastPaiType;

    public GameScreen(Game1 game, ScreenManager screenManager, bool isOnline = false)
        : base(game, screenManager)
    {
        _isOnline = isOnline;
    }

    public override void Initialize()
    {
        base.Initialize();

        // 初始化游戏对象
        _chupai = new Chupai();
        _jiepai = new Jiepai();
        _computerChuPai = new ComputerChuPai();

        _selection = new CardSelectionHandler(0);
        _handPositions = Array.Empty<Vector2>();
        _handSelected = Array.Empty<bool>();

        _myPlayedCards = new List<(string, int)>();
        _leftPlayedCards = new List<(string, int)>();
        _rightPlayedCards = new List<(string, int)>();
        _tableCards = new List<(string, int)>();

        if (_isOnline)
        {
            InitOnline();
        }
        else
        {
            InitOffline();
        }
        InitUI();

        // 订阅网络事件(联机模式)
        if (_isOnline)
        {
            Models.OnGetCards += OnGetCardsReceived;
            Models.OnTurnGrab += OnTurnGrabReceived;
            Models.OnGrabLandlord += OnGrabLandlordReceived;
            Models.OnTurnDeal += OnTurnDealReceived;
            Models.OnDealBroadcast += OnDealBroadcastReceived;
            Models.OnDealResponse += OnDealResponseReceived;
            Models.OnPassResponse += OnPassResponseReceived;
            Models.OnGameOver += OnGameOverReceived;
            Models.OnMultipleChange += OnMultipleChangeReceived;
            Models.OnGameStart += OnGameStartReceived;
        }
    }

    public override void UnloadContent()
    {
        if (_isOnline)
        {
            Models.OnGetCards -= OnGetCardsReceived;
            Models.OnTurnGrab -= OnTurnGrabReceived;
            Models.OnGrabLandlord -= OnGrabLandlordReceived;
            Models.OnTurnDeal -= OnTurnDealReceived;
            Models.OnDealBroadcast -= OnDealBroadcastReceived;
            Models.OnDealResponse -= OnDealResponseReceived;
            Models.OnPassResponse -= OnPassResponseReceived;
            Models.OnGameOver -= OnGameOverReceived;
            Models.OnMultipleChange -= OnMultipleChangeReceived;
            Models.OnGameStart -= OnGameStartReceived;
        }
    }

    #region 单机模式初始化（一比一复刻原load函数）

    private void InitOffline()
    {
        NewPlayer();
        NewPai();
        Suiji1();
    }

    /// <summary>
    /// 新建3个角色（对应原newPlayer）
    /// </summary>
    private void NewPlayer()
    {
        _juese1 = new Juese();
        _juese2 = new Juese();
        _juese3 = new Juese();
        _juese1.WeiZhi = 1;
        _juese2.WeiZhi = 2;
        _juese3.WeiZhi = 3;
    }

    /// <summary>
    /// 新建54张牌（对应原newpai）
    /// 原项目牌顺序：pai[0]=大王, pai[1]=小王, pai[2..53]=普通牌
    /// </summary>
    private void NewPai()
    {
        _pai = new Pai[54];
        // 大小王
        _pai[0] = new Pai("", 17); // 大王
        _pai[1] = new Pai("", 16); // 小王

        // 4种花色，每种13张(3-15)
        string[] huases = { "heitao", "hongtao", "meihua", "fangkuai" };
        int k = 0;
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 13; j++)
            {
                _pai[k + j + 2] = new Pai(huases[i], j + 3);
            }
            k = k + 13;
        }
    }

    /// <summary>
    /// 随机洗牌（对应原suiji1）
    /// 原项目为每张牌分配随机Index
    /// </summary>
    private void Suiji1()
    {
        Random rd = new Random();
        for (int i = 0; i < _pai.Length; i++)
        {
            int k = rd.Next(54);
            if (i == 0)
            {
                _pai[i].Index = k;
            }
            for (int j = 0; j < i; j++)
            {
                if (_pai[j].Index == k)
                {
                    i--;
                    break;
                }
                else if (j == i - 1)
                {
                    _pai[i].Index = k;
                }
            }
        }
    }

    /// <summary>
    /// 开始单机游戏（对应原button1_Click中的load逻辑）
    /// </summary>
    public void StartOfflineGame()
    {
        // 重新洗牌
        Suiji1();

        // 清空所有角色数据
        _juese1.ShengYuPai.Clear();
        _juese2.ShengYuPai.Clear();
        _juese3.ShengYuPai.Clear();
        _juese1.ImagePaiSub.Clear();
        _juese2.ImagePaiSub.Clear();
        _juese3.ImagePaiSub.Clear();
        _juese1.Dizhu = false;
        _juese2.Dizhu = false;
        _juese3.Dizhu = false;
        _juese1.YiChuPai.Clear();
        _juese2.YiChuPai.Clear();
        _juese3.YiChuPai.Clear();
        _juese1.ShangShouPai.Clear();
        _juese2.ShangShouPai.Clear();
        _juese3.ShangShouPai.Clear();

        // 发牌（对应原newPaiImage中的发牌逻辑）
        _tableCards.Clear();
        _tableCardsRevealed = false;

        for (int i = 0; i < 51; i++)
        {
            // 原项目：i % 3 == 0 → juese1, (i+2) % 3 == 0 → juese2, else → juese3
            if (i % 3 == 0)
            {
                _juese1.ImagePaiSub.Add(_pai[i].Index);
            }
            else if ((i + 2) % 3 == 0)
            {
                _juese2.ImagePaiSub.Add(_pai[i].Index);
            }
            else
            {
                _juese3.ImagePaiSub.Add(_pai[i].Index);
            }
        }

        // 底牌(51-53) - 存储牌的索引，用于后续地主获取
        for (int i = 51; i < 54; i++)
        {
            _tableCards.Add((_pai[i].Huase, _pai[i].Size));
        }

        // 排序手牌（对应原pai_paixu）
        PaiPaixu(_juese1);
        PaiPaixu(_juese2);
        PaiPaixu(_juese3);

        // 添加牌值（对应原addJuesePai）
        AddJuesePai(_juese1);
        AddJuesePai(_juese2);
        AddJuesePai(_juese3);

        // 初始化显示
        _leftCardCount = _juese1.ShengYuPai.Count;
        _rightCardCount = _juese3.ShengYuPai.Count;

        // 初始化手牌UI
        int handCount = _juese2.ShengYuPai.Count;
        _handPositions = CardLayoutManager.CalculateHandPositions(handCount);
        _handSelected = new bool[handCount];
        _selection = new CardSelectionHandler(handCount);

        _myPlayedCards.Clear();
        _leftPlayedCards.Clear();
        _rightPlayedCards.Clear();

        // 重置状态变量
        _buChuPai = 0;
        _tishi = 0;
        _bl_chuPaiOver = false;
        _noDiZhu = false;
        _bl_isFirst = false;
        _bl_isDiZhu = false;

        // 进入发牌阶段
        _state = GameState.DEALING;
        _dealtCount = 0;
        _dealAccumulator = 0;
        _totalDealCards = 54;

        // 隐藏所有按钮，显示状态标签
        ButtonSet(false, false, false, false, false);
        _lblStatus.Visible = true;
        _lblStatus.Text = "发牌中...";
        _lblLeftRemain.Visible = true;
        _lblRightRemain.Visible = true;
        _lblLeftStatus.Visible = false;
        _lblRightStatus.Visible = false;
        _lblMyStatus.Visible = false;
    }

    /// <summary>
    /// 牌排序（对应原pai_paixu）- 按Size降序排列ImagePaiSub
    /// </summary>
    private void PaiPaixu(Juese juese)
    {
        for (int i = 0; i < juese.ImagePaiSub.Count; i++)
        {
            for (int j = i; j < juese.ImagePaiSub.Count; j++)
            {
                if (_pai[(int)juese.ImagePaiSub[i]].Size < _pai[(int)juese.ImagePaiSub[j]].Size)
                {
                    int temp = (int)juese.ImagePaiSub[i];
                    juese.ImagePaiSub[i] = juese.ImagePaiSub[j];
                    juese.ImagePaiSub[j] = temp;
                }
            }
        }
    }

    /// <summary>
    /// 添加牌值（对应原addJuesePai）
    /// </summary>
    private void AddJuesePai(Juese juese)
    {
        for (int i = 0; i < juese.ImagePaiSub.Count; i++)
        {
            juese.ShengYuPai.Add(_pai[(int)juese.ImagePaiSub[i]].Size);
        }
    }

    #endregion

    #region 联机模式初始化

    private void InitOnline()
    {
        _netManager = new NetManager();
        _onlineCards = new List<CardDto>();
        _tableCards = new List<(string, int)>();
        _myPlayedCards = new List<(string, int)>();
        _leftPlayedCards = new List<(string, int)>();
        _rightPlayedCards = new List<(string, int)>();
        _handPositions = Array.Empty<Vector2>();
        _handSelected = Array.Empty<bool>();
        _selection = new CardSelectionHandler(0);

        _state = GameState.WAITING;
        _lblStatus.Text = "等待游戏开始...";

        ButtonSet(false, false, false, false, false);
    }

    #endregion

    #region UI初始化

    private void InitUI()
    {
        // 背景
        string bgPath = System.IO.Path.Combine(ConfigManager.ThemePath, "main seq.jpg");
        if (File.Exists(bgPath))
            _bgTexture = TextureManager.Load("_game_bg", bgPath);
        string cardBackPath = System.IO.Path.Combine(ConfigManager.ThemePath, "牌背3.png");
        if (!File.Exists(cardBackPath))
            cardBackPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Pokers", "牌背3.png");
        if (File.Exists(cardBackPath))
            _cardBackTexture = TextureManager.Load("_cardback", cardBackPath);

        // 按钮纹理
        string btnNormalPath = System.IO.Path.Combine(ConfigManager.ThemePath, "btn1.png");
        string btnPressedPath = System.IO.Path.Combine(ConfigManager.ThemePath, "btn2.png");
        Texture2D? btnNormal = File.Exists(btnNormalPath) ? TextureManager.Load("_btn_normal", btnNormalPath) : null;
        Texture2D? btnPressed = File.Exists(btnPressedPath) ? TextureManager.Load("_btn_pressed", btnPressedPath) : null;

        // 开始按钮
        _btnStart.Position = new Vector2(596, 329);
        _btnStart.Size = new Vector2(90, 45);
        _btnStart.Text = "开始";
        _btnStart.TextColor = Color.Gold;
        _btnStart.NormalTexture = btnNormal;
        _btnStart.HoverTexture = btnNormal;
        _btnStart.PressedTexture = btnPressed;
        _btnStart.OnClick = StartOfflineGame;

        // 主操作按钮（原button2）
        _btnAction.Position = new Vector2(547, 420);
        _btnAction.Size = new Vector2(90, 30);
        _btnAction.Text = "";
        _btnAction.TextColor = Color.Gold;
        _btnAction.NormalTexture = btnNormal;
        _btnAction.HoverTexture = btnNormal;
        _btnAction.PressedTexture = btnPressed;
        _btnAction.OnClick = OnActionClick;

        // 取消按钮（原button3）
        _btnPass.Position = new Vector2(643, 420);
        _btnPass.Size = new Vector2(90, 30);
        _btnPass.Text = "";
        _btnPass.TextColor = Color.Gold;
        _btnPass.NormalTexture = btnNormal;
        _btnPass.HoverTexture = btnNormal;
        _btnPass.PressedTexture = btnPressed;
        _btnPass.OnClick = OnPassClick;

        // 提示按钮（原button6）
        _btnHint.Position = new Vector2(739, 420);
        _btnHint.Size = new Vector2(90, 30);
        _btnHint.Text = "提示";
        _btnHint.TextColor = Color.Gold;
        _btnHint.NormalTexture = btnNormal;
        _btnHint.HoverTexture = btnNormal;
        _btnHint.PressedTexture = btnPressed;
        _btnHint.OnClick = OnHintClick;

        // 重选按钮（原button4）
        _btnReselect.Position = new Vector2(451, 420);
        _btnReselect.Size = new Vector2(90, 30);
        _btnReselect.Text = "重选";
        _btnReselect.TextColor = Color.Gold;
        _btnReselect.NormalTexture = btnNormal;
        _btnReselect.HoverTexture = btnNormal;
        _btnReselect.PressedTexture = btnPressed;
        _btnReselect.OnClick = OnReselectClick;

        // 窗口控制
        _btnBack.Position = new Vector2(1202, 12);
        _btnBack.Size = new Vector2(30, 30);
        _btnBack.Text = "^";
        _btnBack.TextColor = Color.Gold;
        _btnBack.NormalTexture = btnNormal;
        _btnBack.HoverTexture = btnNormal;
        _btnBack.PressedTexture = btnPressed;
        _btnBack.OnClick = () => ScreenManager.Pop();

        _btnMinimize.Position = new Vector2(1166, 12);
        _btnMinimize.Size = new Vector2(30, 30);
        _btnMinimize.Text = "-";
        _btnMinimize.TextColor = Color.Gold;
        _btnMinimize.NormalTexture = btnNormal;
        _btnMinimize.HoverTexture = btnNormal;
        _btnMinimize.PressedTexture = btnPressed;

        _btnClose.Position = new Vector2(1238, 12);
        _btnClose.Size = new Vector2(30, 30);
        _btnClose.Text = "X";
        _btnClose.TextColor = Color.Gold;
        _btnClose.NormalTexture = btnNormal;
        _btnClose.HoverTexture = btnNormal;
        _btnClose.PressedTexture = btnPressed;
        _btnClose.OnClick = () => Game.Exit();

        // 状态标签（对应原项目label1/label2/label3的映射）
        // 原项目：label1=右AI状态, label2=左AI状态, label3=玩家状态
        _lblRightStatus.Position = new Vector2(1110, 190); // 右AI状态
        _lblRightStatus.Text = "";
        _lblRightStatus.TextColor = Color.White;
        _lblRightStatus.Size = new Vector2(100, 30);
        _lblRightStatus.BackgroundColor = new Color(0, 0, 0, 100);

        _lblLeftStatus.Position = new Vector2(20, 190); // 左AI状态
        _lblLeftStatus.Text = "";
        _lblLeftStatus.TextColor = Color.White;
        _lblLeftStatus.Size = new Vector2(100, 30);
        _lblLeftStatus.BackgroundColor = new Color(0, 0, 0, 100);

        _lblMyStatus.Position = new Vector2(540, 370); // 玩家状态
        _lblMyStatus.Text = "";
        _lblMyStatus.TextColor = Color.White;
        _lblMyStatus.Size = new Vector2(200, 30);
        _lblMyStatus.BackgroundColor = new Color(0, 0, 0, 100);

        _lblLeftRemain.Position = new Vector2(60, 470);
        _lblLeftRemain.Text = "剩余: 17";
        _lblLeftRemain.TextColor = Color.White;
        _lblLeftRemain.Size = new Vector2(100, 30);
        _lblLeftRemain.BackgroundColor = new Color(0, 0, 0, 100);

        _lblRightRemain.Position = new Vector2(1120, 470);
        _lblRightRemain.Text = "剩余: 17";
        _lblRightRemain.TextColor = Color.White;
        _lblRightRemain.Size = new Vector2(100, 30);
        _lblRightRemain.BackgroundColor = new Color(0, 0, 0, 100);

        _lblStatus.Position = new Vector2(540, 340);
        _lblStatus.Text = "";
        _lblStatus.TextColor = Color.Yellow;
        _lblStatus.Size = new Vector2(200, 30);
        _lblStatus.TextAlignment = UILabel.AlignmentType.Center;
        _lblStatus.BackgroundColor = new Color(0, 0, 0, 120);

        // 初始状态
        if (_isOnline)
        {
            ButtonSet(false, false, false, false, false);
        }
        else
        {
            ButtonSet(true, false, false, false, false);
        }
        _lblTurnTimer.Visible = false;
        _lblLeftRemain.Visible = false;
        _lblRightRemain.Visible = false;
        _lblStatus.Visible = false;
        _lblLeftStatus.Visible = false;
        _lblRightStatus.Visible = false;
        _lblMyStatus.Visible = false;
    }

    /// <summary>
    /// 按钮设置（对应原buttonset函数）
    /// status=1: 叫地主按钮
    /// status=2: 出牌按钮（出牌/不出/重选/提示）
    /// status=3: 隐藏所有按钮
    /// status=4: 仅显示出牌按钮（首出）
    /// </summary>
    private void ButtonSet(int status, bool visible)
    {
        if (status == 1)
        {
            _btnAction.Visible = visible;
            _btnPass.Visible = visible;
            _btnAction.Text = "叫地主";
            _btnPass.Text = "不叫";
            _btnHint.Visible = false;
            _btnReselect.Visible = false;
        }
        else if (status == 2)
        {
            _btnAction.Position = new Vector2(547, 420);
            _btnReselect.Visible = visible;
            _btnHint.Visible = visible;
            _btnAction.Visible = visible;
            _btnPass.Visible = visible;
            _btnReselect.Text = "重选";
            _btnAction.Text = "出牌";
            _btnPass.Text = "不出";
            _btnHint.Text = "提示";
        }
        else if (status == 3)
        {
            _btnAction.Position = new Vector2(547, 420);
            _btnReselect.Visible = visible;
            _btnHint.Visible = visible;
            _btnAction.Visible = visible;
            _btnPass.Visible = visible;
            _btnReselect.Text = "";
            _btnAction.Text = "";
            _btnPass.Text = "";
            _btnHint.Text = "";
        }
        else if (status == 4)
        {
            _btnAction.Position = new Vector2(595, 420);
            _btnAction.Visible = visible;
            _btnAction.Text = "出牌";
            _btnPass.Visible = false;
            _btnHint.Visible = false;
            _btnReselect.Visible = false;
        }
    }

    // 简化版ButtonSet
    private void ButtonSet(bool start, bool action, bool pass, bool hint, bool reselect)
    {
        _btnStart.Visible = start;
        _btnAction.Visible = action;
        _btnPass.Visible = pass;
        _btnHint.Visible = hint;
        _btnReselect.Visible = reselect;
    }

    #endregion

    #region 更新与渲染

    public override void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

        if (!_isOnline)
        {
            UpdateOffline(dt);
        }
        else
        {
            UpdateOnline(dt);
        }
    }

    private void UpdateOffline(float dt)
    {
        switch (_state)
        {
            case GameState.DEALING:
                UpdateDealing(dt);
                break;
            case GameState.GRABBING:
                UpdateGrabbing(dt);
                break;
            case GameState.MY_TURN:
                UpdateMyTurn(dt);
                break;
            case GameState.AI_TURN:
                UpdateAiTurn(dt);
                break;
        }

        // 更新剩余牌数显示（对应原shengyupai函数）
        _lblLeftRemain.Text = $"剩余牌：{_juese1.ShengYuPai.Count}";
        _lblRightRemain.Text = $"剩余牌：{_juese3.ShengYuPai.Count}";
    }

    private void UpdateDealing(float dt)
    {
        _dealAccumulator += dt;
        while (_dealAccumulator >= DealInterval && _dealtCount < _totalDealCards)
        {
            _dealAccumulator -= DealInterval;
            _dealtCount++;
        }

        if (_dealtCount >= _totalDealCards)
        {
            // 发牌完成，开始叫地主（对应原fapai中的叫地主逻辑）
            StartGrabbingPhase();
        }
    }

    /// <summary>
    /// 开始叫地主阶段（对应原fapai中的叫地主循环）
    /// </summary>
    private void StartGrabbingPhase()
    {
        _state = GameState.GRABBING;

        // 随机选择谁先叫地主（对应原fapai中的Random rd = new Random(); int num = rd.Next(3) + 1;）
        Random rd = new Random();
        _turnNum = rd.Next(3) + 1;

        // 叫地主循环变量
        bool bl1 = false, bl2 = false, bl3 = false;
        int count = 0;
        int switchDiZhu = 0;

        // 开始叫地主逻辑（对应原fapai中的do-while循环）
        ProcessGrabbing(ref bl1, ref bl2, ref bl3, ref count, ref switchDiZhu);
    }

    /// <summary>
    /// 处理叫地主（对应原fapai中的叫地主循环）
    /// </summary>
    private void ProcessGrabbing(ref bool bl1, ref bool bl2, ref bool bl3, ref int count, ref int switchDiZhu)
    {
        // 保存状态变量到类成员，以便ContinueGrabbingAfterPlayerPass使用
        _grabBl1 = bl1;
        _grabBl2 = bl2;
        _grabBl3 = bl3;
        _grabCount = count;
        _grabSwitchDiZhu = switchDiZhu;

        // 原项目逻辑：num=1→juese1, num=2→juese3, num=3→juese2
        // 循环直到有人叫地主或所有人都轮过

        while (count != 3)
        {
            switch (_turnNum)
            {
                case 1:
                    if (!bl1)
                    {
                        bl1 = true; count++; _turnNum++;
                        // juese3叫地主（原项目case 1中是juese3！）
                        if (IsJiaoDiZhu(_juese3))
                        {
                            count = 3; _juese3.Dizhu = true; switchDiZhu = 3;
                            _lblRightStatus.Text = "叫地主"; // label1对应juese3
                            _lblRightStatus.Visible = true;
                        }
                        else
                        {
                            _lblRightStatus.Text = "不  叫";
                            _lblRightStatus.Visible = true;
                        }
                    }
                    else
                    {
                        _turnNum++; // 已轮过，跳到下一个
                    }
                    break;
                case 2:
                    if (!bl2)
                    {
                        bl2 = true; count++; _turnNum++;
                        // juese1叫地主（原项目case 2中是juese1！）
                        if (IsJiaoDiZhu(_juese1))
                        {
                            count = 3; _juese1.Dizhu = true; switchDiZhu = 1;
                            _lblLeftStatus.Text = "叫地主"; // label2对应juese1
                            _lblLeftStatus.Visible = true;
                        }
                        else
                        {
                            _lblLeftStatus.Text = "不  叫";
                            _lblLeftStatus.Visible = true;
                        }
                    }
                    else
                    {
                        _turnNum++; // 已轮过，跳到下一个
                    }
                    break;
                case 3:
                    if (!bl3)
                    {
                        bl3 = true; count++; _turnNum = 1;
                        // 玩家叫地主（需要等待玩家选择）
                        ButtonSet(1, true);
                        _lblMyStatus.Text = "";
                        _lblMyStatus.Visible = true;
                        // 保存当前状态，等待玩家操作
                        _grabBl1 = bl1;
                        _grabBl2 = bl2;
                        _grabBl3 = bl3;
                        _grabCount = count;
                        _grabSwitchDiZhu = switchDiZhu;
                        return; // 等待玩家操作
                    }
                    else
                    {
                        _turnNum = 1; // 已轮过，跳到下一个
                    }
                    break;
            }

            // 更新状态变量
            _grabBl1 = bl1;
            _grabBl2 = bl2;
            _grabBl3 = bl3;
            _grabCount = count;
            _grabSwitchDiZhu = switchDiZhu;
        }

        // 更新最终状态
        _grabSwitchDiZhu = switchDiZhu;

        // 叫地主结束，处理结果
        FinishGrabbing(switchDiZhu);
    }

    /// <summary>
    /// 电脑是否叫地主（对应原isJiaoDiZhu）
    /// </summary>
    private bool IsJiaoDiZhu(Juese juese)
    {
        int quanzhi = 0;
        foreach (int size in juese.ShengYuPai)
        {
            switch (size)
            {
                case 14: quanzhi += 1; break; // A
                case 15: quanzhi += 2; break; // 2
                case 16: quanzhi += 3; break; // 小王
                case 17: quanzhi += 4; break; // 大王
            }
        }
        return quanzhi >= 7;
    }

    /// <summary>
    /// 叫地主结束处理（对应原fapai中叫地主结束后的逻辑）
    /// </summary>
    private void FinishGrabbing(int switchDiZhu)
    {
        // 清空所有状态标签
        _lblLeftStatus.Text = "";
        _lblRightStatus.Text = "";
        _lblMyStatus.Text = "";
        _lblLeftStatus.Visible = false;
        _lblRightStatus.Visible = false;
        _lblMyStatus.Visible = false;

        ButtonSet(3, false);

        if (switchDiZhu != 0)
        {
            // 显示底牌
            _tableCardsRevealed = true;

            // 根据地主位置处理
            if (switchDiZhu == 1)
            {
                // juese1是地主
                KouDiPai(_juese1);
                _landlordId = 1;
            }
            else if (switchDiZhu == 2)
            {
                // juese2是地主
                KouDiPai(_juese2);
                _landlordId = 2;
                _bl_isDiZhu = true;
            }
            else if (switchDiZhu == 3)
            {
                // juese3是地主
                KouDiPai(_juese3);
                _landlordId = 3;
            }

            // 开始出牌
            StartDaPai(switchDiZhu);
        }
        else
        {
            // 无人叫地主
            _lblStatus.Text = "没有人选择地主，重新发牌";
            _noDiZhu = true;
            StartOfflineGame();
        }
    }

    /// <summary>
    /// 扣底牌（对应原kouDiPai）
    /// </summary>
    private void KouDiPai(Juese juese)
    {
        for (int i = 51; i < 54; i++)
        {
            juese.ImagePaiSub.Add(_pai[i].Index);
            juese.ShengYuPai.Add(_pai[i].Size); // 直接使用_pai[i].Size
        }
        PaiPaixu(juese); // 排序ImagePaiSub（按Size降序）

        // ShengYuPai需要同步更新（原项目没有做这个，但我们需要正确）
        juese.ShengYuPai.Clear();
        for (int i = 0; i < juese.ImagePaiSub.Count; i++)
        {
            juese.ShengYuPai.Add(_pai[(int)juese.ImagePaiSub[i]].Size);
        }

        // 更新手牌显示（如果是玩家）
        if (juese.WeiZhi == 2)
        {
            int newCount = _juese2.ShengYuPai.Count;
            _handPositions = CardLayoutManager.CalculateHandPositions(newCount);
            _handSelected = new bool[newCount];
            _selection = new CardSelectionHandler(newCount);
        }
    }

    /// <summary>
    /// 开始打牌（对应原daPai函数）
    /// </summary>
    private void StartDaPai(int landlordPos)
    {
        // 设置轮转计数器（对应原daPai中的num初始值）
        // 原项目：juese1.Dizhu → num=2; juese2.Dizhu → num=1; juese3.Dizhu → num=3
        if (landlordPos == 1)
        {
            _turnNum = 2;
            // 地主(juese1)先出牌
            ComputerChuPai(_juese1);
            _bl_isFirst = true;
        }
        else if (landlordPos == 2)
        {
            _turnNum = 1;
            // 地主(juese2)先出牌 - 等待玩家
            ButtonSet(4, true);
            _state = GameState.MY_TURN;
            _bl_isFirst = true;
            _lblStatus.Text = "请出牌";
            return;
        }
        else if (landlordPos == 3)
        {
            _turnNum = 3;
            // 地主(juese3)先出牌
            ComputerChuPai(_juese3);
            _bl_isFirst = true;
        }

        // 进入出牌循环
        _state = GameState.AI_TURN;
        _aiAccumulator = 0;
    }

    private void UpdateGrabbing(float dt)
    {
        // 等待玩家叫地主选择
    }

    private void UpdateMyTurn(float dt)
    {
        // 等待玩家出牌
    }

    private void UpdateAiTurn(float dt)
    {
        _aiAccumulator += dt;
        if (_aiAccumulator >= AiDelay)
        {
            _aiAccumulator = 0;
            ProcessDaPaiLoop();
        }
    }

    private void UpdateOnline(float dt)
    {
        _netManager?.Update();
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        // 背景
        if (_bgTexture != null)
        {
            spriteBatch.Draw(_bgTexture, new Rectangle(0, 0, 1280, 720), Color.White);
        }
        else
        {
            spriteBatch.Draw(TextureManager.Get("_white") ?? CreateWhitePixel(),
                new Rectangle(0, 0, 1280, 720), new Color(30, 80, 30));
        }

        if (!_isOnline)
        {
            DrawOffline(spriteBatch);
        }
        else
        {
            DrawOnline(spriteBatch);
        }

        DrawUI(spriteBatch);
    }

    private void DrawOffline(SpriteBatch spriteBatch)
    {
        // 底牌
        var tablePositions = CardLayoutManager.CalculateTableCardPositions();
        for (int i = 0; i < 3 && i < _tableCards.Count; i++)
        {
            var tc = _tableCards[i];
            if (_tableCardsRevealed)
                CardRenderer.DrawCard(spriteBatch, tablePositions[i], tc.huase, tc.size);
            else if (_cardBackTexture != null)
                spriteBatch.Draw(_cardBackTexture,
                    new Rectangle((int)tablePositions[i].X, (int)tablePositions[i].Y, CardRenderer.CardWidth, CardRenderer.CardHeight),
                    Color.White);
        }

        // 左侧玩家牌背
        if (_leftCardCount > 0 && _cardBackTexture != null)
        {
            spriteBatch.Draw(_cardBackTexture,
                new Rectangle(20, 220, CardRenderer.CardWidth, CardRenderer.CardHeight),
                Color.White);
        }

        // 右侧玩家牌背
        if (_rightCardCount > 0 && _cardBackTexture != null)
        {
            spriteBatch.Draw(_cardBackTexture,
                new Rectangle(1110, 220, CardRenderer.CardWidth, CardRenderer.CardHeight),
                Color.White);
        }

        // 左侧玩家已出牌
        if (_leftPlayedCards.Count > 0)
        {
            var lp = CardLayoutManager.CalculatePlayedCardPositions(_leftPlayedCards.Count);
            for (int i = 0; i < _leftPlayedCards.Count; i++)
            {
                CardRenderer.DrawCard(spriteBatch, lp[i] - new Vector2(200, 0), _leftPlayedCards[i].huase, _leftPlayedCards[i].size);
            }
        }

        // 右侧玩家已出牌
        if (_rightPlayedCards.Count > 0)
        {
            var rp = CardLayoutManager.CalculatePlayedCardPositions(_rightPlayedCards.Count);
            for (int i = 0; i < _rightPlayedCards.Count; i++)
            {
                CardRenderer.DrawCard(spriteBatch, rp[i] + new Vector2(200, 0), _rightPlayedCards[i].huase, _rightPlayedCards[i].size);
            }
        }

        // 中央已出牌(自己的牌)
        if (_myPlayedCards.Count > 0)
        {
            var cp = CardLayoutManager.CalculatePlayedCardPositions(_myPlayedCards.Count);
            for (int i = 0; i < _myPlayedCards.Count; i++)
            {
                CardRenderer.DrawCard(spriteBatch, cp[i], _myPlayedCards[i].huase, _myPlayedCards[i].size);
            }
        }

        // 玩家手牌
        if (_state != GameState.WAITING)
        {
            DrawHand(spriteBatch);
        }

        // 玩家名
        DrawPlayerNames(spriteBatch);
    }

    private void DrawHand(SpriteBatch spriteBatch)
    {
        int cardCount = _juese2?.ShengYuPai.Count ?? 0;
        if (cardCount == 0) return;

        _handPositions = CardLayoutManager.CalculateHandPositions(cardCount, _handSelected);

        for (int i = cardCount - 1; i >= 0; i--)
        {
            int size = (int)_juese2.ShengYuPai[i];
            int index = (int)_juese2.ImagePaiSub[i];
            string huase = _pai[index].Huase;
            CardRenderer.DrawCard(spriteBatch, _handPositions[i], huase, size, _handSelected[i]);
        }
    }

    private void DrawPlayerNames(SpriteBatch spriteBatch)
    {
        var font = FontManager.Default;
        if (font == null) return;

        spriteBatch.DrawString(font, _leftName, new Vector2(20, 450), Color.White);
        spriteBatch.DrawString(font, _selfName, new Vector2(1005, 630), Color.White);
        spriteBatch.DrawString(font, _rightName, new Vector2(1110, 130), Color.White);
    }

    private void DrawOnline(SpriteBatch spriteBatch)
    {
        // 联机模式绘制（暂不实现）
        DrawOffline(spriteBatch);
    }

    private void DrawUI(SpriteBatch spriteBatch)
    {
        _btnStart.Draw(spriteBatch);
        _btnAction.Draw(spriteBatch);
        _btnPass.Draw(spriteBatch);
        _btnHint.Draw(spriteBatch);
        _btnReselect.Draw(spriteBatch);
        _btnBack.Draw(spriteBatch);
        _btnMinimize.Draw(spriteBatch);
        _btnClose.Draw(spriteBatch);
        _lblLeftStatus.Draw(spriteBatch);
        _lblRightStatus.Draw(spriteBatch);
        _lblMyStatus.Draw(spriteBatch);
        _lblLeftRemain.Draw(spriteBatch);
        _lblRightRemain.Draw(spriteBatch);
        _lblStatus.Draw(spriteBatch);
    }

    #endregion

    #region 输入处理

    public override void HandleInput(InputManager input)
    {
        _mousePos = new Point((int)input.MousePosition.X, (int)input.MousePosition.Y);

        if (input.LeftMouseClicked && _mousePos.Y < 60)
        {
            _isDragging = true;
            _dragOffset = input.MousePosition;
        }
        if (input.LeftMouseReleased) _isDragging = false;

        _btnStart.Update(input);
        _btnAction.Update(input);
        _btnPass.Update(input);
        _btnHint.Update(input);
        _btnReselect.Update(input);
        _btnBack.Update(input);
        _btnMinimize.Update(input);
        _btnClose.Update(input);

        if ((_state == GameState.MY_TURN || _state == GameState.GRABBING))
        {
            HandleCardInput(input);
        }

        if (input.KeyPressed(Keys.Escape))
            ScreenManager.Pop();
    }

    private void HandleCardInput(InputManager input)
    {
        var mousePt = new Point((int)input.MousePosition.X, (int)input.MousePosition.Y);

        if (input.LeftMouseClicked)
        {
            int idx = FindCardIndexAtPosition(mousePt);
            if (idx >= 0)
            {
                _isSelecting = true;
                _selection.OnMouseDown(mousePt, _handPositions, _handSelected);
            }
        }

        if (_isSelecting && input.LeftMouseHeld)
        {
            _selection.OnMouseMove(mousePt, _handPositions, _handSelected);
            var sel = _selection.Selected;
            for (int i = 0; i < sel.Length && i < _handSelected.Length; i++)
                _handSelected[i] = sel[i];
        }

        if (_isSelecting && input.LeftMouseReleased)
        {
            _selection.OnMouseUp(mousePt, _handPositions, _handSelected);

            if (!_selection.HasSlided)
            {
                int idx = FindCardIndexAtPosition(mousePt);
                if (idx >= 0)
                    _selection.ToggleCard(idx);

                var sel = _selection.Selected;
                for (int i = 0; i < sel.Length && i < _handSelected.Length; i++)
                    _handSelected[i] = sel[i];
            }

            _isSelecting = false;
        }
    }

    private int FindCardIndexAtPosition(Point pos)
    {
        int cardCount = _handPositions.Length;
        for (int i = 0; i < cardCount; i++)
        {
            var rect = new Rectangle(
                (int)_handPositions[i].X,
                (int)_handPositions[i].Y,
                CardRenderer.CardWidth,
                CardRenderer.CardHeight
            );
            if (rect.Contains(pos))
                return i;
        }
        return -1;
    }

    #endregion

    #region 按钮事件

    private void OnActionClick()
    {
        if (_isOnline)
        {
            OnOnlineAction();
            return;
        }

        if (_btnAction.Text == "叫地主")
        {
            // 玩家叫地主
            _bl_isDiZhu = true;
            _juese2.Dizhu = true;
            ButtonSet(3, false);
            _lblMyStatus.Text = "叫地主";
            _lblMyStatus.Visible = true;

            // 继续叫地主流程
            bool bl1 = true, bl2 = true, bl3 = true;
            int count = 3;
            int switchDiZhu = 2;
            FinishGrabbing(switchDiZhu);
        }
        else if (_btnAction.Text == "出牌")
        {
            PlaySelectedCards();
        }
    }

    private void OnPassClick()
    {
        if (_isOnline)
        {
            OnOnlinePass();
            return;
        }

        if (_btnPass.Text == "不叫")
        {
            // 玩家不叫地主
            ButtonSet(3, false);
            _lblMyStatus.Text = "不  叫";
            _lblMyStatus.Visible = true;

            // 继续叫地主循环（玩家不叫后继续让AI轮转）
            // 原项目逻辑：玩家不叫后，继续从num=1开始，判断剩余玩家是否叫地主
            ContinueGrabbingAfterPlayerPass();
        }
        else if (_btnPass.Text == "不出")
        {
            PassMyTurn();
        }
    }

    // 叫地主流程状态变量
    private bool _grabBl1, _grabBl2, _grabBl3;
    private int _grabCount;
    private int _grabSwitchDiZhu;

    /// <summary>
    /// 继续叫地主流程（玩家不叫后）
    /// 玩家轮过后（bl3=true, count++），需要继续让剩余玩家轮转
    /// </summary>
    private void ContinueGrabbingAfterPlayerPass()
    {
        // 玩家已经轮过了（bl3=true），count已经++
        // 从_turnNum=1继续轮转

        while (_grabCount != 3)
        {
            switch (_turnNum)
            {
                case 1:
                    if (!_grabBl1)
                    {
                        _grabBl1 = true; _grabCount++; _turnNum++;
                        if (IsJiaoDiZhu(_juese3))
                        {
                            _grabCount = 3; _juese3.Dizhu = true; _grabSwitchDiZhu = 3;
                            _lblRightStatus.Text = "叫地主";
                            _lblRightStatus.Visible = true;
                        }
                        else
                        {
                            _lblRightStatus.Text = "不  叫";
                            _lblRightStatus.Visible = true;
                        }
                    }
                    else
                    {
                        // 已轮过，跳到下一个
                        _turnNum++;
                    }
                    break;
                case 2:
                    if (!_grabBl2)
                    {
                        _grabBl2 = true; _grabCount++; _turnNum++;
                        if (IsJiaoDiZhu(_juese1))
                        {
                            _grabCount = 3; _juese1.Dizhu = true; _grabSwitchDiZhu = 1;
                            _lblLeftStatus.Text = "叫地主";
                            _lblLeftStatus.Visible = true;
                        }
                        else
                        {
                            _lblLeftStatus.Text = "不  叫";
                            _lblLeftStatus.Visible = true;
                        }
                    }
                    else
                    {
                        // 已轮过，跳到下一个
                        _turnNum++;
                    }
                    break;
                case 3:
                    // 玩家已轮过，跳到下一个
                    _turnNum = 1;
                    break;
            }
        }

        FinishGrabbing(_grabSwitchDiZhu);
    }

    private void OnHintClick()
    {
        if (_isOnline) return;

        _tishi++;
        ShowHint();
    }

    private void OnReselectClick()
    {
        _selection.ClearSelection();
        for (int i = 0; i < _handSelected.Length; i++)
            _handSelected[i] = false;
    }

    #endregion

    #region 打牌主循环（一比一复刻原daPai函数）

    /// <summary>
    /// 打牌循环（对应原daPai中的do-while循环）
    /// 原项目num映射：num=1→juese1, num=2→juese3, num=3→juese2
    /// </summary>
    private void ProcessDaPaiLoop()
    {
        if (_bl_chuPaiOver)
        {
            GameOver();
            return;
        }

        // 原项目循环逻辑：
        // num=1: juese1出牌 → num++ (变成2)
        // num=2: juese3出牌 → num++ (变成3)
        // num=3: juese2出牌 → num=1

        switch (_turnNum)
        {
            case 1:
                // juese1(左AI)出牌
                _turnNum++;
                if (_buChuPai == 2)
                {
                    ComputerChuPai(_juese1);
                    _buChuPai = 0;
                }
                else
                {
                    ComputerJiePai(_juese1);
                }
                // 清空状态标签（对应原this.label3.Text = ""）
                _lblRightStatus.Text = "";
                _lblRightStatus.Visible = false;
                break;

            case 2:
                // juese3(右AI)出牌
                _turnNum++;
                if (_buChuPai == 2)
                {
                    ComputerChuPai(_juese3);
                    _buChuPai = 0;
                }
                else
                {
                    ComputerJiePai(_juese3);
                }
                // 清空状态标签（对应原this.label2.Text = ""）
                _lblLeftStatus.Text = "";
                _lblLeftStatus.Visible = false;
                break;

            case 3:
                // juese2(玩家)出牌
                _turnNum = 1;
                if (_buChuPai == 2)
                {
                    ButtonSet(4, true); // 仅显示出牌按钮（首出）
                }
                else
                {
                    ButtonSet(2, true); // 显示所有出牌按钮
                }
                _state = GameState.MY_TURN;
                _lblStatus.Text = "请出牌";
                // 清空状态标签（对应原this.label1.Text = ""）
                _lblMyStatus.Text = "";
                _lblMyStatus.Visible = false;
                return; // 等待玩家操作
        }

        // 检查是否出完
        if (_bl_chuPaiOver)
        {
            GameOver();
            return;
        }

        // 继续AI轮转
        _state = GameState.AI_TURN;
        _aiAccumulator = 0;
    }

    /// <summary>
    /// 电脑出牌（对应原computerChuPai）
    /// </summary>
    private void ComputerChuPai(Juese juese)
    {
        YiChu(); // 隐藏已出的牌

        // 初始化AI
        int landlordPos = _juese1.Dizhu ? 1 : (_juese2.Dizhu ? 2 : 3);
        _computerChuPai.InitializeGame(juese.WeiZhi, juese.Dizhu, landlordPos);

        ArrayList list = _computerChuPai.chuPai(juese.ShengYuPai);
        if (list != null && _chupai.isRight(list))
        {
            // 清空所有ShangShouPai，然后传递给下一个
            _juese1.ShangShouPai.Clear();
            _juese2.ShangShouPai.Clear();
            _juese3.ShangShouPai.Clear();
            MovePai(juese, _jiepai.arrayToArgs(list));
        }
    }

    /// <summary>
    /// 电脑接牌（对应原computerJiePai）
    /// </summary>
    private void ComputerJiePai(Juese juese)
    {
        // 检查ShangShouPai是否为空
        if (juese.ShangShouPai == null || juese.ShangShouPai.Count == 0)
        {
            // 无法接牌，不出
            if (juese == _juese1)
            {
                _lblLeftStatus.Text = "不出";
                _lblLeftStatus.Visible = true;
                // 把牌传给下一个（juese3）
                _juese3.ShangShouPai = (ArrayList)juese.ShangShouPai.Clone();
            }
            else if (juese == _juese3)
            {
                _lblRightStatus.Text = "不出";
                _lblRightStatus.Visible = true;
                // 把牌传给下一个（juese2）
                _juese2.ShangShouPai = (ArrayList)juese.ShangShouPai.Clone();
            }
            _buChuPai++;
            juese.ShangShouPai.Clear();
            return;
        }

        // 尝试接牌
        ArrayList possibleMoves = _jiepai.isRight(_chupai.PaiType, juese.ShangShouPai, juese.ShengYuPai);
        bool success = TiShiJiePai(possibleMoves, juese, false);

        if (!success)
        {
            // 接牌失败，不出
            if (juese == _juese1)
            {
                _lblLeftStatus.Text = "不出";
                _lblLeftStatus.Visible = true;
                _juese3.ShangShouPai = (ArrayList)juese.ShangShouPai.Clone();
            }
            else if (juese == _juese3)
            {
                _lblRightStatus.Text = "不出";
                _lblRightStatus.Visible = true;
                _juese2.ShangShouPai = (ArrayList)juese.ShangShouPai.Clone();
            }
            _buChuPai++;
            juese.ShangShouPai.Clear();
        }
        else
        {
            _buChuPai = 0;
        }
    }

    /// <summary>
    /// 移牌（对应原movePai）- 出牌并传递给下一个接牌者
    /// 原项目逻辑：
    /// WeiZhi=1(左AI)出牌 → 传给juese3的ShangShouPai
    /// WeiZhi=2(玩家)出牌 → 传给juese1的ShangShouPai
    /// WeiZhi=3(右AI)出牌 → 传给juese2的ShangShouPai
    /// </summary>
    private void MovePai(Juese juese, int[] whatPai)
    {
        juese.ShangShouPai.Clear();
        _chupai.format(whatPai);

        // 更新显示
        var playedList = juese.WeiZhi == 1 ? _leftPlayedCards : (juese.WeiZhi == 3 ? _rightPlayedCards : _myPlayedCards);
        playedList.Clear();

        foreach (int size in whatPai)
        {
            // 添加到显示列表
            int imgIdx = -1;
            foreach (int idx in juese.ImagePaiSub)
            {
                if (_pai[idx].Size == size)
                {
                    imgIdx = idx;
                    break;
                }
            }
            if (imgIdx >= 0)
            {
                playedList.Add((_pai[imgIdx].Huase, _pai[imgIdx].Size));
            }

            // 传递给下一个接牌者（对应原movePai中的switch逻辑）
            switch (juese.WeiZhi)
            {
                case 1:
                    _juese3.ShangShouPai.Add(size); // 左AI出牌 → 传给右AI
                    break;
                case 2:
                    _juese1.ShangShouPai.Add(size); // 玩家出牌 → 传给左AI
                    break;
                case 3:
                    _juese2.ShangShouPai.Add(size); // 右AI出牌 → 传给玩家
                    break;
            }

            // 从手牌移除
            int idxToRemove = -1;
            for (int i = 0; i < juese.ShengYuPai.Count; i++)
            {
                if ((int)juese.ShengYuPai[i] == size)
                {
                    idxToRemove = i;
                    break;
                }
            }
            if (idxToRemove >= 0)
            {
                juese.ImagePaiSub.RemoveAt(idxToRemove);
                juese.ShengYuPai.RemoveAt(idxToRemove);
            }
        }

        // 检查是否出完
        if (juese.ShengYuPai.Count == 0)
        {
            _bl_chuPaiOver = true;
        }

        // 更新剩余牌数
        _leftCardCount = _juese1.ShengYuPai.Count;
        _rightCardCount = _juese3.ShengYuPai.Count;

        // 更新手牌显示（如果是玩家）
        if (juese.WeiZhi == 2)
        {
            int newCount = _juese2.ShengYuPai.Count;
            _handPositions = CardLayoutManager.CalculateHandPositions(newCount);
            _handSelected = new bool[newCount];
            _selection = new CardSelectionHandler(newCount);
        }
    }

    /// <summary>
    /// 隐藏已出的牌（对应原yichu）
    /// </summary>
    private void YiChu()
    {
        _leftPlayedCards.Clear();
        _rightPlayedCards.Clear();
        _myPlayedCards.Clear();
    }

    /// <summary>
    /// 提示接牌（对应原tiShiJiePai）- 完整复刻原项目逻辑
    /// </summary>
    private bool TiShiJiePai(ArrayList list, Juese juese, bool isHint)
    {
        if (_chupai.PaiType == (int)Guize.天炸) return false;

        int paiType = _chupai.PaiType;

        #region 单张
        if (paiType == (int)Guize.一张)
        {
            if (list != null)
            {
                int[] jie = null;
                if (((ArrayList)list[0]).Count != 0) jie = _jiepai.mArrayToArgs((ArrayList)list[0]);
                else if (((ArrayList)list[1]).Count != 0) jie = _jiepai.mArrayToArgs((ArrayList)list[1]);
                else if (((ArrayList)list[2]).Count != 0) jie = _jiepai.mArrayToArgs((ArrayList)list[2]);
                else if (((ArrayList)list[3]).Count != 0) jie = _jiepai.mArrayToArgs((ArrayList)list[3]);

                if (jie != null)
                {
                    if (_tishi == jie.Length) _tishi = 0;
                    int[] _jie = new int[] { jie[_tishi] };
                    if (isHint) { TiShiButton(_jie); }
                    else { YiChu(); MovePai(juese, _jie); }
                    return true;
                }
            }
        }
        #endregion
        #region 对子
        else if (paiType == (int)Guize.对子)
        {
            if (list != null)
            {
                int[] jie = null;
                if (((ArrayList)list[0]).Count != 0) jie = _jiepai.mArrayToArgs((ArrayList)list[0]);
                else if (((ArrayList)list[1]).Count != 0) jie = _jiepai.mArrayToArgs((ArrayList)list[1]);
                else if (((ArrayList)list[2]).Count != 0) jie = _jiepai.mArrayToArgs((ArrayList)list[2]);

                if (jie != null)
                {
                    if (_tishi == jie.Length) _tishi = 0;
                    int[] _jie = new int[] { jie[_tishi], jie[_tishi] };
                    if (isHint) { TiShiButton(_jie); }
                    else { YiChu(); MovePai(juese, _jie); }
                    return true;
                }
            }
        }
        #endregion
        #region 三张
        else if (paiType == (int)Guize.三不带)
        {
            if (list != null)
            {
                int[] jie = null;
                if (((ArrayList)list[0]).Count != 0) jie = _jiepai.mArrayToArgs((ArrayList)list[0]);
                else if (((ArrayList)list[1]).Count != 0) jie = _jiepai.mArrayToArgs((ArrayList)list[1]);

                if (jie != null)
                {
                    if (_tishi == jie.Length) _tishi = 0;
                    int[] _jie = new int[] { jie[_tishi], jie[_tishi], jie[_tishi] };
                    if (isHint) { TiShiButton(_jie); }
                    else { YiChu(); MovePai(juese, _jie); }
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
                int[] jie = _jiepai.mArrayToArgs(list);
                if (jie != null)
                {
                    if (_tishi == jie.Length) _tishi = 0;
                    int[] _jie = new int[] { jie[_tishi], jie[_tishi], jie[_tishi], jie[_tishi] };
                    if (isHint) { TiShiButton(_jie); }
                    else { YiChu(); MovePai(juese, _jie); }
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
                if (_tishi == list.Count) _tishi = 0;
                int[] jie = _jiepai.mArrayToArgs((ArrayList)list[_tishi]);
                if (isHint) { TiShiButton(jie); }
                else { YiChu(); MovePai(juese, jie); }
                return true;
            }
        }
        #endregion
        #region 飞机带牌等其他牌型
        else if (paiType > 10 && paiType < 20)
        {
            if (list != null && list.Count != 0)
            {
                if (_tishi == list.Count) _tishi = 0;
                int[] jie = _jiepai.mArrayToArgs((ArrayList)list[_tishi]);
                if (isHint) { TiShiButton(jie); }
                else { YiChu(); MovePai(juese, jie); }
                return true;
            }
        }
        #endregion

        return false;
    }

    /// <summary>
    /// 提示按钮高亮（对应原tiShiBottun）
    /// </summary>
    private void TiShiButton(int[] cards)
    {
        // 清空所有选中
        _selection.ClearSelection();
        for (int i = 0; i < _handSelected.Length; i++)
            _handSelected[i] = false;

        // 高亮提示的牌
        foreach (int size in cards)
        {
            for (int i = 0; i < _juese2.ShengYuPai.Count; i++)
            {
                if ((int)_juese2.ShengYuPai[i] == size && !_handSelected[i])
                {
                    _handSelected[i] = true;
                    _selection.ToggleCard(i);
                    break;
                }
            }
        }
    }

    #endregion

    #region 玩家出牌逻辑（一比一复刻原SinglePlayerDealCards）

    private void PlaySelectedCards()
    {
        var selectedIndices = GetSelectedCardIndices();
        if (selectedIndices.Count == 0)
        {
            _lblStatus.Text = "请选择要出的牌";
            return;
        }

        // 获取选中的牌值
        ArrayList saveList = new ArrayList();
        foreach (int idx in selectedIndices)
        {
            saveList.Add((int)_juese2.ShengYuPai[idx]);
        }

        int paiType = _chupai.PaiType;

        if (saveList.Count != 0)
        {
            if (_chupai.isRight(saveList))
            {
                // 验证通过
                if (_buChuPai != 2 && !_bl_isFirst)
                {
                    // 需要接牌（对应原项目：jiepai.isRight(juese2.ShangShouPai, saveList, paiType)）
                    if (_jiepai.isRight(_juese2.ShangShouPai, saveList, paiType))
                    {
                        YiChu();
                        ButtonSet(3, false);
                        // 玩家出牌前清空juese1.ShangShouPai（因为MovePai会传给juese1）
                        _juese1.ShangShouPai.Clear();
                        MovePai(_juese2, _jiepai.arrayToArgs(saveList));
                        _buChuPai = 0;
                        ContinueDaPaiLoop();
                    }
                    else
                    {
                        _lblStatus.Text = _chupai.PaiType == paiType ? "您出的牌小于上手的牌!" : "您出的牌型不符!";
                        _chupai.PaiType = paiType;
                    }
                }
                else
                {
                    // 首出（对应原项目：清空所有ShangShouPai）
                    YiChu();
                    ButtonSet(3, false);
                    _juese1.ShangShouPai.Clear();
                    _juese2.ShangShouPai.Clear();
                    _juese3.ShangShouPai.Clear();
                    MovePai(_juese2, _jiepai.arrayToArgs(saveList));
                    _buChuPai = 0;
                    _bl_isFirst = false;
                    ContinueDaPaiLoop();
                }
            }
            else
            {
                _chupai.PaiType = paiType;
                _lblStatus.Text = "您出的牌不符合规则!";
            }
        }

        // 重置选中状态
        _selection.ClearSelection();
        for (int i = 0; i < _handSelected.Length; i++)
            _handSelected[i] = false;
    }

    /// <summary>
    /// 玩家不出（对应原button3_Click中不出逻辑）
    /// </summary>
    private void PassMyTurn()
    {
        // 重置牌位置（原项目中把牌顶回到483）
        _selection.ClearSelection();
        for (int i = 0; i < _handSelected.Length; i++)
            _handSelected[i] = false;

        _buChuPai++;
        _tishi = 0;
        ButtonSet(3, false);

        // 把上家的牌传给下一个（左AI）
        _juese1.ShangShouPai.Clear();
        _juese1.ShangShouPai = (ArrayList)_juese2.ShangShouPai.Clone();
        _juese2.ShangShouPai.Clear();

        _lblMyStatus.Text = "不出";
        _lblMyStatus.Visible = true;

        ContinueDaPaiLoop();
    }

    /// <summary>
    /// 继续打牌循环（玩家操作后）
    /// </summary>
    private void ContinueDaPaiLoop()
    {
        // 检查是否出完
        if (_bl_chuPaiOver)
        {
            GameOver();
            return;
        }

        // 继下一个轮次（_turnNum已经是1了）
        _state = GameState.AI_TURN;
        _aiAccumulator = 0;
    }

    private void ShowHint()
    {
        ArrayList possibleMoves = _jiepai.isRight(_chupai.PaiType, _juese2.ShangShouPai, _juese2.ShengYuPai);
        bool success = TiShiJiePai(possibleMoves, _juese2, true);
        if (!success)
        {
            _lblStatus.Text = "没有能大过的牌";
        }
    }

    private List<int> GetSelectedCardIndices()
    {
        var result = new List<int>();
        for (int i = 0; i < _handSelected.Length; i++)
            if (_handSelected[i]) result.Add(i);
        return result;
    }

    #endregion

    #region 游戏结束

    private void GameOver()
    {
        _state = GameState.FINISHED;
        ButtonSet(false, false, false, false, false);

        // 计算胜负（对应原result函数）
        bool[] results = new bool[3];
        string[] names = new string[3];
        names[0] = _leftName;
        names[1] = _selfName;
        names[2] = _rightName;

        if (_juese2.Dizhu && _juese2.ShengYuPai.Count == 0)
        {
            results[1] = true;
        }
        else if (_juese2.ShengYuPai.Count != 0 && _juese2.Dizhu)
        {
            results[0] = true;
            results[2] = true;
        }
        else if (_juese1.Dizhu && _juese1.ShengYuPai.Count == 0)
        {
            results[0] = true;
        }
        else if (_juese1.ShengYuPai.Count != 0 && _juese1.Dizhu)
        {
            results[1] = true;
            results[2] = true;
        }
        else if (_juese3.Dizhu && _juese3.ShengYuPai.Count == 0)
        {
            results[2] = true;
        }
        else if (_juese3.ShengYuPai.Count != 0 && _juese3.Dizhu)
        {
            results[0] = true;
            results[1] = true;
        }

        _lblStatus.Text = results[1] ? "胜利!" : "失败!";
        ScreenManager.Push(new WinScreen(Game, ScreenManager, results, names));
    }

    #endregion

    #region 联机模式（暂时保留原实现）

    private void OnOnlineAction() { }
    private void OnOnlinePass() { }
    private void OnGetCardsReceived(List<CardDto> cardList) { }
    private void OnTurnGrabReceived(int userId) { }
    private void OnGrabLandlordReceived(GrabDto grabDto) { }
    private void OnTurnDealReceived(int userId) { }
    private void OnDealBroadcastReceived(DealDto dealDto) { }
    private void OnDealResponseReceived(int result) { }
    private void OnPassResponseReceived(int result) { }
    private void OnGameOverReceived(OverDto overDto) { }
    private void OnMultipleChangeReceived(int multiple) { }
    private void OnGameStartReceived() { }

    private int _leftCardBackCount = 17;
    private int _rightCardBackCount = 17;

    private static Texture2D? _whitePixel;
    private static Texture2D CreateWhitePixel()
    {
        if (_whitePixel != null) return _whitePixel;
        _whitePixel = new Texture2D(Game1.Instance.GraphicsDevice, 1, 1);
        _whitePixel.SetData(new[] { Color.White });
        TextureManager.Load("_white", "");
        return _whitePixel;
    }

    private readonly UILabel _lblTurnTimer = new();

    #endregion
}