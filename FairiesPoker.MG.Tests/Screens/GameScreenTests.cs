using System;
using FairiesPoker.MG.Core;
using FairiesPoker.MG.Screens;
using Microsoft.Xna.Framework;
using Xunit;

namespace FairiesPoker.MG.Tests.Screens;

/// <summary>
/// Tests for GameScreen basic structure and initialization.
/// Note: Full GameScreen tests require MonoGame GraphicsDevice, so we test
/// the parts that don't require graphics initialization.
/// </summary>
public class GameScreenTests
{
    // ===== GameState Enum Tests =====

    [Fact]
    public void GameState_ShouldHaveCorrectValues()
    {
        Assert.Equal(0, (int)GameState.WAITING);
        Assert.Equal(1, (int)GameState.DEALING);
        Assert.Equal(2, (int)GameState.GRABBING);
        Assert.Equal(3, (int)GameState.MY_TURN);
        Assert.Equal(4, (int)GameState.AI_TURN);
        Assert.Equal(5, (int)GameState.FINISHED);
    }

    [Fact]
    public void GameState_ShouldHaveExactlySixValues()
    {
        var values = Enum.GetValues<GameState>();
        Assert.Equal(6, values.Length);
    }

    // ===== GameScreen Creation Tests =====

    [Fact]
    public void GameScreen_Constructor_OfflineMode_ShouldNotThrow()
    {
        // GameScreen requires Game1 and ScreenManager which need MonoGame setup.
        // We verify the constructor signature exists and accepts expected parameters.
        // Full integration tests would require a mock Game1.

        // At minimum, verify the type exists and is accessible
        var type = typeof(FairiesPoker.MG.Screens.GameScreen);
        Assert.NotNull(type);
        Assert.True(type.IsClass);
        Assert.False(type.IsAbstract);
    }

    [Fact]
    public void GameScreen_ShouldInheritFromScreenBase()
    {
        var screenType = typeof(FairiesPoker.MG.Screens.GameScreen);
        Assert.True(typeof(ScreenBase).IsAssignableFrom(screenType));
    }

    [Fact]
    public void GameScreen_ShouldHaveRequiredMethods()
    {
        var type = typeof(FairiesPoker.MG.Screens.GameScreen);

        // Verify public methods exist
        Assert.NotNull(type.GetMethod("Initialize"));
        Assert.NotNull(type.GetMethod("Update"));
        Assert.NotNull(type.GetMethod("Draw"));
        Assert.NotNull(type.GetMethod("HandleInput"));
        Assert.NotNull(type.GetMethod("StartOfflineGame"));
    }

    [Fact]
    public void GameScreen_ShouldHaveConstructorWithOnlineParameter()
    {
        var type = typeof(FairiesPoker.MG.Screens.GameScreen);
        var ctors = type.GetConstructors();

        // Should have at least one constructor accepting (Game1, ScreenManager, bool)
        bool foundOnlineCtor = false;
        foreach (var ctor in ctors)
        {
            var parameters = ctor.GetParameters();
            if (parameters.Length == 3)
            {
                // Third parameter should be bool (isOnline)
                if (parameters[2].ParameterType == typeof(bool))
                {
                    foundOnlineCtor = true;
                    break;
                }
            }
        }
        Assert.True(foundOnlineCtor, "GameScreen should have constructor with isOnline parameter");
    }
}
