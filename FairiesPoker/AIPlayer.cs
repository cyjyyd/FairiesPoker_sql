using System;
using System.Collections;
using System.Collections.Generic;

namespace FairiesPoker
{
    /// <summary>
    /// AI策略类型
    /// </summary>
    public enum AIStrategy
    {
        Aggressive,     // 激进（地主或牌很好时）
        Conservative,   // 保守（牌不好时）
        Cooperative,    // 配合（农民配合）
        Defensive       // 防守（保护队友）
    }

    /// <summary>
    /// 增强版AI出牌逻辑
    /// </summary>
    public class AIPlayer
    {
        private CardMemory cardMemory;
        private Jiepai jiepai;
        private Chupai chupai;
        private Random random;

        // 游戏状态
        private int myPosition;         // 我的位置 (1, 2, 3)
        private bool isLandlord;        // 是否是地主
        private int landlordPosition;   // 地主位置

        // 策略参数
        private AIStrategy currentStrategy;

        public AIPlayer()
        {
            cardMemory = new CardMemory();
            jiepai = new Jiepai();
            chupai = new Chupai();
            random = new Random();
        }

        /// <summary>
        /// 初始化新游戏
        /// </summary>
        public void NewGame(int myPos, bool isLandlord, int landlordPos)
        {
            cardMemory.Initialize();
            this.myPosition = myPos;
            this.isLandlord = isLandlord;
            this.landlordPosition = landlordPos;
            DetermineStrategy();
        }

        /// <summary>
        /// 记录出牌
        /// </summary>
        public void RecordPlay(int playerId, int[] cards)
        {
            cardMemory.RecordPlay(playerId, cards);
        }

        /// <summary>
        /// 确定当前策略
        /// </summary>
        private void DetermineStrategy()
        {
            if (isLandlord)
            {
                currentStrategy = AIStrategy.Aggressive;
            }
            else
            {
                currentStrategy = AIStrategy.Cooperative;
            }
        }

        /// <summary>
        /// 根据手牌调整策略
        /// </summary>
        public void AdjustStrategyByCards(ArrayList myCards)
        {
            int handStrength = EvaluateHandStrength(myCards);

            if (isLandlord)
            {
                if (handStrength > 70)
                    currentStrategy = AIStrategy.Aggressive;
                else if (handStrength < 30)
                    currentStrategy = AIStrategy.Conservative;
            }
            else
            {
                // 农民根据情况调整
                if (handStrength > 60)
                    currentStrategy = AIStrategy.Cooperative;
                else
                    currentStrategy = AIStrategy.Defensive;
            }
        }

        /// <summary>
        /// 评估手牌强度 (0-100)
        /// </summary>
        private int EvaluateHandStrength(ArrayList cards)
        {
            if (cards == null || cards.Count == 0) return 0;

            int score = 0;
            Dictionary<int, int> cardCounts = new Dictionary<int, int>();

            foreach (int card in cards)
            {
                if (cardCounts.ContainsKey(card))
                    cardCounts[card]++;
                else
                    cardCounts[card] = 1;
            }

            // 评估炸弹
            foreach (var kvp in cardCounts)
            {
                if (kvp.Value == 4)
                {
                    score += 25; // 炸弹加分
                    if (kvp.Key >= 14) score += 10; // 大炸弹额外加分
                }
                else if (kvp.Value == 3)
                {
                    score += 8;
                }
                else if (kvp.Value == 2)
                {
                    score += 3;
                }
            }

            // 评估大牌
            score += cardCounts.ContainsKey(17) ? 15 : 0; // 大王
            score += cardCounts.ContainsKey(16) ? 12 : 0; // 小王
            score += (cardCounts.ContainsKey(15) ? cardCounts[15] : 0) * 6; // 2
            score += (cardCounts.ContainsKey(14) ? cardCounts[14] : 0) * 4; // A

            // 王炸
            if (cardCounts.ContainsKey(16) && cardCounts.ContainsKey(17))
                score += 10;

            // 根据牌数调整
            int cardCount = cards.Count;
            if (cardCount <= 5) score += 20;
            else if (cardCount <= 10) score += 10;

            return Math.Min(100, score);
        }

        /// <summary>
        /// 智能出牌（主动出牌）
        /// </summary>
        public ArrayList SmartPlay(ArrayList myCards, int previousPlayerCards, int nextPlayerCards)
        {
            if (myCards == null || myCards.Count == 0) return null;

            AdjustStrategyByCards(myCards);

            // 如果只剩一手牌，直接出
            if (chupai.isRight(myCards))
            {
                return myCards;
            }

            // 分析手牌结构
            ArrayList basicCards = jiepai.basic(2, myCards);
            if (basicCards == null) return null;

            int[] singles = jiepai.mArrayToArgs((ArrayList)basicCards[0]);
            int[] pairs = jiepai.mArrayToArgs((ArrayList)basicCards[1]);
            int[] triples = jiepai.mArrayToArgs((ArrayList)basicCards[2]);
            int[] bombs = jiepai.mArrayToArgs((ArrayList)basicCards[3]);

            // 根据剩余牌数和身份选择策略
            int myCardCount = myCards.Count;

            // 地主且牌少时激进出牌
            if (isLandlord && myCardCount <= 5)
            {
                return PlayAggressive(myCards, singles, pairs, triples, bombs);
            }

            // 农民配合逻辑
            if (!isLandlord)
            {
                return PlayAsFarmer(myCards, singles, pairs, triples, bombs, previousPlayerCards, nextPlayerCards);
            }

            // 默认：从小到大出
            return PlayConservative(myCards, singles, pairs, triples, bombs);
        }

        /// <summary>
        /// 激进出牌
        /// </summary>
        private ArrayList PlayAggressive(ArrayList myCards, int[] singles, int[] pairs, int[] triples, int[] bombs)
        {
            ArrayList result = new ArrayList();

            // 优先出大牌
            if (triples != null && triples.Length > 0)
            {
                // 三带
                int card = triples[triples.Length - 1]; // 出最大的三张
                for (int i = 0; i < 3; i++) result.Add(card);
                if (singles != null && singles.Length > 0)
                    result.Add(singles[0]);
                else if (pairs != null && pairs.Length > 0)
                    result.Add(pairs[0]);
                return result;
            }

            if (pairs != null && pairs.Length > 0)
            {
                int card = pairs[pairs.Length - 1];
                result.Add(card);
                result.Add(card);
                return result;
            }

            if (singles != null && singles.Length > 0)
            {
                result.Add(singles[singles.Length - 1]);
                return result;
            }

            // 只剩炸弹了
            if (bombs != null && bombs.Length > 0)
            {
                int card = bombs[0];
                for (int i = 0; i < 4; i++) result.Add(card);
                return result;
            }

            return myCards;
        }

        /// <summary>
        /// 保守出牌（先出小牌）
        /// </summary>
        private ArrayList PlayConservative(ArrayList myCards, int[] singles, int[] pairs, int[] triples, int[] bombs)
        {
            ArrayList result = new ArrayList();

            // 优先出小单张
            if (singles != null && singles.Length > 0)
            {
                // 出最小的单张，但保留A、2、王
                for (int i = 0; i < singles.Length; i++)
                {
                    if (singles[i] < 14) // 不出A以上的牌
                    {
                        result.Add(singles[i]);
                        return result;
                    }
                }
                // 如果只剩大牌，出最小的那个
                result.Add(singles[0]);
                return result;
            }

            // 出对子
            if (pairs != null && pairs.Length > 0)
            {
                int card = pairs[0];
                result.Add(card);
                result.Add(card);
                return result;
            }

            // 出三张
            if (triples != null && triples.Length > 0)
            {
                int card = triples[0];
                for (int i = 0; i < 3; i++) result.Add(card);
                return result;
            }

            // 只有炸弹了
            if (bombs != null && bombs.Length > 0)
            {
                int card = bombs[0];
                for (int i = 0; i < 4; i++) result.Add(card);
                return result;
            }

            return null;
        }

        /// <summary>
        /// 农民配合出牌
        /// </summary>
        private ArrayList PlayAsFarmer(ArrayList myCards, int[] singles, int[] pairs, int[] triples, int[] bombs,
                                        int previousPlayerCards, int nextPlayerCards)
        {
            ArrayList result = new ArrayList();

            // 判断队友（另一个农民）的情况
            bool teammateHasFewCards = false; // 队友是否牌少
            int teammatePosition = GetTeammatePosition();

            // 如果地主在后面，要小心出牌
            bool landlordIsNext = IsLandlordNext();

            // 如果队友牌少，帮忙跑牌
            if (teammateHasFewCards && !landlordIsNext)
            {
                // 出小牌帮队友跑
                return PlayConservative(myCards, singles, pairs, triples, bombs);
            }

            // 如果地主牌少，要顶地主
            if (IsLandlordLowOnCards())
            {
                return PlayToBlockLandlord(myCards, singles, pairs, triples, bombs);
            }

            // 正常出牌
            return PlayConservative(myCards, singles, pairs, triples, bombs);
        }

        /// <summary>
        /// 顶地主的牌
        /// </summary>
        private ArrayList PlayToBlockLandlord(ArrayList myCards, int[] singles, int[] pairs, int[] triples, int[] bombs)
        {
            ArrayList result = new ArrayList();

            // 出大牌顶住
            if (singles != null && singles.Length > 0)
            {
                // 出较大的单张，迫使地主用更大的牌
                int index = Math.Max(0, singles.Length / 2); // 出中等大小的牌
                if (singles[index] >= 12) // Q以上开始顶
                {
                    result.Add(singles[index]);
                    return result;
                }
            }

            if (pairs != null && pairs.Length > 0)
            {
                int index = Math.Max(0, pairs.Length / 2);
                result.Add(pairs[index]);
                result.Add(pairs[index]);
                return result;
            }

            return PlayConservative(myCards, singles, pairs, triples, bombs);
        }

        /// <summary>
        /// 智能接牌
        /// </summary>
        public ArrayList SmartRespond(int paiType, ArrayList upperCards, ArrayList myCards,
                                       int upperPlayerPosition, int previousPlayerCards, int nextPlayerCards)
        {
            if (myCards == null || myCards.Count == 0) return null;

            AdjustStrategyByCards(myCards);

            // 获取可以接的牌
            ArrayList possibleResponses = jiepai.isRight(paiType, upperCards, myCards);

            if (possibleResponses == null || possibleResponses.Count == 0)
            {
                // 检查是否有炸弹
                ArrayList bomb = jiepai.findZhadan(myCards);
                ArrayList rocket = jiepai.findTianzha(myCards);

                if (bomb != null || rocket != null)
                {
                    // 决定是否炸
                    if (ShouldUseBomb(paiType, myCards, upperPlayerPosition, previousPlayerCards, nextPlayerCards))
                    {
                        if (rocket != null)
                        {
                            return rocket;
                        }
                        ArrayList result = new ArrayList();
                        int bombCard = (int)bomb[0];
                        for (int i = 0; i < 4; i++) result.Add(bombCard);
                        return result;
                    }
                }
                return null; // 不接
            }

            // 选择最佳接牌策略
            return ChooseBestResponse(possibleResponses, paiType, upperCards, myCards,
                                       upperPlayerPosition, previousPlayerCards, nextPlayerCards);
        }

        /// <summary>
        /// 选择最佳接牌
        /// </summary>
        private ArrayList ChooseBestResponse(ArrayList possibleResponses, int paiType, ArrayList upperCards,
                                              ArrayList myCards, int upperPlayerPosition,
                                              int previousPlayerCards, int nextPlayerCards)
        {
            if (possibleResponses == null || possibleResponses.Count == 0) return null;

            // 判断是否应该接牌
            bool shouldRespond = ShouldRespond(upperPlayerPosition, previousPlayerCards, nextPlayerCards);

            if (!shouldRespond && !IsLandlordNext())
            {
                return null; // 不接，让队友接
            }

            // 选择最小的能接的牌
            ArrayList bestResponse = null;
            int minValue = int.MaxValue;

            foreach (var response in possibleResponses)
            {
                ArrayList resp = response as ArrayList;
                if (resp != null && resp.Count > 0)
                {
                    int[] args = jiepai.arrayToArgs(resp);
                    if (args != null && args[0] < minValue)
                    {
                        minValue = args[0];
                        bestResponse = resp;
                    }
                }
            }

            // 如果是单张或对子，可能选择最小接牌
            if (paiType == (int)Guize.一张 || paiType == (int)Guize.对子)
            {
                return bestResponse;
            }

            // 其他情况，返回第一个可用的
            if (possibleResponses[0] is ArrayList)
            {
                return (ArrayList)possibleResponses[0];
            }

            return possibleResponses.Count > 0 ? (ArrayList)possibleResponses[0] : null;
        }

        /// <summary>
        /// 判断是否应该接牌
        /// </summary>
        private bool ShouldRespond(int upperPlayerPosition, int previousPlayerCards, int nextPlayerCards)
        {
            if (isLandlord)
            {
                // 地主一定要接
                return true;
            }

            // 农民逻辑
            bool upperIsLandlord = (upperPlayerPosition == landlordPosition);
            bool upperIsTeammate = (!upperIsLandlord && upperPlayerPosition != myPosition);

            if (upperIsTeammate)
            {
                // 队友出的牌
                if (previousPlayerCards <= 3 || nextPlayerCards <= 3)
                {
                    // 队友牌少，让他跑
                    return false;
                }
                // 牌多时可以接
                return random.Next(100) < 30; // 30%概率接队友的牌
            }

            if (upperIsLandlord)
            {
                // 地主出的牌，尽量接
                return true;
            }

            return true;
        }

        /// <summary>
        /// 判断是否应该使用炸弹
        /// </summary>
        private bool ShouldUseBomb(int paiType, ArrayList myCards, int upperPlayerPosition,
                                    int previousPlayerCards, int nextPlayerCards)
        {
            if (isLandlord)
            {
                // 地主：牌少就炸
                return myCards.Count <= 6;
            }
            else
            {
                // 农民
                bool upperIsLandlord = (upperPlayerPosition == landlordPosition);

                if (upperIsLandlord)
                {
                    // 地主出的牌
                    if (nextPlayerCards <= 2 || previousPlayerCards <= 2)
                    {
                        // 地主快赢了，必须炸
                        return true;
                    }
                    // 地主牌少时炸
                    return nextPlayerCards <= 5 || previousPlayerCards <= 5;
                }
                else
                {
                    // 队友出的牌，不炸
                    return false;
                }
            }
        }

        /// <summary>
        /// 获取队友位置
        /// </summary>
        private int GetTeammatePosition()
        {
            if (isLandlord) return -1;

            for (int i = 1; i <= 3; i++)
            {
                if (i != myPosition && i != landlordPosition)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// 判断地主是否在下一个出牌
        /// </summary>
        private bool IsLandlordNext()
        {
            // 简化实现
            return false;
        }

        /// <summary>
        /// 判断地主是否牌少
        /// </summary>
        private bool IsLandlordLowOnCards()
        {
            // 需要从外部获取信息
            return false;
        }

        /// <summary>
        /// 判断地主是否在后面
        /// </summary>
        private bool IsLandlordAfterMe()
        {
            return false;
        }
    }
}