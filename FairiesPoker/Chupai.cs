using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FairiesPoker
{
    public enum Guize
    {
        不出, 一张, 对子, 三不带, 炸弹, 顺子, 连对,
        飞机不带, 三飞机不带, 四飞机不带, 五飞机不带,
        三带一, 三带二, 四带二, 飞机带二,
        飞机带二对, 三飞机带三, 四飞机带四,
        三飞机带三对, 四带二对,
        天炸, 四飞机带四对, 五飞机带五, 六飞机不带
    }
    class Chupai
    {
        #region 出牌的类型,如单张，对子等  (属性)
        private int paiType = 0;
        public int PaiType
        {
            get { return paiType; }
            set { paiType = value; }
        }
        #endregion

        #region 外部调用方法，判断出牌是否符合规则
        public bool isRight(ArrayList list)
        {
            int[] args = new int[list.Count];
            for (int i = 0; i < args.Length; i++)
            {
                args[i] = (int)list[i];
            }
            return judge(args);
        }
        #endregion

        #region 外部调用方法，判断出牌是否符合规则 (知道数量,传数组)
        public bool isRight(int[] args)
        {
            return judge(args);
        }
        #endregion

        #region 开始判断
        private bool judge(int[] args)
        {
            format(args);
            bool bl = false;
            switch (args.Length)
            {
                case 1: bl = true; paiType = (int)Guize.一张; break;
                case 2: bl = erzhang(args); break;
                case 3: bl = sanzhang(args); break;
                case 4: bl = sizhang(args); break;
                case 5: bl = wuzhang(args); break;
                case 6: bl = liuzhang(args); break;
                case 7: bl = qizhang(args); break;
                case 8: bl = bazhang(args); break;
                case 9: bl = jiuzhang(args); break;
                case 10: bl = shizhang(args); break;
                case 11: bl = shiyizhang(args); break;
                case 12: bl = shierzhang(args); break;
                case 14: bl = shisizhang(args); break;
                case 15: bl = shiwuzhang(args); break;
                case 16: bl = shiliuzhang(args); break;
                case 18: bl = shibazhang(args); break;
                case 20: bl = ershizhang(args); break;
            }
            return bl;
        }
        #endregion

        #region 辅助方法(也为主要算法)
        #region 排序(从大到小)
        public void format(int[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                for (int j = i; j < args.Length; j++)
                {
                    if (args[i] < args[j])
                    {
                        int temp = args[i];
                        args[i] = args[j];
                        args[j] = temp;
                    }
                }
            }
        }
        #endregion
        #region 排序(从小到大)
        public void minToBig(int[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                for (int j = i; j < args.Length; j++)
                {
                    if (args[i] > args[j])
                    {
                        int temp = args[i];
                        args[i] = args[j];
                        args[j] = temp;
                    }
                }
            }
        }
        #endregion
        #region 判断偶数数组里是否全为对子
        private bool isAllDuiZi(int[] args)
        {
            for (int i = 0; i < args.Length; i += 2)
            {
                if (args[i] != args[i + 1]) return false;
            }
            return true;
        }
        #endregion
        #region 飞机不带的通用算法 （判断所有的飞机不带）
        private bool fly(int[] args)
        {
            if (args[0] > 14) return false;
            int[] a = new int[3];
            int k = 0;
            for (int i = 0; i < args.Length / a.Length; i++)
            {
                for (int j = 0; j < a.Length; j++)
                {
                    a[j] = args[k]; k++;
                }
                if (sanzhang(a) == false) return false;
                if (k < args.Length)
                {
                    if (args[k - 1] - 1 != args[k]) return false;
                }
            }
            return true;
        }
        #endregion
        #region 三飞机以上带牌,如果中间包含炸弹,则移动其中一张牌到数组的最后面
        private void changeArgs(int[] args)
        {
            ArrayList list = breakUpArgs(args, 4, false);
            List<int> list1 = new List<int>();
            foreach (int[] a in list)
            {
                if (zhadan(a))
                {
                    for (int i = 0; i < args.Length; i++)
                    {
                        if (args[i] == a[0])
                        {
                            list1.Add(i); i += 4;
                        }
                    }
                }
            }
            for (int i = 0; i < list1.Count; i++)
            {
                for (int j = list1[i]; j < args.Length - 1; j++)
                {
                    args[j] = args[j + 1];
                }
                args[args.Length - 1] = args[list1[i]];
            }
        }
        #endregion
        #region 判断三张或四张带牌的一部分通用算法（将牌分解成多个需要的数组）
        private ArrayList breakUpArgs(int[] args, int len, bool daiDui)
        {
            ArrayList list = new ArrayList();
            int[] a = new int[len];
            int count = 0;
            if (daiDui == false)
            {
                count = args.Length - len + 1;
                for (int i = 0; i < count; i++)
                {
                    int k = 0;
                    for (int j = i; j < args.Length; j++)
                    {
                        a[k] = args[j]; k++;
                        if (k == a.Length) break;
                    }
                    list.Add(a.Clone());
                }
            }
            else
            {
                count = (args.Length - len) / 2 + 1;
                ArrayList list1 = new ArrayList();
                ArrayList list2 = new ArrayList();
                int[] b = new int[args.Length - len];
                for (int i = 0; i < count * 2; i += 2)
                {
                    int ii = 0; int j = 0; int k = 0;
                    for (ii = 0; ii < i; ii++)
                    {
                        b[ii] = args[ii];
                    }
                    for (j = i; j < args.Length; j++)
                    {
                        a[k] = args[j]; k++;
                        if (k == a.Length) break;
                    }
                    k = 0;
                    for (int jj = j + 1; jj < args.Length; jj++)
                    {
                        b[ii + k] = args[jj]; k++;
                    }
                    list1.Add(a.Clone());
                    list2.Add(b.Clone());
                }
                list.Add(list1);
                list.Add(list2);
            }
            return list;
        }
        #endregion
        #endregion

        #region 所有的出牌数量
        #region 两张牌
        private bool erzhang(int[] args)
        {
            if (duizi(args) || tianzha(args)) return true;
            return false;
        }
        #endregion
        #region 四张牌
        private bool sizhang(int[] args)
        {
            if (zhadan(args) || san_1(args)) return true;
            return false;
        }
        #endregion
        #region 五张牌
        private bool wuzhang(int[] args)
        {
            if (san_2(args) || shunzi(args)) return true;
            return false;
        }
        #endregion
        #region 六张牌
        private bool liuzhang(int[] args)
        {
            if (shunzi(args) || liandui(args) || fly6(args) || si_2(args)) return true;
            return false;
        }
        #endregion
        #region 七张牌
        private bool qizhang(int[] args)
        {
            if (shunzi(args)) return true;
            return false;
        }
        #endregion
        #region 八张牌
        private bool bazhang(int[] args)
        {
            if (fly6_2(args) || shunzi(args) || liandui(args) || si_4(args)) return true;
            return false;
        }
        #endregion
        #region 九张牌
        private bool jiuzhang(int[] args)
        {
            if (shunzi(args) || fly9(args)) return true;
            return false;
        }
        #endregion
        #region 十张牌
        private bool shizhang(int[] args)
        {
            if (fly6_4(args) || shunzi(args) || liandui(args)) return true;
            return false;
        }
        #endregion
        #region 十一张牌
        private bool shiyizhang(int[] args)
        {
            if (shunzi(args)) return true;
            return false;
        }
        #endregion
        #region 十二张牌
        private bool shierzhang(int[] args)
        {
            if (shunzi(args) || liandui(args) || fly12(args) || fly9_3(args)) return true;
            return false;
        }
        #endregion
        #region 十四张牌
        private bool shisizhang(int[] args)
        {
            if (liandui(args)) return true;
            return false;
        }
        #endregion
        #region 十五张牌
        private bool shiwuzhang(int[] args)
        {
            if (fly9_6(args) || fly15(args)) return true;
            return false;
        }
        #endregion
        #region 十六张牌
        private bool shiliuzhang(int[] args)
        {
            if (fly12_4(args) || liandui(args)) return true;
            return false;
        }
        #endregion
        #region 十八张牌
        private bool shibazhang(int[] args)
        {
            if (liandui(args) || fly18(args)) return true;
            return false;
        }
        #endregion
        #region 二十张牌
        private bool ershizhang(int[] args)
        {
            if (fly15_5(args) || fly12_8(args) || liandui(args)) return true;
            return false;
        }
        #endregion
        #endregion

        #region 所有符合规则的出牌
        #region 对子(2张)
        private bool duizi(int[] args)
        {
            if (args[0] == args[1])
            {
                paiType = (int)Guize.对子; return true;
            }
            return false;
        }
        #endregion
        #region 天炸(2)
        private bool tianzha(int[] args)
        {
            if (args[0] == 17 && args[1] == 16)
            {
                paiType = (int)Guize.天炸; return true;
            }
            return false;
        }
        #endregion
        #region 三张(3)
        private bool sanzhang(int[] args)
        {
            if (args[0] == args[1] && args[1] == args[2])
            {
                paiType = (int)Guize.三不带; return true;
            }
            return false;
        }
        #endregion
        #region 三带一(4)
        private bool san_1(int[] args)
        {
            ArrayList list = breakUpArgs(args, 3, false);
            foreach (int[] a in list)
            {
                if (sanzhang(a))
                {
                    paiType = (int)Guize.三带一; return true;
                }
            }
            return false;
        }
        #endregion
        #region 三带二(5)
        private bool san_2(int[] args)
        {
            ArrayList list = breakUpArgs(args, 3, true);
            for (int i = 0; i < ((ArrayList)list[0]).Count; i++)
            {
                int[] a = (int[])((ArrayList)list[0])[i];
                int[] b = (int[])((ArrayList)list[1])[i];
                if (sanzhang(a) && isAllDuiZi(b))
                {
                    paiType = (int)Guize.三带二; return true;
                }
            }
            return false;
        }
        #endregion
        #region 炸弹(4)
        private bool zhadan(int[] args)
        {
            if (args[0] == args[1] && args[1] == args[2] && args[2] == args[3])
            {
                paiType = (int)Guize.炸弹; return true;
            }
            return false;
        }
        #endregion
        #region 四带二(6)
        private bool si_2(int[] args)
        {
            ArrayList list = breakUpArgs(args, 4, false);
            foreach (int[] a in list)
            {
                if (zhadan(a))
                {
                    paiType = (int)Guize.四带二; return true;
                }
            }
            return false;
        }
        #endregion
        #region 四带两对(8)
        private bool si_4(int[] args)
        {
            ArrayList list = breakUpArgs(args, 4, true);
            for (int i = 0; i < ((ArrayList)list[0]).Count; i++)
            {
                int[] a = (int[])((ArrayList)list[0])[i];
                int[] b = (int[])((ArrayList)list[1])[i];
                if (zhadan(a) && isAllDuiZi(b))
                {
                    paiType = (int)Guize.四带二对; return true;
                }
            }
            return false;
        }
        #endregion
        #region 顺子(5--12)
        private bool shunzi(int[] args)
        {
            if (args[0] > 14) return false;
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] - 1 != args[i + 1]) return false;
            }
            paiType = (int)Guize.顺子;
            return true;
        }
        #endregion
        #region 连对(6--20)
        private bool liandui(int[] args)
        {
            if (args[0] > 14) return false;
            if (isAllDuiZi(args) == false) return false;
            for (int i = 1; i < args.Length - 1; i += 2)
            {
                if (args[i] - 1 != args[i + 1]) return false;
            }
            paiType = (int)Guize.连对;
            return true;
        }
        #endregion
        #region 飞机不带(6)
        private bool fly6(int[] args)
        {
            if (fly(args))
            {
                paiType = (int)Guize.飞机不带; return true;
            }
            return false;
        }
        #endregion
        #region 飞机带2(8)
        private bool fly6_2(int[] args)
        {
            ArrayList list = breakUpArgs(args, 6, false);
            foreach (int[] a in list)
            {
                if (fly6(a))
                {
                    paiType = (int)Guize.飞机带二; return true;
                }
            }
            return false;
        }
        #endregion
        #region 飞机带2对(10)
        private bool fly6_4(int[] args)
        {
            ArrayList list = breakUpArgs(args, 6, true);
            for (int i = 0; i < ((ArrayList)list[0]).Count; i++)
            {
                int[] a = (int[])((ArrayList)list[0])[i];
                int[] b = (int[])((ArrayList)list[1])[i];
                if (fly6(a) && isAllDuiZi(b))
                {
                    paiType = (int)Guize.飞机带二对; return true;
                }
            }
            return false;
        }
        #endregion
        #region 3飞机不带(9)
        private bool fly9(int[] args)
        {
            if (fly(args))
            {
                paiType = (int)Guize.三飞机不带; return true;
            }
            return false;
        }
        #endregion
        #region 3飞机带3(12)
        private bool fly9_3(int[] args)
        {
            changeArgs(args);
            ArrayList list = breakUpArgs(args, 9, false);
            foreach (int[] a in list)
            {
                if (fly9(a))
                {
                    paiType = (int)Guize.三飞机带三; return true;
                }
            }
            return false;
        }
        #endregion
        #region 3飞机带3对(15)
        private bool fly9_6(int[] args)
        {
            ArrayList list = breakUpArgs(args, 9, true);
            for (int i = 0; i < ((ArrayList)list[0]).Count; i++)
            {
                int[] a = (int[])((ArrayList)list[0])[i];
                int[] b = (int[])((ArrayList)list[1])[i];
                if (fly9(a) && isAllDuiZi(b))
                {
                    paiType = (int)Guize.三飞机带三对; return true;
                }
            }
            return false;
        }
        #endregion
        #region 4飞机不带(12)
        private bool fly12(int[] args)
        {
            if (fly(args))
            {
                paiType = (int)Guize.四飞机不带; return true;
            }
            return false;
        }
        #endregion
        #region 4飞机带4(16)
        private bool fly12_4(int[] args)
        {
            changeArgs(args);
            ArrayList list = breakUpArgs(args, 12, false);
            foreach (int[] a in list)
            {
                if (fly12(a))
                {
                    paiType = (int)Guize.四飞机带四; return true;
                }
            }
            return false;
        }
        #endregion
        #region 4飞机带4对(20)
        private bool fly12_8(int[] args)
        {
            ArrayList list = breakUpArgs(args, 12, true);
            for (int i = 0; i < ((ArrayList)list[0]).Count; i++)
            {
                int[] a = (int[])((ArrayList)list[0])[i];
                int[] b = (int[])((ArrayList)list[1])[i];
                if (fly12(a) && isAllDuiZi(b))
                {
                    paiType = (int)Guize.四飞机带四对; return true;
                }
            }
            return false;
        }
        #endregion
        #region 5飞机不带(15)
        private bool fly15(int[] args)
        {
            if (fly(args))
            {
                paiType = (int)Guize.五飞机不带; return true;
            }
            return false;
        }
        #endregion
        #region 5飞机带5(20)
        private bool fly15_5(int[] args)
        {
            changeArgs(args);
            ArrayList list = breakUpArgs(args, 15, false);
            foreach (int[] a in list)
            {
                if (fly15(a))
                {
                    paiType = (int)Guize.五飞机带五; return true;
                }
            }
            return false;
        }
        #endregion
        #region 6飞机不带(18)
        private bool fly18(int[] args)
        {
            if (fly(args))
            {
                paiType = (int)Guize.六飞机不带; return true;
            }
            return false;
        }
        #endregion
        #endregion
    }
}
