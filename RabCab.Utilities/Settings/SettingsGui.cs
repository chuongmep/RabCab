using System.Drawing;
using System.Windows.Forms;
using Autodesk.AutoCAD.ApplicationServices;

namespace RabCab.Settings
{
    public class SettingsGui : Panel
    {
        public SettingsGui()
        {
            //FlowDirection = FlowDirection.TopDown;
            Dock = DockStyle.Fill;
            this.BackColor = SystemColors.Window;
            this.ForeColor = SystemColors.WindowText;
            AutoScroll = true;

            InitializeComponent();
        }

        private void InitializeComponent()
        {

            CreateCommonSettings(this);

            CreateAnalysisSettings(this);
            
            CreateAnnotationSettings(this);

            CreateAssemblySettings(this);

            CreateCarpentrySettings(this);

            CreateCncSettings(this);

            CreateCombineSettings(this);

            CreateContentSettings(this);

            CreateMeshSettings(this);

            CreateReferenceSettings(this);
            
            CreateStructuralSettings(this);
            
            CreateTidySettings(this);

        }

        #region Option Panels

        public void CreateCommonSettings(Control parent)
        {
            
            var bodyPanel = new BodyPanel(parent);
            parent.Controls.Add(bodyPanel);
            
            var tPanel = new TitlePanel();
            bodyPanel.Controls.Add(tPanel);

            
        }

        public void CreateAnalysisSettings(Control parent)
        {

        }

        public void CreateAnnotationSettings(Control parent)
        {

        }

        public void CreateAssemblySettings(Control parent)
        {

        }

        public void CreateCarpentrySettings(Control parent)
        {

        }

        public void CreateCncSettings(Control parent)
        {

        }

        public void CreateCombineSettings(Control parent)
        {

        }

        public void CreateContentSettings(Control parent)
        {

        }

        public void CreateMeshSettings(Control parent)
        {

        }

        public void CreateReferenceSettings(Control parent)
        {

        }

        public void CreateStructuralSettings(Control parent)
        {

        }
        public void CreateTidySettings(Control parent)
        {

        }


        #endregion
    }

    public sealed class TitlePanel : Panel
    {
        private TableLayoutPanel tLayout;
        private PictureBox pBox;
        private Label lbl;
        private ComboBox templates;
        private Button resDef;

        public TitlePanel()
        {
            this.Height = 40;
            this.BackColor = SystemColors.AppWorkspace;
            this.ForeColor = SystemColors.WindowText;
            this.Dock = DockStyle.Top;

            tLayout = new TableLayoutPanel {ColumnCount = 4, RowCount = 1, Dock = DockStyle.Fill, BackColor = this.BackColor, ForeColor =  this.ForeColor};
            this.Controls.Add(tLayout);

            tLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, this.Height));
            tLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            tLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            tLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, this.Height));

            tLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, this.Height));

            pBox = new PictureBox()
            {
                BackColor = this.BackColor,
                ForeColor =  this.ForeColor,
                Dock = DockStyle.Fill
            };

            lbl = new Label()
            {
                BackColor = this.BackColor,
                ForeColor = this.ForeColor,
                Dock = DockStyle.Fill,
                Text = "Placeholder"
            };

            templates = new ComboBox()
            {
                BackColor = this.BackColor,
                ForeColor = this.ForeColor,
                Dock = DockStyle.Fill
            };

            resDef = new Button()
            {
                BackColor = this.BackColor,
                ForeColor = this.ForeColor,
                Dock = DockStyle.Fill,
                Text = "NAN"
            };

            tLayout.Controls.Add(pBox, 0, 0);
            tLayout.Controls.Add(lbl, 1, 0);
            tLayout.Controls.Add(templates, 2, 0);
            tLayout.Controls.Add(resDef, 3, 0);

        }

    }


    public sealed class BodyPanel : Panel
    {

        public BodyPanel(Control parent)
        {
            this.BackColor = SystemColors.Window;
            this.ForeColor = SystemColors.WindowText;
            this.Dock = DockStyle.Top;
        }

    }
}