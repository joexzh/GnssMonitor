using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZHD.CoordLib;
using ZHD.CoordSetCE;
using System.IO;
using System.Windows.Forms;
using GPSData;

namespace ZHDCommon
{
    public class CoordinateOperation
    {
        //
        public ZHDDatumPar myDatumPar = new ZHDDatumPar();
        public ZHDTempPar myTempPar = new ZHDTempPar();
        public string DatumName = "unamed";//坐标系统的名称
        //1.读取椭球数组
        public void LoadEllipsoidnDatum(string szEllipseFile)
        {
            myTempPar.pEllipser = DatumReader.ReadEllipses(szEllipseFile);
        }

        //2.读取转换参数
        public void LoadDatum()
        {
            string AppPath = Application.ExecutablePath;
            AppPath = AppPath.Substring(0, AppPath.LastIndexOf(@"\") + 1);
            if (File.Exists(AppPath + "/" + DatumName + ".dam"))
            {
                bool succeed = false;
                myDatumPar = DatumReader.ReadDatumPar(AppPath + "/" + DatumName + ".dam", ref succeed, AppPath);
                if (succeed == false)
                {
                    myDatumPar = new ZHDDatumPar();//不成功,则上面已经为null
                }
                //
                MointorProcessing.ProcessConfig.myDatumPar = myDatumPar;
                MointorProcessing.ProcessConfig.myTempPar = myTempPar;
            }
        }   

        //赋值给processconfig
        public void Apply2ProcessConfig(OptionSetting OptionSet)
        {
            //给processconfig赋值

            MointorProcessing.ProcessConfig.cutAngleMin = Math.PI * OptionSet.elevationmask / 180.0;
            MointorProcessing.ProcessConfig.SloveMode = OptionSet.SloveMode;
            MointorProcessing.ProcessConfig.PosChangeLimit = OptionSet.PosChangeLimit;
            MointorProcessing.ProcessConfig.WeightModel = OptionSet.WeightModel;
            MointorProcessing.ProcessConfig.MaxIteration = OptionSet.MaxIteration;
            MointorProcessing.ProcessConfig.FilterMode = OptionSet.FilterMode;
            MointorProcessing.ProcessConfig.MoniFilerWindow = OptionSet.MoniFilerWindow;

            MointorProcessing.ProcessConfig.AutoAdjustMoniPos = OptionSet.AutoAdjustPos;
            MointorProcessing.ProcessConfig.ProcessMode = OptionSet.ProcessMode;
            MointorProcessing.ProcessConfig.StaticGap = OptionSet.StaticGap;
            MointorProcessing.ProcessConfig.StaticSolveInterval = OptionSet.StaticSolveInterval;
            //
            MointorProcessing.ProcessConfig.RunMode = OptionSet.nRunMode;
            MointorProcessing.ProcessConfig.ZHDHeaderTime = OptionSet.ZHDHeaderTime;

            MointorProcessing.ProcessConfig.WriteDBInterval = OptionSet.WriteDBInterval;

            MointorProcessing.ProcessConfig.IsWritetoDb = OptionSet.IsWritetoDb;
            MointorProcessing.ProcessConfig.isOutPutLogFile = OptionSet.isSaveLogFile;
            MointorProcessing.ProcessConfig.AjustDeformLimit = OptionSet.AjustDeformLimit;
            MointorProcessing.ProcessConfig.AjustHD2003Ratio = OptionSet.AjustHD2003Ratio;
            MointorProcessing.ProcessConfig.AjustHD2003RMS = OptionSet.AjustHD2003RMS;

            MointorProcessing.ProcessConfig.DataInternal = OptionSet.DataInternal;
            MointorProcessing.ProcessConfig.MoniProcessFilterMode = OptionSet.MoniProcessFilterMode;
            MointorProcessing.ProcessConfig.RmsLimit = OptionSet.RmsLimit;
            MointorProcessing.ProcessConfig.obssigma = OptionSet.obssigma;
            MointorProcessing.ProcessConfig.GapBetweenTwoStaticSlove = OptionSet.GapBetweenTwoStaticSlove;

            MointorProcessing.ProcessConfig.UsedRTKinforMethod = OptionSet.UsedRTKinforMethod;

            MointorProcessing.ProcessConfig.AmSearchNum = OptionSet.AmSearchNum;
            MointorProcessing.ProcessConfig.InitialMinutes = OptionSet.InitialMinutes;

            //先在这里测试运行，设置StaticGap_Max,后面改为界面
            MointorProcessing.ProcessConfig.StaticGap_Max = MointorProcessing.ProcessConfig.StaticGap + 0.1;
            if (OptionSet.StaticGap < 0.333)
            {
                MointorProcessing.ProcessConfig.StaticGap_Max = 0.5;
            }
            //tya 130808
            for (int i = 0; i < 3; i++)
            {
                MointorProcessing.ProcessConfig.sysMark[i] = OptionSet.sysMark[i];
            }

        }

        public List<NavData> ReadNavfile()
        {
            string AppPath = Application.ExecutablePath;
            AppPath = AppPath.Substring(0, AppPath.LastIndexOf(@"\") + 1);
            string file = Path.Combine(AppPath, "GPSNav.txt");
            List<NavData> list = new List<NavData>();
            if (File.Exists(file))
            {
                Code.NFileHeader nfileHeader = new Code.NFileHeader();
                Code.RinexDecoder.Load_NFile(file, ref list, ref nfileHeader);
            }
            return list;
        }

        public void SaveNavFile(List<NavData> list)
        {
            string AppPath = Application.ExecutablePath;
            AppPath = AppPath.Substring(0, AppPath.LastIndexOf(@"\") + 1);
            string file = Path.Combine(AppPath, "GPSNav.txt");
            Code.RinexBuilder.SaveRinexNfile(file, list.ToArray(), new Code.NFileHeader(), new Code.RinexHeader());
        }

        public List<CMPNav> ReadCMPNavfile()
        {
            string AppPath = Application.ExecutablePath;
            AppPath = AppPath.Substring(0, AppPath.LastIndexOf(@"\") + 1);
            string file = Path.Combine(AppPath, "CMPNav.txt");
            List<CMPNav> list = new List<CMPNav>();
            if (File.Exists(file))
            {
                Code.NFileHeader nfileHeader = new Code.NFileHeader();
                Code.RinexDecoder.Load_CMPNFile(file, ref list, ref nfileHeader);
            }
            return list;
        }

        public void SaveCMPNavFile(List<CMPNav> list)
        {
            string AppPath = Application.ExecutablePath;
            AppPath = AppPath.Substring(0, AppPath.LastIndexOf(@"\") + 1);
            string file = Path.Combine(AppPath, "CMPNav.txt");
            Code.RinexBuilder.SaveRinexCfile(file, list.ToArray(), new Code.NFileHeader(), new Code.RinexHeader());
        }

        public List<GLONav> ReadGLoNavFile()
        {
            string AppPath = Application.ExecutablePath;
            AppPath = AppPath.Substring(0, AppPath.LastIndexOf(@"\") + 1);
            string file = Path.Combine(AppPath, "GloNav.txt");
            List<GLONav> list = new List<GLONav>();
            if (File.Exists(file))
            {
                double leapSe = 0;
                Code.RinexDecoder.Load_GFile(file, ref list, ref leapSe);
            }
            return list;
        }

        public void SaveGLONavFile(List<GLONav> list)
        {
            string AppPath = Application.ExecutablePath;
            AppPath = AppPath.Substring(0, AppPath.LastIndexOf(@"\") + 1);
            string file = Path.Combine(AppPath, "GloNav.txt");
            Code.RinexBuilder.SaveRinexGfile(file, list.ToArray(), new Code.NFileHeader(), new Code.RinexHeader());
        }
    }
}
