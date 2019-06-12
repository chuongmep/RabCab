using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UCImageCombo;

namespace RabCab.Engine.GUI
{
    public partial class WeldGui : Form
    {
        public WeldGui()
        {
            InitializeComponent();
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
            WeldType_B.SelectedIndex = 1;
        }

        private void WeldFlag_CheckedChanged(object sender, EventArgs e)
        {
            if (WeldFlag.Checked)
            {
                WeldFlag.Image = global::RabCab.Properties.Resources.Weld_Flag;
            }
            else
            {
                WeldFlag.Image = global::RabCab.Properties.Resources.Weld_NoFlag;
            }
        }

        private void WeldAllAround_CheckedChanged(object sender, EventArgs e)
        {
            if (WeldAllAround.Checked)
            {
                WeldAllAround.Image = global::RabCab.Properties.Resources.Weld_AllAround;
            }
            else
            {
                WeldAllAround.Image = global::RabCab.Properties.Resources.Weld_Single;
            }
        }

        private void FlipSyms_Click(object sender, EventArgs e)
        {

        }

        private void WeldType_T_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.SuspendLayout();
            
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

                    if (Contour_T.SelectedIndex > 0)
                    {
                        Method_T.Visible = true;
                    }

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

                    if (Contour_T.SelectedIndex > 0)
                    {
                        Method_T.Visible = true;
                    }

                    break;
                case 3: //Spot

                    Prefix_T.Visible = true;

                    Size_T.Visible = true;
                    Length_T.Visible = true;

                    Contour_T.Visible = true;

                    if (Contour_T.SelectedIndex > 0)
                    {
                        Method_T.Visible = true;
                    }

                    break;
                case 4: //Seam

                    Prefix_T.Visible = true;

                    Size_T.Visible = true;
                    Length_T.Visible = true;
                    Pitch_T.Visible = true;
                    Contour_T.Visible = true;

                    Minus_T.Visible = true;

                    if (Contour_T.SelectedIndex > 0)
                    {
                        Method_T.Visible = true;
                    }

                    break;
                case 5: //Backing

                    Prefix_T.Visible = true;
                    Size_T.Visible = true;
                    Length_T.Visible = true;
                    Pitch_T.Visible = true;
                    Contour_T.Visible = true;

                    Minus_T.Visible = true;

                    if (Contour_T.SelectedIndex > 0)
                    {
                        Method_T.Visible = true;
                    }

                    break;
                case 6: //Melt

                    Prefix_T.Visible = true;
                    Size_T.Visible = true;
                    Contour_T.Visible = true;
                    Angle_T.Visible = true;

                    if (Contour_T.SelectedIndex > 0)
                    {
                        Method_T.Visible = true;
                    }

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

                    if (Contour_T.SelectedIndex > 0)
                    {
                        Method_T.Visible = true;
                    }

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

                    if (Contour_T.SelectedIndex > 0)
                    {
                        Method_T.Visible = true;
                    }

                    break;
                case 9: //Square Groove

                    Prefix_T.Visible = true;
                    Size_T.Visible = true;
                    Length_T.Visible = true;
                    Pitch_T.Visible = true;
                    Contour_T.Visible = true;
                    Angle_T.Visible = true;

                    Minus_T.Visible = true;

                    if (Contour_T.SelectedIndex > 0)
                    {
                        Method_T.Visible = true;
                    }

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

                    if (Contour_T.SelectedIndex > 0)
                    {
                        Method_T.Visible = true;
                    }

                    break;

            }

            this.ResumeLayout();
        }

        private void WeldType_B_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.SuspendLayout();

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

                    if (Contour_B.SelectedIndex > 0)
                    {
                        Method_B.Visible = true;
                    }

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

                    if (Contour_B.SelectedIndex > 0)
                    {
                        Method_B.Visible = true;
                    }

                    break;
                case 3: //Spot

                    Prefix_B.Visible = true;

                    Size_B.Visible = true;
                    Length_B.Visible = true;

                    Contour_B.Visible = true;

                    if (Contour_B.SelectedIndex > 0)
                    {
                        Method_B.Visible = true;
                    }

                    break;
                case 4: //Seam

                    Prefix_B.Visible = true;

                    Size_B.Visible = true;
                    Length_B.Visible = true;
                    Pitch_B.Visible = true;
                    Contour_B.Visible = true;

                    Minus_B.Visible = true;

                    if (Contour_B.SelectedIndex > 0)
                    {
                        Method_B.Visible = true;
                    }

                    break;
                case 5: //Backing

                    Prefix_B.Visible = true;
                    Size_B.Visible = true;
                    Length_B.Visible = true;
                    Pitch_B.Visible = true;
                    Contour_B.Visible = true;

                    Minus_B.Visible = true;

                    if (Contour_B.SelectedIndex > 0)
                    {
                        Method_B.Visible = true;
                    }

                    break;
                case 6: //Melt

                    Prefix_B.Visible = true;
                    Size_B.Visible = true;
                    Contour_B.Visible = true;
                    Angle_B.Visible = true;

                    if (Contour_B.SelectedIndex > 0)
                    {
                        Method_B.Visible = true;
                    }

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

                    if (Contour_B.SelectedIndex > 0)
                    {
                        Method_B.Visible = true;
                    }

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

                    if (Contour_B.SelectedIndex > 0)
                    {
                        Method_B.Visible = true;
                    }

                    break;
                case 9: //Square Groove

                    Prefix_B.Visible = true;
                    Size_B.Visible = true;
                    Length_B.Visible = true;
                    Pitch_B.Visible = true;
                    Contour_B.Visible = true;
                    Angle_B.Visible = true;

                    Minus_B.Visible = true;

                    if (Contour_B.SelectedIndex > 0)
                    {
                        Method_B.Visible = true;
                    }

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

                    if (Contour_B.SelectedIndex > 0)
                    {
                        Method_B.Visible = true;
                    }

                    break;

            }

            this.ResumeLayout();
        }

        private void Contour_T_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Contour_T.SelectedIndex > 0)
            {
                Method_T.Visible = true;
            }
            else
            {
                Method_T.Visible = false;
            }
        }

        private void Contour_B_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Contour_B.SelectedIndex > 0)
            {
                Method_B.Visible = true;
            }
            else
            {
                Method_B.Visible = false;
            }
        }
    }
}
