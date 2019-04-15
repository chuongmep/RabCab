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
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using RabCab.Agents;
using RabCab.Analysis;
using RabCab.Extensions;
using RabCab.Settings;
using static RabCab.Engine.Enumerators.Enums;
using Exception = Autodesk.AutoCAD.Runtime.Exception;

namespace RabCab.Commands.AssemblySuite
{
    internal class RcNameParts
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_NAMEPARTS",
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

            //Start a selection set
            SelectionSet acSet;

            //Check for pick-first selection -> if none, get selection
            if (!acCurEd.CheckForPickFirst(out acSet))
                acSet = SelectionSet.FromObjectIds(acCurEd.GetFilteredSelection(DxfNameEnum._3Dsolid, false));

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

                    eList.SortSolids();
                           
                    var groups = eList.GroupBy(x => new
                    {
                        Layer = SettingsUser.SortByLayer ? x.EntLayer : null,
                        Color = SettingsUser.SortByColor ? x.EntColor : null,
                        Thickness = SettingsUser.SortByThickness ? x.Thickness : double.NaN,
                        //Name = compNames ? SettingsUser.SortByName ? x.RcName : null : null,
                         x.Length,
                         x.Width,
                         x.Volume,
                         x.Asymmetry,
                         x.TxDirection
                    });

                   
                    if (SettingsUser.ResetPartCount)
                        SortingAgent.CurrentPartNumber = 1;

                    pWorker.Reset("Naming Solids: ");
                    var enumerable = groups.ToList();
                    pWorker.SetTotalOperations(enumerable.Count());

                    if (enumerable.Count > 0)
                        try
                        {
                            foreach (var group in enumerable)
                            {                                
                                var nameString = SettingsUser.NamingConvention;

                                //Tick progress bar or exit if ESC has been pressed
                                if (!pWorker.Tick())
                                {
                                    acTrans.Abort();
                                    return;
                                }

                                var baseInfo = @group.First();
                                var baseSolid = acTrans.GetObject(baseInfo.ObjId, OpenMode.ForRead) as Solid3d;

                                if (baseSolid == null) continue;

                                var partCount = 1;

                                if (SortingAgent.CurrentPartNumber < 10)
                                    nameString += "0";
                                nameString += SortingAgent.CurrentPartNumber;

                                var nonMirrors = new List<EntInfo>();
                                var mirrors = new List<EntInfo>();

                                var firstParse = true;

                                //Find Mirrors
                                foreach (var eInfo in group)
                                {
                                    if (firstParse)
                                    {
                                        nonMirrors.Add(eInfo);
                                        firstParse = false;
                                        continue;
                                    }

                                    if (eInfo.IsMirrorOf(baseInfo))
                                    {
                                        mirrors.Add(eInfo);
                                    }
                                    else
                                    {
                                        nonMirrors.Add(eInfo);
                                    }
                                }

                                var groupTotal = nonMirrors.Count;

                                foreach (var eInfo in nonMirrors)
                                {
                                   
                                    var acSol = acTrans.GetObject(eInfo.ObjId, OpenMode.ForRead) as Solid3d;

                                    if (acSol == null) continue;

                                    var handle = acSol.Handle;

                                    eInfo.RcName = nameString;

                                    eInfo.RcQtyOf = partCount;

                                    eInfo.RcQtyTotal = groupTotal;

                                    var supressPartName = false;

                                    if (baseInfo.Hndl.ToString() != handle.ToString())
                                    {
                                        supressPartName = true;
                                        eInfo.BaseHandle = baseInfo.Hndl;
                                        baseInfo.ChildHandles.Add(handle);
                                        baseSolid.UpdateXData(baseInfo.ChildHandles, XDataCode.ChildObjects, acCurDb, acTrans);
                                    }

                                    acSol.AddXData(eInfo, acCurDb, acTrans);

                                    string printStr;

                                    if (supressPartName)
                                    {
                                        printStr = "\n\t\u2022 [C] |" + eInfo.PrintInfo(true);
                                    }
                                    else
                                    {
                                        printStr = "\n[P] | " + eInfo.PrintInfo(false);
                                    }

                                    acCurEd.WriteMessage(printStr + " | Part " + partCount + " Of " + groupTotal);

                                    partCount++;
                                }

                                if (mirrors.Count > 0)
                                {
                                    nameString = SettingsUser.NamingConvention;

                                    baseInfo = mirrors.First();
                                    baseSolid = acTrans.GetObject(baseInfo.ObjId, OpenMode.ForRead) as Solid3d;

                                    if (baseSolid == null) continue;

                                    partCount = 1;
                                    SortingAgent.CurrentPartNumber++;

                                    if (SortingAgent.CurrentPartNumber < 10)
                                        nameString += "0";
                                    nameString += SortingAgent.CurrentPartNumber;

                                    groupTotal = nonMirrors.Count;
                                }
                             
                                foreach (var eInfo in mirrors)
                                {

                                    var acSol = acTrans.GetObject(eInfo.ObjId, OpenMode.ForRead) as Solid3d;

                                    if (acSol == null) continue;

                                    var handle = acSol.Handle;

                                    eInfo.RcName = nameString;

                                    eInfo.RcQtyOf = partCount;

                                    eInfo.RcQtyTotal = groupTotal;

                                    var supressPartName = false;

                                    if (baseInfo.Hndl.ToString() != handle.ToString())
                                    {
                                        supressPartName = true;
                                        eInfo.BaseHandle = baseInfo.Hndl;
                                        baseInfo.ChildHandles.Add(handle);
                                        baseSolid.UpdateXData(baseInfo.ChildHandles, XDataCode.ChildObjects, acCurDb, acTrans);
                                    }

                                    acSol.AddXData(eInfo, acCurDb, acTrans);

                                    string printStr;

                                    if (supressPartName)
                                    {
                                        printStr = "\n\t\u2022 [C] |" + eInfo.PrintInfo(true);
                                    }
                                    else
                                    {
                                        printStr = "\n[P] | " + eInfo.PrintInfo(false);
                                    }

                                    acCurEd.WriteMessage(printStr + " | Part " + partCount + " Of " + groupTotal);

                                    partCount++;
                                }                

                                SortingAgent.CurrentPartNumber ++;
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }

                }

                acTrans.Commit();
            }
        }
    }
}