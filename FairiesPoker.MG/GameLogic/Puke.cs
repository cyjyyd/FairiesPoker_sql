namespace FairiesPoker
{
    class Puke
    {
        public Puke(int size, object image)
            : this(size, 0, image)
        {
        }

        public Puke(int size, int color, object image)
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

        private static object backImage;//背面图，静态

        public static object BackImage
        {
            get { return Puke.backImage; }
            set { Puke.backImage = value; }
        }

        private int color;//牌的花色

        public int Color
        {
            get { return color; }
        }

        private object image;//牌的正面图

        public object Image
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
