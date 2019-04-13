using System;
using System.Collections.Generic;
using System.Windows.Navigation;
using Autodesk.AutoCAD.BoundaryRepresentation;
using Autodesk.AutoCAD.Geometry;
using RabCab.Calculators;
using RabCab.Engine.Enumerators;
using RabCab.Settings;
using Exception = Autodesk.AutoCAD.BoundaryRepresentation.Exception;

namespace RabCab.Extensions
{
    public static class BrepExtensions
    {
        /// <summary>
        /// Todo
        /// </summary>
        /// <param name="comp1"></param>
        /// <param name="comp2"></param>
        /// <returns></returns>
        public static bool IsEqualTo(this Face comp1, Face comp2) =>
            (comp1 == comp2);

        /// <summary>
        /// Todo
        /// </summary>
        /// <param name="comp1"></param>
        /// <param name="comp2"></param>
        /// <returns></returns>
        public static bool IsEqualTo(this Vertex comp1, Vertex comp2) =>
            (comp1 == comp2);

        /// <summary>
        /// Todo
        /// </summary>
        /// <param name="comp1"></param>
        /// <param name="comp2"></param>
        /// <returns></returns>
        public static bool IsEqualTo(this Shell comp1, Shell comp2) =>
            (comp1 == comp2);

        /// <summary>
        /// Todo
        /// </summary>
        /// <param name="comp1"></param>
        /// <param name="comp2"></param>
        /// <returns></returns>
        public static bool IsEqualTo(this BoundaryLoop comp1, BoundaryLoop comp2) =>
            (comp1 == comp2);

        /// <summary>
        /// Todo
        /// </summary>
        /// <param name="comp1"></param>
        /// <param name="comp2"></param>
        /// <returns></returns>
        public static bool IsEqualTo(this Edge comp1, Edge comp2) =>
            (comp1 == comp2);

        /// <summary>
        /// Todo
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="loop"></param>
        /// <returns></returns>
        public static bool IsOnLoop(this Edge edge, BoundaryLoop loop)
        {
            try
            {
                foreach (var lEdge in loop.Edges)
                {
                    if (lEdge.IsEqualTo(edge))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        /// <summary>
        /// Todo
        /// </summary>
        /// <param name="loop"></param>
        /// <returns></returns>
        public static Enums.LoopKit GetLoopType(this BoundaryLoop loop)
        {
            if (!loop.IsLargestLoop())
                return Enums.LoopKit.Interior;

            if (!SettingsUser.PrioritizeRightAngles)
                return Enums.LoopKit.Exterior;

            try
            {
                foreach (var vtx in loop.Vertices)
                {
                    if (vtx.IsRightAngle3D())
                    {
                        return Enums.LoopKit.RightAngle;
                    }
                }
            }
            catch
            {
                return Enums.LoopKit.Error;
            }

            return Enums.LoopKit.Undetermined;

        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="vtx"></param>
        /// <returns></returns>
        public static bool IsRightAngle3D(this Vertex vtx)
        {
            List<Vector3d> vList = new List<Vector3d>();

            try
            {
                foreach (var edge in vtx.Edges)
                {
                    var vFrom = edge.GetVectorFrom(vtx.Point);
                    if (vFrom.Length.IsGreaterThanTol())
                    {
                        vList.Add(vFrom);
                    }
                }
            }
            catch
            {
                return false;
            }

            if (vList.Count != 3 
                || Math.Abs(vList[0].GetAngleTo(vList[1]) - 1.5707963267948966) >= SettingsInternal.TolVector 
                || Math.Abs(vList[0].GetAngleTo(vList[2]) - 1.5707963267948966) >= SettingsInternal.TolVector)
            {
                return false;
            }

            return (Math.Abs(vList[1].GetAngleTo(vList[2]) - 1.5707963267948966) < SettingsInternal.TolVector);
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="acEdge"></param>
        /// <param name="startPt"></param>
        /// <returns></returns>
        public static Vector3d GetVectorFrom(this Edge acEdge, Point3d startPt)
        {
            Vector3d vectorTo;

            try
            {
                using (var acCurve = acEdge.Curve)
                {
                    if (acCurve is ExternalCurve3d)
                    {
                        using (var exCurve = acCurve as ExternalCurve3d)
                        {
                            using (var natCurve = exCurve.NativeCurve)
                            {
                                if (!(natCurve is LinearEntity3d))
                                {
                                    if (!natCurve.IsClosed())
                                    {
                                        if (natCurve.StartPoint.DistanceTo(startPt).IsGreaterThanTol())
                                        {
                                            Point3d endPt = natCurve.EndPoint;

                                            if (endPt.DistanceTo(startPt).IsGreaterThanTol())
                                            {
                                                return new Vector3d();
                                            }
                                        }

                                        using (PointOnCurve3d pCurve = natCurve.GetClosestPointTo(startPt))
                                        {
                                            Vector3d deriv = pCurve.GetDerivative(1);

                                            using (PointOnCurve3d pDeriv = natCurve.GetClosestPointTo(startPt + deriv))
                                            {
                                                if (pDeriv.Point.IsEqualTo(startPt))
                                                {
                                                    deriv = deriv.Negate();
                                                }
                                            }

                                            vectorTo = deriv;
                                        }
                                    }
                                    else
                                    {
                                        vectorTo = new Vector3d();
                                    }
                                }
                                else if (natCurve.StartPoint.DistanceTo(startPt).IsGreaterThanTol())
                                {
                                    vectorTo = startPt.GetVectorTo(natCurve.EndPoint);
                                }
                                else if (natCurve.EndPoint.DistanceTo(startPt).IsGreaterThanTol())
                                {
                                    vectorTo = startPt.GetVectorTo(natCurve.StartPoint);
                                }
                                else
                                {
                                    vectorTo = new Vector3d();
                                }
                            }
                        }
                    }
                    else
                    {
                        vectorTo = new Vector3d();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                vectorTo = new Vector3d();
            }

            return vectorTo;
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="loop"></param>
        /// <returns></returns>
        public static bool IsLargestLoop(this BoundaryLoop loop)
        {
            var length = loop.GetLength();

            if (length == 0) return false;

            try
            {
                foreach (var fLoop in loop.Face.Loops)
                {                 
                    if (fLoop.IsEqualTo(loop)) continue;

                    double fLength = fLoop.GetLength();

                    if (fLength > length)
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        /// <summary>
        /// Todo
        /// </summary>
        /// <param name="loop"></param>
        /// <returns></returns>
        public static double GetLength(this BoundaryLoop loop)
        {
            try
            {
                double length = 0;

                foreach (var edge in loop.Edges)
                {
                    length += edge.GetLength();
                }

                return length;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 0;
            }
        }

        /// <summary>
        /// Todo
        /// </summary>
        /// <param name="acEdge"></param>
        /// <returns></returns>
        public static double GetLength(this Edge acEdge)
        {
            try
            {
                using (Curve3d acCurve = acEdge.Curve)
                {
                    using (Interval intv = acCurve.GetInterval())
                    {
                        return acCurve.GetLength(intv.LowerBound, intv.UpperBound, SettingsUser.TolPoint);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 0;
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="face"></param>
        /// <returns></returns>
        public static Matrix3d GetLayMatrix(this Face face)
        {
            Matrix3d matrix3d;
            double num;
            double num1;
            Vector3d yVec;
            Vector3d zVec;
            Matrix3d matrix3d1;
            Vector3d vector3d1;

            if (face.IsNull)
            {
                matrix3d = new Matrix3d();
                return matrix3d;
            }

            try
            {
                using (Surface surface = face.Surface)
                {
                    if (surface == null || !(surface is ExternalBoundedSurface))
                    {
                        matrix3d = new Matrix3d();                    
                    }
                    else
                    {
                        ExternalBoundedSurface externalBoundedSurface = surface as ExternalBoundedSurface;
                        Interval[] envelope = externalBoundedSurface.GetEnvelope();
                        double lowerBound = envelope[0].LowerBound;
                        double lowerBound1 = envelope[1].LowerBound;
                        Point2d point2d = new Point2d(lowerBound, lowerBound1);
                        using (PointOnSurface pointOnSurface = new PointOnSurface(externalBoundedSurface, point2d))
                        {
                            Vector3d uDerivative = pointOnSurface.GetNormal();
                            Point3d point = pointOnSurface.GetPoint();
                            num = (!externalBoundedSurface.IsClosedInU(CalcTol.CadTolerance) ? envelope[0].UpperBound : (envelope[0].LowerBound + envelope[0].UpperBound) / 2);
                            num1 = (!externalBoundedSurface.IsClosedInV(CalcTol.CadTolerance) ? envelope[1].UpperBound : (envelope[1].LowerBound + envelope[1].UpperBound) / 2);
                            Point3d point3d = pointOnSurface.GetPoint(new Point2d(num, lowerBound1));
                            Vector3d vectorTo = point.GetVectorTo(point3d);
                            Point3d point1 = pointOnSurface.GetPoint(new Point2d(lowerBound, num1));
                            Vector3d vectorTo1 = point.GetVectorTo(point1);
                            if (vectorTo.Length < SettingsUser.TolPoint)
                            {
                                if (vectorTo1.Length >= SettingsUser.TolPoint)
                                {
                                    vectorTo = vectorTo1;
                                    vectorTo1 = new Vector3d();
                                }
                                else
                                {
                                    matrix3d1 = new Matrix3d();
                                    matrix3d = matrix3d1;
                                    return matrix3d;
                                }
                            }
                            if (vectorTo.Length < vectorTo1.Length)
                            {
                                Vector3d vector3d2 = vectorTo;
                                vectorTo = vectorTo1;
                                vectorTo1 = vector3d2;
                            }
                            double angleTo = vectorTo.GetAngleTo(uDerivative);
                            if (angleTo < SettingsInternal.TolVector || Math.PI - angleTo < SettingsInternal.TolVector)
                            {
                                uDerivative = pointOnSurface.GetUDerivative(1, point2d);
                            }
                            Vector3d xVec = vectorTo.GetNormal();
                            if (uDerivative.Length >= SettingsUser.TolPoint)
                            {
                                zVec = uDerivative.GetNormal();
                                vector3d1 = uDerivative.CrossProduct(vectorTo);
                                yVec = vector3d1.GetNormal();
                                vector3d1 = xVec.CrossProduct(yVec);
                                zVec = vector3d1.GetNormal();
                            }
                            else if (vectorTo1.Length >= SettingsUser.TolPoint)
                            {
                                double angleTo1 = vectorTo.GetAngleTo(uDerivative);
                                if (angleTo1 < SettingsInternal.TolVector || Math.PI - angleTo1 < SettingsInternal.TolVector)
                                {
                                    matrix3d1 = new Matrix3d();
                                    matrix3d = matrix3d1;
                                    return matrix3d;
                                }
                                else
                                {
                                    yVec = vectorTo1.GetNormal();
                                    vector3d1 = vectorTo.CrossProduct(vectorTo1);
                                    zVec = vector3d1.GetNormal();
                                    vector3d1 = zVec.CrossProduct(xVec);
                                    yVec = vector3d1.GetNormal();
                                }
                            }
                            else
                            {
                                matrix3d1 = new Matrix3d();
                                matrix3d = matrix3d1;
                                return matrix3d;
                            }
                            matrix3d = Matrix3d.AlignCoordinateSystem(point, xVec, yVec, zVec, Point3d.Origin, Vector3d.XAxis, Vector3d.YAxis, Vector3d.ZAxis);
                        }
                    }
                }
            }
            catch
            {
                matrix3d1 = new Matrix3d();
                matrix3d = matrix3d1;
            }
            return matrix3d;
        }
    }
}
