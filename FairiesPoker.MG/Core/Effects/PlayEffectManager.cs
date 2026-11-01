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
        // A travelling wave of paired dust vortices. Every visible mark is a dust grain.
        for (int pair = 0; pair < 5; pair++)
        {
            float phase = (t - pair * 0.075f) / 0.7f;
            if (phase <= 0) continue;
            float gather = MathHelper.SmoothStep(0, 1, MathHelper.Clamp(phase * 4, 0, 1));
            float release = MathHelper.Clamp((phase - 0.62f) / 0.5f, 0, 1);
            var center = effect.Origin + new Vector2((pair - 2) * 88, -15 - MathF.Sin(phase * MathF.PI) * 8);
            float alpha = opacity * gather * (1 - release);

            for (int side = -1; side <= 1; side += 2)
            {
                var orbitCenter = center + new Vector2(side * 17, 0);
                Glow(batch, orbitCenter, 34, Gold, alpha * 0.08f);
                for (int grain = 0; grain < 24; grain++)
                {
                    int seed = pair * 73 + grain * 3 + side + 2;
                    float angle = grain * MathHelper.TwoPi / 24 + phase * side * 2.4f;
                    float radius = (15 + Hash(seed) * 6) * gather + release * 15;
                    var jitter = new Vector2(MathF.Sin(seed * 3.1f + t * 9), MathF.Cos(seed * 1.7f - t * 7)) * 2;
                    var position = orbitCenter + Unit(angle) * radius + jitter +
                        new Vector2(0, release * release * (12 + Hash(seed + 2) * 18));
                    float shimmer = 0.55f + 0.45f * MathF.Sin(seed + t * 14) * MathF.Sin(seed + t * 14);
                    Dust(batch, position, 1 + Hash(seed + 5) * 0.9f, alpha * shimmer, seed);
                }
            }
        }

        // Sparse dust threads connect the pairs as the wave advances.
        for (int grain = 0; grain < 36; grain++)
        {
            float u = grain / 35f;
            float visible = MathHelper.Clamp((t - u * 0.3f) * 5, 0, 1);
            var position = effect.Origin + new Vector2((u - 0.5f) * 390,
                13 + MathF.Sin(u * MathHelper.TwoPi * 2 - t * 6) * 8);
            Dust(batch, position, 0.8f + Hash(grain) * 0.5f, opacity * visible * 0.5f, grain + 500);
        }
    }

    private void DrawAirplane(SpriteBatch batch, EffectInstance effect, float t, float opacity)
    {
        float u = MathHelper.Clamp(t * 1.12f, 0, 1);
        var center = DustFlightPosition(effect.Origin, u);

        // A broad, gently dispersing comet trail, rather than a solid flying card.
        for (int grain = 0; grain < 132; grain++)
        {
            float age = grain / 131f;
            float trailU = u - age * 0.42f;
            if (trailU < 0) continue;
            float spread = 3 + age * 28;
            var drift = new Vector2((Hash(grain + 13) - 0.5f) * spread,
                MathF.Sin(grain * 1.9f - t * 12) * (3 + age * 6) +
                (Hash(grain + 47) - 0.5f) * spread + age * age * 12);
            var position = DustFlightPosition(effect.Origin, trailU) + drift;
            float alpha = opacity * MathF.Pow(1 - age, 1.3f) * (0.4f + Hash(grain + 2) * 0.6f);
            Dust(batch, position, 0.8f + Hash(grain + 5) * 1.1f, alpha, grain + 700);
        }

        // Four wing-shaped dust orbits shimmer around the moving concentration of light.
        // There are no opaque wings, petals, card rectangles or cool-coloured particles.
        float flap = 0.85f + MathF.Sin(t * 28) * 0.13f;
        for (int side = -1; side <= 1; side += 2)
        for (int lobe = 0; lobe < 2; lobe++)
        for (int grain = 0; grain < 24; grain++)
        {
            int seed = grain + lobe * 53 + (side + 1) * 61;
            float angle = grain * MathHelper.TwoPi / 24;
            float reach = (1 - MathF.Cos(angle)) * (lobe == 0 ? 27 : 19) * flap;
            float height = MathF.Sin(angle) * (lobe == 0 ? 22 : 14);
            var position = center + new Vector2(side * reach,
                (lobe == 0 ? -7 : 8) + height + reach * (lobe == 0 ? -0.15f : 0.15f));
            position += new Vector2(MathF.Sin(seed + t * 11), MathF.Cos(seed * 3 - t * 9)) * 1.7f;
            float shimmer = 0.55f + Hash(seed + 17) * 0.45f;
            Dust(batch, position, 1.1f + Hash(seed + 19) * 0.8f, opacity * shimmer, seed + 1000);
        }

        Glow(batch, center, 35, Gold, opacity * 0.16f);
        for (int grain = 0; grain < 24; grain++)
        {
            float angle = grain * 2.399963f + t * 5;
            float radius = 3 + Hash(grain + 3) * 13;
            var position = center + new Vector2(MathF.Cos(angle) * radius * 0.55f, MathF.Sin(angle) * radius);
            Dust(batch, position, 1 + Hash(grain + 7), opacity, grain + 1300);
        }
    }

    private static Vector2 DustFlightPosition(Vector2 origin, float progress) =>
        new(130 + 1020 * progress, origin.Y - 24 - MathF.Sin(progress * MathF.PI) * 45);

    private void Dust(SpriteBatch batch, Vector2 position, float radius, float opacity, int seed)
    {
        // Amber, gold and champagne are all warm variations of the same fairy dust.
        var color = Color.Lerp(new Color(255, 179, 65), new Color(255, 237, 168), Hash(seed + 31));
        Glow(batch, position, radius * 4, color, opacity * 0.2f);
        Glow(batch, position, radius * 1.6f, color, opacity);
        if (seed % 23 == 0)
            Spark(batch, position, radius * 2.1f, color, opacity * 0.75f, seed);
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
