using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Drawing.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Forms;

namespace FairiesPoker
{
    public enum Path
    {
        [Description("第一部")]
        UI_TB=1,
        [Description("第二部")]
        UI_LT=2,
        [Description("第三部")]
        UI_FR=3,
        [Description("第四部")]
        UI_SW=4,
        [Description("第五部")]
        UI_PF=5,
        [Description("第六部")]
        UI_LN=6,
        [Description("仙子杯")]
        UI_PG=7

    }
    class UI
    {
        private Image button;
        private Image buttonpress;
        private Image background;
        private string apppath = Application.StartupPath;
        private string uipath = "";
        private string fpath = "";
        public int uiselect = 0;
        public UI()
        {
            
        }
        public Image Button
        {
            get { return button; }
            set { button = value; }
        }
        public Image Buttonpress
        {
            get { return buttonpress; }
            set { buttonpress = value; }
        }
        public Image Background
        {
            get { return background; }
            set { background = value; }
        }

        public bool setfont (Control c)
        {
            PrivateFontCollection font = new PrivateFontCollection();
            try
            {
                font.AddFontFile(fpath + "font.ttf");
                Font myfont = new Font(font.Families[0].Name,c.Font.Size,c.Font.Style);
                c.Font = myfont;
            }
            catch (Exception)
            {
                MessageBox.Show("字体不存在或加载失败，程序将以默认字体显示，请检查文件是否完整！","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
                return false;
            }
            return true;
        }
        public void setUI(int path)
        {
            switch (path)
            {
                case 1:
                    uipath = Path.UI_TB.ToString(); uiselect = Convert.ToInt32(Path.UI_TB);
                    break;
                case 2:
                    uipath = Path.UI_LT.ToString(); uiselect = Convert.ToInt32(Path.UI_LT);
                    break;
                case 3:
                    uipath = Path.UI_FR.ToString(); uiselect = Convert.ToInt32(Path.UI_FR);
                    break;
                case 4:
                    uipath = Path.UI_SW.ToString(); uiselect = Convert.ToInt32(Path.UI_SW);
                    break;
                case 5:
                    uipath = Path.UI_PF.ToString(); uiselect = Convert.ToInt32(Path.UI_PF);
                    break;
                case 6:
                    uipath = Path.UI_LN.ToString(); uiselect = Convert.ToInt32(Path.UI_LN);
                    break;
                case 7:
                    uipath = Path.UI_PG.ToString(); uiselect = Convert.ToInt32(Path.UI_PG);
                    break;
            }
            fpath = apppath + "\\" + uipath + "\\";
            try
            {
                button = Image.FromFile(fpath + "btn1.png");
                buttonpress = Image.FromFile(fpath + "btn2.png");
                background = Image.FromFile(fpath + "main seq.jpg");
            }
            catch (Exception)
            {
                MessageBox.Show("Error while loading UI components,Please check if files exists!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Environment.Exit(0);
            }
        }
    }
}
