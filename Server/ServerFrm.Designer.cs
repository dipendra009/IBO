namespace Server
{
    partial class ServerFrm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (true && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(true);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.stopServerBtn = new System.Windows.Forms.Button();
            this.sendBtn = new System.Windows.Forms.Button();
            this.txtHostIP = new System.Windows.Forms.TextBox();
            this.txtHostname = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.strtServerBtn = new System.Windows.Forms.Button();
            this.txtPortNo = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.strtCaptureBtn = new System.Windows.Forms.Button();
            this.stpCaptureBtn = new System.Windows.Forms.Button();
            this.continueBtn = new System.Windows.Forms.Button();
            this.camSettingBtn = new System.Windows.Forms.Button();
            this.resolutionSettingBtn = new System.Windows.Forms.Button();
            this.camSettingGrpBx = new System.Windows.Forms.GroupBox();
            this.controlBtnGrpBx = new System.Windows.Forms.GroupBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.camSettingGrpBx.SuspendLayout();
            this.controlBtnGrpBx.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.stopServerBtn);
            this.groupBox1.Controls.Add(this.sendBtn);
            this.groupBox1.Controls.Add(this.txtHostIP);
            this.groupBox1.Controls.Add(this.txtHostname);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.strtServerBtn);
            this.groupBox1.Controls.Add(this.txtPortNo);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 40);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(200, 181);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Settings";
            // 
            // stopServerBtn
            // 
            this.stopServerBtn.Location = new System.Drawing.Point(19, 148);
            this.stopServerBtn.Name = "stopServerBtn";
            this.stopServerBtn.Size = new System.Drawing.Size(162, 24);
            this.stopServerBtn.TabIndex = 8;
            this.stopServerBtn.Text = "Stop Server";
            this.stopServerBtn.Click += new System.EventHandler(this.stopServerBtn_Click);
            // 
            // sendBtn
            // 
            this.sendBtn.Location = new System.Drawing.Point(19, 118);
            this.sendBtn.Name = "sendBtn";
            this.sendBtn.Size = new System.Drawing.Size(162, 24);
            this.sendBtn.TabIndex = 7;
            this.sendBtn.Text = "Send";
            this.sendBtn.Click += new System.EventHandler(this.sendBtn_Click);
            // 
            // txtHostIP
            // 
            this.txtHostIP.Enabled = false;
            this.txtHostIP.Location = new System.Drawing.Point(77, 36);
            this.txtHostIP.Name = "txtHostIP";
            this.txtHostIP.Size = new System.Drawing.Size(112, 20);
            this.txtHostIP.TabIndex = 6;
            // 
            // txtHostname
            // 
            this.txtHostname.Enabled = false;
            this.txtHostname.Location = new System.Drawing.Point(77, 13);
            this.txtHostname.Name = "txtHostname";
            this.txtHostname.Size = new System.Drawing.Size(112, 20);
            this.txtHostname.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(16, 39);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(72, 16);
            this.label3.TabIndex = 4;
            this.label3.Text = "Host IP";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(16, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 16);
            this.label2.TabIndex = 3;
            this.label2.Text = "Host Name";
            // 
            // strtServerBtn
            // 
            this.strtServerBtn.Location = new System.Drawing.Point(19, 88);
            this.strtServerBtn.Name = "strtServerBtn";
            this.strtServerBtn.Size = new System.Drawing.Size(162, 24);
            this.strtServerBtn.TabIndex = 2;
            this.strtServerBtn.Text = "Start Server";
            this.strtServerBtn.Click += new System.EventHandler(this.strtServerBtn_Click);
            // 
            // txtPortNo
            // 
            this.txtPortNo.Enabled = false;
            this.txtPortNo.Location = new System.Drawing.Point(77, 62);
            this.txtPortNo.Name = "txtPortNo";
            this.txtPortNo.Size = new System.Drawing.Size(112, 20);
            this.txtPortNo.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(16, 65);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Port No:";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.pictureBox1.Location = new System.Drawing.Point(218, 40);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(280, 242);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 8;
            this.pictureBox1.TabStop = false;
            // 
            // strtCaptureBtn
            // 
            this.strtCaptureBtn.Location = new System.Drawing.Point(19, 19);
            this.strtCaptureBtn.Name = "strtCaptureBtn";
            this.strtCaptureBtn.Size = new System.Drawing.Size(109, 23);
            this.strtCaptureBtn.TabIndex = 0;
            this.strtCaptureBtn.Text = "Start";
            this.strtCaptureBtn.UseVisualStyleBackColor = true;
            this.strtCaptureBtn.Click += new System.EventHandler(this.strtCaptureBtn_Click);
            // 
            // stpCaptureBtn
            // 
            this.stpCaptureBtn.Location = new System.Drawing.Point(147, 19);
            this.stpCaptureBtn.Name = "stpCaptureBtn";
            this.stpCaptureBtn.Size = new System.Drawing.Size(109, 23);
            this.stpCaptureBtn.TabIndex = 1;
            this.stpCaptureBtn.Text = "Stop";
            this.stpCaptureBtn.UseVisualStyleBackColor = true;
            this.stpCaptureBtn.Click += new System.EventHandler(this.stpCaptureBtn_Click);
            // 
            // continueBtn
            // 
            this.continueBtn.Location = new System.Drawing.Point(279, 19);
            this.continueBtn.Name = "continueBtn";
            this.continueBtn.Size = new System.Drawing.Size(108, 23);
            this.continueBtn.TabIndex = 9;
            this.continueBtn.Text = "Continue";
            this.continueBtn.UseVisualStyleBackColor = true;
            this.continueBtn.Click += new System.EventHandler(this.continueBtn_Click);
            // 
            // camSettingBtn
            // 
            this.camSettingBtn.Location = new System.Drawing.Point(19, 19);
            this.camSettingBtn.Name = "camSettingBtn";
            this.camSettingBtn.Size = new System.Drawing.Size(162, 23);
            this.camSettingBtn.TabIndex = 10;
            this.camSettingBtn.Text = "Advance Camera Setting";
            this.camSettingBtn.UseVisualStyleBackColor = true;
            this.camSettingBtn.Click += new System.EventHandler(this.camSettingBtn_Click);
            // 
            // resolutionSettingBtn
            // 
            this.resolutionSettingBtn.Location = new System.Drawing.Point(19, 48);
            this.resolutionSettingBtn.Name = "resolutionSettingBtn";
            this.resolutionSettingBtn.Size = new System.Drawing.Size(162, 23);
            this.resolutionSettingBtn.TabIndex = 11;
            this.resolutionSettingBtn.Text = "Resolution Setting";
            this.resolutionSettingBtn.UseVisualStyleBackColor = true;
            this.resolutionSettingBtn.Click += new System.EventHandler(this.resolutionSettingBtn_Click);
            // 
            // camSettingGrpBx
            // 
            this.camSettingGrpBx.Controls.Add(this.camSettingBtn);
            this.camSettingGrpBx.Controls.Add(this.resolutionSettingBtn);
            this.camSettingGrpBx.Location = new System.Drawing.Point(12, 257);
            this.camSettingGrpBx.Name = "camSettingGrpBx";
            this.camSettingGrpBx.Size = new System.Drawing.Size(200, 84);
            this.camSettingGrpBx.TabIndex = 12;
            this.camSettingGrpBx.TabStop = false;
            this.camSettingGrpBx.Text = "Camera Settings";
            // 
            // controlBtnGrpBx
            // 
            this.controlBtnGrpBx.Controls.Add(this.strtCaptureBtn);
            this.controlBtnGrpBx.Controls.Add(this.stpCaptureBtn);
            this.controlBtnGrpBx.Controls.Add(this.continueBtn);
            this.controlBtnGrpBx.Location = new System.Drawing.Point(151, 395);
            this.controlBtnGrpBx.Name = "controlBtnGrpBx";
            this.controlBtnGrpBx.Size = new System.Drawing.Size(390, 68);
            this.controlBtnGrpBx.TabIndex = 13;
            this.controlBtnGrpBx.TabStop = false;
            this.controlBtnGrpBx.Text = "Control Button";
            // 
            // pictureBox2
            // 
            this.pictureBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox2.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.pictureBox2.Location = new System.Drawing.Point(564, 40);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(237, 242);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox2.TabIndex = 14;
            this.pictureBox2.TabStop = false;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(31, 414);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 15;
            this.button1.Text = "Check";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // ServerFrm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(891, 371);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.controlBtnGrpBx);
            this.Controls.Add(this.camSettingGrpBx);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.groupBox1);
            this.Name = "ServerFrm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ServerFrm";
            this.Load += new System.EventHandler(this.ServerFrm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.camSettingGrpBx.ResumeLayout(false);
            this.controlBtnGrpBx.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button strtServerBtn;
        private System.Windows.Forms.TextBox txtPortNo;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtHostIP;
        private System.Windows.Forms.TextBox txtHostname;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button strtCaptureBtn;
        private System.Windows.Forms.Button stpCaptureBtn;
        private System.Windows.Forms.Button continueBtn;
        private System.Windows.Forms.Button camSettingBtn;
        private System.Windows.Forms.Button resolutionSettingBtn;
        private System.Windows.Forms.GroupBox camSettingGrpBx;
        private System.Windows.Forms.GroupBox controlBtnGrpBx;
        private System.Windows.Forms.Button sendBtn;
        private System.Windows.Forms.Button stopServerBtn;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Button button1;
    }
}