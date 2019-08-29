using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using RabCab.Agents;
using RabCab.Entities.Annotation;
using RabCab.Settings;
using Exception = System.Exception;

namespace RabCab.Commands.AnnotationSuite
{
    internal class RcPartLeader
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_PARTLEADER",
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
        public void Cmd_PartLeader()
        {
            if (!LicensingAgent.Check()) return;
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            try
            {
                Entity jigEnt = MLeaderJigger.Jig();
                if (jigEnt != null)
                    using (var acTrans = acCurDb.TransactionManager.StartTransaction())
                    {
                        var btr = (BlockTableRecord) acTrans.GetObject(acCurDb.CurrentSpaceId, OpenMode.ForWrite);
                        btr.AppendEntity(jigEnt);
                        acTrans.AddNewlyCreatedDBObject(jigEnt, true);

                        var mL = jigEnt as MLeader;
                        if (mL != null) RcLeader.UpdateLeader(mL, acCurDoc, acCurEd, acTrans);
                        acTrans.Commit();
                    }
            }
            catch (Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(ex.ToString());
            }
        }
    }
}