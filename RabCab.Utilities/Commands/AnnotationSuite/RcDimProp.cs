using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.Runtime;
using RabCab.Analysis;
using RabCab.Entities.Annotation;
using RabCab.Gui.DimSystem;
using RabCab.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using RabCab.Calculators;
using static Autodesk.AutoCAD.ApplicationServices.Application;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace RabCab.Commands.AnnotationSuite
{
    class RcDimProp
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_DIMPROP",
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
        public void Cmd_DimProp()
        {
            //Get the current document utilities
            var acCurDoc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            PromptPointResult point;

            var myDialogPointProp = new DimSystemProperties();

            ShowModalDialog(null, myDialogPointProp, false);
            if (!myDialogPointProp.clickedOK)
            {
                return;
            }


            bool @checked = myDialogPointProp.chBoxArrowheadsOption.Checked;
            string arrowheadBlkName = myDialogPointProp.ArrowheadBlkName;
            bool flag = myDialogPointProp.chBoxExtLineOption.Checked;
            bool checked1 = myDialogPointProp.chBoxSuppressExtLine.Checked;
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
            double equalPointDistance = CalcTol.ReturnCurrentTolerance();
            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                Entity obj = (Entity)transaction.GetObject(objectId, OpenMode.ForWrite);
                obj.Unhighlight();
                RotatedDimension rotatedDimension = (RotatedDimension)obj;
                DimSystem dimSet = new DimSystem();
                dimSet = DimSystem.GetDimSystem(rotatedDimension, equalPointDistance, equalPointDistance);
                editor.WriteMessage(string.Concat("\nNumber of dimensions in set: ", dimSet.SystemCount));
                dimSet.Highlight();
                PromptPointOptions promptPointOption = new PromptPointOptions("\nSelect point to modify properties :");
                while (true)
                {
                    if (dimSet.SystemCount == 0)
                    {
                        break;
                    }
                    dimSet.Highlight();
                    int[] numArray = DimSystem.ViewportNumbers();
                    TransientManager currentTransientManager = TransientManager.CurrentTransientManager;
                    Circle circle = new Circle();
                    Circle dynPreviewColor = new Circle();
                    Line line = new Line(new Point3d(0, 0, 0), new Point3d(0, 0, 0));
                    circle.Color = Colors.LayerColorPreview;
                    dynPreviewColor.Color = Colors.LayerColorPreview;
                    line.Color = Colors.LayerColorPreview;
                    IntegerCollection integerCollections = new IntegerCollection(numArray);
                    currentTransientManager.AddTransient(circle, TransientDrawingMode.Main, 128, integerCollections);
                    currentTransientManager.AddTransient(dynPreviewColor, TransientDrawingMode.Main, 128, integerCollections);
                    currentTransientManager.AddTransient(line, TransientDrawingMode.Highlight, 128, integerCollections);
                    List<DimPoint> dimSetPoints = dimSet.GetDimPoints(equalPointDistance);
                    PointMonitorEventHandler pointMonitorEventHandler = (object sender, PointMonitorEventArgs e) =>
                    {
                        int closestDimSetPoint = dimSet.GetClosestDimPoint(e.Context.ComputedPoint, equalPointDistance);
                        DimPoint item = dimSetPoints[closestDimSetPoint];
                        Point3d dimLinePoint = item.DimLinePoint;
                        circle.Center = dimLinePoint;
                        double sreenSize = ScreenReader.GetSreenSize();
                        circle.Radius = sreenSize / 200;
                        circle.Normal = rotatedDimension.Normal;
                        dynPreviewColor.Radius = sreenSize / 200;
                        dynPreviewColor.Normal = rotatedDimension.Normal;
                        Point3d point3d = new Point3d();
                        Point3d point3d1 = new Point3d();
                        Point3d point3d2 = new Point3d();
                        if (item.IsLast)
                        {
                            point3d = (item.Dim1PointIndex != 1 ? item.Dim1.XLine2Point : item.Dim1.XLine1Point);
                            point3d2 = point3d;
                        }
                        else
                        {
                            point3d = (item.Dim1PointIndex != 1 ? item.Dim1.XLine2Point : item.Dim1.XLine1Point);
                            point3d1 = (item.Dim2PointIndex != 1 ? item.Dim2.XLine2Point : item.Dim2.XLine1Point);
                            point3d2 = (dimLinePoint.DistanceTo(point3d) <= dimLinePoint.DistanceTo(point3d1) ? point3d1 : point3d);
                        }
                        line.StartPoint = dimLinePoint;
                        line.EndPoint = point3d2;
                        dynPreviewColor.Center = point3d2;
                        currentTransientManager.UpdateTransient(circle, integerCollections);
                        currentTransientManager.UpdateTransient(dynPreviewColor, integerCollections);
                        currentTransientManager.UpdateTransient(line, integerCollections);
                    };
                    editor.PointMonitor += pointMonitorEventHandler;
                    try
                    {
                        point = mdiActiveDocument.Editor.GetPoint(promptPointOption);
                    }
                    finally
                    {
                        editor.PointMonitor -= pointMonitorEventHandler;
                        currentTransientManager.EraseTransient(circle, integerCollections);
                        currentTransientManager.EraseTransient(dynPreviewColor, integerCollections);
                        currentTransientManager.EraseTransient(line, integerCollections);
                        circle.Dispose();
                        dynPreviewColor.Dispose();
                        line.Dispose();
                    }
                    if (point.Status == PromptStatus.OK)
                    {
                        Point3d point3d3 = point.Value.TransformBy(matrix3d);
                        int num = dimSet.GetClosestDimPoint(point3d3, equalPointDistance);
                        dimSet.PointProperties(num, @checked, arrowheadBlkName, flag, checked1, equalPointDistance);
                        transaction.TransactionManager.QueueForGraphicsFlush();
                    }
                    else
                    {
                        dimSet.Unhighlight();
                        break;
                    }
                }
                dimSet.Unhighlight();
                transaction.Commit();
            }
        }
    }
}
