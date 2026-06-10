using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpriteFontPlus;
using IOPath = System.IO.Path;

namespace FairiesPoker.MG.Core;

/// <summary>
/// SpriteFont字体管理器
/// </summary>
public static class FontManager
{
    private static readonly Dictionary<string, SpriteFont> _fonts = new();
    private static readonly Dictionary<string, Texture2D> _emojiTextures = new();
    private static readonly Dictionary<string, GlyphTexture> _fallbackGlyphTextures = new();
    private static readonly HashSet<char> _bakedCharacters = BuildInitialCharacterSet();
    private static GraphicsDevice? _graphicsDevice;
    private static string? _cachedFontPath;
    private static byte[]? _cachedFontBytes;
    private static float _currentFontSize;

    private const float BaseFontSize = 20f;
    private const float MaxFontSize = 32f;
    private const int AndroidAtlasSize = 1024;
    private const int DefaultAtlasSize = 2048;
    private const int LargeAtlasSize = 4096;

    private const string CommonChineseText =
        "斗地主单人模式多人模式联机模式设置退出返回登录注册用户名密码确认头像大厅房间创建加入快速匹配准备取消开始离开刷新在线玩家聊天发送系统消息" +
        "胜利失败结算继续地主农民叫地主抢地主不叫不抢出牌不出提示托管重新开始剩余底牌我的上家下家左边右边等待连接服务器端口断开错误成功失败" +
        "主题天波龙女芙蓉素问霹雳琳琅背景音乐音效音量窗口全屏无边框保存应用默认恢复返回主菜单请输入暂无加载中游戏结束分数积分倍数春天炸弹王炸顺子连对飞机三带一对" +
        "黑桃红桃梅花方块大小王一二三四五六七八九十零百千万上下左右中高低大小是否已未当前回合轮到玩家房主观战重连掉线自动通过拒绝审核列表测试" +
        "奇妙仙子失落宝藏拯救精灵大作战羽翼之谜海盗永无兽传奇世界私聊表情频道已断线等待加入未准备已准备房间列表在线用户创建房间加入房间离开房间" +
        "用户名密码确认密码修改密码连接状态服务器配置当前房间暂无房间请创建或快速匹配游戏即将开始正在结算出牌不符合规则不能不出当前回合必须出牌左玩家右玩家自己对手" +
        "关于产品名称版本版权公司说明图片欣赏点击左侧图片可以欣赏牌面关闭牌面预览版权所有保留权利经典复刻包含窗口" +
        "警告本计算机程序受著作权法和国际条约保护如未经授权而擅自复制或传播其中任何部分将受到严厉的民事及刑事制裁并将在法律许可范围内受到最大程度的起诉" +
        "账号旧新相同稍后重试换吧未知联系自定义上传下载解析格式不支持超限长度之间两次输入不一致正在今日历史全服频道目标时间戳分页条数默认推送私聊" +
        "大家好很高兴见到各位合作真是太愉快啦快点吧我等到花都谢了不要吵有什么专心玩吧走决战天亮再见想念" +
        "房间信息状态人数最大满移除候选最终实际获得仅剩清零封顶平分完整单张牌型双顺三顺三不带对儿权值花色方片合法非法" +
        "数据库初始化连接新增字段会话记录批量指定拒绝无效停止监听命令开启自动审核待审核列表通过找到离线下线配置文件创建默认错误" +
        "微软雅黑宋体黑体不可用不完整未安装丢失物理分辨率超过保存设置确认提示退出登录返回上一级" +
        "另幽残" +
        "与且个串临为举也争些交仍从他代们价份优估似但住你佳使供依便倒假偏做偶储充先光克免兜共具兼写冲况减函切删判别剧力助势区半占卡厂压原去参友反叠只台向吗员响唯善因圈场址垂域基堆填处外够央夹它守客容宽导射少尝就尽尾局层居展属工币布帧帮常幕广废延异弃引强归形往径循志忽思性总您悬意慢截才扑打扣执扩扫把抽拆拓拖拥择括拼拽按捕排控描插搜携操收放料旦早星映昵显普智替期杂来松板构枚果柄某染查栈校样根案桌检概橙此步殊毁每毫水求没沿洗活流浮淡深添渲源滑滚激烁焦然父特独环现生由电画略白盖盘直矩短础禁种秒空立第策签简类粘索累纯纳纹组细绑给络绪维综绿缀缓编缘缩网罩翻考者耗聚脑至般节若英荐虑行被覆视觉角触言订让访证评识询详读谁调豆象负责质资赛赢足跑距路跳践踪身转较辅辑达运还这进远迟迫追适逃透逐递造逻遍遮避采释里金钮链销锁键门闪阅阈队阶阻降随隐隔集需露静项顿颜额风首验黄鼠齐" +
        "绘制补偿虚拟坐标尺寸比例烘焙更变该";

    private const string CommonSymbolText = "\u00A0→▲▼●☹✌✓✗";

    /// <summary>
    /// 当前默认字体相对于1280x720设计字体的绘制补偿。
    /// 高分辨率下字体会重新烘焙得更大，绘制时用该比例保持虚拟坐标尺寸不变。
    /// </summary>
    public static float RenderScale => _currentFontSize > 0f ? BaseFontSize / _currentFontSize : 1f;

    public static void Initialize(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
        UpdateForDisplayScale(DisplayManager.Scale);
    }

    public static void Load(string key, SpriteFont font)
    {
        _fonts[key] = font;
    }

    public static SpriteFont? Get(string key)
    {
        return _fonts.GetValueOrDefault(key);
    }

    /// <summary>
    /// 默认字体
    /// </summary>
    public static SpriteFont Default
    {
        get
        {
            if (_fonts.TryGetValue("default", out var f))
                return f;

            if (_graphicsDevice != null)
                UpdateForDisplayScale(DisplayManager.Scale);

            if (_fonts.TryGetValue("default", out f))
                return f;

            throw new InvalidOperationException("Default font is not available.");
        }
    }

    public static void UpdateForDisplayScale(float displayScale)
    {
        if (_graphicsDevice == null) return;

        float targetSize = OperatingSystem.IsAndroid()
            ? BaseFontSize
            : Math.Clamp(BaseFontSize * Math.Max(1f, displayScale), BaseFontSize, MaxFontSize);
        if (Math.Abs(targetSize - _currentFontSize) < 0.5f && _fonts.ContainsKey("default"))
            return;

        if (TryBakeDefaultFont(targetSize, GetPreferredAtlasSizes()))
        {
            return;
        }
    }

    public static Vector2 MeasureString(string text, SpriteFont? font = null, float scale = 1f)
    {
        EnsureTextGlyphsBaked(text);
        var resolvedFont = font ?? Default;
        if (NeedsInlineTextureRendering(text))
            return MeasureStringWithInlineTextures(text, resolvedFont, scale);

        try
        {
            return resolvedFont.MeasureString(text) * RenderScale * scale;
        }
        catch (ArgumentException)
        {
            return resolvedFont.MeasureString(SanitizeForSpriteFont(text, resolvedFont)) * RenderScale * scale;
        }
    }

    public static void DrawString(SpriteBatch spriteBatch, string text, Vector2 position, Color color, float scale = 1f)
    {
        DrawString(spriteBatch, null, text, position, color, scale);
    }

    public static void DrawString(SpriteBatch spriteBatch, SpriteFont? font, string text, Vector2 position, Color color, float scale = 1f)
    {
        EnsureTextGlyphsBaked(text);
        var resolvedFont = font ?? Default;
        if (NeedsInlineTextureRendering(text))
        {
            DrawStringWithInlineTextures(spriteBatch, resolvedFont, text, position, color, scale);
            return;
        }

        try
        {
            spriteBatch.DrawString(resolvedFont, text, position, color, 0f, Vector2.Zero, scale * RenderScale, SpriteEffects.None, 0f);
        }
        catch (ArgumentException)
        {
            spriteBatch.DrawString(resolvedFont, SanitizeForSpriteFont(text, resolvedFont), position, color, 0f, Vector2.Zero, scale * RenderScale, SpriteEffects.None, 0f);
        }
    }

    public static void DrawString(SpriteBatch spriteBatch, SpriteFont? font, string text, Vector2 position, Color color,
        float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
    {
        EnsureTextGlyphsBaked(text);
        var resolvedFont = font ?? Default;
        var adjustedOrigin = RenderScale > 0f ? origin / RenderScale : origin;
        try
        {
            spriteBatch.DrawString(resolvedFont, text, position, color, rotation, adjustedOrigin, scale * RenderScale, effects, layerDepth);
        }
        catch (ArgumentException)
        {
            spriteBatch.DrawString(resolvedFont, SanitizeForSpriteFont(text, resolvedFont), position, color, rotation, adjustedOrigin, scale * RenderScale, effects, layerDepth);
        }
    }

    internal static bool ContainsEmoji(string text)
    {
        if (string.IsNullOrEmpty(text))
            return false;

        var enumerator = StringInfo.GetTextElementEnumerator(text);
        while (enumerator.MoveNext())
        {
            if (IsEmojiTextElement((string)enumerator.GetTextElement()))
                return true;
        }

        return false;
    }

    private static bool NeedsInlineTextureRendering(string text)
    {
        if (string.IsNullOrEmpty(text))
            return false;

        EnsureTextGlyphsBaked(text);

        var enumerator = StringInfo.GetTextElementEnumerator(text);
        while (enumerator.MoveNext())
        {
            if (NeedsTextureForTextElement((string)enumerator.GetTextElement()))
                return true;
        }

        return false;
    }

    private static bool NeedsTextureForTextElement(string textElement)
    {
        if (IsEmojiTextElement(textElement) || !IsBakedTextElement(textElement))
            return true;

        return textElement.Length == 1 &&
            _fonts.TryGetValue("default", out var font) &&
            !CanMeasure(font, textElement[0]);
    }

    private static void EnsureTextGlyphsBaked(string text)
    {
        if (_graphicsDevice == null || string.IsNullOrEmpty(text))
            return;

        if (OperatingSystem.IsAndroid())
            return;

        var addedCharacters = new List<char>();
        var enumerator = StringInfo.GetTextElementEnumerator(text);
        while (enumerator.MoveNext())
        {
            string element = (string)enumerator.GetTextElement();
            if (IsLineBreak(element) || IsEmojiTextElement(element) || element.Length != 1)
                continue;

            char c = element[0];
            if (char.IsControl(c) || IsVariationSelector(c) || IsKnownBakedCharacter(c))
                continue;

            if (_bakedCharacters.Add(c))
                addedCharacters.Add(c);
        }

        if (addedCharacters.Count == 0)
            return;

        float targetSize = _currentFontSize > 0f ? _currentFontSize : BaseFontSize;
        bool rebaked = TryBakeDefaultFont(targetSize, GetPreferredAtlasSizes());

        if (rebaked)
            return;

        foreach (char c in addedCharacters)
        {
            _bakedCharacters.Remove(c);
        }
    }

    private static bool IsBakedTextElement(string textElement)
    {
        if (IsLineBreak(textElement))
            return true;

        if (string.IsNullOrEmpty(textElement) || textElement.Length != 1)
            return false;

        return IsKnownBakedCharacter(textElement[0]);
    }

    private static Vector2 MeasureStringWithInlineTextures(string text, SpriteFont font, float scale)
    {
        float lineHeight = GetScaledLineHeight(font, scale);
        float x = 0f;
        float y = 0f;
        float maxX = 0f;
        var textRun = new StringBuilder();

        void FlushTextRun()
        {
            if (textRun.Length == 0)
                return;

            x += MeasureTextRun(font, textRun.ToString()).X * RenderScale * scale;
            textRun.Clear();
        }

        var enumerator = StringInfo.GetTextElementEnumerator(text);
        while (enumerator.MoveNext())
        {
            string element = (string)enumerator.GetTextElement();
            if (IsLineBreak(element))
            {
                FlushTextRun();
                maxX = Math.Max(maxX, x);
                x = 0f;
                y += lineHeight;
                continue;
            }

            if (NeedsTextureForTextElement(element))
            {
                FlushTextRun();
                x += MeasureInlineTextElement(font, element, scale);
                continue;
            }

            textRun.Append(element);
        }

        FlushTextRun();
        maxX = Math.Max(maxX, x);
        return new Vector2(maxX, y + lineHeight);
    }

    private static void DrawStringWithInlineTextures(SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 position, Color color, float scale)
    {
        float lineHeight = GetScaledLineHeight(font, scale);
        float inlineTextureSize = GetInlineTextureAdvance(font, scale);
        var cursor = position;
        var textRun = new StringBuilder();

        void FlushTextRun()
        {
            if (textRun.Length == 0)
                return;

            string run = textRun.ToString();
            DrawTextRun(spriteBatch, font, run, cursor, color, scale);
            cursor.X += MeasureTextRun(font, run).X * RenderScale * scale;
            textRun.Clear();
        }

        var enumerator = StringInfo.GetTextElementEnumerator(text);
        while (enumerator.MoveNext())
        {
            string element = (string)enumerator.GetTextElement();
            if (IsLineBreak(element))
            {
                FlushTextRun();
                cursor.X = position.X;
                cursor.Y += lineHeight;
                continue;
            }

            if (NeedsTextureForTextElement(element))
            {
                FlushTextRun();
                if (IsEmojiTextElement(element))
                {
                    var texture = GetEmojiTexture(element);
                    if (texture != null)
                    {
                        var rect = new Rectangle(
                            (int)Math.Round(cursor.X),
                            (int)Math.Round(cursor.Y + Math.Max(0f, (lineHeight - inlineTextureSize) / 2f)),
                            (int)Math.Ceiling(inlineTextureSize),
                            (int)Math.Ceiling(inlineTextureSize));
                        spriteBatch.Draw(texture, rect, color);
                        cursor.X += inlineTextureSize;
                    }
                    else
                    {
                        textRun.Append('?');
                    }
                }
                else
                {
                    var glyph = GetFallbackGlyphTexture(element);
                    if (glyph != null)
                    {
                        DrawTextRun(spriteBatch, glyph.Font, element, cursor, color, scale);
                        cursor.X += MeasureTextRun(glyph.Font, element).X * RenderScale * scale;
                    }
                    else
                    {
                        textRun.Append('?');
                    }
                }
                continue;
            }

            textRun.Append(element);
        }

        FlushTextRun();
    }

    private static void DrawTextRun(SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 position, Color color, float scale)
    {
        try
        {
            spriteBatch.DrawString(font, text, position, color, 0f, Vector2.Zero, scale * RenderScale, SpriteEffects.None, 0f);
        }
        catch (ArgumentException)
        {
            spriteBatch.DrawString(font, SanitizeForSpriteFont(text, font), position, color, 0f, Vector2.Zero, scale * RenderScale, SpriteEffects.None, 0f);
        }
    }

    private static Vector2 MeasureTextRun(SpriteFont font, string text)
    {
        try
        {
            return font.MeasureString(text);
        }
        catch (ArgumentException)
        {
            return font.MeasureString(SanitizeForSpriteFont(text, font));
        }
    }

    private static float GetScaledLineHeight(SpriteFont font, float scale)
    {
        float lineHeight = font.LineSpacing > 0 ? font.LineSpacing : font.MeasureString("M").Y;
        return Math.Max(1f, lineHeight * RenderScale * scale);
    }

    private static float GetInlineTextureAdvance(SpriteFont font, float scale)
    {
        return GetScaledLineHeight(font, scale);
    }

    private static float MeasureInlineTextElement(SpriteFont font, string textElement, float scale)
    {
        if (IsEmojiTextElement(textElement))
            return GetInlineTextureAdvance(font, scale);

        var glyph = GetFallbackGlyphTexture(textElement);
        if (glyph == null)
            return GetInlineTextureAdvance(font, scale);

        return MeasureTextRun(glyph.Font, textElement).X * RenderScale * scale;
    }

    private static bool IsLineBreak(string textElement)
    {
        return textElement == "\n" || textElement == "\r" || textElement == "\r\n";
    }

    private static bool IsEmojiTextElement(string textElement)
    {
        if (string.IsNullOrEmpty(textElement))
            return false;

        bool hasEmojiPresentation = textElement.Contains('\uFE0F') || textElement.Contains('\u200D');
        for (int i = 0; i < textElement.Length; i++)
        {
            int codePoint;
            char c = textElement[i];
            if (char.IsHighSurrogate(c))
            {
                if (i + 1 >= textElement.Length || !char.IsLowSurrogate(textElement[i + 1]))
                    continue;

                codePoint = char.ConvertToUtf32(c, textElement[++i]);
            }
            else if (char.IsLowSurrogate(c))
            {
                continue;
            }
            else
            {
                codePoint = c;
            }

            if (codePoint >= 0x1F000 && codePoint <= 0x1FAFF)
                return true;

            if (hasEmojiPresentation && codePoint >= 0x2600 && codePoint <= 0x27BF)
                return true;
        }

        return false;
    }

    public static Texture2D? GetEmojiTexture(string emoji)
    {
#if ANDROID
        if (_graphicsDevice == null || string.IsNullOrEmpty(emoji))
            return null;

        if (_emojiTextures.TryGetValue(emoji, out var cached))
            return cached;

        try
        {
            const int textureSize = 64;
            using var bitmap = global::Android.Graphics.Bitmap.CreateBitmap(
                textureSize,
                textureSize,
                global::Android.Graphics.Bitmap.Config.Argb8888);
            using var canvas = new global::Android.Graphics.Canvas(bitmap);
            canvas.DrawColor(global::Android.Graphics.Color.Transparent, global::Android.Graphics.PorterDuff.Mode.Clear);

            using var paint = new global::Android.Graphics.Paint(
                global::Android.Graphics.PaintFlags.AntiAlias |
                global::Android.Graphics.PaintFlags.FilterBitmap |
                global::Android.Graphics.PaintFlags.SubpixelText);
            paint.TextSize = 48f;
            paint.TextAlign = global::Android.Graphics.Paint.Align.Center;
            paint.Color = global::Android.Graphics.Color.White;
            paint.SetTypeface(global::Android.Graphics.Typeface.Default);

            var metrics = paint.GetFontMetrics();
            float baseline = textureSize / 2f - (metrics.Ascent + metrics.Descent) / 2f;
            canvas.DrawText(emoji, textureSize / 2f, baseline, paint);

            var pixels = new int[textureSize * textureSize];
            bitmap.GetPixels(pixels, 0, textureSize, 0, 0, textureSize, textureSize);

            var data = new Color[pixels.Length];
            bool hasVisiblePixel = false;
            for (int i = 0; i < pixels.Length; i++)
            {
                int argb = pixels[i];
                byte a = (byte)((argb >> 24) & 0xFF);
                byte r = (byte)((argb >> 16) & 0xFF);
                byte g = (byte)((argb >> 8) & 0xFF);
                byte b = (byte)(argb & 0xFF);
                if (a > 0)
                    hasVisiblePixel = true;
                data[i] = new Color(r, g, b, a);
            }

            if (!hasVisiblePixel)
                return null;

            var texture = new Texture2D(_graphicsDevice, textureSize, textureSize);
            texture.SetData(data);
            _emojiTextures[emoji] = texture;
            return texture;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to render emoji '{emoji}': {ex.Message}");
        }
#endif
        return null;
    }

    private static GlyphTexture? GetFallbackGlyphTexture(string textElement)
    {
        if (_graphicsDevice == null || string.IsNullOrEmpty(textElement) || textElement.Length != 1)
            return null;

        char c = textElement[0];
        if (char.IsControl(c) || IsVariationSelector(c))
            return null;

        string key = c.ToString();
        if (_fallbackGlyphTextures.TryGetValue(key, out var cached))
            return cached;

        if (!TryGetFontBytes(out var fontBytes))
            return null;

        try
        {
            float size = _currentFontSize > 0f ? _currentFontSize : BaseFontSize;
            var result = TtfFontBaker.Bake(
                fontBytes,
                size,
                128,
                128,
                new[] { new CharacterRange(c, c) });

            var font = result.CreateSpriteFont(_graphicsDevice);
            if (font.MeasureString(key).X <= 0)
                return null;

            var glyph = new GlyphTexture(font);
            _fallbackGlyphTextures[key] = glyph;
            return glyph;
        }
        catch
        {
            return null;
        }
    }

    private static string SanitizeForSpriteFont(string text, SpriteFont? font = null)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var sb = new StringBuilder(text.Length);
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (char.IsHighSurrogate(c))
            {
                sb.Append('?');
                if (i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
                    i++;
                continue;
            }

            if (char.IsLowSurrogate(c) || IsVariationSelector(c))
                continue;

            sb.Append(IsKnownBakedCharacter(c) && CanMeasure(font, c) ? c : '?');
        }

        return sb.ToString();
    }

    private static bool CanMeasure(SpriteFont? font, char c)
    {
        if (font == null || c == '\n' || c == '\r')
            return true;

        try
        {
            font.MeasureString(c.ToString());
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    private static bool IsKnownBakedCharacter(char c)
    {
        return c == '\n' || c == '\r' || _bakedCharacters.Contains(c);
    }

    private static bool IsVariationSelector(char c)
    {
        return c >= 0xFE00 && c <= 0xFE0F;
    }

    private static HashSet<char> BuildInitialCharacterSet()
    {
        var chars = new HashSet<char>();
        AddCharacterRange(chars, 0x20, 0x7E);
        AddCharacterRange(chars, 0x2000, 0x206F);
        AddCharacterRange(chars, 0x3000, 0x303F);
        AddCharacterRange(chars, 0xFF00, 0xFFEF);

        foreach (char c in CommonChineseText)
        {
            if (!char.IsControl(c))
                chars.Add(c);
        }

        foreach (char c in CommonSymbolText)
        {
            if (!char.IsControl(c))
                chars.Add(c);
        }

        return chars;
    }

    private static void AddCharacterRange(HashSet<char> chars, int start, int end)
    {
        for (int codePoint = start; codePoint <= end; codePoint++)
        {
            chars.Add((char)codePoint);
        }
    }

    private static CharacterRange[] BuildCharacterRanges(IEnumerable<char> chars)
    {
        var ordered = chars
            .Where(c => !char.IsControl(c))
            .Distinct()
            .OrderBy(c => c)
            .ToArray();

        if (ordered.Length == 0)
            return Array.Empty<CharacterRange>();

        var ranges = new List<CharacterRange>();
        char rangeStart = ordered[0];
        char previous = ordered[0];

        for (int i = 1; i < ordered.Length; i++)
        {
            char current = ordered[i];
            if (current == previous + 1)
            {
                previous = current;
                continue;
            }

            ranges.Add(new CharacterRange(rangeStart, previous));
            rangeStart = current;
            previous = current;
        }

        ranges.Add(new CharacterRange(rangeStart, previous));
        return ranges.ToArray();
    }

    public static void Clear()
    {
        foreach (var font in _fonts.Values)
        {
            font.Texture.Dispose();
        }
        _fonts.Clear();

        foreach (var texture in _emojiTextures.Values)
        {
            texture.Dispose();
        }
        _emojiTextures.Clear();

        foreach (var glyph in _fallbackGlyphTextures.Values)
        {
            glyph.Texture.Dispose();
        }
        _fallbackGlyphTextures.Clear();

        _currentFontSize = 0f;
    }

    private sealed class GlyphTexture
    {
        public GlyphTexture(SpriteFont font)
        {
            Font = font;
        }

        public SpriteFont Font { get; }
        public Texture2D Texture => Font.Texture;
    }

    private static int[] GetPreferredAtlasSizes()
    {
        return OperatingSystem.IsAndroid()
            ? new[] { DefaultAtlasSize, LargeAtlasSize }
            : new[] { DefaultAtlasSize, LargeAtlasSize };
    }

    private static bool TryBakeDefaultFont(float size, IEnumerable<int> atlasSizes)
    {
        foreach (int atlasSize in atlasSizes)
        {
            if (TryBakeDefaultFont(size, atlasSize))
                return true;
        }

        if (Math.Abs(size - BaseFontSize) >= 0.5f)
        {
            foreach (int atlasSize in atlasSizes)
            {
                if (TryBakeDefaultFont(BaseFontSize, atlasSize))
                    return true;
            }
        }

        return false;
    }

    private static bool TryBakeDefaultFont(float size, int atlasSize)
    {
        if (_graphicsDevice == null) return false;

        foreach (var ttfPath in GetCandidateFontFiles())
        {
            if (!TryReadFontBytes(ttfPath, out var fontBytes)) continue;

            try
            {
                var result = TtfFontBaker.Bake(
                    fontBytes,
                    size,
                    atlasSize,
                    atlasSize,
                    BuildCharacterRanges(_bakedCharacters));

                var spriteFont = result.CreateSpriteFont(_graphicsDevice);
                if (spriteFont.MeasureString("测试").X <= 10) continue;

                if (_fonts.TryGetValue("default", out var oldFont))
                {
                    oldFont.Texture.Dispose();
                }

                _fonts["default"] = spriteFont;
                _currentFontSize = size;
                return true;
            }
            catch
            {
                // Try the next font or atlas size.
            }
        }

        return false;
    }

    private static bool TryGetFontBytes(out byte[] fontBytes)
    {
        foreach (string path in GetCandidateFontFiles())
        {
            if (TryReadFontBytes(path, out fontBytes))
                return true;
        }

        fontBytes = Array.Empty<byte>();
        return false;
    }

    private static bool TryReadFontBytes(string path, out byte[] fontBytes)
    {
        if (string.Equals(_cachedFontPath, path, StringComparison.OrdinalIgnoreCase) && _cachedFontBytes != null)
        {
            fontBytes = _cachedFontBytes;
            return true;
        }

        fontBytes = Array.Empty<byte>();

        try
        {
            if (!File.Exists(path))
            {
#if ANDROID
                if (TryReadAndroidAssetFont(path, out fontBytes))
                {
                    _cachedFontPath = path;
                    _cachedFontBytes = fontBytes;
                    return true;
                }
#endif
                return false;
            }

            _cachedFontPath = path;
            _cachedFontBytes = File.ReadAllBytes(path);
            fontBytes = _cachedFontBytes;
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to read font '{path}': {ex.Message}");
            _cachedFontPath = null;
            _cachedFontBytes = null;
            return false;
        }
    }

#if ANDROID
    private static bool TryReadAndroidAssetFont(string path, out byte[] fontBytes)
    {
        fontBytes = Array.Empty<byte>();
        string assetPath = GetAndroidFontAssetPath(path);
        if (string.IsNullOrWhiteSpace(assetPath))
            return false;

        try
        {
            using Stream input = global::Android.App.Application.Context.Assets.Open(assetPath);
            using var memory = new MemoryStream();
            input.CopyTo(memory);
            fontBytes = memory.ToArray();
            return fontBytes.Length > 0;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to read Android font asset '{assetPath}': {ex.Message}");
            return false;
        }
    }

    private static string GetAndroidFontAssetPath(string path)
    {
        string normalizedPath = path.Replace('\\', '/');
        int fontsIndex = normalizedPath.LastIndexOf("/Fonts/", StringComparison.OrdinalIgnoreCase);
        if (fontsIndex >= 0)
            return normalizedPath[(fontsIndex + 1)..];

        if (normalizedPath.StartsWith("Fonts/", StringComparison.OrdinalIgnoreCase))
            return normalizedPath;

        string fileName = IOPath.GetFileName(normalizedPath);
        if (string.Equals(fileName, "NotoSansCJK-Regular.ttc", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(fileName, "HarmonyOS_Sans_SC_Regular.ttf", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(fileName, "NotoSansCJKsc-Regular.otf", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(fileName, "NotoSansSC-Regular.otf", StringComparison.OrdinalIgnoreCase))
        {
            return "Fonts/" + fileName;
        }

        return string.Empty;
    }
#endif

    private static string[] GetCandidateFontFiles()
    {
        return new[]
        {
            IOPath.Combine(ConfigManager.ResourceBaseDirectory, "Fonts", "HarmonyOS_Sans_SC_Regular.ttf"),
            IOPath.Combine(ConfigManager.ResourceBaseDirectory, "Fonts", "NotoSansCJK-Regular.ttc"),
            IOPath.Combine(ConfigManager.ResourceBaseDirectory, "Fonts", "NotoSansCJKsc-Regular.otf"),
            IOPath.Combine(ConfigManager.ResourceBaseDirectory, "Fonts", "NotoSansSC-Regular.otf"),
            "/System/Library/Fonts/PingFang.ttc",
            "/System/Library/Fonts/STHeiti Light.ttc",
            "/System/Library/Fonts/Supplemental/Songti.ttc",
            "/System/Library/Fonts/Supplemental/Arial Unicode.ttf",
            "/usr/share/fonts/opentype/noto/NotoSansCJK-Regular.ttc",
            "/usr/share/fonts/truetype/noto/NotoSansCJK-Regular.ttc",
            "/usr/share/fonts/opentype/noto/NotoSansCJK-Regular.otf",
            "/usr/share/fonts/truetype/wqy/wqy-microhei.ttc",
            "/usr/share/fonts/truetype/wqy/wqy-zenhei.ttc",
            "C:\\WINDOWS\\Fonts\\msyh.ttc",
            "C:\\WINDOWS\\Fonts\\STKAITI.TTF",
            "C:\\WINDOWS\\Fonts\\simhei.ttf",
            "C:\\WINDOWS\\Fonts\\STXIHEI.TTF",
            "C:\\WINDOWS\\Fonts\\SIMYOU.TTF",
        };
    }
}
