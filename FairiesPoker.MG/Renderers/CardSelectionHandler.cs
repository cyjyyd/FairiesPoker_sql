using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace FairiesPoker.MG.Renderers;

/// <summary>
/// 卡牌选择处理器 - 替代WinForms的PaiImage_MouseDown/Move/Up事件
/// 采用范围滑动选牌(与原DdzMian.cs的slideStartIndex/slideEndIndex逻辑一致)
/// </summary>
public class CardSelectionHandler
{
    private bool _isSelecting;
    private Point _lastMousePos;
    private int _slideStartIndex = -1;  // 滑动起始牌索引(在sorted数组中的位置)
    private int _slideEndIndex = -1;    // 滑动终点牌索引
    private bool _slideSelectMode;      // true=选中(上移), false=取消选中(下移)
    private int _cardCount;
    private bool[] _selected;
    private bool _hasSlided; // 是否真正滑动到了不同的牌
    private bool _hadLayoutOnMouseDown;

    /// <summary>选中状态数组</summary>
    public bool[] Selected => _selected;

    /// <summary>当前选中的牌索引</summary>
    public List<int> SelectedIndices
    {
        get
        {
            var indices = new List<int>();
            for (int i = 0; i < _selected.Length; i++)
            {
                if (_selected[i])
                    indices.Add(i);
            }
            return indices;
        }
    }

    /// <summary>是否有滑动操作(用于区分点击和滑动)</summary>
    public bool HasSlided => _hasSlided;

    public CardSelectionHandler(int cardCount)
    {
        _cardCount = cardCount;
        _selected = new bool[cardCount];
    }

    /// <summary>
    /// 鼠标按下 - 记录起始牌
    /// </summary>
    public void OnMouseDown(Point mousePos)
    {
        _isSelecting = true;
        _lastMousePos = mousePos;
        _hasSlided = false;
        _slideStartIndex = -1;
        _slideEndIndex = -1;
        _hadLayoutOnMouseDown = false;
    }

    /// <summary>
    /// 鼠标按下 - 记录起始牌
    /// </summary>
    /// <param name="mousePos">鼠标位置</param>
    /// <param name="cardPositions">卡牌位置数组</param>
    /// <param name="selectedStates">当前选中状态(用于调整hitbox)</param>
    public void OnMouseDown(Point mousePos, Vector2[] cardPositions, bool[] selectedStates)
    {
        _isSelecting = true;
        _lastMousePos = mousePos;
        _hasSlided = false;
        _hadLayoutOnMouseDown = true;

        // 找到按下的牌
        _slideStartIndex = FindCardIndexAtPosition(mousePos, cardPositions, selectedStates);
        _slideEndIndex = _slideStartIndex;

        if (_slideStartIndex >= 0)
        {
            // 记录初始状态: 未选中=true(选中模式), 已选中=false(取消选中模式)
            _slideSelectMode = !_selected[_slideStartIndex];
        }
    }

    /// <summary>
    /// 鼠标移动 - 更新滑动终点
    /// </summary>
    public void OnMouseMove(Point mousePos, Vector2[] cardPositions)
    {
        OnMouseMove(mousePos, cardPositions, _selected);
    }

    /// <summary>
    /// 鼠标移动 - 更新滑动终点
    /// </summary>
    public void OnMouseMove(Point mousePos, Vector2[] cardPositions, bool[] selectedStates)
    {
        if (!_isSelecting) return;

        int currentIndex = FindCardIndexAtPosition(mousePos, cardPositions, selectedStates);

        if (_slideStartIndex < 0)
        {
            _slideStartIndex = currentIndex;
            _slideEndIndex = currentIndex;
            if (_slideStartIndex >= 0)
            {
                _slideSelectMode = !_selected[_slideStartIndex];
                if (!_hadLayoutOnMouseDown)
                {
                    _selected[_slideStartIndex] = _slideSelectMode;
                }
            }
            _lastMousePos = mousePos;
            return;
        }

        if (currentIndex >= 0)
        {
            if (currentIndex == _slideEndIndex)
            {
                if (!_hadLayoutOnMouseDown)
                {
                    _selected[currentIndex] = _slideSelectMode;
                }
                _lastMousePos = mousePos;
                return;
            }

            _slideEndIndex = currentIndex;
            _hasSlided = true;
            // 实时更新预览
            UpdateSlidePreview(cardPositions, selectedStates);
        }

        _lastMousePos = mousePos;
    }

    /// <summary>
    /// 鼠标松开 - 应用滑动结果或切换单张
    /// </summary>
    public void OnMouseUp(Point mousePos, Vector2[] cardPositions)
    {
        OnMouseUp(mousePos, cardPositions, _selected);
    }

    /// <summary>
    /// 鼠标松开 - 应用滑动结果或切换单张
    /// </summary>
    public void OnMouseUp(Point mousePos, Vector2[] cardPositions, bool[] selectedStates)
    {
        _isSelecting = false;

        if (_hasSlided && _slideStartIndex >= 0 && _slideEndIndex >= 0)
        {
            // 有滑动: 对start到end范围内的牌应用选择模式
            ApplySlideSelection();
        }
        // 无滑动: 不做任何操作, 由调用方处理点击切换
    }

    /// <summary>
    /// 实时更新滑动预览
    /// </summary>
    private void UpdateSlidePreview(Vector2[] cardPositions, bool[] selectedStates)
    {
        int min = Math.Min(_slideStartIndex, _slideEndIndex);
        int max = Math.Max(_slideStartIndex, _slideEndIndex);

        for (int i = 0; i < _cardCount; i++)
        {
            // 不在滑动范围内的牌: 恢复原始状态
            bool shouldSelect = _slideSelectMode;
            if (i >= min && i <= max)
            {
                _selected[i] = shouldSelect;
            }
        }
    }

    /// <summary>
    /// 应用滑动选牌结果
    /// </summary>
    private void ApplySlideSelection()
    {
        int min = Math.Min(_slideStartIndex, _slideEndIndex);
        int max = Math.Max(_slideStartIndex, _slideEndIndex);

        for (int i = min; i <= max; i++)
        {
            if (i >= 0 && i < _cardCount)
            {
                _selected[i] = _slideSelectMode;
            }
        }
    }

    /// <summary>
    /// 点击切换单张牌选中状态
    /// </summary>
    public void ToggleCard(int cardIndex)
    {
        if (cardIndex >= 0 && cardIndex < _cardCount)
        {
            _selected[cardIndex] = !_selected[cardIndex];
        }
    }

    /// <summary>
    /// 查找鼠标位置对应的牌索引
    /// </summary>
    public static int FindCardIndexAtPosition(Point pos, Vector2[] cardPositions, bool[] selectedStates)
    {
        if (cardPositions.Length == 0)
            return -1;

        bool rightToLeft = cardPositions[0].X >= cardPositions[cardPositions.Length - 1].X;
        int start = rightToLeft ? 0 : cardPositions.Length - 1;
        int end = rightToLeft ? cardPositions.Length : -1;
        int step = rightToLeft ? 1 : -1;

        for (int i = start; i != end; i += step)
        {
            if (GetCardBounds(i, cardPositions, selectedStates).Contains(pos))
                return i;
        }

        return -1;
    }

    private static Rectangle GetCardBounds(int index, Vector2[] cardPositions, bool[] selectedStates)
    {
        int yOffset = selectedStates != null && index < selectedStates.Length && selectedStates[index]
            ? -CardRenderer.SelectedOffset
            : 0;

        return new Rectangle(
            (int)cardPositions[index].X,
            (int)cardPositions[index].Y + yOffset,
            CardRenderer.CardWidth,
            CardRenderer.CardHeight
        );
    }

    /// <summary>
    /// 清除所有选中
    /// </summary>
    public void ClearSelection()
    {
        for (int i = 0; i < _cardCount; i++)
            _selected[i] = false;
    }

    /// <summary>
    /// 重置选择器并按新的牌数调整状态数组。
    /// </summary>
    public void Reset(int cardCount)
    {
        _cardCount = Math.Max(0, cardCount);
        _selected = new bool[_cardCount];
        _isSelecting = false;
        _slideStartIndex = -1;
        _slideEndIndex = -1;
        _hasSlided = false;
        _hadLayoutOnMouseDown = false;
    }

    /// <summary>
    /// 设置所有选中
    /// </summary>
    public void SelectAll()
    {
        for (int i = 0; i < _cardCount; i++)
            _selected[i] = true;
    }

    /// <summary>
    /// 检查是否有选中的牌
    /// </summary>
    public bool HasSelection()
    {
        for (int i = 0; i < _cardCount; i++)
            if (_selected[i]) return true;
        return false;
    }
}
