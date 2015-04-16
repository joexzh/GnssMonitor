using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeformationMonitor;
using System.IO;
using System.Windows.Forms;
using ZHDCommon;
using ZHD.SYS.CommonUtility.DatabaseLib;
using System.Threading;
using MoniGPSData;
using GPSBasics;
using NodisDatabase;
using MointorProcessing;
using ZHD.CoordLib;
using Position;
using ZedGraph;
using GPSData;
using ZHD.SYS.CommonUtility.CommunicationLib;
using System.Net;
using System.Net.Sockets;

namespace GnssMonitor
{
    public class GnssManager
    {
        #region 属性
        GPSDecoder m_Decoder = new GPSDecoder();
        DatabaseManager2 m_dbManger2;
        Thread m_DbTread;
        object dbroot = new object();
        public readonly ManualResetEvent DbTransEvent = new ManualResetEvent(false);
        List<GPSStationInfo> m_listStation;
        List<string> m_listLine;
        List<DeviceInfo> m_listDevice = new List<DeviceInfo>();
        private System.Threading.Timer m_SatTimer;

        //传入发送数据库线程的数据
        List<DbMsg> DbStrsForTrans = new List<DbMsg>();
        OptionSetting m_OptionSet;
        CoordinateOperation m_CoorOperate;
        GnssDBOperate m_DBOperate;
        int m_nPID = -1;
        DBConfigInfo m_dbConfig;
        string m_strAppPath = "";

        //各站点各方向形变信息存于此
        public List<List<RollingPointPairList>> stationDat = new List<List<RollingPointPairList>>();

        //各站点最后一次获得形变信息
        public SortedList<string, MoniInfoEventArgs> LatestInfo = new SortedList<string, MoniInfoEventArgs>();
        #endregion

        /// <summary>
        /// 设置多个对象
        /// </summary>
        /// <param name="nPID"></param>
        /// <param name="DBOperate"></param>
        /// <param name="CoorOperate"></param>
        /// <param name="OptionSet"></param>
        public void SetParam(int nPID, GnssDBOperate DBOperate, CoordinateOperation CoorOperate,
            OptionSetting OptionSet, DBConfigInfo dbConfig = null)
        {
            m_nPID = nPID;
            m_DBOperate = DBOperate;
            m_CoorOperate = CoorOperate;
            m_OptionSet = OptionSet;
            m_dbConfig = dbConfig;
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        /// <returns></returns>
        public int StartService()
        {
            int nReturn = -1;
            if (null == m_DBOperate || 0 >= m_nPID)
            {
                MessageBox.Show(Properties.Resources.DBOperateHandleError,
                    Properties.Resources.strPrompt,
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                return nReturn;
            }

            nReturn = m_DBOperate.GetGPSStation(m_nPID, out m_listStation);
            nReturn = m_DBOperate.GetBaseLineInfo(m_nPID, out m_listLine);

            m_strAppPath = Application.ExecutablePath;
            m_strAppPath = m_strAppPath.Substring(0, m_strAppPath.LastIndexOf(@"\") + 1);

            InitSatellite();                //0. 初始化卫星信息
            InitDecoder();                  //1.初始化decoder
            InitDBManager();                //2.初始化工程库连接
            StartAllConnect();              //3.启动该工程的所有站点网络连接
            CalculateAndSaveLocalCoor();    //4.计算各站点当地坐标
            UpdateDatabase();               //5.更新数据库表

            //6.接收到形变数据进行处理
            m_Decoder.Dataprocess.MoniMsg -= new MoniInfoEvent(ReceiveMoniInfo);
            m_Decoder.Dataprocess.MoniMsg += new MoniInfoEvent(ReceiveMoniInfo);
            BuildDataStorageUnit();         // 7.构造数据存储器

            m_SatTimer = new System.Threading.Timer(new TimerCallback(SatTimer_Tick), null, 0, 30000);
            
            return nReturn;
        }

        /// <summary>
        /// 停止本工程服务
        /// </summary>
        public void StopService()
        {
            if (null != m_SatTimer)
            {
                m_SatTimer.Dispose();
            }

            if (null != m_DbTread)
            {
                m_DbTread.Abort();
                m_DbTread.Join();
            }

            int nCount = m_listDevice.Count;
            for (int i = 0; i < nCount; i++)
            {
                if (null != m_listDevice[i].ntripConnector)
                {
                    m_listDevice[i].ntripConnector.Close();
                }
            }
            m_listDevice.Clear();

            if (null != m_DBOperate)
            {
                m_DBOperate.ResetStatus(m_nPID);
            }

            if (null != m_Decoder)
            {
                m_Decoder.EndProcessThread();
            }
        }

        #region 初始化
        /// <summary>
        ///0. 初始化卫星信息
        /// </summary>
        private void InitSatellite()
        {
            //读取C盘下面的历史星历数据(依赖于Document.RunMode，故调至此处）
            int i = 0;
            List<GPSData.NavData> navlist = m_CoorOperate.ReadNavfile();
            for (i = 0; i < navlist.Count; i++)
            {
                MoniGPSData.Satellite.PutinBroadcastEphemeris(navlist[i]);
                MoniConstant.CurrNavWeek = navlist[i].referenceTime.Week;
            }

            List<GPSData.GLONav> gnavlist = m_CoorOperate.ReadGLoNavFile();
            for (i = 0; i < gnavlist.Count; i++)
            {
                MoniGPSData.GloSatellite.AppendEphemeris(gnavlist[i]);
            }

            List<GPSData.CMPNav> cmpnavlist = m_CoorOperate.ReadCMPNavfile();
            for (i = 0; i < cmpnavlist.Count; i++)
            {
                MoniGPSData.CmpSatellite.PutinBroadcastEphemeris(cmpnavlist[i]);
                MoniConstant.CurrNavWeek = cmpnavlist[i].referenceTime.Week;
            }
            //开始往数据库里写的线程
            m_DbTread = new Thread(new ParameterizedThreadStart(DbWriteThread));
            m_DbTread.Start();
        }

        /// <summary>
        /// 1.初始化decoder
        /// </summary>
        private void InitDecoder()
        {
            if (null == m_listStation || null == m_listLine)
            {
                return;
            }

           // m_Decoder = new GPSDecoder();
            m_Decoder.SetOption(m_OptionSet);
            m_Decoder.WorkPathProjectName = m_strAppPath;
            m_Decoder.IsSaveBindata = m_OptionSet.IsSaveBindata;
            m_Decoder.InitStations(m_listStation, m_CoorOperate);
            m_Decoder.InitBaselines(m_listLine);
            m_Decoder.IniMoniSummary();
            m_Decoder.BeginProcessThread();
        }

        /// <summary>
        /// 2.初始化工程库连接
        /// </summary>
        private void InitDBManager()
        {
            //0.建立数据库连接
            try
            {
                if (m_OptionSet.IsWritetoDb == true)
                {
                    if (null != m_DBOperate && 0 < m_nPID)
                    {
                        //当pid为10000时，表示当前只有一个库
                        if (10000 != m_nPID)
                        {
                            m_DBOperate.GetDBInfo(m_nPID, out m_dbConfig);
                        }
                        m_dbManger2 = new DatabaseManager2(m_dbConfig);

                        CreateGnssStationTable();
                    }
                }
            }
            catch (System.Exception ex)
            {
                ZHD.SYS.CommonUtility.DatabaseLib.FileOperator.ExceptionLog(ex.Message);
            }
        }

        private void CreateGnssStationTable()
        {
            if (null == m_dbManger2)
            {
                return;
            }

            //只执行一次，为了使数据库中有参考站的站名，以便于网页终端显示
            for (int i = 0; i < m_Decoder.Dataprocess.StationList.Count; i++)
            {
                string stname = m_Decoder.Dataprocess.StationList[i].ID;
                if (m_Decoder.Dataprocess.StationList[i] is ReferenceStation)
                {
                    m_dbManger2.CreateStationTable(stname, 0);
                }
                else if (m_Decoder.Dataprocess.StationList[i] is MonitorStation)
                {
                    m_dbManger2.CreateStationTable(stname, 1);
                }

                m_DBOperate.InsertStatus(m_nPID, m_Decoder.Dataprocess.StationList[i].ID);
            }
        }

        /// <summary>
        /// 3.启动该工程的所有站点网络连接
        /// </summary>
        private void StartAllConnect()
        {
            string strDevID;

            for (int i = 0; i < m_listStation.Count; i++)
            {
                NetworkPointInfo m_ServerPoint = new NetworkPointInfo();
                m_ServerPoint.pro_LocalIP = IPAddress.Parse(m_listStation[i].p_St_IP.ToString());
                m_ServerPoint.pro_LocalPort = int.Parse(m_listStation[i].p_St_PORT.ToString());
                if (m_OptionSet.sysMark[0] == 1 && m_OptionSet.sysMark[1] == 0 && m_OptionSet.sysMark[2] == 0)
                {
                    strDevID = "GPS$" + m_listStation[i].p_StName;
                }
                else if (m_OptionSet.sysMark[0] == 0 && m_OptionSet.sysMark[1] == 1 && m_OptionSet.sysMark[2] == 0)
                {
                    strDevID = "BDS$" + m_listStation[i].p_StName;
                }
                else
                {
                    strDevID = "GGB$" + m_listStation[i].p_StName;
                }

                NtripClientProtocol ntripClientPro = new NtripClientProtocol();
                ntripClientPro.pro_MountPoint = m_listStation[i].p_Sb_Team.ToString();
                ntripClientPro.pro_UserName = strDevID;
                ntripClientPro.pro_Password = m_listStation[i].p_Sb_Password.ToString();

                TCPClientClass ntripConnector = new TCPClientClass(m_ServerPoint);
                ntripConnector.pro_EnableAutoReconnect = true;
                ntripConnector.pro_ServerPointInfo.pro_CommunicationProtocol = ntripClientPro;
                
                ntripConnector.evConnectServerSuccess += new dConnectServerSuccessEventHandler(ntripConnector_evConnectServerSuccess);
                ntripConnector.evLinkDisconnect += new dLinkDisconnectEventHandler(ntripConnector_evLinkDisconnect);
                ntripConnector.evProtocolConfirm += new dProtocolConfirmEventHandler(ntripConnector_evProtocolConfirm);
                ntripConnector.evDataReceived += new dDataReceivedEventHandler(ntripConnector_evDataReceived);
                ntripConnector.Start();

                //存储设备信息
                DeviceInfo Device = new DeviceInfo();
                Device.nIndex = i;
                Device.strDeviceID = strDevID;
                Device.ntripConnector = ntripConnector;
                m_listDevice.Add(Device);
            }
        }

        /// <summary>
        /// 4.计算各站点当地坐标
        /// </summary>
        private void CalculateAndSaveLocalCoor()
        {
            if (null == m_CoorOperate)
            {
                return;
            }

            //计算各站点的当地坐标
            for (int i = 0; i < m_Decoder.Dataprocess.StationList.Count; i++)
            {
                double x = 0, y = 0, h = 0;
                Coord.BLHtoxyh(m_CoorOperate.myDatumPar, m_CoorOperate.myTempPar, m_Decoder.Dataprocess.StationList[i].StOriGeoPos.b, m_Decoder.Dataprocess.StationList[i].StOriGeoPos.l, m_Decoder.Dataprocess.StationList[i].StOriGeoPos.h, ref x, ref y, ref h);

                m_Decoder.Dataprocess.StationList[i].xyh.e = y;
                m_Decoder.Dataprocess.StationList[i].xyh.n = x;
                m_Decoder.Dataprocess.StationList[i].xyh.u = h;
            }


            //yby 保存当地坐标
            StreamWriter sw = new StreamWriter(m_strAppPath + "/LocalPosition.txt", false);
            sw.WriteLine("Plane Coord:");
            for (int i = 0; i < m_Decoder.Dataprocess.StationList.Count; i++)
            {
                double x = 0;
                double y = 0;
                double h = 0;
                GeoPosition geoPos = m_Decoder.Dataprocess.StationList[i].StOriGeoPos.Clone();
                if (m_Decoder.Dataprocess.StationList[i] is MonitorStation)
                {
                    MonitorStation moniStation = (MonitorStation)m_Decoder.Dataprocess.StationList[i];
                    geoPos = moniStation.StForUpdateStationSpatialPos.ConvertToGeoPosition();
                    Coord.BLHtoxyh(m_CoorOperate.myDatumPar, m_CoorOperate.myTempPar, geoPos.b, geoPos.l, geoPos.h, ref x, ref y, ref h);
                }

                sw.WriteLine(string.Format("{0},{1:f3},{2:f3},{3:f3},   {4:f3},{5:f3},{6:f3}", m_Decoder.Dataprocess.StationList[i].ID,
                    m_Decoder.Dataprocess.StationList[i].xyh.n,
                    m_Decoder.Dataprocess.StationList[i].xyh.e,
                    m_Decoder.Dataprocess.StationList[i].xyh.u, x, y, h));
            }
            sw.WriteLine("\r\nSpatial Coord:");
            for (int i = 0; i < m_Decoder.Dataprocess.StationList.Count; i++)
            {
                SpatialPosition spaPos = (SpatialPosition)(m_Decoder.Dataprocess.StationList[i].StOriSpatialPos.Clone());
                if (m_Decoder.Dataprocess.StationList[i] is MonitorStation)
                {
                    MonitorStation moniStation = (MonitorStation)m_Decoder.Dataprocess.StationList[i];
                    spaPos = (SpatialPosition)(moniStation.StForUpdateStationSpatialPos.Clone());
                }

                sw.WriteLine(string.Format("{0},{1:f3},{2:f3},{3:f3},   {4:f3},{5:f3},{6:f3}", m_Decoder.Dataprocess.StationList[i].ID,
                    m_Decoder.Dataprocess.StationList[i].StOriSpatialPos.x,
                    m_Decoder.Dataprocess.StationList[i].StOriSpatialPos.y,
                    m_Decoder.Dataprocess.StationList[i].StOriSpatialPos.z, spaPos.x, spaPos.y, spaPos.z));
            }
            sw.Close();
        }

        /// <summary>
        /// 5.更新数据库表
        /// </summary>
        private void UpdateDatabase()
        {
            if (m_dbManger2 != null && m_dbManger2.IsDatabaseOpen())
            {
                m_dbManger2.CreateGPSInfoTable();
               // m_dbManger2.CleanGpsInfo();
                for (int i = 0; i < m_Decoder.Dataprocess.StationList.Count; i++)
                {
                    Station station = m_Decoder.Dataprocess.StationList[i];
                    if (station is MonitorStation)
                    {
                        m_dbManger2.InsertGpsInfo(station.ID, station.StationName, station.sComment, station.xyh.n, station.xyh.e, station.xyh.u, 0);
                    }
                }
                if (!m_dbManger2.IsExistTEMPGPSTable())
                {
                    m_dbManger2.CreateTEMPGPSTable();
                }
            }
        }

        /// <summary>
        /// 7.构造数据存储器
        /// </summary>
        private void BuildDataStorageUnit()
        {
            stationDat.Clear();
            for (int i = 0; i < m_Decoder.Dataprocess.StationList.Count; i++)
            {
                //开了一个固定数组，滚动
                double solveGap = 0;
                if (ProcessConfig.ProcessMode == 3)
                {
                    solveGap = ProcessConfig.StaticSolveInterval;
                }
                else
                {
                    solveGap = ProcessConfig.DataInternal;
                }

                RollingPointPairList listx = new RollingPointPairList((int)(3600 / solveGap * m_OptionSet.MoniinfoShowWindow));
                RollingPointPairList listy = new RollingPointPairList((int)(3600 / solveGap * m_OptionSet.MoniinfoShowWindow));
                RollingPointPairList listh = new RollingPointPairList((int)(3600 / solveGap * m_OptionSet.MoniinfoShowWindow));
                List<RollingPointPairList> onestationdatalist = new List<RollingPointPairList>();
                onestationdatalist.Add(listx);
                onestationdatalist.Add(listy);
                onestationdatalist.Add(listh);
                //
                stationDat.Add(onestationdatalist);
            }

            //8.最后一次接收到数据
            this.LatestInfo.Clear();
        }
        #endregion

        //写入数据库线程
        private void DbWriteThread(object parm)
        {
            while (true)
            {
                if (DbTransEvent.WaitOne(1000, false))
                {
                    if (null == m_Decoder)
                    {
                        DbTransEvent.Reset();
                        continue;
                    }

                    try
                    {
                        List<DbMsg> RevMsgs = new List<DbMsg>();
                        lock (dbroot)
                        {
                            for (int ii = 0; ii < DbStrsForTrans.Count; ii++)
                            {
                                RevMsgs.Add(DbStrsForTrans[ii]);
                            }
                            DbStrsForTrans.Clear();
                        }

                        if (m_OptionSet.IsWritetoDb == true)
                        {
                            if (m_dbManger2 != null && m_dbManger2.IsDatabaseOpen())
                            {
                                for (int i = 0; i < RevMsgs.Count; i++)
                                {
                                    m_dbManger2.InsertOneRecord(RevMsgs[i].id, RevMsgs[i].dateTime,
                                        RevMsgs[i].x, RevMsgs[i].y, RevMsgs[i].h, RevMsgs[i].dx, RevMsgs[i].dy, RevMsgs[i].dh, 1);
                                }
                            }
                            else
                            {
                                if (null != m_DBOperate && 0 < m_nPID)
                                {
                                    //当pid为10000时，表示当前只有一个库
                                    if (10000 != m_nPID)
                                    {
                                        m_DBOperate.GetDBInfo(m_nPID, out m_dbConfig);
                                    }
                                    m_dbManger2 = new DatabaseManager2(m_dbConfig);

                                    CreateGnssStationTable();
                                }
                            }
                            
                        }
                    }
                    catch(Exception Error)
                    {
                        ZHD.SYS.CommonUtility.DatabaseLib.FileOperator.ExceptionLog(Error.Message);
                    }
                    DbTransEvent.Reset();
                }
            }

        }

        private void ReceiveMoniInfo(object sender, MoniInfoEventArgs e)
        {
            //写入数据库
            DateTime dt = new DateTime();
            dt = e.Gpstime.ConvertToDatetime() + new TimeSpan(8, 0, 0);//yby 20121010 防止跳秒的情况发生，固定为16
            DbMsg dbmsg = new DbMsg();
            dbmsg.dateTime = dt;
            dbmsg.second = DateTimeToUnixTimestamp(dt, out dbmsg.Millsecond);
            dbmsg.id = e.SationID;
            dbmsg.ymd = int.Parse(dt.ToString("yyyyMMdd"));
            dbmsg.hms = int.Parse(dt.ToString("HHmmss"));

            //转换为当地坐标
            GeoPosition gpos = new SpatialPosition(e.Deform.X, e.Deform.Y, e.Deform.Z).ConvertToGeoPosition();
            double x = 0, y = 0, h = 0;
            Coord.BLHtoxyh(m_CoorOperate.myDatumPar, m_CoorOperate.myTempPar, gpos.b, gpos.l, gpos.h, ref x, ref y, ref h);

            dbmsg.x = x; 
            dbmsg.y = y;
            dbmsg.h = h;
            for (int ii = 0; ii < m_Decoder.Dataprocess.StationList.Count; ii++)
            {
                if (m_Decoder.Dataprocess.StationList[ii].ID == dbmsg.id)
                {
                    dbmsg.dx = (x - m_Decoder.Dataprocess.StationList[ii].xyh.n) * 1000;
                    dbmsg.dy = (y - m_Decoder.Dataprocess.StationList[ii].xyh.e) * 1000;
                    dbmsg.dh = (h - m_Decoder.Dataprocess.StationList[ii].xyh.u) * 1000;
                    break;
                }
            }
            //间隔一定时间写一个数据库点
            double RoundSecond = Math.Round(e.Gpstime.Second, 2);
            double isecond = Math.Round(RoundSecond / m_OptionSet.WriteDBInterval);
            if (Math.Abs(RoundSecond - isecond * m_OptionSet.WriteDBInterval) < 1e-6)
            {
                lock (this.dbroot)
                {
                    this.DbStrsForTrans.Add(dbmsg);
                }
                DbTransEvent.Set();
            }

            int idx = -1;
            for (int i = 0; i < m_Decoder.Dataprocess.StationList.Count; i++)
            {
                if (m_Decoder.Dataprocess.StationList[i].ID == e.SationID)
                {
                    idx = i;
                    break;
                }
            }
            if (idx >= 0)
            {
                //将形变数据存起来
                XDate time = new XDate(dt);

                //数据合法性检查
                if (double.IsInfinity(dbmsg.dx) ||
                    double.IsNaN(dbmsg.dx) ||
                    double.IsInfinity(dbmsg.dy) ||
                    double.IsNaN(dbmsg.dy) ||
                    double.IsInfinity(dbmsg.dh) ||
                    double.IsNaN(dbmsg.dh)
                    )
                {
                    MessageBox.Show(Properties.Resources.strDataError);
                    return;
                }
                //yby  改为画当地坐标的坐标差
                stationDat[idx][0].Add(time, dbmsg.dx);//以mm作为单位
                stationDat[idx][1].Add(time, dbmsg.dy);
                stationDat[idx][2].Add(time, dbmsg.dh);
                //更新最后一次的形变信息
                if (this.LatestInfo.ContainsKey(e.SationID))
                {
                    MoniInfoEventArgs showInfo = e.Clone();
                    showInfo.Gpstime = new GPStime(dt);
                    LatestInfo[e.SationID] = showInfo;
                }
                else
                {
                    MoniInfoEventArgs showInfo = e.Clone();
                    showInfo.Gpstime = new GPStime(dt);
                    LatestInfo.Add(e.SationID, showInfo);
                }
            }
        }

        private int DateTimeToUnixTimestamp(DateTime dt, out int millSecond)
        {
            double totalsecond = (dt - new DateTime(1970, 1, 1)).TotalSeconds;
            int second = (int)totalsecond;
            millSecond = (int)Math.Round((totalsecond - second) * 1000);
            return second;
        }

        #region 通讯回调
        private void ntripConnector_evConnectServerSuccess(CommunicationPointInfoBase sender)
        {
        }

        private void ntripConnector_evLinkDisconnect(CommunicationPointInfoBase serverPointInfo, EventArgs errorArgs)
        {
            int nCount = m_listDevice.Count;
            for (int i = 0; i < nCount; i++)
            {
                if (m_listDevice[i].strIP == ((NetworkPointInfo)serverPointInfo).pro_RemoteIP.ToString() &&
                    m_listDevice[i].nPort == ((NetworkPointInfo)serverPointInfo).pro_RemotePort)
                {
                    m_DBOperate.UpdateStatus(false, m_listDevice[i].strDeviceID, m_nPID);
                    break;
                }
            }
        }

        private void ntripConnector_evProtocolConfirm(NetworkPointInfo sender, EventArgs args)
        {
            NtripClientProtocol gnssProtocol = sender.pro_CommunicationProtocol as NtripClientProtocol;
            
            if (null == gnssProtocol)
            {
                return;
            }

            string strDevID = "";
            strDevID = gnssProtocol.pro_UserName;
            if ("" == strDevID)
            { 
                return;
            }

            int nCount = m_listDevice.Count;
            for (int i = 0; i < nCount; i++)
            {
                if (strDevID == m_listDevice[i].strDeviceID)
                {
                    m_listDevice[i].strIP = sender.pro_RemoteIP.ToString();
                    m_listDevice[i].nPort = sender.pro_RemotePort;
                    m_DBOperate.UpdateStatus(true, m_listDevice[i].strDeviceID, m_nPID);
                    break;
                }
            }
        }

        private void ntripConnector_evDataReceived(CommunicationPointInfoBase sender, CommunicationDataBase data)
        {
            int nCount = m_listDevice.Count;
            for (int i = 0; i < nCount; i++)
            {
                if (m_listDevice[i].strIP == ((NetworkPointInfo)sender).pro_RemoteIP.ToString() &&
                    m_listDevice[i].nPort == ((NetworkPointInfo)sender).pro_RemotePort)
                {
                    DataPackage dpData = new DataPackage();
                    dpData.BinaryData = data.pro_Data;
                    dpData.DataType = 0;
                    dpData.sindex = m_listDevice[i].nIndex;
                    m_Decoder.PutinBinaryData(dpData);
                    break;
                }
            }
        }
        #endregion

        private void SatTimer_Tick(Object sender)
        {
            if (!m_OptionSet.IsWritetoDb)
            {
                return ;
            }
            try
            {
                int i, j;

                if (m_dbManger2 != null && m_dbManger2.IsDatabaseOpen())
                {
                    for (i = 0; i < m_Decoder.Dataprocess.StationList.Count; i++)
                    {
                        Station station = m_Decoder.Dataprocess.StationList[i];
                        m_dbManger2.DeleteSatInfo(station.ID);      //一个个删，影响效率，但是针对多解算模式只能这样

                        if (station.CurrentListIndex < 0)
                            continue;
                        for (j = 0; j < station.UdGPSDatas[station.CurrentListIndex].prnForL1AboveCutAngle.Count; j++)
                        {
                            int tempprn = station.UdGPSDatas[station.CurrentListIndex].prnForL1AboveCutAngle[j];

                            m_dbManger2.InsertSatInfo(station.ID, tempprn, (int)(station.UdGPSDatas[station.CurrentListIndex].Ele[tempprn] * 180 / Math.PI), (int)(station.UdGPSDatas[station.CurrentListIndex].Azi[tempprn] * 180 / Math.PI),
                                (int)station.UdGPSDatas[station.CurrentListIndex].S1[tempprn], (int)station.UdGPSDatas[station.CurrentListIndex].S2[tempprn]);
                        }
                    }

                    //yby 110709 为成都项目增加临时状态显示表
                    for (i = 0; i < m_Decoder.Dataprocess.StationList.Count; i++)
                    {
                        if (m_Decoder.Dataprocess.StationList[i] is ReferenceStation)
                        {
                            continue;
                        }
                        MonitorStation Monist = (MonitorStation)(m_Decoder.Dataprocess.StationList[i]);
                        m_dbManger2.DeleteTempGpsInfo(Monist.ID);  //一个个删，影响效率，但是针对多解算模式只能这样
                        GPStime gt = new GPStime();
                        Deformation deform = new Deformation();
                        int satenum = 0;
                        Monist.GetCurrentMoniInfo(ref gt, ref satenum, ref deform);
                        double x = 0, y = 0, h = 0;
                        if (deform.X == 0 || deform.Y == 0 || deform.Z == 0)//刚启动时
                        {
                            x = m_Decoder.Dataprocess.StationList[i].xyh.n;
                            y = m_Decoder.Dataprocess.StationList[i].xyh.e;
                            h = m_Decoder.Dataprocess.StationList[i].xyh.u;
                        }
                        else
                        {
                            GeoPosition gpos = new SpatialPosition(deform.X, deform.Y, deform.Z).ConvertToGeoPosition();
                            Coord.BLHtoxyh(m_CoorOperate.myDatumPar, m_CoorOperate.myTempPar, gpos.b, gpos.l, gpos.h, ref x, ref y, ref h);
                        }
                        double dx = (x - m_Decoder.Dataprocess.StationList[i].xyh.n) * 1000;
                        double dy = (y - m_Decoder.Dataprocess.StationList[i].xyh.e) * 1000;
                        double dh = (h - m_Decoder.Dataprocess.StationList[i].xyh.u) * 1000;
                        double ddis = Math.Sqrt(dx * dx + dy * dy);

                        DateTime dt = new DateTime();
                        dt = gt.ConvertToDatetime() + new TimeSpan(8, 0, -MoniConstant.LeapSecond);

                        int flag = 0;//目前设定为如果与当前时刻超过1个小时认为是断线
                        if ((DateTime.Now - dt).TotalSeconds > 60 * 3600)
                        {
                            flag = -1;
                        }
                            
                        m_dbManger2.InsertTempGpsInfo(Monist.ID, x, y, h, ddis, dh, satenum, flag, dt);
                    }
                }
                else
                {
                    if (null != m_DBOperate && 0 < m_nPID)
                    {
                        //当pid为10000时，表示当前只有一个库
                        if (10000 != m_nPID)
                        {
                            m_DBOperate.GetDBInfo(m_nPID, out m_dbConfig);
                        }
                        m_dbManger2 = new DatabaseManager2(m_dbConfig);

                        CreateGnssStationTable();
                    }
                }
            }
            catch (System.Exception ex)
            {
                ZHD.SYS.CommonUtility.DatabaseLib.FileOperator.ExceptionLog(ex.Message);
            }
        }
    }
}
