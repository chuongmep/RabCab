using System;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using RabCab.Calculators;
using RabCab.Extensions;

namespace RabCab.Entities.Linework
{
    internal class Axis
    {
        public Point3d End;
        public Point3d Start;

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public Axis(Point3d start, Point3d end)
        {
            Start = start;
            End = end;
        }

        /// <summary>
        ///     TODO
        /// </summary>
        public bool IsNull => Start.IsNull() || End.IsNull();

        /// <summary>
        ///     TODO
        /// </summary>
        public bool IsZero => Start.DistanceTo(End).IsLessThanTol();

        //TODO
        public double AngleX
        {
            get
            {
                var vectorTo = Start.GetVectorTo(End);
                return (vectorTo.Y >= 0.0 ? 1 : -1) * vectorTo.GetAngleTo(new Vector3d(1.0, 0.0, 0.0));
            }
        }

        /// <summary>
        ///     TODO
        /// </summary>
        public Matrix3d RotationToX =>
            Matrix3d.Rotation(-AngleX, new Vector3d(Start.X, Start.Y, 1.0), Start);

        /// <summary>
        ///     TODO
        /// </summary>
        public double Length =>
            !IsNull ? Start.DistanceTo(End) : 0.0;

        /// <summary>
        ///     TODO
        /// </summary>
        public Point3d MidPoint
        {
            get
            {
                if (!IsNull)
                    return (Start + End.GetAsVector()) / 2.0;
                return new Point3d(double.NaN, double.NaN, double.NaN);
            }
        }

        /// <summary>
        ///     TODO
        /// </summary>
        public Plane MidPlane
        {
            get
            {
                var midPoint = MidPoint;
                return new Plane(midPoint, midPoint.GetVectorTo(End));
            }
        }

        /// <summary>
        ///     TODO
        /// </summary>
        public void Reverse()
        {
            var tempPt = Start;
            Start = End;
            End = tempPt;
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="transMat"></param>
        public void TransformBy(Matrix3d transMat)
        {
            try
            {
                Start.TransformBy(transMat);
                End.TransformBy(transMat);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <returns></returns>
        public Vector3d ToVector()
        {
            return Start.GetVectorTo(End);
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <returns></returns>
        public Line ToLine()
        {
            return new Line(Start, End);
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        public bool IsParallelTo(Axis axis)
        {
            return Start.GetVectorTo(End).IsParallelTo(axis.Start.GetVectorTo(axis.End), CalcTol.CadTolerance);
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="tol"></param>
        /// <returns></returns>
        public bool IsParallelTo(Axis axis, Tolerance tol)
        {
            return Start.GetVectorTo(End).IsParallelTo(axis.Start.GetVectorTo(axis.End), tol);
        }
    }
}