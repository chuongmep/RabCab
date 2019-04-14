using Autodesk.AutoCAD.Geometry;
using RabCab.Calculators;

namespace RabCab.Extensions
{
    public static class Curve3dExtensions
    {
        public static Vector3d GetNormal(this Curve3d curve)
        {
            if (curve is ExternalCurve3d)
                using (var extCurve = curve as ExternalCurve3d)
                {
                    using (var natCurve = extCurve.NativeCurve)
                    {
                        return natCurve.GetNormal();
                    }
                }

            if (curve is LinearEntity3d) return new Vector3d();

            if (curve is CircularArc3d) return ((CircularArc3d) curve).Normal;

            if (curve is EllipticalArc3d) return ((EllipticalArc3d) curve).Normal;

            var samplePoints = curve.GetSamplePoints(4);
            var vectorTo = samplePoints[1].Point.GetVectorTo(samplePoints[0].Point);
            var v = samplePoints[1].Point.GetVectorTo(samplePoints[2].Point);
            if (!vectorTo.IsLessThanTol() && !v.IsLessThanTol() && !vectorTo.IsParallelTo(v, CalcTol.CadTolerance))
                return vectorTo.CrossProduct(v);

            return new Vector3d();
        }
    }
}