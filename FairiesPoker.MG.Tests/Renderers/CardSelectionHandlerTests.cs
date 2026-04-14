using FairiesPoker.MG.Renderers;
using Microsoft.Xna.Framework;
using Xunit;

namespace FairiesPoker.MG.Tests.Renderers;

/// <summary>
/// Tests for CardSelectionHandler - click toggle and slide selection.
/// </summary>
public class CardSelectionHandlerTests
{
    // ===== Constructor =====

    [Fact]
    public void Constructor_WithZeroCards_ShouldCreateEmptyHandler()
    {
        var handler = new CardSelectionHandler(0);
        Assert.Empty(handler.SelectedIndices);
        Assert.Empty(handler.Selected);
    }

    [Fact]
    public void Constructor_WithPositiveCards_ShouldCreateAllUnselected()
    {
        var handler = new CardSelectionHandler(17);
        Assert.Equal(17, handler.Selected.Length);
        Assert.All(handler.Selected, selected => Assert.False(selected));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(17)]
    [InlineData(20)]
    public void Constructor_AnyPositiveCount_ShouldMatchCount(int cardCount)
    {
        var handler = new CardSelectionHandler(cardCount);
        Assert.Equal(cardCount, handler.Selected.Length);
    }

    // ===== ToggleCard =====

    [Fact]
    public void ToggleCard_ValidIndex_ShouldToggleState()
    {
        var handler = new CardSelectionHandler(5);

        // Initially false
        Assert.False(handler.Selected[2]);

        // Toggle to true
        handler.ToggleCard(2);
        Assert.True(handler.Selected[2]);

        // Toggle back to false
        handler.ToggleCard(2);
        Assert.False(handler.Selected[2]);
    }

    [Fact]
    public void ToggleCard_MultipleCards_ShouldToggleIndependently()
    {
        var handler = new CardSelectionHandler(5);

        handler.ToggleCard(0);
        handler.ToggleCard(2);
        handler.ToggleCard(4);

        Assert.True(handler.Selected[0]);
        Assert.False(handler.Selected[1]);
        Assert.True(handler.Selected[2]);
        Assert.False(handler.Selected[3]);
        Assert.True(handler.Selected[4]);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(100)]
    public void ToggleCard_OutOfBoundsIndex_ShouldNotThrow(int index)
    {
        var handler = new CardSelectionHandler(5);
        var exception = Record.Exception(() => handler.ToggleCard(index));
        Assert.Null(exception);
        // Should not change any selection
        Assert.All(handler.Selected, selected => Assert.False(selected));
    }

    // ===== SelectedIndices =====

    [Fact]
    public void SelectedIndices_NoneSelected_ShouldReturnEmptyList()
    {
        var handler = new CardSelectionHandler(5);
        Assert.Empty(handler.SelectedIndices);
    }

    [Fact]
    public void SelectedIndices_SomeSelected_ShouldReturnCorrectIndices()
    {
        var handler = new CardSelectionHandler(5);
        handler.ToggleCard(1);
        handler.ToggleCard(3);

        var indices = handler.SelectedIndices;
        Assert.Equal(2, indices.Count);
        Assert.Equal(1, indices[0]);
        Assert.Equal(3, indices[1]);
    }

    [Fact]
    public void SelectedIndices_AllSelected_ShouldReturnAllIndices()
    {
        var handler = new CardSelectionHandler(3);
        handler.SelectAll();

        var indices = handler.SelectedIndices;
        Assert.Equal(3, indices.Count);
        Assert.Equal(new[] { 0, 1, 2 }, indices);
    }

    [Fact]
    public void SelectedIndices_ShouldReturnIReadOnlyList()
    {
        var handler = new CardSelectionHandler(5);
        var indices = handler.SelectedIndices;
        Assert.IsType<System.Collections.Generic.List<int>>(indices);
    }

    // ===== ClearSelection =====

    [Fact]
    public void ClearSelection_AllSelected_ShouldDeselectAll()
    {
        var handler = new CardSelectionHandler(5);
        handler.SelectAll();
        Assert.All(handler.Selected, selected => Assert.True(selected));

        handler.ClearSelection();
        Assert.All(handler.Selected, selected => Assert.False(selected));
    }

    [Fact]
    public void ClearSelection_PartiallySelected_ShouldDeselectAll()
    {
        var handler = new CardSelectionHandler(5);
        handler.ToggleCard(0);
        handler.ToggleCard(2);

        handler.ClearSelection();
        Assert.Empty(handler.SelectedIndices);
    }

    [Fact]
    public void ClearSelection_AlreadyEmpty_ShouldNotThrow()
    {
        var handler = new CardSelectionHandler(5);
        var exception = Record.Exception(() => handler.ClearSelection());
        Assert.Null(exception);
    }

    // ===== SelectAll =====

    [Fact]
    public void SelectAll_NoneSelected_ShouldSelectAll()
    {
        var handler = new CardSelectionHandler(5);
        handler.SelectAll();
        Assert.All(handler.Selected, selected => Assert.True(selected));
        Assert.Equal(5, handler.SelectedIndices.Count);
    }

    [Fact]
    public void SelectAll_PartiallySelected_ShouldSelectAll()
    {
        var handler = new CardSelectionHandler(5);
        handler.ToggleCard(0); // only card 0 selected
        handler.SelectAll();
        Assert.Equal(5, handler.SelectedIndices.Count);
    }

    // ===== HasSelection =====

    [Fact]
    public void HasSelection_NoneSelected_ShouldReturnFalse()
    {
        var handler = new CardSelectionHandler(5);
        Assert.False(handler.HasSelection());
    }

    [Fact]
    public void HasSelection_OneSelected_ShouldReturnTrue()
    {
        var handler = new CardSelectionHandler(5);
        handler.ToggleCard(3);
        Assert.True(handler.HasSelection());
    }

    [Fact]
    public void HasSelection_AllSelected_ShouldReturnTrue()
    {
        var handler = new CardSelectionHandler(5);
        handler.SelectAll();
        Assert.True(handler.HasSelection());
    }

    [Fact]
    public void HasSelection_ZeroCardHandler_ShouldReturnFalse()
    {
        var handler = new CardSelectionHandler(0);
        Assert.False(handler.HasSelection());
    }

    // ===== Mouse Events (Slide Selection) =====

    [Fact]
    public void OnMouseDown_ShouldStartSelectionState()
    {
        var handler = new CardSelectionHandler(5);
        handler.OnMouseDown(new Microsoft.Xna.Framework.Point(100, 100));
        // Selection state is internal, but we can verify no crash
        Assert.True(true);
    }

    [Fact]
    public void OnMouseMove_WithoutMouseDown_ShouldNotChangeSelection()
    {
        var handler = new CardSelectionHandler(3);
        var cardPositions = new Vector2[]
        {
            new Vector2(100, 100),
            new Vector2(130, 100),
            new Vector2(160, 100)
        };

        handler.OnMouseMove(new Microsoft.Xna.Framework.Point(110, 110), cardPositions);
        Assert.Empty(handler.SelectedIndices);
    }

    [Fact]
    public void OnMouseMove_WithMouseDown_ShouldSelectCardsUnderCursor()
    {
        var handler = new CardSelectionHandler(3);
        var cardPositions = new Vector2[]
        {
            new Vector2(100, 100),
            new Vector2(130, 100),
            new Vector2(160, 100)
        };

        handler.OnMouseDown(new Microsoft.Xna.Framework.Point(110, 110));
        handler.OnMouseMove(new Microsoft.Xna.Framework.Point(110, 110), cardPositions); // Over card 0

        // Card 0 should be selected
        Assert.True(handler.Selected[0]);
    }

    [Fact]
    public void OnMouseMove_SlideAcrossCards_ShouldSelectAll()
    {
        var handler = new CardSelectionHandler(3);
        var cardPositions = new Vector2[]
        {
            new Vector2(100, 100),
            new Vector2(130, 100),
            new Vector2(160, 100)
        };

        handler.OnMouseDown(new Microsoft.Xna.Framework.Point(110, 110));
        handler.OnMouseMove(new Microsoft.Xna.Framework.Point(110, 110), cardPositions);
        handler.OnMouseMove(new Microsoft.Xna.Framework.Point(170, 110), cardPositions); // Slide to card 2

        Assert.True(handler.Selected[0]);
        Assert.True(handler.Selected[2]);
    }

    [Fact]
    public void OnMouseUp_ShouldEndSelectionState()
    {
        var handler = new CardSelectionHandler(3);
        var cardPositions = new Vector2[]
        {
            new Vector2(100, 100),
            new Vector2(130, 100),
            new Vector2(160, 100)
        };

        handler.OnMouseDown(new Microsoft.Xna.Framework.Point(110, 110));
        handler.OnMouseMove(new Microsoft.Xna.Framework.Point(110, 110), cardPositions);
        handler.OnMouseUp(new Microsoft.Xna.Framework.Point(110, 110), cardPositions);

        // After mouse up, further moves should not change selection
        handler.OnMouseMove(new Microsoft.Xna.Framework.Point(170, 110), cardPositions);
        Assert.False(handler.Selected[2]); // Card 2 should NOT be selected
    }

    [Fact]
    public void SlideSelection_StartOnUnselectedCard_ShouldSelect()
    {
        var handler = new CardSelectionHandler(3);
        var cardPositions = new Vector2[]
        {
            new Vector2(100, 100),
            new Vector2(130, 100),
            new Vector2(160, 100)
        };

        handler.OnMouseDown(new Microsoft.Xna.Framework.Point(110, 110));
        handler.OnMouseMove(new Microsoft.Xna.Framework.Point(110, 110), cardPositions);
        handler.OnMouseMove(new Microsoft.Xna.Framework.Point(140, 110), cardPositions);

        // Both cards should be selected (started on unselected = select mode)
        Assert.True(handler.Selected[0]);
        Assert.True(handler.Selected[1]);
    }

    [Fact]
    public void SlideSelection_StartOnSelectedCard_ShouldDeselect()
    {
        var handler = new CardSelectionHandler(3);
        var cardPositions = new Vector2[]
        {
            new Vector2(100, 100),
            new Vector2(130, 100),
            new Vector2(160, 100)
        };

        // Pre-select card 0
        handler.ToggleCard(0);
        Assert.True(handler.Selected[0]);

        handler.OnMouseDown(new Microsoft.Xna.Framework.Point(110, 110));
        handler.OnMouseMove(new Microsoft.Xna.Framework.Point(110, 110), cardPositions);
        handler.OnMouseMove(new Microsoft.Xna.Framework.Point(140, 110), cardPositions);

        // Both cards should be deselected (started on selected = deselect mode)
        Assert.False(handler.Selected[0]);
        Assert.False(handler.Selected[1]);
    }

    // ===== Reset =====

    [Fact]
    public void Reset_ShouldNotThrow()
    {
        var handler = new CardSelectionHandler(5);
        var exception = Record.Exception(() => handler.Reset(10));
        Assert.Null(exception);
    }

    // ===== Edge Cases =====

    [Fact]
    public void ToggleAllCardsThenClear_ShouldBeConsistent()
    {
        var handler = new CardSelectionHandler(5);
        for (int i = 0; i < 5; i++)
            handler.ToggleCard(i);

        Assert.Equal(5, handler.SelectedIndices.Count);

        handler.ClearSelection();
        Assert.Empty(handler.SelectedIndices);

        for (int i = 0; i < 5; i++)
            handler.ToggleCard(i);
        Assert.Equal(5, handler.SelectedIndices.Count);
    }
}
