using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZHDCommon
{
    public class GPSStationInfo
    {
        #region 字段
        private int nPID;
        private string IsBaseSation;//是否基站
        private string StName;//站点名称
        private string StName2;//标记，类似站点别名
        private string StComment;//备注
        private string Sb_Team;//设备分组
        private string Sb_Password;//密码

        private string St_IP;//服务器IP
        private int St_PORT;//主端口
        private int St_PORT2;//副端口

        private string CoordType;//坐标系类型
        private double B_X_x;//基准坐标B_X_x
        private double L_Y_y;//基准坐标L_Y_y
        private double H_Z_h;//基准坐标H_Z_h

        //监测坐标
        private double B_X_x_i;
        private double L_Y_y_i;
        private double H_Z_h_i;

        private double AntHeight;//天线高
        private string AntType;//天线类型
        private string ReceiverType;//机器类型

        private double AntDN;//北偏移
        private double AntDE;//东偏移
        private double RMS;//精度
        #endregion

        #region 属性
        public string p_IsBaseSation//是否基站
        {
            get { return IsBaseSation; }
            set { IsBaseSation = value; }
        }
        public string p_StName//站点名称
        {
            get { return StName; }
            set { StName = value; }
        }
        public string p_StName2//标记，类似站点别名
        {
            get { return StName2; }
            set { StName2 = value; }
        }
        public string p_StComment//备注
        {
            get { return StComment; }
            set { StComment = value; }
        }
        public string p_Sb_Team//设备分组
        {
            get { return Sb_Team; }
            set { Sb_Team = value; }
        }
        public string p_Sb_Password//密码
        {
            get { return Sb_Password; }
            set { Sb_Password = value; }
        }

        public string p_St_IP//服务器IP
        {
            get { return St_IP; }
            set { St_IP = value; }
        }
        public int p_St_PORT//主端口
        {
            get { return St_PORT; }
            set { St_PORT = value; }
        }
        public int p_St_PORT2//副端口
        {
            get { return St_PORT2; }
            set { St_PORT2 = value; }
        }

        public string p_CoordType//坐标系类型
        {
            get { return CoordType; }
            set { CoordType = value; }
        }
        public double p_B_X_x//基准坐标B_X_x
        {
            get { return B_X_x; }
            set { B_X_x = value; }
        }
        public double p_L_Y_y//基准坐标L_Y_y
        {
            get { return L_Y_y; }
            set { L_Y_y = value; }
        }
        public double p_H_Z_h//基准坐标H_Z_h
        {
            get { return H_Z_h; }
            set { H_Z_h = value; }
        }

        //监测坐标
        public double p_B_X_x_i
        {
            get { return B_X_x_i; }
            set { B_X_x_i = value; }
        }
        public double p_L_Y_y_i
        {
            get { return L_Y_y_i; }
            set { L_Y_y_i = value; }
        }
        public double p_H_Z_h_i
        {
            get { return H_Z_h_i; }
            set { H_Z_h_i = value; }
        }

        public double p_AntHeight//天线高
        {
            get { return AntHeight; }
            set { AntHeight = value; }
        }
        public string p_AntType//天线类型
        {
            get { return AntType; }
            set { AntType = value; }
        }
        public string p_ReceiverType//机器类型
        {
            get { return ReceiverType; }
            set { ReceiverType = value; }
        }

        public double p_AntDN//北偏移
        {
            get { return AntDN; }
            set { AntDN = value; }
        }
        public double p_AntDE//东偏移
        {
            get { return AntDE; }
            set { AntDE = value; }
        }
        public double p_RMS//精度
        {
            get { return RMS; }
            set { RMS = value; }
        }
        public int p_nPID
        {
            get { return nPID; }
            set { nPID = value; }
        }

        #endregion
    }
}
