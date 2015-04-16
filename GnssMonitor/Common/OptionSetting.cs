using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZHDCommon
{
    public class OptionSetting
    {
        #region【解算部分参数】

        public double elevationmask = 10;//卫星高度截至角
        //型变量滤波窗口大小
        public int MoniFilerWindow = 1800;
        public int FilterMode = 1; //0 不滤波, 1 均值滤波
        public double PosChangeLimit = 10000.0;//自动变更监测点位置的阈值
        public int SloveMode = 1;  //1 L1, 2 L1L2, 3 Ifree ,4 L1epoch 
        public int MaxIteration = 2;
        public int WeightModel = 1;
        //yby 10.12.15 显示变形信息窗口的大小
        public double MoniinfoShowWindow = 4;
        //yby 10.12.30 是否自动校准各测站位置
        public int AutoAdjustPos = 1;//0 静态校正 1 自主RTK校正 2机载RTK校正 3 无
        //静动态处理模型
        public int ProcessMode = 0;//0 dynamic单历元 1 动态（自主RTK）2 动态（机载RTK） 3
        //静态或者动态校正的时间间隔
        public double StaticGap = 1.0;
        public int StaticSolveInterval = 300;
        public DateTime ZHDHeaderTime = new DateTime(2010, 1, 7);

        //默认一秒一次写数据入数据库
        public double WriteDBInterval = 1;

        public bool IsWritetoDb = false;
        public bool isSaveLogFile = true;

        public bool IsSaveBindata = true;

        public double AjustDeformLimit = 0.01;
        public double AjustHD2003Ratio = 8;
        public double AjustHD2003RMS = 0.01;

        //110811添加
        public int DatabaseType = 0;
        //在不存在LineConfig的情况下如何形成基线 0 单基站 1 多基站
        public int DatabaseVerType = 0;

        //yby 110901添加 接受监测站得RTK定位信息和基准站得差分信息
        public int UsedRTKinforMethod = 0;//0 不使用RTK，1 只用RTK，2 RTK辅助校正监测站监测坐标

        //yby 110921添加 
        public int AmSearchNum = 3;//模糊度搜索空间
        public int InitialMinutes = 0;//过多长时间开始进入数据库(分钟)

        //接受原始数据时间间隔
        public double DataInternal = 5;

        //动态求解采用依赖基准的SloveMode时定位解算模型
        public int MoniProcessFilterMode = 0;//动态依赖基准：0 单历元解 1 卡尔曼滤波解
        //质量控制
        public double RmsLimit = 0.01;
        public double obssigma = 0.002;

        public double GapBetweenTwoStaticSlove = 24;
        #endregion

        //语言0=cn,1=en
        public int lang = 0;

        //tya 130808
        public int[] sysMark = new int[3];//0-GPS;1-BD;2-GLONASS

        /// <summary>
        /// 运行模式
        /// </summary>
        public int nRunMode = 1;
    }
}
