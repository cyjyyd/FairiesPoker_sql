using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FairiesPoker
{
    class format
    {
        public ArrayList convert (ArrayList arr)
        {
            ArrayList returnlist = new ArrayList();
            string[] final = (string[])arr.ToArray(typeof(string));
            int[] judge = new int[20];
            for (int i = 0; i < 20; i++)
            {
                int number = 0;
                string num = null;
                foreach (char item in final[i])
                {
                    if (item >= 48 && item <= 58)
                    {
                        num += item;
                    }
                }
                number = int.Parse(num);
                judge[i] = number;
            }
            for (int i = 0; i < judge.Length; i++)
            {
                returnlist.Add(judge[i]);
            }
            return returnlist;
        }
    }
}
