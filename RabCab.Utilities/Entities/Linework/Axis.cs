using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using RabCab.Calculators;
using RabCab.Extensions;

namespace RabCab.Entities.Linework
{
    class Axis
    {
        public Point3d Start;
        public Point3d End;

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public Axis(Point3d start, Point3d end)
        {
            Start = start;
            End = end;
        }

        /// <summary>
        /// TODO
        /// </summary>
        public bool IsNull => Start.IsNull() || End.IsNull();

        /// <summary>
        /// TODO
        /// </summary>
        public bool IsZero => Start.DistanceTo(End).IsLessThanTol();

        /// <summary>
        /// TODO
        /// </summary>
        public void Reverse()
        {
            Point3d tempPt = Start;
            Start = End;
            End = tempPt;
        }

        /// <summary>
        /// TODO
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
        /// TODO
        /// </summary>
        /// <returns></returns>
        public Vector3d ToVector() =>
            this.Start.GetVectorTo(this.End);

        /// <summary>
        /// TODO
        /// </summary>
        /// <returns></returns>
        public Line ToLine() =>
            new Line(this.Start, this.End);

        //TODO
        public double AngleX
        {
            get
            {
                Vector3d vectorTo = this.Start.GetVectorTo(this.End);
                return (((vectorTo.Y >= 0.0) ? ((double)1) : ((double)(-1))) * vectorTo.GetAngleTo(new Vector3d(1.0, 0.0, 0.0)));
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        public Matrix3d RotationToX =>
            Matrix3d.Rotation(-this.AngleX, new Vector3d(this.Start.X, this.Start.Y, 1.0), this.Start);

        /// <summary>
        /// TODO
        /// </summary>
        public double Length =>
            (!this.IsNull ? this.Start.DistanceTo(this.End) : 0.0);

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        public bool IsParallelTo(Axis axis) =>
            this.Start.GetVectorTo(this.End).IsParallelTo(axis.Start.GetVectorTo(axis.End), CalcTol.CadTolerance);

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="tol"></param>
        /// <returns></returns>
        public bool IsParallelTo(Axis axis, Tolerance tol) =>
            this.Start.GetVectorTo(this.End).IsParallelTo(axis.Start.GetVectorTo(axis.End), tol);

        /// <summary>
        /// TODO
        /// </summary>
        public Point3d MidPoint
        {
            get
            {
                if (!this.IsNull)
                    return (this.Start + this.End.GetAsVector()) / 2.0;
                else
                    return new Point3d(double.NaN, double.NaN, double.NaN);
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        public Plane MidPlane
        {
            get
            {
                Point3d midPoint = this.MidPoint;
                return new Plane(midPoint, midPoint.GetVectorTo(this.End));
            }
        }

        

    }
}
