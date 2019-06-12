using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using RabCab.Agents;
using RabCab.Calculators;
using RabCab.Extensions;
using RabCab.Settings;
using UCImageCombo;
using static Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace RabCab.Engine.GUI
{
    public partial class WeldGui : Form
    {
        public List<Entity> drawnEnts = new List<Entity>();
        public Point3d SymStart;
        public Point3d SymEnd;
        public bool Success;

        public WeldGui(Point3d sStart, Point3d sEnd)
        {
            InitializeComponent();

            SymStart = sStart;
            SymEnd = sEnd;

            foreach (Control c in groupBox1.Controls)
                switch (c)
                {
                    case CheckBox box:
                        box.CheckedChanged += c_ControlChanged;
                        break;
                    case ComboBox combo:
                        combo.SelectedIndexChanged += c_ControlChanged;
                        break;
                    default:
                        c.TextChanged += c_ControlChanged;
                        break;
                }
        }

        private void WeldGui_Load(object sender, EventArgs e)
        {
            WeldType_T.Items.Add(new ImageComboItem("None", 0));
            WeldType_T.Items.Add(new ImageComboItem("Fillet", 1));
            WeldType_T.Items.Add(new ImageComboItem("Plug", 2));
            WeldType_T.Items.Add(new ImageComboItem("Spot", 3));
            WeldType_T.Items.Add(new ImageComboItem("Seam", 4));
            WeldType_T.Items.Add(new ImageComboItem("Backing", 5));
            WeldType_T.Items.Add(new ImageComboItem("Melt Thru", 6));
            WeldType_T.Items.Add(new ImageComboItem("Flange Edge", 7));
            WeldType_T.Items.Add(new ImageComboItem("Flange Corner", 8));
            WeldType_T.Items.Add(new ImageComboItem("Square Groove", 9));
            WeldType_T.Items.Add(new ImageComboItem("V Groove", 10));

            WeldType_B.Items.Add(new ImageComboItem("None", 0));
            WeldType_B.Items.Add(new ImageComboItem("Fillet", 1));
            WeldType_B.Items.Add(new ImageComboItem("Plug", 2));
            WeldType_B.Items.Add(new ImageComboItem("Spot", 3));
            WeldType_B.Items.Add(new ImageComboItem("Seam", 4));
            WeldType_B.Items.Add(new ImageComboItem("Backing", 5));
            WeldType_B.Items.Add(new ImageComboItem("Melt Thru", 6));
            WeldType_B.Items.Add(new ImageComboItem("Flange Edge", 7));
            WeldType_B.Items.Add(new ImageComboItem("Flange Corner", 8));
            WeldType_B.Items.Add(new ImageComboItem("Square Groove", 9));
            WeldType_B.Items.Add(new ImageComboItem("V Groove", 10));

            Contour_B.Items.Add(new ImageComboItem("None", 0));
            Contour_B.Items.Add(new ImageComboItem("Concave", 1));
            Contour_B.Items.Add(new ImageComboItem("Flush", 2));
            Contour_B.Items.Add(new ImageComboItem("Convex", 3));

            Contour_T.Items.Add(new ImageComboItem("None", 0));
            Contour_T.Items.Add(new ImageComboItem("Concave", 1));
            Contour_T.Items.Add(new ImageComboItem("Flush", 2));
            Contour_T.Items.Add(new ImageComboItem("Convex", 3));

            IdCombo.Items.Add(new ImageComboItem("No ID", 0));
            IdCombo.Items.Add(new ImageComboItem("ID on Top", 1));
            IdCombo.Items.Add(new ImageComboItem("ID on Bottom", 2));

            StaggerCombo.Items.Add(new ImageComboItem("No Stagger", 0));
            StaggerCombo.Items.Add(new ImageComboItem("Move", 1));
            StaggerCombo.Items.Add(new ImageComboItem("Mirror", 2));

            Method_B.Items.Add(new ImageComboItem("None", 0));
            Method_B.Items.Add(new ImageComboItem("Chipping", 1));
            Method_B.Items.Add(new ImageComboItem("Grinding", 2));
            Method_B.Items.Add(new ImageComboItem("Hammering", 3));
            Method_B.Items.Add(new ImageComboItem("Machining", 4));
            Method_B.Items.Add(new ImageComboItem("Rolling", 5));

            Method_T.Items.Add(new ImageComboItem("None", 0));
            Method_T.Items.Add(new ImageComboItem("Chipping", 1));
            Method_T.Items.Add(new ImageComboItem("Grinding", 2));
            Method_T.Items.Add(new ImageComboItem("Hammering", 3));
            Method_T.Items.Add(new ImageComboItem("Machining", 4));
            Method_T.Items.Add(new ImageComboItem("Rolling", 5));

            WeldType_T.SelectedIndex = 0;
            WeldType_B.SelectedIndex = 0;
            IdCombo.SelectedIndex = 0;
            StaggerCombo.SelectedIndex = 0;
            PresetCombo.SelectedIndex = 0;
            Focus();
        }

        #region Draw Transients

        private void c_ControlChanged(object sender, EventArgs e)
        {
            TransientAgent.Clear();

            foreach (var ent in drawnEnts)
            {
                ent.Dispose();
            }

            drawnEnts.Clear();
            
            //TODO add all drawbles here
            
            //Draw weld symbol

            #region TopWeldSymbol

            var index = WeldType_T.SelectedIndex;
            var cIndex = Contour_T.SelectedIndex;

            switch (index)
            {
                case 1: //Fillet

                    //Draw Symbol
                    var mPoint = SymStart.GetMidPoint(SymEnd);
                    var fLineLength = CalcUnit.GetProportion(.2, 1, SettingsUser.WeldSymbolLength);
                    var fLinePt1 = new Point2d(mPoint.X, mPoint.Y);
                    var fLinePt2 = new Point2d(fLinePt1.X, fLinePt1.Y + fLineLength);
                    var fLinePt3 = new Point2d(fLinePt1.X + fLineLength, fLinePt1.Y);

                    var fLine = new Polyline(3);
                    fLine.AddVertexAt(0, fLinePt1, 0, 0, 0);
                    fLine.AddVertexAt(0, fLinePt2, 0, 0, 0);
                    fLine.AddVertexAt(0, fLinePt3, 0, 0, 0);
                    fLine.Closed = false;

                    drawnEnts.Add(fLine);

                    if (Contour_T.SelectedIndex > 0)
                    {
                        switch (cIndex)
                        {
                            case 1: //Concave
                                break;
                            case 2: //Flush
                                break;
                            case 3: //Convex
                                break;
                        }
                    }

                    break;
                case 2: //Plug

                    //Draw Symbol

                    if (Contour_T.SelectedIndex > 0)
                    {
                        switch (cIndex)
                        {
                            case 1: //Concave
                                break;
                            case 2: //Flush
                                break;
                            case 3: //Convex
                                break;
                        }
                    }

                    break;
                case 3: //Spot

                    //Draw Symbol

                    if (Contour_T.SelectedIndex > 0)
                    {
                        switch (cIndex)
                        {
                            case 1: //Concave
                                break;
                            case 2: //Flush
                                break;
                            case 3: //Convex
                                break;
                        }
                    }

                    break;
                case 4: //Seam

                    //Draw Symbol

                    if (Contour_T.SelectedIndex > 0)
                    {
                        switch (cIndex)
                        {
                            case 1: //Concave
                                break;
                            case 2: //Flush
                                break;
                            case 3: //Convex
                                break;
                        }
                    }

                    break;
                case 5: //Backing

                    //Draw Symbol

                    if (Contour_T.SelectedIndex > 0)
                    {
                        switch (cIndex)
                        {
                            case 1: //Concave
                                break;
                            case 2: //Flush
                                break;
                            case 3: //Convex
                                break;
                        }
                    }

                    break;
                case 6: //Melt

                    //Draw Symbol

                    if (Contour_T.SelectedIndex > 0)
                    {
                        switch (cIndex)
                        {
                            case 1: //Concave
                                break;
                            case 2: //Flush
                                break;
                            case 3: //Convex
                                break;
                        }
                    }

                    break;
                case 7: //Flange Edge

                    //Draw Symbol

                    if (Contour_T.SelectedIndex > 0)
                    {
                        switch (cIndex)
                        {
                            case 1: //Concave
                                break;
                            case 2: //Flush
                                break;
                            case 3: //Convex
                                break;
                        }
                    }

                    break;
                case 8: //Flange Corner

                    //Draw Symbol

                    if (Contour_T.SelectedIndex > 0)
                    {
                        switch (cIndex)
                        {
                            case 1: //Concave
                                break;
                            case 2: //Flush
                                break;
                            case 3: //Convex
                                break;
                        }
                    }

                    break;
                case 9: //Square Groove

                    //Draw Symbol

                    if (Contour_T.SelectedIndex > 0)
                    {
                        switch (cIndex)
                        {
                            case 1: //Concave
                                break;
                            case 2: //Flush
                                break;
                            case 3: //Convex
                                break;
                        }
                    }

                    break;
                case 10: //V Groove

                    //Draw Symbol

                    if (Contour_T.SelectedIndex > 0)
                    {
                        switch (cIndex)
                        {
                            case 1: //Concave
                                break;
                            case 2: //Flush
                                break;
                            case 3: //Convex
                                break;
                        }
                    }
                    break;
            }
            #endregion

            #region BottomWeldSymbol

            index = WeldType_B.SelectedIndex;

            #endregion

            TransientAgent.Add(drawnEnts.ToArray());
            TransientAgent.Draw();
            DocumentManager.MdiActiveDocument.Editor.Regen();
        }

        #endregion

        #region Visibility Handlers

        private void WeldFlag_CheckedChanged(object sender, EventArgs e)
        {
            if (WeldFlag.Checked)
                WeldFlag.Image = Properties.Resources.Weld_Flag;
            else
                WeldFlag.Image = Properties.Resources.Weld_NoFlag;
        }

        private void WeldAllAround_CheckedChanged(object sender, EventArgs e)
        {
            if (WeldAllAround.Checked)
                WeldAllAround.Image = Properties.Resources.Weld_AllAround;
            else
                WeldAllAround.Image = Properties.Resources.Weld_Single;
        }

        private void FlipSyms_Click(object sender, EventArgs e)
        {
            //TODO
            //Implement method for swapping
        }

        private void WeldType_T_SelectedIndexChanged(object sender, EventArgs e)
        {
            SuspendLayout();

            var index = WeldType_T.SelectedIndex;

            Prefix_T.Visible = false;
            Leg1_T.Visible = false;
            Leg2_T.Visible = false;
            Size_T.Visible = false;
            Length_T.Visible = false;
            Pitch_T.Visible = false;
            Contour_T.Visible = false;
            Method_T.Visible = false;
            Depth_T.Visible = false;
            Angle_T.Visible = false;

            Plus_T.Visible = false;
            Minus_T.Visible = false;

            switch (index)
            {
                case 0:
                    break;
                case 1: //Fillet

                    Prefix_T.Visible = true;
                    Leg1_T.Visible = true;
                    Leg2_T.Visible = true;
                    Length_T.Visible = true;
                    Pitch_T.Visible = true;
                    Contour_T.Visible = true;

                    Minus_T.Visible = true;

                    if (Contour_T.SelectedIndex > 0) Method_T.Visible = true;

                    break;
                case 2: //Plug
                    Prefix_T.Visible = true;

                    Size_T.Visible = true;
                    Length_T.Visible = true;
                    Pitch_T.Visible = true;
                    Contour_T.Visible = true;
                    Depth_T.Visible = true;
                    Angle_T.Visible = true;

                    Minus_T.Visible = true;

                    if (Contour_T.SelectedIndex > 0) Method_T.Visible = true;

                    break;
                case 3: //Spot

                    Prefix_T.Visible = true;

                    Size_T.Visible = true;
                    Length_T.Visible = true;

                    Contour_T.Visible = true;

                    if (Contour_T.SelectedIndex > 0) Method_T.Visible = true;

                    break;
                case 4: //Seam

                    Prefix_T.Visible = true;

                    Size_T.Visible = true;
                    Length_T.Visible = true;
                    Pitch_T.Visible = true;
                    Contour_T.Visible = true;

                    Minus_T.Visible = true;

                    if (Contour_T.SelectedIndex > 0) Method_T.Visible = true;

                    break;
                case 5: //Backing

                    Prefix_T.Visible = true;
                    Size_T.Visible = true;
                    Length_T.Visible = true;
                    Pitch_T.Visible = true;
                    Contour_T.Visible = true;

                    Minus_T.Visible = true;

                    if (Contour_T.SelectedIndex > 0) Method_T.Visible = true;

                    break;
                case 6: //Melt

                    Prefix_T.Visible = true;
                    Size_T.Visible = true;
                    Contour_T.Visible = true;
                    Angle_T.Visible = true;

                    if (Contour_T.SelectedIndex > 0) Method_T.Visible = true;

                    break;
                case 7: //Flange Edge

                    Prefix_T.Visible = true;
                    Leg1_T.Visible = true;
                    Leg2_T.Visible = true;
                    Length_T.Visible = true;
                    Pitch_T.Visible = true;
                    Contour_T.Visible = true;
                    Depth_T.Visible = true;

                    Plus_T.Visible = true;
                    Minus_T.Visible = true;

                    if (Contour_T.SelectedIndex > 0) Method_T.Visible = true;

                    break;
                case 8: //Flange Corner

                    Prefix_T.Visible = true;
                    Leg1_T.Visible = true;
                    Leg2_T.Visible = true;
                    Length_T.Visible = true;
                    Pitch_T.Visible = true;
                    Contour_T.Visible = true;
                    Depth_T.Visible = true;

                    Plus_T.Visible = true;
                    Minus_T.Visible = true;

                    if (Contour_T.SelectedIndex > 0) Method_T.Visible = true;

                    break;
                case 9: //Square Groove

                    Prefix_T.Visible = true;
                    Size_T.Visible = true;
                    Length_T.Visible = true;
                    Pitch_T.Visible = true;
                    Contour_T.Visible = true;
                    Angle_T.Visible = true;

                    Minus_T.Visible = true;

                    if (Contour_T.SelectedIndex > 0) Method_T.Visible = true;

                    break;
                case 10: //V Groove

                    Prefix_T.Visible = true;
                    Size_T.Visible = true;
                    Length_T.Visible = true;
                    Pitch_T.Visible = true;
                    Contour_T.Visible = true;
                    Depth_T.Visible = true;
                    Angle_T.Visible = true;

                    Minus_T.Visible = true;

                    if (Contour_T.SelectedIndex > 0) Method_T.Visible = true;

                    break;
            }

            ResumeLayout();
        }

        private void WeldType_B_SelectedIndexChanged(object sender, EventArgs e)
        {
            SuspendLayout();

            var index = WeldType_B.SelectedIndex;

            Prefix_B.Visible = false;
            Leg1_B.Visible = false;
            Leg2_B.Visible = false;
            Size_B.Visible = false;
            Length_B.Visible = false;
            Pitch_B.Visible = false;
            Contour_B.Visible = false;
            Method_B.Visible = false;
            Depth_B.Visible = false;
            Angle_B.Visible = false;

            Plus_B.Visible = false;
            Minus_B.Visible = false;

            switch (index)
            {
                case 0:
                    break;
                case 1: //Fillet
                    Prefix_B.Visible = true;
                    Leg1_B.Visible = true;
                    Leg2_B.Visible = true;
                    Length_B.Visible = true;
                    Pitch_B.Visible = true;
                    Contour_B.Visible = true;

                    Minus_B.Visible = true;

                    if (Contour_B.SelectedIndex > 0) Method_B.Visible = true;

                    break;
                case 2: //Plug
                    Prefix_B.Visible = true;

                    Size_B.Visible = true;
                    Length_B.Visible = true;
                    Pitch_B.Visible = true;
                    Contour_B.Visible = true;
                    Depth_B.Visible = true;
                    Angle_B.Visible = true;

                    Minus_B.Visible = true;

                    if (Contour_B.SelectedIndex > 0) Method_B.Visible = true;

                    break;
                case 3: //Spot

                    Prefix_B.Visible = true;

                    Size_B.Visible = true;
                    Length_B.Visible = true;

                    Contour_B.Visible = true;

                    if (Contour_B.SelectedIndex > 0) Method_B.Visible = true;

                    break;
                case 4: //Seam

                    Prefix_B.Visible = true;

                    Size_B.Visible = true;
                    Length_B.Visible = true;
                    Pitch_B.Visible = true;
                    Contour_B.Visible = true;

                    Minus_B.Visible = true;

                    if (Contour_B.SelectedIndex > 0) Method_B.Visible = true;

                    break;
                case 5: //Backing

                    Prefix_B.Visible = true;
                    Size_B.Visible = true;
                    Length_B.Visible = true;
                    Pitch_B.Visible = true;
                    Contour_B.Visible = true;

                    Minus_B.Visible = true;

                    if (Contour_B.SelectedIndex > 0) Method_B.Visible = true;

                    break;
                case 6: //Melt

                    Prefix_B.Visible = true;
                    Size_B.Visible = true;
                    Contour_B.Visible = true;
                    Angle_B.Visible = true;

                    if (Contour_B.SelectedIndex > 0) Method_B.Visible = true;

                    break;
                case 7: //Flange Edge

                    Prefix_B.Visible = true;
                    Leg1_B.Visible = true;
                    Leg2_B.Visible = true;
                    Length_B.Visible = true;
                    Pitch_B.Visible = true;
                    Contour_B.Visible = true;
                    Depth_B.Visible = true;

                    Plus_B.Visible = true;
                    Minus_B.Visible = true;

                    if (Contour_B.SelectedIndex > 0) Method_B.Visible = true;

                    break;
                case 8: //Flange Corner

                    Prefix_B.Visible = true;
                    Leg1_B.Visible = true;
                    Leg2_B.Visible = true;
                    Length_B.Visible = true;
                    Pitch_B.Visible = true;
                    Contour_B.Visible = true;
                    Depth_B.Visible = true;

                    Plus_B.Visible = true;
                    Minus_B.Visible = true;

                    if (Contour_B.SelectedIndex > 0) Method_B.Visible = true;

                    break;
                case 9: //Square Groove

                    Prefix_B.Visible = true;
                    Size_B.Visible = true;
                    Length_B.Visible = true;
                    Pitch_B.Visible = true;
                    Contour_B.Visible = true;
                    Angle_B.Visible = true;

                    Minus_B.Visible = true;

                    if (Contour_B.SelectedIndex > 0) Method_B.Visible = true;

                    break;
                case 10: //V Groove

                    Prefix_B.Visible = true;
                    Size_B.Visible = true;
                    Length_B.Visible = true;
                    Pitch_B.Visible = true;
                    Contour_B.Visible = true;
                    Depth_B.Visible = true;
                    Angle_B.Visible = true;

                    Minus_B.Visible = true;

                    if (Contour_B.SelectedIndex > 0) Method_B.Visible = true;

                    break;
            }

            ResumeLayout();
        }

        private void Contour_T_SelectedIndexChanged(object sender, EventArgs e)
        {
            Method_T.Visible = Contour_T.SelectedIndex > 0;
        }

        private void Contour_B_SelectedIndexChanged(object sender, EventArgs e)
        {
            Method_B.Visible = Contour_B.SelectedIndex > 0;
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Visible = false;

        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Visible = false;
        }

        #endregion


    }
}