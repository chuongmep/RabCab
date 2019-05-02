using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.Runtime;
using RabCab.Entities.Annotation;
using RabCab.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using RabCab.Calculators;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace RabCab.Commands.AnnotationSuite
{
    class RcDimMove
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_CMDDEFAULT",
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
        public void Cmd_Default()
        {
            //Get the current document utilities
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            PromptPointResult point;
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
                dimSet = DimSystem.GetSystem(rotatedDimension, equalPointDistance, equalPointDistance);
                editor.WriteMessage(string.Concat("\nNumber of dimensions in set: ", dimSet.Count));
                dimSet.Highlight();
                PromptPointOptions promptPointOption = new PromptPointOptions("\nSelect new position of dimensions:");
                while (true)
                {
                    dimSet.Highlight();
                    int[] numArray = DimSystem.GetActiveViewCount();
                    TransientManager currentTransientManager = TransientManager.CurrentTransientManager;
                    IntegerCollection integerCollections = new IntegerCollection(numArray);
                    List<RotatedDimension> rotatedDimensions = new List<RotatedDimension>();
                    foreach (RotatedDimension listOfDim in dimSet.SysList)
                    {
                        RotatedDimension rotatedDimension1 = (RotatedDimension)listOfDim.Clone();
                        currentTransientManager.AddTransient(rotatedDimension1, TransientDrawingMode.Highlight, 128, integerCollections);
                        rotatedDimensions.Add(rotatedDimension1);
                    }
                    PointMonitorEventHandler pointMonitorEventHandler = (object sender, PointMonitorEventArgs e) =>
                    {
                        foreach (RotatedDimension tDim in rotatedDimensions)
                        {
                            if (!tDim.UsingDefaultTextPosition)
                            {
                                Point3d dimLinePoint = tDim.DimLinePoint;
                                Point3d textPosition = tDim.TextPosition;
                                tDim.DimLinePoint = e.Context.ComputedPoint;
                                currentTransientManager.UpdateTransient(tDim, integerCollections);
                                tDim.TextPosition = textPosition.Add(dimLinePoint.GetVectorTo(tDim.DimLinePoint));
                            }
                            else
                            {
                                tDim.DimLinePoint = e.Context.ComputedPoint;
                            }
                            currentTransientManager.UpdateTransient(tDim, integerCollections);
                        }
                    };
                    editor.PointMonitor += pointMonitorEventHandler;
                    try
                    {
                        point = mdiActiveDocument.Editor.GetPoint(promptPointOption);
                    }
                    finally
                    {
                        editor.PointMonitor -= pointMonitorEventHandler;
                        foreach (RotatedDimension rotatedDimension2 in rotatedDimensions)
                        {
                            currentTransientManager.EraseTransient(rotatedDimension2, integerCollections);
                            rotatedDimension2.Dispose();
                        }
                    }
                    if (point.Status != PromptStatus.OK)
                    {
                        break;
                    }
                    dimSet.MoveSystem(point.Value.TransformBy(matrix3d), equalPointDistance);
                    transaction.TransactionManager.QueueForGraphicsFlush();
                }
                dimSet.Unhighlight();
                dimSet.Unhighlight();
                transaction.Commit();
            }
        }
    }
}
