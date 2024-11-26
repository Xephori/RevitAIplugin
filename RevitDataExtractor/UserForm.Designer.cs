namespace WallDataPlugin
{
    partial class UserForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox txtWebLink;
        private System.Windows.Forms.CheckBox chkLaunchLocalhost;
        private System.Windows.Forms.Button btnLaunchWebUI;
        private System.Windows.Forms.Label lblWebLink;

        private void InitializeComponent()
        {
            this.txtWebLink = new System.Windows.Forms.TextBox();
            this.chkLaunchLocalhost = new System.Windows.Forms.CheckBox();
            this.btnLaunchWebUI = new System.Windows.Forms.Button();
            this.lblWebLink = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtWebLink
            // 
            this.txtWebLink.Location = new System.Drawing.Point(15, 25);
            this.txtWebLink.Name = "txtWebLink";
            this.txtWebLink.Size = new System.Drawing.Size(260, 29);
            this.txtWebLink.TabIndex = 0;
            this.txtWebLink.Text = "https://revitaiplugin.streamlit.app/";
            // 
            // chkLaunchLocalhost
            // 
            this.chkLaunchLocalhost.AutoSize = true;
            this.chkLaunchLocalhost.Location = new System.Drawing.Point(15, 60);
            this.chkLaunchLocalhost.Name = "chkLaunchLocalhost";
            this.chkLaunchLocalhost.Size = new System.Drawing.Size(255, 29);
            this.chkLaunchLocalhost.TabIndex = 1;
            this.chkLaunchLocalhost.Text = "Launch Localhost Server";
            this.chkLaunchLocalhost.UseVisualStyleBackColor = true;
            // 
            // btnLaunchWebUI
            // 
            this.btnLaunchWebUI.Location = new System.Drawing.Point(15, 90);
            this.btnLaunchWebUI.Name = "btnLaunchWebUI";
            this.btnLaunchWebUI.Size = new System.Drawing.Size(260, 23);
            this.btnLaunchWebUI.TabIndex = 2;
            this.btnLaunchWebUI.Text = "Launch Web UI";
            this.btnLaunchWebUI.UseVisualStyleBackColor = true;
            this.btnLaunchWebUI.Click += new System.EventHandler(this.btnLaunchWebUI_Click);
            // 
            // lblWebLink
            // 
            this.lblWebLink.AutoSize = true;
            this.lblWebLink.Location = new System.Drawing.Point(12, 9);
            this.lblWebLink.Name = "lblWebLink";
            this.lblWebLink.Size = new System.Drawing.Size(101, 25);
            this.lblWebLink.TabIndex = 3;
            this.lblWebLink.Text = "Web Link:";
            // 
            // UserForm
            // 
            this.ClientSize = new System.Drawing.Size(542, 345);
            this.Controls.Add(this.lblWebLink);
            this.Controls.Add(this.btnLaunchWebUI);
            this.Controls.Add(this.chkLaunchLocalhost);
            this.Controls.Add(this.txtWebLink);
            this.Name = "UserForm";
            this.Text = "Wall Data Plugin";
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
