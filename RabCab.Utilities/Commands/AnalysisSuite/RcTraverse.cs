// -----------------------------------------------------------------------------------
//     <copyright file="RcTraverse.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>04/11/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using RabCab.Agents;
using RabCab.Analysis;
using RabCab.Engine.Enumerators;
using RabCab.Extensions;
using RabCab.Settings;

namespace RabCab.Commands.AnalysisSuite
{
    internal class RcTraverse
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_TRAVERSE",
            CommandFlags.Modal
            //| CommandFlags.Transparent
            | CommandFlags.UsePickSet
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
            | CommandFlags.NoActionRecording
            //| CommandFlags.ActionMacro
            //| CommandFlags.NoInferConstraint 
        )]
        public void Cmd_Traverse()
        {
            //Get the current document utilities
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            //Start a selection set
            SelectionSet acSet;

            //Check for pick-first selection -> if none, get selection
            if (!acCurEd.CheckForPickFirst(out acSet))
                acSet = SelectionSet.FromObjectIds(acCurEd.GetFilteredSelection(Enums.DxfNameEnum._3Dsolid, false));

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                using (var pWorker = new ProgressAgent("Parsing Solids: ", acSet.Count))
                {
                    var parseCount = 1;

                    foreach (var objId in acSet.GetObjectIds())
                    {
                        //Tick progress bar or exit if ESC has been pressed
                        if (!pWorker.Tick())
                        {
                            acTrans.Abort();
                            return;
                        }

                        var acSol = acTrans.GetObject(objId, OpenMode.ForRead) as Solid3d;

                        if (acSol == null) continue;

                        var entInfo = new EntInfo(acSol);
                        acCurEd.WriteMessage("\n" + entInfo.PrintInfo(parseCount));
                        parseCount++;
                    }
                }

                acTrans.Commit();
            }
        }
    }
}