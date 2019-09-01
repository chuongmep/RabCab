using System.Drawing;
using System.Windows.Forms;

namespace RabCab.Settings
{
    public class SettingsGui : Panel
    {
        public SettingsComponent SetComp;

        public SettingsGui()
        {
            //FlowDirection = FlowDirection.TopDown;
            Dock = DockStyle.Fill;
            BackColor = SystemColors.Window;
            ForeColor = SystemColors.WindowText;
            AutoScroll = true;

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            SetComp = new SettingsComponent {Dock = DockStyle.Fill};
            Controls.Add(SetComp);
        }
    }
}