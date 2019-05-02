using System;
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
    internal class RcDimExtend
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_DIMEXTEND",
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
        public void Cmd_DimExtend()
        {
            //Get the current document utilities
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            var prEntOpt = new PromptEntityOptions("\nSelect a dimension system to extend: ");

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
                        var dimSys = DimSystem.GetSystem(acRotDim, eqPoint, eqPoint);

                        dimSys.Highlight();

                        var promptPointOption2 =
                            new PromptPointOptions(
                                "\nSelect a dimension line to extend or press CTRL to start crossing line:");

                        while (true)
                        {
                            if (dimSys.Count == 0) break;

                            dimSys.Highlight();

                            var nArray = DimSystem.GetActiveViewCount();
                            var ctManager = TransientManager.CurrentTransientManager;

                            var acCirc = new Circle();
                            var acLine = new Line(new Point3d(0, 0, 0), new Point3d(0, 0, 0));

                            acCirc.Color = Colors.LayerColorPreview;
                            acCirc.Normal = acRotDim.Normal;

                            acLine.Color = Colors.LayerColorPreview;

                            var intCol = new IntegerCollection(nArray);

                            ctManager.AddTransient(acCirc, TransientDrawingMode.Highlight, 128,
                                intCol);
                            ctManager.AddTransient(acLine, TransientDrawingMode.Highlight, 128,
                                intCol);

                            var sysPts = dimSys.GetSystemPoints(eqPoint);

                            void Handler(object sender, PointMonitorEventArgs e)
                            {
                                var cPt = dimSys.GetNearest(e.Context.ComputedPoint, eqPoint);
                                var sysPt = sysPts[cPt];
                                var dlPt = sysPt.DimLinePoint;
                                var scrSize = ScreenReader.GetSreenSize();

                                acCirc.Radius = scrSize / 200;

                                var point3d = sysPt.Dim1PointIndex != 1
                                    ? sysPt.Dim1.XLine2Point
                                    : sysPt.Dim1.XLine1Point;

                                if (Math.Abs(point3d.DistanceTo(dlPt)) <= eqPoint)
                                {
                                    acCirc.Center = dlPt;
                                    acLine.StartPoint = dlPt;
                                    acLine.EndPoint = dlPt;
                                    ctManager.UpdateTransient(acCirc, intCol);
                                    ctManager.UpdateTransient(acLine, intCol);
                                    return;
                                }

                                var point = new Line3d(dlPt, point3d).GetClosestPointTo(e.Context.ComputedPoint).Point;
                                acLine.StartPoint = dlPt;
                                acLine.EndPoint = point;
                                acCirc.Center = point;
                                ctManager.UpdateTransient(acCirc, intCol);
                                ctManager.UpdateTransient(acLine, intCol);
                            }

                            acCurEd.PointMonitor += Handler;

                            PromptPointResult ptRes;
                            try
                            {
                                ptRes = acCurDoc.Editor.GetPoint(promptPointOption2);
                            }
                            finally
                            {
                                acCurEd.PointMonitor -= Handler;
                                ctManager.EraseTransient(acCirc, intCol);
                                ctManager.EraseTransient(acLine, intCol);
                                acCirc.Dispose();
                                acLine.Dispose();
                            }

                            var cntrlPressed = (Control.ModifierKeys & Keys.Control) > Keys.None;

                            PromptPointResult ctrlRes = null;
                            if (cntrlPressed)
                            {
                                var promptPointOption3 =
                                    new PromptPointOptions("\nSelect second point of crossing line:")
                                    {
                                        UseBasePoint = true,
                                        UseDashedLine = true,
                                        BasePoint = ptRes.Value
                                    };

                                ctrlRes = acCurDoc.Editor.GetPoint(promptPointOption3);
                                if (ctrlRes.Status != PromptStatus.OK) break;
                            }

                            if (ptRes.Status != PromptStatus.OK) break;

                            if (cntrlPressed)
                            {
                                var point3d6 = ptRes.Value.TransformBy(matrix3d);
                                var point3d7 = ctrlRes.Value.TransformBy(matrix3d);
                                var nums = dimSys.GetSystemByLine(point3d6, point3d7, eqPoint);

                                if (nums.Count <= 0) continue;

                                var sysPoints2 = dimSys.GetSystemPoints(eqPoint);
                                foreach (var num3 in nums)
                                {
                                    var point3d8 = DimSystem.GetCrossing(dimSys, sysPoints2, num3, point3d6,
                                        point3d7, eqPoint);
                                    if (point3d8.X != -99999 || point3d8.Y != -99999 || point3d8.Z != -99999)
                                        dimSys.Extend(num3, 0, point3d8, eqPoint);
                                    else
                                        acCurEd.WriteMessage("\nCannot extend lines with zero length.");
                                }

                                acTrans.TransactionManager.QueueForGraphicsFlush();
                            }
                            else
                            {
                                var point3d9 = ptRes.Value.TransformBy(matrix3d);
                                var num4 = dimSys.GetNearest(point3d9, eqPoint);
                                var sysPoint = dimSys.GetSystemPoints(eqPoint)[num4];
                                var point3d10 = sysPoint.DimLinePoint;
                                var point3d11 = sysPoint.Dim1PointIndex != 1
                                    ? sysPoint.Dim1.XLine2Point
                                    : sysPoint.Dim1.XLine1Point;
                                if (Math.Abs(point3d10.DistanceTo(point3d11)) >= eqPoint)
                                {
                                    dimSys.Extend(num4, 0, point3d9, eqPoint);
                                    acTrans.TransactionManager.QueueForGraphicsFlush();
                                }
                                else
                                {
                                    acCurEd.WriteMessage("\nCannot extend lines with zero length.");
                                }
                            }
                        }

                        dimSys.Unhighlight();
                        acTrans.Commit();
                    }
                }
            }
        }
    }
}