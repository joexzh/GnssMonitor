using System;
using System.Collections.Generic;

using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net;
using System.Windows.Forms;
using System.Net.Sockets;
using ZHD.SYS.CommonUtility.DatabaseLib;

namespace ZHDCommon
{
    public class SetFileRW
    {
        /// <summary>
        /// 获取配置文件所在目录地址
        /// </summary>
        /// <returns></returns>
        public static string GetConfigDirPath()
        {
            string curdir = System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase.ToString();
            curdir = curdir.Substring("file:///".Length);
            curdir = Path.GetDirectoryName(curdir);
            curdir = curdir + "\\";
            curdir = curdir + "Config\\";

            if (!Directory.Exists(curdir))
            {
                Directory.CreateDirectory(curdir);
            }

            return curdir;
        }

        private static string strConfigName = GetConfigDirPath() + "ConfigDatabase.ini";

        public void GetConfigMsg(DBConfigInfo DBConfigInfo, out bool isGPS, out bool isBDS, out bool isGlonass)
        {
            if (!File.Exists(strConfigName))
            {
                FileStream fs = new FileStream(strConfigName, FileMode.Create, FileAccess.ReadWrite);
                fs.Close();
            }

            isGPS = false;
            isBDS = false;
            isGlonass = false;

            using (StreamReader sr1 = new StreamReader(strConfigName))
            {
                string strDBStyle = sr1.ReadLine();
                if(null != strDBStyle)
                {
                    DBConfigInfo.DbStyle = (DBStyle)(int.Parse(strDBStyle));
                }
                DBConfigInfo.DbServer = sr1.ReadLine();
                DBConfigInfo.DbName = sr1.ReadLine();
                DBConfigInfo.DbUser = sr1.ReadLine();
                DBConfigInfo.DbPassword = sr1.ReadLine();

                #region 解算模式读取
                string strMsg = sr1.ReadLine();
                if (null != strMsg && "" != strMsg)
                {
                    isGPS = bool.Parse(strMsg);
                }

                strMsg = sr1.ReadLine();
                if (null != strMsg && "" != strMsg)
                {
                    isBDS = bool.Parse(strMsg);
                }

                strMsg = sr1.ReadLine();
                if (null != strMsg && "" != strMsg)
                {
                    isGlonass = bool.Parse(strMsg);
                }
                #endregion
                sr1.Close();
            }
        }

        public void SetConfigMsg(DBConfigInfo DBConfigInfo, bool isGPS, bool isBDS, bool isGlonass)
        {
            using (StreamWriter sw1 = new StreamWriter(strConfigName, false))
            {
                sw1.WriteLine((int)DBConfigInfo.DbStyle);
                sw1.WriteLine(DBConfigInfo.DbServer);
                sw1.WriteLine(DBConfigInfo.DbName);
                sw1.WriteLine(DBConfigInfo.DbUser);
                sw1.WriteLine(DBConfigInfo.DbPassword);
                sw1.WriteLine(isGPS);
                sw1.WriteLine(isBDS);
                sw1.WriteLine(isGlonass);
                sw1.Close();
            }
        }

        /// <summary>
        /// 写入坐标系统信息
        /// </summary>
        /// <param name="strCoorName"></param>
        /// <param name="listCoor"></param>
        public void WriteCoordinateDam(string strCoorName, List<string> listCoor)
        {
            string curdir = System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase.ToString();
            curdir = curdir.Substring("file:///".Length);
            curdir = Path.GetDirectoryName(curdir);
            curdir = curdir + "\\" + strCoorName;

            using (StreamWriter sw1 = new StreamWriter(curdir, false))
            {
                int nCount = listCoor.Count;
                for (int i = 0; i < nCount; i++)
                {
                    sw1.WriteLine(listCoor[i]);
                }
            }
        }

        public bool isExistFile(string strFileName)
        {
            return File.Exists(strFileName);
        }
    }
}
