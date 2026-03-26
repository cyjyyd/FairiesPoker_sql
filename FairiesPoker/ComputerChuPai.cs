using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace FairiesPoker
{
    /// <summary>
    /// 电脑出牌类 - 增强版AI
    /// </summary>
    class ComputerChuPai
    {
        private AIPlayer aiPlayer;
        private Jiepai jiepai;
        private int myPosition;
        private bool isLandlord;
        private int landlordPosition;

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

        #region 出牌（主动出牌）
        /// <summary>
        /// 出牌 - 增强版
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

            int leave = listPai.Count;
            ArrayList list1 = new ArrayList();

            // 策略选择
            int handStrength = EvaluateHandStrength(listPai);
            bool useAggressiveStrategy = ShouldPlayAggressive(listPai, handStrength);

            // 如果只剩一手牌
            Chupai chupai = new Chupai();
            if (chupai.isRight(listPai))
            {
                return listPai;
            }

            // 根据策略出牌
            if (useAggressiveStrategy)
            {
                return PlayAggressive(listPai, singles, pairs, triples, bombs);
            }
            else
            {
                return PlayStrategic(listPai, singles, pairs, triples, bombs, handStrength);
            }
        }

        /// <summary>
        /// 评估手牌强度
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

        /// <summary>
        /// 判断是否应该激进出牌
        /// </summary>
        private bool ShouldPlayAggressive(ArrayList cards, int handStrength)
        {
            if (isLandlord)
            {
                // 地主且牌好或牌少时激进
                return handStrength > 50 || cards.Count <= 8;
            }
            else
            {
                // 农民牌少时激进（准备跑牌）
                return cards.Count <= 6;
            }
        }

        /// <summary>
        /// 激进出牌（先出大牌）
        /// </summary>
        private ArrayList PlayAggressive(ArrayList listPai, int[] singles, int[] pairs, int[] triples, int[] bombs)
        {
            ArrayList result = new ArrayList();
            int leave = listPai.Count;

            // 优先处理炸弹
            if (bombs != null && bombs.Length > 0)
            {
                if (leave == 4)
                {
                    int card = bombs[0];
                    for (int i = 0; i < 4; i++) result.Add(card);
                    return result;
                }

                // 有其他牌时，炸弹带牌
                if (leave > 4 && (singles != null || pairs != null))
                {
                    int card = bombs[bombs.Length - 1]; // 用最大的炸弹
                    for (int i = 0; i < 4; i++) result.Add(card);

                    // 带单张或对子
                    if (pairs != null && pairs.Length >= 2)
                    {
                        result.Add(pairs[0]); result.Add(pairs[0]);
                        result.Add(pairs[1]); result.Add(pairs[1]);
                    }
                    else if (singles != null && singles.Length >= 2)
                    {
                        result.Add(singles[0]);
                        result.Add(singles[1]);
                    }
                    else if (pairs != null)
                    {
                        result.Add(pairs[0]); result.Add(pairs[0]);
                    }
                    else if (singles != null)
                    {
                        result.Add(singles[0]);
                    }
                    return result;
                }
            }

            // 处理三张
            if (triples != null && triples.Length > 0)
            {
                if (leave == 3)
                {
                    for (int i = 0; i < 3; i++) result.Add(triples[0]);
                    return result;
                }
                if (leave == 4 || leave == 5)
                {
                    int card = triples[triples.Length - 1]; // 最大的三张
                    for (int i = 0; i < 3; i++) result.Add(card);
                    if (singles != null) result.Add(singles[singles.Length - 1]);
                    else if (pairs != null)
                    {
                        result.Add(pairs[0]);
                        result.Add(pairs[0]);
                    }
                    return result;
                }

                // 三带一或三带二
                int tripleCard = triples[triples.Length - 1];
                for (int i = 0; i < 3; i++) result.Add(tripleCard);

                if (pairs != null && pairs.Length > 0)
                {
                    result.Add(pairs[0]);
                    result.Add(pairs[0]);
                }
                else if (singles != null && singles.Length > 0)
                {
                    result.Add(singles[0]);
                }
                return result;
            }

            // 处理对子
            if (pairs != null && pairs.Length > 0)
            {
                if (leave == 2)
                {
                    result.Add(pairs[0]);
                    result.Add(pairs[0]);
                    return result;
                }

                // 出较大的对子
                int pairIndex = Math.Max(0, pairs.Length - 1);
                int card = pairs[pairIndex];
                result.Add(card);
                result.Add(card);
                return result;
            }

            // 处理单张
            if (singles != null && singles.Length > 0)
            {
                if (leave == 1)
                {
                    result.Add(singles[0]);
                    return result;
                }

                // 出较大的单张
                int index = Math.Max(0, singles.Length - 1);
                result.Add(singles[index]);
                return result;
            }

            // 只剩炸弹
            if (bombs != null && bombs.Length > 0)
            {
                int card = bombs[0];
                for (int i = 0; i < 4; i++) result.Add(card);
                return result;
            }

            return listPai;
        }

        /// <summary>
        /// 策略出牌（综合考虑）
        /// </summary>
        private ArrayList PlayStrategic(ArrayList listPai, int[] singles, int[] pairs, int[] triples, int[] bombs, int handStrength)
        {
            ArrayList result = new ArrayList();
            int leave = listPai.Count;

            // 如果是农民，需要考虑配合
            if (!isLandlord)
            {
                return PlayAsFarmer(listPai, singles, pairs, triples, bombs, handStrength);
            }

            // 地主策略
            return PlayAsLandlord(listPai, singles, pairs, triples, bombs, handStrength);
        }

        /// <summary>
        /// 农民出牌策略
        /// </summary>
        private ArrayList PlayAsFarmer(ArrayList listPai, int[] singles, int[] pairs, int[] triples, int[] bombs, int handStrength)
        {
            ArrayList result = new ArrayList();
            int leave = listPai.Count;

            // 手牌很好时，激进出牌准备跑
            if (handStrength > 60 || leave <= 5)
            {
                return PlayAggressive(listPai, singles, pairs, triples, bombs);
            }

            // 正常出牌策略：先出小牌，保留大牌顶地主

            // 优先出小的顺子、连对
            ArrayList straightResult = TryPlayStraight(listPai, singles, pairs);
            if (straightResult != null) return straightResult;

            // 出小对子
            if (pairs != null && pairs.Length > 0)
            {
                // 出小对子
                if (pairs[0] < 13) // 不出K以上的对子
                {
                    result.Add(pairs[0]);
                    result.Add(pairs[0]);
                    return result;
                }
            }

            // 出小三张（带牌）
            if (triples != null && triples.Length > 0)
            {
                int card = triples[0]; // 最小的三张
                if (card < 12) // 不出Q以上的三张
                {
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
            }

            // 出小单张
            if (singles != null && singles.Length > 0)
            {
                // 找一张小牌出
                for (int i = 0; i < singles.Length; i++)
                {
                    if (singles[i] < 14) // 不出A以上的牌
                    {
                        result.Add(singles[i]);
                        return result;
                    }
                }
                // 只剩大牌，出最小的
                result.Add(singles[0]);
                return result;
            }

            return PlayAggressive(listPai, singles, pairs, triples, bombs);
        }

        /// <summary>
        /// 地主出牌策略
        /// </summary>
        private ArrayList PlayAsLandlord(ArrayList listPai, int[] singles, int[] pairs, int[] triples, int[] bombs, int handStrength)
        {
            ArrayList result = new ArrayList();
            int leave = listPai.Count;

            // 牌好或牌少时激进
            if (handStrength > 50 || leave <= 8)
            {
                return PlayAggressive(listPai, singles, pairs, triples, bombs);
            }

            // 正常策略

            // 尝试出顺子、连对
            ArrayList straightResult = TryPlayStraight(listPai, singles, pairs);
            if (straightResult != null) return straightResult;

            // 出三带
            if (triples != null && triples.Length > 0)
            {
                int card = triples[0];
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

            return PlayAggressive(listPai, singles, pairs, triples, bombs);
        }

        /// <summary>
        /// 尝试出顺子或连对
        /// </summary>
        private ArrayList TryPlayStraight(ArrayList listPai, int[] singles, int[] pairs)
        {
            // 简化实现：尝试找顺子
            if (singles != null && singles.Length >= 5)
            {
                ArrayList result = new ArrayList();
                // 检查是否有连续的牌
                int consecutive = 1;
                int startIdx = 0;

                for (int i = 0; i < singles.Length - 1; i++)
                {
                    if (singles[i] == singles[i + 1] + 1 && singles[i] < 15)
                    {
                        consecutive++;
                        if (consecutive >= 5)
                        {
                            startIdx = i - 3;
                            break;
                        }
                    }
                    else
                    {
                        consecutive = 1;
                    }
                }

                if (consecutive >= 5)
                {
                    for (int i = startIdx; i < startIdx + 5; i++)
                    {
                        if (i >= 0 && i < singles.Length)
                            result.Add(singles[i]);
                    }
                    if (result.Count >= 5)
                    {
                        Chupai chupai = new Chupai();
                        if (chupai.isRight(result))
                            return result;
                    }
                }
            }

            return null;
        }
        #endregion

        #region 接牌（被动出牌）
        /// <summary>
        /// 接牌 - 增强版
        /// </summary>
        public ArrayList jiePai(int paiType, ArrayList upperCards, ArrayList myCards,
                                int upperPlayerPosition, int landlordPos)
        {
            if (myCards == null || myCards.Count == 0) return null;
            if (upperCards == null || upperCards.Count == 0) return null;

            // 判断是否应该接牌
            bool shouldRespond = ShouldRespond(paiType, upperCards, myCards, upperPlayerPosition, landlordPos);

            if (!shouldRespond) return null;

            // 获取可以接的牌
            ArrayList basicResult = jiepai.isRight(paiType, upperCards, myCards);

            ArrayList result = null;

            // 根据牌型提取对应的牌（完全复制原tiShiJiePai的逻辑）
            if (paiType == (int)Guize.一张) // 单张
            {
                result = ExtractSingle(basicResult);
            }
            else if (paiType == (int)Guize.对子) // 对子
            {
                result = ExtractPair(basicResult);
            }
            else if (paiType == (int)Guize.三不带) // 三张
            {
                result = ExtractTriple(basicResult);
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
        /// 提取单张牌
        /// </summary>
        private ArrayList ExtractSingle(ArrayList list)
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
                result.Add(jie[0]); // 取最小的一个牌值
                return result;
            }
            return null;
        }

        /// <summary>
        /// 提取对子
        /// </summary>
        private ArrayList ExtractPair(ArrayList list)
        {
            if (list == null || list.Count < 4) return null;

            int[] jie = null;
            if (((ArrayList)list[0]).Count != 0) jie = jiepai.mArrayToArgs((ArrayList)list[0]);
            else if (((ArrayList)list[1]).Count != 0) jie = jiepai.mArrayToArgs((ArrayList)list[1]);
            else if (((ArrayList)list[2]).Count != 0) jie = jiepai.mArrayToArgs((ArrayList)list[2]);

            if (jie != null && jie.Length > 0)
            {
                ArrayList result = new ArrayList();
                result.Add(jie[0]);
                result.Add(jie[0]); // 同一个牌值重复两次
                return result;
            }
            return null;
        }

        /// <summary>
        /// 提取三张
        /// </summary>
        private ArrayList ExtractTriple(ArrayList list)
        {
            if (list == null || list.Count < 4) return null;

            int[] jie = null;
            if (((ArrayList)list[0]).Count != 0) jie = jiepai.mArrayToArgs((ArrayList)list[0]);
            else if (((ArrayList)list[1]).Count != 0) jie = jiepai.mArrayToArgs((ArrayList)list[1]);

            if (jie != null && jie.Length > 0)
            {
                ArrayList result = new ArrayList();
                result.Add(jie[0]);
                result.Add(jie[0]);
                result.Add(jie[0]); // 同一个牌值重复三次
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
                result.Add(jie[0]); // 同一个牌值重复四次
                return result;
            }
            return null;
        }

        /// <summary>
        /// 提取复杂牌型（三带一、三带二、顺子、连对、飞机不带）
        /// </summary>
        private ArrayList ExtractComplex(ArrayList list)
        {
            if (list == null || list.Count == 0) return null;

            // 取第一个选项
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
            bool upperIsTeammate = (!upperIsLandlord && upperPlayerPosition != myPosition);

            if (isLandlord)
            {
                // 地主必须接
                return true;
            }

            // 农民逻辑
            if (upperIsTeammate)
            {
                // 队友出的牌，考虑是否要接
                // 如果队友牌少，让他跑
                // 这里简化处理：随机决定是否接队友的牌
                Random rd = new Random();
                return rd.Next(100) < 40; // 40%概率接队友的牌
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

            if (isLandlord)
            {
                // 地主：牌少就炸
                return myCards.Count <= 6;
            }
            else
            {
                // 农民
                if (upperIsLandlord)
                {
                    // 地主出的牌，如果他牌少就炸
                    return myCards.Count <= 8;
                }
                // 队友出的牌，不炸
                return false;
            }
        }
        #endregion

        #region 未实现的方法（保留原接口）
        private void feiji()//判断是否有飞机
        {

        }
        private void shunzi()//判断是否有顺子
        {

        }
        private void liandui()//判断是否有连对
        {

        }
        #endregion
    }
}