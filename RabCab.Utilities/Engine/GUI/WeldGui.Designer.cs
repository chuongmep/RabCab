namespace RabCab.Engine.GUI
{
    partial class WeldGui
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WeldGui));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.OkButton = new System.Windows.Forms.Button();
            this.CancelButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.Method_B = new UCImageCombo.ImageCombo();
            this.WLetList = new System.Windows.Forms.ImageList(this.components);
            this.Method_T = new UCImageCombo.ImageCombo();
            this.Contour_B = new UCImageCombo.ImageCombo();
            this.WTypeList = new System.Windows.Forms.ImageList(this.components);
            this.WeldType_B = new UCImageCombo.ImageCombo();
            this.WSymbolList = new System.Windows.Forms.ImageList(this.components);
            this.Contour_T = new UCImageCombo.ImageCombo();
            this.WeldType_T = new UCImageCombo.ImageCombo();
            this.WeldFlag = new System.Windows.Forms.CheckBox();
            this.WeldAllAround = new System.Windows.Forms.CheckBox();
            this.TailNote = new System.Windows.Forms.TextBox();
            this.shapeContainer1 = new Microsoft.VisualBasic.PowerPacks.ShapeContainer();
            this.lineShape3 = new Microsoft.VisualBasic.PowerPacks.LineShape();
            this.lineShape2 = new Microsoft.VisualBasic.PowerPacks.LineShape();
            this.lineShape1 = new Microsoft.VisualBasic.PowerPacks.LineShape();
            this.WIDList = new System.Windows.Forms.ImageList(this.components);
            this.WStaggerList = new System.Windows.Forms.ImageList(this.components);
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(384, 221);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 3;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel3.Controls.Add(this.OkButton, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.CancelButton, 2, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 189);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(378, 29);
            this.tableLayoutPanel3.TabIndex = 1;
            // 
            // OkButton
            // 
            this.OkButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.OkButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.OkButton.Location = new System.Drawing.Point(181, 3);
            this.OkButton.Name = "OkButton";
            this.OkButton.Size = new System.Drawing.Size(94, 23);
            this.OkButton.TabIndex = 0;
            this.OkButton.Text = "OK";
            this.OkButton.UseVisualStyleBackColor = true;
            this.OkButton.Click += new System.EventHandler(this.OkButton_Click);
            // 
            // CancelButton
            // 
            this.CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CancelButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CancelButton.Location = new System.Drawing.Point(281, 3);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(94, 23);
            this.CancelButton.TabIndex = 1;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.Method_B);
            this.groupBox1.Controls.Add(this.Method_T);
            this.groupBox1.Controls.Add(this.Contour_B);
            this.groupBox1.Controls.Add(this.WeldType_B);
            this.groupBox1.Controls.Add(this.Contour_T);
            this.groupBox1.Controls.Add(this.WeldType_T);
            this.groupBox1.Controls.Add(this.WeldFlag);
            this.groupBox1.Controls.Add(this.WeldAllAround);
            this.groupBox1.Controls.Add(this.TailNote);
            this.groupBox1.Controls.Add(this.shapeContainer1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(378, 180);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            // 
            // Method_B
            // 
            this.Method_B.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.Method_B.DropDownHeight = 200;
            this.Method_B.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Method_B.DropDownWidth = 75;
            this.Method_B.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.Method_B.ImageList = this.WLetList;
            this.Method_B.IntegralHeight = false;
            this.Method_B.ItemHeight = 25;
            this.Method_B.Location = new System.Drawing.Point(231, 140);
            this.Method_B.MaxDropDownItems = 10;
            this.Method_B.Name = "Method_B";
            this.Method_B.Size = new System.Drawing.Size(47, 31);
            this.Method_B.TabIndex = 42;
            // 
            // WLetList
            // 
            this.WLetList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("WLetList.ImageStream")));
            this.WLetList.TransparentColor = System.Drawing.Color.Transparent;
            this.WLetList.Images.SetKeyName(0, "Weld_Blank.png");
            this.WLetList.Images.SetKeyName(1, "Weld_LetterC.png");
            this.WLetList.Images.SetKeyName(2, "Weld_LetterG.png");
            this.WLetList.Images.SetKeyName(3, "Weld_LetterH.png");
            this.WLetList.Images.SetKeyName(4, "Weld_LetterM.png");
            this.WLetList.Images.SetKeyName(5, "Weld_LetterR.png");
            // 
            // Method_T
            // 
            this.Method_T.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.Method_T.DropDownHeight = 200;
            this.Method_T.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Method_T.DropDownWidth = 75;
            this.Method_T.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.Method_T.ImageList = this.WLetList;
            this.Method_T.IntegralHeight = false;
            this.Method_T.ItemHeight = 25;
            this.Method_T.Location = new System.Drawing.Point(231, 16);
            this.Method_T.MaxDropDownItems = 10;
            this.Method_T.Name = "Method_T";
            this.Method_T.Size = new System.Drawing.Size(47, 31);
            this.Method_T.TabIndex = 41;
            // 
            // Contour_B
            // 
            this.Contour_B.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.Contour_B.DropDownHeight = 125;
            this.Contour_B.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Contour_B.DropDownWidth = 250;
            this.Contour_B.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.Contour_B.ImageList = this.WTypeList;
            this.Contour_B.IntegralHeight = false;
            this.Contour_B.ItemHeight = 25;
            this.Contour_B.Location = new System.Drawing.Point(231, 103);
            this.Contour_B.MaxDropDownItems = 4;
            this.Contour_B.Name = "Contour_B";
            this.Contour_B.Size = new System.Drawing.Size(60, 31);
            this.Contour_B.TabIndex = 38;
            this.Contour_B.SelectedIndexChanged += new System.EventHandler(this.Contour_B_SelectedIndexChanged);
            // 
            // WTypeList
            // 
            this.WTypeList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("WTypeList.ImageStream")));
            this.WTypeList.TransparentColor = System.Drawing.Color.Transparent;
            this.WTypeList.Images.SetKeyName(0, "Weld_Blank.png");
            this.WTypeList.Images.SetKeyName(1, "Weld_Concave.png");
            this.WTypeList.Images.SetKeyName(2, "Weld_Flush.png");
            this.WTypeList.Images.SetKeyName(3, "Weld_Convex.png");
            // 
            // WeldType_B
            // 
            this.WeldType_B.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.WeldType_B.DropDownHeight = 300;
            this.WeldType_B.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.WeldType_B.DropDownWidth = 120;
            this.WeldType_B.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.WeldType_B.ImageList = this.WSymbolList;
            this.WeldType_B.IntegralHeight = false;
            this.WeldType_B.ItemHeight = 24;
            this.WeldType_B.Location = new System.Drawing.Point(177, 103);
            this.WeldType_B.MaxDropDownItems = 12;
            this.WeldType_B.Name = "WeldType_B";
            this.WeldType_B.Size = new System.Drawing.Size(48, 30);
            this.WeldType_B.TabIndex = 26;
            this.WeldType_B.SelectedIndexChanged += new System.EventHandler(this.WeldType_B_SelectedIndexChanged);
            // 
            // WSymbolList
            // 
            this.WSymbolList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("WSymbolList.ImageStream")));
            this.WSymbolList.TransparentColor = System.Drawing.Color.Transparent;
            this.WSymbolList.Images.SetKeyName(0, "Weld_Blank.png");
            this.WSymbolList.Images.SetKeyName(1, "Weld_Fillet.png");
            this.WSymbolList.Images.SetKeyName(2, "Weld_Plug.png");
            this.WSymbolList.Images.SetKeyName(3, "Weld_Spot.png");
            this.WSymbolList.Images.SetKeyName(4, "Weld_Seam.png");
            this.WSymbolList.Images.SetKeyName(5, "Weld_Backing.png");
            this.WSymbolList.Images.SetKeyName(6, "Weld_Melt.png");
            this.WSymbolList.Images.SetKeyName(7, "Weld_FlangeEdge.png");
            this.WSymbolList.Images.SetKeyName(8, "Weld_FlangeCorner.png");
            this.WSymbolList.Images.SetKeyName(9, "Weld_SquareGroove.png");
            this.WSymbolList.Images.SetKeyName(10, "Weld_VGroove.png");
            // 
            // Contour_T
            // 
            this.Contour_T.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.Contour_T.DropDownHeight = 125;
            this.Contour_T.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Contour_T.DropDownWidth = 250;
            this.Contour_T.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.Contour_T.ImageList = this.WTypeList;
            this.Contour_T.IntegralHeight = false;
            this.Contour_T.ItemHeight = 25;
            this.Contour_T.Location = new System.Drawing.Point(231, 52);
            this.Contour_T.MaxDropDownItems = 4;
            this.Contour_T.Name = "Contour_T";
            this.Contour_T.Size = new System.Drawing.Size(60, 31);
            this.Contour_T.TabIndex = 17;
            this.Contour_T.SelectedIndexChanged += new System.EventHandler(this.Contour_T_SelectedIndexChanged);
            // 
            // WeldType_T
            // 
            this.WeldType_T.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.WeldType_T.DropDownHeight = 300;
            this.WeldType_T.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.WeldType_T.DropDownWidth = 120;
            this.WeldType_T.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.WeldType_T.ImageList = this.WSymbolList;
            this.WeldType_T.IntegralHeight = false;
            this.WeldType_T.ItemHeight = 24;
            this.WeldType_T.Location = new System.Drawing.Point(177, 53);
            this.WeldType_T.MaxDropDownItems = 12;
            this.WeldType_T.Name = "WeldType_T";
            this.WeldType_T.Size = new System.Drawing.Size(48, 30);
            this.WeldType_T.TabIndex = 11;
            this.WeldType_T.SelectedIndexChanged += new System.EventHandler(this.WeldType_T_SelectedIndexChanged);
            // 
            // WeldFlag
            // 
            this.WeldFlag.Appearance = System.Windows.Forms.Appearance.Button;
            this.WeldFlag.AutoSize = true;
            this.WeldFlag.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.WeldFlag.Image = global::RabCab.Properties.Resources.Weld_NoFlag;
            this.WeldFlag.ImageAlign = System.Drawing.ContentAlignment.TopRight;
            this.WeldFlag.Location = new System.Drawing.Point(329, 32);
            this.WeldFlag.MaximumSize = new System.Drawing.Size(40, 40);
            this.WeldFlag.MinimumSize = new System.Drawing.Size(40, 40);
            this.WeldFlag.Name = "WeldFlag";
            this.WeldFlag.Size = new System.Drawing.Size(40, 40);
            this.WeldFlag.TabIndex = 3;
            this.WeldFlag.UseVisualStyleBackColor = true;
            this.WeldFlag.CheckedChanged += new System.EventHandler(this.WeldFlag_CheckedChanged);
            // 
            // WeldAllAround
            // 
            this.WeldAllAround.Appearance = System.Windows.Forms.Appearance.Button;
            this.WeldAllAround.AutoSize = true;
            this.WeldAllAround.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.WeldAllAround.Image = global::RabCab.Properties.Resources.Weld_Single;
            this.WeldAllAround.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.WeldAllAround.Location = new System.Drawing.Point(329, 78);
            this.WeldAllAround.MaximumSize = new System.Drawing.Size(40, 40);
            this.WeldAllAround.MinimumSize = new System.Drawing.Size(40, 40);
            this.WeldAllAround.Name = "WeldAllAround";
            this.WeldAllAround.Size = new System.Drawing.Size(40, 40);
            this.WeldAllAround.TabIndex = 2;
            this.WeldAllAround.UseVisualStyleBackColor = true;
            this.WeldAllAround.CheckedChanged += new System.EventHandler(this.WeldAllAround_CheckedChanged);
            // 
            // TailNote
            // 
            this.TailNote.Location = new System.Drawing.Point(4, 62);
            this.TailNote.Multiline = true;
            this.TailNote.Name = "TailNote";
            this.TailNote.Size = new System.Drawing.Size(100, 60);
            this.TailNote.TabIndex = 0;
            this.TailNote.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // shapeContainer1
            // 
            this.shapeContainer1.Location = new System.Drawing.Point(3, 16);
            this.shapeContainer1.Margin = new System.Windows.Forms.Padding(0);
            this.shapeContainer1.Name = "shapeContainer1";
            this.shapeContainer1.Shapes.AddRange(new Microsoft.VisualBasic.PowerPacks.Shape[] {
            this.lineShape3,
            this.lineShape2,
            this.lineShape1});
            this.shapeContainer1.Size = new System.Drawing.Size(372, 161);
            this.shapeContainer1.TabIndex = 1;
            this.shapeContainer1.TabStop = false;
            // 
            // lineShape3
            // 
            this.lineShape3.BorderWidth = 2;
            this.lineShape3.Name = "lineShape3";
            this.lineShape3.X1 = 151;
            this.lineShape3.X2 = 335;
            this.lineShape3.Y1 = 76;
            this.lineShape3.Y2 = 76;
            // 
            // lineShape2
            // 
            this.lineShape2.BorderWidth = 2;
            this.lineShape2.Name = "lineShape2";
            this.lineShape2.X1 = 101;
            this.lineShape2.X2 = 151;
            this.lineShape2.Y1 = 116;
            this.lineShape2.Y2 = 76;
            // 
            // lineShape1
            // 
            this.lineShape1.BorderWidth = 2;
            this.lineShape1.Name = "lineShape1";
            this.lineShape1.X1 = 101;
            this.lineShape1.X2 = 151;
            this.lineShape1.Y1 = 36;
            this.lineShape1.Y2 = 76;
            // 
            // WIDList
            // 
            this.WIDList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("WIDList.ImageStream")));
            this.WIDList.TransparentColor = System.Drawing.Color.Transparent;
            this.WIDList.Images.SetKeyName(0, "Weld_NoId.png");
            this.WIDList.Images.SetKeyName(1, "Weld_IdAbove.png");
            this.WIDList.Images.SetKeyName(2, "Weld_IdBelow.png");
            // 
            // WStaggerList
            // 
            this.WStaggerList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("WStaggerList.ImageStream")));
            this.WStaggerList.TransparentColor = System.Drawing.Color.Transparent;
            this.WStaggerList.Images.SetKeyName(0, "Weld_NoStagger.png");
            this.WStaggerList.Images.SetKeyName(1, "Weld_StaggerMove.png");
            this.WStaggerList.Images.SetKeyName(2, "Weld_StaggerMirror.png");
            // 
            // WeldGui
            // 
            this.AcceptButton = this.OkButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 221);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(400, 260);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(400, 260);
            this.Name = "WeldGui";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Welding Symbol";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.WeldGui_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Button OkButton;
        private System.Windows.Forms.Button CancelButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox TailNote;
        private System.Windows.Forms.CheckBox WeldFlag;
        private System.Windows.Forms.CheckBox WeldAllAround;
        private Microsoft.VisualBasic.PowerPacks.ShapeContainer shapeContainer1;
        private Microsoft.VisualBasic.PowerPacks.LineShape lineShape3;
        private Microsoft.VisualBasic.PowerPacks.LineShape lineShape2;
        private Microsoft.VisualBasic.PowerPacks.LineShape lineShape1;
        private UCImageCombo.ImageCombo Method_B;
        private UCImageCombo.ImageCombo Method_T;
        private UCImageCombo.ImageCombo Contour_B;
        private UCImageCombo.ImageCombo WeldType_B;
        private UCImageCombo.ImageCombo Contour_T;
        private UCImageCombo.ImageCombo WeldType_T;
        private System.Windows.Forms.ImageList WSymbolList;
        private System.Windows.Forms.ImageList WTypeList;
        private System.Windows.Forms.ImageList WIDList;
        private System.Windows.Forms.ImageList WStaggerList;
        private System.Windows.Forms.ImageList WLetList;
    }
}