using FairiesPoker.MG.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FairiesPoker.MG.Renderers;

/// <summary>
/// 底牌渲染器(3张底牌显示在顶部)
/// </summary>
public class TableCardRenderer
{
    private Vector2[] _positions = CardLayoutManager.CalculateTableCardPositions();
    private string[]? _huases;
    private int[]? _sizes;
    private bool _revealed;

    public void SetCards(string[] huases, int[] sizes)
    {
        _huases = huases;
        _sizes = sizes;
    }

    public void Reveal() => _revealed = true;
    public void Hide() => _revealed = false;

    public void Draw(SpriteBatch sb)
    {
        if (_huases == null || _sizes == null) return;

        for (int i = 0; i < 3 && i < _huases.Length; i++)
        {
            if (_revealed)
                CardRenderer.DrawCard(sb, _positions[i], _huases[i], _sizes[i]);
            else
                CardRenderer.DrawCardBack(sb, _positions[i]);
        }
    }
}
