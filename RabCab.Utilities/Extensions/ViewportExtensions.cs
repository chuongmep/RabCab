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
using System.ComponentModel;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using RabCab.Calculators;

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

        #region VPSelection

        public static ObjectIdCollection GetModelObjects(this ObjectId viewportId,
            SelectionFilter selectionFilter = null)
        {
            Point3dCollection pts;
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurEd = acCurDoc.Editor;
            int curPort;
            using (var acTrans = acCurDoc.Database.TransactionManager.StartTransaction())
            {
                var vPort = acTrans.GetObject(viewportId, OpenMode.ForRead) as Viewport;
                if (vPort != null)
                {
                    curPort = vPort.Number;
                    pts = vPort.P2M(vPort.GetExtents(acTrans));
                    acTrans.Commit();
                }
                else
                {
                    return new ObjectIdCollection();
                }
            }

            acCurEd.SwitchToModelSpace();
            Application.SetSystemVariable("CVPORT", curPort);
            var selArea = new Point3dCollection();
            var xForm = acCurEd.CurrentUserCoordinateSystem.Inverse();
            foreach (Point3d pt in pts) selArea.Add(pt.TransformBy(xForm));

            var result = selectionFilter == null
                ? acCurEd.SelectCrossingPolygon(selArea)
                : acCurEd.SelectCrossingPolygon(selArea, selectionFilter);

            acCurEd.SwitchToPaperSpace();

            return result.Status != PromptStatus.OK
                ? new ObjectIdCollection()
                : new ObjectIdCollection(result.Value.GetObjectIds());
        }

        public static ObjectIdCollection GetPaperObjects(this ObjectId viewportId)
        {
            var objCol = new ObjectIdCollection();
            if (viewportId.IsNull) return objCol;

            if (viewportId.IsErased || viewportId.Database == null) return objCol;

            using (var acTrans = viewportId.Database.TransactionManager.StartTransaction())
            {
                var vPort = acTrans.GetObject(viewportId, OpenMode.ForRead) as Viewport;

                if (vPort != null)
                {
                    if (vPort.Number == 1) return objCol;
                    var unitVector = CalcTol.UnitVector;

                    foreach (var objId in (BlockTableRecord) acTrans.GetObject(vPort.BlockId, OpenMode.ForRead))
                    {
                        if (objId.IsNull || objId.IsErased || objId == viewportId ||
                            objId.ObjectClass == RXObject.GetClass(typeof(Viewport)) ||
                            objId.ObjectClass == RXObject.GetClass(typeof(AttributeDefinition)) ||
                            objId.ObjectClass == RXObject.GetClass(typeof(AttributeReference)) ||
                            objId.ObjectClass == RXObject.GetClass(typeof(ProxyEntity))) continue;

                        var entity = acTrans.GetObject(objId, OpenMode.ForRead) as Entity;

                        if (entity == null) continue;

                        Extents2d ext2d;
                        try
                        {
                            ext2d = entity.GeometricExtents.Convert2d();
                        }
                        catch
                        {
                            return objCol;
                        }

                        if (ext2d != new Extents2d() &&
                            (ext2d.IsInside(ext2d, unitVector) || ext2d.Intersects(ext2d, unitVector)))
                            objCol.Add(objId);
                    }
                }
                else
                {
                    return objCol;
                }

                acTrans.Commit();
            }

            return objCol;
        }

        #endregion

        #region Viewport Points

        private static Point3dCollection GetExtents(this Viewport vp, Transaction tr)
        {
            if (!vp.NonRectClipOn) return vp.GeometricExtents.ExtPoints();

            var vpPnts = new Point3dCollection();

            using (var entity = tr.GetObject(vp.NonRectClipEntityId, OpenMode.ForRead) as Entity)
            {
                var curve = entity as Curve;
                if (curve == null) throw new WarningException();

                AddCurvePoints(curve, vpPnts);
            }

            return vpPnts;
        }

        private static void AddCurvePoints(Curve curve, Point3dCollection psVpPnts)
        {
            var startParam = curve.StartParam;
            var endParam = curve.EndParam;

            var lNum = (endParam - startParam) / 100.0;

            for (var i = startParam; i < endParam; i += lNum)
            {
                var pointAtParameter = curve.GetPointAtParameter(i);
                psVpPnts.Add(pointAtParameter);
            }
        }

        #endregion

        #region PointCoversions

        public static Matrix3d M2P(this Viewport vp)
        {
            if (vp.PerspectiveOn) return new Matrix3d();
            if (vp.Number == 1) return new Matrix3d();
            return vp.Dcs2Psdcs() * vp.Dcs2Wcs().Inverse();
        }

        public static Point3d ModelToPaper(this Viewport vp, Point3d modelPoint)
        {
            return modelPoint.TransformBy(vp.M2P());
        }

        public static Point3dCollection M2P(this Viewport vp, Point3dCollection modelPoints)
        {
            var pts = new Point3dCollection();
            var xForm = vp.M2P();
            foreach (Point3d pt in modelPoints) pts.Add(pt.TransformBy(xForm));
            return pts;
        }

        public static Matrix3d P2M(this Viewport vp)
        {
            if (vp.PerspectiveOn) return new Matrix3d();
            if (vp.Number == 1) return new Matrix3d();
            return vp.Dcs2Wcs() * vp.Dcs2Psdcs().Inverse();
        }

        public static Point3d P2M(this Viewport vp, Point3d paperPoint)
        {
            return paperPoint.TransformBy(vp.P2M());
        }

        public static Point3dCollection P2M(this Viewport vp, Point3dCollection paperPoints)
        {
            var pts = new Point3dCollection();
            var xForm = vp.P2M();
            foreach (Point3d pt in paperPoints) pts.Add(pt.TransformBy(xForm));
            return pts;
        }

        #endregion
    }
}