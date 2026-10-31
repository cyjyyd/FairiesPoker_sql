using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FairiesPoker.MG.Core.Effects;

/// <summary>
/// Bounded, decorative animations. No gameplay callbacks, waits, network access or shared card-animation state.
/// GPU resources are created only by Draw; mapping and lifetime tests need no GraphicsDevice.
/// </summary>
public sealed class PlayEffectManager : IDisposable
{
    public const int MaxActiveEffects = 3;
    private readonly List<EffectInstance> _active = new();
    private Texture2D _pixel;
    private Texture2D _glow;
    private Texture2D _petal;
    private static readonly Color Gold = new(255, 216, 125);
    private static readonly Color Mint = new(142, 235, 204);
    private static readonly Color Ivory = new(255, 247, 215);

    public int ActiveCount => _active.Count;

    public void Trigger(PlayEffectKind kind, Vector2 origin)
    {
        if (kind == PlayEffectKind.None || !Enum.IsDefined(kind) ||
            !float.IsFinite(origin.X) || !float.IsFinite(origin.Y)) return;
        if (_active.Count == MaxActiveEffects) _active.RemoveAt(0);
        _active.Add(new EffectInstance(kind,
            new Vector2(MathHelper.Clamp(origin.X, 320, 960), MathHelper.Clamp(origin.Y, 250, 360))));
    }

    public void Update(float elapsedMs)
    {
        if (!float.IsFinite(elapsedMs) || elapsedMs <= 0) return;
        for (int i = _active.Count - 1; i >= 0; i--)
        {
            _active[i].AgeMs += elapsedMs;
            if (_active[i].AgeMs >= _active[i].DurationMs) _active.RemoveAt(i);
        }
    }

    public void Clear() => _active.Clear();

    public void Draw(SpriteBatch batch)
    {
        if (_active.Count == 0) return;
        EnsureTextures(batch.GraphicsDevice);
        foreach (var effect in _active)
        {
            float t = effect.AgeMs / effect.DurationMs;
            float opacity = Math.Min(1f, t * 9f) * MathHelper.Clamp((1 - t) * 5f, 0, 1);
            switch (effect.Kind)
            {
                case PlayEffectKind.Straight: DrawStraight(batch, effect, t, opacity); break;
                case PlayEffectKind.ConsecutivePairs: DrawPairs(batch, effect, t, opacity); break;
                case PlayEffectKind.Airplane: DrawAirplane(batch, effect, t, opacity); break;
                case PlayEffectKind.Bomb:
                case PlayEffectKind.JokerBomb: DrawBloom(batch, effect, t, opacity); break;
            }
            DrawTitle(batch, effect, opacity);
        }
    }

    private void DrawStraight(SpriteBatch batch, EffectInstance effect, float t, float opacity)
    {
        var center = effect.Origin;
        for (int i = 0; i < 28; i++)
        {
            float u = t * 1.35f - i * 0.014f;
            if (u < 0 || u > 1) continue;
            var p = center + new Vector2((u - 0.5f) * 480, MathF.Sin(u * MathF.PI * 2) * 26);
            float a = opacity * (1 - i / 30f);
            Glow(batch, p, 24, Gold, a * 0.28f);
            Spark(batch, p, 7 - i * 0.13f, Gold, a, u * 2);
        }
        for (int i = 0; i < 18; i++)
        {
            float u = i / 17f;
            var p = center + new Vector2((u - 0.5f) * 440,
                MathF.Sin(u * 6 + t * 5) * 22 + MathF.Sin(i * 7) * 22);
            Spark(batch, p, 2.5f, Mint, opacity * MathHelper.Clamp(t * 2 - u, 0, 1), i);
        }
    }

    private void DrawPairs(SpriteBatch batch, EffectInstance effect, float t, float opacity)
    {
        for (int pair = 0; pair < 5; pair++)
        {
            float phase = MathHelper.Clamp((t - pair * 0.075f) * 3, 0, 1);
            if (phase <= 0) continue;
            var center = effect.Origin + new Vector2((pair - 2) * 80, 10 - MathF.Sin(phase * MathF.PI) * 12);
            float a = opacity * Math.Min(1, phase * 4);
            Glow(batch, center, 50, Mint, a * 0.2f);
            for (int side = -1; side <= 1; side += 2)
            {
                var p = center + new Vector2(side * 14, -phase * 20);
                Petal(batch, p, side * (0.65f + phase * 0.4f), new Vector2(22, 10) * phase, Mint, a);
                Spark(batch, p + new Vector2(side * 8, -16), 5 * phase, Gold, a, phase);
            }
            Ring(batch, center, 28 * phase, Gold, a * (1 - phase) * 0.6f, 1.2f);
        }
    }

    private void DrawAirplane(SpriteBatch batch, EffectInstance effect, float t, float opacity)
    {
        float u = MathHelper.Clamp(t * 1.12f, 0, 1);
        var center = new Vector2(130 + 1020 * u, effect.Origin.Y - 22 - MathF.Sin(u * MathF.PI) * 65);
        for (int i = 1; i <= 26; i++)
        {
            float trailU = u - i * 0.013f;
            if (trailU < 0) continue;
            var p = new Vector2(130 + 1020 * trailU,
                effect.Origin.Y - 22 - MathF.Sin(trailU * MathF.PI) * 65 + MathF.Sin(i * 2 + t * 9) * 10);
            Spark(batch, p, 4 * (1 - i / 28f), Gold, opacity * (1 - i / 28f), i);
        }
        Glow(batch, center, 90, Mint, opacity * 0.18f);
        float flap = 0.75f + MathF.Sin(t * 36) * 0.2f;
        for (int side = -1; side <= 1; side += 2)
        {
            Petal(batch, center + new Vector2(side * 29, -12), side * 0.55f,
                new Vector2(46 * flap, 18), Mint, opacity * 0.85f);
            Petal(batch, center + new Vector2(side * 24, 15), -side * 0.55f,
                new Vector2(33 * flap, 13), Gold, opacity * 0.75f);
        }
        var rect = new Rectangle((int)center.X - 17, (int)center.Y - 25, 34, 50);
        batch.Draw(_pixel, rect, Ivory * opacity);
        Border(batch, rect, Gold * opacity);
        Spark(batch, center, 9, Gold, opacity, MathF.PI / 4);
    }

    private void DrawBloom(SpriteBatch batch, EffectInstance effect, float t, float opacity)
    {
        bool royal = effect.Kind == PlayEffectKind.JokerBomb;
        var center = effect.Origin;
        if (t < 0.24f)
        {
            float charge = t / 0.24f;
            Glow(batch, center, 45 + 35 * charge, Gold, opacity * 0.45f);
            for (int i = 0; i < 8; i++)
            {
                float angle = i * MathHelper.TwoPi / 8 + charge * 0.4f;
                Petal(batch, center + Unit(angle) * (30 * (1 - charge)), angle,
                    new Vector2(23, 9), Mint, opacity);
            }
            return;
        }
        float burst = (t - 0.24f) / 0.76f;
        float expansion = 1 - MathF.Pow(1 - burst, 3);
        Glow(batch, center, 140 * (1 - burst) + 35, Ivory, opacity * (1 - burst) * 0.6f);
        Ring(batch, center, 30 + expansion * (royal ? 190 : 145), Gold,
            opacity * (1 - burst) * 0.6f, 2);
        if (royal)
            Ring(batch, center, 15 + expansion * 130, Mint, opacity * (1 - burst) * 0.7f, 2);
        int count = royal ? 44 : 30;
        for (int i = 0; i < count; i++)
        {
            float angle = i * 2.399963f;
            float radius = (60 + Hash(i) * (royal ? 145 : 110)) * expansion;
            var p = center + Unit(angle) * radius + new Vector2(0, burst * burst * 38);
            Color color = i % 3 == 0 ? Mint : Gold;
            if (i % 2 == 0)
                Petal(batch, p, angle + burst * 2, new Vector2(12, 5) * (1 - burst * 0.6f), color, opacity * (1 - burst));
            else
                Spark(batch, p, 3 + Hash(i + 5) * 3, color, opacity * (1 - burst), angle + burst);
        }
        if (royal)
        {
            for (int i = 0; i < 5; i++)
            {
                float angle = -MathF.PI / 2 + (i - 2) * 0.4f;
                Spark(batch, center + Unit(angle) * (70 + expansion * 35), 9,
                    Ivory, opacity * (1 - burst), burst);
            }
        }
    }

    private void DrawTitle(SpriteBatch batch, EffectInstance effect, float opacity)
    {
        string text = GetTitle(effect.Kind);
        float scale = 1.35f;
        var size = FontManager.MeasureString(text, FontManager.Default, scale);
        var position = new Vector2(effect.Origin.X - size.X / 2, 158);
        FontManager.DrawString(batch, text, position + new Vector2(2, 3), new Color(12, 30, 28) * opacity, scale);
        FontManager.DrawString(batch, text, position, Gold * opacity, scale);
    }

    public static string GetTitle(PlayEffectKind kind) => kind switch
    {
        PlayEffectKind.Straight => "顺子",
        PlayEffectKind.ConsecutivePairs => "连对",
        PlayEffectKind.Airplane => "飞机",
        PlayEffectKind.Bomb => "炸弹",
        PlayEffectKind.JokerBomb => "王炸",
        _ => ""
    };

    private void Glow(SpriteBatch batch, Vector2 p, float radius, Color color, float opacity) =>
        batch.Draw(_glow, p, null, color * opacity, 0, new Vector2(32), radius / 32, SpriteEffects.None, 0);

    private void Petal(SpriteBatch batch, Vector2 p, float rotation, Vector2 halfSize, Color color, float opacity) =>
        batch.Draw(_petal, p, null, color * opacity, rotation, new Vector2(32, 16),
            new Vector2(halfSize.X / 32, halfSize.Y / 16), SpriteEffects.None, 0);

    private void Spark(SpriteBatch batch, Vector2 p, float radius, Color color, float opacity, float rotation)
    {
        Glow(batch, p, radius * 3, color, opacity * 0.4f);
        var a = Unit(rotation) * radius;
        var b = Unit(rotation + MathHelper.PiOver2) * radius;
        Line(batch, p - a, p + a, color * opacity, 1.4f);
        Line(batch, p - b, p + b, color * opacity, 1.4f);
    }

    private void Ring(SpriteBatch batch, Vector2 center, float radius, Color color, float opacity, float thickness)
    {
        for (int i = 0; i < 48; i++)
            Line(batch, center + Unit(i * MathHelper.TwoPi / 48) * radius,
                center + Unit((i + 1) * MathHelper.TwoPi / 48) * radius, color * opacity, thickness);
    }

    private void Line(SpriteBatch batch, Vector2 start, Vector2 end, Color color, float thickness)
    {
        var d = end - start;
        batch.Draw(_pixel, start, null, color, MathF.Atan2(d.Y, d.X), new Vector2(0, 0.5f),
            new Vector2(d.Length(), thickness), SpriteEffects.None, 0);
    }

    private void Border(SpriteBatch batch, Rectangle rect, Color color)
    {
        batch.Draw(_pixel, new Rectangle(rect.X, rect.Y, rect.Width, 1), color);
        batch.Draw(_pixel, new Rectangle(rect.X, rect.Bottom - 1, rect.Width, 1), color);
        batch.Draw(_pixel, new Rectangle(rect.X, rect.Y, 1, rect.Height), color);
        batch.Draw(_pixel, new Rectangle(rect.Right - 1, rect.Y, 1, rect.Height), color);
    }

    private static Vector2 Unit(float angle) => new(MathF.Cos(angle), MathF.Sin(angle));
    private static float Hash(int value) => (MathF.Sin(value * 127.1f + 31.7f) + 1) * 0.5f;

    private void EnsureTextures(GraphicsDevice device)
    {
        if (_pixel != null) return;
        _pixel = new Texture2D(device, 1, 1);
        _pixel.SetData(new[] { Color.White });
        _glow = new Texture2D(device, 64, 64);
        var glow = new Color[64 * 64];
        for (int y = 0; y < 64; y++)
        for (int x = 0; x < 64; x++)
        {
            float r = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(32)) / 32;
            glow[y * 64 + x] = Color.White * MathF.Pow(Math.Max(0, 1 - r), 2);
        }
        _glow.SetData(glow);
        _petal = new Texture2D(device, 64, 32);
        var petals = new Color[64 * 32];
        for (int y = 0; y < 32; y++)
        for (int x = 0; x < 64; x++)
        {
            float nx = (x - 31.5f) / 32, ny = (y - 15.5f) / 16;
            float alpha = MathHelper.Clamp((1 - nx * nx - ny * ny) * 5, 0, 1);
            petals[y * 64 + x] = Color.White * alpha;
        }
        _petal.SetData(petals);
    }

    public void Dispose()
    {
        Clear();
        _pixel?.Dispose(); _pixel = null;
        _glow?.Dispose(); _glow = null;
        _petal?.Dispose(); _petal = null;
    }

    private sealed class EffectInstance
    {
        public readonly PlayEffectKind Kind;
        public readonly Vector2 Origin;
        public readonly float DurationMs;
        public float AgeMs;

        public EffectInstance(PlayEffectKind kind, Vector2 origin)
        {
            Kind = kind; Origin = origin;
            DurationMs = kind switch
            {
                PlayEffectKind.Airplane => 1450,
                PlayEffectKind.JokerBomb => 1450,
                PlayEffectKind.Bomb => 1250,
                _ => 1100
            };
        }
    }
}
