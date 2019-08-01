using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using RabCab.Entities.Controls;
using RabCab.Extensions;
using RabCab.Initialization;
using RabCab.Settings;
using AcAp = Autodesk.AutoCAD.ApplicationServices.Application;
using Exception = System.Exception;

namespace RabCab.Commands.AnalysisSuite
{
    internal class RcDump
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_DUMPALLPROPS",
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
        public void Dump()
        {
            if (!Agents.LicensingAgent.Check()) return;

            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            var entRes = acCurEd.GetEntity("\nSelect object: ");
            if (entRes.Status == PromptStatus.OK)
            {
                PrintDump(entRes.ObjectId, acCurEd);
                AcAp.DisplayTextScreen = true;
            }
        }

        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_DUMPCOMPROPS",
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
        public static void ListComProps()
        {
            if (!Agents.LicensingAgent.Check()) return;

            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurEd = acCurDoc.Editor;

            var peo = new PromptEntityOptions("\nSelect object: ");
            var res = acCurEd.GetEntity(peo);

            if (res.Status != PromptStatus.OK)
                return;

            using (Transaction acTrans = acCurDoc.TransactionManager.StartOpenCloseTransaction())
            {
                var acEnt = (Entity) acTrans.GetObject(res.ObjectId, OpenMode.ForRead);
                var acadObj = acEnt.AcadObject;
                var props = TypeDescriptor.GetProperties(acadObj);
                foreach (PropertyDescriptor prop in props)
                {
                    var value = prop.GetValue(acadObj);
                    if (value != null) acCurEd.WriteMessage("\n{0} = {1}", prop.DisplayName, value.ToString());
                }

                acTrans.Commit();
            }
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ed"></param>
        private void PrintDump(ObjectId id, Editor ed)
        {
            var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

            using (var tr = id.Database.TransactionManager.StartTransaction())
            {
                var dbObj = tr.GetObject(id, OpenMode.ForRead);
                var types = new List<Type>();
                types.Add(dbObj.GetType());
                while (true)
                {
                    var type = types[0].BaseType;
                    types.Insert(0, type);
                    if (type == typeof(RXObject))
                        break;
                }

                foreach (var t in types)
                {
                    ed.WriteMessage($"\n\n - {t.Name} -");
                    foreach (var prop in t.GetProperties(flags))
                    {
                        ed.WriteMessage("\n{0,-40}: ", prop.Name);
                        try
                        {
                            ed.WriteMessage("{0}", prop.GetValue(dbObj, null));
                        }
                        catch (Exception e)
                        {
                            ed.WriteMessage(e.Message);
                        }
                    }
                }

                tr.Commit();
            }
        }

        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_DUMPBLOCKREFS",
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
        public void DumpBlockRefs()
        {
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;


            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                var blkTable = (BlockTable) acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead);
                foreach (var id in blkTable)
                {
                    var btRecord = (BlockTableRecord) acTrans.GetObject(id, OpenMode.ForRead);
                    foreach (var bR in btRecord.GetBlockReferences()) acCurEd.WriteMessage("\n" + bR.Name);
                }

                acTrans.Commit();
            }
        }
    }
}