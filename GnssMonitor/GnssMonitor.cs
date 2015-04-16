using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ZHDCommon;
using ZHD.SYS.CommonUtility.DatabaseLib;
using System.Threading;

namespace GnssMonitor
{
    public partial class GnssMonitor : Form
    {
        private DBConfigInfo m_DBConfig;              //数据库连接信息
        private SetFileRW m_Config;                             //读写文件类对象
        private LocalCommunication m_LocalService;              //本地通信及管理类
        private System.Threading.Thread m_LocalServiceThread;   //实时监控线程
        private bool m_isStart = false;                         //服务是否启动成功
        private bool m_isButtonOK = false;                      //是否按下‘确定’按钮
        private bool m_isPrompt = false;                        //是否已弹出过提示对话框
        bool m_isGPS = false;
        bool m_isBDS = false;
        bool m_isGlonass = false;

        public GnssMonitor()
        {
            InitializeComponent();
            m_DBConfig = new DBConfigInfo();
            m_Config = new SetFileRW();
            m_LocalService = new LocalCommunication();
            m_LocalServiceThread = new Thread(new ThreadStart(LocalServiceThread));
            m_LocalServiceThread.Start();

            m_Config.GetConfigMsg(m_DBConfig, out m_isGPS, out m_isBDS, out m_isGlonass);

            if (!m_isGPS && !m_isBDS && !m_isGlonass)
            {
                MessageBox.Show(Properties.Resources.strProcessModeError, Properties.Resources.strPrompt,
                MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            }
            else
            {
                if (m_isGPS && !m_isBDS && !m_isGlonass)
                {
                    this.Text = Properties.Resources.strGPSProcessMode;
                    this.notifyIcon1.Text = Properties.Resources.strGPSProcessMode;
                }
                else if (!m_isGPS && m_isBDS && !m_isGlonass)
                {
                    this.Text = Properties.Resources.strBDSProcessMode;
                    this.notifyIcon1.Text = Properties.Resources.strBDSProcessMode;
                }
                else
                {
                    this.Text = Properties.Resources.strGlonassProcessMode;
                    this.notifyIcon1.Text = Properties.Resources.strGlonassProcessMode;
                }
                m_LocalService.SetProcessMode(m_isGPS, m_isBDS, m_isGlonass);
            }

            if (null == m_DBConfig.DbServer || null == m_DBConfig.DbName
                || null == m_DBConfig.DbUser || null == m_DBConfig.DbPassword)
            {
                MessageBox.Show(Properties.Resources.strConfigPromote, Properties.Resources.strPrompt,
                MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                return;
            }

            this.comboBoxDatabaseType.Text = this.comboBoxDatabaseType.Items[(int)m_DBConfig.DbStyle].ToString();
            this.textBoxIP.Text = m_DBConfig.DbServer;
            this.textBoxDatabase.Text = m_DBConfig.DbName;
            this.textBoxUser.Text = m_DBConfig.DbUser;
            this.textBoxPsw.Text = m_DBConfig.DbPassword;
            this.ckbGPS.Checked = m_isGPS;
            this.ckbBD.Checked = m_isBDS;
            this.ckbGLONASS.Checked = m_isGlonass;
        }

        /// <summary>
        /// 判断传入信息是否齐全，是则启动服务
        /// </summary>
        private void LocalServiceThread()
        {
            while (true)
            {
                if (null == m_DBConfig.DbServer || null == m_DBConfig.DbName
                    || null == m_DBConfig.DbUser || null == m_DBConfig.DbPassword
                    || "" == m_DBConfig.DbServer || "" == m_DBConfig.DbName
                    || "" == m_DBConfig.DbUser || "" == m_DBConfig.DbPassword
                    || (!m_isGPS && !m_isBDS && !m_isGlonass))
                {
                    Thread.Sleep(1000);
                    continue;
                }

                if (m_isStart && m_isButtonOK)
                {
                    m_isButtonOK = false;
                    m_LocalService.StopService();
                    m_isStart = m_LocalService.StartService(m_DBConfig, m_Config);
                    if (m_isStart)
                    {
                        MessageBox.Show(Properties.Resources.strGnssAnalysisStartSuccess,
                            Properties.Resources.strPrompt,
                        MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                    }
                    else
                    {
                        MessageBox.Show(Properties.Resources.strGnssAnalysisStartFailure,
                            Properties.Resources.strPrompt,
                        MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                    }
                }
                else if (!m_isStart)
                {
                    m_isStart = m_LocalService.StartService(m_DBConfig, m_Config);

                    if (m_isStart)
                    {
                        MessageBox.Show(Properties.Resources.strGnssAnalysisStartSuccess,
                            Properties.Resources.strPrompt,
                        MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                    }
                    else if (!m_isPrompt || m_isButtonOK)
                    {
                        m_isPrompt = true;
                        MessageBox.Show(Properties.Resources.strGnssAnalysisStartFailure,
                            Properties.Resources.strPrompt,
                        MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                    }

                    m_isButtonOK = false;
                }
                Thread.Sleep(1000);
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            m_isButtonOK = true;
            m_DBConfig.DbStyle = (DBStyle)this.comboBoxDatabaseType.SelectedIndex;
            m_DBConfig.DbServer = this.textBoxIP.Text;
            m_DBConfig.DbName = this.textBoxDatabase.Text;
            m_DBConfig.DbUser = this.textBoxUser.Text;
            m_DBConfig.DbPassword = this.textBoxPsw.Text;
            m_isGPS = this.ckbGPS.Checked;
            m_isBDS = this.ckbBD.Checked;
            m_isGlonass = this.ckbGLONASS.Checked;

            if ("" == m_DBConfig.DbServer || "" == m_DBConfig.DbName
                || "" == m_DBConfig.DbUser || "" == m_DBConfig.DbPassword)
            {
                MessageBox.Show(Properties.Resources.strInputIncomplete,
                    Properties.Resources.strPrompt,
                MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                return;
            }

            if (!m_isGPS && !m_isBDS && !m_isGlonass)
            {
                MessageBox.Show(Properties.Resources.strProcessModeError, Properties.Resources.strPrompt,
                MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                return;
            }
            else if (m_isGPS && !m_isBDS && !m_isGlonass)
            {
                this.Text = Properties.Resources.strGPSProcessMode;
                this.notifyIcon1.Text = Properties.Resources.strGPSProcessMode;
            }
            else if (!m_isGPS && m_isBDS && !m_isGlonass)
            {
                this.Text = Properties.Resources.strBDSProcessMode;
                this.notifyIcon1.Text = Properties.Resources.strBDSProcessMode;
            }
            else
            {
                this.Text = Properties.Resources.strGlonassProcessMode;
                this.notifyIcon1.Text = Properties.Resources.strGlonassProcessMode;
            }

            m_LocalService.SetProcessMode(m_isGPS, m_isBDS, m_isGlonass);
            m_Config.SetConfigMsg(m_DBConfig, this.ckbGPS.Checked, this.ckbBD.Checked, this.ckbGLONASS.Checked);
        }

        private void SHOW_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.Activate();
        }

        private void HIDE_Click(object sender, EventArgs e)
        {
            if (this.WindowState != FormWindowState.Minimized)
            {
                this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
                this.Hide();
                this.notifyIcon1.Visible = true;
            }
        }

        private void EXIT_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(Properties.Resources.strExitPromote, Properties.Resources.strPrompt,
                MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != System.Windows.Forms.DialogResult.Cancel)
            {
                if (this.WindowState != FormWindowState.Minimized)
                {
                    this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
                    this.Hide();
                    this.notifyIcon1.Visible = true;
                }
                m_LocalServiceThread.Abort();
                m_LocalServiceThread.Join();
                m_LocalService.StopService();
                this.Close();
            }
        }

        private void GnssMonitor_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.WindowState != FormWindowState.Minimized)
            {
                e.Cancel = true;
                this.WindowState = System.Windows.Forms.FormWindowState.Minimized;

                this.Hide();
                this.notifyIcon1.Visible = true;
                notifyIcon1.ShowBalloonTip(3000);
            }
        }
    }
}
