using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using RabCab.Analysis;
using RabCab.Engine.Enumerators;
using RabCab.Extensions;
using RabCab.Settings;

namespace RabCab.Commands.AssemblySuite
{
    internal class RcUpdateChildren
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_RCUPDCHILDREN",
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
            | CommandFlags.NoBlockEditor
            | CommandFlags.NoActionRecording
            | CommandFlags.ActionMacro
            //| CommandFlags.NoInferConstraint 
        )]
        public void Cmd_RcUdateChildren()
        {
            //Get the current document utilities
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            var objIds =
                acCurEd.GetFilteredSelection(Enums.DxfNameEnum._3Dsolid, true, null, "\nSelect parent object: ");
            if (objIds.Length <= 0) return;

            UpdateChildren(objIds, acCurEd, acCurDb);

        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="objIds"></param>
        /// <param name="acCurEd"></param>
        /// <param name="acCurDb"></param>
        internal static void UpdateChildren(ObjectId[] objIds, Editor acCurEd, Database acCurDb)
        {
            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                foreach (var oId in objIds)
                {
                    var pSol = acTrans.GetObject(oId, OpenMode.ForRead) as Solid3d;

                    if (pSol != null)
                    {
                        var eInfo = new EntInfo(pSol, acCurDb, acTrans);

                        if (eInfo.ChildHandles.Count > 0)
                            foreach (var cHandle in eInfo.ChildHandles)
                            {
                                var objId = acCurDb.GetObjectId(false, cHandle, 0);

                                if (objId == ObjectId.Null) continue;

                                Solid3d cSol = null;

                                try
                                {
                                    cSol = acTrans.GetObject(objId, OpenMode.ForWrite) as Solid3d;
                                }
                                catch (Exception)
                                {
                                    acCurEd.WriteMessage("\nChild was erased.");
                                }

                                if (cSol == null) continue;

                                var cInfo = new EntInfo(cSol, acCurDb, acTrans);

                                var pClone = pSol.Clone() as Solid3d;
                                if (pClone == null) continue;

                                pClone.TransformBy(eInfo.LayMatrix);


                                acCurDb.AppendEntity(pClone);
                                pClone.TopLeftTo(Point3d.Origin);
                                pClone.TransformBy(cInfo.LayMatrix.Inverse());

                                pClone.TransformBy(Matrix3d.Displacement(
                                    pClone.GeometricExtents.MinPoint.GetVectorTo(cSol.GeometricExtents.MinPoint)));

                                pClone.SwapIdWith(cSol.ObjectId, true, true);

                                cSol.Erase();
                                cSol.Dispose();
                            }
                        else
                            acCurEd.WriteMessage("\nObject has no child objects attached.");
                    }
                }

                acTrans.Commit();
            }
        }
    }
}