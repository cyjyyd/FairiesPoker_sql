using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace FairiesPoker.MG.Renderers;

/// <summary>
/// 卡牌布局管理器 - 计算手牌/对手牌/底牌/出牌区域的卡牌位置
/// 公式完全复用原DdzMian.cs的image_paixu逻辑
/// </summary>
public static class CardLayoutManager
{
    // === 坐标常量(与DdzMian.Designer.cs一致) ===
    private const int WindowCenterX = 640;
    private const int SelfHandY = 483;
    private const int SelectedHandY = 453;
    private const int PlayedCardsY = 193;
    private const int LeftPlayerCardX = 20;
    private const int LeftPlayerCardStartY = 220;
    private const int RightPlayerCardX = 1110;
    private const int RightPlayerCardStartY = 220;
    private const int TableCardStartX = 440;
    private const int TableCardY = 6;
    private const int TableCardSpacing = 140;

    /// <summary>
    /// 计算手牌位置(玩家自己的牌)
    /// 公式: startX = 640 + (cardCount * 30 + 120) / 2 - 150
    /// </summary>
    public static Vector2[] CalculateHandPositions(int cardCount)
    {
        var positions = new Vector2[cardCount];
        int startX = WindowCenterX + (cardCount * CardRenderer.OverlapSpacing + 120) / 2 - 150;

        for (int i = 0; i < cardCount; i++)
        {
            positions[i] = new Vector2(startX - i * CardRenderer.OverlapSpacing, SelfHandY);
        }
        return positions;
    }

    /// <summary>
    /// 计算手牌位置(带选中状态)
    /// </summary>
    public static Vector2[] CalculateHandPositions(int cardCount, bool[] selected)
    {
        var positions = CalculateHandPositions(cardCount);
        for (int i = 0; i < cardCount; i++)
        {
            if (selected[i])
                positions[i].Y = SelectedHandY;
        }
        return positions;
    }

    /// <summary>
    /// 计算左侧玩家牌背位置(垂直堆叠, 3层x6张)
    /// </summary>
    public static Vector2[] CalculateLeftPlayerBackPositions(int cardCount)
    {
        var positions = new Vector2[cardCount];
        int cardsPerRow = 6;
        int rowSpacing = 65;
        int colSpacing = 15;

        for (int i = 0; i < cardCount; i++)
        {
            int row = i / cardsPerRow;
            int col = i % cardsPerRow;
            positions[i] = new Vector2(
                LeftPlayerCardX + col * colSpacing,
                LeftPlayerCardStartY + row * rowSpacing
            );
        }
        return positions;
    }

    /// <summary>
    /// 计算右侧玩家牌背位置(垂直堆叠, 3层x6张)
    /// </summary>
    public static Vector2[] CalculateRightPlayerBackPositions(int cardCount)
    {
        var positions = new Vector2[cardCount];
        int cardsPerRow = 6;
        int rowSpacing = 65;
        int colSpacing = 15;

        for (int i = 0; i < cardCount; i++)
        {
            int row = i / cardsPerRow;
            int col = i % cardsPerRow;
            positions[i] = new Vector2(
                RightPlayerCardX + col * colSpacing,
                RightPlayerCardStartY + row * rowSpacing
            );
        }
        return positions;
    }

    /// <summary>
    /// 计算底牌3张位置(顶部居中)
    /// </summary>
    public static Vector2[] CalculateTableCardPositions()
    {
        return new Vector2[]
        {
            new Vector2(TableCardStartX, TableCardY),
            new Vector2(TableCardStartX + TableCardSpacing, TableCardY),
            new Vector2(TableCardStartX + TableCardSpacing * 2, TableCardY)
        };
    }

    /// <summary>
    /// 计算出牌区域位置(屏幕中央居中)
    /// </summary>
    public static Vector2[] CalculatePlayedCardPositions(int cardCount)
    {
        var positions = new Vector2[cardCount];
        int totalWidth = cardCount * CardRenderer.OverlapSpacing + 120;
        int startX = WindowCenterX - totalWidth / 2;

        for (int i = 0; i < cardCount; i++)
        {
            positions[i] = new Vector2(startX + i * CardRenderer.OverlapSpacing, PlayedCardsY);
        }
        return positions;
    }

    /// <summary>
    /// 计算单机模式发牌起始动画位置(从右侧飞出)
    /// </summary>
    public static Vector2 GetDealStartPos()
    {
        return new Vector2(1110, 220);
    }

    /// <summary>
    /// 根据牌在手中的索引和总张数计算水平位置
    /// </summary>
    public static float CalculateCardX(int index, int totalCards)
    {
        int startX = WindowCenterX + (totalCards * CardRenderer.OverlapSpacing + 120) / 2 - 150;
        return startX - index * CardRenderer.OverlapSpacing;
    }
}
