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
        }
    }
}
