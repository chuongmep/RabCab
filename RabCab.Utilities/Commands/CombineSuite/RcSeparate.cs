// -----------------------------------------------------------------------------------
//     <copyright file="RcSeparate.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>04/10/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.Runtime;
using RabCab.Agents;
using RabCab.Extensions;
using RabCab.Settings;

namespace RabCab.Commands.CombineSuite
{
    internal class RcSeparate
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_SEPSOLS",
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
            //| CommandFlags.NoActionRecording
            //| CommandFlags.ActionMacro
            //| CommandFlags.NoInferConstraint 
        )]
        public void Cmd_SepSols()
        {
            if (!LicensingAgent.Check()) return;
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                var objIds = acCurEd.SelectAllOfType("3DSOLID", acTrans);

                if (objIds.Length > 0) objIds.Separate(acCurDb, acTrans);
                acTrans.Commit();
            }
        }
    }
}