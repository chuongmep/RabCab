// -----------------------------------------------------------------------------------
//     <copyright file="RcSubtrahend.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>04/08/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.Runtime;
using RabCab.Agents;
using RabCab.Engine.Enumerators;
using RabCab.Extensions;
using RabCab.Settings;

namespace RabCab.Commands.CombineSuite
{
    internal class RcSubtrahend
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_SUBTRAHEND",
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
            //| CommandFlags.NoBlockEditor
            //| CommandFlags.NoActionRecording
            //| CommandFlags.ActionMacro
            //| CommandFlags.NoInferConstraint 
        )]
        public void Cmd_Subtrahend()
        {
            if (!LicensingAgent.Check()) return;
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            var objIds1 =
                acCurEd.GetFilteredSelection(Enums.DxfNameEnum._3Dsolid, false, null,
                    "\nSelect solids to be subtracted from: ");
            if (objIds1.Length <= 0) return;

            var objIds2 =
                acCurEd.GetFilteredSelection(Enums.DxfNameEnum._3Dsolid, false, null,
                    "\nSelect solids to be used as subtrahends: ");
            if (objIds2.Length <= 0) return;

            var delSols = acCurEd.GetBool("\nDelete consumed solids after performing operation? ");
            if (delSols == null) return;

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                objIds1.SolidSubtrahend(objIds2, acCurDb, acTrans, delSols.Value);
                acTrans.Commit();
            }
        }
    }
}