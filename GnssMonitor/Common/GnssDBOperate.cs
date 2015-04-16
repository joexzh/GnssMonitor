using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZHD.SYS.CommonUtility.DatabaseLib;
using System.Data;

namespace ZHDCommon
{
    public class GnssDBOperate
    {
        private DataInfoSaver m_dbSaver;
        DBConfigInfo m_dbConfigInfo;

        #region 数据库操作
        /// <summary>
        /// 连接数据库
        /// </summary>
        /// <param name="DBConfig"></param>
        public void LinkDatabase(DBConfigInfo DBConfig)
        {
            m_dbConfigInfo = DBConfig;
            m_dbSaver = new DataInfoSaver(m_dbConfigInfo);
        }

        /// <summary>
        /// 打开数据库
        /// </summary>
        /// <returns></returns>
        public int OpenDatabase()
        {
            if (null == m_dbSaver)
            {
                return -1;
            }
            return m_dbSaver.Connect();
        }

        /// <summary>
        /// 关闭数据库
        /// </summary>
        /// <returns></returns>
        public int Close()
        {
            if (null == m_dbSaver)
            {
                return -1;
            }
            return m_dbSaver.Disconnect();
        }
        #endregion

        public int GetProjectID(out List<int> listPID)
        {
            if (null == m_dbSaver)
            {
                listPID = null;
                return -1;
            }

            int nReturn = -1;
            listPID = new List<int>();
            string strExecuteCmd = "select PID from PROJECT";
            DataTable dtTable;
            nReturn = m_dbSaver.ExecuteSql(strExecuteCmd, out dtTable);

            if (0 == nReturn && null != dtTable && 0 < dtTable.Rows.Count)
            {
                int nCount = dtTable.Rows.Count;
                for (int i = 0; i < nCount; i++)
                {
                    int nPID = int.Parse(dtTable.Rows[i]["PID"].ToString());
                    listPID.Add(nPID);
                }
            }
            return nReturn;
        }

        /// <summary>
        /// 根据工程ID获取工程数据库信息
        /// </summary>
        /// <param name="nProjectID"></param>
        /// <param name="DBInfo"></param>
        public void GetDBInfo(int nProjectID, out DBConfigInfo DBInfo)
        {
            if (null == m_dbSaver)
            {
                DBInfo = null;
                return;
            }

            string strExecuteCmd = string.Format(@"select DBADDRESS,
                   DBNAME, DBPASSNAME, DBPASSWORD from [PROJECT] where 
                [PID] = {0}", nProjectID);

            DBInfo = new DBConfigInfo();
            DataTable StationTable;
            int nReturn = m_dbSaver.ExecuteSql(strExecuteCmd, out StationTable);
            if (null == StationTable || 0 > nReturn || 0 >= StationTable.Rows.Count)
            {
                FileOperator.ExceptionLog(GnssMonitor.Properties.Resources.strNoRecord);
            }

            if (StationTable.Rows[0]["DBADDRESS"] == DBNull.Value || StationTable.Rows[0]["DBNAME"] == DBNull.Value
                 || StationTable.Rows[0]["DBPASSNAME"] == DBNull.Value || StationTable.Rows[0]["DBPASSWORD"] == DBNull.Value)
            {
                FileOperator.ExceptionLog(GnssMonitor.Properties.Resources.strRecordUncompleted);
                return;
            }

            DBInfo.DbServer = StationTable.Rows[0]["DBADDRESS"].ToString();
            DBInfo.DbName = StationTable.Rows[0]["DBNAME"].ToString();
            DBInfo.DbUser = StationTable.Rows[0]["DBPASSNAME"].ToString();
            DBInfo.DbPassword = StationTable.Rows[0]["DBPASSWORD"].ToString();
            DBInfo.DbStyle = m_dbConfigInfo.DbStyle;
        }

        /// <summary>
        /// 获取本地通讯信息
        /// </summary>
        /// <param name="strIP"></param>
        /// <param name="nPort"></param>
        /// <returns></returns>
        public int GetLocalCommInfo(out string strIP, out int nPort)
        {
            strIP = "";
            nPort = -1;
            if (null == m_dbSaver)
            {
                return -1;
            }

            DataTable StationTable = new DataTable();
            string strExecuteCmd = string.Format(@"select * from [COMMUNICATION]");

            int nReturn = m_dbSaver.ExecuteSql(strExecuteCmd, out StationTable);
            if (null == StationTable || 0 > nReturn || 0 >= StationTable.Rows.Count)
            {
                FileOperator.ExceptionLog(GnssMonitor.Properties.Resources.strNoRecord);
                return nReturn;
            }

            if (StationTable.Rows[0]["IPADDRESS"] != DBNull.Value)
            {
                strIP = StationTable.Rows[0]["IPADDRESS"].ToString();
            }
            if (StationTable.Rows[0]["PORT"] != DBNull.Value)
            {
                nPort = int.Parse(StationTable.Rows[0]["PORT"].ToString());
            }

            return nReturn;
        }

        /// <summary>
        /// 获取GNSS站点信息
        /// </summary>
        /// <param name="listGPSStation"></param>
        /// <returns></returns>
        public int GetGPSStation(int nPID, out List<GPSStationInfo> listGPSStation)
        {
            int nReturn = -1;
            if (null == m_dbSaver)
            {
                listGPSStation = null;
                return nReturn;
            }

            listGPSStation = new List<GPSStationInfo>();
            DataTable StationTable = new DataTable();

            string strExecuteCmd = string.Format("select * from GPS_SITE where PID = {0}", nPID);
            nReturn = m_dbSaver.ExecuteSql(strExecuteCmd, out StationTable);

            if (StationTable != null)
            {
                int nRows = StationTable.Rows.Count;
                for (int i = 0; i < nRows; i++)
                {
                    GPSStationInfo myStation = new GPSStationInfo();
                    myStation.p_nPID = Int32.Parse(StationTable.Rows[i][1].ToString());
                    myStation.p_IsBaseSation = StationTable.Rows[i][2].ToString();

                    if (StationTable.Rows[i][3] != DBNull.Value)
                    {
                        myStation.p_StName = StationTable.Rows[i][3].ToString();
                    }
                    if (StationTable.Rows[i][4] != DBNull.Value)
                    {
                        myStation.p_StName2 = StationTable.Rows[i][4].ToString();
                    }
                    if (StationTable.Rows[i][5] != DBNull.Value)
                    {
                        myStation.p_StComment = StationTable.Rows[i][5].ToString();
                    }
                    if (StationTable.Rows[i][6] != DBNull.Value)
                    {
                        myStation.p_St_IP = StationTable.Rows[i][6].ToString();
                    }
                    if (StationTable.Rows[i][7] != DBNull.Value)
                    {
                        myStation.p_St_PORT = Int32.Parse(StationTable.Rows[i][7].ToString());

                    }
                    if (StationTable.Rows[i][8] != DBNull.Value)
                    {
                        myStation.p_St_PORT2 = Int32.Parse(StationTable.Rows[i][8].ToString());
                    }
                    if (StationTable.Rows[i][9] != DBNull.Value)
                    {
                        myStation.p_CoordType = StationTable.Rows[i][9].ToString();
                    }
                    if (StationTable.Rows[i][10] != DBNull.Value)
                    {
                        myStation.p_B_X_x = Convert.ToDouble(StationTable.Rows[i][10].ToString());
                    }
                    if (StationTable.Rows[i][11] != DBNull.Value)
                    {
                        myStation.p_L_Y_y = Convert.ToDouble(StationTable.Rows[i][11].ToString());
                    }
                    if (StationTable.Rows[i][12] != DBNull.Value)
                    {
                        myStation.p_H_Z_h = Convert.ToDouble(StationTable.Rows[i][12].ToString());
                    }
                    if (StationTable.Rows[i][13] != DBNull.Value)
                    {
                        myStation.p_B_X_x_i = Convert.ToDouble(StationTable.Rows[i][13].ToString());
                    }
                    if (StationTable.Rows[i][14] != DBNull.Value)
                    {
                        myStation.p_L_Y_y_i = Convert.ToDouble(StationTable.Rows[i][14].ToString());
                    }
                    if (StationTable.Rows[i][15] != DBNull.Value)
                    {
                        myStation.p_H_Z_h_i = Convert.ToDouble(StationTable.Rows[i][15].ToString());
                    }
                    if (StationTable.Rows[i][16] != DBNull.Value)
                    {
                        myStation.p_AntHeight = Convert.ToDouble(StationTable.Rows[i][16].ToString());
                    }
                    if (StationTable.Rows[i][17] != DBNull.Value)
                    {
                        myStation.p_AntType = StationTable.Rows[i][17].ToString();
                    }
                    if (StationTable.Rows[i][18] != DBNull.Value)
                    {
                        myStation.p_AntDN = Convert.ToDouble(StationTable.Rows[i][18].ToString());
                    }
                    if (StationTable.Rows[i][19] != DBNull.Value)
                    {
                        myStation.p_AntDE = Convert.ToDouble(StationTable.Rows[i][19].ToString());
                    }
                    if (StationTable.Rows[i][20] != DBNull.Value)
                    {
                        myStation.p_RMS = Convert.ToDouble(StationTable.Rows[i][20].ToString());
                    }
                    if (StationTable.Rows[i][21] != DBNull.Value)
                    {
                        myStation.p_ReceiverType = StationTable.Rows[i][21].ToString();
                    }

                    if (StationTable.Rows[i][22] != DBNull.Value)
                    {
                        myStation.p_Sb_Team = StationTable.Rows[i][22].ToString();
                    }
                    if (StationTable.Rows[i][23] != DBNull.Value)
                    {
                        myStation.p_Sb_Password = StationTable.Rows[i][23].ToString();
                    }

                    listGPSStation.Add(myStation);
                }
            }

            return nReturn;
        }

        /// <summary>
        /// 获取setting信息
        /// </summary>
        /// <param name="OptionSet"></param>
        /// <returns></returns>
        public int GetSettingInfo(out OptionSetting OptionSet, bool isGPS, bool isBDS, bool isGlonass)
        {
            int nReturn = -1;
            OptionSet = null;

            if (null == m_dbSaver)
            {
                return nReturn;
            }

            #region
            int nGPS = 0, nBDS = 0, nGlonass = 0;
            if (isGPS)
            {
                nGPS = 1;
            }
            if (isBDS)
            {
                nBDS = 1;
            }
            if (isGlonass)
            {
                nGlonass = 1;
            }

            DataTable StationTable = new DataTable();
            string strExecuteCmd = string.Format(@"select * from GPS_SETTINGS where 
                    SysMark_GPS = {0} and SysMark_BDS = {1} and SysMark_GLONASS = {2}",nGPS, nBDS, nGlonass);
            nReturn = m_dbSaver.ExecuteSql(strExecuteCmd, out StationTable);
            #endregion

            try
            {
                if (StationTable != null && 0 < StationTable.Rows.Count)
                {
                    OptionSet = new OptionSetting();
                    OptionSet.lang = Int32.Parse(StationTable.Rows[0]["Lang"].ToString());////语言，int类型 0 中文 1英文

                    //写参数
                    OptionSet.elevationmask = double.Parse(StationTable.Rows[0]["Elevationmask"].ToString());//卫星高度角 double类型，范围10.0~30.0
                    OptionSet.MoniFilerWindow = Int32.Parse(StationTable.Rows[0]["MoniFilerWindow"].ToString());//平滑窗口大小 int类型
                    OptionSet.FilterMode = Int32.Parse(StationTable.Rows[0]["FilterMode"].ToString());// 结果处理方法，int类型，0 滤波 1 不滤波
                    OptionSet.PosChangeLimit = double.Parse(StationTable.Rows[0]["PosChangeLimit"].ToString());//动态改变基准限制，double 类型
                    OptionSet.SloveMode = Int32.Parse(StationTable.Rows[0]["SloveMode"].ToString());//观测值模型 int类型 

                    OptionSet.MaxIteration = Int32.Parse(StationTable.Rows[0]["MaxIteration"].ToString());//抗差求解迭代次数 int类型 
                    OptionSet.WeightModel = Int32.Parse(StationTable.Rows[0]["WeightModel"].ToString());//抗差求解类型 int类型 
                    OptionSet.MoniinfoShowWindow = Int32.Parse(StationTable.Rows[0]["MoniinfoShowWindow"].ToString());//显示窗口大小 int类型
                    OptionSet.nRunMode = Int32.Parse(StationTable.Rows[0]["RunMode"].ToString());
                    OptionSet.ProcessMode = Int32.Parse(StationTable.Rows[0]["ProcessMode"].ToString());//解算模型 int类型
                    OptionSet.AutoAdjustPos = Int32.Parse(StationTable.Rows[0]["AutoAdjustPos"].ToString());//是否自动校正 int类型
                    OptionSet.StaticGap = double.Parse(StationTable.Rows[0]["StaticGap"].ToString());//静态解时间间隔 double类型
                    OptionSet.ZHDHeaderTime = DateTime.Parse(StationTable.Rows[0]["ZHDHeaderTime"].ToString());//事后模拟时间 string类型
                    OptionSet.WriteDBInterval = Int32.Parse(StationTable.Rows[0]["WriteDBInterval"].ToString());//存储间隔（s) int类型
                    OptionSet.IsWritetoDb = bool.Parse(StationTable.Rows[0]["IsWritetoDb"].ToString());//是否启动数据库 bool类型
                    OptionSet.isSaveLogFile = bool.Parse(StationTable.Rows[0]["IsSaveLogFile"].ToString());//是否输出log文件 bool类型
                    OptionSet.IsSaveBindata = bool.Parse(StationTable.Rows[0]["Issavebindata"].ToString());//是否保存原始电文 bool类型
                    OptionSet.AjustDeformLimit = double.Parse(StationTable.Rows[0]["AjustDeformLimit"].ToString());//静态校正变形阈值 double
                    OptionSet.AjustHD2003Ratio = double.Parse(StationTable.Rows[0]["AjustHD2003Ratio"].ToString());//静态Ratio因子 double
                    OptionSet.AjustHD2003RMS = double.Parse(StationTable.Rows[0]["AjustHD2003RMS"].ToString());//静态RMS double
                    OptionSet.DataInternal = Int32.Parse(StationTable.Rows[0]["DataInternal"].ToString());//接入数据间隔(s) int类型
                    OptionSet.MoniProcessFilterMode = Int32.Parse(StationTable.Rows[0]["MoniProcessFilterMode"].ToString());//定位计算模型 int 
                    OptionSet.RmsLimit = double.Parse(StationTable.Rows[0]["RmsLimit"].ToString());//动态定位RMS限差 double 
                    OptionSet.obssigma = double.Parse(StationTable.Rows[0]["Obssigma"].ToString());//观测噪声 double
                    OptionSet.GapBetweenTwoStaticSlove = double.Parse(StationTable.Rows[0]["GapBetweenTwoStaticSlove"].ToString());//两次校正时间间隔 double类型
                    OptionSet.DatabaseType = Int32.Parse(StationTable.Rows[0]["DatabaseType"].ToString());//数据库类型 int
                    OptionSet.DatabaseVerType = Int32.Parse(StationTable.Rows[0]["DatabaseVerType"].ToString());//自动组网类型 int类型
                    OptionSet.UsedRTKinforMethod = Int32.Parse(StationTable.Rows[0]["UsedRTKinforMethod"].ToString());//RTK结果使用方法
                    OptionSet.AmSearchNum = Int32.Parse(StationTable.Rows[0]["AmSearchNum"].ToString());
                    OptionSet.InitialMinutes = Int32.Parse(StationTable.Rows[0]["InitialMinutes"].ToString());
                    OptionSet.StaticSolveInterval = Int32.Parse(StationTable.Rows[0]["StaticSolveInterval"].ToString());

                    //三星
                    OptionSet.sysMark[0] = Int32.Parse(StationTable.Rows[0]["SysMark_GPS"].ToString());
                    OptionSet.sysMark[1] = Int32.Parse(StationTable.Rows[0]["SysMark_BDS"].ToString());
                    OptionSet.sysMark[2] = Int32.Parse(StationTable.Rows[0]["SysMark_GLONASS"].ToString());
                }
            }
            catch (Exception Error)
            {
                FileOperator.ExceptionLog(Error.Message);
                return -21;     //数据库中数据不完整,存在空值
            }
            return nReturn;
        }

        /// <summary>
        /// 获取基线信息
        /// </summary>
        /// <param name="nPID"></param>
        /// <param name="listBaseLine"></param>
        /// <returns></returns>
        public int GetBaseLineInfo(int nPID, out List<string> listBaseLine)
        {
            int nReturn = -1;
            if (null == m_dbSaver)
            {
                listBaseLine = null;
                return nReturn;
            }
            listBaseLine = new List<string>();
            DataTable StationTable = new DataTable();

            string strExecuteCmd = string.Format("select BASE_MOVE_LINE from GPS_LINECONFIG where PID = {0}", nPID);
            nReturn = m_dbSaver.ExecuteSql(strExecuteCmd, out StationTable);

            if (StationTable != null)
            {
                int nRows = StationTable.Rows.Count;
                for (int i = 0; i < nRows; i++)
                {
                    string strBasePoint = "";
                    if (StationTable.Rows[i][0] != DBNull.Value)
                    {
                        strBasePoint = StationTable.Rows[i][0].ToString();
                    }
                    listBaseLine.Add(strBasePoint);
                }
            }
            return nReturn;
        }

        /// <summary>
        /// 获取坐标信息
        /// </summary>
        /// <param name="nPID"></param>
        /// <param name="listCoor"></param>
        /// <returns></returns>
        public int GetCoordinateInfo(out List<string> listCoor)
        {
            int nReturn = -1;
            if (null == m_dbSaver)
            {
                listCoor = null;
                return nReturn;
            }
            listCoor = new List<string>();
            DataTable StationTable = new DataTable();

            string strExecuteCmd = "select COORDINATE from GPS_COORDINATE";
            nReturn = m_dbSaver.ExecuteSql(strExecuteCmd, out StationTable);

            if (StationTable != null)
            {
                int nRows = StationTable.Rows.Count;
                for (int i = 0; i < nRows; i++)
                {
                    string strBasePoint = "";
                    if (StationTable.Rows[i][0] != DBNull.Value)
                    {
                        strBasePoint = StationTable.Rows[i][0].ToString();
                    }
                    listCoor.Add(strBasePoint);
                }
            }
            return nReturn;
        }

        /// <summary>
        /// 更新sb_status表
        /// </summary>
        /// <param name="isOnline"></param>
        /// <param name="strStationName"></param>
        /// <param name="nPID"></param>
        /// <returns></returns>
        public int UpdateStatus(bool isOnline, string strStationName, int nPID)
        {
            string strExecuteCmd;
            int nOnline = 0;
            int nReturn = -1;

            if (isOnline)
            {
                nOnline = 1;
            }
            else
            {
                nOnline = 0;
            }

            //更新sb_status表中LINKSTATUS属性
            strExecuteCmd = string.Format(@"update [SB_STATUS] set [LINKSTATUS] = {0} 
                where [SNAME] = '{1}' and [PID] = {2}",
                nOnline, strStationName, nPID);
            m_dbSaver.ExecuteSql(strExecuteCmd, out nReturn);

            return nReturn;
        }

        public int InsertStatus(int nPID, string strStationName)
        {
            int nReturn = -1;
            string strExecuteCmd = string.Format(@"select * from SB_STATUS where PID = {0} 
                        and SNAME = '{1}'",nPID, strStationName);
            DataTable dtTable;
            nReturn = m_dbSaver.ExecuteSql(strExecuteCmd, out dtTable);

            if (0 == nReturn && null != dtTable && 0 < dtTable.Rows.Count)
            {
                //更新sb_status表
                strExecuteCmd = string.Format(@"update SB_STATUS set QUITSTATUS = 0 where 
                    SNAME = '{0}' and PID = {1}", strStationName, nPID);
                m_dbSaver.ExecuteSql(strExecuteCmd, out nReturn);
            }
            else
            {
                #region 插入sb_status表记录
                switch (m_dbConfigInfo.DbStyle)
                {
                    #region oracle insert
                    case DBStyle.OracleStyle:
                        strExecuteCmd = string.Format(@"insert into [SB_STATUS](
                                        [SN],
                                        [PID],
	                                    [SNAME],
                                        [QUITSTATUS]) values 
                        (SN.nextval, {0}, '{1}', 0)",
                            nPID, strStationName);
                        break;
                    #endregion
                    #region default insert
                    default:
                        strExecuteCmd = string.Format(@"insert into [SB_STATUS](
                                        [PID],
	                                    [SNAME],
                                        [QUITSTATUS]) values 
                                         ({0}, '{1}', 0)",
                            nPID, strStationName);
                        break;
                    #endregion
                }

                m_dbSaver.ExecuteSql(strExecuteCmd, out nReturn);
                #endregion
            }

            return nReturn;
        }
        
        /// <summary>
        /// 重置sb_status表连接状态
        /// </summary>
        /// <returns></returns>
        public int ResetStatus(int nPID)
        {
            int nReturn = -1;
            string strExecuteCmd = string.Format("update [SB_STATUS] set [LINKSTATUS] = 0 where PID = {0}", nPID);
            m_dbSaver.ExecuteSql(strExecuteCmd, out nReturn);

            return nReturn;
        }
    }
}
