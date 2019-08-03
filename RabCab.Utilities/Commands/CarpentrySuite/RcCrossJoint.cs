using System;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using RabCab.Analysis;
using RabCab.Engine.Enumerators;
using RabCab.Entities.Linework;
using RabCab.Extensions;
using RabCab.Settings;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = Autodesk.AutoCAD.Runtime.Exception;

namespace RabCab.Commands.CarpentrySuite
{
    internal class RcCrossJoint
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_CROSSJOINT",
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
            //| CommandFlags.NoBlockEditor
            //| CommandFlags.NoActionRecording
            //| CommandFlags.ActionMacro
            //| CommandFlags.NoInferConstraint 
        )]
        public void Cmd_CrossJoint()
        {
            if (!Agents.LicensingAgent.Check()) return;
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            var cutId = acCurEd.GetFilteredSelection(Enums.DxfNameEnum._3Dsolid, true, null, "\nSelect 3D Solid to cut: ");
            if (cutId.Length <= 0) return;
            
            var cutterId = acCurEd.GetFilteredSelection(Enums.DxfNameEnum._3Dsolid, true, null, "\nSelect 3D Solid use as cutter: ");
            if (cutterId.Length <= 0) return;

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                var objToCut = acTrans.GetObject(cutId[0], OpenMode.ForWrite) as Solid3d;
                var objCutter = acTrans.GetObject(cutterId[0], OpenMode.ForWrite) as Solid3d;

                if (objToCut != null && objCutter != null)
                {
                    var toCutClone = objToCut.Clone() as Solid3d;
                    var cutterClone = objCutter.Clone() as Solid3d;

                    if (cutterClone != null && toCutClone != null)
                    {
                        toCutClone.BooleanOperation(BooleanOperationType.BoolIntersect, cutterClone);

                        acCurDb.AppendEntity(toCutClone, acTrans);

                        var gExt = toCutClone.GeometricExtents;
                        var gCen = gExt.MinPoint.GetMidPoint(gExt.MaxPoint);

                        var gPlane = new Plane(gCen, acCurEd.CurrentUserCoordinateSystem.CoordinateSystem3d.Zaxis);
                        try
                        {
                            var slicedObj = toCutClone.Slice(gPlane, true);
                            acCurDb.AppendEntity(slicedObj, acTrans);

                            objToCut.BooleanOperation(BooleanOperationType.BoolSubtract, toCutClone);
                            objCutter.BooleanOperation(BooleanOperationType.BoolSubtract, slicedObj);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }
                    }
                }

                acTrans.Commit();
            }

        }
    }
}
