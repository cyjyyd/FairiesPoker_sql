using FairiesPoker.MG.Renderers;
using Microsoft.Xna.Framework;
using Xunit;

namespace FairiesPoker.MG.Tests.Renderers;

/// <summary>
/// Tests for CardLayoutManager position calculations.
/// Verifies card layout formulas match the original WinForms DdzMian.Designer.cs logic.
/// </summary>
public class CardLayoutManagerTests
{
    private const float Epsilon = 0.01f;

    // ===== CalculateHandPositions =====

    [Fact]
    public void CalculateHandPositions_ZeroCards_ShouldReturnEmptyArray()
    {
        var positions = CardLayoutManager.CalculateHandPositions(0);
        Assert.Empty(positions);
    }

    [Fact]
    public void CalculateHandPositions_OneCard_ShouldReturnSinglePosition()
    {
        var positions = CardLayoutManager.CalculateHandPositions(1);
        Assert.Single(positions);
        // Formula: startX = 640 + (1*30 + 120)/2 - 150 = 640 + 75 - 150 = 565
        Assert.Equal(565f, positions[0].X, Epsilon);
        Assert.Equal(483f, positions[0].Y, Epsilon);
    }

    [Fact]
    public void CalculateHandPositions_Standard17Cards_ShouldReturn17Positions()
    {
        var positions = CardLayoutManager.CalculateHandPositions(17);
        Assert.Equal(17, positions.Length);

        // Verify Y coordinate is consistent for all cards
        for (int i = 0; i < positions.Length; i++)
        {
            Assert.Equal(483f, positions[i].Y, Epsilon);
        }

        // Verify cards are spaced from right to left (descending X)
        for (int i = 0; i < positions.Length - 1; i++)
        {
            Assert.True(positions[i].X > positions[i + 1].X);
            Assert.Equal(30f, positions[i].X - positions[i + 1].X, Epsilon);
        }
    }

    [Fact]
    public void CalculateHandPositions_20Cards_ShouldReturn20Positions()
    {
        var positions = CardLayoutManager.CalculateHandPositions(20);
        Assert.Equal(20, positions.Length);

        // All Y should be SelfHandY = 483
        for (int i = 0; i < positions.Length; i++)
        {
            Assert.Equal(483f, positions[i].Y, Epsilon);
        }
    }

    [Fact]
    public void CalculateHandPositions_AllYCoordsShouldBeSelfHandY()
    {
        for (int count = 1; count <= 20; count++)
        {
            var positions = CardLayoutManager.CalculateHandPositions(count);
            for (int i = 0; i < positions.Length; i++)
            {
                Assert.Equal(483f, positions[i].Y);
            }
        }
    }

    [Fact]
    public void CalculateHandPositions_CenteringShouldBeCorrect()
    {
        // For 17 cards: startX = 640 + (17*30+120)/2 - 150 = 640 + 315 - 150 = 805
        // Last card X = 805 - 16*30 = 805 - 480 = 325
        // Center of spread = (805 + 325) / 2 = 565... but formula aims for centering
        // The formula centers the hand at the window center
        var positions = CardLayoutManager.CalculateHandPositions(17);
        float minX = positions[16].X;
        float maxX = positions[0].X;
        float centerX = (minX + maxX + 120) / 2; // +120 for card width offset
        Assert.True(centerX > 500 && centerX < 800, $"Center X {centerX} should be near window center");
    }

    // ===== CalculateHandPositions with Selected =====

    [Fact]
    public void CalculateHandPositions_WithSelected_ShouldMoveSelectedCardsUp()
    {
        var selected = new bool[] { false, true, false, true, false };
        var positions = CardLayoutManager.CalculateHandPositions(5, selected);

        // Unselected should be at SelfHandY = 483
        Assert.Equal(483f, positions[0].Y, Epsilon);
        Assert.Equal(483f, positions[2].Y, Epsilon);
        Assert.Equal(483f, positions[4].Y, Epsilon);

        // Selected should be at SelectedHandY = 453
        Assert.Equal(453f, positions[1].Y, Epsilon);
        Assert.Equal(453f, positions[3].Y, Epsilon);
    }

    [Fact]
    public void CalculateHandPositions_WithAllSelected_ShouldMoveAllUp()
    {
        var selected = new bool[] { true, true, true };
        var positions = CardLayoutManager.CalculateHandPositions(3, selected);

        for (int i = 0; i < 3; i++)
        {
            Assert.Equal(453f, positions[i].Y, Epsilon);
        }
    }

    [Fact]
    public void CalculateHandPositions_WithNoneSelected_ShouldKeepAllAtBase()
    {
        var selected = new bool[] { false, false, false };
        var positions = CardLayoutManager.CalculateHandPositions(3, selected);

        for (int i = 0; i < 3; i++)
        {
            Assert.Equal(483f, positions[i].Y, Epsilon);
        }
    }

    // ===== CalculateTableCardPositions =====

    [Fact]
    public void CalculateTableCardPositions_ShouldReturnExactly3Positions()
    {
        var positions = CardLayoutManager.CalculateTableCardPositions();
        Assert.Equal(3, positions.Length);
    }

    [Fact]
    public void CalculateTableCardPositions_ShouldBeHorizontalAndEvenlySpaced()
    {
        var positions = CardLayoutManager.CalculateTableCardPositions();

        // All should have same Y
        Assert.Equal(positions[0].Y, positions[1].Y);
        Assert.Equal(positions[1].Y, positions[2].Y);

        // X spacing should be 140
        Assert.Equal(140f, positions[1].X - positions[0].X, Epsilon);
        Assert.Equal(140f, positions[2].X - positions[1].X, Epsilon);

        // First card at TableCardStartX = 440
        Assert.Equal(440f, positions[0].X, Epsilon);
    }

    [Fact]
    public void CalculateTableCardPositions_YShouldBeTableCardY()
    {
        var positions = CardLayoutManager.CalculateTableCardPositions();
        for (int i = 0; i < positions.Length; i++)
        {
            Assert.Equal(6f, positions[i].Y, Epsilon);
        }
    }

    // ===== CalculateLeftPlayerBackPositions =====

    [Fact]
    public void CalculateLeftPlayerBackPositions_ZeroCards_ShouldReturnEmpty()
    {
        var positions = CardLayoutManager.CalculateLeftPlayerBackPositions(0);
        Assert.Empty(positions);
    }

    [Fact]
    public void CalculateLeftPlayerBackPositions_Standard17Cards_ShouldReturn17Positions()
    {
        var positions = CardLayoutManager.CalculateLeftPlayerBackPositions(17);
        Assert.Equal(17, positions.Length);
    }

    [Fact]
    public void CalculateLeftPlayerBackPositions_ShouldUseGridLayout()
    {
        var positions = CardLayoutManager.CalculateLeftPlayerBackPositions(12);

        // 12 cards = 2 rows of 6
        // Cards 0-5: row 0, cards 6-11: row 1
        Assert.Equal(positions[0].Y, positions[5].Y); // Same row
        Assert.True(positions[6].Y > positions[0].Y);  // Next row is lower

        // Within a row, X increases
        Assert.True(positions[1].X > positions[0].X);
        Assert.Equal(15f, positions[1].X - positions[0].X, Epsilon); // colSpacing

        // Row spacing should be 65
        Assert.Equal(65f, positions[6].Y - positions[0].Y, Epsilon);

        // First card at LeftPlayerCardX = 20, LeftPlayerCardStartY = 220
        Assert.Equal(20f, positions[0].X, Epsilon);
        Assert.Equal(220f, positions[0].Y, Epsilon);
    }

    [Fact]
    public void CalculateLeftPlayerBackPositions_20Cards_ShouldReturn20Positions()
    {
        var positions = CardLayoutManager.CalculateLeftPlayerBackPositions(20);
        Assert.Equal(20, positions.Length);
        // 20 cards = 3 full rows (6+6+6) + 2 more = 4 rows
        Assert.Equal(4, GetUniqueRows(positions));
    }

    // ===== CalculateRightPlayerBackPositions =====

    [Fact]
    public void CalculateRightPlayerBackPositions_ZeroCards_ShouldReturnEmpty()
    {
        var positions = CardLayoutManager.CalculateRightPlayerBackPositions(0);
        Assert.Empty(positions);
    }

    [Fact]
    public void CalculateRightPlayerBackPositions_Standard17Cards_ShouldReturn17Positions()
    {
        var positions = CardLayoutManager.CalculateRightPlayerBackPositions(17);
        Assert.Equal(17, positions.Length);
    }

    [Fact]
    public void CalculateRightPlayerBackPositions_ShouldBeOnRightSide()
    {
        var positions = CardLayoutManager.CalculateRightPlayerBackPositions(6);

        // First card at RightPlayerCardX = 1110
        Assert.True(positions[0].X >= 1110f);
        Assert.Equal(220f, positions[0].Y, Epsilon);
    }

    [Fact]
    public void CalculateRightPlayerBackPositions_LayoutShouldMatchLeftPattern()
    {
        var leftPositions = CardLayoutManager.CalculateLeftPlayerBackPositions(6);
        var rightPositions = CardLayoutManager.CalculateRightPlayerBackPositions(6);

        // Same row/col pattern, different X base
        Assert.Equal(leftPositions[0].Y, rightPositions[0].Y);
        Assert.True(rightPositions[0].X > leftPositions[0].X);

        // Same spacing pattern
        var leftSpacing = leftPositions[1].X - leftPositions[0].X;
        var rightSpacing = rightPositions[1].X - rightPositions[0].X;
        Assert.Equal(leftSpacing, rightSpacing);
    }

    // ===== CalculatePlayedCardPositions =====

    [Fact]
    public void CalculatePlayedCardPositions_ZeroCards_ShouldReturnEmpty()
    {
        var positions = CardLayoutManager.CalculatePlayedCardPositions(0);
        Assert.Empty(positions);
    }

    [Fact]
    public void CalculatePlayedCardPositions_OneCard_ShouldReturnSinglePosition()
    {
        var positions = CardLayoutManager.CalculatePlayedCardPositions(1);
        Assert.Single(positions);
        // totalWidth = 1*30 + 120 = 150, startX = 640 - 75 = 565
        Assert.Equal(565f, positions[0].X, Epsilon);
        Assert.Equal(193f, positions[0].Y, Epsilon);
    }

    [Fact]
    public void CalculatePlayedCardPositions_FiveCards_ShouldBeCentered()
    {
        var positions = CardLayoutManager.CalculatePlayedCardPositions(5);
        Assert.Equal(5, positions.Length);

        // All Y should be PlayedCardsY = 193
        for (int i = 0; i < positions.Length; i++)
        {
            Assert.Equal(193f, positions[i].Y, Epsilon);
        }

        // Should be evenly spaced
        for (int i = 0; i < positions.Length - 1; i++)
        {
            Assert.Equal(30f, positions[i + 1].X - positions[i].X, Epsilon);
        }

        // Center should be near WindowCenterX = 640
        float center = (positions[0].X + positions[4].X + 120) / 2;
        Assert.True(center > 600 && center < 680);
    }

    [Fact]
    public void CalculatePlayedCardPositions_MoreCardsMeansWiderSpread()
    {
        var positions5 = CardLayoutManager.CalculatePlayedCardPositions(5);
        var positions10 = CardLayoutManager.CalculatePlayedCardPositions(10);

        float spread5 = positions5[4].X - positions5[0].X;
        float spread10 = positions10[9].X - positions10[0].X;

        Assert.True(spread10 > spread5);
    }

    // ===== CalculateCardX =====

    [Fact]
    public void CalculateCardX_ShouldMatchArrayPosition()
    {
        int totalCards = 17;
        var arrayPositions = CardLayoutManager.CalculateHandPositions(totalCards);

        for (int i = 0; i < totalCards; i++)
        {
            float x = CardLayoutManager.CalculateCardX(i, totalCards);
            Assert.Equal(arrayPositions[i].X, x, Epsilon);
        }
    }

    // ===== GetDealStartPos =====

    [Fact]
    public void GetDealStartPos_ShouldReturnFixedPosition()
    {
        var pos = CardLayoutManager.GetDealStartPos();
        Assert.Equal(1110f, pos.X, Epsilon);
        Assert.Equal(220f, pos.Y, Epsilon);
    }

    // ===== Utility Methods =====

    private static int GetUniqueRows(Vector2[] positions)
    {
        var uniqueYs = new System.Collections.Generic.HashSet<float>();
        foreach (var p in positions)
        {
            uniqueYs.Add(p.Y);
        }
        return uniqueYs.Count;
    }
}
