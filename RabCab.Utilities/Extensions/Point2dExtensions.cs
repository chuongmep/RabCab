// -----------------------------------------------------------------------------------
//     <copyright file="Point2dExtensions.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>03/08/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using System;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace RabCab.Extensions
{
    /// <summary>
    ///     Provides extension methods for the Point2d type.
    /// </summary>
    public static class Point2DExtensions
    {
        /// <summary>
        ///     Converts a 2d point into a 3d point with Z coodinate equal to 0.
        /// </summary>
        /// <param name="pt">The instance to which the method applies.</param>
        /// <returns>The corresponding 3d point.</returns>
        public static Point3d Convert3D(this Point2d pt)
        {
            return new Point3d(pt.X, pt.Y, 0.0);
        }

        /// <summary>
        ///     Converts a 2d point into a 3d point according to the specified plane.
        /// </summary>
        /// <param name="pt">The instance to which the method applies.</param>
        /// <param name="plane">The plane which the point lies on.</param>
        /// <returns>The corresponding 3d point</returns>
        public static Point3d Convert3D(this Point2d pt, Plane plane)
        {
            return new Point3d(pt.X, pt.Y, 0.0).TransformBy(Matrix3d.PlaneToWorld(plane));
        }

        /// <summary>
        ///     Converts a 2d point into a 3d point according to the plane defined by
        ///     the specified normal vector and elevation.
        /// </summary>
        /// <param name="pt">The instance to which the method applies.</param>
        /// <param name="normal">The normal vector of the plane which the point lies on.</param>
        /// <param name="elevation">The elevation of the plane which the point lies on.</param>
        /// <returns>The corresponding 3d point</returns>
        public static Point3d Convert3D(this Point2d pt, Vector3d normal, double elevation)
        {
            return new Point3d(pt.X, pt.Y, elevation).TransformBy(Matrix3d.PlaneToWorld(normal));
        }

        /// <summary>
        ///     Projects the point on the WCS XY plane.
        /// </summary>
        /// <param name="pt">The point 2d to project.</param>
        /// <param name="normal">The normal vector of the entity which owns the point 2d.</param>
        /// <returns>The transformed Point2d.</returns>
        public static Point2d Flatten(this Point2d pt, Vector3d normal)
        {
            return new Point3d(pt.X, pt.Y, 0.0)
                .TransformBy(Matrix3d.PlaneToWorld(normal))
                .Convert2d(new Plane());
        }


        /// <summary>
        ///     Method to get the midpoint between the current point and an input point
        /// </summary>
        /// <param name="pt1"></param>
        /// <param name="pt2"></param>
        /// <returns></returns>
        public static Point2d GetMidPoint(this Point2d pt1, Point2d pt2)
        {
            var vector = pt1.GetVectorTo(pt2);
            var halfwayPoint = pt1 + vector * 0.5;
            return halfwayPoint;
        }

        /// <summary>
        ///     Gets a value indicating whether the specified point is on the segment defined by two points.
        /// </summary>
        /// <param name="pt">The instance to which the method applies.</param>
        /// <param name="p1">The segment start point.</param>
        /// <param name="p2">The segment end point.</param>
        /// <returns>true if the point is on the segment; otherwise, false.</returns>
        public static bool IsBetween(this Point2d pt, Point2d p1, Point2d p2)
        {
            return p1.GetVectorTo(pt).GetNormal().Equals(pt.GetVectorTo(p2).GetNormal());
        }

        /// <summary>
        ///     Gets a value indicating whether the specified point is on the segment defined by two points.
        /// </summary>
        /// <param name="pt">The instance to which the method applies.</param>
        /// <param name="p1">The segment start point.</param>
        /// <param name="p2">The segment end point.</param>
        /// <param name="tol">The tolerance used in comparisons.</param>
        /// <returns>true if the point is on the segment; otherwise, false.</returns>
        public static bool IsBetween(this Point2d pt, Point2d p1, Point2d p2, Tolerance tol)
        {
            return p1.GetVectorTo(pt).GetNormal(tol).Equals(pt.GetVectorTo(p2).GetNormal(tol));
        }

        /// <summary>
        ///     Gets a value indicating whether the specified point is inside the extents.
        /// </summary>
        /// <param name="pt">The instance to which the method applies.</param>
        /// <param name="extents">The extents 2d supposed to contain the point.</param>
        /// <returns>true if the point is inside the extents; otherwise, false.</returns>
        public static bool IsInside(this Point2d pt, Extents2d extents)
        {
            return
                pt.X >= extents.MinPoint.X &&
                pt.Y >= extents.MinPoint.Y &&
                pt.X <= extents.MaxPoint.X &&
                pt.Y <= extents.MaxPoint.Y;
        }

        /// <summary>
        ///     Defines a point with polar coordinates from an origin point.
        /// </summary>
        /// <param name="org">The instance to which the method applies.</param>
        /// <param name="angle">The angle about the X axis.</param>
        /// <param name="distance">The distance from the origin</param>
        /// <returns>The new 2d point.</returns>
        public static Point2d Polar(this Point2d org, double angle, double distance)
        {
            return new Point2d(
                org.X + distance * Math.Cos(angle),
                org.Y + distance * Math.Sin(angle));
        }
    }
}