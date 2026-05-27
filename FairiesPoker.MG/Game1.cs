using FairiesPoker.MG.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

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
        _graphics.GraphicsProfile = GraphicsProfile.HiDef;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        // 加载配置
        ConfigManager.Load();
        ConfigureBackBuffer();
    }

    protected override void Initialize()
    {
        DisplayManager.Update(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
        TextureManager.Initialize(GraphicsDevice);
        UIResourceManager.Initialize(GraphicsDevice, ConfigManager.UITheme);
        UIResourceManager.LoadThemeResources();
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

        // 创建默认字体 (使用系统字体位图，随显示缩放重新烘焙)
        FontManager.Initialize(GraphicsDevice);

        ScreenManager = new ScreenManager(this, _spriteBatch);

        // 压入第一个屏幕
        ScreenManager.Push(new Screens.MainMenuScreen(this, ScreenManager));
    }

    protected override void Update(GameTime gameTime)
    {
        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        DisplayManager.Update(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
        FontManager.UpdateForDisplayScale(DisplayManager.Scale);
        InputManager.Update();
        ScreenManager.Update(gameTime, InputManager);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        DisplayManager.Update(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
        FontManager.UpdateForDisplayScale(DisplayManager.Scale);
        GraphicsDevice.Clear(Color.Black);
        ScreenManager.Draw(gameTime);
    }

    public void ApplyDisplaySettings()
    {
        ConfigManager.NormalizeWindowSize();
        ConfigureBackBuffer();
        _graphics.ApplyChanges();
        DisplayManager.Update(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
        FontManager.UpdateForDisplayScale(DisplayManager.Scale);
    }

    private void ConfigureBackBuffer()
    {
        ConfigManager.NormalizeWindowSize();
        _graphics.PreferredBackBufferWidth = Math.Max(640, ConfigManager.WindowWidth);
        _graphics.PreferredBackBufferHeight = Math.Max(360, ConfigManager.WindowHeight);
        _graphics.IsFullScreen = ConfigManager.FullScreen;
        _graphics.HardwareModeSwitch = true;
        Window.IsBorderless = !ConfigManager.FullScreen && ConfigManager.BorderlessWindow;
    }

    protected override void EndRun()
    {
        AudioManager?.Dispose();
        ConfigManager.Save();
        base.EndRun();
    }
}
