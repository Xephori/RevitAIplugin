using System;
using System.Windows.Forms;

namespace RevitWebApp
    public partial class StartupForm : Form
    {
        public bool UseLocalhost { get; private set; }

        public StartupForm()
        {
            InitializeComponent();
        }

        private void openButton_Click(object sender, EventArgs e)
        {
            UseLocalhost = useLocalhostCheckbox.Checked;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }