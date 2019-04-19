using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

namespace RabCab.Exceptions
{
    [Serializable]
    class NullException : Exception
    {
        public NullException()
        {

        }

        public NullException(string name, Editor acCurEd, Transaction acTrans) : base(String.Format("Object selected could not continue: {0}", name))
        {
            //Get the current document utilities
            acCurEd.WriteMessage("Object returned a null value - Operation aborted");
            acTrans.Abort();
        }
    }
}
