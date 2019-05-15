using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace RabCab.Entities.Controls
{
    public sealed class ReadOnlyTextBox : TextBox
    {
        public ReadOnlyTextBox()
        {
            ReadOnly = true;
            GotFocus += TextBoxGotFocus;
            //this.Paint += TextBorder_Paint;
            BorderStyle = BorderStyle.None;
            Cursor = Cursors.Arrow; // mouse cursor like in other controls
        }

        [DllImport("user32.dll")]
        private static extern bool HideCaret(IntPtr hWnd);

        private void TextBoxGotFocus(object sender, EventArgs args)
        {
            HideCaret(Handle);
        }
    }
}