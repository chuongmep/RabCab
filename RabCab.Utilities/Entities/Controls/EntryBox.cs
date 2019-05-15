using RabCab.Settings;

namespace RabCab.Entities.Controls
{
    using System.Drawing;
    using System.Windows.Forms;

    namespace RabCab.Entities.Controls
    {
        public class EntryBox : TextBox
        {

            public EntryBox()
            {
                this.ReadOnly = false;
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
                    this.DisplayRectangle,
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

}
