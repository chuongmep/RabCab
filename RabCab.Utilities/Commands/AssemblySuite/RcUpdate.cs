using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using RabCab.Agents;
using RabCab.Entities.Annotation;
using RabCab.Extensions;
using RabCab.Settings;

namespace RabCab.Commands.AssemblySuite
{
    internal class RcUpdate
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_RCUPDATE",
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
            | CommandFlags.NoBlockEditor
            | CommandFlags.NoActionRecording
            | CommandFlags.ActionMacro
            //| CommandFlags.NoInferConstraint 
        )]
        public void Cmd_RcUpdate()
        {
            if (!Agents.LicensingAgent.Check()) return;
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                var objIds = acCurEd.SelectAllOfType("3DSOLID", acTrans);

                using (var pWorker = new ProgressAgent("Updating Solids:", objIds.Length))
                {
                    foreach (var obj in objIds)
                    {
                        if (!pWorker.Progress())
                        {
                            acTrans.Abort();
                            return;
                        }

                        var acSol = acTrans.GetObject(obj, OpenMode.ForRead) as Solid3d;
                        if (acSol == null) continue;

                        acSol.Update(acCurDb, acTrans);
                    }
                }

                acCurEd.WriteMessage($"\n{objIds.Length} objects updated.");
                acTrans.Commit();
            }

            RcLeader.UpdateMleaders();
        }
    }
}