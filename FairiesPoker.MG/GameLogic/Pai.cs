namespace FairiesPoker
{
    /// <summary>
    /// 牌类，保留花色、大小和随机索引。渲染层通过 CardRenderer 按花色/大小加载纹理。
    /// </summary>
    class Pai
    {
        private string huase;
        private int size;
        private int index;

        public Pai(string huase, int size)
        {
            if (size == 16 || size == 17)
                huase = "";

            this.huase = huase;
            this.size = size;
        }

        public string Huase
        {
            get { return huase; }
        }

        public int Size
        {
            get { return size; }
        }

        public int Index
        {
            get { return index; }
            set { index = value; }
        }
    }
}
