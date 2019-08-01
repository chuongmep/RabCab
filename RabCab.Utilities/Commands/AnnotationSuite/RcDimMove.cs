using System.Collections.Generic;
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
    internal class RcDimMove
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_DIMMOVE",
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
        public void Cmd_DimMove()
        {
            if (!Agents.LicensingAgent.Check()) return;
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            var prEntOpt = new PromptEntityOptions("\nSelect a dimension system to move: ");

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
                            new PromptPointOptions("\nSelect new position for dimensions: ");

                        while (true)
                        {
                            dimSys.Highlight();

                            var nArray = DimSystem.GetActiveViewCount();
                            var ctManager = TransientManager.CurrentTransientManager;
                            var intCol = new IntegerCollection(nArray);
                            var rotatedDimensions = new List<RotatedDimension>();

                            foreach (var dim in dimSys.SysList)
                            {
                                var dimClone = (RotatedDimension) dim.Clone();
                                ctManager.AddTransient(dimClone, TransientDrawingMode.Highlight, 128, intCol);
                                rotatedDimensions.Add(dimClone);
                            }

                            void Handler(object sender, PointMonitorEventArgs e)
                            {
                                foreach (var tDim in rotatedDimensions)
                                {
                                    if (!tDim.UsingDefaultTextPosition)
                                    {
                                        var dimLinePoint = tDim.DimLinePoint;
                                        var textPosition = tDim.TextPosition;
                                        tDim.DimLinePoint = e.Context.ComputedPoint;
                                        ctManager.UpdateTransient(tDim, intCol);
                                        tDim.TextPosition =
                                            textPosition.Add(dimLinePoint.GetVectorTo(tDim.DimLinePoint));
                                    }
                                    else
                                    {
                                        tDim.DimLinePoint = e.Context.ComputedPoint;
                                    }

                                    ctManager.UpdateTransient(tDim, intCol);
                                }
                            }

                            acCurEd.PointMonitor += Handler;
                            PromptPointResult prRes;

                            try
                            {
                                prRes = acCurEd.GetPoint(prPtOpts);
                            }
                            finally
                            {
                                acCurEd.PointMonitor -= Handler;

                                foreach (var acRotRim in rotatedDimensions)
                                {
                                    ctManager.EraseTransient(acRotRim, intCol);
                                    acRotRim.Dispose();
                                }
                            }

                            if (prRes.Status != PromptStatus.OK) break;

                            dimSys.MoveSystem(prRes.Value.TransformBy(addMatrix), eqPoint);
                            acTrans.TransactionManager.QueueForGraphicsFlush();
                        }

                        dimSys.Unhighlight();
                    }
                }

                acTrans.Commit();
            }
        }
    }
}