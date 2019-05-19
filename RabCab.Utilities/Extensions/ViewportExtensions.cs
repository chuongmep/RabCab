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
            int number;
            using (var acTrans = acCurDoc.Database.TransactionManager.StartTransaction())
            {
                var vp = acTrans.GetObject(viewportId, OpenMode.ForRead) as Viewport;
                if (vp != null)
                {
                    number = vp.Number;
                    pts = vp.PaperToModel(vp.GetClipPoints(acTrans));
                    acTrans.Commit();
                }
                else
                {
                    return new ObjectIdCollection();
                }
            }

            acCurEd.SwitchToModelSpace();
            Application.SetSystemVariable("CVPORT", number);
            var polygon = new Point3dCollection();
            var leftSide = acCurEd.CurrentUserCoordinateSystem.Inverse();
            foreach (Point3d pt in pts) polygon.Add(pt.TransformBy(leftSide));

            var result = selectionFilter == null
                ? acCurEd.SelectCrossingPolygon(polygon)
                : acCurEd.SelectCrossingPolygon(polygon, selectionFilter);

            acCurEd.SwitchToPaperSpace();

            return result.Status != PromptStatus.OK
                ? new ObjectIdCollection()
                : new ObjectIdCollection(result.Value.GetObjectIds());
        }

        public static ObjectIdCollection GetPaperObjects(this ObjectId viewportId)
        {
            var ids = new ObjectIdCollection();
            if (viewportId.IsNull) return ids;

            if (viewportId.IsErased || viewportId.Database == null) return ids;

            using (var acTrans = viewportId.Database.TransactionManager.StartTransaction())
            {
                var viewport = acTrans.GetObject(viewportId, OpenMode.ForRead) as Viewport;
                if (viewport != null)
                {
                    if (viewport.Number == 1) throw new NotSupportedException("Viewport.Number == 1");
                    var unitVector = CalcTol.UnitVector;
                    using (var enumerator =
                        (acTrans.GetObject(viewport.BlockId, OpenMode.ForRead) as BlockTableRecord)
                        ?.GetEnumerator())
                    {
                        while (true)
                        {
                            if (enumerator != null && !enumerator.MoveNext()) break;
                            if (enumerator == null) continue;
                            var current = enumerator.Current;
                            if (current.IsNull || current.IsErased || current == viewportId ||
                                current.ObjectClass == RXObject.GetClass(typeof(Viewport)) ||
                                current.ObjectClass == RXObject.GetClass(typeof(AttributeDefinition)) ||
                                current.ObjectClass == RXObject.GetClass(typeof(AttributeReference)) ||
                                current.ObjectClass == RXObject.GetClass(typeof(ProxyEntity))) continue;
                            var entity = acTrans.GetObject(current, OpenMode.ForRead) as Entity;
                            if (entity != null)
                            {
                                var ext2d = new Extents2d();
                                try
                                {
                                    ext2d = entity.GeometricExtents.Convert2d();
                                }
                                catch
                                {
                                    // ignored
                                }

                                var extentsd3 = new Extents2d();
                                if (ext2d != extentsd3 &&
                                    (ext2d.IsInside(ext2d, unitVector) ||
                                     ext2d.Intersects(ext2d, unitVector))) ids.Add(current);
                                continue;
                            }

                            return ids;
                        }
                    }

                    acTrans.Commit();
                }
                else
                {
                    return ids;
                }
            }

            return ids;
        }

        #endregion

        #region Viewport Points

        public static Point3dCollection GetClipPoints(this Viewport vp, Transaction tr)
        {
            if (!vp.NonRectClipOn) return vp.GeometricExtents.ExtPoints();
            var psVpPnts = new Point3dCollection();
            using (var entity = tr.GetObject(vp.NonRectClipEntityId, OpenMode.ForRead) as Entity)
            {
                var curve = entity as Curve;
                if (curve == null) throw new WarningException();
                AddCurvePoints(curve, psVpPnts);
            }

            return psVpPnts;
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

        public static Matrix3d ModelToPaper(this Viewport vp)
        {
            if (vp.PerspectiveOn) throw new NotSupportedException();
            if (vp.Number == 1) throw new NotSupportedException("Viewport.Number == 1");
            return vp.Dcs2Psdcs() * vp.Dcs2Wcs().Inverse();
        }

        public static Point3d ModelToPaper(this Viewport vp, Point3d modelPoint)
        {
            return modelPoint.TransformBy(vp.ModelToPaper());
        }

        public static Point3dCollection ModelToPaper(this Viewport vp, Point3dCollection modelPoints)
        {
            var pts = new Point3dCollection();
            var leftSide = vp.ModelToPaper();
            foreach (Point3d pt in modelPoints) pts.Add(pt.TransformBy(leftSide));
            return pts;
        }

        public static Matrix3d PaperToModel(this Viewport vp)
        {
            if (vp.PerspectiveOn) throw new NotSupportedException();
            if (vp.Number == 1) throw new NotSupportedException("Viewport.Number == 1");
            return vp.Dcs2Wcs() * vp.Dcs2Psdcs().Inverse();
        }

        public static Point3d PaperToModel(this Viewport vp, Point3d paperPoint)
        {
            return paperPoint.TransformBy(vp.PaperToModel());
        }

        public static Point3dCollection PaperToModel(this Viewport vp, Point3dCollection paperPoints)
        {
            var pts = new Point3dCollection();
            var leftSide = vp.PaperToModel();
            foreach (Point3d pt in paperPoints) pts.Add(pt.TransformBy(leftSide));
            return pts;
        }

        #endregion
    }
}