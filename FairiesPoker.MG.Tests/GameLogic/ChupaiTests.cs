using System.Collections;
using FairiesPoker;
using Xunit;

namespace FairiesPoker.MG.Tests.GameLogic;

/// <summary>
/// Comprehensive tests for Chupai card combination validation (牌型判断).
/// Card values: 3-14 (3-A), 15=2, 16=小王, 17=大王
/// </summary>
public class ChupaiTests
{
    private readonly Chupai _chupai;

    public ChupaiTests()
    {
        _chupai = new Chupai();
    }

    // ===== Helper Methods =====

    private static ArrayList ToArrayList(params int[] cards)
    {
        var list = new ArrayList();
        foreach (var c in cards) list.Add(c);
        return list;
    }

    private bool IsRight(params int[] cards)
    {
        return _chupai.isRight(ToArrayList(cards));
    }

    // ===== Single Card (一张) =====

    [Fact]
    public void IsRight_SingleCard_ShouldReturnTrue()
    {
        Assert.True(IsRight(3));
        Assert.Equal((int)Guize.一张, _chupai.PaiType);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(10)]
    [InlineData(14)] // A
    [InlineData(15)] // 2
    [InlineData(16)] // 小王
    [InlineData(17)] // 大王
    public void IsRight_AnySingleCardValue_ShouldReturnTrue(int cardValue)
    {
        Assert.True(IsRight(cardValue));
    }

    // ===== Pair (对子) =====

    [Fact]
    public void IsRight_PairOfSameCards_ShouldReturnTrue()
    {
        Assert.True(IsRight(5, 5));
        Assert.Equal((int)Guize.对子, _chupai.PaiType);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(10)]
    [InlineData(14)]
    [InlineData(15)] // 2
    public void IsRight_PairOfAnyRank_ShouldReturnTrue(int rank)
    {
        Assert.True(IsRight(rank, rank));
    }

    [Fact]
    public void IsRight_TwoDifferentCards_ShouldReturnFalse()
    {
        Assert.False(IsRight(3, 5));
    }

    [Fact]
    public void IsRight_JokersAsRocket_ShouldReturnTrue()
    {
        Assert.True(IsRight(17, 16)); // 大王+小王
        Assert.Equal((int)Guize.天炸, _chupai.PaiType);
    }

    // ===== Three of a Kind (三不带) =====

    [Fact]
    public void IsRight_ThreeOfAKind_ShouldReturnTrue()
    {
        Assert.True(IsRight(7, 7, 7));
        Assert.Equal((int)Guize.三不带, _chupai.PaiType);
    }

    [Fact]
    public void IsRight_ThreeDifferentCards_ShouldReturnFalse()
    {
        Assert.False(IsRight(3, 5, 7));
    }

    [Fact]
    public void IsRight_TwoOfAKind_ShouldReturnFalse()
    {
        Assert.False(IsRight(5, 5, 3));
    }

    // ===== Three with One (三带一) =====

    [Fact]
    public void IsRight_ThreeWithOne_ShouldReturnTrue()
    {
        Assert.True(IsRight(8, 8, 8, 3));
        Assert.Equal((int)Guize.三带一, _chupai.PaiType);
    }

    [Fact]
    public void IsRight_ThreeWithOne_ReversedOrder_ShouldReturnTrue()
    {
        Assert.True(IsRight(3, 8, 8, 8)); // Input not pre-sorted
        Assert.Equal((int)Guize.三带一, _chupai.PaiType);
    }

    [Fact]
    public void IsRight_FourOfAKind_DetectedAsBombNotThreeWithOne()
    {
        // Four of a kind should be detected as 炸弹, not 三带一
        Assert.True(IsRight(5, 5, 5, 5));
        Assert.Equal((int)Guize.炸弹, _chupai.PaiType);
    }

    // ===== Three with Two (三带二) =====

    [Fact]
    public void IsRight_ThreeWithPair_ShouldReturnTrue()
    {
        Assert.True(IsRight(9, 9, 9, 4, 4));
        Assert.Equal((int)Guize.三带二, _chupai.PaiType);
    }

    [Fact]
    public void IsRight_ThreeWithTwoDifferentCards_ShouldReturnFalse()
    {
        Assert.False(IsRight(9, 9, 9, 4, 5));
    }

    // ===== Bomb (炸弹) =====

    [Fact]
    public void IsRight_FourOfAKindBomb_ShouldReturnTrue()
    {
        Assert.True(IsRight(6, 6, 6, 6));
        Assert.Equal((int)Guize.炸弹, _chupai.PaiType);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(10)]
    [InlineData(14)]
    [InlineData(15)] // 2
    public void IsRight_BombOfAnyRank_ShouldReturnTrue(int rank)
    {
        Assert.True(IsRight(rank, rank, rank, rank));
        Assert.Equal((int)Guize.炸弹, _chupai.PaiType);
    }

    // ===== Straight (顺子) - 5 to 12 consecutive cards =====

    [Fact]
    public void IsRight_StraightOfFive_ShouldReturnTrue()
    {
        Assert.True(IsRight(3, 4, 5, 6, 7));
        Assert.Equal((int)Guize.顺子, _chupai.PaiType);
    }

    [Fact]
    public void IsRight_StraightOfTwelve_ShouldReturnTrue()
    {
        Assert.True(IsRight(3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14));
        Assert.Equal((int)Guize.顺子, _chupai.PaiType);
    }

    [Theory]
    [InlineData(3, 5, 6, 7, 8)]     // Gap
    [InlineData(3, 4, 5, 6, 15)]    // Contains 2 (value 15)
    [InlineData(3, 4, 5, 6, 16)]    // Contains 小王
    [InlineData(3, 4, 5, 6, 17)]    // Contains 大王
    public void IsRight_InvalidStraight_ShouldReturnFalse(params int[] cards)
    {
        Assert.False(IsRight(cards));
    }

    // ===== Consecutive Pairs (连对) - 6 to 20 cards =====

    [Fact]
    public void IsRight_ConsecutivePairsOfThreePairs_ShouldReturnTrue()
    {
        Assert.True(IsRight(3, 3, 4, 4, 5, 5));
        Assert.Equal((int)Guize.连对, _chupai.PaiType);
    }

    [Fact]
    public void IsRight_ConsecutivePairsOfSevenPairs_ShouldReturnTrue()
    {
        Assert.True(IsRight(3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 8, 8, 9, 9));
        Assert.Equal((int)Guize.连对, _chupai.PaiType);
    }

    [Fact]
    public void IsRight_NonConsecutivePairs_ShouldReturnFalse()
    {
        Assert.False(IsRight(3, 3, 5, 5, 7, 7));
    }

    // ===== Airplane without Kickers (飞机不带) =====

    [Fact]
    public void IsRight_TwoTriplets_AirplaneWithout_ShouldReturnTrue()
    {
        Assert.True(IsRight(3, 3, 3, 4, 4, 4));
        Assert.Equal((int)Guize.飞机不带, _chupai.PaiType);
    }

    [Fact]
    public void IsRight_ThreeTriplets_AirplaneWithout_ShouldReturnTrue()
    {
        Assert.True(IsRight(5, 5, 5, 6, 6, 6, 7, 7, 7));
        Assert.Equal((int)Guize.三飞机不带, _chupai.PaiType);
    }

    [Fact]
    public void IsRight_FourTriplets_AirplaneWithout_ShouldReturnTrue()
    {
        Assert.True(IsRight(3, 3, 3, 4, 4, 4, 5, 5, 5, 6, 6, 6));
        Assert.Equal((int)Guize.四飞机不带, _chupai.PaiType);
    }

    [Fact]
    public void IsRight_NonConsecutiveTriplets_ShouldReturnFalse()
    {
        Assert.False(IsRight(3, 3, 3, 5, 5, 5));
    }

    [Fact]
    public void IsRight_AirplaneWithJokers_ShouldReturnFalse()
    {
        Assert.False(IsRight(15, 15, 15, 16, 16, 16)); // 2 and 小王 cannot form airplane
    }

    // ===== Four with Two (四带二) =====

    [Fact]
    public void IsRight_FourWithTwo_ShouldReturnTrue()
    {
        Assert.True(IsRight(5, 5, 5, 5, 3, 8));
        Assert.Equal((int)Guize.四带二, _chupai.PaiType);
    }

    // ===== Four with Two Pairs (四带二对) =====

    [Fact]
    public void IsRight_FourWithTwoPairs_ShouldReturnTrue()
    {
        Assert.True(IsRight(7, 7, 7, 7, 3, 3, 5, 5));
        Assert.Equal((int)Guize.四带二对, _chupai.PaiType);
    }

    // ===== Airplane with Kickers =====

    [Fact]
    public void IsRight_AirplaneWithTwo_ShouldReturnTrue()
    {
        // 飞机带二: 6-card airplane + 2 kickers = 8 cards
        Assert.True(IsRight(3, 3, 3, 4, 4, 4, 5, 8));
        Assert.Equal((int)Guize.飞机带二, _chupai.PaiType);
    }

    [Fact]
    public void IsRight_AirplaneWithTwoPairs_ShouldReturnTrue()
    {
        // 飞机带二对: 6-card airplane + 2 pairs = 10 cards
        Assert.True(IsRight(3, 3, 3, 4, 4, 4, 5, 5, 7, 7));
        Assert.Equal((int)Guize.飞机带二对, _chupai.PaiType);
    }

    [Fact]
    public void IsRight_ThreeAirplaneWithThree_ShouldReturnTrue()
    {
        // 三飞机带三: 9-card airplane + 3 kickers = 12 cards
        Assert.True(IsRight(3, 3, 3, 4, 4, 4, 5, 5, 5, 7, 8, 9));
        Assert.Equal((int)Guize.三飞机带三, _chupai.PaiType);
    }

    // ===== Edge Cases =====

    [Theory]
    [InlineData(-1)]      // Negative value
    [InlineData(0)]       // Zero
    [InlineData(1)]       // One (not a valid card)
    [InlineData(2)]       // Two (not a valid card, 2 maps to 15)
    [InlineData(18)]      // Beyond max
    [InlineData(20)]      // Far beyond max
    [InlineData(100)]     // Way out of range
    public void IsRight_InvalidCardValue_ShouldReturnFalse(int badValue)
    {
        // Card values must be 3-15 (3~A), 16=小王, 17=大王
        Assert.False(IsRight(badValue));
    }

    [Theory]
    [InlineData(0, 0)]        // Pair of zeros
    [InlineData(-5, -5)]      // Pair of negatives
    [InlineData(20, 20)]      // Pair of out-of-range
    [InlineData(1, 2)]        // Both invalid
    [InlineData(0, 0, 0)]     // Triplet of zeros
    [InlineData(20, 20, 20, 20)] // Bomb of out-of-range
    public void IsRight_InvalidCardValuesInCombo_ShouldReturnFalse(params int[] cards)
    {
        Assert.False(IsRight(cards));
    }

    [Theory]
    [InlineData(3)]
    [InlineData(10)]
    [InlineData(14)]
    [InlineData(15)]
    [InlineData(16)]
    [InlineData(17)]
    public void IsRight_ValidCardValueBoundary_ShouldAccept(int value)
    {
        Assert.True(IsRight(value));
    }

    [Fact]
    public void IsRight_ValidPairAfterFix_ShouldReturnTrue()
    {
        Assert.True(IsRight(5, 5));
        Assert.Equal((int)Guize.对子, _chupai.PaiType);
    }

    [Fact]
    public void IsRight_EmptyArray_ShouldReturnFalse()
    {
        var list = new ArrayList();
        Assert.False(_chupai.isRight(list));
    }

    [Fact]
    public void IsRight_ThirteenCards_ShouldReturnFalse()
    {
        // No handler for 13 cards in the switch statement
        Assert.False(IsRight(3, 3, 3, 4, 4, 4, 5, 5, 5, 7, 8, 9, 10));
    }

    [Fact]
    public void IsRight_SeventeenCards_ShouldReturnFalse()
    {
        // No handler for 17 cards
        var cards = new int[17];
        for (int i = 0; i < 17; i++) cards[i] = 3;
        Assert.False(_chupai.isRight(ToArrayList(cards)));
    }

    [Fact]
    public void IsRight_FiveOfAKind_MatchesSanDaiErPattern()
    {
        // Chupai checks structural patterns, not card counts
        // Five 3's matches the 三带二 pattern in wuzhang (san_2 checks first 3 equal + remaining as pair)
        Assert.True(IsRight(3, 3, 3, 3, 3));
        // wuzhang checks san_2 before shunzi, and five identical cards match san_2
        Assert.Equal((int)Guize.三带二, _chupai.PaiType);
    }

    // ===== PaiType Consistency =====

    [Fact]
    public void PaiType_AfterValidSingleCard_ShouldBe一张()
    {
        IsRight(10);
        Assert.Equal((int)Guize.一张, _chupai.PaiType);
    }

    [Fact]
    public void PaiType_AfterValidBomb_ShouldBe炸弹()
    {
        IsRight(10, 10, 10, 10);
        Assert.Equal((int)Guize.炸弹, _chupai.PaiType);
    }

    [Fact]
    public void PaiType_AfterInvalidCombination_ShouldRemainZero()
    {
        Assert.False(IsRight(3, 5, 7));
        Assert.Equal(0, _chupai.PaiType);
    }

    // ===== Format Method (Sorting) =====

    [Fact]
    public void Format_DescendingSort_ShouldSortCorrectly()
    {
        int[] args = { 3, 10, 5, 15, 7 };
        _chupai.format(args);
        Assert.Equal(new[] { 15, 10, 7, 5, 3 }, args);
    }

    [Fact]
    public void Format_AlreadySortedDescending_ShouldStaySame()
    {
        int[] args = { 15, 10, 7, 5, 3 };
        _chupai.format(args);
        Assert.Equal(new[] { 15, 10, 7, 5, 3 }, args);
    }

    [Fact]
    public void MinToBig_AscendingSort_ShouldSortCorrectly()
    {
        int[] args = { 10, 3, 15, 5, 7 };
        _chupai.minToBig(args);
        Assert.Equal(new[] { 3, 5, 7, 10, 15 }, args);
    }

    // ===== Array-based isRight =====

    [Fact]
    public void IsRight_IntArray_SingleCard_ShouldReturnTrue()
    {
        Assert.True(_chupai.isRight(new[] { 10 }));
    }

    [Fact]
    public void IsRight_IntArray_Pair_ShouldReturnTrue()
    {
        Assert.True(_chupai.isRight(new[] { 8, 8 }));
    }

    [Fact]
    public void IsRight_IntArray_Invalid_ShouldReturnFalse()
    {
        Assert.False(_chupai.isRight(new[] { 3, 5 }));
    }
}
