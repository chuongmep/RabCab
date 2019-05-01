using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Autodesk.AutoCAD.Colors;
using Color = Autodesk.AutoCAD.Colors.Color;
using ColorDialog = Autodesk.AutoCAD.Windows.ColorDialog;

namespace RabCab.Gui.DimSystem
{
    internal class DimSystemSettings : Form
    {
        public bool ClickedOk;

        private bool _allFieldsCorrect = true;

        private IContainer components;

        private GroupBox _groupBox1;

        private Label _label1;

        private GroupBox _groupBox2;

        private Label _label2;

        public TextBox TboxTolerance;

        public Button BtnDynColor;

        private Button _btnOk;

        private Button _btnCancel;

        private GroupBox _groupBox3;

        private Label _label6;

        private Label _label5;

        private Label _label4;

        private Label _label3;

        public CheckBox ChbOriginalDimRemoveOverride;

        public ComboBox CmbOriginalTextPosition;

        public CheckBox ChbNewDimRemoveOverride;

        public ComboBox CmbNewTextPosition;

        private Label _label7;

        public DimSystemSettings()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            ClickedOk = false;
            Close();
        }

        private void btnDynColor_Click(object sender, EventArgs e)
        {
            if (!_allFieldsCorrect) return;
            var colorDialog = new ColorDialog
            {
                IncludeByBlockByLayer = false
            };

            var r = BtnDynColor.BackColor.R;
            var g = BtnDynColor.BackColor.G;
            var backColor = BtnDynColor.BackColor;
            var color = Color.FromRgb(r, g, backColor.B);
            colorDialog.Color = Color.FromColorIndex(ColorMethod.ByAci, color.ColorIndex);
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                var color1 = colorDialog.Color;
                var button = BtnDynColor;
                var num = color1.ColorValue.R;
                var g1 = color1.ColorValue.G;
                var colorValue = color1.ColorValue;
                button.BackColor = System.Drawing.Color.FromArgb(num, g1, colorValue.B);
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (_allFieldsCorrect)
            {
                ClickedOk = true;
                Close();
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void Form1_Settings_Load(object sender, EventArgs e)
        {
            ClickedOk = false;
            _allFieldsCorrect = true;
            _btnOk.Focus();
        }

        private void InitializeComponent()
        {
            _groupBox1 = new GroupBox();
            _label1 = new Label();
            TboxTolerance = new TextBox();
            _groupBox2 = new GroupBox();
            _label2 = new Label();
            BtnDynColor = new Button();
            _btnOk = new Button();
            _btnCancel = new Button();
            _groupBox3 = new GroupBox();
            CmbNewTextPosition = new ComboBox();
            CmbOriginalTextPosition = new ComboBox();
            _label6 = new Label();
            _label5 = new Label();
            ChbNewDimRemoveOverride = new CheckBox();
            ChbOriginalDimRemoveOverride = new CheckBox();
            _label4 = new Label();
            _label3 = new Label();
            _label7 = new Label();
            _groupBox1.SuspendLayout();
            _groupBox2.SuspendLayout();
            _groupBox3.SuspendLayout();
            SuspendLayout();
            _groupBox1.Controls.Add(_label1);
            _groupBox1.Controls.Add(TboxTolerance);
            _groupBox1.Location = new Point(155, 179);
            _groupBox1.Name = "_groupBox1";
            _groupBox1.Size = new Size(278, 63);
            _groupBox1.TabIndex = 3;
            _groupBox1.TabStop = false;
            _groupBox1.Text = "Tolerance";
            _label1.AutoSize = true;
            _label1.Location = new Point(119, 28);
            _label1.Name = "_label1";
            _label1.Size = new Size(137, 26);
            _label1.TabIndex = 1;
            _label1.Text = "Tolerance for searching for \r\ncontinuous dimension string";
            TboxTolerance.Location = new Point(16, 28);
            TboxTolerance.Name = "TboxTolerance";
            TboxTolerance.Size = new Size(97, 20);
            TboxTolerance.TabIndex = 0;
            TboxTolerance.KeyDown += tboxTolerance_KeyDown;
            TboxTolerance.Leave += tboxTolerance_Leave;
            _groupBox2.Controls.Add(_label2);
            _groupBox2.Controls.Add(BtnDynColor);
            _groupBox2.Location = new Point(12, 179);
            _groupBox2.Name = "_groupBox2";
            _groupBox2.Size = new Size(137, 63);
            _groupBox2.TabIndex = 2;
            _groupBox2.TabStop = false;
            _groupBox2.Text = "Dynamic preview";
            _label2.AutoSize = true;
            _label2.Location = new Point(9, 28);
            _label2.Name = "_label2";
            _label2.Size = new Size(34, 13);
            _label2.TabIndex = 1;
            _label2.Text = "Color:";
            BtnDynColor.BackColor = System.Drawing.Color.Red;
            BtnDynColor.FlatStyle = FlatStyle.Flat;
            BtnDynColor.Location = new Point(49, 28);
            BtnDynColor.Name = "BtnDynColor";
            BtnDynColor.Size = new Size(15, 15);
            BtnDynColor.TabIndex = 0;
            BtnDynColor.Text = " ";
            BtnDynColor.UseVisualStyleBackColor = false;
            BtnDynColor.Click += btnDynColor_Click;
            _btnOk.Location = new Point(285, 261);
            _btnOk.Name = "_btnOk";
            _btnOk.Size = new Size(75, 23);
            _btnOk.TabIndex = 0;
            _btnOk.Text = "OK";
            _btnOk.UseVisualStyleBackColor = true;
            _btnOk.Click += btnOk_Click;
            _btnCancel.Location = new Point(366, 261);
            _btnCancel.Name = "_btnCancel";
            _btnCancel.Size = new Size(75, 23);
            _btnCancel.TabIndex = 1;
            _btnCancel.Text = "Cancel";
            _btnCancel.UseVisualStyleBackColor = true;
            _btnCancel.Click += btnCancel_Click;
            _groupBox3.Controls.Add(CmbNewTextPosition);
            _groupBox3.Controls.Add(CmbOriginalTextPosition);
            _groupBox3.Controls.Add(_label6);
            _groupBox3.Controls.Add(_label5);
            _groupBox3.Controls.Add(ChbNewDimRemoveOverride);
            _groupBox3.Controls.Add(ChbOriginalDimRemoveOverride);
            _groupBox3.Controls.Add(_label4);
            _groupBox3.Controls.Add(_label3);
            _groupBox3.Location = new Point(12, 12);
            _groupBox3.Name = "_groupBox3";
            _groupBox3.Size = new Size(421, 145);
            _groupBox3.TabIndex = 4;
            _groupBox3.TabStop = false;
            _groupBox3.Text = "DIMSETINSERT and DIMSETDELETE command";
            CmbNewTextPosition.DropDownStyle = ComboBoxStyle.DropDownList;
            CmbNewTextPosition.FormattingEnabled = true;
            CmbNewTextPosition.Items.AddRange(new object[] {"Do not change", "Home"});
            CmbNewTextPosition.Location = new Point(237, 100);
            CmbNewTextPosition.Name = "CmbNewTextPosition";
            CmbNewTextPosition.Size = new Size(121, 21);
            CmbNewTextPosition.TabIndex = 7;
            CmbOriginalTextPosition.DropDownStyle = ComboBoxStyle.DropDownList;
            CmbOriginalTextPosition.FormattingEnabled = true;
            CmbOriginalTextPosition.Items.AddRange(new object[] {"Do not change", "Home"});
            CmbOriginalTextPosition.Location = new Point(8, 100);
            CmbOriginalTextPosition.Name = "CmbOriginalTextPosition";
            CmbOriginalTextPosition.Size = new Size(121, 21);
            CmbOriginalTextPosition.TabIndex = 4;
            CmbOriginalTextPosition.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            _label6.AutoSize = true;
            _label6.Location = new Point(234, 84);
            _label6.Name = "_label6";
            _label6.Size = new Size(70, 13);
            _label6.TabIndex = 6;
            _label6.Text = "Text position:";
            _label5.AutoSize = true;
            _label5.Location = new Point(10, 84);
            _label5.Name = "_label5";
            _label5.Size = new Size(70, 13);
            _label5.TabIndex = 5;
            _label5.Text = "Text position:";
            ChbNewDimRemoveOverride.AutoSize = true;
            ChbNewDimRemoveOverride.Location = new Point(237, 55);
            ChbNewDimRemoveOverride.Name = "ChbNewDimRemoveOverride";
            ChbNewDimRemoveOverride.Size = new Size(127, 17);
            ChbNewDimRemoveOverride.TabIndex = 3;
            ChbNewDimRemoveOverride.Text = "Remove text override";
            ChbNewDimRemoveOverride.UseVisualStyleBackColor = true;
            ChbOriginalDimRemoveOverride.AutoSize = true;
            ChbOriginalDimRemoveOverride.Location = new Point(13, 55);
            ChbOriginalDimRemoveOverride.Name = "ChbOriginalDimRemoveOverride";
            ChbOriginalDimRemoveOverride.Size = new Size(127, 17);
            ChbOriginalDimRemoveOverride.TabIndex = 2;
            ChbOriginalDimRemoveOverride.Text = "Remove text override";
            ChbOriginalDimRemoveOverride.UseVisualStyleBackColor = true;
            _label4.AutoSize = true;
            _label4.Location = new Point(234, 29);
            _label4.Name = "_label4";
            _label4.Size = new Size(82, 13);
            _label4.TabIndex = 1;
            _label4.Text = "New dimension:";
            _label3.AutoSize = true;
            _label3.Location = new Point(10, 29);
            _label3.Name = "_label3";
            _label3.Size = new Size(96, 13);
            _label3.TabIndex = 0;
            _label3.Text = "Existing dimension:";
            _label7.AutoSize = true;
            _label7.Font = new Font("Microsoft Sans Serif", 6.75f, FontStyle.Regular, GraphicsUnit.Point, 0);
            _label7.Location = new Point(392, 287);
            _label7.Name = "_label7";
            _label7.Size = new Size(49, 12);
            _label7.TabIndex = 5;
            _label7.Text = "Stima2014";
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(453, 298);
            Controls.Add(_label7);
            Controls.Add(_groupBox3);
            Controls.Add(_btnCancel);
            Controls.Add(_btnOk);
            Controls.Add(_groupBox2);
            Controls.Add(_groupBox1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "Form1_Settings";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "DimSet Settings";
            Load += Form1_Settings_Load;
            _groupBox1.ResumeLayout(false);
            _groupBox1.PerformLayout();
            _groupBox2.ResumeLayout(false);
            _groupBox2.PerformLayout();
            _groupBox3.ResumeLayout(false);
            _groupBox3.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        private void tboxTolerance_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return) tboxTolerance_Leave(sender, e);
        }

        private void tboxTolerance_Leave(object sender, EventArgs e)
        {
            try
            {
                if (Convert.ToDouble(TboxTolerance.Text) >= 0)
                {
                    _allFieldsCorrect = true;
                }
                else
                {
                    TboxTolerance.Select(0, TboxTolerance.Text.Length);
                    TboxTolerance.Focus();
                    _allFieldsCorrect = false;
                }
            }
            catch
            {
                TboxTolerance.Select(0, TboxTolerance.Text.Length);
                TboxTolerance.Focus();
                _allFieldsCorrect = false;
            }
        }
    }
}