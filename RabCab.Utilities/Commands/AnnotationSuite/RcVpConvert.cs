// -----------------------------------------------------------------------------------
//     <copyright file="RcVpConvert.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>04/08/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using RabCab.Agents;
using RabCab.Engine.Enumerators;
using RabCab.Engine.System;
using RabCab.Extensions;
using RabCab.Settings;

namespace RabCab.Commands.AnnotationSuite
{
    internal class RcVpConvert
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_VPTOVB",
            CommandFlags.Modal
            //| CommandFlags.Transparent
            //| CommandFlags.UsePickSet
            //| CommandFlags.Redraw
            //| CommandFlags.NoPerspective
            //| CommandFlags.NoMultiple
            | CommandFlags.NoTileMode
            //| CommandFlags.NoPaperSpace
            //| CommandFlags.NoOem
            //| CommandFlags.Undefined
            //| CommandFlags.InProgress
            //| CommandFlags.Defun
            //| CommandFlags.NoNewStack
            //| CommandFlags.NoInternalLock
            //| CommandFlags.DocReadLock
            //| CommandFlags.DocExclusiveLock
            //| CommandFlags.Session
            //| CommandFlags.Interruptible
            | CommandFlags.NoHistory
            | CommandFlags.NoUndoMarker
            | CommandFlags.NoBlockEditor
            | CommandFlags.NoActionRecording
            | CommandFlags.ActionMacro
            //| CommandFlags.NoInferConstraint 
        )]
        public void Cmd_VpToVb()
        {
            //Get the current document utilities
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            var userCmdEcho = AcVars.CmdEcho;
            AcVars.CmdEcho = Enums.CmdEcho.Off;

            // Set up our selection to only select 3D solids
            var pso = new PromptSelectionOptions {MessageForAdding = "\nSelect viewports to convert: "};
            var sf = new SelectionFilter(new[] {new TypedValue((int) DxfCode.Start, "VIEWPORT")});

            //Get the 3d Solid Selection
            var res = acCurEd.GetSelection(pso, sf);

            if (res.Status == PromptStatus.OK)
                // extract the viewport points
                using (var acTrans = acCurDb.TransactionManager.StartTransaction())
                {
                    acCurDb.TransactionManager.QueueForGraphicsFlush();

                    using (
                        var pWorker = new ProgressAgent("Converting Viewports: ", res.Value.Count))
                    {
                        var deleteVps = false;

                        foreach (var objId in res.Value.GetObjectIds())
                        {
                            if (!pWorker.Progress())
                            {
                                acTrans.Abort();
                                return;
                            }

                            var psVpPnts = new Point3dCollection();

                            using (var psVp = acTrans.GetObject(objId, OpenMode.ForWrite) as Viewport)
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

                                    var msVpPnts = new Point3dCollection();
                                    foreach (Point3d pnt in psVpPnts)
                                    {
                                        var xform = psVp.Dcs2Wcs() * psVp.Psdcs2Dcs();
                                        // add the resulting point to the ms pnt array
                                        msVpPnts.Add(pnt.TransformBy(xform));
                                    }

                                    var layoutName = LayoutManager.Current.CurrentLayout;

                                    LayoutManager.Current.CurrentLayout = "Model";

                                    var extents = acCurDb.TileMode
                                        ? new Extents3d(acCurDb.Extmin, acCurDb.Extmax)
                                        : (int) Application.GetSystemVariable("CVPORT") == 1
                                            ? new Extents3d(acCurDb.Pextmin, acCurDb.Pextmax)
                                            : new Extents3d(acCurDb.Extmin, acCurDb.Extmax);

                                    using (var view = acCurEd.GetCurrentView())
                                    {
                                        var viewTransform =
                                            Matrix3d.PlaneToWorld(psVp.ViewDirection)
                                                .PreMultiplyBy(Matrix3d.Displacement(view.Target - Point3d.Origin))
                                                .PreMultiplyBy(Matrix3d.Rotation(-view.ViewTwist, view.ViewDirection,
                                                    view.Target))
                                                .Inverse();

                                        extents.TransformBy(viewTransform);

                                        view.ViewDirection = psVp.ViewDirection;
                                        view.Width = (extents.MaxPoint.X - extents.MinPoint.X) * 1.2;
                                        view.Height = (extents.MaxPoint.Y - extents.MinPoint.Y) * 1.2;
                                        view.CenterPoint = new Point2d(
                                            (extents.MinPoint.X + extents.MaxPoint.X) / 2.0,
                                            (extents.MinPoint.Y + extents.MaxPoint.Y) / 2.0);
                                        acCurEd.SetCurrentView(view);
                                    }

                                    // once switched, we can use the normal selection mode to select
                                    var selectionresult = acCurEd.SelectCrossingPolygon(msVpPnts);

                                    if (selectionresult.Status != PromptStatus.OK) return;

                                    var layout =
                                        (Layout)
                                        acTrans.GetObject(LayoutManager.Current.GetLayoutId(layoutName),
                                            OpenMode.ForRead);

                                    LayoutManager.Current.CurrentLayout = layoutName;

                                    try
                                    {
                                        var ext = acTrans.GetExtents(selectionresult.Value.GetObjectIds());
                                        var cenPt = Solid3DExtensions.GetBoxCenter(ext.MinPoint, ext.MaxPoint);
                                        var insPt = cenPt.TransformBy(psVp.Ms2Ps());

                                        CreateBaseViewFromVp(selectionresult, psVp, acCurEd, acCurDb, layout, insPt);

                                        deleteVps = true;
                                    }
                                    catch (Exception e)
                                    {
                                        deleteVps = false;
                                        acCurEd.WriteMessage(e.Message);
                                    }
                                }
                            }
                        }

                        if (deleteVps)
                            foreach (var objId in res.Value.GetObjectIds())
                            {
                                var vp = acTrans.GetObject(objId, OpenMode.ForWrite);
                                vp.Erase();
                                vp.Dispose();
                            }

                        acTrans.Commit();
                    }
                }

            AcVars.CmdEcho = userCmdEcho;
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
        private void CreateBaseViewFromVp(PromptSelectionResult prRes, Viewport acVp, Editor acCurEd, Database acCurDb,
            Layout curLayout, Point3d insertPoint)
        {
            LayoutManager.Current.CurrentLayout = "Model";

            var ss = SelectionSet.FromObjectIds(prRes.Value.GetObjectIds());
            var scaleString = GetScaleString(acVp.StandardScale);

            if (scaleString == "Custom" || scaleString == "1:1")
                // ReSharper disable once SpecifyACultureInStringConversionExplicitly
                scaleString = acVp.CustomScale.ToString();

            var extents = acCurDb.TileMode
                ? new Extents3d(acCurDb.Extmin, acCurDb.Extmax)
                : (int) Application.GetSystemVariable("CVPORT") == 1
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

            acCurEd.Command("_VIEWBASE", "M", "T", "B", "E", "R", "ALL", "A", ss, "", "O", "C", insertPoint, "H",
                "V", "V", "I", "Y", "TA", "Y", "N", "X", "S", scaleString, "", "");
        }

        /// <summary>
        ///     Adds a scale to the  DB based on the needed scale
        /// </summary>
        /// <param name="scale"></param>
        private string GetScaleString(StandardScaleType scale)
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
    }
}