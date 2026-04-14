using FairiesPoker.MG.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.IO;

namespace FairiesPoker.MG.Renderers;

/// <summary>
/// 卡牌Sprite渲染器 - 替代WinForms的PictureBox方案
/// </summary>
public class CardRenderer
{
    // 卡牌标准尺寸
    public const int CardWidth = 120;
    public const int CardHeight = 180;
    public const int OverlapSpacing = 30;  // 卡牌重叠间距
    public const int SelectedOffset = 30;  // 选中时上移距离

    private static Texture2D? _cardBack;

    /// <summary>
    /// 加载牌背纹理
    /// </summary>
    public static void LoadCardBack(string backImagePath)
    {
        if (File.Exists(backImagePath))
            _cardBack = TextureManager.Load("_cardback", backImagePath);
    }

    /// <summary>
    /// 获取卡牌纹理(按花色+大小)
    /// </summary>
    public static Texture2D GetCardTexture(string huase, int size)
    {
        string key = $"card_{huase}_{size}";
        if (TextureManager.Get(key) is Texture2D cached) return cached;

        // 花色前缀映射
        string fileName = huase switch
        {
            "heitao" => $"heitao{size}",
            "hongtao" => $"hongtao{size}",
            "meihua" => $"meihua{size}",
            "fangkuai" => $"fangkuai{size}",
            "" => $"{size}",  // 大小王
            _ => $"{huase}{size}"
        };

        string path = System.IO.Path.Combine(ConfigManager.CardImagePath, fileName + ".png");
        return TextureManager.Load(key, path);
    }

    /// <summary>
    /// 获取牌背纹理
    /// </summary>
    public static Texture2D? CardBack => _cardBack;

    /// <summary>
    /// 绘制单张牌
    /// </summary>
    public static void DrawCard(SpriteBatch sb, Vector2 position, string huase, int size, bool selected = false)
    {
        var texture = GetCardTexture(huase, size);
        float yOffset = selected ? -SelectedOffset : 0;
        var destRect = new Rectangle((int)position.X, (int)position.Y + (int)yOffset, CardWidth, CardHeight);
        sb.Draw(texture, destRect, Color.White);
    }

    /// <summary>
    /// 绘制牌背
    /// </summary>
    public static void DrawCardBack(SpriteBatch sb, Vector2 position)
    {
        if (_cardBack == null) return;
        var destRect = new Rectangle((int)position.X, (int)position.Y, CardWidth, CardHeight);
        sb.Draw(_cardBack, destRect, Color.White);
    }

    /// <summary>
    /// 预加载主题的所有卡牌纹理(54张)
    /// </summary>
    public static void PreloadCards()
    {
        string[] suits = { "heitao", "hongtao", "meihua", "fangkuai" };
        // 预加载常用牌(3-A: size 3-15)
        for (int s = 3; s <= 15; s++)
        {
            foreach (var suit in suits)
            {
                GetCardTexture(suit, s);
            }
        }
        // 大小王 (16, 17)
        GetCardTexture("", 16);
        GetCardTexture("", 17);
    }
}
