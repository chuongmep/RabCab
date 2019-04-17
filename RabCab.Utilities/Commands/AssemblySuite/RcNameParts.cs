// -----------------------------------------------------------------------------------
//     <copyright file="RcNumParts.cs" company="CraterSpace">
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
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using RabCab.Agents;
using RabCab.Analysis;
using RabCab.Extensions;
using RabCab.Settings;
using static RabCab.Engine.Enumerators.Enums;

namespace RabCab.Commands.AssemblySuite
{
    internal class RcNameParts
    {
        /// <summary>
        ///     TODO
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_NAMEPARTS",
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
            //| CommandFlags.NoInferConstrain
        )]
        public void Cmd_NameParts()
        {
            //Get the current document utilities
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            var keyList = new List<KeywordAgent>();

            var userResetPartCounter = SettingsUser.ResetPartCount;

            //Reset Part Number to 1 if specified in bool
            if (SettingsUser.ResetPartCount)
                SortingAgent.CurrentPartNumber = 1;

            var keyAgentName = new KeywordAgent(acCurEd, "NameFormat", "Enter naming format: ", TypeCode.String,
                SettingsUser.NamingConvention);
            var keyAgentNum = new KeywordAgent(acCurEd, "StartFrom", "Enter part number to start from: ",
                TypeCode.Int32, SortingAgent.CurrentPartNumber.ToString());

            keyList.Add(keyAgentName);
            keyList.Add(keyAgentNum);

            //Check for pick-first selection -> if none, get selection      
            var acSet = SelectionSet.FromObjectIds(acCurEd.GetFilteredSelection(DxfNameEnum._3Dsolid, false, keyList));

            keyAgentName.Set(ref SettingsUser.NamingConvention);
            keyAgentNum.Set(ref SortingAgent.CurrentPartNumber);

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

                    //eList.SortSolids();
                    eList.SortAndName(pWorker, acCurDb, acCurEd, acTrans);
                }

                acTrans.Commit();
            }

            SettingsUser.ResetPartCount = userResetPartCounter;
        }
    }
}