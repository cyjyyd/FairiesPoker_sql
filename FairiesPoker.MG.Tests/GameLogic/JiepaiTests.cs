using System.Collections;
using FairiesPoker;
using Xunit;

namespace FairiesPoker.MG.Tests.GameLogic;

/// <summary>
/// Tests for Jiepai card-beating logic (接牌判断).
/// Verifies whether a hand can beat the previous play.
/// </summary>
public class JiepaiTests
{
    private readonly Jiepai _jiepai;

    public JiepaiTests()
    {
        _jiepai = new Jiepai();
    }

    // ===== Helper Methods =====

    private static ArrayList ToArrayList(params int[] cards)
    {
        var list = new ArrayList();
        foreach (var c in cards) list.Add(c);
        return list;
    }

    // ===== Can Beat Tests =====

    [Fact]
    public void IsRight_SingleCardBiggerThanPrevious_ShouldReturnTrue()
    {
        // Previous: single 5, Next: single 8
        var upPai = ToArrayList(5);
        var nextPai = ToArrayList(8);
        Assert.True(_jiepai.isRight(upPai, nextPai, (int)Guize.一张));
    }

    [Fact]
    public void IsRight_SingleCardSmallerThanPrevious_ShouldReturnFalse()
    {
        // Previous: single 10, Next: single 5
        var upPai = ToArrayList(10);
        var nextPai = ToArrayList(5);
        Assert.False(_jiepai.isRight(upPai, nextPai, (int)Guize.一张));
    }

    [Fact]
    public void IsRight_BombBeatsAnyNonBomb_ShouldReturnTrue()
    {
        // Previous: pair of 10s, Next: bomb of 5s
        var upPai = ToArrayList(10, 10);
        var nextPai = ToArrayList(5, 5, 5, 5);
        Assert.True(_jiepai.isRight(upPai, nextPai, (int)Guize.对子));
    }

    [Fact]
    public void IsRight_RocketBeatsBomb_ShouldReturnTrue()
    {
        // Previous: bomb of 3s, Next: rocket (jokers)
        var upPai = ToArrayList(3, 3, 3, 3);
        var nextPai = ToArrayList(17, 16);
        Assert.True(_jiepai.isRight(upPai, nextPai, (int)Guize.炸弹));
    }

    [Fact]
    public void IsRight_RocketBeatsAnyType_ShouldReturnTrue()
    {
        // Previous: straight, Next: rocket
        var upPai = ToArrayList(3, 4, 5, 6, 7);
        var nextPai = ToArrayList(17, 16);
        Assert.True(_jiepai.isRight(upPai, nextPai, (int)Guize.顺子));
    }

    [Fact]
    public void IsRight_PairBiggerThanPrevious_ShouldReturnTrue()
    {
        var upPai = ToArrayList(5, 5);
        var nextPai = ToArrayList(8, 8);
        Assert.True(_jiepai.isRight(upPai, nextPai, (int)Guize.对子));
    }

    [Fact]
    public void IsRight_PairSmallerThanPrevious_ShouldReturnFalse()
    {
        var upPai = ToArrayList(10, 10);
        var nextPai = ToArrayList(5, 5);
        Assert.False(_jiepai.isRight(upPai, nextPai, (int)Guize.对子));
    }

    [Fact]
    public void IsRight_StraightBiggerThanPrevious_ShouldReturnTrue()
    {
        var upPai = ToArrayList(3, 4, 5, 6, 7);
        var nextPai = ToArrayList(4, 5, 6, 7, 8);
        Assert.True(_jiepai.isRight(upPai, nextPai, (int)Guize.顺子));
    }

    [Fact]
    public void IsRight_DifferentTypeCannotBeat_ShouldReturnFalse()
    {
        // Previous: pair, Next: single (different type, can't beat even if bigger)
        var upPai = ToArrayList(3, 3);
        var nextPai = ToArrayList(10);
        Assert.False(_jiepai.isRight(upPai, nextPai, (int)Guize.对子));
    }

    [Fact]
    public void IsRight_SameTypeSameValue_ShouldReturnFalse()
    {
        // Same value cannot beat
        var upPai = ToArrayList(8, 8);
        var nextPai = ToArrayList(8, 8);
        Assert.False(_jiepai.isRight(upPai, nextPai, (int)Guize.对子));
    }

    // ===== Edge Cases =====

    [Fact]
    public void IsRight_EmptyUpPai_ShouldReturnFalse()
    {
        var upPai = new ArrayList();
        var nextPai = ToArrayList(5);
        Assert.False(_jiepai.isRight(upPai, nextPai, (int)Guize.一张));
    }

    [Fact]
    public void IsRight_EmptyNextPai_ShouldReturnFalse()
    {
        var upPai = ToArrayList(5);
        var nextPai = new ArrayList();
        Assert.False(_jiepai.isRight(upPai, nextPai, (int)Guize.一张));
    }

    [Fact]
    public void IsRight_InvalidNextPaiCombination_ShouldReturnFalse()
    {
        var upPai = ToArrayList(5);
        var nextPai = ToArrayList(3, 7); // Not a valid combination
        Assert.False(_jiepai.isRight(upPai, nextPai, (int)Guize.一张));
    }

    [Fact]
    public void IsRight_ConsecutivePairsBigger_ShouldReturnTrue()
    {
        var upPai = ToArrayList(3, 3, 4, 4, 5, 5);
        var nextPai = ToArrayList(4, 4, 5, 5, 6, 6);
        Assert.True(_jiepai.isRight(upPai, nextPai, (int)Guize.连对));
    }

    [Fact]
    public void IsRight_ThreeOfKindBigger_ShouldReturnTrue()
    {
        var upPai = ToArrayList(5, 5, 5);
        var nextPai = ToArrayList(8, 8, 8);
        Assert.True(_jiepai.isRight(upPai, nextPai, (int)Guize.三不带));
    }

    // ===== 2 as High Card =====

    [Fact]
    public void IsRight_TwoBeatsAce_ShouldReturnTrue()
    {
        // 2 (value 15) beats A (value 14)
        var upPai = ToArrayList(14);
        var nextPai = ToArrayList(15);
        Assert.True(_jiepai.isRight(upPai, nextPai, (int)Guize.一张));
    }

    [Fact]
    public void IsRight_SmallJokerBeatsTwo_ShouldReturnTrue()
    {
        var upPai = ToArrayList(15);
        var nextPai = ToArrayList(16);
        Assert.True(_jiepai.isRight(upPai, nextPai, (int)Guize.一张));
    }

    [Fact]
    public void IsRight_BigJokerBeatsSmallJoker_ShouldReturnTrue()
    {
        var upPai = ToArrayList(16);
        var nextPai = ToArrayList(17);
        Assert.True(_jiepai.isRight(upPai, nextPai, (int)Guize.一张));
    }
}
