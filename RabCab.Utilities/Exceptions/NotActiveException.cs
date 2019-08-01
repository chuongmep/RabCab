using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcAp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RabCab.Exceptions
{
    class NotActiveException : Exception
    {
        public NotActiveException()
        {
            AcAp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nRabCab is not activated! Please enter an activation key to continue using the plugin!");
        }
    }
}