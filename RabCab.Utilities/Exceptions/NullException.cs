using System;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

namespace RabCab.Exceptions
{
    [Serializable]
    internal class NullException : Exception
    {
        public NullException()
        {
        }

        public NullException(string name, Editor acCurEd, Transaction acTrans) : base(
            string.Format("Object selected could not continue: {0}", name))
        {
            //Get the current document utilities
            acCurEd.WriteMessage("Object returned a null value - Operation aborted");
            acTrans.Abort();
        }
    }
}