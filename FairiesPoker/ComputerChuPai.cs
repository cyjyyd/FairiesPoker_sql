using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace FairiesPoker
{
    /// <summary>
    /// 电脑出牌类 - 策略优化版AI
    /// 核心策略：大牌保留、小牌先行、阶段出牌、智能控场
    /// </summary>
    class ComputerChuPai
    {
        private AIPlayer aiPlayer;
        private Jiepai jiepai;
        private int myPosition;
        private bool isLandlord;
        private int landlordPosition;

        // 游戏阶段常量
        private const int STAGE_EARLY = 1;      // 早期：手牌>12张
        private const int STAGE_MID = 2;        // 中期：手牌8-12张
        private const int STAGE_LATE = 3;       // 后期：手牌<8张

        // 控场牌阈值（A=14, 2=15, 小王=16, 大王=17）
        private const int CONTROL_CARD_THRESHOLD = 14;  // A及以上为控场牌

        public ComputerChuPai()
        {
            aiPlayer = new AIPlayer();
            jiepai = new Jiepai();
        }

        /// <summary>
        /// 初始化新游戏
        /// </summary>
        public void InitializeGame(int myPos, bool isLandlord, int landlordPos)
        {
            this.myPosition = myPos;
            this.isLandlord = isLandlord;
            this.landlordPosition = landlordPos;
            aiPlayer.NewGame(myPos, isLandlord, landlordPos);
        }

        /// <summary>
        /// 记录出牌
        /// </summary>
        public void RecordPlay(int playerId, int[] cards)
        {
            aiPlayer.RecordPlay(playerId, cards);
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

        #region 出牌（主动出牌）
        /// <summary>
        /// 出牌 - 策略优化版
        /// </summary>
        public ArrayList chuPai(ArrayList listPai)
        {
            if (listPai == null || listPai.Count == 0) return null;

            // 分析手牌结构
            ArrayList basicCards = jiepai.basic(2, listPai);
            if (basicCards == null) return null;

            int[] singles = jiepai.mArrayToArgs((ArrayList)basicCards[0]);
            int[] pairs = jiepai.mArrayToArgs((ArrayList)basicCards[1]);
            int[] triples = jiepai.mArrayToArgs((ArrayList)basicCards[2]);
            int[] bombs = jiepai.mArrayToArgs((ArrayList)basicCards[3]);

            int cardCount = listPai.Count;
            Chupai chupai = new Chupai();

            // 如果只剩一手牌，直接出
            if (chupai.isRight(listPai))
            {
                return listPai;
            }

            // 根据阶段选择策略
            int stage = GetGameStage(cardCount);

            if (stage == STAGE_EARLY)
            {
                // 早期：出小牌，建立出牌路线，保留大牌
                return PlayEarlyStage(listPai, singles, pairs, triples, bombs);
            }
            else if (stage == STAGE_MID)
            {
                // 中期：根据手牌强度选择策略
                return PlayMidStage(listPai, singles, pairs, triples, bombs);
            }
            else
            {
                // 后期：激进出牌，争取跑牌
                return PlayLateStage(listPai, singles, pairs, triples, bombs);
            }
        }

        /// <summary>
        /// 早期阶段出牌策略
        /// 核心：出小牌、建立出牌路线、保留大牌
        /// </summary>
        private ArrayList PlayEarlyStage(ArrayList listPai, int[] singles, int[] pairs, int[] triples, int[] bombs)
        {
            ArrayList result = new ArrayList();

            // 1. 优先出顺子、连对等组合牌（消耗多张牌）
            ArrayList comboResult = TryPlayComboFirst(listPai);
            if (comboResult != null) return comboResult;

            // 2. 出小三带（带小牌）
            if (triples != null && triples.Length > 0)
            {
                int smallTriple = GetSmallTriple(triples);
                if (smallTriple > 0 && smallTriple < 13) // Q以下
                {
                    for (int i = 0; i < 3; i++) result.Add(smallTriple);
                    // 带小单或小对
                    if (singles != null && singles.Length > 0)
                    {
                        int smallSingle = GetSmallCard(singles, CONTROL_CARD_THRESHOLD);
                        if (smallSingle > 0) result.Add(smallSingle);
                        else result.Add(singles[0]); // 没有小牌就带最小的
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

            // 3. 出小对子
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

            // 4. 出小单张
            if (singles != null && singles.Length > 0)
            {
                int smallSingle = GetSmallCard(singles, CONTROL_CARD_THRESHOLD);
                if (smallSingle > 0)
                {
                    result.Add(smallSingle);
                    return result;
                }
                // 只有大牌单张时，出最小的
                result.Add(singles[0]);
                return result;
            }

            // 5. 只剩三张或炸弹
            if (triples != null && triples.Length > 0)
            {
                int card = triples[0];
                for (int i = 0; i < 3; i++) result.Add(card);
                return result;
            }

            // 没有合适的牌，出最小的牌
            return PlaySmallestCard(listPai, singles, pairs, triples, bombs);
        }

        /// <summary>
        /// 中期阶段出牌策略
        /// 核心：根据局势调整，开始考虑跑牌
        /// </summary>
        private ArrayList PlayMidStage(ArrayList listPai, int[] singles, int[] pairs, int[] triples, int[] bombs)
        {
            ArrayList result = new ArrayList();
            int handStrength = EvaluateHandStrength(listPai);

            // 手牌好或农民身份时，可以激进
            bool canAggressive = isLandlord ? (handStrength > 50) : (handStrength > 40);

            if (canAggressive)
            {
                // 有一定实力，开始推进
                // 但仍然先出小牌为主
                return PlayMidAggressive(listPai, singles, pairs, triples, bombs);
            }
            else
            {
                // 手牌一般，继续保守策略
                return PlayEarlyStage(listPai, singles, pairs, triples, bombs);
            }
        }

        /// <summary>
        /// 中期激进出牌（仍保留关键大牌）
        /// </summary>
        private ArrayList PlayMidAggressive(ArrayList listPai, int[] singles, int[] pairs, int[] triples, int[] bombs)
        {
            ArrayList result = new ArrayList();

            // 尝试出组合牌
            ArrayList comboResult = TryPlayComboFirst(listPai);
            if (comboResult != null) return comboResult;

            // 出中等大小的三带
            if (triples != null && triples.Length > 0)
            {
                int tripleCard = GetMediumCard(triples);
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

            // 出中等对子
            if (pairs != null && pairs.Length > 0)
            {
                int pairCard = GetMediumCard(pairs);
                result.Add(pairCard);
                result.Add(pairCard);
                return result;
            }

            // 出中等单张
            if (singles != null && singles.Length > 0)
            {
                int singleCard = GetMediumCard(singles);
                result.Add(singleCard);
                return result;
            }

            return PlaySmallestCard(listPai, singles, pairs, triples, bombs);
        }

        /// <summary>
        /// 后期阶段出牌策略
        /// 核心：激进出牌，争取快速跑牌
        /// </summary>
        private ArrayList PlayLateStage(ArrayList listPai, int[] singles, int[] pairs, int[] triples, int[] bombs)
        {
            ArrayList result = new ArrayList();
            int cardCount = listPai.Count;

            // 牌很少时，直接出大牌跑牌
            if (cardCount <= 3)
            {
                return PlayQuickFinish(listPai, singles, pairs, triples, bombs);
            }

            // 优先出组合牌
            ArrayList comboResult = TryPlayComboFirst(listPai);
            if (comboResult != null) return comboResult;

            // 出三带
            if (triples != null && triples.Length > 0)
            {
                int card = triples[triples.Length - 1]; // 出最大的三张
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

            // 出对子
            if (pairs != null && pairs.Length > 0)
            {
                int card = pairs[pairs.Length - 1]; // 出较大的对子
                result.Add(card);
                result.Add(card);
                return result;
            }

            // 出单张
            if (singles != null && singles.Length > 0)
            {
                result.Add(singles[singles.Length - 1]); // 出较大的单张
                return result;
            }

            // 只有炸弹了
            if (bombs != null && bombs.Length > 0)
            {
                int card = bombs[0];
                for (int i = 0; i < 4; i++) result.Add(card);
                return result;
            }

            return listPai;
        }

        /// <summary>
        /// 快速结束出牌（牌数<=3时）
        /// </summary>
        private ArrayList PlayQuickFinish(ArrayList listPai, int[] singles, int[] pairs, int[] triples, int[] bombs)
        {
            ArrayList result = new ArrayList();
            Chupai chupai = new Chupai();

            // 检查是否一手能出完
            if (chupai.isRight(listPai))
            {
                return listPai;
            }

            // 优先出炸弹
            if (bombs != null && bombs.Length > 0)
            {
                int card = bombs[0];
                for (int i = 0; i < 4; i++) result.Add(card);
                return result;
            }

            // 出三张
            if (triples != null && triples.Length > 0)
            {
                int card = triples[0];
                for (int i = 0; i < 3; i++) result.Add(card);
                return result;
            }

            // 出对子
            if (pairs != null && pairs.Length > 0)
            {
                result.Add(pairs[0]);
                result.Add(pairs[0]);
                return result;
            }

            // 出单张
            if (singles != null && singles.Length > 0)
            {
                result.Add(singles[0]);
                return result;
            }

            return listPai;
        }

        /// <summary>
        /// 尝试优先出组合牌（顺子、连对、飞机等）
        /// </summary>
        private ArrayList TryPlayComboFirst(ArrayList listPai)
        {
            Chupai chupai = new Chupai();

            // 尝试找顺子
            ArrayList straight = FindAndPlayStraight(listPai);
            if (straight != null) return straight;

            // 尝试找连对
            ArrayList doubleStraight = FindAndPlayDoubleStraight(listPai);
            if (doubleStraight != null) return doubleStraight;

            // 尝试找飞机
            ArrayList plane = FindAndPlayPlane(listPai);
            if (plane != null) return plane;

            return null;
        }

        /// <summary>
        /// 查找并出顺子
        /// </summary>
        private ArrayList FindAndPlayStraight(ArrayList listPai)
        {
            ArrayList sorted = new ArrayList(listPai);
            sorted.Sort();
            // 倒序排列（从大到小）
            ArrayList reversed = new ArrayList();
            for (int i = sorted.Count - 1; i >= 0; i--)
            {
                reversed.Add(sorted[i]);
            }

            ArrayList result = new ArrayList();
            int consecutive = 1;
            int startValue = (int)reversed[0];

            for (int i = 1; i < reversed.Count; i++)
            {
                int current = (int)reversed[i];
                int prev = (int)reversed[i - 1];

                // 检查是否连续（且不是2和王）
                if (prev - current == 1 && current < 15)
                {
                    consecutive++;
                    if (consecutive >= 5)
                    {
                        // 找到顺子，取最小的5张
                        int startIdx = i - 4;
                        for (int j = startIdx; j <= i; j++)
                        {
                            result.Add(reversed[j]);
                        }
                        Chupai chupai = new Chupai();
                        if (chupai.isRight(result))
                        {
                            return result;
                        }
                        result.Clear();
                        consecutive = 1;
                    }
                }
                else
                {
                    consecutive = 1;
                    startValue = current;
                }
            }

            return null;
        }

        /// <summary>
        /// 查找并出连对
        /// </summary>
        private ArrayList FindAndPlayDoubleStraight(ArrayList listPai)
        {
            // 统计每个牌值的数量
            Dictionary<int, int> cardCounts = new Dictionary<int, int>();
            foreach (int card in listPai)
            {
                if (cardCounts.ContainsKey(card))
                    cardCounts[card]++;
                else
                    cardCounts[card] = 1;
            }

            // 找连续的对子
            ArrayList pairs = new ArrayList();
            foreach (var kvp in cardCounts)
            {
                if (kvp.Value >= 2 && kvp.Key < 15) // 不是2和王
                {
                    pairs.Add(kvp.Key);
                }
            }

            if (pairs.Count < 3) return null;

            pairs.Sort();

            // 检查是否有连续的3对以上
            int consecutive = 1;
            for (int i = 1; i < pairs.Count; i++)
            {
                if ((int)pairs[i] - (int)pairs[i - 1] == 1)
                {
                    consecutive++;
                    if (consecutive >= 3)
                    {
                        // 找到连对，出最小的3对
                        ArrayList result = new ArrayList();
                        int startIdx = i - 2;
                        for (int j = startIdx; j <= i; j++)
                        {
                            int card = (int)pairs[j];
                            result.Add(card);
                            result.Add(card);
                        }
                        Chupai chupai = new Chupai();
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
        /// 查找并出飞机
        /// </summary>
        private ArrayList FindAndPlayPlane(ArrayList listPai)
        {
            // 统计每个牌值的数量
            Dictionary<int, int> cardCounts = new Dictionary<int, int>();
            foreach (int card in listPai)
            {
                if (cardCounts.ContainsKey(card))
                    cardCounts[card]++;
                else
                    cardCounts[card] = 1;
            }

            // 找连续的三张
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

            // 检查是否有连续的2个三张以上
            int consecutive = 1;
            for (int i = 1; i < triples.Count; i++)
            {
                if ((int)triples[i] - (int)triples[i - 1] == 1)
                {
                    consecutive++;
                    if (consecutive >= 2)
                    {
                        // 找到飞机
                        ArrayList result = new ArrayList();
                        int startIdx = i - 1;
                        for (int j = startIdx; j <= i; j++)
                        {
                            int card = (int)triples[j];
                            result.Add(card);
                            result.Add(card);
                            result.Add(card);
                        }
                        Chupai chupai = new Chupai();
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
        /// 出最小的牌
        /// </summary>
        private ArrayList PlaySmallestCard(ArrayList listPai, int[] singles, int[] pairs, int[] triples, int[] bombs)
        {
            ArrayList result = new ArrayList();

            // 出最小的单张
            if (singles != null && singles.Length > 0)
            {
                result.Add(singles[0]);
                return result;
            }

            // 出最小的对子
            if (pairs != null && pairs.Length > 0)
            {
                result.Add(pairs[0]);
                result.Add(pairs[0]);
                return result;
            }

            // 出最小的三张
            if (triples != null && triples.Length > 0)
            {
                for (int i = 0; i < 3; i++) result.Add(triples[0]);
                return result;
            }

            // 出炸弹
            if (bombs != null && bombs.Length > 0)
            {
                for (int i = 0; i < 4; i++) result.Add(bombs[0]);
                return result;
            }

            return listPai;
        }
        #endregion

        #region 辅助方法
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
            return 0; // 没有小牌
        }

        /// <summary>
        /// 获取小三张（Q以下）
        /// </summary>
        private int GetSmallTriple(int[] triples)
        {
            if (triples == null || triples.Length == 0) return 0;

            for (int i = 0; i < triples.Length; i++)
            {
                if (triples[i] < 13) // Q以下
                {
                    return triples[i];
                }
            }
            return 0;
        }

        /// <summary>
        /// 获取中等大小的牌（中间值）
        /// </summary>
        private int GetMediumCard(int[] cards)
        {
            if (cards == null || cards.Length == 0) return 0;

            // 出中间偏小的牌
            int index = Math.Max(0, cards.Length / 3);

            // 确保不出大牌
            if (cards[index] >= CONTROL_CARD_THRESHOLD && cards.Length > 1)
            {
                // 找一个小于阈值的最大牌
                for (int i = cards.Length - 1; i >= 0; i--)
                {
                    if (cards[i] < CONTROL_CARD_THRESHOLD)
                    {
                        return cards[i];
                    }
                }
            }

            return cards[index];
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

            // 炸弹加分
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

            // 大牌加分
            score += cardCounts.ContainsKey(17) ? 15 : 0;
            score += cardCounts.ContainsKey(16) ? 12 : 0;
            score += (cardCounts.ContainsKey(15) ? cardCounts[15] : 0) * 6;
            score += (cardCounts.ContainsKey(14) ? cardCounts[14] : 0) * 4;

            // 王炸加分
            if (cardCounts.ContainsKey(16) && cardCounts.ContainsKey(17))
                score += 10;

            // 牌数调整
            if (cards.Count <= 5) score += 20;
            else if (cards.Count <= 10) score += 10;

            return Math.Min(100, score);
        }
        #endregion

        #region 接牌（被动出牌）
        /// <summary>
        /// 接牌 - 策略优化版
        /// </summary>
        public ArrayList jiePai(int paiType, ArrayList upperCards, ArrayList myCards,
                                int upperPlayerPosition, int landlordPos)
        {
            if (myCards == null || myCards.Count == 0) return null;
            if (upperCards == null || upperCards.Count == 0) return null;

            // 分析手牌结构
            ArrayList basicCards = jiepai.basic(2, myCards);
            if (basicCards == null) return null;

            int[] singles = jiepai.mArrayToArgs((ArrayList)basicCards[0]);
            int[] pairs = jiepai.mArrayToArgs((ArrayList)basicCards[1]);
            int[] triples = jiepai.mArrayToArgs((ArrayList)basicCards[2]);
            int[] bombs = jiepai.mArrayToArgs((ArrayList)basicCards[3]);

            // 判断是否应该接牌
            bool shouldRespond = ShouldRespond(paiType, upperCards, myCards, upperPlayerPosition, landlordPos);

            if (!shouldRespond) return null;

            // 获取可以接的牌
            ArrayList basicResult = jiepai.isRight(paiType, upperCards, myCards);

            ArrayList result = null;

            // 根据牌型提取对应的牌
            if (paiType == (int)Guize.一张) // 单张
            {
                result = ExtractSingleSmart(basicResult, singles, myCards.Count);
            }
            else if (paiType == (int)Guize.对子) // 对子
            {
                result = ExtractPairSmart(basicResult, pairs, myCards.Count);
            }
            else if (paiType == (int)Guize.三不带) // 三张
            {
                result = ExtractTripleSmart(basicResult, triples, myCards.Count);
            }
            else if (paiType == (int)Guize.炸弹) // 炸弹
            {
                result = ExtractBomb(basicResult);
            }
            else if (paiType > 4 && paiType < 13) // 三带一、三带二、顺子、连对、飞机不带
            {
                result = ExtractComplex(basicResult);
            }
            else if (paiType > 12 && paiType < 20) // 四带二、四带两对、飞机带等
            {
                result = basicResult;
            }

            // 如果没有可接的牌，检查是否应该用炸弹
            if (result == null || result.Count == 0)
            {
                if (ShouldUseBomb(paiType, myCards, upperPlayerPosition, landlordPos))
                {
                    // 尝试用炸弹
                    ArrayList bomb = jiepai.findZhadan(myCards);
                    ArrayList rocket = jiepai.findTianzha(myCards);

                    if (rocket != null) return rocket;

                    if (bomb != null && bomb.Count > 0)
                    {
                        int[] bombCards = jiepai.mArrayToArgs(bomb);
                        if (bombCards != null && bombCards.Length > 0)
                        {
                            ArrayList bombResult = new ArrayList();
                            for (int i = 0; i < 4; i++) bombResult.Add(bombCards[0]);
                            return bombResult;
                        }
                    }
                }
                return null;
            }

            return result;
        }

        /// <summary>
        /// 智能提取单张牌 - 尽量用小牌接
        /// </summary>
        private ArrayList ExtractSingleSmart(ArrayList list, int[] singles, int totalCards)
        {
            if (list == null || list.Count < 4) return null;

            int[] jie = null;
            if (((ArrayList)list[0]).Count != 0) jie = jiepai.mArrayToArgs((ArrayList)list[0]);
            else if (((ArrayList)list[1]).Count != 0) jie = jiepai.mArrayToArgs((ArrayList)list[1]);
            else if (((ArrayList)list[2]).Count != 0) jie = jiepai.mArrayToArgs((ArrayList)list[2]);
            else if (((ArrayList)list[3]).Count != 0) jie = jiepai.mArrayToArgs((ArrayList)list[3]);

            if (jie != null && jie.Length > 0)
            {
                ArrayList result = new ArrayList();

                // 根据阶段选择策略
                int stage = GetGameStage(totalCards);

                if (stage == STAGE_EARLY || stage == STAGE_MID)
                {
                    // 早期和中期：尽量用小牌接
                    int smallCard = GetSmallCard(jie, CONTROL_CARD_THRESHOLD);
                    if (smallCard > 0)
                    {
                        result.Add(smallCard);
                        return result;
                    }
                }

                // 后期或没有小牌：用最小的能接的牌
                result.Add(jie[0]);
                return result;
            }
            return null;
        }

        /// <summary>
        /// 智能提取对子 - 尽量用小对子接
        /// </summary>
        private ArrayList ExtractPairSmart(ArrayList list, int[] pairs, int totalCards)
        {
            if (list == null || list.Count < 4) return null;

            int[] jie = null;
            if (((ArrayList)list[0]).Count != 0) jie = jiepai.mArrayToArgs((ArrayList)list[0]);
            else if (((ArrayList)list[1]).Count != 0) jie = jiepai.mArrayToArgs((ArrayList)list[1]);
            else if (((ArrayList)list[2]).Count != 0) jie = jiepai.mArrayToArgs((ArrayList)list[2]);

            if (jie != null && jie.Length > 0)
            {
                ArrayList result = new ArrayList();

                int stage = GetGameStage(totalCards);

                if (stage == STAGE_EARLY || stage == STAGE_MID)
                {
                    // 早期和中期：尽量用小对子接
                    int smallPair = GetSmallCard(jie, CONTROL_CARD_THRESHOLD);
                    if (smallPair > 0)
                    {
                        result.Add(smallPair);
                        result.Add(smallPair);
                        return result;
                    }
                }

                result.Add(jie[0]);
                result.Add(jie[0]);
                return result;
            }
            return null;
        }

        /// <summary>
        /// 智能提取三张
        /// </summary>
        private ArrayList ExtractTripleSmart(ArrayList list, int[] triples, int totalCards)
        {
            if (list == null || list.Count < 4) return null;

            int[] jie = null;
            if (((ArrayList)list[0]).Count != 0) jie = jiepai.mArrayToArgs((ArrayList)list[0]);
            else if (((ArrayList)list[1]).Count != 0) jie = jiepai.mArrayToArgs((ArrayList)list[1]);

            if (jie != null && jie.Length > 0)
            {
                ArrayList result = new ArrayList();

                int stage = GetGameStage(totalCards);

                if (stage == STAGE_EARLY || stage == STAGE_MID)
                {
                    int smallTriple = GetSmallCard(jie, 13); // Q以下
                    if (smallTriple > 0)
                    {
                        result.Add(smallTriple);
                        result.Add(smallTriple);
                        result.Add(smallTriple);
                        return result;
                    }
                }

                result.Add(jie[0]);
                result.Add(jie[0]);
                result.Add(jie[0]);
                return result;
            }
            return null;
        }

        /// <summary>
        /// 提取炸弹（当上手是炸弹时）
        /// </summary>
        private ArrayList ExtractBomb(ArrayList list)
        {
            if (list == null || list.Count == 0) return null;

            int[] jie = jiepai.mArrayToArgs(list);
            if (jie != null && jie.Length > 0)
            {
                ArrayList result = new ArrayList();
                result.Add(jie[0]);
                result.Add(jie[0]);
                result.Add(jie[0]);
                result.Add(jie[0]);
                return result;
            }
            return null;
        }

        /// <summary>
        /// 提取复杂牌型
        /// </summary>
        private ArrayList ExtractComplex(ArrayList list)
        {
            if (list == null || list.Count == 0) return null;

            if (list[0] is ArrayList)
            {
                return (ArrayList)list[0];
            }
            return list;
        }

        /// <summary>
        /// 判断是否应该接牌
        /// </summary>
        private bool ShouldRespond(int paiType, ArrayList upperCards, ArrayList myCards,
                                    int upperPlayerPosition, int landlordPos)
        {
            bool upperIsLandlord = (upperPlayerPosition == landlordPos);
            bool upperIsTeammate = (!upperIsLandlord && upperPlayerPosition != myPosition && !isLandlord);

            if (isLandlord)
            {
                // 地主必须接
                return true;
            }

            // 农民逻辑
            if (upperIsTeammate)
            {
                // 队友出的牌
                // 计算队友出了多少牌
                int teammatePlayedCount = upperCards.Count;

                // 如果队友出了很多牌（可能是大牌），可能要帮他跑
                // 或者随机决定是否接
                Random rd = new Random();
                return rd.Next(100) < 30; // 30%概率接队友的牌
            }

            // 地主出的牌，必须接
            return true;
        }

        /// <summary>
        /// 判断是否应该使用炸弹
        /// </summary>
        private bool ShouldUseBomb(int paiType, ArrayList myCards, int upperPlayerPosition, int landlordPos)
        {
            bool upperIsLandlord = (upperPlayerPosition == landlordPos);
            int stage = GetGameStage(myCards.Count);

            if (isLandlord)
            {
                // 地主：后期或牌少时炸
                return stage == STAGE_LATE || myCards.Count <= 5;
            }
            else
            {
                // 农民
                if (upperIsLandlord)
                {
                    // 地主出的牌
                    // 后期阶段必须炸
                    if (stage == STAGE_LATE) return true;

                    // 地主牌少时炸
                    return myCards.Count <= 6;
                }
                // 队友出的牌，不炸
                return false;
            }
        }
        #endregion

        #region 牌型识别方法
        /// <summary>
        /// 判断手牌中是否有飞机（连续的三张）
        /// 飞机：连续的2个或以上的三张，可以带单张或对子
        /// </summary>
        /// <param name="listPai">手牌</param>
        /// <returns>如果有飞机返回牌型信息，否则返回null</returns>
        public PlaneResult HasFeiji(ArrayList listPai)
        {
            if (listPai == null || listPai.Count < 6) return null;

            // 统计每个牌值的数量
            Dictionary<int, int> cardCounts = new Dictionary<int, int>();
            foreach (int card in listPai)
            {
                if (cardCounts.ContainsKey(card))
                    cardCounts[card]++;
                else
                    cardCounts[card] = 1;
            }

            // 找出所有三张
            ArrayList triples = new ArrayList();
            foreach (var kvp in cardCounts)
            {
                if (kvp.Value >= 3 && kvp.Key < 15) // 不包括2和王
                {
                    triples.Add(kvp.Key);
                }
            }

            if (triples.Count < 2) return null;

            triples.Sort();

            // 查找连续的三张
            int maxConsecutive = 1;
            int maxStartIndex = 0;
            int currentConsecutive = 1;
            int currentStart = 0;

            for (int i = 1; i < triples.Count; i++)
            {
                if ((int)triples[i] - (int)triples[i - 1] == 1)
                {
                    currentConsecutive++;
                    if (currentConsecutive > maxConsecutive)
                    {
                        maxConsecutive = currentConsecutive;
                        maxStartIndex = currentStart;
                    }
                }
                else
                {
                    currentConsecutive = 1;
                    currentStart = i;
                }
            }

            if (maxConsecutive >= 2)
            {
                PlaneResult result = new PlaneResult();
                result.Count = maxConsecutive;

                // 获取飞机的三张牌值
                for (int i = maxStartIndex; i < maxStartIndex + maxConsecutive; i++)
                {
                    result.CardValues.Add((int)triples[i]);
                }

                // 计算可以带的牌数量
                // 飞机可以不带、带单张、带对子
                int needWings = maxConsecutive; // 需要带的单张数量
                int needPairWings = maxConsecutive; // 需要带的对子数量

                // 统计可带的牌
                ArrayList availableSingles = new ArrayList();
                ArrayList availablePairs = new ArrayList();

                foreach (var kvp in cardCounts)
                {
                    // 排除已经作为飞机的牌
                    if (result.CardValues.Contains(kvp.Key)) continue;

                    if (kvp.Value >= 1 && kvp.Value <= 2)
                    {
                        availableSingles.Add(kvp.Key);
                    }
                    if (kvp.Value >= 2)
                    {
                        availablePairs.Add(kvp.Key);
                    }
                }

                result.CanPlayWithoutWings = true;
                result.CanPlayWithSingles = availableSingles.Count >= needWings;
                result.CanPlayWithPairs = availablePairs.Count >= needPairWings;

                return result;
            }

            return null;
        }

        /// <summary>
        /// 判断手牌中是否有顺子（连续的5张或以上单牌）
        /// 顺子：5张或以上连续的牌，不包括2和王
        /// </summary>
        /// <param name="listPai">手牌</param>
        /// <returns>如果有顺子返回牌型信息，否则返回null</returns>
        public StraightResult HasShunzi(ArrayList listPai)
        {
            if (listPai == null || listPai.Count < 5) return null;

            // 统计每个牌值的数量
            Dictionary<int, int> cardCounts = new Dictionary<int, int>();
            foreach (int card in listPai)
            {
                if (cardCounts.ContainsKey(card))
                    cardCounts[card]++;
                else
                    cardCounts[card] = 1;
            }

            // 找出所有可以作为顺子的牌（单张或对子都可以拆开用）
            ArrayList availableCards = new ArrayList();
            foreach (var kvp in cardCounts)
            {
                if (kvp.Key < 15) // 不包括2和王
                {
                    availableCards.Add(kvp.Key);
                }
            }

            if (availableCards.Count < 5) return null;

            availableCards.Sort();

            // 查找最长的连续序列
            int maxLength = 1;
            int maxStartIndex = 0;
            int currentLength = 1;
            int currentStart = 0;

            for (int i = 1; i < availableCards.Count; i++)
            {
                if ((int)availableCards[i] - (int)availableCards[i - 1] == 1)
                {
                    currentLength++;
                    if (currentLength > maxLength)
                    {
                        maxLength = currentLength;
                        maxStartIndex = currentStart;
                    }
                }
                else
                {
                    currentLength = 1;
                    currentStart = i;
                }
            }

            if (maxLength >= 5)
            {
                StraightResult result = new StraightResult();
                result.Length = maxLength;

                for (int i = maxStartIndex; i < maxStartIndex + maxLength; i++)
                {
                    result.CardValues.Add((int)availableCards[i]);
                }

                // 找出所有可能的顺子长度（5张、6张...）
                for (int len = 5; len <= maxLength; len++)
                {
                    result.PossibleLengths.Add(len);
                }

                return result;
            }

            return null;
        }

        /// <summary>
        /// 判断手牌中是否有连对（连续的3对或以上）
        /// 连对：3对或以上连续的对子，不包括2和王
        /// </summary>
        /// <param name="listPai">手牌</param>
        /// <returns>如果有连对返回牌型信息，否则返回null</returns>
        public DoubleStraightResult HasLiandui(ArrayList listPai)
        {
            if (listPai == null || listPai.Count < 6) return null;

            // 统计每个牌值的数量
            Dictionary<int, int> cardCounts = new Dictionary<int, int>();
            foreach (int card in listPai)
            {
                if (cardCounts.ContainsKey(card))
                    cardCounts[card]++;
                else
                    cardCounts[card] = 1;
            }

            // 找出所有对子
            ArrayList pairs = new ArrayList();
            foreach (var kvp in cardCounts)
            {
                if (kvp.Value >= 2 && kvp.Key < 15) // 不包括2和王
                {
                    pairs.Add(kvp.Key);
                }
            }

            if (pairs.Count < 3) return null;

            pairs.Sort();

            // 查找连续的对子
            int maxConsecutive = 1;
            int maxStartIndex = 0;
            int currentConsecutive = 1;
            int currentStart = 0;

            for (int i = 1; i < pairs.Count; i++)
            {
                if ((int)pairs[i] - (int)pairs[i - 1] == 1)
                {
                    currentConsecutive++;
                    if (currentConsecutive > maxConsecutive)
                    {
                        maxConsecutive = currentConsecutive;
                        maxStartIndex = currentStart;
                    }
                }
                else
                {
                    currentConsecutive = 1;
                    currentStart = i;
                }
            }

            if (maxConsecutive >= 3)
            {
                DoubleStraightResult result = new DoubleStraightResult();
                result.Count = maxConsecutive;

                for (int i = maxStartIndex; i < maxStartIndex + maxConsecutive; i++)
                {
                    result.CardValues.Add((int)pairs[i]);
                }

                return result;
            }

            return null;
        }

        /// <summary>
        /// 综合分析手牌中的所有组合牌
        /// </summary>
        /// <param name="listPai">手牌</param>
        /// <returns>包含所有组合牌信息的对象</returns>
        public ComboAnalysisResult AnalyzeCombos(ArrayList listPai)
        {
            ComboAnalysisResult result = new ComboAnalysisResult();

            // 检查飞机
            result.Plane = HasFeiji(listPai);

            // 检查顺子
            result.Straight = HasShunzi(listPai);

            // 检查连对
            result.DoubleStraight = HasLiandui(listPai);

            // 统计基础牌型
            ArrayList basicCards = jiepai.basic(2, listPai);
            if (basicCards != null)
            {
                result.Singles = jiepai.mArrayToArgs((ArrayList)basicCards[0]);
                result.Pairs = jiepai.mArrayToArgs((ArrayList)basicCards[1]);
                result.Triples = jiepai.mArrayToArgs((ArrayList)basicCards[2]);
                result.Bombs = jiepai.mArrayToArgs((ArrayList)basicCards[3]);
            }

            return result;
        }

        /// <summary>
        /// 根据手牌选择最佳的出牌组合
        /// </summary>
        /// <param name="listPai">手牌</param>
        /// <returns>推荐出的牌</returns>
        public ArrayList GetBestCombo(ArrayList listPai)
        {
            if (listPai == null || listPai.Count == 0) return null;

            Chupai chupai = new Chupai();

            // 如果只剩一手牌，直接出
            if (chupai.isRight(listPai)) return listPai;

            // 分析组合牌
            ComboAnalysisResult analysis = AnalyzeCombos(listPai);

            ArrayList result = new ArrayList();

            // 优先级：飞机 > 连对 > 顺子（飞机消耗牌最多）

            // 1. 检查是否可以出飞机
            if (analysis.Plane != null)
            {
                PlaneResult plane = analysis.Plane;

                // 添加飞机的三张
                foreach (int card in plane.CardValues)
                {
                    result.Add(card);
                    result.Add(card);
                    result.Add(card);
                }

                // 如果可以带对子，优先带对子
                if (plane.CanPlayWithPairs && analysis.Pairs != null)
                {
                    int needPairs = plane.Count;
                    int addedPairs = 0;
                    foreach (int card in analysis.Pairs)
                    {
                        if (!plane.CardValues.Contains(card) && addedPairs < needPairs)
                        {
                            result.Add(card);
                            result.Add(card);
                            addedPairs++;
                        }
                    }
                }
                // 带单张
                else if (plane.CanPlayWithSingles && analysis.Singles != null)
                {
                    int needSingles = plane.Count;
                    int addedSingles = 0;
                    foreach (int card in analysis.Singles)
                    {
                        if (!plane.CardValues.Contains(card) && addedSingles < needSingles)
                        {
                            result.Add(card);
                            addedSingles++;
                        }
                    }
                }

                if (chupai.isRight(result)) return result;
                result.Clear();
            }

            // 2. 检查是否可以出连对
            if (analysis.DoubleStraight != null)
            {
                DoubleStraightResult doubleStraight = analysis.DoubleStraight;

                foreach (int card in doubleStraight.CardValues)
                {
                    result.Add(card);
                    result.Add(card);
                }

                if (chupai.isRight(result)) return result;
                result.Clear();
            }

            // 3. 检查是否可以出顺子
            if (analysis.Straight != null)
            {
                StraightResult straight = analysis.Straight;

                // 出最短的顺子（5张）
                int len = Math.Min(5, straight.Length);
                for (int i = 0; i < len; i++)
                {
                    result.Add(straight.CardValues[i]);
                }

                if (chupai.isRight(result)) return result;
                result.Clear();
            }

            return null;
        }
        #endregion
    }

    #region 牌型识别结果类
    /// <summary>
    /// 飞机识别结果
    /// </summary>
    public class PlaneResult
    {
        /// <summary>
        /// 飞机的三张数量（2个三张=2，3个三张=3...）
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 飞机的牌值列表
        /// </summary>
        public ArrayList CardValues { get; set; } = new ArrayList();

        /// <summary>
        /// 是否可以不带牌
        /// </summary>
        public bool CanPlayWithoutWings { get; set; }

        /// <summary>
        /// 是否可以带单张
        /// </summary>
        public bool CanPlayWithSingles { get; set; }

        /// <summary>
        /// 是否可以带对子
        /// </summary>
        public bool CanPlayWithPairs { get; set; }
    }

    /// <summary>
    /// 顺子识别结果
    /// </summary>
    public class StraightResult
    {
        /// <summary>
        /// 顺子长度
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// 顺子的牌值列表
        /// </summary>
        public ArrayList CardValues { get; set; } = new ArrayList();

        /// <summary>
        /// 可能的顺子长度列表（5、6、7...）
        /// </summary>
        public ArrayList PossibleLengths { get; set; } = new ArrayList();
    }

    /// <summary>
    /// 连对识别结果
    /// </summary>
    public class DoubleStraightResult
    {
        /// <summary>
        /// 连对的对数（3对=3，4对=4...）
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 连对的牌值列表
        /// </summary>
        public ArrayList CardValues { get; set; } = new ArrayList();
    }

    /// <summary>
    /// 组合牌分析结果
    /// </summary>
    public class ComboAnalysisResult
    {
        /// <summary>
        /// 飞机信息
        /// </summary>
        public PlaneResult Plane { get; set; }

        /// <summary>
        /// 顺子信息
        /// </summary>
        public StraightResult Straight { get; set; }

        /// <summary>
        /// 连对信息
        /// </summary>
        public DoubleStraightResult DoubleStraight { get; set; }

        /// <summary>
        /// 单张列表
        /// </summary>
        public int[] Singles { get; set; }

        /// <summary>
        /// 对子列表
        /// </summary>
        public int[] Pairs { get; set; }

        /// <summary>
        /// 三张列表
        /// </summary>
        public int[] Triples { get; set; }

        /// <summary>
        /// 炸弹列表
        /// </summary>
        public int[] Bombs { get; set; }

        /// <summary>
        /// 是否有任何组合牌
        /// </summary>
        public bool HasAnyCombo
        {
            get { return Plane != null || Straight != null || DoubleStraight != null; }
        }
    }
    #endregion
}