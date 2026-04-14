using FairiesPoker.MG.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FairiesPoker.MG.Screens;

/// <summary>
/// 加载屏幕 - 替代Form1
/// 进度条 + 淡入动画
/// </summary>
public class LoadingScreen : ScreenBase
{
    private Texture2D? _bgTexture;
    private Texture2D? _progressBg;

    private float _progressWidth;
    private float _progressStep = 0.8f;  // 每帧增加量
    private readonly float _maxProgress = 375f;
    private bool _fadeComplete;

    public LoadingScreen(Game1 game, ScreenManager screenManager)
        : base(game, screenManager)
    {
        Opacity = 0f;
    }

    public override void LoadContent()
    {
        _bgTexture = TextureManager.Load("_loading_bg", "Resources/loading.png");
        _progressBg = TextureManager.Load("_progress_bg", "Resources/捕获.png");
    }

    public override void Update(GameTime gameTime)
    {
        if (!_fadeComplete)
        {
            FadeIn(0.03);
            if (Opacity >= 1f)
            {
                _fadeComplete = true;
                Opacity = 1f;
            }
        }

        // 进度条动画
        if (_progressWidth < _maxProgress)
        {
            _progressWidth += _progressStep;

            // 触发检查点(与原timer1逻辑对应)
            if (_progressWidth >= 95 && _progressWidth < 95 + _progressStep)
                CheckDlls();
            if (_progressWidth >= 195 && _progressWidth < 195 + _progressStep)
                CheckFiles();
            if (_progressWidth >= 285 && _progressWidth < 285 + _progressStep)
                CheckNet();
            if (_progressWidth >= 375 && _progressWidth < 375 + _progressStep)
                LoadSettings();
        }

        // 进度完成，进入主菜单
        if (_progressWidth >= _maxProgress)
        {
            ScreenManager.Replace(new MainMenuScreen(Game, ScreenManager));
        }
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        var color = Color.White * Opacity;

        if (_bgTexture != null)
        {
            var dest = new Rectangle(0, 0, 742, 346);
            spriteBatch.Draw(_bgTexture, dest, color);
        }

        // 进度条
        if (_progressBg != null)
        {
            var barRect = new Rectangle(236, 267, (int)_progressWidth, 75);
            spriteBatch.Draw(_progressBg, barRect, color);
        }
    }

    public override void HandleInput(InputManager input) { }

    private void CheckDlls() { /* 检查DLL文件 */ }
    private void CheckFiles() { /* 检查资源文件 */ }
    private void CheckNet() { /* 检查网络 */ }
    private void LoadSettings() { ConfigManager.Load(); }
}
