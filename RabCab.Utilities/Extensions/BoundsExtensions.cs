using System;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace RabCab.Extensions
{
    public static class BoundsExtensions
    {
        public static double GetLengthAcross(this Extents3d? extents, Point3d pt1, Point3d pt2)
        {
            double distanceAcross = 0;

            if (extents == null) return distanceAcross;

            var minPt = extents.Value.MinPoint;
            var maxPt = extents.Value.MaxPoint;

            var minX = minPt.X;
            var minY = minPt.Y;

            var maxX = maxPt.X;
            var maxY = maxPt.Y;

            var botLeft = new Point2d(minX, minY);
            var topLeft = new Point2d(minX, maxY);
            var topRight = new Point2d(maxX, maxY);
            var botRight = new Point2d(maxX, minY);
            var center = minPt.Flatten().GetMidPoint(maxPt.Flatten());

            //Create rectangle lines from the extents
            var pline = new Polyline();
            pline.AddVertexAt(0, botLeft, 0, 0, 0);
            pline.AddVertexAt(0, topLeft, 0, 0, 0);
            pline.AddVertexAt(0, topRight, 0, 0, 0);
            pline.AddVertexAt(0, botRight, 0, 0, 0);
            pline.Closed = true;

            var lineStart = new Point3d(pt1.X, pt1.Y, 0);
            var lineEnd = new Point3d(pt2.X, pt2.Y, 0);
            var line = new Line(lineStart, lineEnd);

            pline.TransformBy(Matrix3d.Displacement(center.GetVectorTo(Point3d.Origin)));
            line.TransformBy(Matrix3d.Displacement(lineStart.GetMidPoint(lineEnd).GetVectorTo(Point3d.Origin)));

            var ixPoints = new Point3dCollection();
            line.IntersectWith(pline, Intersect.ExtendThis, ixPoints, IntPtr.Zero, IntPtr.Zero);

            if (ixPoints.Count == 2)
                distanceAcross = ixPoints[0].DistanceTo(ixPoints[1]);
            else
                distanceAcross = Math.Abs(maxX - minX);

            pline.Dispose();
            line.Dispose();

            return distanceAcross;
        }
    }
}