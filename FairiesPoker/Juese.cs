using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FairiesPoker
{
    class Juese
    {
        /// <summary>
        /// 角色类，用于保存每个玩家的信息
        /// </summary>
        private int weiZhi;

        public int WeiZhi //角色位置
        {
            get { return weiZhi; }
            set { weiZhi = value; }
        }

        private bool dizhu = false;

        public bool Dizhu //是否是地主
        {
            get { return dizhu; }
            set { dizhu = value; }
        }

        private ArrayList imagePaiSub = new ArrayList();

        public ArrayList ImagePaiSub //牌图形链表
        {
            get { return imagePaiSub; }
            set { imagePaiSub = value; }
        }

        private ArrayList shengYuPai = new ArrayList();

        public ArrayList ShengYuPai //牌值链表
        {
            get { return shengYuPai; }
            set { shengYuPai = value; }
        }

        private ArrayList shangShouPai = new ArrayList();
        public ArrayList ShangShouPai //上家出牌链表
        {
            get { return shangShouPai; }
            set { shangShouPai = value; }
        }
        private ArrayList yiChuPai = new ArrayList();

        public ArrayList YiChuPai //已经出的牌
        {
            get { return yiChuPai; }
            set { yiChuPai = value; }
        }
        private string name; //名字（后续会拓展）

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private bool bigger;//牌总值，仅做参考
        public bool Bigger
        {
            get { return bigger; }
            set { bigger = value; }
        }
    }
}
