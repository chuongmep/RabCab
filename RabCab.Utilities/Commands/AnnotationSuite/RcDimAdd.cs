// -----------------------------------------------------------------------------------
//     <copyright file="RcDimJoin.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>04/11/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.Runtime;
using RabCab.Entities.Annotation;
using RabCab.Extensions;
using RabCab.Settings;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace RabCab.Commands.AnnotationSuite
{
    internal class RcDimAdd
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_DIMADD",
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
        public void Cmd_DimAdd()
        {
            //Get the current document utilities
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            var prEntOpt = new PromptEntityOptions("\nSelect a dimension system to add to: ");
            prEntOpt.SetRejectMessage("\nOnly linear dimensions may be selected.");
            prEntOpt.AllowNone = false;
            prEntOpt.AddAllowedClass(typeof(RotatedDimension), false);

            var prEntRes = acCurEd.GetEntity(prEntOpt);

            if (prEntRes.Status != PromptStatus.OK) return;

            var objId = prEntRes.ObjectId;
            var matrix3d = acCurEd.GetAlignedMatrix();

            var sysSettings = DimSystemSettings.GetDimSystemSettings();
            var eqPoint = sysSettings.EqPoint;

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
                            new PromptPointOptions("\nSelect point to add: ");

                        while (true)
                        {
                            dimSys.Highlight();

                            var dPoints = dimSys.GetDimPoints(eqPoint);

                            var dimLinPt = new Point3d();
                            var pt = new Point3d();
                            var bl = false;

                            foreach (var dPt in dPoints)
                            {
                                if (bl)
                                {
                                    pt = dPt.DimLinePoint;
                                    break;
                                }

                                dimLinPt = dPt.DimLinePoint;
                                bl = true;
                            }

                            var nArray = DimSystem.ViewportNumbers();

                            var currentTransientManager = TransientManager.CurrentTransientManager;
                            Line line = new Line(new Point3d(0, 0, 0), new Point3d(0, 0, 0));
                            Line dynPreviewColor = new Line(new Point3d(0, 0, 0), new Point3d(0, 0, 0));
                            line.ColorIndex = sysSettings.DynPreviewColor;
                            dynPreviewColor.ColorIndex = sysSettings.DynPreviewColor;
                            line.ColorIndex = sysSettings.DynPreviewColor;
                            IntegerCollection integerCollections = new IntegerCollection(nArray);

                            currentTransientManager.AddTransient(line, TransientDrawingMode.Main, 128,
                                integerCollections);
                            currentTransientManager.AddTransient(dynPreviewColor, TransientDrawingMode.Main, 128,
                                integerCollections);

                            void PointMonitorEventHandler(object sender, PointMonitorEventArgs e)
                            {
                                Point3d[] point3dArray = DimSystem.zGetPointOnDimSet(dimLinPt, pt, e.Context.ComputedPoint);
                                line.StartPoint = point3dArray[0];
                                line.EndPoint = point3dArray[1];
                                dynPreviewColor.EndPoint = e.Context.ComputedPoint;
                                dynPreviewColor.StartPoint = point3dArray[1];
                                currentTransientManager.UpdateTransient(line, integerCollections);
                                currentTransientManager.UpdateTransient(dynPreviewColor, integerCollections);
                            }

                            acCurEd.PointMonitor += PointMonitorEventHandler;

                            PromptPointResult ptRes;
                            try
                            {
                                ptRes = acCurEd.GetPoint(prPtOpts);
                            }
                            finally
                            {
                                acCurEd.PointMonitor -= PointMonitorEventHandler;
                                currentTransientManager.EraseTransient(line, integerCollections);
                                currentTransientManager.EraseTransient(dynPreviewColor, integerCollections);
                                line.Dispose();
                                dynPreviewColor.Dispose();
                            }

                            if (ptRes.Status != PromptStatus.OK)
                            {
                                break;
                            }

                            dimSys.InsertPoint(ptRes.Value.TransformBy(matrix3d), sysSettings);
                        }

                        dimSys.Unhighlight();
                    }
                }

                acTrans.Commit();
            }
        }

    }
}