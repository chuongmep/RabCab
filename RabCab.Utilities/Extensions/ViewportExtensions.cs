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
using System.Collections.Generic;
using System.ComponentModel;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using RabCab.Calculators;
using Exception = Autodesk.AutoCAD.BoundaryRepresentation.Exception;

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
        ///     Finds the best scale for the chosen items
        /// </summary>
        /// <param name="acVp"></param>
        /// <param name="extents"></param>
        /// <param name="isoScale"></param>
        /// <returns></returns>
        public static StandardScaleType FindBestScale(this Viewport acVp, Extents3d extents)
        {
            //Set vp to top view
            var curView = acVp.ViewDirection;

            if (curView != ViewDirection.TopView)
            {
                acVp.ViewDirection = ViewDirection.TopView;
                acVp.UpdateDisplay();
            }

            var scaleList = new List<StandardScaleType>
            {
                StandardScaleType.Scale100To1,
                StandardScaleType.Scale10To1,
                StandardScaleType.Scale8To1,
                StandardScaleType.Scale4To1,
                StandardScaleType.Scale2To1,
                StandardScaleType.Scale1To1,
                StandardScaleType.Scale1To2,
                StandardScaleType.Scale1To4,
                StandardScaleType.Scale1To5,
                StandardScaleType.Scale1To8,
                StandardScaleType.Scale1To10,
                StandardScaleType.Scale1To16,
                StandardScaleType.Scale1To20,
                StandardScaleType.Scale1To30,
                StandardScaleType.Scale1To40,
                StandardScaleType.Scale1To50,
                StandardScaleType.Scale1To100
            };

            for (var index = 0; index < scaleList.Count; index++)
            {
                try
                {
                    var scale = scaleList[index];
                    acVp.StandardScale = scale;
                    acVp.UpdateDisplay();

                    if (IsInsideVpExtents(scale, acVp, extents))
                    {
                        AddScaleToDb(scaleList[index]);
                        acVp.ViewDirection = curView;
                        return scaleList[index];
                    }
                }
                catch (Exception)
                {
                    acVp.ViewDirection = curView;
                    return StandardScaleType.CustomScale;
                }
            }

            acVp.ViewDirection = curView;
            return StandardScaleType.CustomScale;
        }

        /// <summary>
        ///     Adds a scale to the  DB based on the needed scale
        /// </summary>
        /// <param name="scale"></param>
        private static void AddScaleToDb(StandardScaleType scale)
        {
            switch (scale)
            {
                case StandardScaleType.Scale100To1:
                    AddScale("100:1", 100, 1);
                    break;
                case StandardScaleType.Scale10To1:
                    AddScale("10:1", 10, 1);
                    break;
                case StandardScaleType.Scale8To1:
                    AddScale("8:1", 8, 1);
                    break;
                case StandardScaleType.Scale4To1:
                    AddScale("4:1", 4, 1);
                    break;
                case StandardScaleType.Scale2To1:
                    AddScale("2:1", 2, 1);
                    break;
                case StandardScaleType.Scale1To1:
                    AddScale("1:1", 1, 1);
                    break;
                case StandardScaleType.Scale1To2:
                    AddScale("1:2", 1, 2);
                    break;
                case StandardScaleType.Scale1To4:
                    AddScale("1:4", 1, 4);
                    break;
                case StandardScaleType.Scale1To5:
                    AddScale("1:5", 1, 5);
                    break;
                case StandardScaleType.Scale1To8:
                    AddScale("1:8", 1, 8);
                    break;
                case StandardScaleType.Scale1To10:
                    AddScale("1:10", 1, 10);
                    break;
                case StandardScaleType.Scale1To16:
                    AddScale("1:16", 1, 16);
                    break;
                case StandardScaleType.Scale1To20:
                    AddScale("1:20", 1, 20);
                    break;
                case StandardScaleType.Scale1To30:
                    AddScale("1:30", 1, 30);
                    break;
                case StandardScaleType.Scale1To40:
                    AddScale("1:40", 1, 40);
                    break;
                case StandardScaleType.Scale1To50:
                    AddScale("1:50", 1, 50);
                    break;
                case StandardScaleType.Scale1To100:
                    AddScale("1:100", 1, 100);
                    break;
            }
        }

        /// <summary>
        ///     Adds a scale to the scalelist if it doesnt exits
        /// </summary>
        /// <param name="scaleName"></param>
        /// <param name="pUnits"></param>
        /// <param name="dwgUnits"></param>
        private static void AddScale(string scaleName, int pUnits, int dwgUnits)
        {
            try
            {
                var cm =
                    Application.DocumentManager.CurrentDocument.Database.ObjectContextManager;
                // Now get the Annotation Scaling context collection
                // (named ACDB_ANNOTATIONSCALES_COLLECTION)
                var occ =
                    cm?.GetContextCollection("ACDB_ANNOTATIONSCALES");
                if (occ != null)
                {
                    if (!occ.HasContext(scaleName))
                    {
                        // Create a brand new scale context
                        var asc = new AnnotationScale
                        {
                            Name = scaleName,
                            PaperUnits = pUnits,
                            DrawingUnits = dwgUnits
                        };
                        // Add it to the drawing's context collection
                        occ.AddContext(asc);
                    }
                }
            }
            catch (Exception)
            {
                //Ignored
            }
        }

        /// <summary>
        ///     Checks if objects shown are inside VP extents
        /// </summary>
        /// <param name="scaleType"></param>
        /// <param name="acVp"></param>
        /// <param name="extents"></param>
        /// <returns></returns>
        private static bool IsInsideVpExtents(StandardScaleType scaleType, Viewport acVp, Extents3d extents)
        {
            acVp.StandardScale = scaleType;

            var vpExt = acVp.GeometricExtents;
            var xform = acVp.Dcs2Wcs() * acVp.Psdcs2Dcs();
            var vpMin = vpExt.MinPoint.TransformBy(xform);
            var vpMax = vpExt.MaxPoint.TransformBy(xform);

            var solMin = extents.MinPoint;
            var solMax = extents.MaxPoint;

            if (vpMin.X < solMin.X && vpMin.Y < solMin.Y)
            {
                if (vpMax.X > solMax.X && vpMax.Y > solMax.Y)
                {
                    return true;
                }
            }

            return false;
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

        /// <summary>
        ///     Method to convert viewport to View base
        /// </summary>
        /// <param name="prRes"></param>
        /// <param name="acVp"></param>
        /// <param name="acCurEd"></param>
        /// <param name="acCurDb"></param>
        /// <param name="curLayout"></param>
        /// <param name="insertPoint"></param>
        public static void CreateBaseViewFromVp(this Viewport acVp, ObjectId id, Editor acCurEd, Database acCurDb,
            Layout curLayout, Point3d insertPoint)
        {
            LayoutManager.Current.CurrentLayout = "Model";

            var ss = SelectionSet.FromObjectIds(new[] { id });
            var scaleString = GetScaleString(acVp.StandardScale);

            if (scaleString == "Custom" || scaleString == "1:1")
                // ReSharper disable once SpecifyACultureInStringConversionExplicitly
                scaleString = acVp.CustomScale.ToString();

            var extents = acCurDb.TileMode
                ? new Extents3d(acCurDb.Extmin, acCurDb.Extmax)
                : (int)Application.GetSystemVariable("CVPORT") == 1
                    ? new Extents3d(acCurDb.Pextmin, acCurDb.Pextmax)
                    : new Extents3d(acCurDb.Extmin, acCurDb.Extmax);

            using (var view = acCurEd.GetCurrentView())
            {
                var viewTransform =
                    Matrix3d.PlaneToWorld(acVp.ViewDirection)
                        .PreMultiplyBy(Matrix3d.Displacement(view.Target - Point3d.Origin))
                        .PreMultiplyBy(Matrix3d.Rotation(-view.ViewTwist, view.ViewDirection, view.Target))
                        .Inverse();

                extents.TransformBy(viewTransform);

                view.ViewDirection = acVp.ViewDirection;
                view.Width = (extents.MaxPoint.X - extents.MinPoint.X) * 1.2;
                view.Height = (extents.MaxPoint.Y - extents.MinPoint.Y) * 1.2;
                view.CenterPoint = new Point2d(
                    (extents.MinPoint.X + extents.MaxPoint.X) / 2.0,
                    (extents.MinPoint.Y + extents.MaxPoint.Y) / 2.0);
                acCurEd.SetCurrentView(view);
            }

            LayoutManager.Current.CurrentLayout = curLayout.LayoutName;
            System.Threading.Thread.Sleep(100);

            try
            {
                acCurEd.Command("_VIEWBASE", "M", "T", "B", "E", "R", "ALL", "A", ss, string.Empty, "O", "C", insertPoint,
                    "H",
                    "V", "V", "I", "Y", "TA", "Y", "N", "X", "S", scaleString, string.Empty, string.Empty);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
          
        }

        /// <summary>
        ///     Adds a scale to the  DB based on the needed scale
        /// </summary>
        /// <param name="scale"></param>
        public static string GetScaleString(StandardScaleType scale)
        {
            //TODO add all scales

            switch (scale)
            {
                case StandardScaleType.Scale100To1:
                    return "100:1";
                case StandardScaleType.Scale10To1:
                    return "10:1";
                case StandardScaleType.Scale8To1:
                    return "8:1";
                case StandardScaleType.Scale4To1:
                    return "4:1";
                case StandardScaleType.Scale2To1:
                    return "2:1";
                case StandardScaleType.Scale1To1:
                    return "1:1";
                case StandardScaleType.Scale1To2:
                    return "1:2";
                case StandardScaleType.Scale1To4:
                    return "1:4";
                case StandardScaleType.Scale1To5:
                    return "1:5";
                case StandardScaleType.Scale1To8:
                    return "1:8";
                case StandardScaleType.Scale1To10:
                    return "1:10";
                case StandardScaleType.Scale1To16:
                    return "1:16";
                case StandardScaleType.Scale1To20:
                    return "1:20";
                case StandardScaleType.Scale1To30:
                    return "1:30";
                case StandardScaleType.Scale1To40:
                    return "1:40";
                case StandardScaleType.Scale1To50:
                    return "1:50";
                case StandardScaleType.Scale1To100:
                    return "1:100";
                default:
                    return "Custom";
            }
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