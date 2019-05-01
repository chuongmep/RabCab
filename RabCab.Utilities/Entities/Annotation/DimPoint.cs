using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace RabCab.Entities.Annotation
{
    internal class DimPoint
    {
        public RotatedDimension Dim1 { get; set; }

        public int Dim1PointIndex { get; set; }

        public RotatedDimension Dim2 { get; set; }

        public int Dim2PointIndex { get; set; }

        public Point3d DimLinePoint { get; set; }

        public bool IsLast { get; set; }

        public DimPoint()
        {
            Dim1 = null;
            Dim2 = null;
            Dim1PointIndex = 0;
            Dim1PointIndex = 0;
            IsLast = true;
            DimLinePoint = new Point3d();
        }
    }
}