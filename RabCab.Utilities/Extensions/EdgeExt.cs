using System;
using Autodesk.AutoCAD.BoundaryRepresentation;
using Autodesk.AutoCAD.Geometry;
using RabCab.Agents;
using RabCab.Entities.Linework;
using Exception = System.Exception;

namespace RabCab.Extensions
{
    internal class EdgeExt
    {
        public Axis Eaxis;
        public bool IsClosed;
        public bool IsLinear;
        public double Length;
        public Vector3d Normal;
        public bool OnLoop;
        public Vector3d Tangent;

        public EdgeExt()
        {
            //Empty Constructor
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="vtx"></param>
        /// <param name="owner"></param>
        public EdgeExt(Edge edge, Vertex vtx, BoundaryLoop owner)
        {
            OnLoop = edge.IsOnLoop(owner);

            try
            {
                using (var curve = edge.Curve)
                {
                    if (!(curve is ExternalCurve3d)) return;

                    using (var extCurve = curve as ExternalCurve3d)
                    {
                        using (var natCurve = extCurve.NativeCurve)
                        {
                            IsClosed = natCurve.IsClosed();

                            if (IsClosed)
                            {
                                using (var inv = natCurve.GetInterval())
                                {
                                    Eaxis = new Axis(natCurve.StartPoint,
                                        natCurve.EvaluatePoint(inv.LowerBound + inv.UpperBound) / 2);
                                }
                            }
                            else
                            {
                                if (natCurve.StartPoint.IsEqualTo(vtx.Point))
                                    Eaxis = new Axis(natCurve.EndPoint, natCurve.StartPoint);
                                else
                                    Eaxis = new Axis(natCurve.StartPoint, natCurve.EndPoint);
                            }

                            IsLinear = natCurve is LinearEntity3d;

                            if (IsLinear) Normal = natCurve.GetNormal();

                            Tangent = GetTangent(natCurve, Eaxis);

                            Length = Eaxis.Length;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                MailAgent.Report(e.Message);
            }
        }

        public bool IsNull => Length == 0;

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="nat"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        private Vector3d GetTangent(Curve3d nat, Axis axis)
        {
            var vec = new Vector3d();

            try
            {
                if (nat is LinearEntity3d || nat.IsClosed())
                {
                    vec = axis.ToVector();
                }
                else if (nat.StartPoint == axis.Start || nat.EndPoint == axis.Start)
                {
                    using (var curved = nat.GetClosestPointTo(axis.Start))
                    {
                        vec = curved.GetDerivative(1).GetNormal() * axis.Length;
                    }

                    using (var curved2 = nat.GetClosestPointTo(axis.Start + vec))
                    {
                        if (curved2.Point.IsEqualTo(axis.Start)) vec = vec.Negate();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                MailAgent.Report(e.Message);
            }

            return vec;
        }
    }
}