using System;
using System.IO;

namespace WallDataPlugin
{
    partial class UserForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox txtWebLink;
        private System.Windows.Forms.CheckBox chkLaunchLocalhost;
        private System.Windows.Forms.Button btnLaunchWebUI;
        private System.Windows.Forms.Label lblWebLink;
        //private System.Windows.Forms.PictureBox pictureBoxGif;
       

        public void InitializeComponent()
        {
            this.txtWebLink = new System.Windows.Forms.TextBox();
            this.chkLaunchLocalhost = new System.Windows.Forms.CheckBox();
            this.btnLaunchWebUI = new System.Windows.Forms.Button();
            this.lblWebLink = new System.Windows.Forms.Label();
            //this.pictureBoxGif = new System.Windows.Forms.PictureBox();
            //((System.ComponentModel.ISupportInitialize)(this.pictureBoxGif)).BeginInit();
            this.SuspendLayout();
            // 
            // txtWebLink
            // 
            this.txtWebLink.Location = new System.Drawing.Point(15, 80);
            this.txtWebLink.Name = "txtWebLink";
            this.txtWebLink.Size = new System.Drawing.Size(700, 100);
            this.txtWebLink.TabIndex = 0;
            this.txtWebLink.Text = "https://revitaiplugin.streamlit.app/";
            this.txtWebLink.Font = new System.Drawing.Font("Papyrus", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            // 
            // chkLaunchLocalhost
            // 
            this.chkLaunchLocalhost.AutoSize = true;
            this.chkLaunchLocalhost.Location = new System.Drawing.Point(15, 150);
            this.chkLaunchLocalhost.Name = "chkLaunchLocalhost";
            this.chkLaunchLocalhost.Size = new System.Drawing.Size(400, 50);
            this.chkLaunchLocalhost.TabIndex = 1;
            this.chkLaunchLocalhost.Text = "Use Localhost Server";
            this.chkLaunchLocalhost.UseVisualStyleBackColor = true;
            this.chkLaunchLocalhost.Font = new System.Drawing.Font("Comic Sans MS", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            // 
            // btnLaunchWebUI
            // 
            this.btnLaunchWebUI.Location = new System.Drawing.Point(15, 220);
            this.btnLaunchWebUI.Name = "btnLaunchWebUI";
            this.btnLaunchWebUI.Size = new System.Drawing.Size(400, 75);
            this.btnLaunchWebUI.TabIndex = 2;
            this.btnLaunchWebUI.Text = "Launch Web UI";
            this.btnLaunchWebUI.UseVisualStyleBackColor = true;
            this.btnLaunchWebUI.Click += new System.EventHandler(this.btnLaunchWebUI_Click);
            this.btnLaunchWebUI.Font = new System.Drawing.Font("Papyrus", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            // 
            // lblWebLink
            // 
            this.lblWebLink.AutoSize = true;
            this.lblWebLink.Location = new System.Drawing.Point(15, 10);
            this.lblWebLink.Name = "lblWebLink";
            this.lblWebLink.Size = new System.Drawing.Size(101, 50);
            this.lblWebLink.TabIndex = 3;
            this.lblWebLink.Text = "Hosted Web Link:";
            this.lblWebLink.Font = new System.Drawing.Font("Comic Sans MS", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            //// 
            //// pictureBoxGif
            //// 
            //this.pictureBoxGif.Location = new System.Drawing.Point(500, 250);
            //this.pictureBoxGif.Name = "pictureBoxGif";
            //this.pictureBoxGif.Size = new System.Drawing.Size(300, 150);
            //this.pictureBoxGif.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            //this.pictureBoxGif.TabIndex = 4;
            //this.pictureBoxGif.TabStop = false;
            //string gifPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "bongo-cat.gif");
            //this.pictureBoxGif.Image = System.Drawing.Image.FromFile(gifPath);
            //  
            // UserForm
            // 
            this.ClientSize = new System.Drawing.Size(800, 400);
            this.Controls.Add(this.lblWebLink);
            this.Controls.Add(this.btnLaunchWebUI);
            this.Controls.Add(this.chkLaunchLocalhost);
            this.Controls.Add(this.txtWebLink);
            this.Name = "UserForm";
            this.Text = "Web UI Startup";
            //this.Controls.Add(this.pictureBoxGif);
            //((System.ComponentModel.ISupportInitialize)(this.pictureBoxGif)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
