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

using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.Runtime;
using RabCab.Calculators;
using RabCab.Entities.Annotation;
using RabCab.Extensions;
using RabCab.Settings;

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
            var addMatrix = acCurEd.GetAlignedMatrix();

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
                            new PromptPointOptions("\nSelect point to add: ");

                        while (true)
                        {
                            dimSys.Highlight();

                            var dPoints = dimSys.GetSystemPoints(eqPoint);

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

                            var nArray = DimSystem.GetActiveViewCount();

                            var currentTransientManager = TransientManager.CurrentTransientManager;

                            var acLine = new Line(new Point3d(0, 0, 0), new Point3d(0, 0, 0));
                            var acPreview = new Line(new Point3d(0, 0, 0), new Point3d(0, 0, 0));

                            acLine.Color = SettingsUser.DynPreviewColor;
                            acPreview.Color = SettingsUser.DynPreviewColor;

                            var integerCollections = new IntegerCollection(nArray);

                            currentTransientManager.AddTransient(acLine, TransientDrawingMode.Main, 128,
                                integerCollections);
                            currentTransientManager.AddTransient(acPreview, TransientDrawingMode.Main, 128,
                                integerCollections);

                            void PointMonitorEventHandler(object sender, PointMonitorEventArgs e)
                            {
                                var point3dArray = DimSystem.GetSystemPoint(dimLinPt, pt, e.Context.ComputedPoint);
                                acLine.StartPoint = point3dArray[0];
                                acLine.EndPoint = point3dArray[1];

                                acPreview.StartPoint = point3dArray[1];
                                acPreview.EndPoint = e.Context.ComputedPoint;

                                currentTransientManager.UpdateTransient(acLine, integerCollections);
                                currentTransientManager.UpdateTransient(acPreview, integerCollections);
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

                                currentTransientManager.EraseTransient(acLine, integerCollections);
                                currentTransientManager.EraseTransient(acPreview, integerCollections);

                                acLine.Dispose();
                                acPreview.Dispose();
                            }

                            if (ptRes.Status != PromptStatus.OK) break;

                            dimSys.Insert(ptRes.Value.TransformBy(addMatrix));
                        }

                        dimSys.Unhighlight();
                    }
                }

                acTrans.Commit();
            }
        }
    }
}