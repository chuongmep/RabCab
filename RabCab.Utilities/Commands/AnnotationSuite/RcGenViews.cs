// -----------------------------------------------------------------------------------
//     <copyright file="RcGenViews.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>04/08/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using RabCab.Calculators;
using RabCab.Extensions;
using RabCab.Settings;
using Exception = Autodesk.AutoCAD.BoundaryRepresentation.Exception;

namespace RabCab.Commands.AnnotationSuite
{
    internal class RcGenViews
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_GENVIEWS",
            CommandFlags.Modal
            //| CommandFlags.Transparent
            //| CommandFlags.UsePickSet
            //| CommandFlags.Redraw
            //| CommandFlags.NoPerspective
            //| CommandFlags.NoMultiple
            //| CommandFlags.NoTileMode
            | CommandFlags.NoPaperSpace
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
            //| CommandFlags.NoHistory
            //| CommandFlags.NoUndoMarker
            | CommandFlags.NoBlockEditor
            //| CommandFlags.NoActionRecording
            //| CommandFlags.ActionMacro
            //| CommandFlags.NoInferConstraint 
        )]
        public void Cmd_GenViews()
        {
            //Get the current document utilities
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;
            var modelView = acCurEd.GetCurrentView().Clone() as ViewTableRecord;

            var uSelOpts = new PromptSelectionOptions
            {
                MessageForAdding = "\nSelect objects to use in view creation: "
            };

            var userSelection = acCurEd.GetSelection(uSelOpts);

            if (userSelection.Status != PromptStatus.OK) return;

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                var boundBox = acTrans.GetBoundingBox(userSelection.Value.GetObjectIds(), acCurDb);

                //Get all Layouts in the Drawing
                var layoutList = new List<Layout>();
                var dbDict = (DBDictionary) acTrans.GetObject(acCurDb.LayoutDictionaryId, OpenMode.ForRead);
                foreach (var curEntry in dbDict)
                {
                    var layout = (Layout) acTrans.GetObject(curEntry.Value, OpenMode.ForRead);
                    if (layout != null)
                        layoutList.Add(layout);
                }

                var pKeyOpts = new PromptKeywordOptions("")
                {
                    Message = "\nWhich layout would you like to create views in?",
                    AllowArbitraryInput = true
                };
                var iterator = 'A';
                var keyDict = new Dictionary<string, string>();

                foreach (var layout in layoutList)
                    if (layout.LayoutName != "Model")
                    {
                        keyDict.Add(layout.LayoutName, iterator.ToString());
                        pKeyOpts.Keywords.Add(iterator.ToString(), iterator.ToString(),
                            iterator + ": " + layout.LayoutName.ToLower());
                        iterator++;
                    }

                pKeyOpts.AllowNone = false;

                var pKeyRes = acCurEd.GetKeywords(pKeyOpts);

                if (pKeyRes.Status != PromptStatus.OK) return;
                var returnIterator = pKeyRes.StringResult;

                ObjectId id;
                var layoutName = "";

                foreach (var entry in keyDict)
                    if (entry.Value == returnIterator)
                    {
                        layoutName = entry.Key;
                        break;
                    }

                if (dbDict.Contains(layoutName))
                {
                    id = dbDict.GetAt(layoutName);
                }
                else
                {
                    acCurEd.WriteMessage("\nLayout not found. Cannot continue.");
                    acTrans.Abort();
                    return;
                }

                var chosenLayout = acTrans.GetObject(id, OpenMode.ForRead) as Layout;
                if (chosenLayout == null) return;

                // Reference the Layout Manager
                var acLayoutMgr = LayoutManager.Current;
                // Set the layout current if it is not already
                if (chosenLayout.TabSelected == false) acLayoutMgr.CurrentLayout = chosenLayout.LayoutName;

                acCurEd.SwitchToPaperSpace();

                var layOutSize = GetLayoutSize(chosenLayout);
                var importSize = new LayoutSize(0, 0);

                var iPorts = ImportViewports(acCurEd, ref importSize.Height, ref importSize.Width);
                var vStyles = acTrans.GetObject(acCurDb.VisualStyleDictionaryId, OpenMode.ForRead) as DBDictionary;

                if (iPorts.Count > 0)
                    foreach (var iPort in iPorts)
                    {
                        var ht = CalcUnit.GetProportion(iPort.VHeight, importSize.Height, layOutSize.Height);
                        var wd = CalcUnit.GetProportion(iPort.VWidth, importSize.Width, layOutSize.Width);

                        if (ht > layOutSize.Height || wd > layOutSize.Width)
                            continue;

                        var vPort = new Viewport
                        {
                            Height = ht,
                            Width = wd
                        };

                        var importPt = iPort.InsertPoint;
                        var xProp = CalcUnit.GetProportion(importPt.X, importSize.Width, layOutSize.Width);
                        var yProp = CalcUnit.GetProportion(importPt.Y, importSize.Height, layOutSize.Height);

                        vPort.CenterPoint = new Point3d(xProp, yProp, 0);

                        acCurDb.AppendEntity(vPort, acTrans);

                        vPort.ViewDirection = iPort.ViewDirection;

                        ZoomViewport(acCurDb, vPort, boundBox);
                        //TODO find closest scale to zoom
                        //TODO allow user to set these
                        if (vStyles != null) vPort.SetShadePlot(ShadePlotType.Hidden, vStyles.GetAt("Hidden"));

                        // Enable the viewport
                        vPort.Visible = true;
                        vPort.On = true;

                        vPort.UpdateDisplay();
                    }

                boundBox.Dispose();

                LayoutManager.Current.CurrentLayout = "Model";
                using (var view = acCurEd.GetCurrentView())
                {
                    view.CopyFrom(modelView);
                    acCurEd.SetCurrentView(view);
                }

                try
                {
                    Thread.Sleep(100);
                    LayoutManager.Current.CurrentLayout = chosenLayout.LayoutName;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                if (modelView != null) modelView.Dispose();
                acTrans.Commit();
            }
        }

        private LayoutSize GetLayoutSize(Layout layout)
        {
            // If the drawing template is imperial, we need to divide by
            // 1" in mm (25.4)
            var div = layout.PlotPaperUnits == PlotPaperUnit.Inches ? 25.4 : 1.0;

            // We need to flip the axis if the plot is rotated by 90 or 270 deg
            var doIt = layout.PlotRotation == PlotRotation.Degrees090 ||
                       layout.PlotRotation == PlotRotation.Degrees270;

            // Get the extents in the correct units and orientation
            var min = layout.PlotPaperMargins.MinPoint.Swap(doIt) / div;
            var max = (layout.PlotPaperSize.Swap(doIt) -
                       layout.PlotPaperMargins.MaxPoint.Swap(doIt).GetAsVector()) / div;
            var paperLength = max.X - min.X;
            var paperHeight = max.Y - min.Y;

            return new LayoutSize(paperLength, paperHeight);
        }

        /// <summary>
        ///     Method to prompt user for a layout to import the layout template of
        /// </summary>
        /// <returns></returns>
        private List<ImportedViewport> ImportViewports(Editor acCurEd, ref double layoutHeight, ref double layoutWidth)
        {
            var viewports = new List<ImportedViewport>();

            try
            {
                using (var exDb = acCurEd.GetTemplate(ref SettingsUser.ViewTemplatePath))
                {
                    var exLayouts = new List<Layout>();

                    using (var exTrans = exDb.TransactionManager.StartTransaction())
                    {
                        var dbDict =
                            (DBDictionary) exTrans.GetObject(exDb.LayoutDictionaryId, OpenMode.ForRead);
                        foreach (var curEntry in dbDict)
                        {
                            var exLayout = (Layout) exTrans.GetObject(curEntry.Value, OpenMode.ForRead);

                            exLayouts.Add(exLayout);
                        }

                        var pKeyOpts = new PromptKeywordOptions("")
                        {
                            Message = "\nSelect layout to copy viewports from: ",
                            AllowArbitraryInput = true
                        };
                        var iterator = 'A';
                        var keyDict = new Dictionary<string, string>();

                        foreach (var layout in exLayouts)
                            if (layout.LayoutName != "Model")
                            {
                                keyDict.Add(layout.LayoutName, iterator.ToString());
                                pKeyOpts.Keywords.Add(iterator.ToString(), iterator.ToString(),
                                    iterator + ": " + layout.LayoutName.ToLower());
                                iterator++;
                            }

                        pKeyOpts.AllowNone = false;

                        var pKeyRes = acCurEd.GetKeywords(pKeyOpts);

                        if (pKeyRes.Status != PromptStatus.OK) return viewports;
                        var returnIterator = pKeyRes.StringResult;

                        ObjectId id;
                        var layoutName = "";

                        foreach (var entry in keyDict)
                            if (entry.Value == returnIterator)
                            {
                                layoutName = entry.Key;
                                break;
                            }

                        if (dbDict.Contains(layoutName))
                        {
                            id = dbDict.GetAt(layoutName);
                        }
                        else
                        {
                            acCurEd.WriteMessage("\nLayout contains no viewports.");
                            return viewports;
                        }

                        var chosenLayout = exTrans.GetObject(id, OpenMode.ForRead) as Layout;
                        if (chosenLayout == null) return viewports;

                        var laySize = GetLayoutSize(chosenLayout);
                        layoutHeight = laySize.Height;
                        layoutWidth = laySize.Width;

                        //Get viewports from chosen layout
                        using (
                            var blkTblRec =
                                exTrans.GetObject(chosenLayout.BlockTableRecordId, OpenMode.ForRead) as BlockTableRecord
                        )
                        {
                            if (blkTblRec != null)
                                foreach (var objId in blkTblRec)
                                {
                                    if (objId == exDb.PaperSpaceVportId) continue;

                                    var exView = exTrans.GetObject(objId, OpenMode.ForRead) as Viewport;
                                    if (exView != null && !exView.IsErased && exView.Visible)
                                    {
                                        var vHeight = exView.Height;
                                        var vWidth = exView.Width;
                                        var vCen = exView.CenterPoint;

                                        viewports.Add(new ImportedViewport(vHeight, vWidth, vCen,
                                            exView.ViewDirection));
                                    }
                                }
                        }

                        exTrans.Commit();
                    }
                }
            }
            catch (System.Exception e)
            {
                acCurEd.WriteMessage(e.Message);
            }

            return viewports;
        }

        /// <summary>
        ///     Method to zoom a viewport to the extents of an input entity
        /// </summary>
        /// <param name="acCurDb"></param>
        /// <param name="acCurVp"></param>
        /// <param name="acEnt"></param>
        private void ZoomViewport(Database acCurDb, Viewport acCurVp, Entity acEnt = null)
        {
            // get the screen aspect ratio to calculate
            // the height and width
            // width/height
            var mScrRatio = acCurVp.Width / acCurVp.Height;
            var mMaxExt = acCurDb.Extmax;
            var mMinExt = acCurDb.Extmin;

            if (acEnt != null)
            {
                mMaxExt = acEnt.GeometricExtents.MaxPoint;
                mMinExt = acEnt.GeometricExtents.MinPoint;
            }

            var mExtents = new Extents3d();
            mExtents.Set(mMinExt, mMaxExt);

            // prepare Matrix for DCS to WCS transformation
            var matWcs2Dcs = Matrix3d.PlaneToWorld(acCurVp.ViewDirection);
            matWcs2Dcs = Matrix3d.Displacement(acCurVp.ViewTarget - Point3d.Origin) * matWcs2Dcs;
            matWcs2Dcs = Matrix3d.Rotation(-acCurVp.TwistAngle, acCurVp.ViewDirection, acCurVp.ViewTarget) * matWcs2Dcs;
            matWcs2Dcs = matWcs2Dcs.Inverse();

            // tranform the extents to the DCS
            // defined by the viewdir
            mExtents.TransformBy(matWcs2Dcs);

            // width of the extents in current view
            var mWidth = mExtents.MaxPoint.X - mExtents.MinPoint.X;

            // height of the extents in current view
            var mHeight = mExtents.MaxPoint.Y - mExtents.MinPoint.Y;

            // get the view center point
            var mCentPt = new Point2d(
                (mExtents.MaxPoint.X + mExtents.MinPoint.X) * 0.5,
                (mExtents.MaxPoint.Y + mExtents.MinPoint.Y) * 0.5);

            // check if the width 'fits' in current window,
            // if not then get the new height as
            // per the viewports aspect ratio
            if (mWidth > mHeight * mScrRatio)
                mHeight = mWidth / mScrRatio;

            // set the view height - adjusted by view Identifier
            acCurVp.ViewHeight = mHeight * 1.25;

            // set the view center
            acCurVp.ViewCenter = mCentPt;
        }
    }

    internal class ImportedViewport
    {
        public Point3d InsertPoint;
        public double VHeight;
        public Vector3d ViewDirection;
        public double VWidth;

        public ImportedViewport(double height, double width, Point3d cen, Vector3d view)
        {
            VHeight = height;
            VWidth = width;
            InsertPoint = cen;
            ViewDirection = view;
        }
    }

    internal class LayoutSize
    {
        public double Height;
        public double Width;

        public LayoutSize(double width, double height)
        {
            Width = width;
            Height = height;
        }
    }

    /// <summary>
    ///     Internal class to hold view direction vectors
    /// </summary>
    internal static class ViewDirection
    {
        internal static Vector3d TopView = new Vector3d(0, 0, 1);
        internal static Vector3d BottomView = new Vector3d(0, 0, -1);
        internal static Vector3d FrontView = new Vector3d(0, -1, 0);
        internal static Vector3d BackView = new Vector3d(0, 1, 0);
        internal static Vector3d LeftView = new Vector3d(-1, 0, 0);
        internal static Vector3d RightView = new Vector3d(1, 0, 0);
        internal static Vector3d TopIsoSw = new Vector3d(-1, -1, 1); //Front - Left
        internal static Vector3d TopIsoSe = new Vector3d(1, -1, 1); //Front - Right
        internal static Vector3d TopIsoNw = new Vector3d(-1, 1, 1); //Back - Left
        internal static Vector3d TopIsoNe = new Vector3d(1, 1, 1); //Back - Right
        internal static Vector3d BottomIsoSw = new Vector3d(-1, -1, -1);
        internal static Vector3d BottomIsoSe = new Vector3d(1, -1, -1);
        internal static Vector3d BottomIsoNw = new Vector3d(-1, 1, -1);
        internal static Vector3d BottomIsoNe = new Vector3d(1, 1, -1);
    }
}