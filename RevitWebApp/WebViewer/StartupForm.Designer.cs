namespace RevitWebApp
    partial class StartupForm
    {
        private System.ComponentModel.IContainer components = null;
        private CheckBox useLocalhostCheckbox;
        private Button openButton;

        private void InitializeComponent()
        {
            this.useLocalhostCheckbox = new System.Windows.Forms.CheckBox();
            this.openButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // useLocalhostCheckbox
            // 
            this.useLocalhostCheckbox.AutoSize = true;
            this.useLocalhostCheckbox.Location = new System.Drawing.Point(12, 12);
            this.useLocalhostCheckbox.Name = "useLocalhostCheckbox";
            this.useLocalhostCheckbox.Size = new System.Drawing.Size(123, 19);
            this.useLocalhostCheckbox.TabIndex = 0;
            this.useLocalhostCheckbox.Text = "Use localhost server";
            this.useLocalhostCheckbox.UseVisualStyleBackColor = true;
            // 
            // openButton
            // 
            this.openButton.Location = new System.Drawing.Point(12, 37);
            this.openButton.Name = "openButton";
            this.openButton.Size = new System.Drawing.Size(75, 23);
            this.openButton.TabIndex = 1;
            this.openButton.Text = "Open Web UI";
            this.openButton.UseVisualStyleBackColor = true;
            this.openButton.Click += new System.EventHandler(this.openButton_Click);
            // 
            // StartupForm
            // 
            this.ClientSize = new System.Drawing.Size(200, 75);
            this.Controls.Add(this.openButton);
            this.Controls.Add(this.useLocalhostCheckbox);
            this.Name = "StartupForm";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }