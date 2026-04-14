using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FairiesPoker.MG.Renderers;

/// <summary>
/// 已出牌区域渲染器(屏幕中央显示当前玩家打出的牌)
/// </summary>
public class DealtCardRenderer
{
    private string[]? _huases;
    private int[]? _sizes;

    public void SetCards(string[] huases, int[] sizes)
    {
        _huases = huases;
        _sizes = sizes;
    }

    public void Clear()
    {
        _huases = null;
        _sizes = null;
    }

    public bool HasCards => _huases != null && _huases.Length > 0;

    public void Draw(SpriteBatch sb)
    {
        if (_huases == null || _sizes == null) return;

        int count = _huases.Length;
        var positions = CardLayoutManager.CalculatePlayedCardPositions(count);

        for (int i = 0; i < count; i++)
        {
            CardRenderer.DrawCard(sb, positions[i], _huases[i], _sizes[i]);
        }
    }
}
