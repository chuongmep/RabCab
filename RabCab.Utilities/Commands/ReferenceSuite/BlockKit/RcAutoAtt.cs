using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using RabCab.Engine.Enumerators;
using RabCab.Extensions;
using RabCab.Settings;
using System;
using System.Linq;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace RabCab.Commands.ReferenceSuite.BlockKit
{
    class RcAutoAtt
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_AUTOATT",
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
        public void Cmd_AutoAtt()
        {
            //Get the current document utilities
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            var objIds = acCurEd.GetFilteredSelection(Enums.DxfNameEnum.Insert, false);
            if (objIds.Length <= 0) return;
            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for write
                var acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                if (acBlkTbl == null)
                {
                    acTrans.Abort();
                    return;
                }

                foreach (var obj in objIds)
                {
                    var acBref = acTrans.GetObject(obj, OpenMode.ForRead) as BlockReference;

                    if (acBref == null) continue;

                    BlockTableRecord btr = null;

                    if (acBref.IsDynamicBlock)
                    {
                        btr = acTrans.GetObject(acBref.DynamicBlockTableRecord, OpenMode.ForRead) as BlockTableRecord;
                    }
                    else
                    {
                        btr = acTrans.GetObject(acBref.BlockTableRecord, OpenMode.ForRead) as BlockTableRecord;
                    }

                    if (btr == null) continue;

                    #region TODO replace with xml reading
                    var bName = acBref.Name;

                    //Add attribute definitions
                    //TODO read XML to get attributes to add
                    //For now we'll use a test value

                    var attTag = new AttributeDefinition
                    {
                        Justify = AttachmentPoint.MiddleCenter,
                        AlignmentPoint = btr.Origin,
                        Prompt = "TAG:",
                        Tag = "TAG",
                        TextString = bName,
                        Height = 1,
                        Invisible = true,
                        LockPositionInBlock = true
                    };

                    var attCrate = new AttributeDefinition
                    {
                        Justify = AttachmentPoint.MiddleCenter,
                        AlignmentPoint = btr.Origin,
                        Prompt = "CRATE:",
                        Tag = "CRATE",
                        TextString = "",
                        Height = 1,
                        Invisible = true,
                        LockPositionInBlock = true
                    };

                    btr.UpgradeOpen();
                    btr.AppendEntity(attTag);
                    btr.AppendEntity(attCrate);


                    #endregion
                    acBref.UpgradeOpen();
                    acBref.AppendAttributes(btr, acTrans);
                    acBref.DowngradeOpen();
                    btr.DowngradeOpen();
                }

                acTrans.Commit();
            }

            acCurDb.Audit(true, false);
        }
    }
}
