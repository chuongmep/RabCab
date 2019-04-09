﻿using System;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using AcRx = Autodesk.AutoCAD.Runtime;

namespace RabCab.Utilities.Extensions
{
    /// <summary>
    ///     Provides extension methods for the Point3d type.
    /// </summary>
    public static class Point3DExtensions
    {
        /// <summary>
        ///     Method to get the midpoint between the current point and an input point
        /// </summary>
        /// <param name="pt1"></param>
        /// <param name="pt2"></param>
        /// <returns></returns>
        public static Point3d GetMidPoint(this Point3d pt1, Point3d pt2)
        {
            var vector = pt1.GetVectorTo(pt2);
            var halfwayPoint = pt1 + vector*0.5;
            return halfwayPoint;
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
            var halfwayPoint = pt1 + vector*0.5;
            return halfwayPoint;
        }

        /// <summary>
        ///     Converts a 3d point into a 2d point (projection on XY plane).
        /// </summary>
        /// <param name="pt">The instance to which the method applies.</param>
        /// <returns>The corresponding 2d point.</returns>
        public static Point2d Convert2d(this Point3d pt)
        {
            return new Point2d(pt.X, pt.Y);
        }

        /// <summary>
        ///     Projects the point on the WCS XY plane.
        /// </summary>
        /// <param name="pt">The point to be projected.</param>
        /// <returns>The projected point</returns>
        public static Point3d Flatten(this Point3d pt)
        {
            return new Point3d(pt.X, pt.Y, 0.0);
        }

        /// <summary>
        ///     Gets a value indicating whether the specified point is on the segment defined by two points.
        /// </summary>
        /// <param name="pt">The instance to which the method applies.</param>
        /// <param name="p1">The segment start point.</param>
        /// <param name="p2">The segment end point.</param>
        /// <returns>true if the point is on the segment; otherwise, false.</returns>
        public static bool IsBetween(this Point3d pt, Point3d p1, Point3d p2)
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
        public static bool IsBetween(this Point3d pt, Point3d p1, Point3d p2, Tolerance tol)
        {
            return p1.GetVectorTo(pt).GetNormal(tol).Equals(pt.GetVectorTo(p2).GetNormal(tol));
        }

        /// <summary>
        ///     Gets a value indicating whether the specified point is inside the extents.
        /// </summary>
        /// <param name="pt">The instance to which the method applies.</param>
        /// <param name="extents">The extents 3d supposed to contain the point.</param>
        /// <returns>true if the point is inside the extents; otherwise, false.</returns>
        public static bool IsInside(this Point3d pt, Extents3d extents)
        {
            return
                pt.X >= extents.MinPoint.X &&
                pt.Y >= extents.MinPoint.Y &&
                pt.Z >= extents.MinPoint.Z &&
                pt.X <= extents.MaxPoint.X &&
                pt.Y <= extents.MaxPoint.Y &&
                pt.Z <= extents.MaxPoint.Z;
        }

        /// <summary>
        ///     Defines a point with polar coordinates from an origin point.
        /// </summary>
        /// <param name="org">The instance to which the method applies.</param>
        /// <param name="angle">The angle about the X axis.</param>
        /// <param name="distance">The distance from the origin</param>
        /// <returns>The new 3d point.</returns>
        public static Point3d Polar(this Point3d org, double angle, double distance)
        {
            return new Point3d(
                org.X + distance*Math.Cos(angle),
                org.Y + distance*Math.Sin(angle),
                org.Z);
        }

        /// <summary>
        ///     Transforms a point from a coordinate system to another one in the current editor.
        /// </summary>
        /// <param name="pt">The instance to which the method applies.</param>
        /// <param name="from">The origin coordinate system flag.</param>
        /// <param name="to">The target coordinate system flag.</param>
        /// <returns>The corresponding 3d point.</returns>
        /// <exception cref="Autodesk.AutoCAD.Runtime.Exception">
        ///     eInvalidInput is thrown if 3 (CoordSystem.PSDCS) is used with other than 2 (CoordSystem.DCS).
        /// </exception>
        public static Point3d Trans(this Point3d pt, int from, int to)
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            return pt.Trans(ed, (CoordSystem) from, (CoordSystem) to);
        }

        /// <summary>
        ///     Transforms a point from a coordinate system to another one in the specified editor.
        /// </summary>
        /// <param name="pt">The instance to which the method applies.</param>
        /// <param name="ed">The current editor.</param>
        /// <param name="from">The origin coordinate system flag.</param>
        /// <param name="to">The target coordinate system flag.</param>
        /// <returns>The corresponding 3d point.</returns>
        /// <exception cref="Autodesk.AutoCAD.Runtime.Exception">
        ///     eInvalidInput is thrown if 3 (CoordSystem.PSDCS) is used with other than 2 (CoordSystem.DCS).
        /// </exception>
        public static Point3d Trans(this Point3d pt, Editor ed, int from, int to)
        {
            return pt.Trans(ed, (CoordSystem) from, (CoordSystem) to);
        }

        /// <summary>
        ///     Transforms a point from a coordinate system to another one in the current editor.
        /// </summary>
        /// <param name="pt">The instance to which the method applies.</param>
        /// <param name="from">The origin coordinate system.</param>
        /// <param name="to">The target coordinate system.</param>
        /// <returns>The corresponding 3d point.</returns>
        /// <exception cref="Autodesk.AutoCAD.Runtime.Exception">
        ///     eInvalidInput is thrown if CoordSystem.PSDCS is used with other than CoordSystem.DCS.
        /// </exception>
        public static Point3d Trans(this Point3d pt, CoordSystem from, CoordSystem to)
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            return pt.Trans(ed, from, to);
        }

        /// <summary>
        ///     Transforms a point from a coordinate system to another one in the specified editor.
        /// </summary>
        /// <param name="pt">The instance to which the method applies.</param>
        /// <param name="ed">An instance of the Editor to which the method applies.</param>
        /// <param name="from">The origin coordinate system.</param>
        /// <param name="to">The target coordinate system.</param>
        /// <returns>The corresponding 3d point.</returns>
        /// <exception cref="Autodesk.AutoCAD.Runtime.Exception">
        ///     eInvalidInput is thrown if CoordSystem.PSDCS is used with other than CoordSystem.DCS.
        /// </exception>
        public static Point3d Trans(this Point3d pt, Editor ed, CoordSystem from, CoordSystem to)
        {
            var mat = new Matrix3d();
            switch (from)
            {
                case CoordSystem.WCS:
                    switch (to)
                    {
                        case CoordSystem.UCS:
                            mat = ed.WCS2UCS();
                            break;
                        case CoordSystem.DCS:
                            mat = ed.WCS2DCS();
                            break;
                        case CoordSystem.PSDCS:
                            throw new AcRx.Exception(
                                AcRx.ErrorStatus.InvalidInput,
                                "To be used only with DCS");
                        default:
                            mat = Matrix3d.Identity;
                            break;
                    }

                    break;
                case CoordSystem.UCS:
                    switch (to)
                    {
                        case CoordSystem.WCS:
                            mat = ed.UCS2WCS();
                            break;
                        case CoordSystem.DCS:
                            mat = ed.UCS2WCS()*ed.WCS2DCS();
                            break;
                        case CoordSystem.PSDCS:
                            throw new AcRx.Exception(
                                AcRx.ErrorStatus.InvalidInput,
                                "To be used only with DCS");
                        default:
                            mat = Matrix3d.Identity;
                            break;
                    }

                    break;
                case CoordSystem.DCS:
                    switch (to)
                    {
                        case CoordSystem.WCS:
                            mat = ed.DCS2WCS();
                            break;
                        case CoordSystem.UCS:
                            mat = ed.DCS2WCS()*ed.WCS2UCS();
                            break;
                        case CoordSystem.PSDCS:
                            mat = ed.DCS2PSDCS();
                            break;
                        default:
                            mat = Matrix3d.Identity;
                            break;
                    }

                    break;
                case CoordSystem.PSDCS:
                    switch (to)
                    {
                        case CoordSystem.WCS:
                            throw new AcRx.Exception(
                                AcRx.ErrorStatus.InvalidInput,
                                "To be used only with DCS");
                        case CoordSystem.UCS:
                            throw new AcRx.Exception(
                                AcRx.ErrorStatus.InvalidInput,
                                "To be used only with DCS");
                        case CoordSystem.DCS:
                            mat = ed.PSDCS2DCS();
                            break;
                        default:
                            mat = Matrix3d.Identity;
                            break;
                    }

                    break;
            }

            return pt.TransformBy(mat);
        }
    }
}