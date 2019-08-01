// -----------------------------------------------------------------------------------
//     <copyright file="RcDimAppend.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>04/08/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.Runtime;
using RabCab.Analysis;
using RabCab.Calculators;
using RabCab.Entities.Annotation;
using RabCab.Extensions;
using RabCab.Settings;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace RabCab.Commands.AnnotationSuite
{
    internal class RcDimDelete
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_DIMDELETE",
            CommandFlags.Modal
            //| CommandFlags.Transparent
            //| CommandFlags.UsePickSet
            //| CommandFlags.Redraw
            //| CommandFlags.NoPerspective
            //| CommandFlags.NoMultiple
            //| CommandFlags.NoTileMode
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
            //| CommandFlags.NoHistory
            //| CommandFlags.NoUndoMarker
            //| CommandFlags.NoBlockEditor
            //| CommandFlags.NoActionRecording
            //| CommandFlags.ActionMacro
            //| CommandFlags.NoInferConstraint 
        )]
        public void Cmd_DimDelete()
        {
            if (!Agents.LicensingAgent.Check()) return;
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            var prEntOpt = new PromptEntityOptions("\nSelect a dimension system to remove from: ");
            prEntOpt.SetRejectMessage("\nOnly linear dimensions may be selected.");
            prEntOpt.AllowNone = false;
            prEntOpt.AddAllowedClass(typeof(RotatedDimension), false);

            var prEntRes = acCurEd.GetEntity(prEntOpt);

            if (prEntRes.Status != PromptStatus.OK) return;

            var objId = prEntRes.ObjectId;
            var delMatrix = acCurEd.GetAlignedMatrix();

            var eqPoint = CalcTol.ReturnCurrentTolerance();

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                var acEnt = acTrans.GetObject(objId, OpenMode.ForWrite) as Entity;
                if (acEnt != null)
                {
                    acEnt.Unhighlight();

                    var acRotDim = acEnt as RotatedDimension;
                    if (acRotDim != null)
                    {
                        var dimSys = DimSystem.GetSystem(acRotDim, eqPoint, eqPoint);

                        var prPtOpts =
                            new PromptPointOptions("\nSelect point to delete or press CTRL to start a crossing line: ");
                        PromptPointResult ptRes;

                        while (true)
                        {
                            if (dimSys.Count <= 0) break;

                            dimSys.Highlight();

                            var nArray = DimSystem.GetActiveViewCount();

                            var ctManager = TransientManager.CurrentTransientManager;

                            var acCirc = new Circle();
                            var acPreview = new Circle();
                            var acLine = new Line(new Point3d(0, 0, 0), new Point3d(0, 0, 0));

                            acCirc.Color = SettingsUser.DynPreviewColor;
                            acPreview.Color = SettingsUser.DynPreviewColor;
                            acLine.Color = SettingsUser.DynPreviewColor;

                            var integerCollections = new IntegerCollection(nArray);

                            ctManager.AddTransient(acCirc, TransientDrawingMode.Main, 128,
                                integerCollections);
                            ctManager.AddTransient(acPreview, TransientDrawingMode.Main, 128,
                                integerCollections);
                            ctManager.AddTransient(acLine, TransientDrawingMode.Highlight, 128,
                                integerCollections);

                            var dPoints = dimSys.GetSystemPoints(eqPoint);

                            void PreviewHandler(object sender, PointMonitorEventArgs e)
                            {
                                var cdPoint = dimSys.GetNearest(e.Context.ComputedPoint, eqPoint);

                                var cPt = dPoints[cdPoint];
                                var dlPoint = cPt.DimLinePoint;
                                acCirc.Center = dlPoint;

                                var scrSize = ScreenReader.GetSreenSize();

                                acCirc.Radius = scrSize / 200;
                                acCirc.Normal = acRotDim.Normal;
                                acPreview.Radius = scrSize / 200;
                                acPreview.Normal = acRotDim.Normal;

                                Point3d tempPt;
                                Point3d tempPt1;
                                Point3d tempPt2;

                                if (cPt.IsLast)
                                {
                                    tempPt = cPt.Dim1PointIndex != 1
                                        ? cPt.Dim1.XLine2Point
                                        : cPt.Dim1.XLine1Point;
                                    tempPt2 = tempPt;
                                }
                                else
                                {
                                    tempPt = cPt.Dim1PointIndex != 1
                                        ? cPt.Dim1.XLine2Point
                                        : cPt.Dim1.XLine1Point;
                                    tempPt1 = cPt.Dim2PointIndex != 1
                                        ? cPt.Dim2.XLine2Point
                                        : cPt.Dim2.XLine1Point;
                                    tempPt2 = dlPoint.DistanceTo(tempPt) <= dlPoint.DistanceTo(tempPt1)
                                        ? tempPt1
                                        : tempPt;
                                }

                                acLine.StartPoint = dlPoint;
                                acLine.EndPoint = tempPt2;
                                acPreview.Center = tempPt2;

                                ctManager.UpdateTransient(acCirc, integerCollections);
                                ctManager.UpdateTransient(acPreview, integerCollections);
                                ctManager.UpdateTransient(acLine, integerCollections);
                            }

                            acCurEd.PointMonitor += PreviewHandler;

                            try
                            {
                                ptRes = acCurEd.GetPoint(prPtOpts);
                            }
                            finally
                            {
                                acCurEd.PointMonitor -= PreviewHandler;

                                ctManager.EraseTransient(acCirc, integerCollections);
                                ctManager.EraseTransient(acPreview, integerCollections);
                                ctManager.EraseTransient(acLine, integerCollections);

                                acCirc.Dispose();
                                acPreview.Dispose();
                                acLine.Dispose();
                            }

                            #region CTRL Modifier

                            var ctrlPressed = (Control.ModifierKeys & Keys.Control) > Keys.None;

                            PromptPointResult promptPointResult = null;

                            if (ctrlPressed)
                            {
                                var promptPointOption1 =
                                    new PromptPointOptions("\nSelect second point of crossing line:")
                                    {
                                        UseBasePoint = true,
                                        UseDashedLine = true,
                                        BasePoint = ptRes.Value
                                    };

                                promptPointResult = acCurEd.GetPoint(promptPointOption1);

                                if (promptPointResult.Status != PromptStatus.OK)
                                {
                                    dimSys.Unhighlight();
                                    break;
                                }
                            }

                            if (ptRes.Status != PromptStatus.OK)
                            {
                                dimSys.Unhighlight();
                                break;
                            }

                            #endregion

                            if (ctrlPressed)
                            {
                                var delPt1 = ptRes.Value.TransformBy(delMatrix);
                                var delPt2 = promptPointResult.Value.TransformBy(delMatrix);
                                dimSys.Delete(delPt1, delPt2);
                            }
                            else
                            {
                                dimSys.Delete(ptRes.Value.TransformBy(delMatrix));
                            }
                        }

                        dimSys.Unhighlight();
                    }
                }

                acTrans.Commit();
            }
        }
    }
}