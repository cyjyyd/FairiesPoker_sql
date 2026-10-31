using FairiesPoker;
using FairiesPoker.MG.Core;
using FairiesPoker.MG.Core.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Protocol.Constant;

namespace FairiesPoker.MG.Tests.Core;

public class PlayEffectTests
{
    [Fact]
    public void ConflictingOfflineAndWireValues_AreMappedSeparately()
    {
        // Both have numeric value 4, but mean a bomb offline and consecutive pairs online.
        Assert.Equal(PlayEffectKind.Bomb, PlayEffectMapping.FromOffline((Guize)4));
        Assert.Equal(PlayEffectKind.ConsecutivePairs, PlayEffectMapping.FromOnline(4));
    }

    [Theory]
    [InlineData(Guize.飞机不带)]
    [InlineData(Guize.三飞机不带)]
    [InlineData(Guize.四飞机不带)]
    [InlineData(Guize.五飞机不带)]
    [InlineData(Guize.六飞机不带)]
    [InlineData(Guize.飞机带二)]
    [InlineData(Guize.飞机带二对)]
    [InlineData(Guize.三飞机带三)]
    [InlineData(Guize.四飞机带四)]
    [InlineData(Guize.三飞机带三对)]
    [InlineData(Guize.四飞机带四对)]
    [InlineData(Guize.五飞机带五)]
    public void EverySupportedOfflineAirplane_UsesAirplaneEffect(Guize type) =>
        Assert.Equal(PlayEffectKind.Airplane, PlayEffectMapping.FromOffline(type));

    [Theory]
    [InlineData(CardType.STRAIGHT, PlayEffectKind.Straight)]
    [InlineData(CardType.DOUBLE_STRAIGHT, PlayEffectKind.ConsecutivePairs)]
    [InlineData(CardType.TRIPLE_STRAIGHT, PlayEffectKind.Airplane)]
    [InlineData(CardType.BOOM, PlayEffectKind.Bomb)]
    [InlineData(CardType.JOKER_BOOM, PlayEffectKind.JokerBomb)]
    [InlineData(CardType.THREE, PlayEffectKind.None)]
    [InlineData(-1, PlayEffectKind.None)]
    public void WireTypes_MapWithoutReinterpretingOfflineValues(int type, PlayEffectKind expected) =>
        Assert.Equal(expected, PlayEffectMapping.FromOnline(type));

    [Fact]
    public void RapidBroadcasts_StayBoundedAndExpireWithoutCompletionCallbacks()
    {
        using var effects = new PlayEffectManager();
        for (int i = 0; i < 30; i++) effects.Trigger(PlayEffectKind.JokerBomb, new Vector2(640, 310));
        Assert.Equal(PlayEffectManager.MaxActiveEffects, effects.ActiveCount);
        effects.Update(2000); // A delayed frame must retire effects, not prolong game state.
        Assert.Equal(0, effects.ActiveCount);
    }

    [Fact]
    public void UnsupportedOrInvalidInput_DoesNotCreateAnEffect()
    {
        using var effects = new PlayEffectManager();
        effects.Trigger(PlayEffectKind.None, Vector2.Zero);
        effects.Trigger((PlayEffectKind)99, Vector2.Zero);
        effects.Trigger(PlayEffectKind.Bomb, new Vector2(float.NaN, 300));
        Assert.Equal(0, effects.ActiveCount);
    }

    [Fact]
    public void ResetAndDispose_DoNotNeedAGraphicsDeviceOrKeepOldEffects()
    {
        var effects = new PlayEffectManager();
        effects.Trigger(PlayEffectKind.Airplane, new Vector2(640, 310));
        effects.Update(float.NaN);
        effects.Update(-1);
        Assert.Equal(1, effects.ActiveCount);
        effects.Clear();
        Assert.Equal(0, effects.ActiveCount);
        effects.Trigger(PlayEffectKind.Bomb, new Vector2(640, 310));
        effects.Dispose();
        effects.Dispose();
        Assert.Equal(0, effects.ActiveCount);
    }

    [Fact]
    public void Overlay_UpdatesBackgroundPresentationButNotBackgroundGameLogic()
    {
        var manager = new ScreenManager(null!, null!);
        var game = new ProbeScreen();
        var overlay = new ProbeScreen();
        manager.Push(game);
        manager.Push(overlay);
        manager.Update(new GameTime(), null!);
        Assert.Equal(1, game.PresentationTicks);
        Assert.Equal(0, game.GameTicks);
        Assert.Equal(1, overlay.PresentationTicks);
        Assert.Equal(1, overlay.GameTicks);
        manager.Pop();
        manager.Update(new GameTime(), null!);
        Assert.Equal(2, game.PresentationTicks);
        Assert.Equal(1, game.GameTicks);
    }

    private sealed class ProbeScreen : ScreenBase
    {
        public int PresentationTicks;
        public int GameTicks;
        public ProbeScreen() : base(null!, null!) { }
        public override void UpdatePresentation(GameTime time) => PresentationTicks++;
        public override void Update(GameTime time) => GameTicks++;
        public override void Draw(GameTime time, SpriteBatch batch) { }
        public override void HandleInput(InputManager input) { }
    }
}
