using Protocol.Constant;
using Protocol.Dto.Fight;

namespace FPServer.Game
{
    /// <summary>
    /// 服务端托管出牌。策略沿用单机AI的核心思路：小牌优先、能接则用最小代价接牌。
    /// </summary>
    internal static class ServerAiPlayer
    {
        public static DealDto ChooseDeal(GameState gameState, int userId)
        {
            var hand = SortCards(gameState.GetPlayerCards(userId));
            if (hand.Count == 0)
                return null;

            var lastDeal = gameState.LastDeal;
            if (lastDeal == null || gameState.LastDealUserId == userId)
            {
                return CreateDeal(ChooseLeadCards(hand), userId);
            }

            foreach (var candidate in BuildResponseCandidates(hand, lastDeal))
            {
                var deal = CreateDeal(candidate, userId);
                if (deal != null)
                    return deal;
            }

            return null;
        }

        private static DealDto CreateDeal(List<CardDto> cards, int userId)
        {
            if (cards == null || cards.Count == 0)
                return null;

            cards = SortCards(cards);
            var type = CardType.GetCardType(cards);
            if (type == CardType.NONE)
                return null;

            return new DealDto(cards, userId);
        }

        private static List<CardDto> ChooseLeadCards(List<CardDto> hand)
        {
            if (CardType.GetCardType(hand) != CardType.NONE)
                return hand;

            var combo = FindSmallestCombo(hand);
            return combo ?? new List<CardDto> { hand[0] };
        }

        private static List<CardDto> FindSmallestCombo(List<CardDto> hand)
        {
            var straight = FindSequences(hand, 5, 1).FirstOrDefault();
            if (straight != null)
                return straight;

            var pair = GetGroups(hand, 2).FirstOrDefault();
            if (pair != null)
                return pair.Take(2).ToList();

            var triple = GetGroups(hand, 3).FirstOrDefault();
            if (triple != null)
                return triple.Take(3).ToList();

            return null;
        }

        private static IEnumerable<List<CardDto>> BuildResponseCandidates(List<CardDto> hand, DealDto lastDeal)
        {
            foreach (var candidate in BuildSameTypeCandidates(hand, lastDeal))
            {
                yield return candidate;
            }

            if (lastDeal.Type != CardType.BOOM && lastDeal.Type != CardType.JOKER_BOOM)
            {
                foreach (var bomb in GetGroups(hand, 4))
                {
                    yield return bomb.Take(4).ToList();
                }
            }
            else if (lastDeal.Type == CardType.BOOM)
            {
                foreach (var bomb in GetGroups(hand, 4))
                {
                    var cards = bomb.Take(4).ToList();
                    if (CardWeight.GetWeight(cards, CardType.BOOM) > lastDeal.Weight)
                        yield return cards;
                }
            }

            var jokerBoom = TryBuildJokerBoom(hand);
            if (jokerBoom != null && lastDeal.Type != CardType.JOKER_BOOM)
                yield return jokerBoom;
        }

        private static IEnumerable<List<CardDto>> BuildSameTypeCandidates(List<CardDto> hand, DealDto lastDeal)
        {
            switch (lastDeal.Type)
            {
                case CardType.SINGLE:
                    foreach (var card in hand.Where(c => c.Weight > lastDeal.SelectCardList[0].Weight))
                        yield return new List<CardDto> { card };
                    break;

                case CardType.DOUBLE:
                    foreach (var group in GetGroups(hand, 2))
                    {
                        var cards = group.Take(2).ToList();
                        if (CardWeight.GetWeight(cards, CardType.DOUBLE) > lastDeal.Weight)
                            yield return cards;
                    }
                    break;

                case CardType.THREE:
                    foreach (var group in GetGroups(hand, 3))
                    {
                        var cards = group.Take(3).ToList();
                        if (CardWeight.GetWeight(cards, CardType.THREE) > lastDeal.Weight)
                            yield return cards;
                    }
                    break;

                case CardType.THREE_ONE:
                    foreach (var group in GetGroups(hand, 3))
                    {
                        var cards = group.Take(3).ToList();
                        if (CardWeight.GetWeight(cards, CardType.THREE_ONE) <= lastDeal.Weight)
                            continue;

                        var kicker = hand.FirstOrDefault(c => c.Weight != group.Key);
                        if (kicker != null)
                        {
                            cards.Add(kicker);
                            yield return cards;
                        }
                    }
                    break;

                case CardType.THREE_TWO:
                    foreach (var group in GetGroups(hand, 3))
                    {
                        var cards = group.Take(3).ToList();
                        if (CardWeight.GetWeight(cards, CardType.THREE_TWO) <= lastDeal.Weight)
                            continue;

                        var pair = GetGroups(hand.Where(c => c.Weight != group.Key), 2).FirstOrDefault();
                        if (pair != null)
                        {
                            cards.AddRange(pair.Take(2));
                            yield return cards;
                        }
                    }
                    break;

                case CardType.STRAIGHT:
                    foreach (var sequence in FindSequences(hand, lastDeal.SelectCardList.Count, 1))
                    {
                        if (CardWeight.GetWeight(sequence, CardType.STRAIGHT) > lastDeal.Weight)
                            yield return sequence;
                    }
                    break;

                case CardType.DOUBLE_STRAIGHT:
                    foreach (var sequence in FindSequences(hand, lastDeal.SelectCardList.Count / 2, 2))
                    {
                        if (CardWeight.GetWeight(sequence, CardType.DOUBLE_STRAIGHT) > lastDeal.Weight)
                            yield return sequence;
                    }
                    break;

                case CardType.TRIPLE_STRAIGHT:
                    foreach (var sequence in FindSequences(hand, lastDeal.SelectCardList.Count / 3, 3))
                    {
                        if (CardWeight.GetWeight(sequence, CardType.TRIPLE_STRAIGHT) > lastDeal.Weight)
                            yield return sequence;
                    }
                    break;
            }
        }

        private static IEnumerable<IGrouping<int, CardDto>> GetGroups(IEnumerable<CardDto> cards, int count)
        {
            return cards
                .GroupBy(c => c.Weight)
                .Where(g => g.Count() >= count)
                .OrderBy(g => g.Key);
        }

        private static IEnumerable<List<CardDto>> FindSequences(List<CardDto> hand, int length, int cardsPerWeight)
        {
            if (length <= 0)
                yield break;

            var groups = hand
                .Where(c => c.Weight <= CardWeight.ONE)
                .GroupBy(c => c.Weight)
                .Where(g => g.Count() >= cardsPerWeight)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.Take(cardsPerWeight).ToList());

            var weights = groups.Keys.OrderBy(w => w).ToList();
            for (int i = 0; i <= weights.Count - length; i++)
            {
                bool consecutive = true;
                for (int j = 1; j < length; j++)
                {
                    if (weights[i + j] != weights[i] + j)
                    {
                        consecutive = false;
                        break;
                    }
                }

                if (!consecutive)
                    continue;

                var result = new List<CardDto>();
                for (int j = 0; j < length; j++)
                    result.AddRange(groups[weights[i + j]]);

                yield return result;
            }
        }

        private static List<CardDto> TryBuildJokerBoom(List<CardDto> hand)
        {
            var small = hand.FirstOrDefault(c => c.Weight == CardWeight.SJOKER);
            var large = hand.FirstOrDefault(c => c.Weight == CardWeight.LJOKER);
            return small != null && large != null
                ? new List<CardDto> { small, large }
                : null;
        }

        private static List<CardDto> SortCards(IEnumerable<CardDto> cards)
        {
            return cards
                .OrderBy(c => c.Weight)
                .ThenBy(c => c.Color)
                .ToList();
        }
    }
}
