using System.Drawing;
using System.Windows.Forms;

namespace RabCab.Settings
{
    public class SettingsGui : Panel
    {
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
            var setComp = new SettingsComponent {Dock = DockStyle.Fill};
            this.Controls.Add(setComp);

        }
    }
}