// -----------------------------------------------------------------------------------
//     <copyright file="ViewportExtensions.cs" company="CraterSpace">
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

namespace RabCab.Extensions
{
    /// <summary>
    ///     Provides extension methods for the Viewport type.
    /// </summary>
    public static class ViewportExtensions
    {
        /// <summary>
        ///     Gets the transformation matrix from the specified model space viewport Display Coordinate System (DCS)
        ///     to the World Coordinate System (WCS).
        /// </summary>
        /// <param name="vp">The instance to which this method applies.</param>
        /// <returns>The DCS to WDCS transformation matrix.</returns>
        public static Matrix3d Dcs2Wcs(this Viewport vp)
        {
            return
                Matrix3d.Rotation(-vp.TwistAngle, vp.ViewDirection, vp.ViewTarget) *
                Matrix3d.Displacement(vp.ViewTarget - Point3d.Origin) *
                Matrix3d.PlaneToWorld(vp.ViewDirection);
        }

        /// <summary>
        ///     Gets the transformation matrix from the World Coordinate System (WCS)
        ///     to the specified model space viewport Display Coordinate System (DCS).
        /// </summary>
        /// <param name="vp">The instance to which this method applies.</param>
        /// <returns>The WCS to DCS transformation matrix.</returns>
        public static Matrix3d Wcs2Dcs(this Viewport vp)
        {
            return vp.Dcs2Wcs().Inverse();
        }

        /// <summary>
        ///     Gets the transformation matrix from the specified paper space viewport Display Coordinate System (DCS)
        ///     to the paper space Display Coordinate System (PSDCS).
        /// </summary>
        /// <param name="vp">The instance to which this method applies.</param>
        /// <returns>The DCS to PSDCS transformation matrix.</returns>
        public static Matrix3d Dcs2Psdcs(this Viewport vp)
        {
            return
                Matrix3d.Scaling(vp.CustomScale, vp.CenterPoint) *
                Matrix3d.Displacement(vp.CenterPoint.GetAsVector()) *
                Matrix3d.Displacement(vp.ViewCenter.Convert3D().GetAsVector().Negate());
        }

        /// <summary>
        ///     Gets the transformation matrix from the Paper Space Display Coordinate System (PSDCS)
        ///     to the specified paper space viewport Display Coordinate System (DCS).
        /// </summary>
        /// <param name="vp">The instance to which this method applies.</param>
        /// <returns>The PSDCS to DCS transformation matrix.</returns>
        public static Matrix3d Psdcs2Dcs(this Viewport vp)
        {
            return vp.Dcs2Psdcs().Inverse();
        }

        /// <summary>
        ///     Method to select all visible elements in a viewport
        /// </summary>
        /// <param name="acVp"></param>
        /// <param name="acCurDb"></param>
        /// <param name="acCurEd"></param>
        /// <returns></returns>
        public static SelectionSet SelectAllVisible(this Viewport acVp, Database acCurDb, Editor acCurEd)
        {
            SelectionSet ss = null;

            using (var tr = acCurDb.TransactionManager.StartTransaction())
            {
                var psVpPnts = new Point3dCollection();

                using (var psVp = tr.GetObject(acVp.ObjectId, OpenMode.ForWrite) as Viewport)
                {
                    // get the vp number
                    if (psVp != null)
                    {
                        // now extract the viewport geometry
                        psVp.GetGripPoints(psVpPnts, new IntegerCollection(), new IntegerCollection());

                        // let's assume a rectangular vport for now, make the cross-direction grips square
                        var tmp = psVpPnts[2];
                        psVpPnts[2] = psVpPnts[1];
                        psVpPnts[1] = tmp;

                        var geomExt = psVp.GeometricExtents;

                        psVpPnts.Add(geomExt.MinPoint);
                        psVpPnts.Add(new Point3d(geomExt.MinPoint.X, geomExt.MaxPoint.Y, geomExt.MinPoint.Z));
                        psVpPnts.Add(geomExt.MaxPoint);
                        psVpPnts.Add(new Point3d(geomExt.MaxPoint.X, geomExt.MinPoint.Y, geomExt.MaxPoint.Z));

                        var msVpPnts = new Point3dCollection();
                        foreach (Point3d pnt in psVpPnts)
                        {
                            var xform = psVp.Dcs2Wcs() * psVp.Psdcs2Dcs();
                            // add the resulting point to the ms pnt array
                            msVpPnts.Add(pnt.TransformBy(xform));
                        }

                        // now switch to MS
                        acCurEd.SwitchToModelSpace();
                        // set the CVPort
                        Application.SetSystemVariable("CVPORT", acVp.Number);

                        acCurEd.Command("_-VISUALSTYLES", "C", "Hidden");
                        // once switched, we can use the normal selection mode to select
                        var selectionresult = acCurEd.SelectCrossingPolygon(msVpPnts);

                        // now switch back to PS
                        acCurEd.SwitchToPaperSpace();

                        if (selectionresult.Status != PromptStatus.OK)
                        {
                            tr.Abort();
                            return null;
                        }

                        ss = selectionresult.Value;
                    }
                }

                tr.Commit();
            }

            return ss;
        }

        internal static Matrix3d Ms2Ps(this Viewport vp)
        {
            var viewDirection = vp.ViewDirection;
            var center = vp.ViewCenter;
            var viewCenter = new Point3d(center.X, center.Y, 0);
            var viewTarget = vp.ViewTarget;
            var twistAngle = -vp.TwistAngle;
            var centerPoint = vp.CenterPoint;

            var viewHeight = vp.ViewHeight;

            var height = vp.Height;

            var width = vp.Width;

            var scaling = viewHeight / height;

            var lensLength = vp.LensLength;


            var zAxis = viewDirection.GetNormal();

            var xAxis = Vector3d.ZAxis.CrossProduct(viewDirection);


            Vector3d yAxis;

            if (!xAxis.IsZeroLength())
            {
                xAxis = NormalizeVector(xAxis);
                yAxis = zAxis.CrossProduct(xAxis);
            }
            else if (zAxis.Z < 0)
            {
                xAxis = -Vector3d.XAxis;
                yAxis = Vector3d.YAxis;
                zAxis = -Vector3d.ZAxis;
            }
            else
            {
                xAxis = Vector3d.XAxis;
                yAxis = Vector3d.YAxis;
                zAxis = Vector3d.ZAxis;
            }

            var ps2Dcs = Matrix3d.Displacement(Point3d.Origin - centerPoint);
            ps2Dcs = ps2Dcs * Matrix3d.Scaling(scaling, centerPoint);
            var dcs2Wcs = Matrix3d.Displacement(viewCenter - Point3d.Origin);
            var matCoords = Matrix3d.AlignCoordinateSystem(
                Matrix3d.Identity.CoordinateSystem3d.Origin,
                Matrix3d.Identity.CoordinateSystem3d.Xaxis,
                Matrix3d.Identity.CoordinateSystem3d.Yaxis,
                Matrix3d.Identity.CoordinateSystem3d.Zaxis,
                Matrix3d.Identity.CoordinateSystem3d.Origin,
                xAxis, yAxis, zAxis);
            dcs2Wcs = matCoords * dcs2Wcs;

            dcs2Wcs = Matrix3d.Displacement(viewTarget - Point3d.Origin) * dcs2Wcs;

            dcs2Wcs = Matrix3d.Rotation(twistAngle, zAxis, viewTarget) * dcs2Wcs;

            var perspMat = Matrix3d.Identity;
            if (vp.PerspectiveOn)
            {
                var viewsize = viewHeight;
                var aspectRatio = width / height;
                var adjustFactor = 1.0 / 42.0;
                var adjustedLensLength =
                    viewsize * lensLength * Math.Sqrt(1.0 + aspectRatio * aspectRatio) * adjustFactor;

                var eyeDistance = viewDirection.Length;
                var lensDistance = eyeDistance - adjustedLensLength;

                var ed = eyeDistance;
                var ll = adjustedLensLength;
                var l = lensDistance;
                perspMat = new Matrix3d(new[]
                {
                    1, 0, 0, 0,
                    0, 1, 0, 0,
                    0, 0, (ll - l) / ll, l * (ed - ll) / ll,
                    0, 0, -1.0 / ll, ed / ll
                });
            }

            return ps2Dcs.Inverse() * perspMat * dcs2Wcs.Inverse();
        }

        internal static Vector3d NormalizeVector(Vector3d vec)
        {
            var length = Math.Sqrt(vec.X * vec.X + vec.Y * vec.Y + vec.Z * vec.Z);
            var x = vec.X / length;
            var y = vec.Y / length;
            var z = vec.Z / length;
            return new Vector3d(x, y, z);
        }
    }
}