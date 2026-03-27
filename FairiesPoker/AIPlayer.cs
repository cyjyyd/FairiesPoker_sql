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
        Early,          // 早期：保守出牌，保留大牌
        Mid,            // 中期：根据局势调整
        Late,           // 后期：激进出牌，争取跑牌
        Cooperative     // 配合（农民配合队友）
    }

    /// <summary>
    /// 策略优化版AI出牌逻辑
    /// 核心策略：大牌保留、小牌先行、阶段出牌、智能控场
    /// </summary>
    public class AIPlayer
    {
        private CardMemory cardMemory;
        private Jiepai jiepai;
        private Chupai chupai;
        private Random random;

        // 游戏状态
        private int myPosition;
        private bool isLandlord;
        private int landlordPosition;

        // 策略参数
        private AIStrategy currentStrategy;

        // 游戏阶段常量
        private const int STAGE_EARLY = 1;
        private const int STAGE_MID = 2;
        private const int STAGE_LATE = 3;

        // 控场牌阈值
        private const int CONTROL_CARD_THRESHOLD = 14;

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
            currentStrategy = isLandlord ? AIStrategy.Early : AIStrategy.Cooperative;
        }

        /// <summary>
        /// 根据手牌数量调整策略
        /// </summary>
        public void AdjustStrategyByCards(ArrayList myCards)
        {
            if (myCards == null) return;

            int cardCount = myCards.Count;

            if (cardCount > 12)
            {
                currentStrategy = AIStrategy.Early;
            }
            else if (cardCount >= 8)
            {
                currentStrategy = AIStrategy.Mid;
            }
            else
            {
                currentStrategy = AIStrategy.Late;
            }
        }

        /// <summary>
        /// 获取当前游戏阶段
        /// </summary>
        private int GetGameStage(int cardCount)
        {
            if (cardCount > 12) return STAGE_EARLY;
            if (cardCount >= 8) return STAGE_MID;
            return STAGE_LATE;
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
                    score += 25;
                    if (kvp.Key >= 14) score += 10;
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
            score += cardCounts.ContainsKey(17) ? 15 : 0;
            score += cardCounts.ContainsKey(16) ? 12 : 0;
            score += (cardCounts.ContainsKey(15) ? cardCounts[15] : 0) * 6;
            score += (cardCounts.ContainsKey(14) ? cardCounts[14] : 0) * 4;

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

            int myCardCount = myCards.Count;
            int stage = GetGameStage(myCardCount);

            // 根据阶段选择策略
            if (stage == STAGE_EARLY)
            {
                return PlayEarlyStage(myCards, singles, pairs, triples, bombs, previousPlayerCards, nextPlayerCards);
            }
            else if (stage == STAGE_MID)
            {
                return PlayMidStage(myCards, singles, pairs, triples, bombs, previousPlayerCards, nextPlayerCards);
            }
            else
            {
                return PlayLateStage(myCards, singles, pairs, triples, bombs);
            }
        }

        /// <summary>
        /// 早期阶段出牌 - 保守策略，保留大牌
        /// </summary>
        private ArrayList PlayEarlyStage(ArrayList myCards, int[] singles, int[] pairs, int[] triples, int[] bombs,
                                         int previousPlayerCards, int nextPlayerCards)
        {
            ArrayList result = new ArrayList();

            // 1. 尝试出组合牌（顺子、连对、飞机）
            ArrayList comboResult = TryPlayCombo(myCards);
            if (comboResult != null) return comboResult;

            // 2. 农民配合逻辑
            if (!isLandlord)
            {
                // 如果地主在后面，出小牌顶他
                bool landlordIsNext = IsLandlordNext(previousPlayerCards, nextPlayerCards);
                if (landlordIsNext)
                {
                    // 出小牌让地主接
                    return PlaySmallCard(singles, pairs, triples);
                }
            }

            // 3. 出小三带
            if (triples != null && triples.Length > 0)
            {
                int smallTriple = GetSmallCard(triples, 13); // Q以下
                if (smallTriple > 0)
                {
                    for (int i = 0; i < 3; i++) result.Add(smallTriple);
                    // 带小牌
                    if (singles != null && singles.Length > 0)
                    {
                        int smallSingle = GetSmallCard(singles, CONTROL_CARD_THRESHOLD);
                        if (smallSingle > 0) result.Add(smallSingle);
                        else result.Add(singles[0]);
                    }
                    else if (pairs != null && pairs.Length > 0)
                    {
                        int smallPair = GetSmallCard(pairs, CONTROL_CARD_THRESHOLD);
                        if (smallPair > 0)
                        {
                            result.Add(smallPair);
                            result.Add(smallPair);
                        }
                    }
                    return result;
                }
            }

            // 4. 出小对子
            if (pairs != null && pairs.Length > 0)
            {
                int smallPair = GetSmallCard(pairs, CONTROL_CARD_THRESHOLD);
                if (smallPair > 0)
                {
                    result.Add(smallPair);
                    result.Add(smallPair);
                    return result;
                }
            }

            // 5. 出小单张
            if (singles != null && singles.Length > 0)
            {
                int smallSingle = GetSmallCard(singles, CONTROL_CARD_THRESHOLD);
                if (smallSingle > 0)
                {
                    result.Add(smallSingle);
                    return result;
                }
                // 只有大牌，出最小的
                result.Add(singles[0]);
                return result;
            }

            // 6. 只有三张或炸弹
            if (triples != null && triples.Length > 0)
            {
                for (int i = 0; i < 3; i++) result.Add(triples[0]);
                return result;
            }

            return PlaySmallestCard(myCards, singles, pairs, triples, bombs);
        }

        /// <summary>
        /// 中期阶段出牌 - 根据局势调整
        /// </summary>
        private ArrayList PlayMidStage(ArrayList myCards, int[] singles, int[] pairs, int[] triples, int[] bombs,
                                       int previousPlayerCards, int nextPlayerCards)
        {
            ArrayList result = new ArrayList();
            int handStrength = EvaluateHandStrength(myCards);

            // 尝试出组合牌
            ArrayList comboResult = TryPlayCombo(myCards);
            if (comboResult != null) return comboResult;

            // 手牌好，可以激进一点
            bool canAggressive = isLandlord ? (handStrength > 50) : (handStrength > 40);

            // 出三带
            if (triples != null && triples.Length > 0)
            {
                int tripleCard = canAggressive ?
                    GetMediumOrSmallCard(triples) :
                    GetSmallCard(triples, CONTROL_CARD_THRESHOLD);

                if (tripleCard == 0) tripleCard = triples[0];

                for (int i = 0; i < 3; i++) result.Add(tripleCard);

                if (singles != null && singles.Length > 0)
                {
                    int single = GetSmallCard(singles, CONTROL_CARD_THRESHOLD);
                    if (single > 0) result.Add(single);
                    else result.Add(singles[0]);
                }
                else if (pairs != null && pairs.Length > 0)
                {
                    int pair = GetSmallCard(pairs, CONTROL_CARD_THRESHOLD);
                    if (pair > 0)
                    {
                        result.Add(pair);
                        result.Add(pair);
                    }
                }
                return result;
            }

            // 出对子
            if (pairs != null && pairs.Length > 0)
            {
                int pairCard = canAggressive ?
                    GetMediumOrSmallCard(pairs) :
                    GetSmallCard(pairs, CONTROL_CARD_THRESHOLD);

                if (pairCard == 0) pairCard = pairs[0];

                result.Add(pairCard);
                result.Add(pairCard);
                return result;
            }

            // 出单张
            if (singles != null && singles.Length > 0)
            {
                int singleCard = canAggressive ?
                    GetMediumOrSmallCard(singles) :
                    GetSmallCard(singles, CONTROL_CARD_THRESHOLD);

                if (singleCard == 0) singleCard = singles[0];

                result.Add(singleCard);
                return result;
            }

            return PlaySmallestCard(myCards, singles, pairs, triples, bombs);
        }

        /// <summary>
        /// 后期阶段出牌 - 激进策略，争取跑牌
        /// </summary>
        private ArrayList PlayLateStage(ArrayList myCards, int[] singles, int[] pairs, int[] triples, int[] bombs)
        {
            ArrayList result = new ArrayList();
            int cardCount = myCards.Count;

            // 牌很少时，快速结束
            if (cardCount <= 3)
            {
                return PlayQuickFinish(myCards, singles, pairs, triples, bombs);
            }

            // 尝试出组合牌
            ArrayList comboResult = TryPlayCombo(myCards);
            if (comboResult != null) return comboResult;

            // 出大三带
            if (triples != null && triples.Length > 0)
            {
                int card = triples[triples.Length - 1];
                for (int i = 0; i < 3; i++) result.Add(card);

                if (singles != null && singles.Length > 0)
                    result.Add(singles[0]);
                else if (pairs != null && pairs.Length > 0)
                {
                    result.Add(pairs[0]);
                    result.Add(pairs[0]);
                }
                return result;
            }

            // 出大对子
            if (pairs != null && pairs.Length > 0)
            {
                int card = pairs[pairs.Length - 1];
                result.Add(card);
                result.Add(card);
                return result;
            }

            // 出大单张
            if (singles != null && singles.Length > 0)
            {
                result.Add(singles[singles.Length - 1]);
                return result;
            }

            // 只有炸弹
            if (bombs != null && bombs.Length > 0)
            {
                int card = bombs[0];
                for (int i = 0; i < 4; i++) result.Add(card);
                return result;
            }

            return myCards;
        }

        /// <summary>
        /// 快速结束出牌（牌数<=3时）
        /// </summary>
        private ArrayList PlayQuickFinish(ArrayList myCards, int[] singles, int[] pairs, int[] triples, int[] bombs)
        {
            ArrayList result = new ArrayList();

            if (chupai.isRight(myCards))
            {
                return myCards;
            }

            if (bombs != null && bombs.Length > 0)
            {
                int card = bombs[0];
                for (int i = 0; i < 4; i++) result.Add(card);
                return result;
            }

            if (triples != null && triples.Length > 0)
            {
                for (int i = 0; i < 3; i++) result.Add(triples[0]);
                return result;
            }

            if (pairs != null && pairs.Length > 0)
            {
                result.Add(pairs[0]);
                result.Add(pairs[0]);
                return result;
            }

            if (singles != null && singles.Length > 0)
            {
                result.Add(singles[0]);
                return result;
            }

            return myCards;
        }

        /// <summary>
        /// 尝试出组合牌（顺子、连对、飞机）
        /// </summary>
        private ArrayList TryPlayCombo(ArrayList myCards)
        {
            // 尝试顺子
            ArrayList straight = FindStraight(myCards);
            if (straight != null) return straight;

            // 尝试连对
            ArrayList doubleStraight = FindDoubleStraight(myCards);
            if (doubleStraight != null) return doubleStraight;

            // 尝试飞机
            ArrayList plane = FindPlane(myCards);
            if (plane != null) return plane;

            return null;
        }

        /// <summary>
        /// 查找顺子
        /// </summary>
        private ArrayList FindStraight(ArrayList myCards)
        {
            Dictionary<int, int> cardCounts = new Dictionary<int, int>();
            foreach (int card in myCards)
            {
                if (cardCounts.ContainsKey(card))
                    cardCounts[card]++;
                else
                    cardCounts[card] = 1;
            }

            // 找连续的牌
            ArrayList singles = new ArrayList();
            foreach (var kvp in cardCounts)
            {
                if (kvp.Key < 15) // 不包括2和王
                {
                    singles.Add(kvp.Key);
                }
            }
            singles.Sort();

            if (singles.Count < 5) return null;

            // 找最长的连续序列
            int startIdx = -1;
            int maxLength = 0;
            int currentStart = 0;
            int currentLength = 1;

            for (int i = 1; i < singles.Count; i++)
            {
                if ((int)singles[i] - (int)singles[i - 1] == 1)
                {
                    currentLength++;
                    if (currentLength >= 5 && currentLength > maxLength)
                    {
                        maxLength = currentLength;
                        startIdx = currentStart;
                    }
                }
                else
                {
                    currentStart = i;
                    currentLength = 1;
                }
            }

            if (startIdx >= 0 && maxLength >= 5)
            {
                ArrayList result = new ArrayList();
                for (int i = startIdx; i < startIdx + maxLength && i < singles.Count; i++)
                {
                    result.Add(singles[i]);
                }
                if (chupai.isRight(result))
                {
                    return result;
                }
            }

            return null;
        }

        /// <summary>
        /// 查找连对
        /// </summary>
        private ArrayList FindDoubleStraight(ArrayList myCards)
        {
            Dictionary<int, int> cardCounts = new Dictionary<int, int>();
            foreach (int card in myCards)
            {
                if (cardCounts.ContainsKey(card))
                    cardCounts[card]++;
                else
                    cardCounts[card] = 1;
            }

            ArrayList pairs = new ArrayList();
            foreach (var kvp in cardCounts)
            {
                if (kvp.Value >= 2 && kvp.Key < 15)
                {
                    pairs.Add(kvp.Key);
                }
            }

            if (pairs.Count < 3) return null;

            pairs.Sort();

            int consecutive = 1;
            for (int i = 1; i < pairs.Count; i++)
            {
                if ((int)pairs[i] - (int)pairs[i - 1] == 1)
                {
                    consecutive++;
                    if (consecutive >= 3)
                    {
                        ArrayList result = new ArrayList();
                        int startIdx = i - 2;
                        for (int j = startIdx; j <= i; j++)
                        {
                            int card = (int)pairs[j];
                            result.Add(card);
                            result.Add(card);
                        }
                        if (chupai.isRight(result))
                        {
                            return result;
                        }
                    }
                }
                else
                {
                    consecutive = 1;
                }
            }

            return null;
        }

        /// <summary>
        /// 查找飞机
        /// </summary>
        private ArrayList FindPlane(ArrayList myCards)
        {
            Dictionary<int, int> cardCounts = new Dictionary<int, int>();
            foreach (int card in myCards)
            {
                if (cardCounts.ContainsKey(card))
                    cardCounts[card]++;
                else
                    cardCounts[card] = 1;
            }

            ArrayList triples = new ArrayList();
            foreach (var kvp in cardCounts)
            {
                if (kvp.Value >= 3 && kvp.Key < 15)
                {
                    triples.Add(kvp.Key);
                }
            }

            if (triples.Count < 2) return null;

            triples.Sort();

            int consecutive = 1;
            for (int i = 1; i < triples.Count; i++)
            {
                if ((int)triples[i] - (int)triples[i - 1] == 1)
                {
                    consecutive++;
                    if (consecutive >= 2)
                    {
                        ArrayList result = new ArrayList();
                        int startIdx = i - 1;
                        for (int j = startIdx; j <= i; j++)
                        {
                            int card = (int)triples[j];
                            result.Add(card);
                            result.Add(card);
                            result.Add(card);
                        }
                        if (chupai.isRight(result))
                        {
                            return result;
                        }
                    }
                }
                else
                {
                    consecutive = 1;
                }
            }

            return null;
        }

        /// <summary>
        /// 出小牌
        /// </summary>
        private ArrayList PlaySmallCard(int[] singles, int[] pairs, int[] triples)
        {
            ArrayList result = new ArrayList();

            if (singles != null && singles.Length > 0)
            {
                int small = GetSmallCard(singles, CONTROL_CARD_THRESHOLD);
                if (small > 0)
                {
                    result.Add(small);
                    return result;
                }
            }

            if (pairs != null && pairs.Length > 0)
            {
                int small = GetSmallCard(pairs, CONTROL_CARD_THRESHOLD);
                if (small > 0)
                {
                    result.Add(small);
                    result.Add(small);
                    return result;
                }
            }

            if (triples != null && triples.Length > 0)
            {
                int small = GetSmallCard(triples, 13);
                if (small > 0)
                {
                    for (int i = 0; i < 3; i++) result.Add(small);
                    return result;
                }
            }

            // 没有小牌，出最小的
            if (singles != null && singles.Length > 0)
            {
                result.Add(singles[0]);
                return result;
            }

            return null;
        }

        /// <summary>
        /// 出最小的牌
        /// </summary>
        private ArrayList PlaySmallestCard(ArrayList myCards, int[] singles, int[] pairs, int[] triples, int[] bombs)
        {
            ArrayList result = new ArrayList();

            if (singles != null && singles.Length > 0)
            {
                result.Add(singles[0]);
                return result;
            }

            if (pairs != null && pairs.Length > 0)
            {
                result.Add(pairs[0]);
                result.Add(pairs[0]);
                return result;
            }

            if (triples != null && triples.Length > 0)
            {
                for (int i = 0; i < 3; i++) result.Add(triples[0]);
                return result;
            }

            if (bombs != null && bombs.Length > 0)
            {
                for (int i = 0; i < 4; i++) result.Add(bombs[0]);
                return result;
            }

            return myCards;
        }

        /// <summary>
        /// 获取小牌（小于阈值的牌）
        /// </summary>
        private int GetSmallCard(int[] cards, int threshold)
        {
            if (cards == null || cards.Length == 0) return 0;

            for (int i = 0; i < cards.Length; i++)
            {
                if (cards[i] < threshold)
                {
                    return cards[i];
                }
            }
            return 0;
        }

        /// <summary>
        /// 获取中等或偏小的牌
        /// </summary>
        private int GetMediumOrSmallCard(int[] cards)
        {
            if (cards == null || cards.Length == 0) return 0;

            // 先找小于阈值的最大牌
            for (int i = cards.Length - 1; i >= 0; i--)
            {
                if (cards[i] < CONTROL_CARD_THRESHOLD)
                {
                    return cards[i];
                }
            }

            // 没有小于阈值的，返回最小的
            return cards[0];
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
                return null;
            }

            // 选择最佳接牌
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

            bool shouldRespond = ShouldRespondToPlay(upperPlayerPosition, previousPlayerCards, nextPlayerCards);

            if (!shouldRespond && !IsLandlordNext(previousPlayerCards, nextPlayerCards))
            {
                return null;
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

            if (paiType == (int)Guize.一张 || paiType == (int)Guize.对子)
            {
                return bestResponse;
            }

            if (possibleResponses[0] is ArrayList)
            {
                return (ArrayList)possibleResponses[0];
            }

            return possibleResponses.Count > 0 ? (ArrayList)possibleResponses[0] : null;
        }

        /// <summary>
        /// 判断是否应该接牌
        /// </summary>
        private bool ShouldRespondToPlay(int upperPlayerPosition, int previousPlayerCards, int nextPlayerCards)
        {
            if (isLandlord)
            {
                return true;
            }

            bool upperIsLandlord = (upperPlayerPosition == landlordPosition);
            bool upperIsTeammate = (!upperIsLandlord && upperPlayerPosition != myPosition);

            if (upperIsTeammate)
            {
                if (previousPlayerCards <= 3 || nextPlayerCards <= 3)
                {
                    return false;
                }
                return random.Next(100) < 30;
            }

            if (upperIsLandlord)
            {
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
            int stage = GetGameStage(myCards.Count);

            if (isLandlord)
            {
                return stage == STAGE_LATE || myCards.Count <= 5;
            }
            else
            {
                bool upperIsLandlord = (upperPlayerPosition == landlordPosition);

                if (upperIsLandlord)
                {
                    if (stage == STAGE_LATE) return true;
                    return myCards.Count <= 6;
                }
                else
                {
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
        private bool IsLandlordNext(int previousPlayerCards, int nextPlayerCards)
        {
            return false;
        }
    }
}