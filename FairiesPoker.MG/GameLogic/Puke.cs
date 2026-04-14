using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FairiesPoker
{
    class Puke
    {
        public Puke(int size, Image image)
        {
            this.color = color;
            this.size = size;
            this.image = image;
        }

        private int index;//做为牌的随机属性

        public int Index
        {
            get { return index; }
            set { index = value; }
        }

        private static Image backImage;//背面图，静态

        public static Image BackImage
        {
            get { return Puke.backImage; }
            set { Puke.backImage = value; }
        }

        private int color;//牌的花色

        public int Color
        {
            get { return color; }
        }

        private Image image;//牌的正面图

        public Image Image
        {
            get { return image; }
        }

        private int size;//牌的大小

        public int Size
        {
            get { return size; }
        }
    }
}
