using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using RabCab.Engine.Enumerators;
using RabCab.Extensions;
using RabCab.Settings;

namespace RabCab.Commands.ReferenceSuite.BlockKit
{
    internal class RcQuickRename
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_QRENAME",
            CommandFlags.Modal
            //| CommandFlags.Transparent
            //| CommandFlags.UsePickSet
            //| CommandFlags.Redraw
            //| CommandFlags.NoPerspective
            | CommandFlags.NoMultiple
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
        public void Cmd_QRename()
        {
            //Get the current document utilities
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            var objIds = acCurEd.GetFilteredSelection(Enums.DxfNameEnum.Insert, true);
            if (objIds.Length <= 0) return;

            // start a transaction
            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for write
                var acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                if (acBlkTbl == null)
                {
                    acTrans.Abort();
                    return;
                }

                var acBref = acTrans.GetObject(objIds[0], OpenMode.ForRead) as BlockReference;

                if (acBref == null)
                {
                    acTrans.Abort();
                    return;
                }

                var bName = acBref.Name;

                var newName = acCurEd.GetSimpleString("Enter new block name: ", bName);

                if (string.IsNullOrEmpty(newName))
                {
                    acTrans.Abort();
                    return;
                }

                // check if the block table contains the block to rename
                if (acBlkTbl.Has(bName))
                {
                    // check if the block table already contains a block named as the new name
                    if (acBlkTbl.Has(newName))
                    {
                        Application.ShowAlertDialog($"A block named '{newName} already exits");
                    }
                    else
                    {
                        // open the block definition
                        var btr = (BlockTableRecord) acTrans.GetObject(acBlkTbl[bName], OpenMode.ForWrite);
                        // rename the bloc
                        btr.Name = newName;
                    }
                }
                else
                {
                    Application.ShowAlertDialog($"Block '{bName}' not found");
                }

                acTrans.Commit();
            }
        }
    }
}