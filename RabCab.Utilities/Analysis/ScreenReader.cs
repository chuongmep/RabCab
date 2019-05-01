using System;
using System.Drawing;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace RabCab.Analysis
{
    public static class ScreenReader
    {
        public static double GetSreenSize()
        {
            var acCurEd = Application.DocumentManager.MdiActiveDocument.Editor;
            var systemVariable = (Point2d)Application.GetSystemVariable("SCREENSIZE");
            var point = new Point(0, 0);
            var point1 = new Point((int)systemVariable.X, (int)systemVariable.Y);
            var world = acCurEd.PointToWorld(point, 0);
            var point3d = acCurEd.PointToWorld(point1, 0);
            var num = Math.Sqrt(Math.Pow(Math.Abs(point.X - point1.X), 2) + Math.Pow(Math.Abs(point.Y - point1.Y), 2));
            var num1 = world.DistanceTo(point3d);
            var num2 = Math.Abs(point.Y - point1.Y) * (num1 / num);
            return num2;
        }
    }
}