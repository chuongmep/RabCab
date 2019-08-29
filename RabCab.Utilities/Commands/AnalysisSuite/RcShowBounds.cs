using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.Runtime;
using RabCab.Agents;
using RabCab.Extensions;
using RabCab.Settings;

namespace RabCab.Commands.AnalysisSuite
{
    internal class RcShowBounds
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_SHOWBOUNDS",
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
        public void Cmd_ShowBounds()
        {
            if (!LicensingAgent.Check()) return;

            if (!LicensingAgent.Check()) return;
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            var objIds = acCurEd.GetAllSelection(false);
            var boolRes = acCurEd.GetBool("Save bounds? ");
            if (boolRes == null)
                return;

            var append = boolRes.Value;

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                var sol = acTrans.GetBoundingBox(objIds, acCurDb);

                sol.Transparency = new Transparency(75);
                sol.Color = Colors.LayerColorBounds;

                TransientAgent.Add(sol);
                TransientAgent.Draw();

                if (append)
                    acCurDb.AppendEntity(sol);
                else
                    sol.Dispose();

                TransientAgent.Clear();


                acTrans.Commit();
            }
        }
    }
}