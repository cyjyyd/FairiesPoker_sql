using System.Reflection;
using FairiesPoker.MG;
using FairiesPoker.MG.Core;
using FairiesPoker.MG.Core.Effects;
using FairiesPoker.MG.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

string repository = Path.GetFullPath(args.Length > 0 ? args[0] : ".");
string output = Path.Combine(repository, "artifacts", "visual-smoke");
Directory.CreateDirectory(output);
using var game = new CaptureGame(output);
game.Run();

sealed class CaptureGame : Game1
{
    private readonly string _output;
    private int _stage;
    private int _frames;
    private EffectsPreviewScreen _preview;

    public CaptureGame(string output)
    {
        _output = output;
        ConfigManager.BackMusic = false;
        ConfigManager.SoundFX = false;
        ConfigManager.WindowWidth = 1280;
        ConfigManager.WindowHeight = 720;
        ConfigManager.FullScreen = false;
    }

    protected override void Initialize()
    {
        base.Initialize();
        ApplyDisplaySettings();
    }

    protected override void LoadContent()
    {
        base.LoadContent();
        ScreenManager.Clear();
        ScreenManager.Push(new AboutScreen(this, ScreenManager));
    }

    protected override void Update(GameTime time)
    {
        base.Update(time);
        if (_stage > 0 && _frames == 0)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            typeof(EffectsPreviewScreen).GetMethod("Select", flags)!.Invoke(_preview, new object[] { (PlayEffectKind)_stage });
            typeof(EffectsPreviewScreen).GetMethod("TogglePause", flags)!.Invoke(_preview, null);
            var effects = (PlayEffectManager)typeof(EffectsPreviewScreen).GetField("_effects", flags)!.GetValue(_preview)!;
            effects.Update(_stage == 3 ? 620 : 580);
        }
    }

    protected override void Draw(GameTime time)
    {
        // Capture the application's own drawing commands, independent of desktop occlusion.
        using var target = new RenderTarget2D(GraphicsDevice, 1280, 720);
        GraphicsDevice.SetRenderTarget(target);
        base.Draw(time);
        GraphicsDevice.SetRenderTarget(null);
        if (++_frames < (_stage == 0 ? 30 : 3)) return;
        string name = _stage == 0 ? "about" : ((PlayEffectKind)_stage).ToString();
        using (var stream = File.Create(Path.Combine(_output, name + ".png")))
            target.SaveAsPng(stream, target.Width, target.Height);
        Console.WriteLine("Rendered " + name);
        _frames = 0;
        if (++_stage > 5) { Exit(); return; }
        _preview = new EffectsPreviewScreen(this, ScreenManager);
        ScreenManager.Replace(_preview);
    }

    protected override void EndRun()
    {
        // The smoke harness does not write the player's shared configuration.
        InputManager?.DetachTextInput();
        AudioManager?.Dispose();
        FontManager.Clear();
        UIResourceManager.Clear();
        TextureManager.Clear();
    }
}
