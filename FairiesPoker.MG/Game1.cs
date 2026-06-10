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
    private int _themeBgmStartDelayFrames;

    public ScreenManager ScreenManager { get; private set; } = null!;
    public InputManager InputManager { get; private set; } = null!;
    public AudioManager AudioManager { get; private set; } = null!;

    public Game1()
    {
        Instance = this;
        _graphics = new GraphicsDeviceManager(this);
#if ANDROID
        _graphics.GraphicsProfile = GraphicsProfile.Reach;
#else
        _graphics.GraphicsProfile = GraphicsProfile.HiDef;
#endif
        Content.RootDirectory = "Content";
        IsMouseVisible = !OperatingSystem.IsAndroid();

        // 加载配置
        ConfigManager.Load();
        ConfigureBackBuffer();
    }

    protected override void Initialize()
    {
#if !ANDROID
        Window.Title = "FairiesPoker";
        WindowIconManager.Apply(this);
#endif
        DisplayManager.Update(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
        TextureManager.Initialize(GraphicsDevice);
        UIResourceManager.Initialize(GraphicsDevice, ConfigManager.UITheme);
        UIResourceManager.LoadThemeResources();
        InputManager = new InputManager();
        InputManager.AttachTextInput(Window);
        AudioManager = new AudioManager
        {
            BackMusicEnabled = ConfigManager.BackMusic,
            SoundFXEnabled = ConfigManager.SoundFX,
            BgmVolume = ConfigManager.BackMusicVolume,
            SfxVolume = ConfigManager.SoundFXVolume
        };
#if ANDROID
        _themeBgmStartDelayFrames = 30;
#else
        AudioManager.PlayThemeBgm();
#endif

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
#if !ANDROID
        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
#endif

        DisplayManager.Update(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
        FontManager.UpdateForDisplayScale(DisplayManager.Scale);
        PlatformTextInputService.Update();
        TryStartDeferredThemeBgm();
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
#if !ANDROID
        ConfigManager.NormalizeWindowSize();
#endif
        ConfigureBackBuffer();
        _graphics.ApplyChanges();
        DisplayManager.Update(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
        FontManager.UpdateForDisplayScale(DisplayManager.Scale);
    }

    private void ConfigureBackBuffer()
    {
#if ANDROID
        Point backBufferSize = GetAndroidBackBufferSize();
        _graphics.IsFullScreen = true;
        _graphics.PreferredBackBufferWidth = backBufferSize.X;
        _graphics.PreferredBackBufferHeight = backBufferSize.Y;
        _graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
#else
        ConfigManager.NormalizeWindowSize();
        _graphics.PreferredBackBufferWidth = Math.Max(640, ConfigManager.WindowWidth);
        _graphics.PreferredBackBufferHeight = Math.Max(360, ConfigManager.WindowHeight);
        _graphics.IsFullScreen = ConfigManager.FullScreen;
        _graphics.HardwareModeSwitch = true;
        Window.IsBorderless = !ConfigManager.FullScreen && ConfigManager.BorderlessWindow;
#endif
    }

#if ANDROID
    private static Point GetAndroidBackBufferSize()
    {
        int width = ConfigManager.DefaultWindowWidth;
        int height = ConfigManager.DefaultWindowHeight;

        try
        {
            foreach (DisplayMode mode in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
            {
                if (mode.Width <= 0 || mode.Height <= 0)
                    continue;

                if ((long)mode.Width * mode.Height > (long)width * height)
                {
                    width = mode.Width;
                    height = mode.Height;
                }
            }

            DisplayMode currentMode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
            if (currentMode.Width > 0 && currentMode.Height > 0 &&
                (long)currentMode.Width * currentMode.Height > (long)width * height)
            {
                width = currentMode.Width;
                height = currentMode.Height;
            }
        }
        catch
        {
            width = ConfigManager.DefaultWindowWidth;
            height = ConfigManager.DefaultWindowHeight;
        }

        if (height > width)
            (width, height) = (height, width);

        return new Point(Math.Max(1, width), Math.Max(1, height));
    }
#endif

    protected override void EndRun()
    {
        InputManager?.DetachTextInput();
        AudioManager?.Dispose();
        FontManager.Clear();
        UIResourceManager.Clear();
        TextureManager.Clear();
        ConfigManager.Save();
        base.EndRun();
    }

    private void TryStartDeferredThemeBgm()
    {
        if (_themeBgmStartDelayFrames <= 0 || AudioManager == null)
            return;

        _themeBgmStartDelayFrames--;
        if (_themeBgmStartDelayFrames == 0)
            AudioManager.PlayThemeBgm();
    }
}
