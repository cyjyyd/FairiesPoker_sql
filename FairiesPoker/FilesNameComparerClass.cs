using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FairiesPoker
{
    public class FilesNameComparerClass : IComparer
    {
        int IComparer.Compare(Object x, Object y)
        {
            if (x == null || y == null)
                throw new ArgumentException("Parameters can't be null");
            string fileA = Convert.ToString (x);
            string fileB = Convert.ToString (y);
            char[] arr1 = fileA.ToCharArray();
            char[] arr2 = fileB.ToCharArray();
            int i = 0, j = 0;
            string s1 = "", s2 = "";
            while (i < arr1.Length && char.IsDigit(arr1[i])==false)
            {
                i++;
            }
            while (i < arr1.Length && char.IsDigit(arr1[i]))
            {
                s1 += arr1[i];
                i++;
            }
            while (j < arr2.Length && char.IsDigit(arr2[j])==false)
            {
                j++;
            }
            while (j < arr2.Length && char.IsDigit(arr2[j]))
            {
                s2 += arr2[j];
                j++;
            }
            if (int.Parse(s1) > int.Parse(s2))
            {
                return 1;
            }
            else if (int.Parse(s1) < int.Parse(s2))
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }
    }
}
