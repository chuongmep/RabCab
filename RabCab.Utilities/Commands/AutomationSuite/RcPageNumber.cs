// -----------------------------------------------------------------------------------
//     <copyright file="RcPageNumber.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>04/08/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using RabCab.Extensions;
using RabCab.Settings;

namespace RabCab.Commands.AutomationSuite
{
    internal class RcPageNumber
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_PAGENUM",
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
        public void Cmd_PageNum()
        {
           NumberPages();
        }

        public static void NumberPages()
        {
            //Get the current document utilities
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;


            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                var dbDict = (DBDictionary)acTrans.GetObject(acCurDb.LayoutDictionaryId, OpenMode.ForRead);
                var dCount = dbDict.Count - 1;


                foreach (var curEntry in dbDict)
                {
                    var layout = (Layout)acTrans.GetObject(curEntry.Value, OpenMode.ForRead);

                    if (layout.LayoutName == "Model") continue;

                    using (
                        var blkTblRec =
                            acTrans.GetObject(layout.BlockTableRecordId, OpenMode.ForRead) as
                                BlockTableRecord)
                    {
                        if (blkTblRec == null) continue;

                        foreach (var objId in blkTblRec)
                        {
                            var bRef = acTrans.GetObject(objId, OpenMode.ForRead) as BlockReference;

                            if (bRef == null) continue;

                            var curOrder = layout.TabOrder;

                            bRef.UpdateAttributeByTag(SettingsUser.PageNoOf, curOrder.ToString(), acCurDoc, acCurEd,
                                acTrans);
                            bRef.UpdateAttributeByTag(SettingsUser.PageNoTotal, dCount.ToString(), acCurDoc, acCurEd,
                                acTrans);
                        }
                    }
                }

                acCurEd.Regen();
                acTrans.Commit();
            }
        }
    }
}