using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using MoniGPSData;
using AntennaAndReceiver;
using Position;
using System.IO;
using GPSBasics;
using ZHDCommon;
using MointorProcessing;

namespace DeformationMonitor
{

    //
    public class GPSDecoder
    {
        //
        public CDataProcessMain Dataprocess = new CDataProcessMain();

        //计算线程
        Thread ProcessTread;
        object sysroot=new object();
        public readonly ManualResetEvent PRocessTransEvent = new ManualResetEvent(false);
        //传入计算线程的数据

        List<DataPackage> DataForTrans = new List<DataPackage>();
        //
        public string WorkPathProjectName = "C:\\";
        public bool IsSaveBindata = true;
        private OptionSetting m_OptionSet;

        public void SetOption(OptionSetting OptionSet)
        {
            m_OptionSet = OptionSet;
        }

        //计算线程中的数据处理入口函数
        public void PutinBinaryData(DataPackage data)
        {
            //构造文件存储目录

            #region //创建保存数据的文件夹
            string path = WorkPathProjectName + "\\data" + "\\Year_" + DateTime.Now.Year.ToString("d4") + "\\Month_" + DateTime.Now.Month.ToString("d2") + "\\Day_" + DateTime.Now.Day.ToString("d2") + "\\";
            if (!Directory.Exists(path))
            {
                //删除过期binary数据
                int year = DateTime.Now.Year;
                int premonth = DateTime.Now.Month - 1;
                if (premonth < 1)
                {
                    premonth = 12;
                    year = year - 1;
                }
                string prepath = WorkPathProjectName + "\\data" + "\\Year_" + year.ToString("d4") + "\\Month_" + premonth.ToString("d2");
                if (Directory.Exists(prepath))
                {
                    Directory.Delete(prepath, true);
                }

                //创建当天的保存目录

                Directory.CreateDirectory(path);
            }
            #endregion

            #region 
            if (IsSaveBindata)
            {
                //0 原始二进制电文
                if (data.DataType == 0)
                {
                    string file = path + Dataprocess.StationList[data.sindex].ID + ".zhd";
                    FileStream fsdaily;
                    BinaryWriter bwdaily;
                    if (!File.Exists(file))
                    {
                        //输入文件头
                        fsdaily = new FileStream(file, FileMode.Create);
                        bwdaily = new BinaryWriter(fsdaily);

                        #region//zhd head
                        byte[] staticbuff = new byte[672];
                        int i = 0;
                        int wYear = DateTime.Now.Year % 100;

                        string strzhd = "";
                        if (Dataprocess.StationList[data.sindex].ReceiverOBJ.ReceiverType == "CSI")
                        {
                            strzhd = "ZHD COLLECTED DATA FILE\r\nver 60.0\r\n";
                        }
                        else if (Dataprocess.StationList[data.sindex].ReceiverOBJ.ReceiverType == "RTCM3")
                        {
                            strzhd = "ZHD COLLECTED DATA FILE\r\nver 97.0\r\n";//yby 加

                        }
                        else if (Dataprocess.StationList[data.sindex].ReceiverOBJ.ReceiverType == "NOVATEL")
                        {
                            strzhd = "ZHD COLLECTED DATA FILE\r\nver 54.0\r\n";
                        }
                        else if (Dataprocess.StationList[data.sindex].ReceiverOBJ.ReceiverType == "TRIMBLE")
                        {
                            strzhd = "ZHD COLLECTED DATA FILE\r\nver 90.0\r\n";
                        }
                        else if (Dataprocess.StationList[data.sindex].ReceiverOBJ.ReceiverType == "UNICORE_UB240")
                        {
                            strzhd = "ZHD COLLECTED DATA FILE\r\nver 100.0\r\n";
                        }
                        byte[] tempbytes = Encoding.ASCII.GetBytes(strzhd);
                        Array.Copy(tempbytes, 0, staticbuff, 0, tempbytes.Length);

                        if (m_OptionSet.nRunMode == 2)//事后模拟时，如果是RTCM3电文用当前时间会出错，必须是文件的记录时间，手动设置
                        {
                            strzhd = string.Format("Date:{0:d4}/{1:d2}/{2:d2}", m_OptionSet.ZHDHeaderTime.Year, m_OptionSet.ZHDHeaderTime.Month, m_OptionSet.ZHDHeaderTime.Day);
                        }
                        else
                        {
                            strzhd = string.Format("Date:{0:d4}/{1:d2}/{2:d2}", DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day);
                        }
                        //                 strzhd = string.Format("Date:{0:d4}/{1:d2}/{2:d2}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

                        tempbytes = Encoding.ASCII.GetBytes(strzhd);
                        Array.Copy(tempbytes, 0, staticbuff, 49, tempbytes.Length);

                        staticbuff[70] = (byte)DateTime.Now.Day;
                        staticbuff[71] = (byte)DateTime.Now.Month;
                        staticbuff[72] = (byte)(DateTime.Now.Year & 0x00ff);
                        staticbuff[73] = (byte)((DateTime.Now.Year & 0xff00) >> 8);

                        int Sampleval = (int)(1 * 1000);
                        staticbuff[110] = (byte)(Sampleval & 0x00ff);
                        staticbuff[111] = (byte)((Sampleval & 0xff00) >> 8);

                        //160
                        if (Dataprocess.StationList[data.sindex].ReceiverOBJ.ReceiverType == "CSI")
                        {
                            strzhd = "[unknown]";
                        }
                        else if (Dataprocess.StationList[data.sindex].ReceiverOBJ.ReceiverType == "RTCM3")
                        {
                            strzhd = "[unknown]";//yby 加

                        }
                        else if (Dataprocess.StationList[data.sindex].ReceiverOBJ.ReceiverType == "NOVATEL")
                        {
                            strzhd = "[unknown]";
                        }
                        tempbytes = Encoding.ASCII.GetBytes(strzhd);
                        Array.Copy(tempbytes, 0, staticbuff, 160, tempbytes.Length);

                        for (i = 0; i < 5; i++)
                            staticbuff[168 + i] = (byte)0xff;

                        ///receiver ID
                        staticbuff[179] = (byte)'I';
                        staticbuff[180] = (byte)'D';
                        staticbuff[181] = (byte)':';

                        tempbytes = Encoding.ASCII.GetBytes("0960001");
                        Array.Copy(tempbytes, 0, staticbuff, 182, 7);

                        strzhd = string.Format("{0:d2}-{1:d2}-{2:d2},{3:d2}:{4:d2}\r\n", wYear, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                        tempbytes = Encoding.ASCII.GetBytes(strzhd);
                        Array.Copy(tempbytes, 0, staticbuff, 189, tempbytes.Length);

                        for (i = 0; i < 467; i++)
                        {
                            staticbuff[205 + i] = (byte)0xff;
                        }
                        bwdaily.Write(staticbuff);
                        #endregion
                    }
                    else
                    {
                        fsdaily = new FileStream(file, FileMode.Append);
                        bwdaily = new BinaryWriter(fsdaily);
                    }
                    bwdaily.Write(data.BinaryData);
                    bwdaily.Flush();
                    bwdaily.Close();
                    fsdaily.Close();
                }
                else //1 位置信息
                {
                    string file_posInfo = path + Dataprocess.StationList[data.sindex].ID + ".pos";
                    FileStream fsdaily2;
                    BinaryWriter bwdaily2;
                    if (!File.Exists(file_posInfo))
                    {
                        //输入文件头
                        //暂定
                        fsdaily2 = new FileStream(file_posInfo, FileMode.Create);
                        bwdaily2 = new BinaryWriter(fsdaily2);
                    }
                    else
                    {
                        fsdaily2 = new FileStream(file_posInfo, FileMode.Append);
                        bwdaily2 = new BinaryWriter(fsdaily2);
                    }
                    bwdaily2.Write(data.BinaryData);
                    bwdaily2.Flush();
                    bwdaily2.Close();
                    fsdaily2.Close();
                }
            }

            //添加数据进公共内存供计算线程提取
            lock (sysroot)
            {
                DataForTrans.Add(data.Clone());
                PRocessTransEvent.Set();
            }
            #endregion
        }

        public void ProcessingThread(object parm)
        {
            while (true)
            {

                if (PRocessTransEvent.WaitOne(1000, false))
                {
                    List<DataPackage> revdatas = new List<DataPackage>();
                    lock (sysroot)
                    {
                        for (int i = 0; i < DataForTrans.Count; i++)
                        {
                            revdatas.Add(DataForTrans[i].Clone());
                        }
                        DataForTrans.Clear();
                    }

                    for (int i = 0; i < revdatas.Count; i++)
                    {
                        //try
                        //{
                            if (revdatas[i].DataType == 0)
                            {
                                Dataprocess.PutinBinaryDatas(revdatas[i].sindex, revdatas[i].BinaryData);
                            }
                            else
                            {
                                Dataprocess.PutinPosInfoDatas(revdatas[i].sindex, revdatas[i].BinaryData);
                            }
                        //}
                        //catch
                        //{
                        //}
                    }

                    PRocessTransEvent.Reset();
                }
            }
        }

        public void BeginProcessThread()
        {
            //sxy 20130327 如果选选择自主RTK模式，启动线程前要初始化基线RTK控制
            if (m_OptionSet.ProcessMode == 1 || m_OptionSet.AutoAdjustPos == 1)
            {
                IniRTKList();
            }

            //启动计算线程
            if (ProcessTread == null)
            {
                ProcessTread = new Thread(new ParameterizedThreadStart(ProcessingThread));
                ProcessTread.Priority = ThreadPriority.Highest;
                ProcessTread.Start();
            }
            else
            {
                ProcessTread.Abort();
                ProcessTread = new Thread(new ParameterizedThreadStart(ProcessingThread));
                ProcessTread.Priority = ThreadPriority.Highest;
                ProcessTread.Start();
            }

        }

        public void EndProcessThread()
        {
            //sxy 20130327 如果选选择自主RTK模式，结束线程前要释放RTK控制
            if (m_OptionSet.ProcessMode == 1 || m_OptionSet.AutoAdjustPos == 1)
            {
                EndRTKList();
            }
            //结束计算线程
            Dataprocess.EndProcess();
            if (ProcessTread != null )
            {
                ProcessTread.Abort();
            }
        }
        //
        public void InitStations(List<GPSStationInfo> listGPSStation, CoordinateOperation CoorOperate)
        {
            if (null == CoorOperate || null == listGPSStation)
            {
                return;
            }

            #region 添加头
            string strHead = "";
            if (m_OptionSet.sysMark[0] == 1 && m_OptionSet.sysMark[1] == 0 && m_OptionSet.sysMark[2] == 0)
            {
                strHead = "GPS$";
            }
            else if (m_OptionSet.sysMark[0] == 0 && m_OptionSet.sysMark[1] == 1 && m_OptionSet.sysMark[2] == 0)
            {
                strHead = "BDS$";
            }
            else
            {
                strHead = "GGB$";
            }
            #endregion

            int nCount = listGPSStation.Count;
            for (int i = 0; i < nCount; i++)
            {
                SpatialPosition stpos;
                SpatialPosition stN1pos;
                int CoodType = 0;
                if (listGPSStation[i].p_CoordType.ToUpper() == "BLH")
                {
                    stpos = new GeoPosition(listGPSStation[i].p_B_X_x, listGPSStation[i].p_L_Y_y,
                        listGPSStation[i].p_H_Z_h).ConvertToSpatialPosition();
                    stN1pos = new GeoPosition(listGPSStation[i].p_B_X_x_i, listGPSStation[i].p_L_Y_y_i,
                        listGPSStation[i].p_H_Z_h_i).ConvertToSpatialPosition();
                    CoodType = 0;
                }
                else if (listGPSStation[i].p_CoordType.ToUpper() == "XYZ")
                {
                    stpos = new SpatialPosition(listGPSStation[i].p_B_X_x, listGPSStation[i].p_L_Y_y,
                        listGPSStation[i].p_H_Z_h);
                    stN1pos = new SpatialPosition(listGPSStation[i].p_B_X_x_i, listGPSStation[i].p_L_Y_y_i,
                        listGPSStation[i].p_H_Z_h_i);
                    CoodType = 1;
                }
                else if (listGPSStation[i].p_CoordType.ToUpper() == "XYH")
                {
                    double B = 0, L = 0, H = 0;
                    ZHD.CoordLib.Coord.xyhtoBLH(CoorOperate.myDatumPar, CoorOperate.myTempPar,
                        listGPSStation[i].p_B_X_x, listGPSStation[i].p_L_Y_y,
                        listGPSStation[i].p_H_Z_h, ref B, ref L, ref H);
                    stpos = new GeoPosition(B, L, H).ConvertToSpatialPosition();

                    ZHD.CoordLib.Coord.xyhtoBLH(CoorOperate.myDatumPar, CoorOperate.myTempPar, 
                        listGPSStation[i].p_B_X_x_i, listGPSStation[i].p_L_Y_y_i,
                        listGPSStation[i].p_H_Z_h_i, ref B, ref L, ref H);
                    stN1pos = new GeoPosition(B, L, H).ConvertToSpatialPosition();
                    CoodType = 2;
                }
                else
                {
                    throw new Exception(GnssMonitor.Properties.Resources.strUnknownCoordinate);
                }
               
                bool isBase = listGPSStation[i].p_IsBaseSation.ToLower() == "true" ? true : false;
                
                Dataprocess.AddStation(isBase, strHead + listGPSStation[i].p_StName.Trim(), 
                    listGPSStation[i].p_StName2.Trim(),
                listGPSStation[i].p_St_IP.Trim() + "," + listGPSStation[i].p_St_PORT.ToString().Trim(),
                listGPSStation[i].p_St_PORT2.ToString().Trim(), CoodType, stpos, stN1pos,
                listGPSStation[i].p_AntDN, listGPSStation[i].p_AntDE, listGPSStation[i].p_AntHeight, 
                listGPSStation[i].p_ReceiverType.ToUpper(), listGPSStation[i].p_AntType,
                listGPSStation[i].p_RMS, listGPSStation[i].p_StComment);
            }

        }

        //加入站点后，自动给所有的监测点找最近的参考站点，并生成基线列表
        public void InitBaselines(List<string> listBaseLine)
        {
            if (null == listBaseLine)
            {
                return;
            }

            for (int i = 0; i < listBaseLine.Count; i++)
            {
                string[] inf = listBaseLine[i].Split(new string[]{"-"},StringSplitOptions.RemoveEmptyEntries);

                //基线定义
                if (inf.Length == 2)
                {
                    string strHead = "";
                    if (m_OptionSet.sysMark[0] == 1 && m_OptionSet.sysMark[1] == 0 && m_OptionSet.sysMark[2] == 0)
                    {
                        strHead = "GPS$";
                    }
                    else if (m_OptionSet.sysMark[0] == 0 && m_OptionSet.sysMark[1] == 1 && m_OptionSet.sysMark[2] == 0)
                    {
                        strHead = "BDS$";
                    }
                    else
                    {
                        strHead = "GGB$";
                    }

                    Dataprocess.AddBaseLine(strHead + inf[0].Trim() + "-" + strHead + inf[1].Trim(),
                    strHead + inf[0].Trim() + "-" + strHead + inf[1].Trim(),
                    strHead + inf[0].Trim(), strHead + inf[1].Trim());
                }
            }
        }

        public void IniMoniSummary()
        {
            for(int i=0;i<Dataprocess.StationList.Count;i++)
            {
                if(Dataprocess.StationList[i] is MonitorStation)
                {
                    MonitorStation monist = (MonitorStation)Dataprocess.StationList[i];
                    int connectLineNum = monist.LineList.Count;
                    monist.DeformSummary = new DeformInfoSummary(connectLineNum);
                }
            }
        }

        public void IniRTKList()
        {
            int num_bl = Dataprocess.BaselineList.Count;
            RTKProcess.InitialRTKList(num_bl);
            for (int i = 0; i < num_bl;i++ )
            {
                Dataprocess.BaselineList[i].RtkIndex = i;
                Baseline bl = Dataprocess.BaselineList[i];
                int nf = 0;

                nf = m_OptionSet.SloveMode;
                RTKProcess.SetOption(0, 1, 1, 2, 0, nf, m_OptionSet.AjustHD2003Ratio);
                //不设置仪器高，都以天线相位中心坐标为准
                //double[] roverneu = new double[3] { bl.BStation.Dneu.n, bl.BStation.Dneu.e, bl.BStation.Dneu.u };
                //RTKProcess.SetDeltaNEU(0, roverneu);
                //double[] baseneu = new double[3] { bl.AStation.Dneu.n, bl.AStation.Dneu.e, bl.AStation.Dneu.u };
                //RTKProcess.SetDeltaNEU(1, baseneu);

                RTKProcess.SetBaseStationPos(bl.AStation.StOriSpatialPos.x, bl.AStation.StOriSpatialPos.y, bl.AStation.StOriSpatialPos.z);
                RTKProcess.SetLeapSecond(MoniConstant.LeapSecond);

                RTKProcess.InitialRTK();
                RTKProcess.SetRtkList(i);
            }
        }

        public void EndRTKList()
        {
            int num_bl = Dataprocess.BaselineList.Count;
            RTKProcess.FreeRtkList(num_bl);
        }
    }
}
