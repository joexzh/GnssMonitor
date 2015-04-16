using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using ZHD.SYS.CommonUtility.DatabaseLib;
using ZHDCommon;

namespace NodisDatabase
{
    public class DatabaseManager2
    {
        private DataInfoSaver m_dbMgr = null;
        private int m_cnstST = Convert.ToInt32(StationType.GPSMonitor);

        /// <summary>
        /// 判断数据库是否初始化
        /// </summary>
        /// <returns>
        /// 错误代码
        /// 0：已经初始化
        /// -106：未初始化
        /// </returns>
        protected int IsInitSaver()
        {

            if (m_dbMgr == null) return -106;
            return 0;
        }

        protected int GetStationIdByName(string tbName, out int sId)
        {
            int retValue = -1;
            sId = -1;

            #region 判断数据库是否初始化
            retValue = IsInitSaver();
            if (retValue != 0) return retValue;
            #endregion

            string sql;
            sql = string.Format("select [SID] from [SB_STATION] where [SNAME]='{0}'", tbName);
            DataTable dt = null;
            retValue = m_dbMgr.ExecuteSql(sql, out dt);
            if (retValue == 0)
            {
                if (dt == null)
                {
                    //站点不存在
                    retValue = -114;
                }
                else
                {
                    if (dt.Rows.Count > 0)
                    {
                        //站点已经存在
                        //retValue = -115;
                        try
                        {
                            sId = Convert.ToInt32(dt.Rows[0]["SID"].ToString());
                        }
                        catch (Exception)
                        {
                            retValue = -31;
                        }
                    }
                    else
                    {
                        //站点不存在
                        retValue = -114;
                    }
                }
            }

            return retValue;
        }

        public DatabaseManager2(DBConfigInfo DBConfig)
        {
            m_dbMgr = new DataInfoSaver(DBConfig);
            int retValue = m_dbMgr.Connect();
            if (retValue != 0 && retValue != 50)
            {
                m_dbMgr.Disconnect();
                m_dbMgr = null;
            }
        }

        //数据库是否打开
        public bool IsDatabaseOpen()
        {
            int retValue = -1;
            retValue = IsInitSaver();
            if (retValue != 0)
            {
                return false;
            }

            retValue = m_dbMgr.IsConnect();
            if (retValue != 0 && retValue != 50)
            {
                return false;
            }
            return true;
        }

        //插入一条数据记录
        public bool InsertOneRecord(
            string satName,
            DateTime dt,
            double x,
            double y,
            double h,
            double xDlta,
            double yDlta,
            double hDlta,
            int style
            )
        {
            int retValue = -1;
            if (style == 1)//监测站
            {
                string sqlstring
                = string.Format(
                @"insert into {0}(GPSINDEX,ADATETIME,X,Y,HEIGHT,DLTAX,DLTAY,DLTAH) values({1},'{2}',{3},{4},{5},{6},{7},{8})",
                satName,
                DateTimeToGPSIndex(dt),
                dt.ToString(),
                x,
                y,
                h,
                xDlta,
                yDlta,
                hDlta);
                retValue = m_dbMgr.ExecuteSql(sqlstring, out retValue);
                if (retValue == 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 判断站点是否存在
        /// </summary>
        /// <param name="tbName">站点名称</param>
        /// <returns>
        /// 错误代码
        /// -114：站点不存在
        /// -115：站点已经存在
        /// -50：数据库连接失败
        /// -60：执行SQL失败
        /// -41：数据库未连接
        /// </returns>
        protected int IsTableExist(string tbName)
        {
            int retValue = -1;

            #region 判断数据库是否初始化
            retValue = IsInitSaver();
            if (retValue != 0) return retValue;
            #endregion

            string sql;
            sql = string.Format("select [SID] from [SB_STATION] where [SNAME]='{0}'", tbName);
            DataTable dt = null;
            retValue = m_dbMgr.ExecuteSql(sql, out dt);
            if (retValue == 0)
            {
                if (dt == null)
                {
                    //站点不存在
                    retValue = -114;
                }
                else
                {
                    if (dt.Rows.Count > 0)
                    {
                        //站点已经存在
                        retValue = -115;
                    }
                    else
                    {
                        //站点不存在
                        retValue = -114;
                    }
                }
            }
            return retValue;
        }

        /// <summary>
        /// 在station表中插入一条水位记录，以此判断是否已经创建了该表
        /// </summary>
        /// <param name="tbName">表名称</param>
        /// <returns></returns>
        protected int InsertRecord(string tbName)
        {
            int retValue = -1;

            #region 判断数据库是否初始化
            retValue = IsInitSaver();
            if (retValue != 0) return retValue;
            #endregion

            #region 检查该站点是否已存在
            string strSelectCmd = string.Format(@"select * from [SB_STATION] where 
                [SNAME] = '{0}'", tbName);

            int nReturn;
            DataTable DTable;
            nReturn = m_dbMgr.ExecuteSql(strSelectCmd, out DTable);

            if (0 == nReturn && null != DTable && 0 < DTable.Rows.Count)
            {
                FileOperator.ExceptionLog(GnssMonitor.Properties.Resources.strStationIsExist);
                return nReturn;
            }
            #endregion

            string sql = "";
            switch (m_dbMgr.GetDbStyle())
            {
                case DBStyle.OracleStyle:
                    {
                        sql = string.Format(@"insert into [SB_STATION]([SID],[SNAME],[SSTATUS],[SSN],[SCOMMENT],[SX],[SY],[SH],[SB],[SL],[SHEIGHT],
                                       [SCOORNAME],[SFILTERNAME1], [SFILTERNAME2],[SSTYLE], [ESTYLE], [EMODEL], [EPROVIDER], [ESTARTDATE],
                                       [EINSTALLER], [S1X], [S1Y], [S1H], [SCONFIG]) values(SB_STATION_SID.nextval,'{0}',{1},'{2}', '{3}',
                                       {4},{5},{6},{7},{8},{9},'{10}','{11}','{12}',{13},{14},
                                        '{15}','{16}','{17}','{18}',{19},{20},{21}, '{22}')",
                                tbName,
                                0,
                                "",
                                tbName,
                                0,
                                0,
                                0,
                                0,
                                0,
                                0,
                                tbName + "_CLEAN",
                                tbName + "_LS_CLEAN",
                                tbName + "_HIS",
                                m_cnstST,
                                0,
                                "",
                                "",
                                "",
                                "",
                                0,
                                0,
                                0,
                                "");
                    }
                    break;
                default:
                    {
                        sql =
                            string.Format(@"insert into [SB_STATION]([SNAME],[SSTATUS],[SSN],[SCOMMENT],[SX],[SY],[SH],[SB],[SL],[SHEIGHT],
                                       [SCOORNAME],[SFILTERNAME1], [SFILTERNAME2],[SSTYLE], [ESTYLE], [EMODEL], [EPROVIDER], [ESTARTDATE],
                                       [EINSTALLER], [S1X], [S1Y], [S1H], [SCONFIG]) values('{0}',{1},'{2}', '{3}',
                                       {4},{5},{6},{7},{8},{9},'{10}','{11}','{12}',{13},{14},
                                        '{15}','{16}','{17}','{18}',{19},{20},{21}, '{22}')",
                                tbName,
                                0,
                                "",
                                tbName,
                                0,
                                0,
                                0,
                                0,
                                0,
                                0,
                                tbName + "_CLEAN",
                                tbName + "_LS_CLEAN",
                                tbName + "_HIS",
                                m_cnstST,
                                0,
                                "",
                                "",
                                "",
                                "",
                                0,
                                0,
                                0,
                                "");
                    }
                    break;
            }
            return m_dbMgr.ExecuteSql(sql, out retValue);
        }

        /// <summary>
        /// 主要用于回滚，当插入记录成功，但是创建表失败，我们需要做回滚
        /// </summary>
        /// <param name="tbName">表名称</param>
        /// <returns>
        /// 错误代码
        /// 0：回滚成功
        /// -50：数据库连接失败
        /// -60：执行SQL失败
        /// -41：数据库未连接
        /// </returns>
        protected int DeleteRecord(string tbName)
        {
            int retValue = -1;

            #region 判断数据库是否初始化
            retValue = IsInitSaver();
            if (retValue != 0) return retValue;
            #endregion

            string sql;
            sql =
                string.Format(@"delete from [SB_STATION] where [SNAME]='{0}'", tbName);
            return m_dbMgr.ExecuteSql(sql, out retValue);
        }

        /// <summary>
        /// 创建内部位移数据表格
        /// </summary>
        /// <param name="tbName">表格名称</param>
        /// <returns>
        /// 错误代码
        /// -50：数据库连接失败
        /// -60：执行SQL失败
        /// -41：数据库未连接
        /// -106：未初始化
        /// </returns>
        protected int CreateTable(string tbName)
        {
            int retValue = -1;

            #region 判断数据库是否初始化
            retValue = IsInitSaver();
            if (retValue != 0) return retValue;
            #endregion

            #region GPS表格的创建代码，这里被注析掉，暂时保留
            string sql = "";
            switch (m_dbMgr.GetDbStyle())
            {
                case DBStyle.AccessStyle:
                    {
                        sql = string.Format(@"create table [{0}]([GPSINDEX] double not null primary key,[ADATETIME] datetime,
                                    [X] float,[Y] float,[HEIGHT] float,[DLTAX] float,[DLTAY] float,[DLTAH] float)", tbName);
                        break;
                    }
                case DBStyle.MySqlStyle:
                    {
                        sql = string.Format(@"create table [{0}]([GPSINDEX] bigint not null primary key,[ADATETIME] datetime,
                                    [X] double,[Y] double,[HEIGHT] double,[DLTAX] double,[DLTAY] double,[DLTAH] double)", tbName);
                        break;
                    }
                case DBStyle.SqlStyle:
                    {
                        sql = string.Format(@"create table [{0}]([GPSINDEX] bigint not null primary key,[ADATETIME] datetime,
                                    [X] float,[Y] float,[HEIGHT] float,[DLTAX] float,[DLTAY] float,[DLTAH] float)", tbName);
                        break;
                    }
                case DBStyle.UnKnown:
                    {
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            #endregion
            return m_dbMgr.ExecuteSql(sql, out retValue);
        }

        /// <summary>
        /// 创建筛选统计后的表格
        /// </summary>
        /// <param name="tbName">表格名称</param>
        /// <returns>
        /// 错误代码
        /// -50：数据库连接失败
        /// -60：执行SQL失败
        /// -41：数据库未连接
        /// -106：未初始化
        /// </returns>
        protected int CreateCleanTable(string tbName)
        {
            int retValue = -1;

            #region 判断数据库是否初始化
            retValue = IsInitSaver();
            if (retValue != 0) return retValue;
            #endregion

            string sql = "";
            switch (m_dbMgr.GetDbStyle())
            {
                case DBStyle.AccessStyle:
                    {
                        sql = string.Format(@"create table [{0}]([GPSINDEX] double not null primary key,
                            [ADATETIME] datetime,[X] float,[Y] float,[HEIGHT] float,[XSPEED] float,
                            [YSPEED] float,[HSPEED] float,[STYLE] int,[XACCELERATION] float,[YACCELERATION] float,
                             [HACCELERATION] float)", tbName);
                        break;
                    }
                case DBStyle.MySqlStyle:
                    {
                        sql = string.Format(@"create table [{0}]([GPSINDEX] bigint not null primary key,
                            [ADATETIME] datetime,[X] double,[Y] double,[HEIGHT] double,[XSPEED] double,
                            [YSPEED] double,[HSPEED] double,[STYLE] int,[XACCELERATION] double,[YACCELERATION] double,
                             [HACCELERATION] double)", tbName);
                        break;
                    }
                case DBStyle.SqlStyle:
                    {
                        sql = string.Format(@"create table [{0}]([GPSINDEX] bigint not null primary key,
                            [ADATETIME] datetime,[X] float,[Y] float,[HEIGHT] float,[XSPEED] float,
                            [YSPEED] float,[HSPEED] float,[STYLE] int,[XACCELERATION] float,[YACCELERATION] float,
                             [HACCELERATION] float)", tbName);
                        break;
                    }
                case DBStyle.UnKnown:
                default:
                    {
                        return -106;
                    }
            }
            return m_dbMgr.ExecuteSql(sql, out retValue);
        }

        public bool CreateStationTable(string satName, int style)
        {
            int retValue = -1;

            switch (style)
            {
                case 0:
                    {
                        m_cnstST = Convert.ToInt32(StationType.GPSBase);
                        break;
                    }
                case 1:
                    {
                        m_cnstST = Convert.ToInt32(StationType.GPSMonitor);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            retValue = IsInitSaver();
            if (retValue != 0) return false;

            retValue = IsTableExist(satName);
            if (retValue == -114 || retValue == -115)
            {
                if (retValue == -114)
                {
                    retValue = InsertRecord(satName);
                    if (retValue != 0)
                    {
                        return false;
                    }
                }

                retValue = CreateTable(satName);
                if (retValue != 0)
                {
                    //DeleteRecord(satName);
                    return false;
                }

                retValue = CreateCleanTable(satName + "_CLEAN");
                if (retValue != 0)
                {
                    //DeleteRecord(satName);
                    return false;
                }


                retValue = CreateCleanTable(satName + "_LS_CLEAN");
                if (retValue != 0)
                {
                    //DeleteRecord(satName);
                    return false;
                }


                retValue = CreateCleanTable(satName + "_HIS");
                if (retValue != 0)
                {
                    //DeleteRecord(satName);
                    return false;
                }
            }
            return true;
        }

        //更新站点状态信息-已经自动添加Station头部
        public bool UpdateStationStatus(string satName, int status)
        {
            string sqlstring
                = string.Format(
                "Update [SB_STATION] set [SSTATUS]={0} where [SNAME]='{1}'",
                status,
                satName
                );
            int retValue = -1;
            retValue = m_dbMgr.ExecuteSql(sqlstring, out retValue);
            if (retValue == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        
        //清空卫星数据表格
        public bool DeleteSatInfo(string strSName)
        {
            int nSID = -1;
            int retValue = GetStationIdByName(strSName, out nSID);
            string sqlstring = string.Format("delete from [sat] where sId = {0}", nSID);
            retValue = m_dbMgr.ExecuteSql(sqlstring, out retValue);
            if (retValue == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        //插入一条卫星记录信息-已经自动添加Station头部
        public bool InsertSatInfo(string _sName, int _prn, int _elev, int _azim, int _l1, int _l2)
        {
            int retValue = -1;
            int sId = -1;
            retValue = GetStationIdByName(_sName, out sId);
            string sql = string.Empty;

            retValue = IsInitSaver();
            if (retValue != 0) return false;

            sql = string.Format(
                @"insert into [sat]([SID],[sPrn],[sElev],[sAzm],[sL1],[sL2]) values({0},{1},{2},{3},{4},{5})",
                sId,
                _prn,
                _elev,
                _azim,
                _l1,
                _l2);
            retValue = m_dbMgr.ExecuteSql(sql, out retValue);
            if (retValue == 0)
            {
                return true;
            }
            return false;
        }

        //设置基准站基准坐标-已经自动添加Station头部
        public bool SetBaseCoordinate(string _sName, double _x, double _y, double _h, int _style)
        {
            int retValue = -1;
            string sqlstring
                        = string.Format(
                        @"update [SB_STATION] set [sx]={0},[sy]={1},[sh]={2},[sStyle]={3} where [SNAME]='{4}'",
                        _x,
                        _y,
                        _h,
                        _style,
                        _sName
                        );

            retValue = IsInitSaver();
            if (retValue != 0) return false;

            retValue = m_dbMgr.ExecuteSql(sqlstring, out retValue);
            if (retValue == 0)
            {
                return true;
            }
            return false;
        }

        //设置BLH经纬度坐标-已经自动添加Station头部
        public bool SetStationBLH(string _sName, double _b, double _l, double _h)
        {
            int retValue = -1;
            string sqlstring
                        = string.Format(
                        "update [SB_STATION] set [sB]={0},[sL]={1},[sHeight]={2} where [SNAME]='{3}'",
                        _b,
                        _l,
                        _h,
                        _sName
                        );
            retValue = IsInitSaver();
            if (retValue != 0) return false;

            retValue = m_dbMgr.ExecuteSql(sqlstring, out retValue);
            if (retValue == 0)
            {
                return true;
            }
            return false;
        }

        public void CloseDb()
        {
            if (IsDatabaseOpen() == true)
            {
                m_dbMgr.Disconnect();
            }
        }


        /// <summary>
        /// 创建GPSINFO站点表是否存在
        /// </summary>
        /// <returns></returns>
        public bool CreateGPSInfoTable()
        {
            int retValue = -1;

            #region 判断数据库是否初始化
            retValue = IsInitSaver();
            if (retValue != 0) return false;
            #endregion

            #region GPS表格的创建代码，这里被注析掉，暂时保留
            string sql = string.Format(@"create table [GPSINFO]([GPSID] VARCHAR(50),[NAME] VARCHAR(100),[LOCATION] VARCHAR(100),[X] FLOAT,[Y] FLOAT,[H] FLOAT,[FLAG] INT)");
            #endregion

            retValue = m_dbMgr.ExecuteSql(sql, out retValue);
            if (retValue == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 插入GPSINFO数据,如果已经存在就更新该条记录
        /// </summary>
        /// <param name="_gpsId"></param>
        /// <param name="_name"></param>
        /// <param name="_location"></param>
        /// <param name="_x"></param>
        /// <param name="_y"></param>
        /// <param name="_h"></param>
        /// <param name="_flag"></param>
        /// <returns></returns>
        public bool InsertGpsInfo(string _gpsId, string _name, string _location, double _x, double _y, double _h, int _flag)
        {
            int retValue = -1;

            #region 判断数据库是否初始化
            retValue = IsInitSaver();
            if (retValue != 0) return false;
            #endregion

            string strExecuteCmd = string.Format("select * from GPSINFO where GPSID = '{0}'", _gpsId);
            DataTable StationTable;
            retValue = m_dbMgr.ExecuteSql(strExecuteCmd, out StationTable);
            if (0 == retValue && null != StationTable && 0 < StationTable.Rows.Count)
            {
                strExecuteCmd = string.Format(@"update GPSINFO set [NAME] = '{1}',[LOCATION] = '{2}',
                    [X] = {3},[Y] = {4},[H] = {5},[FLAG] = {6} where [GPSID] = '{0}'",
                     _gpsId,
                    _name,
                    _location,
                    _x,
                    _y,
                    _h,
                    _flag);
                m_dbMgr.ExecuteSql(strExecuteCmd, out retValue);
            }
            else
            {
                string sqlstring = string.Format(
                @"insert into [GPSINFO]([GPSID],[NAME],[LOCATION],[X],[Y],[H],[FLAG]) values('{0}','{1}','{2}',{3},{4},{5},{6})",
                _gpsId,
                _name,
                _location,
                _x,
                _y,
                _h,
                _flag
                );
                retValue = m_dbMgr.ExecuteSql(sqlstring, out retValue);
            }
            if (retValue >= 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void CleanGpsInfo(int nSID)
        {
            int retValue = -1;

            #region 判断数据库是否初始化
            retValue = IsInitSaver();
            if (retValue != 0) return;
            #endregion

            string sqlstring = string.Format("delete from [GPSINFO]");
            m_dbMgr.ExecuteSql(sqlstring, out retValue);
        }

        /// <summary>
        /// 判断该TEMPGPS列表是否存在
        /// </summary>
        /// <returns></returns>
        public bool IsExistTEMPGPSTable()
        {
            return false;
        }

        /// <summary>
        /// 创建TEMPGPS表
        /// </summary>
        /// <returns></returns>
        public bool CreateTEMPGPSTable()
        {
            int retValue = -1;

            #region 判断数据库是否初始化
            retValue = IsInitSaver();
            if (retValue != 0) return false;
            #endregion

            string sqlstring = string.Format(
                @"Create Table [TEMPGPS] ([GPSID] VARCHAR(50),[X] FLOAT,[Y] FLOAT,[H] FLOAT,[DX] FLOAT,[DH] FLOAT,[StartCount] INT,[FLAG] INT,[UPDATIME] DATETIME)");
            retValue = m_dbMgr.ExecuteSql(sqlstring, out retValue);
            if (retValue == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 插入TEMPGPS数据
        /// </summary>
        /// <param name="_gpsId"></param>
        /// <param name="_x"></param>
        /// <param name="_y"></param>
        /// <param name="_h"></param>
        /// <param name="_dx"></param>
        /// <param name="_dh"></param>
        /// <param name="_sCount"></param>
        /// <param name="_flag"></param>
        /// <param name="_dt"></param>
        /// <returns></returns>
        public bool InsertTempGpsInfo(string _gpsId, double _x, double _y, double _h, double _dx, double _dh, int _sCount, int _flag, DateTime _dt)
        {
            int retValue = -1;

            #region 判断数据库是否初始化
            retValue = IsInitSaver();
            if (retValue != 0) return false;
            #endregion

            string sqlstring;
            {
                sqlstring
                = string.Format(
                "insert into [TEMPGPS]([GPSID],[X],[Y],[H],[DX],[DH],[StartCount],[FLAG],[UPDATIME]) values('{0}',{1},{2},{3},{4},{5},{6},{7},'{8}')",
                _gpsId,
                _x,
                _y,
                _h,
                _dx,
                _dh,
                _sCount,
                _flag,
                _dt.ToString("yyyy-MM-dd HH:mm:ss")
                );
            }
            retValue = m_dbMgr.ExecuteSql(sqlstring, out retValue);
            if (retValue == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void DeleteTempGpsInfo(string strGpsID)
        {
            int retValue = -1;

            #region 判断数据库是否初始化
            retValue = IsInitSaver();
            if (retValue != 0) return;
            #endregion
            string sqlstring = string.Format("delete from [TEMPGPS] where GPSID = '{0}'", strGpsID);
            m_dbMgr.ExecuteSql(sqlstring, out retValue);
        }

        /// <summary>
        /// DateTime转成毫秒数
        /// </summary>
        /// <param name="dt">时间</param>
        /// <returns>
        /// 毫秒数
        /// </returns>
        private Int64 DateTimeToGPSIndex(DateTime dt)
        {
            return (Int64)(dt - new DateTime(1970, 1, 1)).TotalMilliseconds;//.TotalSeconds;
        }

        /// <summary>
        /// 毫秒数转成DateTime
        /// </summary>
        /// <param name="uts">毫秒数</param>
        /// <returns>DATETIME时间</returns>
        private  DateTime GPSIndexToDateTime(Int64 uts)
        {
            int dtSeconds = (int)(uts / 1000);
            int dtMilliseconds = (int)(uts % 1000);
            return new DateTime(1970, 1, 1) + new TimeSpan(0, 0, 0, dtSeconds, dtMilliseconds);
        }
    }
}
