using System;
using Autodesk.AutoCAD.ApplicationServices.Core;

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