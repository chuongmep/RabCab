using System;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using RabCab.Agents;
using RabCab.Extensions;
using RabCab.Settings;
using Exception = Autodesk.AutoCAD.Runtime.Exception;

namespace RabCab.Commands.TidySuite
{
    internal class RcEmptyDwg
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_EMPTYDWG",
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
            //| CommandFlags.NoActionRecording
            //| CommandFlags.ActionMacro
            //| CommandFlags.NoInferConstraint 
        )]
        public void Cmd_EmptyDwg()
        {
            if (!LicensingAgent.Check()) return;
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var newLayoutName = "BlankLayout";
            LayoutManager.Current.CurrentLayout = "Model";

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                try
                {
                    LayoutManager.Current.CreateLayout(newLayoutName);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    MailAgent.Report(e.Message);
                }

                var layoutDict =
                    acTrans.GetObject(acCurDb.LayoutDictionaryId, OpenMode.ForRead) as DBDictionary;

                if (layoutDict != null)
                    foreach (var de in layoutDict)
                    {
                        var layoutName = de.Key;
                        if (layoutName != "Model" && layoutName != newLayoutName)
                            LayoutManager.Current.DeleteLayout(layoutName);
                    }

                acTrans.Commit();
            }

            acCurDb.PurgeAll(true);
        }
    }
}