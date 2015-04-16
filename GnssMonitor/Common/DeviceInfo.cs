using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZHD.SYS.CommonUtility.CommunicationLib;

namespace ZHDCommon
{
    public class DeviceInfo
    {
        /// <summary>
        /// 设备索引
        /// </summary>
        public int nIndex;
        /// <summary>
        /// 设备ID
        /// </summary>
        public string strDeviceID;
        /// <summary>
        /// 站点本地IP
        /// </summary>
        public string strIP;
        /// <summary>
        /// 站点本地端口
        /// </summary>
        public int nPort;

        /// <summary>
        /// 站点网络连接对象
        /// </summary>
        public TCPClientClass ntripConnector;
    }
}
