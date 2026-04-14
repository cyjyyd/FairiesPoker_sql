using FairiesPoker.MG.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpriteFontPlus;
using System.IO;

namespace FairiesPoker.MG;

public class Game1 : Game
{
    public static Game1 Instance { get; private set; } = null!;

    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch = null!;

    public ScreenManager ScreenManager { get; private set; } = null!;
    public InputManager InputManager { get; private set; } = null!;
    public AudioManager AudioManager { get; private set; } = null!;

    public Game1()
    {
        Instance = this;
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        // 加载配置
        ConfigManager.Load();
        _graphics.PreferredBackBufferWidth = ConfigManager.WindowWidth;
        _graphics.PreferredBackBufferHeight = ConfigManager.WindowHeight;
        _graphics.IsFullScreen = ConfigManager.FullScreen;
    }

    protected override void Initialize()
    {
        TextureManager.Initialize(GraphicsDevice);
        InputManager = new InputManager();
        AudioManager = new AudioManager
        {
            BackMusicEnabled = ConfigManager.BackMusic,
            SoundFXEnabled = ConfigManager.SoundFX
        };

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // 创建1x1白色像素纹理(用于绘制纯色矩形/背景)
        var whitePixel = new Texture2D(GraphicsDevice, 1, 1);
        whitePixel.SetData(new[] { Color.White });
        TextureManager.LoadInternal("_white", whitePixel);

        // 创建默认字体 (使用系统字体位图)
        CreateDefaultFont();

        ScreenManager = new ScreenManager(this, _spriteBatch);

        // 压入第一个屏幕
        ScreenManager.Push(new Screens.MainMenuScreen(this, ScreenManager));
    }

    private void CreateDefaultFont()
    {
        // 从系统TTF运行时烘焙字体(支持中文)
        string[] fontFiles = {
            "C:\\WINDOWS\\Fonts\\msyh.ttc",       // 微软雅黑
            "C:\\WINDOWS\\Fonts\\STKAITI.TTF",    // 华文楷体
            "C:\\WINDOWS\\Fonts\\simhei.ttf",     // 黑体
            "C:\\WINDOWS\\Fonts\\STXIHEI.TTF",    // 华文细黑
            "C:\\WINDOWS\\Fonts\\SIMYOU.TTF",     // 幼圆
        };

        foreach (var ttfPath in fontFiles)
        {
            if (!File.Exists(ttfPath)) continue;

            try
            {
                var result = TtfFontBaker.Bake(
                    File.ReadAllBytes(ttfPath),
                    16f, 2048, 2048,
                    new[] {
                        new CharacterRange((char)0x20, (char)0x7E),    // ASCII
                        new CharacterRange((char)0x2000, (char)0x206F), // 通用标点
                        new CharacterRange((char)0x3000, (char)0x303F), // CJK标点
                        new CharacterRange((char)0x3400, (char)0x4DB5), // CJK扩展A
                        new CharacterRange((char)0x4E00, (char)0x9FEF), // CJK统一汉字
                        new CharacterRange((char)0xFF00, (char)0xFFEF), // 全角
                    }
                );
                var sf = result.CreateSpriteFont(GraphicsDevice);
                // 检查中文字符是否能正确测量(非问号)
                var testWidth = sf.MeasureString("测试");
                if (testWidth.X > 10) // 中文字符应该有实际宽度,不是默认问号
                {
                    FontManager.Load("default", sf);
                    return;
                }
            }
            catch { }
        }
    }

    protected override void Update(GameTime gameTime)
    {
        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        InputManager.Update();
        ScreenManager.Update(gameTime, InputManager);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        ScreenManager.Draw(gameTime);
    }

    protected override void EndRun()
    {
        AudioManager?.Dispose();
        ConfigManager.Save();
        base.EndRun();
    }
}
