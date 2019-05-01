using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RabCab.Gui.DimSystem
{
    class DimSystemProperties : Form
    {
        private IContainer components;

        private GroupBox groupBoxArrowheads;

        private GroupBox groupBox1;

        private Button btnOK;

        private Button btnCancel;

        public CheckBox chBoxArrowheadsOption;

        public CheckBox chBoxExtLineOption;

        public CheckBox chBoxSuppressExtLine;

        private ComboBox cmbArrowhead;

        private DimSystemArrow myDialogCustomArrow = new DimSystemArrow();

        public bool clickedOK;

        public string ArrowheadBlkName = "";

        public DimSystemProperties()
        {
            this.InitializeComponent();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.clickedOK = false;
            base.Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (!this.chBoxArrowheadsOption.Checked && !this.chBoxExtLineOption.Checked)
            {
                MessageBox.Show("One option must by selected to continue. Click 'Cancel' to cancel command");
                return;
            }
            this.clickedOK = true;
            this.ArrowheadBlkName = this.GetArrowheadBlkName(this.cmbArrowhead.SelectedIndex);
            base.Close();
        }

        private void cmbArrowhead_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.cmbArrowhead.SelectedIndex == 20)
            {
                Application.ShowModalDialog(null, this.myDialogCustomArrow, false);
                this.ArrowheadBlkName = this.myDialogCustomArrow.customBlockName;
                if (!this.myDialogCustomArrow.clickedOK)
                {
                    this.cmbArrowhead.SelectedIndex = 0;
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.components != null)
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void Form2_PointProperties_Load(object sender, EventArgs e)
        {
            this.clickedOK = false;
            this.cmbArrowhead.SelectedIndex = 0;
        }

        private string GetArrowheadBlkName(int _index)
        {
            string arrowheadBlkName = "";
            switch (_index)
            {
                case 0:
                    {
                        arrowheadBlkName = ".";
                        break;
                    }
                case 1:
                    {
                        arrowheadBlkName = "_ClosedBlank";
                        break;
                    }
                case 2:
                    {
                        arrowheadBlkName = "_Closed";
                        break;
                    }
                case 3:
                    {
                        arrowheadBlkName = "_Dot";
                        break;
                    }
                case 4:
                    {
                        arrowheadBlkName = "_ArchTick";
                        break;
                    }
                case 5:
                    {
                        arrowheadBlkName = "_Oblique";
                        break;
                    }
                case 6:
                    {
                        arrowheadBlkName = "_Open";
                        break;
                    }
                case 7:
                    {
                        arrowheadBlkName = "_Origin";
                        break;
                    }
                case 8:
                    {
                        arrowheadBlkName = "_Origin2";
                        break;
                    }
                case 9:
                    {
                        arrowheadBlkName = "_Open90";
                        break;
                    }
                case 10:
                    {
                        arrowheadBlkName = "_Open30";
                        break;
                    }
                case 11:
                    {
                        arrowheadBlkName = "_DotSmall";
                        break;
                    }
                case 12:
                    {
                        arrowheadBlkName = "_DotBlank";
                        break;
                    }
                case 13:
                    {
                        arrowheadBlkName = "_Small";
                        break;
                    }
                case 14:
                    {
                        arrowheadBlkName = "_BoxBlank";
                        break;
                    }
                case 15:
                    {
                        arrowheadBlkName = "_BoxFilled";
                        break;
                    }
                case 16:
                    {
                        arrowheadBlkName = "_DatumBlank";
                        break;
                    }
                case 17:
                    {
                        arrowheadBlkName = "_DatumFilled";
                        break;
                    }
                case 18:
                    {
                        arrowheadBlkName = "_Integral";
                        break;
                    }
                case 19:
                    {
                        arrowheadBlkName = "_None";
                        break;
                    }
                case 20:
                    {
                        arrowheadBlkName = this.ArrowheadBlkName;
                        break;
                    }
            }
            return arrowheadBlkName;
        }

        private void InitializeComponent()
        {
            this.chBoxArrowheadsOption = new CheckBox();
            this.groupBoxArrowheads = new GroupBox();
            this.cmbArrowhead = new ComboBox();
            this.chBoxExtLineOption = new CheckBox();
            this.groupBox1 = new GroupBox();
            this.chBoxSuppressExtLine = new CheckBox();
            this.btnOK = new Button();
            this.btnCancel = new Button();
            this.groupBoxArrowheads.SuspendLayout();
            this.groupBox1.SuspendLayout();
            base.SuspendLayout();
            this.chBoxArrowheadsOption.AutoSize = true;
            this.chBoxArrowheadsOption.Location = new Point(7, 0);
            this.chBoxArrowheadsOption.Name = "chBoxArrowheadsOption";
            this.chBoxArrowheadsOption.Size = new Size(82, 17);
            this.chBoxArrowheadsOption.TabIndex = 0;
            this.chBoxArrowheadsOption.Text = "Arrowheads";
            this.chBoxArrowheadsOption.UseVisualStyleBackColor = true;
            this.groupBoxArrowheads.Controls.Add(this.cmbArrowhead);
            this.groupBoxArrowheads.Controls.Add(this.chBoxArrowheadsOption);
            this.groupBoxArrowheads.Location = new Point(12, 12);
            this.groupBoxArrowheads.Name = "groupBoxArrowheads";
            this.groupBoxArrowheads.Size = new Size(181, 54);
            this.groupBoxArrowheads.TabIndex = 1;
            this.groupBoxArrowheads.TabStop = false;
            this.cmbArrowhead.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbArrowhead.FormattingEnabled = true;
            ComboBox.ObjectCollection items = this.cmbArrowhead.Items;
            object[] objArray = new object[] { "Closed filled", "Closed blank", "Closed", "Dot", "Architectural tick", "Oblique", "Open", "Origin indicator", "Origin indicator 2", "Right angle", "Open 30", "Dot small", "Dot blank", "Dot small blank", "Box", "Box filled", "Datum triangle", "Datum triangle filled", "Integral", "None", "User arrow" };
            items.AddRange(objArray);
            this.cmbArrowhead.Location = new Point(46, 19);
            this.cmbArrowhead.Name = "cmbArrowhead";
            this.cmbArrowhead.Size = new Size(121, 21);
            this.cmbArrowhead.TabIndex = 1;
            this.cmbArrowhead.SelectedIndexChanged += new EventHandler(this.cmbArrowhead_SelectedIndexChanged);
            this.chBoxExtLineOption.AutoSize = true;
            this.chBoxExtLineOption.Location = new Point(6, 0);
            this.chBoxExtLineOption.Name = "chBoxExtLineOption";
            this.chBoxExtLineOption.Size = new Size(95, 17);
            this.chBoxExtLineOption.TabIndex = 2;
            this.chBoxExtLineOption.Text = "Extension Line";
            this.chBoxExtLineOption.UseVisualStyleBackColor = true;
            this.groupBox1.Controls.Add(this.chBoxSuppressExtLine);
            this.groupBox1.Controls.Add(this.chBoxExtLineOption);
            this.groupBox1.Location = new Point(12, 81);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new Size(181, 55);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.chBoxSuppressExtLine.AutoSize = true;
            this.chBoxSuppressExtLine.Location = new Point(46, 23);
            this.chBoxSuppressExtLine.Name = "chBoxSuppressExtLine";
            this.chBoxSuppressExtLine.RightToLeft = RightToLeft.Yes;
            this.chBoxSuppressExtLine.Size = new Size(107, 17);
            this.chBoxSuppressExtLine.TabIndex = 3;
            this.chBoxSuppressExtLine.Text = "Suppress Ext line";
            this.chBoxSuppressExtLine.UseVisualStyleBackColor = true;
            this.btnOK.Location = new Point(39, 174);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new Size(75, 23);
            this.btnOK.TabIndex = 4;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new EventHandler(this.btnOK_Click);
            this.btnCancel.Location = new Point(131, 174);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(75, 23);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new EventHandler(this.btnCancel_Click);
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(218, 210);
            base.Controls.Add(this.btnCancel);
            base.Controls.Add(this.btnOK);
            base.Controls.Add(this.groupBox1);
            base.Controls.Add(this.groupBoxArrowheads);
            base.FormBorderStyle = FormBorderStyle.FixedSingle;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "Form2_PointProperties";
            base.ShowInTaskbar = false;
            base.StartPosition = FormStartPosition.CenterParent;
            this.Text = "DimSet Properties at Point";
            base.Load += new EventHandler(this.Form2_PointProperties_Load);
            this.groupBoxArrowheads.ResumeLayout(false);
            this.groupBoxArrowheads.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            base.ResumeLayout(false);
        }
    }
}
