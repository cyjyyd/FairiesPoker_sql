using FairiesPoker;
using Protocol.Constant;

namespace FairiesPoker.MG.Core.Effects;

public enum PlayEffectKind
{
    None,
    Straight,
    ConsecutivePairs,
    Airplane,
    Bomb,
    JokerBomb
}

/// <summary>Offline Guize values and wire CardType values are different numbering systems.</summary>
public static class PlayEffectMapping
{
    public static PlayEffectKind FromOffline(Guize type) => type switch
    {
        Guize.顺子 => PlayEffectKind.Straight,
        Guize.连对 => PlayEffectKind.ConsecutivePairs,
        Guize.炸弹 => PlayEffectKind.Bomb,
        Guize.天炸 => PlayEffectKind.JokerBomb,
        Guize.飞机不带 or Guize.三飞机不带 or Guize.四飞机不带 or
        Guize.五飞机不带 or Guize.六飞机不带 or Guize.飞机带二 or
        Guize.飞机带二对 or Guize.三飞机带三 or Guize.四飞机带四 or
        Guize.三飞机带三对 or Guize.四飞机带四对 or Guize.五飞机带五 => PlayEffectKind.Airplane,
        _ => PlayEffectKind.None
    };

    public static PlayEffectKind FromOnline(int type) => type switch
    {
        CardType.STRAIGHT => PlayEffectKind.Straight,
        CardType.DOUBLE_STRAIGHT => PlayEffectKind.ConsecutivePairs,
        CardType.TRIPLE_STRAIGHT => PlayEffectKind.Airplane,
        CardType.BOOM => PlayEffectKind.Bomb,
        CardType.JOKER_BOOM => PlayEffectKind.JokerBomb,
        _ => PlayEffectKind.None
    };
}
