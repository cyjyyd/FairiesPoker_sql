using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using FairiesPoker;

namespace FairiesPoker
{
    #region 接牌类
    class Jiepai
    {
        Chupai chupai = new Chupai();

        #region 外部调用方法,判断能否接牌
        public bool isRight( ArrayList upPai, ArrayList nextPai,int paiType)
        {
            chupai.PaiType = 0;
            if (chupai.isRight(nextPai))
            {
                if (paiType != (int)Guize.炸弹 && chupai.PaiType == (int)Guize.炸弹 
                   || chupai.PaiType == (int)Guize.天炸) return true;
                if (sameType(paiType, chupai.PaiType, upPai, nextPai)) return true;
            }
            return false;
        }
        #endregion 

        #region 外部调用方法,提示出牌
        public ArrayList isRight(int paiType, ArrayList upPai, ArrayList nextPai)
        {
            int leave = nextPai.Count;
            chupai.PaiType = 0;
            chupai.isRight(nextPai);
            #region 剩余牌的数量小于或等于上手出牌的数量
            if (leave <= upPai.Count)
            {
                ArrayList list = new ArrayList();
                ArrayList list1 = findZha(leave, nextPai);
                if (leave < upPai.Count)
                {
                    if (list1 != null) list = list1;
                }
                else
                {
                    if (sameType(paiType, chupai.PaiType, upPai, nextPai)) list.Add(nextPai);
                    else if (list1 != null) list = list1;
                }
                if (list.Count != 0) return list;
                return null;
            }
            #endregion
            #region 剩余牌的数量大于上手出牌的数量
            else
            {
                int size = upBigPai(paiType, upPai);
                ArrayList list = new ArrayList();
                ArrayList list1 = basic(size, nextPai);
                if (list1 == null) return null;
                switch (paiType)
                {
                    case 1: list = list1; break;
                    case 2: list.Add(list1[1]); list.Add(list1[2]); list.Add(list1[3]); break;
                    case 3: list.Add(list1[2]); list.Add(list1[3]); break;
                    case 4: list = (ArrayList)list1[3]; break;
                    case 5: list = shunzi(paiType, upPai, allDanzhang(size,nextPai)); break;
                    case 6: list = liandui(paiType, upPai, allDuizi(size, nextPai)); break;
                }
                if (paiType > 6 && paiType < 11)   list = findFeiji(size, paiType, nextPai);
                if (paiType > 10 && paiType < 20)  list = fly_n(size, paiType, nextPai);
                return list;
            }
            #endregion
        }
        #endregion

        #region 辅助方法 （含部分算法）
        #region 判断与上手牌是否为同类型牌，并且大于上手牌
        private bool sameType(int paiType, int type, ArrayList upPai, ArrayList nextPai)
        {
            if (paiType == type)
            {
                int[] args = arrayToArgs(upPai);
                int[] next = arrayToArgs(nextPai);
                if (type > 0 && type < 11 && next[0] > args[0]) return true;
                if (type > 10 && type < 15 && next[2] > args[2]) return true;
                if (type > 14 && type < 18 && next[4] > args[4]) return true;
                if (type == 18 && next[6] > args[6]) return true;
                if (type == 19)
                {
                    ArrayList upList = findZhadan(upPai);
                    ArrayList nextList = findZhadan(nextPai);
                    if ((int)nextList[nextList.Count - 1] > (int)upList[upList.Count - 1]) return true;
                }
            }
            return false;
        }
        #endregion
        #region 将集合转换成数组并排序(从大到小)
        public int[] arrayToArgs(ArrayList list)
        {
            if (list != null && list.Count != 0)
            {
                int[] args = new int[list.Count];
                for (int i = 0; i < args.Length; i++)
                {
                    args[i] = (int)list[i];
                }
                chupai.format(args);
                return args;
            }
            return null;
        }
        #endregion
        #region 将集合转换成数组并排序(从小到大)
        public int[] mArrayToArgs(ArrayList list)
        {
            int[] args = arrayToArgs(list);
            if (args != null)
            {
                chupai.minToBig(args); return args;
            }
            return null;
        }
        #endregion
        #region 判断是否为飞机
        private bool isFeiji(ArrayList list)
        {
            int[] args = arrayToArgs(list);
            if (args[0] > 14) return false;
            for (int i = 0; i < args.Length; i += 3)
            {
                if (args[i] != args[i + 1] && args[i + 1] != args[i + 2]) return false;
            }
            for (int i = 2; i < args.Length - 1; i += 3)
            {
                if (args[i] - 1 != args[i + 1]) return false;
            }
            return true;
        }
        #endregion
        #region 获取上手出的牌的最大值
        private int upBigPai(int type, ArrayList upPai)
        {
            int[] args = mArrayToArgs(upPai);
            int size = 18;
            if (type > 0 && type < 11) size = args[0];
            else if (type > 10 && type < 15) size = args[2];
            else if (type > 14 && type < 18) size = args[4];
            else if (type == 18) size = args[6];
            else if (type == 19)
            {
                ArrayList upList = findZhadan(upPai);
                size = (int)upList[upList.Count - 1];
            }
            return size;
        }
        #endregion
        #region 获取上手出的最大牌的下标
        private int findBigSub(int size, int[] next)
        {
            if (size < next[next.Length - 1])
            {
                for (int i = 0; i < next.Length; i++)
                {
                    if (size < next[i]) return i;
                }
            }
            return -1;
        }
        #endregion
        #region 排除不能组合成顺子或连对的牌
        public ArrayList removeBigPai(int size, ArrayList nextPai)
        {
            if (nextPai.Count != 0)
            {
                int[] next = arrayToArgs(nextPai);
                ArrayList list = new ArrayList();
                for (int i = 0; i < next.Length; i++)
                {
                    if (next[i] < 15) list.Add(next[i]);
                }
                return list;
            }
            return nextPai;
        }
        #endregion
        #region 扫描当前牌中所有可以组合顺子的单张，如重复，只取一张
        private ArrayList allDanzhang(int size, ArrayList nextPai)
        {
            ArrayList list = removeBigPai(size, nextPai);
            int[] args = mArrayToArgs(list);
            int sub = findBigSub(size, args);
            list.Clear();
            if (sub != -1)
            {
                for (int i = sub; i < args.Length - 1; i++)
                {
                    if (args[i] != args[i + 1]) list.Add(args[i]);
                    if (i == args.Length - 2) list.Add(args[i + 1]);
                }
                return list;
            }
            return null;
        }
        #endregion
        #region 扫描当前牌中所有能组合连对的对子，如有三张和四张，只取二张
        private ArrayList allDuizi(int size, ArrayList nextPai)
        {
            ArrayList list = removeBigPai(size, nextPai);
            int[] args = mArrayToArgs(list);
            int sub = findBigSub(size, args);
            list.Clear();
            if (sub != -1)
            {
                for (int i = sub; i < args.Length - 1; i++)
                {
                    if (args[i] == args[i + 1])
                    {
                        list.Add(args[i]); i++;
                    }
                }
                return allDanzhang(size, list);
            }
            return null;
        }
        #endregion
        
        #endregion

        #region 主要方法 （返回所有符合要求的牌）
        #region 搜索天炸
        public ArrayList findTianzha(ArrayList nextPai)
        {
            if (nextPai.Count > 1)
            {
                int[] args = arrayToArgs(nextPai);
                ArrayList list = new ArrayList();
                if (args[0] == 17 && args[1] == 16)
                {
                    list.Add(args[0]); 
                    list.Add(args[1]); 
                    return list;
                }
            }
            return null;
        }
        #endregion
        #region 搜索炸弹
        public ArrayList findZhadan(ArrayList nextPai)
        {
            int[] args = arrayToArgs(nextPai);
            ArrayList list = new ArrayList();
            ArrayList list1 = new ArrayList();
            chupai.minToBig(args);
            for (int i = 0; i < args.Length - 3; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    list1.Add(args[j + i]);
                }
                if (chupai.isRight(list1) && chupai.PaiType == (int)Guize.炸弹) list.Add(args[i]);
                list1.Clear();
            }
            if (list.Count != 0) return list;
            return null;
        }
        #endregion 
        #region 搜索炸弹和天炸
        public ArrayList findZha(int leave, ArrayList nextPai)
        {
            if (leave == 1) return null;
            ArrayList list = new ArrayList();
            if (findZhadan(nextPai) != null) list.Add(findZhadan(nextPai));
            if (findTianzha(nextPai) != null) list.Add(findTianzha(nextPai));
            if(list.Count != 0) return list;
            return null;
        }
        #endregion
        #region 搜索所有大于上手牌的顺子
        private ArrayList shunzi(int paiType,ArrayList upPai,ArrayList nextPai)
        {
            ArrayList list = new ArrayList();
            if (nextPai.Count > upPai.Count)
            {
                ArrayList list1 = new ArrayList();
                ArrayList list2 = new ArrayList();
                for (int i = 0; i < nextPai.Count - upPai.Count + 1; i++)
                {
                    for (int j = 0; j < upPai.Count; j++)
                    {
                        list2.Add((int)nextPai[j + i]);
                    }
                    if (chupai.isRight(list2) && chupai.PaiType == (int)Guize.顺子) list1.Add(list2.Clone());
                    list2.Clear();
                }
                for(int i=0;i<list1.Count;i++)
                {
                    if (sameType(paiType, chupai.PaiType, upPai, (ArrayList)list1[i])) list.Add(list1[i]);
                }
                return list;
            }
            else if (nextPai.Count == upPai.Count)
            {
                if (chupai.isRight(nextPai) && chupai.PaiType==(int)Guize.顺子)
                {
                    if (sameType(paiType, chupai.PaiType, upPai, nextPai))
                    {
                        list.Add(nextPai); return list;
                    }
                }
            }
            return null;
        }
        #endregion
        #region 搜索所有大于上手牌的连对
        private ArrayList liandui(int paiType, ArrayList upPai, ArrayList nextPai)
        {
            ArrayList list = new ArrayList();
            ArrayList list1 = new ArrayList();
            int upLen = upPai.Count / 2;
            if (nextPai.Count > upLen)
            {
                ArrayList list2 = new ArrayList();
                for (int i = 0; i < nextPai.Count - upLen + 1; i++)
                {
                    for (int j = 0; j < upLen; j++)
                    {
                        list2.Add((int)nextPai[j + i]); list2.Add((int)nextPai[j + i]);
                    }
                    if (chupai.isRight(list2) && chupai.PaiType==(int)Guize.连对) list1.Add(list2.Clone());
                    list2.Clear();
                }
                for (int i = 0; i < list1.Count; i++)
                {
                    if (sameType(paiType, chupai.PaiType, upPai,(ArrayList)list1[i])) list.Add(list1[i]);
                }
                return list;
            }
            else if (nextPai.Count == upLen)
            {
                for (int i = 0; i < nextPai.Count; i++)
                {
                    list1.Add((int)nextPai[i]); list1.Add((int)nextPai[i]);
                }
                nextPai = list1;
                if (chupai.isRight(nextPai) && chupai.PaiType == (int)Guize.连对)
                {
                    if (sameType(paiType, chupai.PaiType, upPai, nextPai))
                    {
                        list.Add(nextPai); return list;
                    }
                }
            }
            return null;
        }
        #endregion
        #region 搜索所有大于上手牌的飞机
        private ArrayList findFeiji(int size,int paiType,ArrayList nextPai)
        {
            ArrayList list = basic(size, nextPai);
            if (list != null)
            {
                ArrayList list1 = new ArrayList();
                int[] a = arrayToArgs((ArrayList)list[2]);
                int[] b = arrayToArgs((ArrayList)list[3]);
                list.Clear();
                if (a != null)
                {
                    for (int i = 0; i < a.Length; i++)
                    {
                        list1.Add(a[i]);
                    }
                }
                if (b != null)
                {
                    for (int i = 0; i < b.Length; i++)
                    {
                        list1.Add(b[i]);
                    }
                }
                int[] args = mArrayToArgs(list1);
                if (args == null) return null;
                list1.Clear();
                int len = args.Length;
                paiType -= 5;
                if (len > 1 && len >= paiType)
                {
                    for (int i = 0; i < len - paiType + 1; i++)
                    {
                        for (int j = 0; j < paiType; j++)
                        {
                            list1.Add(args[j + i]); list1.Add(args[j + i]); list1.Add(args[j + i]);
                        }
                        if (isFeiji(list1)) list.Add(list1.Clone());
                        list1.Clear();
                    }
                    if(list.Count != 0)  return list;
                    return null;
                }
            }
            return null;
        }
        #endregion
        #region 搜索所有大于上手的三张或飞机带牌
        private ArrayList fly_n(int size, int paiType, ArrayList nextPai)
        {
            ArrayList list = basic(2, nextPai);
            int[] a = mArrayToArgs((ArrayList)list[0]);
            int[] b = mArrayToArgs((ArrayList)list[1]);
            list = basic(size, nextPai);
            int[] c = mArrayToArgs((ArrayList)list[2]);
            int[] d = mArrayToArgs((ArrayList)list[3]);
            ArrayList list1 = new ArrayList();
            ArrayList list2 = new ArrayList();
            #region 三带一、三带二
            if ((paiType == (int)Guize.三带一 || paiType == (int)Guize.三带二) && c != null)
            {
                for (int i = 0; i < c.Length; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        list1.Add(c[i]);
                    }
                    #region 三带一
                    if (paiType == (int)Guize.三带一)
                    {
                        if (a != null) list1.Add(a[0]);
                        else if (b != null) list1.Add(b[0]);
                        else if (c.Length > 1)
                        {
                            if (i == 0) list1.Add(c[1]);
                            else list1.Add(c[0]);
                        }
                        if (list1.Count == 4) list2.Add(list1.Clone());
                    }
                    #endregion
                    #region 三带二
                    else if (paiType == (int)Guize.三带二)
                    {
                        if (b != null)
                        {
                            list1.Add(b[0]); list1.Add(b[0]);
                        }
                        else if (c.Length > 1)
                        {
                            if (i == 0)
                            {
                                list1.Add(c[1]); list1.Add(c[1]);
                            }
                            else
                            {
                                list1.Add(c[0]); list1.Add(c[0]);
                            }
                        }
                        if (list1.Count == 5) list2.Add(list1.Clone());
                    }
                    #endregion 
                    list1.Clear();
                }
                if (list2.Count != 0) return list2;
            }
            #endregion
            #region 四带二、四带两对
            else if ((paiType == (int)Guize.四带二 || paiType == (int)Guize.四带二对) && d != null)
            {
                for (int j = 0; j < 4; j++)
                {
                    list1.Add(d[0]);
                }
                #region 四带二
                if (paiType == (int)Guize.四带二)
                {
                    if (a != null && a.Length > 1)
                    {
                        list1.Add(a[0]); list1.Add(a[1]);
                    }
                    else if (b != null)
                    {
                        list1.Add(b[0]); list1.Add(b[0]);
                    }
                    if (list1.Count == 6) return list1;
                }
                #endregion
                #region 四带二对
                else if (paiType == (int)Guize.四带二对)
                {
                    if (b != null && b.Length > 1)
                    {
                        list1.Add(b[0]); list1.Add(b[0]); list1.Add(b[1]); list1.Add(b[1]);
                    }
                    else if (b != null && c != null)
                    {
                        list1.Add(b[0]); list1.Add(b[0]); list1.Add(c[0]); list1.Add(c[0]);
                    }
                    if (list1.Count == 8) return list1;
                }
                #endregion
            }
            #endregion
            #region 飞机带二、飞机带二对
            else if (paiType == (int)Guize.飞机带二 || paiType == (int)Guize.飞机带二对)
            {
                list = findFeiji(size,(int)Guize.飞机不带,nextPai);
                if (list != null)
                {
                    list1 = (ArrayList)list[0];
                    #region 飞机带二
                    if (paiType == (int)Guize.飞机带二)
                    {
                        if (a != null && a.Length > 1)
                        {
                            list1.Add(a[0]); list1.Add(a[1]);
                        }
                        else if (b != null)
                        {
                            list1.Add(b[0]); list1.Add(b[0]);
                        }
                        else if (c.Length > 2)
                        {
                            for (int i = 0; i < c.Length; i++)
                            {
                                if ((int)list1[0] != c[i] && (int)list1[list1.Count - 1] != c[i])
                                {
                                    list1.Add(c[i]); list1.Add(c[i]); break;
                                }
                            }
                        }
                        if (list1.Count == 8) return list1;
                    }
                    #endregion
                    #region 飞机带二对
                    else if (paiType == (int)Guize.飞机带二对)
                    {
                        if (b != null && b.Length > 1)
                        {
                            list1.Add(b[0]); list1.Add(b[0]); list1.Add(b[1]); list1.Add(b[1]);
                        }
                        else if (c.Length > 2)
                        {
                            for (int i = 0; i < c.Length; i++)
                            {
                                if ((int)list1[0] != c[i] && (int)list1[list1.Count - 1] != c[i])
                                {
                                    list1.Add(c[i]); list1.Add(c[i]); break;
                                }
                            }
                            if (b != null)
                            {
                                list1.Add(b[0]); list1.Add(b[0]);
                            }
                        }
                        if (list1.Count == 10) return list1;
                    }
                    #endregion
                }
            }
            #endregion
            #region 三飞机带三、三飞机带三对
            else if (paiType == (int)Guize.三飞机带三 || paiType == (int)Guize.三飞机带三对)
            {
                list = findFeiji(size, (int)Guize.三飞机不带, nextPai);
                if (list != null)
                {
                    list1 = (ArrayList)list[0];
                    #region 三飞机带三
                    if (paiType == (int)Guize.三飞机带三)
                    {
                        if (a != null && a.Length > 2)
                        {
                            list1.Add(a[0]); list1.Add(a[1]); list1.Add(a[2]);
                        }
                        else if (a != null && b != null)
                        {
                            list1.Add(a[0]); list1.Add(b[0]); list1.Add(b[0]);
                        }
                        else if (c.Length > 3)
                        {
                            int j = -1;
                            for (int i = 0; i < c.Length; i++)
                            {
                                if ((int)list1[0] != c[i] && (int)list1[4] != c[i] && (int)list1[list1.Count - 1] != c[i])
                                {
                                    j = c[i]; break;
                                }
                            }
                            if (b != null && j != -1)
                            {
                                list1.Add(b[0]); list1.Add(b[0]); list1.Add(j);
                            }
                            else if(j != -1)
                            {
                                list1.Add(j); list1.Add(j); list1.Add(j);
                            }
                        }
                        if (list1.Count == 12) return list1;
                    }
                    #endregion
                    #region 三飞机带三对
                    else if (paiType == (int)Guize.三飞机带三对)
                    {
                        if (b != null && b.Length > 2)
                        {
                            list1.Add(b[0]); list1.Add(b[0]); list1.Add(b[1]); list1.Add(b[1]); list1.Add(b[2]); list1.Add(b[2]);
                        }
                        else if (c.Length > 3)
                        {
                            for (int i = 0; i < c.Length; i++)
                            {
                                if ((int)list1[0] != c[i] && (int)list1[4] != c[i] && (int)list1[list1.Count - 1] != c[i])
                                {
                                    list1.Add(c[i]); list1.Add(c[i]); break;
                                }
                            }
                            if (b != null && b.Length > 1)
                            {
                                list1.Add(b[0]); list1.Add(b[0]); list1.Add(b[1]); list1.Add(b[1]);
                            }
                        }
                        if (list1.Count == 15) return list1;
                    }
                    #endregion
                }
            }
            #endregion
            #region 四飞机带四
            else if (paiType == (int)Guize.四飞机带四)
            {
                list = findFeiji(size, (int)Guize.四飞机不带, nextPai);
                if (list != null)
                {
                    list1 = (ArrayList)list[0];
                    int[] next = mArrayToArgs(nextPai);
                    int len = list1.Count;
                    for (int i = 0; i < next.Length; i++)
                    {
                        for(int j=0;j < len;j++)
                        {
                            if (next[i] != (int)list1[j])
                            {
                                list1.Add(next[i]); break;
                            }
                        }
                        if (list1.Count == 16) break;
                    }
                    if (list1.Count == 16) return list1;
                    /*list1 = (ArrayList)nextPai.Clone();
                    int num1 = 0; int num2 = 0;
                    if (a != null) num1 = a[a.Length - 1];
                    else if (b != null) num2 = b[b.Length - 1];
                    if (num1 > num2) list1.Remove(num1);
                    else if (num1 < num2) list1.Remove(num2);
                    if (list1.Count == 16) return list1;*/
                }
            }
            #endregion
            return null;
        }
        #endregion
        #region 搜索所有大于上手的牌，返回一个包含（单张*n,对子*n,三张*n,四张*n）的集合
        public ArrayList basic(int size, ArrayList nextPai)
        {
            int[] next = mArrayToArgs(nextPai);
            int sub = findBigSub(size, next);
            if (sub != -1)
            {
                ArrayList list = new ArrayList();
                ArrayList list1 = new ArrayList();
                ArrayList list2 = new ArrayList();
                ArrayList list3 = new ArrayList();
                ArrayList list4 = new ArrayList();
                for (int i = sub; i < next.Length; i++)
                {
                    int count = next.Length - i;
                    #region count大于四张
                    if (count >= 4)
                    {
                        if (next[i] == next[i + 1] && next[i + 1] == next[i + 2] && next[i + 2] == next[i + 3])
                        {
                            list4.Add(next[i]); i += 3;
                        }
                        else if (next[i] == next[i + 1] && next[i + 1] == next[i + 2])
                        {
                            list3.Add(next[i]); i += 2;
                        }
                        else if (next[i] == next[i + 1])
                        {
                            list2.Add(next[i]); i++;
                        }
                        else list1.Add(next[i]);
                    }
                    #endregion
                    #region count大于三张
                    else if (count == 3)
                    {
                        if (next[i] == next[i + 1] && next[i + 1] == next[i + 2])
                        {
                            list3.Add(next[i]); i += 2;
                        }
                        else if (next[i] == next[i + 1])
                        {
                            list2.Add(next[i]); i++;
                        }
                        else list1.Add(next[i]);
                    }
                    #endregion
                    #region count等于两张和一张
                    else if (count == 2)
                    {
                        if (next[i] == next[i + 1])
                        {
                            list2.Add(next[i]); i++;
                        }
                        else list1.Add(next[i]);
                    }
                    else list1.Add(next[i]);
                    #endregion
                }
                list.Add(list1);
                list.Add(list2);
                list.Add(list3);
                list.Add(list4);
                return list;
            }
            return null;
        }
        #endregion
        #endregion
    }
    #endregion
}
