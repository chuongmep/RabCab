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
    }
}
