using System;
using System.ComponentModel;

namespace FairiesPoker
{
    public enum Path
    {
        [Description("奇妙仙子")]
        UI_TB = 1,
        [Description("失落的宝藏")]
        UI_LT = 2,
        [Description("拯救精灵大作战")]
        UI_FR = 3,
        [Description("羽翼之谜")]
        UI_SW = 4,
        [Description("海盗仙子")]
        UI_PF = 5,
        [Description("永无兽传奇")]
        UI_LN = 6
    }

    class UI
    {
        private string uipath = "";
        public int uiselect = 0;

        public string ThemeFolder => uipath;

        public bool setfont(object c)
        {
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
                default:
                    uipath = Path.UI_PF.ToString(); uiselect = Convert.ToInt32(Path.UI_PF);
                    break;
            }
        }
    }
}
