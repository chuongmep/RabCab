using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.Runtime;
using RabCab.Analysis;
using RabCab.Calculators;
using RabCab.Entities.Annotation;
using RabCab.Settings;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace RabCab.Commands.AnnotationSuite
{
    class RcDimExtend
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

            PromptPointResult promptPointResult;
            PromptPointResult promptPointResult1;
            Entities.Annotation.DimSystem dimSys;
            Document mdiActiveDocument = Application.DocumentManager.MdiActiveDocument;
            Database database = mdiActiveDocument.Database;
            Editor editor = mdiActiveDocument.Editor;
            PromptEntityOptions promptEntityOption = new PromptEntityOptions("\nSelect linear dimension: ");
            promptEntityOption.SetRejectMessage("\nOnly Rotated Dimension");
            promptEntityOption.AllowNone = false;
            promptEntityOption.AddAllowedClass(typeof(RotatedDimension), false);
            PromptEntityResult entity = editor.GetEntity(promptEntityOption);
            if (entity.Status == PromptStatus.Cancel)
            {
                return;
            }
            ObjectId objectId = entity.ObjectId;
            CoordinateSystem3d coordinateSystem3d = mdiActiveDocument.Editor.CurrentUserCoordinateSystem.CoordinateSystem3d;
            Matrix3d matrix3d = Matrix3d.AlignCoordinateSystem(Point3d.Origin, Vector3d.XAxis, Vector3d.YAxis, Vector3d.ZAxis, coordinateSystem3d.Origin, coordinateSystem3d.Xaxis, coordinateSystem3d.Yaxis, coordinateSystem3d.Zaxis);
            double EqPoint = CalcTol.ReturnCurrentTolerance();
            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                Entity obj = (Entity)transaction.GetObject(objectId, OpenMode.ForWrite);
                obj.Unhighlight();
                RotatedDimension rotatedDimension = (RotatedDimension)obj;
                dimSys = new DimSystem();
                dimSys = DimSystem.GetSystem(rotatedDimension, EqPoint, EqPoint);
                
                dimSys.Highlight();
               
                
                    PromptPointOptions promptPointOption2 = new PromptPointOptions("\nSelect new extend of dimension or press CTRL to start crossing line:");
                    while (true)
                    {
                        if (dimSys.Count == 0)
                        {
                            goto Label1;
                        }
                        dimSys.Highlight();
                        int[] numArray1 = DimSystem.GetActiveViewCount();
                        TransientManager transientManager = TransientManager.CurrentTransientManager;
                        Circle normal = new Circle();
                        Line line1 = new Line(new Point3d(0, 0, 0), new Point3d(0, 0, 0));
                        normal.Color = Colors.LayerColorPreview;
                        normal.Normal = rotatedDimension.Normal;
                        line1.Color = Colors.LayerColorPreview;
                        IntegerCollection integerCollections1 = new IntegerCollection(numArray1);
                        transientManager.AddTransient(normal, TransientDrawingMode.Highlight, 128, integerCollections1);
                        transientManager.AddTransient(line1, TransientDrawingMode.Highlight, 128, integerCollections1);
                        List<SysPoint> sysPoints1 = dimSys.GetSystemPoints(EqPoint);
                        PointMonitorEventHandler pointMonitorEventHandler1 = (object sender, PointMonitorEventArgs e) =>
                        {
                            int closestsysPoint = dimSys.GetNearest(e.Context.ComputedPoint, EqPoint);
                            SysPoint item = sysPoints1[closestsysPoint];
                            Point3d dimLinePoint = item.DimLinePoint;
                            double sreenSize = ScreenReader.GetSreenSize();
                            normal.Radius = sreenSize / 200;
                            Point3d point3d = new Point3d();
                            Point3d point = new Point3d();
                            point3d = (item.Dim1PointIndex != 1 ? item.Dim1.XLine2Point : item.Dim1.XLine1Point);
                            if (Math.Abs(point3d.DistanceTo(dimLinePoint)) <= EqPoint)
                            {
                                normal.Center = dimLinePoint;
                                line1.StartPoint = dimLinePoint;
                                line1.EndPoint = dimLinePoint;
                                transientManager.UpdateTransient(normal, integerCollections1);
                                transientManager.UpdateTransient(line1, integerCollections1);
                                return;
                            }
                            point = (new Line3d(dimLinePoint, point3d)).GetClosestPointTo(e.Context.ComputedPoint).Point;
                            line1.StartPoint = dimLinePoint;
                            line1.EndPoint = point;
                            normal.Center = point;
                            transientManager.UpdateTransient(normal, integerCollections1);
                            transientManager.UpdateTransient(line1, integerCollections1);
                        };
                        editor.PointMonitor += pointMonitorEventHandler1;
                        try
                        {
                            promptPointResult1 = mdiActiveDocument.Editor.GetPoint(promptPointOption2);
                        }
                        finally
                        {
                            editor.PointMonitor -= pointMonitorEventHandler1;
                            transientManager.EraseTransient(normal, integerCollections1);
                            transientManager.EraseTransient(line1, integerCollections1);
                            normal.Dispose();
                            line1.Dispose();
                        }
                        bool flag = (Control.ModifierKeys & Keys.Control) > Keys.None;
                        PromptPointResult promptPointResult3 = null;
                        if (flag)
                        {
                            PromptPointOptions promptPointOption3 = new PromptPointOptions("\nSelect second point of crossing line:")
                            {
                                UseBasePoint = true,
                                UseDashedLine = true,
                                BasePoint = promptPointResult1.Value
                            };
                            promptPointResult3 = mdiActiveDocument.Editor.GetPoint(promptPointOption3);
                            if (promptPointResult3.Status != PromptStatus.OK)
                            {
                                goto Label1;
                            }
                        }
                        if (promptPointResult1.Status != PromptStatus.OK)
                        {
                            break;
                        }
                        if (flag)
                        {
                            Point3d point3d6 = promptPointResult1.Value.TransformBy(matrix3d);
                            Point3d point3d7 = promptPointResult3.Value.TransformBy(matrix3d);
                            List<int> nums = dimSys.GetSystemByLine(point3d6, point3d7, EqPoint);
                            if (nums.Count > 0)
                            {
                                List<SysPoint> sysPoints2 = dimSys.GetSystemPoints(EqPoint);
                                foreach (int num3 in nums)
                                {
                                    Point3d point3d8 = DimSystem.GetCrossing(dimSys, sysPoints2, num3, point3d6, point3d7, EqPoint);
                                    if (point3d8.X != -99999 || point3d8.Y != -99999 || point3d8.Z != -99999)
                                    {
                                        dimSys.Extend(num3, 0, point3d8, EqPoint);
                                    }
                                    else
                                    {
                                        editor.WriteMessage("\nExtension line has zero length. Cannot extend it");
                                    }
                                }
                                transaction.TransactionManager.QueueForGraphicsFlush();
                            }
                        }
                        else
                        {
                            Point3d point3d9 = promptPointResult1.Value.TransformBy(matrix3d);
                            int num4 = dimSys.GetNearest(point3d9, EqPoint);
                            SysPoint sysPoint = dimSys.GetSystemPoints(EqPoint)[num4];
                            Point3d point3d10 = sysPoint.DimLinePoint;
                            Point3d point3d11 = new Point3d();
                            point3d11 = (sysPoint.Dim1PointIndex != 1 ? sysPoint.Dim1.XLine2Point : sysPoint.Dim1.XLine1Point);
                            if (Math.Abs(point3d10.DistanceTo(point3d11)) >= EqPoint)
                            {
                                dimSys.Extend(num4, 0, point3d9, EqPoint);
                                transaction.TransactionManager.QueueForGraphicsFlush();
                            }
                            else
                            {
                                editor.WriteMessage("\nExtension line has zero length. Cannot extend it");
                            }
                        }
                    }
                    dimSys.Unhighlight();
                
            Label1:
                dimSys.Unhighlight();
                transaction.Commit();
            }
        }
    }
}
