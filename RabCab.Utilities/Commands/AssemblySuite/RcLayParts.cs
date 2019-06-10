// -----------------------------------------------------------------------------------
//     <copyright file="RcLayParts.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>04/08/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using RabCab.Agents;
using RabCab.Analysis;
using RabCab.Engine.AcSystem;
using RabCab.Engine.Enumerators;
using RabCab.Extensions;
using RabCab.Settings;

namespace RabCab.Commands.AssemblySuite
{
    internal class RcLayParts
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_LAYPARTS",
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
        public void Cmd_LayParts()
        {
            //Get the current document utilities
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            //Check for pick-first selection -> if none, get selection      
            var acSet = SelectionSet.FromObjectIds(acCurEd.GetFilteredSelection(Enums.DxfNameEnum._3Dsolid, false));

            //Set the UCS to World - save the user UCS
            var userCoordSystem = acCurEd.CurrentUserCoordinateSystem;
            acCurEd.CurrentUserCoordinateSystem = Matrix3d.Identity;

            //Set the Grid Mode to ON if it is not ON - Get the users current grid mode
            var userGridMode = AcVars.GridMode;
            if (AcVars.GridMode != Enums.GridMode.On) AcVars.GridMode = Enums.GridMode.On;

            acCurEd.Regen();

            var laypt = acCurEd.Get2DPoint("\nSelect point to lay parts at: ");

            // Return the Grid Mode to the Users Setting
            AcVars.GridMode = userGridMode;

            var multAmount = 1;

            if (SettingsUser.PromptForMultiplication)
                multAmount = acCurEd.GetPositiveInteger("\nEnter number to multiply parts by: ", 1);

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                using (var pWorker = new ProgressAgent("Parsing Solids: ", acSet.Count))
                {
                    var eList = new List<EntInfo>();

                    foreach (var objId in acSet.GetObjectIds())
                    {
                        //Progress progress bar or exit if ESC has been pressed
                        if (!pWorker.Progress())
                        {
                            acTrans.Abort();
                            return;
                        }

                        var acSol = acTrans.GetObject(objId, OpenMode.ForRead) as Solid3d;

                        if (acSol == null) continue;

                        eList.Add(new EntInfo(acSol, acCurDb, acTrans));
                    }

                    eList.SortAndLay(laypt, pWorker, acCurDb, acCurEd, acTrans, multAmount);
                }

                acTrans.Commit();
            }

            //Set the UCS back to the user UCS
            acCurEd.CurrentUserCoordinateSystem = userCoordSystem;
            acCurEd.Regen();
        }
    }
}