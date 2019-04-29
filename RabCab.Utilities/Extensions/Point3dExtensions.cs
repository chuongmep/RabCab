// -----------------------------------------------------------------------------------
//     <copyright file="Point3dExtensions.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>03/08/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using System;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using RabCab.Settings;
using AcRx = Autodesk.AutoCAD.Runtime;

namespace RabCab.Extensions
{
    /// <summary>
    ///     Provides extension methods for the Point3d type.
    /// </summary>
    public static class Point3DExtensions
    {

        ///     Method to return a displacement vector - transformed by the current UCS
        /// </summary>
        /// <param name="point1">Point3d to transform from</param>
        /// <param name="point2">Point3d to transform to</param>
        /// <param name="acCurEd">The Current Working Editor</param>
        /// <returns></returns>
        public static Vector3d GetTransformedVector(this Point3d point1, Point3d point2, Editor acCurEd)
        {
            //Get the vector from point1 to point2
            var acVec3D = point1.GetVectorTo(point2);

            //Transform the vector by the current UCS and return it
            return acVec3D.TransformBy(acCurEd.CurrentUserCoordinateSystem);
        }

        public static Point3d GetOrthoPoint( this Point3d pt, Point3d basePt)
        {
            // Apply a crude orthographic mode
            double x = pt.X;
            double y = pt.Y;

            Vector3d vec = basePt.GetVectorTo(pt);
            if (Math.Abs(vec.X) >= Math.Abs(vec.Y))
               y = basePt.Y;
            else
                x = basePt.X;

            return new Point3d(x, y, 0.0);
        }

        /// <summary>
        ///     Method to get the midpoint between the current point and an input point
        /// </summary>
        /// <param name="pt1"></param>
        /// <param name="pt2"></param>
        /// <returns></returns>
        public static Point3d GetMidPoint(this Point3d pt1, Point3d pt2)
        {
            var vector = pt1.GetVectorTo(pt2);
            var halfwayPoint = pt1 + vector * 0.5;
            return halfwayPoint;
        }

        public static Point3d GetAlong(this Point3d pt1, Point3d pt2, double distance)
        {
            var direction = pt1.GetVectorTo(pt2).GetNormal();
            return pt1 + direction * distance;
        }

        /// <summary>
        ///     Converts a 3d point into a 2d point (projection on XY plane).
        /// </summary>
        /// <param name="pt">The instance to which the method applies.</param>
        /// <returns>The corresponding 2d point.</returns>
        public static Point2d Convert2D(this Point3d pt)
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
                org.X + distance * Math.Cos(angle),
                org.Y + distance * Math.Sin(angle),
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
                case CoordSystem.Wcs:
                    switch (to)
                    {
                        case CoordSystem.Ucs:
                            mat = ed.Wcs2Ucs();
                            break;
                        case CoordSystem.Dcs:
                            mat = ed.Wcs2Dcs();
                            break;
                        case CoordSystem.Psdcs:
                            throw new AcRx.Exception(
                                AcRx.ErrorStatus.InvalidInput,
                                "To be used only with DCS");
                        default:
                            mat = Matrix3d.Identity;
                            break;
                    }

                    break;
                case CoordSystem.Ucs:
                    switch (to)
                    {
                        case CoordSystem.Wcs:
                            mat = ed.Ucs2Wcs();
                            break;
                        case CoordSystem.Dcs:
                            mat = ed.Ucs2Wcs() * ed.Wcs2Dcs();
                            break;
                        case CoordSystem.Psdcs:
                            throw new AcRx.Exception(
                                AcRx.ErrorStatus.InvalidInput,
                                "To be used only with DCS");
                        default:
                            mat = Matrix3d.Identity;
                            break;
                    }

                    break;
                case CoordSystem.Dcs:
                    switch (to)
                    {
                        case CoordSystem.Wcs:
                            mat = ed.Dcs2Wcs();
                            break;
                        case CoordSystem.Ucs:
                            mat = ed.Dcs2Wcs() * ed.Wcs2Ucs();
                            break;
                        case CoordSystem.Psdcs:
                            mat = ed.Dcs2Psdcs();
                            break;
                        default:
                            mat = Matrix3d.Identity;
                            break;
                    }

                    break;
                case CoordSystem.Psdcs:
                    switch (to)
                    {
                        case CoordSystem.Wcs:
                            throw new AcRx.Exception(
                                AcRx.ErrorStatus.InvalidInput,
                                "To be used only with DCS");
                        case CoordSystem.Ucs:
                            throw new AcRx.Exception(
                                AcRx.ErrorStatus.InvalidInput,
                                "To be used only with DCS");
                        case CoordSystem.Dcs:
                            mat = ed.Psdcs2Dcs();
                            break;
                        default:
                            mat = Matrix3d.Identity;
                            break;
                    }

                    break;
            }

            return pt.TransformBy(mat);
        }

        /// <summary>
        ///     Method to move an insert solid to by the interference step point
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Point3d StepXLeft(this Point3d point)
        {
            return new Point3d(point.X - SettingsUser.LayStep, point.Y, point.Z);
        }

        /// <summary>
        ///     Method to move an insert solid to by the interference step point
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Point3d StepXRight(this Point3d point)
        {
            return new Point3d(point.X + SettingsUser.LayStep, point.Y, point.Z);
        }

        /// <summary>
        ///     Method to move an insert solid to by the interference step point
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Point3d StepYPoint(this Point3d point)
        {
            return new Point3d(point.X, point.Y - SettingsUser.LayStep, point.Z);
        }

        /// <summary>
        ///     Method to move an insert solid to by the interference step point
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Point3d StepYUpward(this Point3d point, double stepAmount)
        {
            return new Point3d(point.X, point.Y + stepAmount, point.Z);
        }

        /// <summary>
        ///     Method to move an insert solid to by the interference step point
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Point3d StepYDownward(this Point3d point, double stepAmount)
        {
            return new Point3d(point.X, point.Y - stepAmount, point.Z);
        }
    }
}