using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using RabCab.Agents;
using RabCab.Engine.Enumerators;
using RabCab.Extensions;
using RabCab.Settings;

namespace RabCab.Commands.AnnotationSuite
{
    internal class RcDimUpdate
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_DIMUPDATE",
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
        public void Cmd_DimUpdate()
        {
            if (!LicensingAgent.Check()) return;
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            var dimIds = acCurEd.GetFilteredSelection(Enums.DxfNameEnum.Dimension, false, null,
                "\nSelect dimensions to update to current dimension style: ");
            if (dimIds.Length <= 0) return;

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                foreach (var obj in dimIds)
                {
                    var acDim = acTrans.GetObject(obj, OpenMode.ForRead) as Dimension;
                    if (acDim == null) return;

                    acDim.Upgrade();
                    acDim.DimensionStyle = acCurDb.Dimstyle;
                    acDim.Downgrade();
                }

                acTrans.Commit();
            }
        }
    }
}