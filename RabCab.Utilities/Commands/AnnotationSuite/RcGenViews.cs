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
using System.Linq;
using System.Threading;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using RabCab.Agents;
using RabCab.Calculators;
using RabCab.Engine.Enumerators;
using RabCab.Extensions;
using RabCab.Settings;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
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
            if (!LicensingAgent.Check()) return;
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

                var pKeyOpts = new PromptKeywordOptions(string.Empty)
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
                var layoutName = string.Empty;

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
                    MailAgent.Report(e.Message);
                }

                if (modelView != null) modelView.Dispose();
                acTrans.Commit();
            }
        }

        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_GENPARTS",
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
        public void Cmd_PartsGen()
        {
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;

            if (acCurDoc == null) return;

            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;
            var modelView = acCurEd.GetCurrentView().Clone() as ViewTableRecord;

            var objIds = acCurEd.GetFilteredSelection(Enums.DxfNameEnum._3Dsolid, false);
            if (objIds.Length <= 0) return;

            var splitNum = 8;
            var colCount = splitNum / 2;
            var rowCount = 2;

            var partList = new List<Solid3d>();

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                foreach (var ob in objIds)
                {
                    var acSol = acTrans.GetObject(ob, OpenMode.ForRead) as Solid3d;
                    if (acSol == null) continue;
                    partList.Add(acSol);
                }

                acTrans.Commit();
            }

            var sortedParts = partList.OrderBy(e => e.GetPartName()).ToList();

            var objChunks = sortedParts.ChunkBy(splitNum);

            var layName = "Model";

            var vPorts = new List<Viewport>();

            using (var pWorker = new ProgressAgent("Creating Part Views: ", sortedParts.Count))

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                for (var i = 0; i < objChunks.Count; i++)
                {
                    var curLayout = "PrtGen";
                    var id = LayoutManager.Current.CreateAndMakeLayoutCurrentByAddition(curLayout);
                    // Open the created layout
                    var lay = (Layout) acTrans.GetObject(id, OpenMode.ForWrite);

                    // Open the Block table record Paper space for write
                    var acBlkTableRec = acTrans.GetObject(lay.BlockTableRecordId,
                        OpenMode.ForWrite) as BlockTableRecord;

                    // Make some settings on the layout and get its extents
                    lay.SetPlotSettings(
                        //"ISO_full_bleed_2A0_(1189.00_x_1682.00_MM)", // Try this big boy!
                        // "ANSI_B_(11.00_x_17.00_Inches)",
                        "ARCH_full_bleed_D_(36.00_x_24.00_Inches)",
                        "monochrome.ctb",
                        "AutoCAD PDF (High Quality Print).pc3"
                    );

                    if (acBlkTableRec != null)
                    {
                        foreach (var vpId in acBlkTableRec)
                        {
                            if (vpId == acCurDb.PaperSpaceVportId) continue;
                            var vp = acTrans.GetObject(vpId, OpenMode.ForRead) as Viewport;
                            if (vp != null) vPorts.Add(vp);
                        }

                        var layOutSize = GetLayoutSize(lay);
                        var paperLength = layOutSize.Width;
                        var paperHeight = layOutSize.Height;

                        var dSpacing = CalcUnit.GetProportion(1.5, 36, paperLength);
                        var dBorder = CalcUnit.GetProportion(1.5, 36, paperLength);
                        var availWidth = paperLength - dBorder * 2.5 - dSpacing * (colCount + 1);
                        var availHeight = paperHeight - dBorder * 2.25 - dSpacing * (rowCount + 1);

                        var dWidth = availWidth / colCount;
                        var dWHalf = dWidth / 2;
                        var dHeight = availHeight / rowCount;
                        var dhHalf = dHeight / 2;

                        var col1 = dBorder + dWHalf + dSpacing;
                        var col2 = dBorder + dWHalf + dSpacing * 2 + dWidth;
                        var col3 = dBorder + dWHalf + dSpacing * 3 + dWidth * 2;
                        var col4 = dBorder + dWHalf + dSpacing * 4 + dWidth * 3;
                        //var col5 = dBorder + dWHalf + dSpacing * 5 + dWidth * 4;

                        var row1 = paperHeight - dhHalf - dBorder - dSpacing;
                        var row2 = paperHeight - dhHalf - dBorder - dSpacing * 2 - dHeight;
                        //row3 = paperHeight - dhHalf - dBorder - dSpacing * 3 - dHeight * 2;
                        //row4 = paperHeight - dhHalf - dBorder - dSpacing * 4 - dHeight * 3;

                        var positions = new[]
                        {
                            new Point2d(col1, row1),
                            new Point2d(col2, row1),
                            new Point2d(col3, row1),
                            new Point2d(col4, row1),
                            new Point2d(col1, row2),
                            new Point2d(col2, row2),
                            new Point2d(col3, row2),
                            new Point2d(col4, row2)
                        };

                        for (var j = 0; j < objChunks[i].Count; j++)
                        {
                            //Progress progress bar or exit if ESC has been pressed
                            if (!pWorker.Progress())
                            {
                                acTrans.Abort();
                                return;
                            }

                            var acSol = acTrans.GetObject(objChunks[i][j].ObjectId, OpenMode.ForWrite) as Solid3d;
                            if (acSol == null) continue;
                            var acExt = acSol.GeometricExtents;
                            var acVport = new Viewport
                            {
                                CenterPoint = positions[j].Convert3D(),
                                Width = dWidth,
                                Height = dHeight,
                                ViewDirection = ViewDirection.TopView
                            };

                            acCurDb.AppendEntity(acVport, acTrans);

                            acVport.FitContentToViewport(acExt);
                            acVport.FitViewportToContent(acExt);
                            acVport.CenterPoint = positions[j].Convert3D();
                            acVport.UpdateDisplay();
                            acVport.Layer = "Defpoints";
                            // Enable the viewport
                            acVport.Visible = true;
                            acVport.On = true;

                            //acVport.CreateBaseViewFromVp(acSol.ObjectId, acCurEd, acCurDb, lay, acVport.CenterPoint);

                            System.Threading.Thread.Sleep(300);

                            vPorts.Add(acVport);

                            //Insert Part Tags
                            //Insert block to name the viewport
                            var insertMin = acVport.GeometricExtents.MinPoint;
                            var insertMax = acVport.GeometricExtents.MaxPoint;

                            var mid = insertMin.GetMidPoint(insertMax);
                            var midInsert = new Point3d(mid.X, positions[j].Convert3D().Y - 1, 0);

                            // Open the Block table for read
                            var acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForWrite) as BlockTable;

                            if (!acBlkTbl.Has(SettingsInternal.TagName))
                            {
                                MakeTag_Name(acBlkTbl, acTrans);
                            }

                            var blockName = SettingsInternal.TagName;

                            var acBlkRef = acBlkTableRec.InsertBlock(blockName, midInsert, acCurDb);

                            if (acBlkRef != null)
                            {
                                UpdatePartViewSubs(acBlkRef, acSol, acCurDoc);
                            }
                        }
                    }

                    layName = lay.LayoutName;
                }

                //Save the new objects to the database
                LayoutManager.Current.CurrentLayout = "MODEL";
                using (var view = acCurEd.GetCurrentView())
                {
                    view.CopyFrom(modelView);
                    acCurEd.SetCurrentView(view);
                }
                try
                {
                    Thread.Sleep(300);
                    LayoutManager.Current.CurrentLayout = layName;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                if (modelView != null) modelView.Dispose();

                // Zoom so that we can see our new layout, again with a little padding
                acCurEd.Command("_.ZOOM", "_E");
                acCurEd.Command("_.ZOOM", ".7X");
                acCurEd.Regen();

                // Commit the transaction
                acTrans.Commit();
            }

            //using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            //{
            //    foreach (var acVp in vPorts)
            //    {
            //        if (acVp.ObjectId == acCurDb.PaperSpaceVportId) continue;

            //        var delVp = acTrans.GetObject(acVp.ObjectId, OpenMode.ForWrite) as Viewport;
            //        delVp.Erase();
            //        delVp.Dispose();
            //    }

            //    acTrans.Commit();
            //}

        }

        public void UpdatePartViewSubs(BlockReference acBlkRef, Entity acEnt, Document acCurDoc)
        {
            acBlkRef?.UpdateAttributeBySubstitution("LAYER", acEnt.Layer.ToUpper(), acCurDoc, acCurDoc.Editor);
            acBlkRef?.UpdateAttributeBySubstitution("NAME", acEnt.GetPartName(), acCurDoc, acCurDoc.Editor);
            acBlkRef?.UpdateAttributeBySubstitution("MATERIAL", acEnt.Material.ToUpper(), acCurDoc, acCurDoc.Editor);
            acBlkRef?.UpdateAttributeBySubstitution("QTY", acEnt.GetQtyTotal().ToString(), acCurDoc, acCurDoc.Editor);
        }


        // Returns whether the provided DB extents - retrieved from
        // Database.Extmin/max - are "valid" or whether they are the default
        // invalid values (where the min's coordinates are positive and the
        // max coordinates are negative)


        /// <summary>
        ///     Method to create a Name Tag
        /// </summary>
        /// <param name="acBlkTbl"></param>
        /// <param name="acTrans"></param>
        /// <param name="tailLength"></param>
        /// <returns></returns>
        public ObjectId MakeTag_Name(BlockTable acBlkTbl, Transaction acTrans, double tailLength = 4)
        {
            //Create rectangle with filleted edges to hold atts
            //Create line to go under atts
            //Create att for View, Part, Scale, Qty

            var halfTail = tailLength / 2;
            const double boxHeight = 0.3125;
            const double halfBoxHeight = boxHeight / 2;
            const double boxLength = 0.5625;
            const double halfBoxLength = boxLength / 2;

            //Create housing box
            var boxPoly1 = new Point2d(-halfTail, -halfBoxHeight);
            var boxPoly2 = new Point2d(-halfTail, halfBoxHeight);
            var boxPoly3 = new Point2d(-halfTail + boxLength, halfBoxHeight);
            var boxPoly4 = new Point2d(-halfTail + boxLength, -halfBoxHeight);

            var boxPoly = new Polyline();

            boxPoly.AddVertexAt(0, boxPoly1, 0, 0, 0);
            boxPoly.AddVertexAt(1, boxPoly2, 0, 0, 0);
            boxPoly.AddVertexAt(2, boxPoly3, 0, 0, 0);
            boxPoly.AddVertexAt(3, boxPoly4, 0, 0, 0);
            boxPoly.Closed = true;

            boxPoly.FilletAll(0.03125);

            //Create Divider Line
            var divPoly1 = new Point2d(-halfTail, 0);
            var divPoly2 = new Point2d(halfTail, 0);

            var divPoly = new Polyline();

            divPoly.AddVertexAt(0, divPoly1, 0, 0, 0);
            divPoly.AddVertexAt(0, divPoly2, 0, 0, 0);
            divPoly.Closed = false;

            var textMount = halfBoxHeight / 2;
            var textHeight = .06;

            var attView = new AttributeDefinition
            {
                Justify = AttachmentPoint.BottomLeft,
                AlignmentPoint = new Point3d(-halfTail + boxLength + 0.046875, textMount - textHeight / 2, 0),
                Prompt = "LAYER:",
                Tag = "Layer",
                TextString = "[LAYER]",
                Height = textHeight,
                LockPositionInBlock = true
            };

            var attScale = new AttributeDefinition
            {
                Justify = AttachmentPoint.TopLeft,
                AlignmentPoint = new Point3d(-halfTail + boxLength + 0.046875, -textMount + textHeight / 2, 0),
                Prompt = "MATERIAL:",
                Tag = "MATERIAL",
                TextString = "FINISH: " + "[MATERIAL]",
                Height = textHeight,
                LockPositionInBlock = true
            };

            //Create Attributes
            var attPart = new AttributeDefinition
            {
                Justify = AttachmentPoint.MiddleCenter,
                AlignmentPoint = new Point3d(-halfTail + halfBoxLength, textMount, 0),
                Prompt = "NAME:",
                Tag =  "NAME",
                TextString = "[NAME]",
                Height = textHeight,
                LockPositionInBlock = true
            };

            var attQty = new AttributeDefinition
            {
                Justify = AttachmentPoint.MiddleCenter,
                AlignmentPoint = new Point3d(-halfTail + halfBoxLength, -textMount, 0),
                Prompt = "QTY:",
                Tag ="QTY",
                TextString = "QTY: " + "[QTY]",
                Height = textHeight,
                LockPositionInBlock = true
            };

            var ents = new Entity[] { boxPoly, divPoly };
            var atts = new[] { attView, attScale, attPart, attQty };

            var acBlkTblRec = MakeBlock(SettingsInternal.TagName, ents, atts);

            if (!acBlkTbl.IsWriteEnabled)
                acBlkTblRec.UpgradeOpen();

            acBlkTbl.Add(acBlkTblRec);
            acTrans.AddNewlyCreatedDBObject(acBlkTblRec, true);

            return acBlkTblRec.ObjectId;
        }

        private bool ValidDbExtents(Point3d min, Point3d max)

        {
            return
                !(min.X > 0 && min.Y > 0 && min.Z > 0 &&
                  max.X < 0 && max.Y < 0 && max.Z < 0);
        }

        /// <summary>
        ///     Utility method to create a block
        /// </summary>
        /// <param name="name"></param>
        /// <param name="ents"></param>
        /// <param name="atts"></param>
        private static BlockTableRecord MakeBlock(string name, IEnumerable<Entity> ents,
            AttributeDefinition[] atts = null)
        {
            var acBlkTblRec = new BlockTableRecord
            {
                Name = name,
                Origin = Point3d.Origin
            };

            foreach (var ent in ents)
            {
                acBlkTblRec.AppendEntity(ent);
            }

            if (atts == null) return acBlkTblRec;

            foreach (var acAttDef in atts)
            {
                acBlkTblRec.AppendEntity(acAttDef);
            }

            return acBlkTblRec;
        }

        private void CopyLayout(Database sourceDB,
            string layoutName, Database thisDB, ObjectId thisLayoutID)
        {
            using (var tran = sourceDB.TransactionManager.StartTransaction())
            {
                var lay1Dic = (DBDictionary) tran.GetObject(sourceDB.LayoutDictionaryId, OpenMode.ForRead);
                try
                {
                    //Get source layout object in the source database
                    var lay1ID = lay1Dic.GetAt(layoutName);
                    var lay1 = (Layout) tran.GetObject(lay1ID, OpenMode.ForRead);

                    using (var t = thisDB.TransactionManager.StartTransaction())
                    {
                        //Get the destination layout in current database
                        var lay2 = (Layout) t.GetObject(thisLayoutID, OpenMode.ForWrite);
                        try
                        {
                            //Copy layout
                            lay2.CopyFrom(lay1);
                            t.Commit();
                        }
                        catch
                        {
                            t.Abort();
                            throw;
                        }
                    }
                }
                catch
                {
                    tran.Abort();
                    throw;
                }
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

                        var pKeyOpts = new PromptKeywordOptions(string.Empty)
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
                        var layoutName = string.Empty;

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
                MailAgent.Report(e.Message);
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


        /// <summary>
        ///     Method to zoom a viewport to the extents of an input entity
        /// </summary>
        /// <param name="acCurDb"></param>
        /// <param name="acCurVp"></param>
        /// <param name="acEnt"></param>
        private void ZoomViewport(Database acCurDb, Viewport acCurVp, Extents3d ext)
        {
            // get the screen aspect ratio to calculate
            // the height and width
            // width/height
            var mScrRatio = acCurVp.Width / acCurVp.Height;
            var mMaxExt = acCurDb.Extmax;
            var mMinExt = acCurDb.Extmin;

            if (ext != null)
            {
                mMaxExt = ext.MaxPoint;
                mMinExt = ext.MinPoint;
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