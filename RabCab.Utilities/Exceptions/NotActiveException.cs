using System;
using Autodesk.AutoCAD.ApplicationServices.Core;
using AcAp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RabCab.Exceptions
{
    internal class NotActiveException : Exception
    {
        public NotActiveException()
        {
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(
                "\nRabCab is not activated! Please enter an activation key to continue using the plugin!");
        }
    }
}