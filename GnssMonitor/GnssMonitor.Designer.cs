namespace GnssMonitor
{
    partial class GnssMonitor
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GnssMonitor));
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.comboBoxDatabaseType = new System.Windows.Forms.ComboBox();
            this.label14 = new System.Windows.Forms.Label();
            this.textBoxPsw = new System.Windows.Forms.TextBox();
            this.textBoxUser = new System.Windows.Forms.TextBox();
            this.textBoxIP = new System.Windows.Forms.TextBox();
            this.textBoxDatabase = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.MenuItem = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.SHOW = new System.Windows.Forms.ToolStripMenuItem();
            this.HIDE = new System.Windows.Forms.ToolStripMenuItem();
            this.EXIT = new System.Windows.Forms.ToolStripMenuItem();
            this.ckbBD = new System.Windows.Forms.CheckBox();
            this.ckbGLONASS = new System.Windows.Forms.CheckBox();
            this.ckbGPS = new System.Windows.Forms.CheckBox();
            this.MenuItem.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(164, 245);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 75;
            this.buttonCancel.Text = "取消";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(42, 245);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 74;
            this.buttonOK.Text = "确定";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // comboBoxDatabaseType
            // 
            this.comboBoxDatabaseType.FormattingEnabled = true;
            this.comboBoxDatabaseType.Items.AddRange(new object[] {
            "SQLSERVER",
            "MYSQL",
            "ACCESS",
            "ORACLE"});
            this.comboBoxDatabaseType.Location = new System.Drawing.Point(101, 26);
            this.comboBoxDatabaseType.Name = "comboBoxDatabaseType";
            this.comboBoxDatabaseType.Size = new System.Drawing.Size(180, 20);
            this.comboBoxDatabaseType.TabIndex = 68;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(4, 29);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(77, 12);
            this.label14.TabIndex = 73;
            this.label14.Text = "数据库类型：";
            // 
            // textBoxPsw
            // 
            this.textBoxPsw.Location = new System.Drawing.Point(101, 169);
            this.textBoxPsw.Name = "textBoxPsw";
            this.textBoxPsw.Size = new System.Drawing.Size(180, 21);
            this.textBoxPsw.TabIndex = 72;
            this.textBoxPsw.UseSystemPasswordChar = true;
            // 
            // textBoxUser
            // 
            this.textBoxUser.Location = new System.Drawing.Point(101, 134);
            this.textBoxUser.Name = "textBoxUser";
            this.textBoxUser.Size = new System.Drawing.Size(180, 21);
            this.textBoxUser.TabIndex = 71;
            // 
            // textBoxIP
            // 
            this.textBoxIP.Location = new System.Drawing.Point(101, 62);
            this.textBoxIP.Name = "textBoxIP";
            this.textBoxIP.Size = new System.Drawing.Size(180, 21);
            this.textBoxIP.TabIndex = 69;
            this.textBoxIP.Text = "127.0.0.1";
            // 
            // textBoxDatabase
            // 
            this.textBoxDatabase.Location = new System.Drawing.Point(101, 97);
            this.textBoxDatabase.Name = "textBoxDatabase";
            this.textBoxDatabase.Size = new System.Drawing.Size(180, 21);
            this.textBoxDatabase.TabIndex = 70;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(40, 178);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 12);
            this.label3.TabIndex = 64;
            this.label3.Text = "密码：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(28, 143);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 67;
            this.label2.Text = "用户名：";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(16, 71);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 12);
            this.label4.TabIndex = 66;
            this.label4.Text = "数据库IP：";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 106);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 65;
            this.label1.Text = "数据库名：";
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.notifyIcon1.BalloonTipText = "程序已隐藏运行";
            this.notifyIcon1.ContextMenuStrip = this.MenuItem;
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "解算软件";
            this.notifyIcon1.Visible = true;
            // 
            // MenuItem
            // 
            this.MenuItem.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SHOW,
            this.HIDE,
            this.EXIT});
            this.MenuItem.Name = "MenuItem";
            this.MenuItem.Size = new System.Drawing.Size(101, 70);
            // 
            // SHOW
            // 
            this.SHOW.Name = "SHOW";
            this.SHOW.Size = new System.Drawing.Size(100, 22);
            this.SHOW.Text = "显示";
            this.SHOW.Click += new System.EventHandler(this.SHOW_Click);
            // 
            // HIDE
            // 
            this.HIDE.Name = "HIDE";
            this.HIDE.Size = new System.Drawing.Size(100, 22);
            this.HIDE.Text = "隐藏";
            this.HIDE.Click += new System.EventHandler(this.HIDE_Click);
            // 
            // EXIT
            // 
            this.EXIT.Name = "EXIT";
            this.EXIT.Size = new System.Drawing.Size(100, 22);
            this.EXIT.Text = "退出";
            this.EXIT.Click += new System.EventHandler(this.EXIT_Click);
            // 
            // ckbBD
            // 
            this.ckbBD.AutoSize = true;
            this.ckbBD.Location = new System.Drawing.Point(119, 211);
            this.ckbBD.Name = "ckbBD";
            this.ckbBD.Size = new System.Drawing.Size(36, 16);
            this.ckbBD.TabIndex = 78;
            this.ckbBD.Text = "BD";
            this.ckbBD.UseVisualStyleBackColor = true;
            // 
            // ckbGLONASS
            // 
            this.ckbGLONASS.AutoSize = true;
            this.ckbGLONASS.Location = new System.Drawing.Point(162, 211);
            this.ckbGLONASS.Name = "ckbGLONASS";
            this.ckbGLONASS.Size = new System.Drawing.Size(66, 16);
            this.ckbGLONASS.TabIndex = 77;
            this.ckbGLONASS.Text = "GLONASS";
            this.ckbGLONASS.UseVisualStyleBackColor = true;
            // 
            // ckbGPS
            // 
            this.ckbGPS.AutoSize = true;
            this.ckbGPS.Location = new System.Drawing.Point(61, 211);
            this.ckbGPS.Name = "ckbGPS";
            this.ckbGPS.Size = new System.Drawing.Size(42, 16);
            this.ckbGPS.TabIndex = 76;
            this.ckbGPS.Text = "GPS";
            this.ckbGPS.UseVisualStyleBackColor = true;
            // 
            // GnssMonitor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(300, 280);
            this.Controls.Add(this.ckbBD);
            this.Controls.Add(this.ckbGLONASS);
            this.Controls.Add(this.ckbGPS);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.comboBoxDatabaseType);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.textBoxPsw);
            this.Controls.Add(this.textBoxUser);
            this.Controls.Add(this.textBoxIP);
            this.Controls.Add(this.textBoxDatabase);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "GnssMonitor";
            this.Text = "GNSS解算软件";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GnssMonitor_FormClosing);
            this.MenuItem.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.ComboBox comboBoxDatabaseType;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox textBoxPsw;
        private System.Windows.Forms.TextBox textBoxUser;
        private System.Windows.Forms.TextBox textBoxIP;
        private System.Windows.Forms.TextBox textBoxDatabase;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ContextMenuStrip MenuItem;
        private System.Windows.Forms.ToolStripMenuItem SHOW;
        private System.Windows.Forms.ToolStripMenuItem HIDE;
        private System.Windows.Forms.ToolStripMenuItem EXIT;
        public System.Windows.Forms.CheckBox ckbBD;
        public System.Windows.Forms.CheckBox ckbGLONASS;
        public System.Windows.Forms.CheckBox ckbGPS;
    }
}

