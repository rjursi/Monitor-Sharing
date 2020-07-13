﻿namespace Server
{
    partial class Server
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Server));
            this.btnStart = new System.Windows.Forms.Button();
            this.btnShutdown = new System.Windows.Forms.Button();
            this.btnControl = new System.Windows.Forms.Button();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(14, 16);
            this.btnStart.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(172, 42);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "공유";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnShutdown
            // 
            this.btnShutdown.Location = new System.Drawing.Point(14, 67);
            this.btnShutdown.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.btnShutdown.Name = "btnShutdown";
            this.btnShutdown.Size = new System.Drawing.Size(172, 42);
            this.btnShutdown.TabIndex = 1;
            this.btnShutdown.Text = "종료";
            this.btnShutdown.UseVisualStyleBackColor = true;
            this.btnShutdown.Click += new System.EventHandler(this.btnShutdown_Click);
            // 
            // btnControl
            // 
            this.btnControl.Location = new System.Drawing.Point(194, 16);
            this.btnControl.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnControl.Name = "btnControl";
            this.btnControl.Size = new System.Drawing.Size(111, 94);
            this.btnControl.TabIndex = 2;
            this.btnControl.Text = "제어";
            this.btnControl.UseVisualStyleBackColor = true;
            this.btnControl.Click += new System.EventHandler(this.btnControl_Click);
            // 
            // notifyIcon
            // 
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Text = "MOSH";
            this.notifyIcon.Visible = true;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(132, 131);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(146, 52);
            this.button2.TabIndex = 4;
            this.button2.Text = "전송";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // Server
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(320, 202);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.btnControl);
            this.Controls.Add(this.btnShutdown);
            this.Controls.Add(this.btnStart);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.Name = "Server";
            this.Text = "Server";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Server_FormClosing);
            this.Load += new System.EventHandler(this.Server_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnShutdown;
        private System.Windows.Forms.Button btnControl;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.Button button2;
    }
}

