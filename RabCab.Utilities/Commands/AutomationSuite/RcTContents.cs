// -----------------------------------------------------------------------------------
//     <copyright file="RcTContents.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>04/08/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using RabCab.Extensions;
using RabCab.Settings;

namespace RabCab.Commands.AutomationSuite
{
    internal class RcTContents
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_TCONTENTS",
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
        public void Cmd_TContents()
        {
            //Get the current document utilities
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            RcPageNumber.NumberPages();
            
            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                var dbDict = (DBDictionary)acTrans.GetObject(acCurDb.LayoutDictionaryId, OpenMode.ForRead);

                var pr = acCurEd.GetPoint("\nEnter table insertion point: ");
                if (pr.Status == PromptStatus.OK)
                {
                    var acTable = new Table();
                    acTable.Position = pr.Value;
                    acTable.TableStyle = acCurDb.Tablestyle;

                    var colCount = 2;
                    var rowCount = dbDict.Count + 1;

                    acTable.SetSize(rowCount, colCount);

                    var rowHeight = SettingsUser.TableRowHeight;
                    var textHeight = SettingsUser.TableTextHeight;

                    acTable.SetRowHeight(rowHeight);

                    var header = acTable.Cells[0, 0];
                    header.Value = "TABLE OF CONTENTS";
                    header.Alignment = CellAlignment.MiddleCenter;
                    header.TextHeight = textHeight;

                    acTable.Cells[1, 0].TextString = "Page #";
                    acTable.Cells[1, 1].TextString = "Title";

                    foreach (var curEntry in dbDict)
                    {
                        var layout = (Layout)acTrans.GetObject(curEntry.Value, OpenMode.ForRead);

                        if (layout.LayoutName == "Model") continue;
                        var curOrder = layout.TabOrder;

                        acTable.Cells[curOrder + 1, 0].TextString = curOrder.ToString();
                        acTable.Cells[curOrder + 1, 1].TextString = layout.LayoutName;
                    }

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