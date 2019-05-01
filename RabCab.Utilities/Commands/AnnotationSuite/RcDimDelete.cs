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
            //Get the current document utilities
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
            var matrix3d = acCurEd.GetAlignedMatrix();

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
                        var dimSys = DimSystem.GetDimSystem(acRotDim, eqPoint, eqPoint);

                        var prPtOpts =
                            new PromptPointOptions("\nSelect point to delete or press CTRL to start a crossing line: ");
                        PromptPointResult ptRes;

                        while (true)
                        {
                            if (dimSys.SystemCount <= 0) break;

                            dimSys.Highlight();

                            var nArray = DimSystem.ViewportNumbers();

                            var currentTransientManager = TransientManager.CurrentTransientManager;
                            var circle = new Circle();
                            var dynPreviewColor = new Circle();
                            var line = new Line(new Point3d(0, 0, 0), new Point3d(0, 0, 0));
                            circle.Color = Colors.LayerColorPreview;
                            dynPreviewColor.Color = Colors.LayerColorPreview;
                            line.Color = Colors.LayerColorPreview;
                            var integerCollections = new IntegerCollection(nArray);
                            currentTransientManager.AddTransient(circle, TransientDrawingMode.Main, 128,
                                integerCollections);
                            currentTransientManager.AddTransient(dynPreviewColor, TransientDrawingMode.Main, 128,
                                integerCollections);
                            currentTransientManager.AddTransient(line, TransientDrawingMode.Highlight, 128,
                                integerCollections);

                            var dimSetPoints = dimSys.GetDimPoints(eqPoint);

                            PointMonitorEventHandler pointMonitorEventHandler = (sender, e) =>
                            {
                                var closestDimSetPoint = dimSys.GetClosestDimPoint(e.Context.ComputedPoint, eqPoint);
                                var item = dimSetPoints[closestDimSetPoint];
                                var dimLinePoint = item.DimLinePoint;
                                {
                                    circle.Center = dimLinePoint;
                                    var sreenSize = ScreenReader.GetSreenSize();
                                    circle.Radius = sreenSize / 200;
                                    circle.Normal = acRotDim.Normal;
                                    dynPreviewColor.Radius = sreenSize / 200;
                                    dynPreviewColor.Normal = acRotDim.Normal;
                                    Point3d point3d;
                                    Point3d point3d1;
                                    Point3d point3d2;
                                    if (item.IsLast)
                                    {
                                        point3d = item.Dim1PointIndex != 1
                                            ? item.Dim1.XLine2Point
                                            : item.Dim1.XLine1Point;
                                        point3d2 = point3d;
                                    }
                                    else
                                    {
                                        point3d = item.Dim1PointIndex != 1
                                            ? item.Dim1.XLine2Point
                                            : item.Dim1.XLine1Point;
                                        point3d1 = item.Dim2PointIndex != 1
                                            ? item.Dim2.XLine2Point
                                            : item.Dim2.XLine1Point;
                                        point3d2 = dimLinePoint.DistanceTo(point3d) <= dimLinePoint.DistanceTo(point3d1)
                                            ? point3d1
                                            : point3d;
                                    }

                                    line.StartPoint = dimLinePoint;
                                    line.EndPoint = point3d2;
                                    dynPreviewColor.Center = point3d2;
                                    currentTransientManager.UpdateTransient(circle, integerCollections);
                                }

                                currentTransientManager.UpdateTransient(dynPreviewColor, integerCollections);
                                currentTransientManager.UpdateTransient(line, integerCollections);
                            };

                            acCurEd.PointMonitor += pointMonitorEventHandler;

                            try
                            {
                                ptRes = acCurEd.GetPoint(prPtOpts);
                            }
                            finally
                            {
                                acCurEd.PointMonitor -= pointMonitorEventHandler;
                                currentTransientManager.EraseTransient(circle, integerCollections);
                                currentTransientManager.EraseTransient(dynPreviewColor, integerCollections);
                                currentTransientManager.EraseTransient(line, integerCollections);
                                circle.Dispose();
                                dynPreviewColor.Dispose();
                                line.Dispose();
                            }

                            var modifierKeys = (Control.ModifierKeys & Keys.Control) > Keys.None;
                            PromptPointResult promptPointResult = null;
                            if (modifierKeys)
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

                            if (modifierKeys)
                            {
                                var point3d3 = ptRes.Value.TransformBy(matrix3d);
                                var point3d4 = promptPointResult.Value.TransformBy(matrix3d);
                                dimSys.DeletePointByLine(point3d3, point3d4);
                            }
                            else
                            {
                                dimSys.DeletePointByPoint(ptRes.Value.TransformBy(matrix3d));
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