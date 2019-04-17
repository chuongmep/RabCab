// -----------------------------------------------------------------------------------
//     <copyright file="Triangle3d.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>03/08/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using System;
using Autodesk.AutoCAD.Geometry;
using RabCab.Extensions;

namespace RabCab.Entities.Shapes
{
    /// <summary>
    ///     Represents a triangle in the 3d space. It can be viewed as a structure consisting of three Point3d.
    /// </summary>
    public class Triangle3D : Triangle<Point3d>
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of Triangle3d; that is empty.
        /// </summary>
        public Triangle3D()
        {
        }


        /// <summary>
        ///     Initializes a new instance of Triangle3d that contains elements copied from the specified array.
        /// </summary>
        /// <param name="pts">The Point3d array whose elements are copied to the new Triangle3d.</param>
        public Triangle3D(Point3d[] pts) : base(pts)
        {
        }

        /// <summary>
        ///     Initializes a new instance of Triangle3d that contains the specified elements.
        /// </summary>
        /// <param name="a">The first vertex of the new Triangle3d (origin).</param>
        /// <param name="b">The second vertex of the new Triangle3d (2nd vertex).</param>
        /// <param name="c">The third vertex of the new Triangle3d (3rd vertex).</param>
        public Triangle3D(Point3d a, Point3d b, Point3d c) : base(a, b, c)
        {
        }

        /// <summary>
        ///     Initializes a new instance of Triangle3d according to an origin and two vectors.
        /// </summary>
        /// <param name="org">The origin of the Triangle3d (1st vertex).</param>
        /// <param name="v1">The vector from origin to the second vertex.</param>
        /// <param name="v2">The vector from origin to the third vertex.</param>
        public Triangle3D(Point3d org, Vector3d v1, Vector3d v2)
        {
            Pts[0] = Pt0 = org;
            Pts[0] = Pt1 = org + v1;
            Pts[0] = Pt2 = org + v2;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the triangle area.
        /// </summary>
        public double Area =>
            Math.Abs(
                ((Pt1.X - Pt0.X) * (Pt2.Y - Pt0.Y) -
                 (Pt2.X - Pt0.X) * (Pt1.Y - Pt0.Y)) / 2.0);

        /// <summary>
        ///     Gets the triangle centroid.
        /// </summary>
        public Point3d Centroid => (Pt0 + Pt1.GetAsVector() + Pt2.GetAsVector()) / 3.0;

        /// <summary>
        ///     Gets the circumscribed circle.
        /// </summary>
        public CircularArc3d CircumscribedCircle
        {
            get
            {
                var ca2D = Convert2D().CircumscribedCircle;
                if (ca2D == null)
                    return null;
                return new CircularArc3d(ca2D.Center.Convert3D(GetPlane()), Normal, ca2D.Radius);
            }
        }

        /// <summary>
        ///     Gets the triangle plane elevation.
        /// </summary>
        public double Elevation => Pt0.TransformBy(Matrix3d.WorldToPlane(Normal)).Z;

        /// <summary>
        ///     Gets the unit vector of the triangle plane greatest slope.
        /// </summary>
        public Vector3d GreatestSlope
        {
            get
            {
                var norm = Normal;
                if (norm.IsParallelTo(Vector3d.ZAxis))
                    return new Vector3d(0.0, 0.0, 0.0);
                if (norm.Z == 0.0)
                    return Vector3d.ZAxis.Negate();
                return new Vector3d(-norm.Y, norm.X, 0.0).CrossProduct(norm).GetNormal();
            }
        }

        /// <summary>
        ///     Gets the unit horizontal vector of the triangle plane.
        /// </summary>
        public Vector3d Horizontal
        {
            get
            {
                var norm = Normal;
                if (norm.IsParallelTo(Vector3d.ZAxis))
                    return Vector3d.XAxis;
                return new Vector3d(-norm.Y, norm.X, 0.0).GetNormal();
            }
        }

        /// <summary>
        ///     Gets the inscribed circle.
        /// </summary>
        public CircularArc3d InscribedCircle
        {
            get
            {
                var ca2D = Convert2D().InscribedCircle;
                if (ca2D == null)
                    return null;
                return new CircularArc3d(ca2D.Center.Convert3D(GetPlane()), Normal, ca2D.Radius);
            }
        }

        /// <summary>
        ///     Gets a value indicating whether the triangle plane is horizontal.
        /// </summary>
        public bool IsHorizontal => Pt0.Z == Pt1.Z && Pt0.Z == Pt2.Z;

        /// <summary>
        ///     Gets the normal vector of the triangle plane.
        /// </summary>
        public Vector3d Normal => (Pt1 - Pt0).CrossProduct(Pt2 - Pt0).GetNormal();

        /// <summary>
        ///     Gets the percent slope of the triangle plane.
        /// </summary>
        public double SlopePerCent
        {
            get
            {
                var norm = Normal;
                if (norm.Z == 0.0)
                    return double.PositiveInfinity;
                return Math.Abs(100.0 * Math.Sqrt(Math.Pow(norm.X, 2.0) + Math.Pow(norm.Y, 2.0)) / norm.Z);
            }
        }

        /// <summary>
        ///     Gets the triangle coordinates system
        ///     (origin = centroid, X axis = horizontal vector, Y axis = negated geatest slope vector).
        /// </summary>
        public Matrix3d SlopeUcs
        {
            get
            {
                var origin = Centroid;
                var zaxis = Normal;
                var xaxis = Horizontal;
                var yaxis = zaxis.CrossProduct(xaxis).GetNormal();
                return new Matrix3d(new[]
                {
                    xaxis.X, yaxis.X, zaxis.X, origin.X,
                    xaxis.Y, yaxis.Y, zaxis.Y, origin.Y,
                    xaxis.Z, yaxis.Z, zaxis.Z, origin.Z,
                    0.0, 0.0, 0.0, 1.0
                });
            }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Converts a Triangle3d into a Triangle2d according to the Triangle3d plane.
        /// </summary>
        /// <returns>The resulting Triangle2d.</returns>
        public Triangle2D Convert2D()
        {
            var plane = GetPlane();
            return new Triangle2D(
                Array.ConvertAll(Pts, x => x.Convert2d(plane)));
        }

        /// <summary>
        ///     Projects a Triangle3d on the WCS XY plane.
        /// </summary>
        /// <returns>The resulting Triangle2d.</returns>
        public Triangle2D Flatten()
        {
            return new Triangle2D(
                new Point2d(this[0].X, this[0].Y),
                new Point2d(this[1].X, this[1].Y),
                new Point2d(this[2].X, this[2].Y));
        }

        /// <summary>
        ///     Gets the angle between the two segments at specified vertex.
        /// </summary>
        /// .
        /// <param name="index">The vertex index.</param>
        /// <returns>The angle expressed in radians.</returns>
        public double GetAngleAt(int index)
        {
            return this[index].GetVectorTo(this[(index + 1) % 3]).GetAngleTo(
                this[index].GetVectorTo(this[(index + 2) % 3]));
        }

        /// <summary>
        ///     Gets the bounded plane defined by the triangle.
        /// </summary>
        /// <returns>The bouned plane.</returns>
        public BoundedPlane GetBoundedPlane()
        {
            return new BoundedPlane(this[0], this[1], this[2]);
        }

        /// <summary>
        ///     Gets the unbounded plane defined by the triangle.
        /// </summary>
        /// <returns>The unbouned plane.</returns>
        public Plane GetPlane()
        {
            var normal = Normal;
            var origin =
                new Point3d(0.0, 0.0, Elevation).TransformBy(Matrix3d.PlaneToWorld(normal));
            return new Plane(origin, normal);
        }

        /// <summary>
        ///     Gets the segment at specified index.
        /// </summary>
        /// <param name="index">The segment index.</param>
        /// <returns>The segment 3d</returns>
        /// <exception cref="IndexOutOfRangeException">
        ///     IndexOutOfRangeException is throw if index is less than 0 or more than 2.
        /// </exception>
        public LineSegment3d GetSegmentAt(int index)
        {
            if (index > 2)
                throw new IndexOutOfRangeException("Index out of range");
            return new LineSegment3d(this[index], this[(index + 1) % 3]);
        }

        /// <summary>
        ///     Checks if the distance between every respective Point3d in both Triangle3d is less than or equal to the
        ///     Tolerance.Global.EqualPoint value.
        /// </summary>
        /// <param name="t3D">The triangle3d to compare.</param>
        /// <returns>true if the condition is met; otherwise, false.</returns>
        public bool IsEqualTo(Triangle3D t3D)
        {
            return IsEqualTo(t3D, Tolerance.Global);
        }

        /// <summary>
        ///     Checks if the distance between every respective Point3d in both Triangle3d is less than or equal to the
        ///     Tolerance.EqualPoint value of the specified tolerance.
        /// </summary>
        /// <param name="t3D">The triangle3d to compare.</param>
        /// <param name="tol">The tolerance used in points comparisons.</param>
        /// <returns>true if the condition is met; otherwise, false.</returns>
        public bool IsEqualTo(Triangle3D t3D, Tolerance tol)
        {
            return t3D[0].IsEqualTo(Pt0, tol) && t3D[1].IsEqualTo(Pt1, tol) && t3D[2].IsEqualTo(Pt2, tol);
        }

        /// <summary>
        ///     Gets a value indicating whether the specified point is strictly inside the triangle.
        /// </summary>
        /// <param name="pt">The point to be evaluated.</param>
        /// <returns>true if the point is inside; otherwise, false.</returns>
        public bool IsPointInside(Point3d pt)
        {
            var tol = new Tolerance(1e-9, 1e-9);
            var v1 = pt.GetVectorTo(Pt0).CrossProduct(pt.GetVectorTo(Pt1)).GetNormal();
            var v2 = pt.GetVectorTo(Pt1).CrossProduct(pt.GetVectorTo(Pt2)).GetNormal();
            var v3 = pt.GetVectorTo(Pt2).CrossProduct(pt.GetVectorTo(Pt0)).GetNormal();
            return v1.IsEqualTo(v2, tol) && v2.IsEqualTo(v3, tol);
        }

        /// <summary>
        ///     Gets a value indicating whether the specified point is on a triangle segment.
        /// </summary>
        /// <param name="pt">The point to be evaluated.</param>
        /// <returns>true if the point is on a segment; otherwise, false.</returns>
        public bool IsPointOn(Point3d pt)
        {
            var tol = new Tolerance(1e-9, 1e-9);
            var v0 = new Vector3d(0.0, 0.0, 0.0);
            var v1 = pt.GetVectorTo(Pt0).CrossProduct(pt.GetVectorTo(Pt1));
            var v2 = pt.GetVectorTo(Pt1).CrossProduct(pt.GetVectorTo(Pt2));
            var v3 = pt.GetVectorTo(Pt2).CrossProduct(pt.GetVectorTo(Pt0));
            return v1.IsEqualTo(v0, tol) || v2.IsEqualTo(v0, tol) || v3.IsEqualTo(v0, tol);
        }

        /// <summary>
        ///     Sets the elements of the triangle using an origin and two vectors.
        /// </summary>
        /// <param name="org">The origin of the Triangle3d (1st vertex).</param>
        /// <param name="v1">The vector from origin to the second vertex.</param>
        /// <param name="v2">The vector from origin to the third vertex.</param>
        public void Set(Point3d org, Vector3d v1, Vector3d v2)
        {
            Pt0 = org;
            Pt1 = org + v1;
            Pt2 = org + v2;
            Pts = new Point3d[3] {Pt0, Pt1, Pt2};
        }

        /// <summary>
        ///     Transforms a Triangle3d with a transformation matrix
        /// </summary>
        /// <param name="mat">The 3d transformation matrix.</param>
        /// <returns>The new Triangle3d.</returns>
        public Triangle3D Transformby(Matrix3d mat)
        {
            return new Triangle3D(Array.ConvertAll(
                Pts, p => p.TransformBy(mat)));
        }

        #endregion
    }
}