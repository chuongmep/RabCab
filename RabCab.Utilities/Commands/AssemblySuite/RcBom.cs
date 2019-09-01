// -----------------------------------------------------------------------------------
//     <copyright file="RcBom.cs" company="CraterSpace">
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
using RabCab.Engine.Enumerators;
using RabCab.Extensions;
using RabCab.Settings;
using Exception = Autodesk.AutoCAD.Runtime.Exception;

namespace RabCab.Commands.AssemblySuite
{
    internal class RcBom
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_GENBOM",
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
        public void Cmd_RCBOM()
        {
            if (!LicensingAgent.Check()) return;
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            //Check for pick-first selection -> if none, get selection      
            var acSet = SelectionSet.FromObjectIds(acCurEd.GetFilteredSelection(Enums.DxfNameEnum._3Dsolid, false));

            var multAmount = 1;

            if (SettingsUser.PromptForMultiplication)
                multAmount = acCurEd.GetPositiveInteger("\nEnter number to multiply parts by: ", 1);

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                //Get all Layouts in the Drawing
                var layoutList = new List<Layout>();
                var dbDict = (DBDictionary) acTrans.GetObject(acCurDb.LayoutDictionaryId, OpenMode.ForRead);
                foreach (var curEntry in dbDict)
                {
                    var layout = (Layout) acTrans.GetObject(curEntry.Value, OpenMode.ForRead);
                    if (layout != null)
                        layoutList.Add(layout);
                }

                var pKeyOpts = new PromptKeywordOptions(string.Empty)
                {
                    Message = "\nWhich layout would you like to insert the parts list in?",
                    AllowArbitraryInput = true
                };
                var iterator = 'A';
                var keyDict = new Dictionary<string, string>();

                foreach (var layout in layoutList)
                {
                    keyDict.Add(layout.LayoutName, iterator.ToString());
                    pKeyOpts.Keywords.Add(iterator.ToString(), iterator.ToString(),
                        iterator + ": " + layout.LayoutName.ToLower());
                    iterator++;
                }

                pKeyOpts.AllowNone = false;

                var pKeyRes = acCurEd.GetKeywords(pKeyOpts);

                if (pKeyRes.Status != PromptStatus.OK) return;
                var returnIterator = pKeyRes.StringResult;

                ObjectId id;
                var layoutName = string.Empty;

                foreach (var entry in keyDict)
                    if (entry.Value == returnIterator)
                    {
                        layoutName = entry.Key;
                        break;
                    }

                if (dbDict.Contains(layoutName))
                {
                    id = dbDict.GetAt(layoutName);
                }
                else
                {
                    acCurEd.WriteMessage("\nLayout not found. Cannot continue.");
                    acTrans.Abort();
                    return;
                }

                var chosenLayout = acTrans.GetObject(id, OpenMode.ForRead) as Layout;
                if (chosenLayout == null) return;

                // Reference the Layout Manager
                var acLayoutMgr = LayoutManager.Current;
                // Set the layout current if it is not already
                if (chosenLayout.TabSelected == false) acLayoutMgr.CurrentLayout = chosenLayout.LayoutName;

                if (chosenLayout.LayoutName != "Model")
                    try
                    {
                        acCurEd.SwitchToPaperSpace();
                    }
                    catch (Exception e)
                    {
                        //ignored
                    }


                // Ask for the table insertion point
                var pr = acCurEd.GetPoint("\nEnter table insertion point: ");
                if (pr.Status == PromptStatus.OK)
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

                        var acTable = new Table();
                        acTable.Position = pr.Value;
                        acTable.TableStyle = acCurDb.Tablestyle;

                        eList.SortToTable(pWorker, acCurDb, acCurEd, acTrans, acTable, multAmount);

                        acCurDb.AppendEntity(acTable, acTrans);

                        //Generate the layout
                        acTable.GenerateLayout();
                        acTrans.MoveToAttachment(acTable, SettingsUser.TableAttach, pr.Value, SettingsUser.TableXOffset,
                            SettingsUser.TableYOffset);
                    }

                acTrans.Commit();
            }
        }
    }
}