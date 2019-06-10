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
            imageCombo1.Items.Add(new ImageComboItem("None", 0));
            imageCombo1.Items.Add(new ImageComboItem("Fillet", 1));
            imageCombo1.Items.Add(new ImageComboItem("Plug", 2));
            imageCombo1.Items.Add(new ImageComboItem("Spot", 3));
            imageCombo1.Items.Add(new ImageComboItem("Seam", 4));
            imageCombo1.Items.Add(new ImageComboItem("Backing", 5));
            imageCombo1.Items.Add(new ImageComboItem("Melt Thru", 6));
            imageCombo1.Items.Add(new ImageComboItem("Flange Edge", 7));
            imageCombo1.Items.Add(new ImageComboItem("Flange Corner", 8));
            imageCombo1.Items.Add(new ImageComboItem("Square Groove", 9));
            imageCombo1.Items.Add(new ImageComboItem("V Groove", 10));

            imageCombo5.Items.Add(new ImageComboItem("None", 0));
            imageCombo5.Items.Add(new ImageComboItem("Fillet", 1));
            imageCombo5.Items.Add(new ImageComboItem("Plug", 2));
            imageCombo5.Items.Add(new ImageComboItem("Spot", 3));
            imageCombo5.Items.Add(new ImageComboItem("Seam", 4));
            imageCombo5.Items.Add(new ImageComboItem("Backing", 5));
            imageCombo5.Items.Add(new ImageComboItem("Melt Thru", 6));
            imageCombo5.Items.Add(new ImageComboItem("Flange Edge", 7));
            imageCombo5.Items.Add(new ImageComboItem("Flange Corner", 8));
            imageCombo5.Items.Add(new ImageComboItem("Square Groove", 9));
            imageCombo5.Items.Add(new ImageComboItem("V Groove", 10));

            imageCombo6.Items.Add(new ImageComboItem("None", 0));
            imageCombo6.Items.Add(new ImageComboItem("Concave", 1));
            imageCombo6.Items.Add(new ImageComboItem("Flush", 2));
            imageCombo6.Items.Add(new ImageComboItem("Convex", 3));

            imageCombo2.Items.Add(new ImageComboItem("None", 0));
            imageCombo2.Items.Add(new ImageComboItem("Concave", 1));
            imageCombo2.Items.Add(new ImageComboItem("Flush", 2));
            imageCombo2.Items.Add(new ImageComboItem("Convex", 3));

            imageCombo4.Items.Add(new ImageComboItem("No ID", 0));
            imageCombo4.Items.Add(new ImageComboItem("ID on Top", 1));
            imageCombo4.Items.Add(new ImageComboItem("ID on Bottom", 2));

            imageCombo3.Items.Add(new ImageComboItem("No Stagger", 0));
            imageCombo3.Items.Add(new ImageComboItem("Move", 1));
            imageCombo3.Items.Add(new ImageComboItem("Mirror", 2));

            imageCombo8.Items.Add(new ImageComboItem("None", 0));
            imageCombo8.Items.Add(new ImageComboItem("Chipping", 1));
            imageCombo8.Items.Add(new ImageComboItem("Grinding", 2));
            imageCombo8.Items.Add(new ImageComboItem("Hammering", 3));
            imageCombo8.Items.Add(new ImageComboItem("Machining", 4));
            imageCombo8.Items.Add(new ImageComboItem("Rolling", 5));

            imageCombo7.Items.Add(new ImageComboItem("None", 0));
            imageCombo7.Items.Add(new ImageComboItem("Chipping", 1));
            imageCombo7.Items.Add(new ImageComboItem("Grinding", 2));
            imageCombo7.Items.Add(new ImageComboItem("Hammering", 3));
            imageCombo7.Items.Add(new ImageComboItem("Machining", 4));
            imageCombo7.Items.Add(new ImageComboItem("Rolling", 5));
        }

        private void CheckBox4_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
