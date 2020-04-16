using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Collections;

namespace FairiesPoker
{
    class config
    {
        [DllImport("kernel32")]//返回0表示失败，非0为成功
        private static extern long WritePrivateProfileString(string section, string key,string val, string filePath);
        [DllImport("kernel32")]//返回取得字符串缓冲区的长度
        private static extern long GetPrivateProfileString(string section, string key,string def, StringBuilder retVal, int size, string filePath);
        public ArrayList filelst = new ArrayList();
        public ArrayList ifexists = new ArrayList();
        string apppath = Application.StartupPath;
        string iniFilePath = Application.StartupPath + "\\config.ini";
        string lstFilePath = Application.StartupPath + "\\Filelist.lst";
        string pokFilePath = Application.StartupPath + "\\Pokers\\";
        string resFilePath = Application.StartupPath + "\\Results\\";
        private string ipaddress = "127.0.0.1";
        private int ui;
        private bool backmusic;
        private bool soundfx;
        private bool filmmode;
        private bool fullscreen;
        private int width;
        private int height;
        private int exists = 0;
        private int port;
        public config()
        {
            if (File.Exists(iniFilePath) == false)
            {
                FileStream fs = new FileStream("config.ini", FileMode.Create, FileAccess.ReadWrite);
                fs.Close();
                writeset();
                readset();
            }
            else readset();
            if (File.Exists(lstFilePath) == false)
            {
                FileStream fs = new FileStream("Filelist.lst",FileMode.Create,FileAccess.ReadWrite);
                fs.Close();listadd();
            }
        }
        public int UI
        {
            get { return ui; }
            set { ui = value; }
        }
        public bool BackMusic
        {
            get { return backmusic; }
            set { backmusic = value; }
        }
        public bool SoundFX
        {
            get { return soundfx; }
            set { soundfx = value; }
        }
        public bool FilmMode
        {
            get { return filmmode; }
            set { filmmode = value; }
        }
        public int Width
        {
            get { return width; }
            set { width = value; }
        }
        public int Height
        {
            get { return height; }
            set { height = value; }
        }
        public bool FullScreen
        {
            get { return fullscreen; }
            set { fullscreen = value; }
        }
        public string IPAddress
        {
            get { return ipaddress; }
            set { ipaddress = value; }
        }
        public int Port
        {
            get { return port; }
            set { port = value; }
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
        #region 写Ini文件
        public bool WriteIniData(string Section, string Key, string Value, string iniFilePath)
        {
            if (File.Exists(iniFilePath))
            {
                long OpStation = WritePrivateProfileString(Section, Key, Value, iniFilePath);
                if (OpStation == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
        #endregion

        public void writeset ()
        {
            WriteIniData("Settings", "UI", Convert.ToString(UI), iniFilePath);
            WriteIniData("Settings", "BackMusic", Convert.ToString(BackMusic), iniFilePath);
            WriteIniData("Settings", "SoundFX", Convert.ToString(SoundFX), iniFilePath);
            WriteIniData("Settings", "FilmMode", Convert.ToString(FilmMode), iniFilePath);
            WriteIniData("Video", "ScreenWidth", Convert.ToString(width), iniFilePath);
            WriteIniData("Video", "ScreenHeight", Convert.ToString(height), iniFilePath);
            WriteIniData("Video", "FullScreen", Convert.ToString(FullScreen), iniFilePath);
            WriteIniData("Network", "IPAddress", "127.0.0.1", iniFilePath);
            WriteIniData("Network", "Port", "8088", iniFilePath);
        }
        private void readset ()
        {
            UI = Convert.ToInt32(ReadIniData("Settings", "UI", "5", iniFilePath));
            BackMusic = Convert.ToBoolean(ReadIniData("Settings", "BackMusic", "false", iniFilePath));
            SoundFX = Convert.ToBoolean(ReadIniData("Settings", "SoundFX", "false", iniFilePath));
            FilmMode = Convert.ToBoolean(ReadIniData("Settings", "FilmMode", "false", iniFilePath));
            Width = Convert.ToInt32(ReadIniData("Video", "ScreenWidth", "1280", iniFilePath));
            Height = Convert.ToInt32(ReadIniData("Video", "ScreenHeight", "720", iniFilePath));
            FullScreen = Convert.ToBoolean(ReadIniData("Video", "FullScreen", "false", iniFilePath));
            IPAddress = ReadIniData("Network", "IPAddress", "127.0.0.1", iniFilePath);
            Port = Convert.ToInt32(ReadIniData("Network", "Port", "8088", iniFilePath));
        }
        public int Filecheck(int a)
        {
            string temp = Enum.GetName(typeof(Path), a);
            listadd();
            for (int i = 0; i < 54; i++)
            {
                if (File.Exists(pokFilePath + Convert.ToString(a) + "\\" + filelst[i] + ".png"))
                {
                    ifexists.Add(true);
                    WriteIniData(temp, Convert.ToString(filelst[i]), "true", lstFilePath);
                }
                else
                {
                    ifexists.Add(false);
                    WriteIniData(temp, Convert.ToString(filelst[i]), "false", lstFilePath);
                }
            }
            for (int i = 54; i <= 57; i++)
            {
                if (File.Exists(resFilePath+Convert.ToString(a)+"\\"+filelst[i]+".png"))
                {
                    ifexists.Add(true);
                    WriteIniData(temp, Convert.ToString(filelst[i]), "true", lstFilePath);
                }
                else
                {
                    ifexists.Add(false);
                    WriteIniData(temp, Convert.ToString(filelst[i]), "false", lstFilePath);
                }
            }
            for (int i = 58; i < 60; i++)
            {
                if (File.Exists(apppath+"\\"+temp+"\\"+filelst[i]+".png"))
                {
                    ifexists.Add(true);
                    WriteIniData(temp, Convert.ToString(filelst[i]), "true", lstFilePath);
                }
                else
                {
                    ifexists.Add(false);
                    WriteIniData(temp, Convert.ToString(filelst[i]), "false", lstFilePath);
                }
            }
            if (File.Exists(apppath + "\\" + temp + "\\" + filelst[60] + ".jpg"))
            {
                ifexists.Add(true);
                WriteIniData(temp, Convert.ToString(filelst[60]), "true", lstFilePath);
            }
            else
            {
                ifexists.Add(false);
                WriteIniData(temp, Convert.ToString(filelst[60]), "false", lstFilePath);
            }
            if (File.Exists(apppath + "\\" + temp + "\\" + filelst[61] + ".mp3"))
            {
                ifexists.Add(true);
                WriteIniData(temp, Convert.ToString(filelst[61]), "true", lstFilePath);
            }
            else
            {
                ifexists.Add(false);
                WriteIniData(temp, Convert.ToString(filelst[61]), "false", lstFilePath);
            }
            foreach (var item in ifexists)
            {
                if (Convert.ToBoolean(item))
                {
                    exists++;
                }
            }
            if (exists == 0)
            {
                ifexists.Clear();
                exists = 0;
                return 0;
            }
            else if (exists == 62)
            {
                ifexists.Clear();
                exists = 0;
                return 1;
            }
            else
            {
                ifexists.Clear();
                exists = 0;
                return 2;
            }
        }
        private void listadd ()
        {
            for (int i = 3; i <= 15; i++)
            {
                filelst.Add("meihua" + i);
                filelst.Add("fangkuai" + i);
                filelst.Add("heitao" + i);
                filelst.Add("hongtao" + i);
            }
            filelst.Add("16");
            filelst.Add("17");
            filelst.Add("lose_dz");
            filelst.Add("win_dz");
            filelst.Add("lose_nm");
            filelst.Add("win_nm");
            filelst.Add("btn1");
            filelst.Add("btn2");
            filelst.Add("main seq");
            filelst.Add("background");
        }
    }
}
