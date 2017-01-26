using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MurahAje.Web.Tools
{
    public class Logs
    {
        #region Logs
        public static void WriteLog(string StrMessage)
        {
            string Filename = null;
            string Dirs = null;
            string path = getPath();
            Filename = "TRANSLOG-{0}{1}"+DateTime.Now.ToString("dd-MMM-yyyy")+".log";
            Dirs = "\\Logs\\";
            DirectoryInfo drInfo = new DirectoryInfo(path+Dirs);
            if (!drInfo.Exists)
            {
                drInfo.Create();
            }
            Filename = path + Dirs + Filename;
            if (File.Exists(Filename))
            {
                FileWriter.AppendToFile(StrMessage, Filename);
            }
            else
            {
                FileWriter.WriteFile(StrMessage, Filename);
            }
        }
        public static string getPath()
        {
            String Pth = Directory.GetCurrentDirectory();
            return Pth;
        }
        public static void RemoveLog()
        {
            string Filename = null;
            string Dirs = null;
            string path = getPath();

            Filename = "TRANSLOG-" + DateTime.Now.ToString("dd-MMM-yyyy") + ".log";
            Dirs = "\\Logs\\";

            DirectoryInfo drInfo = new DirectoryInfo(path + Dirs);
            if (!drInfo.Exists)
            {
                return;
            }
            FileInfo[] AllFiles = drInfo.GetFiles();
            Filename = path + Dirs + Filename;
            foreach (FileInfo MyLogFile in AllFiles)
            {
                if (MyLogFile.FullName.ToLower() != Filename.ToLower())
                {
                    MyLogFile.Delete();
                }
            }
        }
        #endregion
    }
}
