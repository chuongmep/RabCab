using System.Windows.Forms;

namespace RabCab.Entities.Controls
{
    namespace RabCab.Entities.Controls
    {
        public class EntryBox : TextBox
        {
            public EntryBox()
            {
                ReadOnly = false;
                //this.Paint += TextBorder_Paint;
                BorderStyle = BorderStyle.None;
            }
        }
    }
}