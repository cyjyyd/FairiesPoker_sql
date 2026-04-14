using System;
using System.Collections;
using System.Collections.Generic;

namespace FairiesPoker
{
    /// <summary>
    /// 记牌器 - 记录已出的牌，推测对手手牌
    /// </summary>
    public class CardMemory
    {
        // 每种牌的总数量 (3-A: 3-14, 小王:16, 大王:17)
        private Dictionary<int, int> totalCards;

        // 已出的牌
        private Dictionary<int, int> playedCards;

        // 记录每个玩家出的牌
        private Dictionary<int, List<int[]>> playerPlayedCards;

        // 牌值名称映射
        private static readonly Dictionary<int, string> cardNames = new Dictionary<int, string>
        {
            {3, "3"}, {4, "4"}, {5, "5"}, {6, "6"}, {7, "7"}, {8, "8"}, {9, "9"},
            {10, "10"}, {11, "J"}, {12, "Q"}, {13, "K"}, {14, "A"}, {15, "2"},
            {16, "小王"}, {17, "大王"}
        };

        public CardMemory()
        {
            Initialize();
        }

        /// <summary>
        /// 初始化记牌器
        /// </summary>
        public void Initialize()
        {
            totalCards = new Dictionary<int, int>();
            playedCards = new Dictionary<int, int>();
            playerPlayedCards = new Dictionary<int, List<int[]>>();

            // 初始化每种牌的数量
            for (int i = 3; i <= 15; i++) // 3-2
            {
                totalCards[i] = 4;
                playedCards[i] = 0;
            }
            totalCards[16] = 1; // 小王
            totalCards[17] = 1; // 大王
            playedCards[16] = 0;
            playedCards[17] = 0;

            // 初始化玩家出牌记录
            for (int i = 1; i <= 3; i++)
            {
                playerPlayedCards[i] = new List<int[]>();
            }
        }

        /// <summary>
        /// 记录出牌
        /// </summary>
        public void RecordPlay(int playerId, int[] cards)
        {
            if (cards == null) return;

            foreach (int card in cards)
            {
                if (playedCards.ContainsKey(card))
                {
                    playedCards[card]++;
                }
            }

            if (playerPlayedCards.ContainsKey(playerId))
            {
                playerPlayedCards[playerId].Add(cards);
            }
        }

        /// <summary>
        /// 获取某张牌剩余数量
        /// </summary>
        public int GetRemainingCount(int cardValue)
        {
            if (totalCards.ContainsKey(cardValue))
            {
                return totalCards[cardValue] - playedCards[cardValue];
            }
            return 0;
        }

        /// <summary>
        /// 获取所有剩余的牌
        /// </summary>
        public Dictionary<int, int> GetAllRemainingCards()
        {
            Dictionary<int, int> remaining = new Dictionary<int, int>();
            foreach (var kvp in totalCards)
            {
                int remainingCount = kvp.Value - playedCards[kvp.Key];
                if (remainingCount > 0)
                {
                    remaining[kvp.Key] = remainingCount;
                }
            }
            return remaining;
        }

        /// <summary>
        /// 检查某张牌是否已全部出完
        /// </summary>
        public bool IsCardExhausted(int cardValue)
        {
            return GetRemainingCount(cardValue) == 0;
        }

        /// <summary>
        /// 检查是否还有炸弹（包括王炸）
        /// </summary>
        public bool HasRemainingBomb()
        {
            // 检查普通炸弹
            for (int i = 3; i <= 15; i++)
            {
                if (GetRemainingCount(i) >= 4)
                    return true;
            }
            // 检查王炸
            if (GetRemainingCount(16) > 0 && GetRemainingCount(17) > 0)
                return true;
            return false;
        }

        /// <summary>
        /// 获取剩余的大牌数量（A, 2, 大小王）
        /// </summary>
        public int GetRemainingBigCards()
        {
            int count = 0;
            count += GetRemainingCount(14); // A
            count += GetRemainingCount(15); // 2
            count += GetRemainingCount(16); // 小王
            count += GetRemainingCount(17); // 大王
            return count;
        }

        /// <summary>
        /// 推测对手是否有炸弹
        /// </summary>
        public bool LikelyHasBomb(ArrayList opponentCards)
        {
            if (opponentCards == null) return false;

            Dictionary<int, int> cardCounts = new Dictionary<int, int>();
            foreach (int card in opponentCards)
            {
                if (cardCounts.ContainsKey(card))
                    cardCounts[card]++;
                else
                    cardCounts[card] = 1;
            }

            foreach (var count in cardCounts.Values)
            {
                if (count >= 4) return true;
            }

            // 检查是否有双王
            if (cardCounts.ContainsKey(16) && cardCounts.ContainsKey(17))
                return true;

            return false;
        }

        /// <summary>
        /// 获取玩家出牌历史
        /// </summary>
        public List<int[]> GetPlayerHistory(int playerId)
        {
            if (playerPlayedCards.ContainsKey(playerId))
                return playerPlayedCards[playerId];
            return new List<int[]>();
        }
    }
}