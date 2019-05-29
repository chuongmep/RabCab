using System;
using System.Windows.Forms;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace RabCab.Entities.Controls
{
    internal class AcadMainWindow : IWin32Window
    {
        public IntPtr Handle => Application.MainWindow.Handle;
    }
}