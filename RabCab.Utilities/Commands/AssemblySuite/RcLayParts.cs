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

using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using RabCab.Agents;
using RabCab.Analysis;
using RabCab.Engine.Enumerators;
using RabCab.Extensions;
using RabCab.Settings;
using Exception = Autodesk.AutoCAD.Runtime.Exception;

namespace RabCab.Commands.AssemblySuite
{
    internal class RcLayParts
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_LAYPARTS",
            CommandFlags.Modal
            //| CommandFlags.Transparent
            | CommandFlags.UsePickSet
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

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                using (var pWorker = new ProgressAgent("Parsing Solids: ", acSet.Count))
                {
                    var eList = new List<EntInfo>();

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

                        eList.Add(new EntInfo(acSol, acCurDb, acTrans));
                    }

                    if (SettingsUser.SortByName)
                    {
                        eList.SortByName();
                    }
                    else
                    {
                        eList.SortSolids();
                    }
                   
                    eList.GroupAndLay(Point3d.Origin, pWorker, acCurDb, acCurEd, acTrans);

                }

                acTrans.Commit();
            }
        }
    }
}