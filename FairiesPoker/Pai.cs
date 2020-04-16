using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FairiesPoker
{
    /// <summary>
    /// 牌类，设置牌的花色和大小等一系列属性，并以此指定牌图形集合应该显示的图片
    /// </summary>
    class Pai
    {
        private string huase;
        private int size;
        private int index;
        private Image image;
        private Image backimage;
        [DllImport("kernel32")]//返回取得字符串缓冲区的长度
        private static extern long GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        public Pai(string huase, int size)
        {
            this.huase = huase;
            this.size = size;
            try
            {
                this.image = Image.FromFile(Application.StartupPath + @"\Pokers\" + ReadIniData("Settings","UI","5",Application.StartupPath+"\\config.ini") + "\\"+ huase + size + ".png");
            }
            catch (Exception)
            {
                MessageBox.Show("错误！文件丢失！程序将关闭！","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
                System.Environment.Exit(0);
            }
        }
        public string Huase //牌的花色
        {
            get { return huase; }
        }
        public int Size  //牌值大小，3-15
        {
            get { return size; }
        }
        public int Index //作为随机属性，便于保存牌值的链表和正确的图片相对应
        {
            get { return index; }
            set { index = value; }
        }
        public Image Image //牌的正面图
        {
            get { return image; }
        }
        public Image Backimage //牌的背面图
        {
            get { return backimage; }
            set { backimage = value; }
        }
        #region 读Ini文件
        public string ReadIniData(string Section, string Key, string NoText, string iniFilePath)
        {
            if (File.Exists(iniFilePath))
            {
                StringBuilder temp = new StringBuilder(1024);
                GetPrivateProfileString(Section, Key, NoText, temp, 1024, iniFilePath);
                return temp.ToString();
            }
            else
            {
                return String.Empty;
            }
        }
        #endregion
    }
}
