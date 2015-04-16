using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZHDCommon;
using ZHD.SYS.CommonUtility.CommunicationLib;
using ZHD.SYS.CommonUtility.DatabaseLib;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.IO;

namespace GnssMonitor
{
    class LocalCommunication
    {
        private bool m_isSensorStart;               //用于标记传感器服务是否启动
        private TCPClientClass m_TCPClientObj;      //用于模块间通讯，根据发来的信息重新读库
        private DBConfigInfo m_DBConfigInfo;  //数据库配置信息
        private GnssDBOperate m_DBOperate;
        private bool m_isDatabaseOpen = false;
        private List<GnssManager> m_listGnss = new List<GnssManager>();

        bool m_isGPS = false;
        bool m_isBDS = false;
        bool m_isGlonass = false;

        /// <summary>
        /// 构造函数，初始化参数
        /// </summary>
        public LocalCommunication()
        {
            m_DBOperate = new GnssDBOperate();
            m_isSensorStart = false;
        }

        public void SetProcessMode(bool isGPS, bool isBDS, bool isGlonass)
        {
            m_isGPS = isGPS;
            m_isBDS = isBDS;
            m_isGlonass = isGlonass;
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        /// <param name="DBConfigInfo">数据库连接信息</param>
        /// <returns></returns>
        public bool StartService(DBConfigInfo DBConfig, SetFileRW SetFile)
        {
            //连接并打开数据库
            m_DBOperate.LinkDatabase(DBConfig);
            int nReturn = m_DBOperate.OpenDatabase();

            if (0 > nReturn)
            {
                ZHD.SYS.CommonUtility.DatabaseLib.FileOperator.ExceptionLog(
                    string.Format(Properties.Resources.strDatabaseLinkError, DBConfig.DbName));
                return false;
            }

            m_isDatabaseOpen = true;
            m_DBConfigInfo = DBConfig;

            #region //初始化解算
            #region 坐标系统
            List<string> listCoordinate;
            CoordinateOperation CoorOperate = new CoordinateOperation();
            nReturn = m_DBOperate.GetCoordinateInfo(out listCoordinate);

            if (0 == nReturn && null != listCoordinate && 0 < listCoordinate.Count && null != SetFile)
            {
                CoorOperate.DatumName = "LocalCoor";
                SetFile.WriteCoordinateDam(CoorOperate.DatumName + ".dam", listCoordinate);

                string AppPath = Application.ExecutablePath;
                AppPath = AppPath.Substring(0, AppPath.LastIndexOf(@"\") + 1);
                MointorProcessing.ProcessConfig.WorkPath = AppPath + "ZmoniWork";     //add2014.9.28
                if (!Directory.Exists(MointorProcessing.ProcessConfig.WorkPath))
                {
                    Directory.CreateDirectory(MointorProcessing.ProcessConfig.WorkPath);
                }

                //读取椭球文件
                string strFilePath = Path.Combine(AppPath, "Ellipse.csv");
                if(SetFile.isExistFile(strFilePath))
                {
                    CoorOperate.LoadEllipsoidnDatum(strFilePath);
                    CoorOperate.LoadDatum();//加载坐标转换参数
                }
                else
                {
                    ZHD.SYS.CommonUtility.DatabaseLib.FileOperator.ExceptionLog(
                        string.Format(Properties.Resources.strFileNotExist, "Ellipse.csv"));
                    return false;
                }
            }
            else
            {
                ZHD.SYS.CommonUtility.DatabaseLib.FileOperator.ExceptionLog(
                    string.Format(Properties.Resources.strFileNotExist, "gps_coordinate"));
                return false;
            }
            #endregion

            //获取setting信息
            OptionSetting OptionSet;
            nReturn = m_DBOperate.GetSettingInfo(out OptionSet, m_isGPS, m_isBDS, m_isGlonass);
            if (0 != nReturn || null == OptionSet)
            {
                ZHD.SYS.CommonUtility.DatabaseLib.FileOperator.ExceptionLog(
                    string.Format(Properties.Resources.strFileNotExist, "gps_settings"));
                return false;
            }

            //启动服务
            CoorOperate.Apply2ProcessConfig(OptionSet);
            List<int> listProjectID;
            nReturn = m_DBOperate.GetProjectID(out listProjectID);
            if (0 != nReturn || null == listProjectID || 0 >= listProjectID.Count)
            {
                //当没有project表或表中没有记录时，默认pid为10000
                GnssManager GnssMan = new GnssManager();
                GnssMan.SetParam(10000, m_DBOperate, CoorOperate, OptionSet, m_DBConfigInfo);
                GnssMan.StartService();
                m_listGnss.Add(GnssMan);
            }
            else
            {
                int nCount = listProjectID.Count;
                for (int j = 0; j < nCount; j++)
                {
                    GnssManager GnssMan = new GnssManager();
                    GnssMan.SetParam(listProjectID[j], m_DBOperate, CoorOperate, OptionSet);
                    GnssMan.StartService();
                    m_listGnss.Add(GnssMan);
                }
            }
            #endregion

            string strLocalIP = "";
            int nLocalPort = -1;
            m_DBOperate.GetLocalCommInfo(out strLocalIP, out nLocalPort);

            if ("" == strLocalIP || 0 >= nLocalPort || 65536 < nLocalPort) //add2014.8.4添加65536限制
            {
                return true;
            }

            NetworkPointInfo TCPPoint = new NetworkPointInfo();          //连接服务器点的信息

            TCPPoint.pro_LocalIP = IPAddress.Parse(strLocalIP);
            TCPPoint.pro_LocalPort = nLocalPort;        //固定，专用于各个模块的通讯服务器
            m_TCPClientObj = new TCPClientClass(TCPPoint);

            //添加各类回调
            m_TCPClientObj.pro_EnableAutoReconnect = true;
            m_TCPClientObj.evConnectServerSuccess += new dConnectServerSuccessEventHandler(m_TCPClientObj_evConnectServerSuccess);
            m_TCPClientObj.evDataReceived += new dDataReceivedEventHandler(DataReceived);
            
            m_isSensorStart = m_TCPClientObj.Start();

            return m_isSensorStart;
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        public void StopService()
        {
            int nCount = m_listGnss.Count;
            for (int i = 0; i < nCount; i++)
            {
                m_listGnss[i].StopService();
            }
            m_listGnss.Clear();

            if (m_isDatabaseOpen && null != m_DBOperate)
            {
                m_DBOperate.Close();
            }

            if (null == m_TCPClientObj)
            {
                return;
            }

            m_TCPClientObj.evConnectServerSuccess -= new dConnectServerSuccessEventHandler(m_TCPClientObj_evConnectServerSuccess);
            m_TCPClientObj.evDataReceived -= new dDataReceivedEventHandler(DataReceived);
            m_TCPClientObj.Close();
            m_TCPClientObj = null;
        }

        /// <summary>
        /// 连接服务器后发送注册信息
        /// </summary>
        /// <param name="sender">通信点信息</param>
        private void m_TCPClientObj_evConnectServerSuccess(CommunicationPointInfoBase sender)
        {
            string strRegister = "register$gnss";
            SendMsg(strRegister, sender);
        }

        /// <summary>
        /// 数据接收，重启服务
        /// </summary>
        /// <param name="sender">通信点信息</param>
        /// <param name="data">接收到的数据</param>
        private void DataReceived(CommunicationPointInfoBase sender, CommunicationDataBase data)
        {
            string strMsg = System.Text.Encoding.Default.GetString(data.pro_Data).TrimEnd();

            //用$分离字符串
            char[] colonSplitChar = new char[] { '$' };
            string[] userDetailInfo = strMsg.Split(colonSplitChar, StringSplitOptions.RemoveEmptyEntries);

            //重启并回应信息
            if ("gnss" == userDetailInfo[0].ToLower() && null != m_DBConfigInfo)
            {
                string strRespond = "config$gnss";
                SendMsg(strRespond, sender);
            }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="strMsg">要发送的数据</param>
        /// <param name="sender">通信点信息</param>
        private void SendMsg(string strMsg, CommunicationPointInfoBase sender)
        {
            CommunicationDataBase cmData = new CommunicationDataBase();
            cmData.pro_Data = System.Text.Encoding.ASCII.GetBytes(strMsg);
            if (null != m_TCPClientObj)
            {
                m_TCPClientObj.Send(cmData, sender);
            }
        }
    }
}
