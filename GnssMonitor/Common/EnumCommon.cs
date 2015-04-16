#region 文件说明
//文件功能描述：公共枚举类型文件

//创建者      ：陈明

//创建时间    ：2011-11-18

#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZHDCommon
{
    /// <summary>
    /// 站点类型
    /// </summary>
    public enum StationType
    {
        /// <summary>
        /// GPS基站
        /// </summary>
        GPSBase = 0,

        /// <summary>
        /// GPS监测站
        /// </summary>
        GPSMonitor = 1,

        /// <summary>
        /// 库水位
        /// </summary>
        WaterLevel = 2,

        /// <summary>
        /// 浸润线
        /// </summary>
        PhreaticLine = 3,

        /// <summary>
        /// 降雨量
        /// </summary>
        Rainfall = 4,

        /// <summary>
        /// 内部位移
        /// </summary>
        InterMove = 5,

        /// <summary>
        /// 渗流量
        /// </summary>
        Seepage = 6,

        /// <summary>
        /// 干滩
        /// </summary>
        DrySand = 7,

        /// <summary>
        /// 裂缝程度
        /// </summary>
        Fissure = 10,

        /// <summary>
        /// 库水位干滩
        /// </summary>
        WaterLevelDrySand = -1,

        /// <summary>
        /// 视频
        /// </summary>
        Video = 1000,
        
        //=====add
        /// <summary>
        /// 风速
        /// </summary>
        WindSpeed = 11,
        //========

        /// <summary>
        /// 土压力
        /// </summary>
        EarthPressure = 12,
        
        /// <summary>
        /// 未知类型，可作为初始值或者默认值
        /// </summary>
        UnKnownType = 0x1986
    }
    /// <summary>
    /// 数据统计的类型
    /// </summary>
    public enum StatisticsType
    {
        /// <summary>
        /// 0：一般统计
        /// </summary>
        Normal,

        /// <summary>
        /// 1：分钟统计
        /// </summary>
        Minute,

        /// <summary>
        /// 2：小时统计
        /// </summary>
        Hour,

        /// <summary>
        /// 3：日统计
        /// </summary>
        Day,

        /// <summary>
        /// 4：月统计
        /// </summary>
        Month,

        /// <summary>
        /// 5：年统计
        /// </summary>
        Year
    }

    /// <summary>
    /// 数据库类型
    /// </summary>
    //public enum DBStyle
    //{
    //    /// <summary>
    //    /// SQL SERVER类型数据库
    //    /// </summary>
    //    SqlStyle,  

    //    /// <summary>
    //    /// MySql类型数据库
    //    /// </summary>
    //    MySqlStyle, 

    //    /// <summary>
    //    /// ACCESS类型数据库
    //    /// </summary>
    //    AccessStyle,

    //    /// <summary>
    //    /// Oracle数据库
    //    /// </summary>
    //    OracleStyle,

    //    /// <summary>
    //    /// UnKnown指未知类型数据库
    //    /// </summary>
    //    UnKnown = 0x1986
    //};
}
