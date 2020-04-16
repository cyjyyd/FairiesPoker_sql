using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace FairiesPoker
{
    class ComputerChuPai
    {
        #region 出牌(暂时只能简单出牌)
        public ArrayList chuPai(ArrayList listPai)
        {
            Jiepai jiepai = new Jiepai();
            ArrayList list = jiepai.basic(2, listPai);
            ArrayList list1 = new ArrayList();
            int leave = listPai.Count;
            int[] a = jiepai.mArrayToArgs((ArrayList)list[0]);
            int[] b = jiepai.mArrayToArgs((ArrayList)list[1]);
            int[] c = jiepai.mArrayToArgs((ArrayList)list[2]);
            int[] d = jiepai.mArrayToArgs((ArrayList)list[3]);
            if (list != null)
            {
                #region 判断是否有炸弹
                if (d != null)
                {
                    if (leave == 4)  return listPai;
                    if (leave > 4 && (a != null || b != null || c != null))
                    {
                        if (d[d.Length - 1] != 15 || d[d.Length - 1] != 14)//判断炸弹是否为4条2或4条1，如果是就先出别的
                        {
                            list[0] = jiepai.removeBigPai(2, (ArrayList)list[0]);
                            list[1] = jiepai.removeBigPai(2, (ArrayList)list[1]);
                            int[] d1 = jiepai.mArrayToArgs((ArrayList)list[0]);
                            int[] d2 = jiepai.mArrayToArgs((ArrayList)list[1]);
                            if (d2 != null && d2.Length >= 2)
                            {
                                for (int i = 0; i < 4; i++)
                                {
                                    list1.Add(d[0]);
                                }
                                list1.Add(d2[0]); list1.Add(d2[0]);
                                list1.Add(d2[1]); list1.Add(d2[1]);
                                return list1;
                            }
                            else if (d1 != null && d1.Length >= 2)
                            {
                                for (int i = 0; i < 4; i++)
                                {
                                    list1.Add(d[0]);
                                }
                                list1.Add(d1[0]); list1.Add(d1[1]);
                                return list1;
                            }
                        }
                    }
                    #region 判断是否只剩下炸弹
                    else if (a == null && b == null && c == null)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            list1.Add(d[0]);
                        }
                        return list1;
                    }
                    #endregion
                }
                #endregion
                #region 判断是否有三张
                if (c != null)
                {
                    if (leave == 3 || leave == 4) return listPai;
                    if (leave == 5)
                    {
                        if (b != null)  return listPai;
                        else if(d == null)
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                list1.Add(c[0]);
                            }
                            list1.Add(a[0]);
                            return list1;
                        }
                    }
                    else if (leave > 5 && (a != null || b != null))
                    {
                        list[1] = jiepai.removeBigPai(2, (ArrayList)list[1]);
                        int[] c1 = jiepai.mArrayToArgs((ArrayList)list[0]);
                        int[] c2 = jiepai.mArrayToArgs((ArrayList)list[1]);
                        if (c2 != null)
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                list1.Add(c[0]);
                            }
                            list1.Add(c2[0]); list1.Add(c2[0]);
                            return list1;
                        }
                        else if (c1 != null)
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                list1.Add(c[0]);
                            }
                            list1.Add(c1[0]);
                            return list1;
                        }
                        else
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                list1.Add(c[0]);
                            }
                            return list1;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            list1.Add(c[0]);
                        }
                        return list1;
                    }
                }
                #endregion
                #region 判断是否有对子
                if (b != null)
                {
                    if (leave == 2)  return listPai;
                    Random rd = new Random();
                    int num = rd.Next(2)+2;
                    if (a == null || a != null && a.Length < num)
                    {
                        list1.Add(b[0]); list1.Add(b[0]);
                        return list1;
                    }
                }
                #endregion
                #region 判断是否有单张
                if (a != null)
                {
                    list1.Add(a[0]);
                    return list1;
                }
                #endregion
            }
            return null;
        }
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
