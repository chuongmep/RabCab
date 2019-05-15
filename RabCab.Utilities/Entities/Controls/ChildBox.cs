using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RabCab.Settings;

namespace RabCab.Entities.Controls
{

    public class ChildBox : ListBox
    {
        public ChildBox()
        {
            //this.Paint += TextBorder_Paint;
            this.BorderStyle = BorderStyle.None;
        }

        private void TextBorder_Paint(object sender, PaintEventArgs e)
        {
            var borderColor = Colors.GetCadBorderColor();
            var borderStyle = ButtonBorderStyle.Solid;
            var borderWidth = 1;

            ControlPaint.DrawBorder(
                e.Graphics,
                this.ClientRectangle,
                borderColor,
                borderWidth,
                borderStyle,
                borderColor,
                borderWidth,
                borderStyle,
                borderColor,
                borderWidth,
                borderStyle,
                borderColor,
                borderWidth,
                borderStyle);

        }
    }
}
