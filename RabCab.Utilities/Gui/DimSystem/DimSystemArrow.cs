using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace RabCab.Gui.DimSystem
{
    class DimSystemArrow : Form
    {
        public bool clickedOK;

        public string customBlockName = "";

        private IContainer components;

        private Button btnOK;

        private Button btnCancel;

        private Label label1;

        private ComboBox cmbBlocksList;

        public DimSystemArrow()
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
            if (this.cmbBlocksList.SelectedValue == null)
            {
                return;
            }
            this.clickedOK = true;
            this.customBlockName = this.cmbBlocksList.SelectedValue.ToString();
            base.Close();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.components != null)
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void Form3_CustomArrow_Load(object sender, EventArgs e)
        {
            this.clickedOK = false;
            this.cmbBlocksList.DataSource = this.GetAllBlocks();
        }

        private List<string> GetAllBlocks()
        {
            Document mdiActiveDocument = Application.DocumentManager.MdiActiveDocument;
            Database database = mdiActiveDocument.Database;
            Editor editor = mdiActiveDocument.Editor;
            List<string> strs = new List<string>()
            {
                "_Closed",
                "_Dot",
                "_ArchTick",
                "_Oblique",
                "_Open",
                "_Origin",
                "_Origin2",
                "_Open90",
                "_Open30",
                "_DotSmall",
                "_DotBlank",
                "_Small",
                "_BoxBlank",
                "_BoxFilled",
                "_DatumBlank",
                "_DatumFilled",
                "_Integral",
                "_None"
            };
            List<string> strs1 = new List<string>();
            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                foreach (ObjectId obj in database.BlockTableId.GetObject(OpenMode.ForRead) as BlockTable)
                {
                    BlockTableRecord blockTableRecords = (BlockTableRecord)obj.GetObject(OpenMode.ForRead);
                    if (blockTableRecords.IsLayout || blockTableRecords.IsAnonymous || blockTableRecords.IsDependent || blockTableRecords.IsFromExternalReference || strs.Contains<string>(blockTableRecords.Name, StringComparer.OrdinalIgnoreCase) || blockTableRecords.HasAttributeDefinitions)
                    {
                        continue;
                    }
                    strs1.Add(blockTableRecords.Name);
                }
            }
            strs1.Sort();
            return strs1;
        }

        private void InitializeComponent()
        {
            this.btnOK = new Button();
            this.btnCancel = new Button();
            this.label1 = new Label();
            this.cmbBlocksList = new ComboBox();
            base.SuspendLayout();
            this.btnOK.Location = new Point(176, 102);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new Size(75, 23);
            this.btnOK.TabIndex = 0;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new EventHandler(this.btnOK_Click);
            this.btnCancel.Location = new Point(269, 102);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(75, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new EventHandler(this.btnCancel_Click);
            this.label1.AutoSize = true;
            this.label1.Location = new Point(19, 37);
            this.label1.Name = "label1";
            this.label1.Size = new Size(140, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Select from Drawing Blocks:";
            this.cmbBlocksList.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbBlocksList.FormattingEnabled = true;
            this.cmbBlocksList.Location = new Point(22, 66);
            this.cmbBlocksList.Name = "cmbBlocksList";
            this.cmbBlocksList.Size = new Size(322, 21);
            this.cmbBlocksList.TabIndex = 3;
            this.cmbBlocksList.SelectedIndexChanged += new EventHandler(this.comboBox1_SelectedIndexChanged);
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(368, 146);
            base.Controls.Add(this.cmbBlocksList);
            base.Controls.Add(this.label1);
            base.Controls.Add(this.btnCancel);
            base.Controls.Add(this.btnOK);
            base.FormBorderStyle = FormBorderStyle.FixedSingle;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "Form3_CustomArrow";
            base.ShowInTaskbar = false;
            base.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Select Custom Arrow Block";
            base.Load += new EventHandler(this.Form3_CustomArrow_Load);
            base.ResumeLayout(false);
            base.PerformLayout();
        }
    }
}
