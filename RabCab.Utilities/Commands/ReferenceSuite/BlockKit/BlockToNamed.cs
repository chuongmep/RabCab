using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using RabCab.Agents;
using RabCab.Engine.Enumerators;
using RabCab.Extensions;
using RabCab.Settings;
using Exception = System.Exception;

namespace RabCab.Commands.ReferenceSuite.BlockKit
{
    class BlockToNamed
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_BTON",
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
        public void Cmd_BTON()
        {
            if (!Agents.LicensingAgent.Check()) return;
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            var objIds = acCurEd.GetFilteredSelection(Enums.DxfNameEnum.Insert, false);
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

                foreach (var obj in objIds)
                {
                    var acBref = acTrans.GetObject(obj, OpenMode.ForWrite) as BlockReference;

                    if (acBref == null)
                    {
                        acTrans.Abort();
                        return;
                    }

                    var bName = acBref.Name;

                    using (DBObjectCollection dbObjCol = new DBObjectCollection())
                    {
                        acBref.Explode(dbObjCol);

                        try
                        {
                            foreach (DBObject dbObj in dbObjCol)
                            {
                                Entity acEnt = dbObj as Entity;

                                acCurDb.AppendEntity(acEnt);

                                acEnt = acTrans.GetObject(dbObj.ObjectId, OpenMode.ForWrite) as Entity;

                                acEnt.UpdateXData(bName, Enums.XDataCode.Name, acCurDb, acTrans);

                                acBref.Erase();
                            }
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

