using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using RabCab.Extensions;
using RabCab.Settings;

namespace RabCab.Commands.AssemblySuite
{
    internal class RcBlockLegend
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_BLOCKLEGEND",
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
        public void Cmd_BlockLegend()
        {
            var rowHeight = SettingsUser.TableRowHeight;
            var colWidth = SettingsUser.TableColumnWidth;
            var textHeight = SettingsUser.TableTextHeight;

            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            if (acCurDoc == null)
                return;

            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            var prRes = acCurEd.GetPoint("\nEnter table insertion point");
            if (prRes.Status != PromptStatus.OK)
                return;

            using (var acTrans = acCurDoc.TransactionManager.StartTransaction())
            {
                var acBTable = (BlockTable)acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead);

                // Create the table, set its style and default row/column size
                var acTable = new Table();
                acTable.TableStyle = acCurDb.Tablestyle;
                acTable.SetRowHeight(rowHeight);
                acTable.SetColumnWidth(colWidth);
                acTable.Position = prRes.Value;

                // Set the header cell
                var header = acTable.Cells[0, 0];
                header.Value = "PROPERTIES LEGEND";
                header.Alignment = CellAlignment.MiddleCenter;
                header.TextHeight = textHeight;

                // Insert an additional column
                acTable.InsertColumns(0, colWidth, 1);

                var bList = new List<BlockTableRecord>();

                // Loop through the blocks in the drawing, creating rows
                foreach (var id in acBTable)
                {
                    var btr = (BlockTableRecord)acTrans.GetObject(id, OpenMode.ForRead);
                    // Only care about user-insertable blocks

                    if (!btr.IsLayout && !btr.IsAnonymous)
                    {
                        bList.Add(btr);
                    }
                }

                var sortedBlocks = bList.OrderBy(e => e.Name);

                foreach (var btr in sortedBlocks)
                {
                    // Add a row
                    acTable.InsertRows(acTable.Rows.Count, rowHeight, 1);
                    var rowIdx = acTable.Rows.Count - 1;

                    // The first cell will hold the block name
                    var first = acTable.Cells[rowIdx, 0];
                    first.Value = btr.Name;
                    first.Alignment = CellAlignment.MiddleCenter;
                    first.TextHeight = textHeight;

                    // The second will contain a thumbnail of the block
                    var second = acTable.Cells[rowIdx, 1];
                    second.Alignment = CellAlignment.MiddleCenter;
                    second.BlockTableRecordId = btr.ObjectId;
                }

                // Now we add the table to the current space
                var curSpace = (BlockTableRecord)acTrans.GetObject(acCurDb.CurrentSpaceId, OpenMode.ForWrite);
                curSpace.AppendEntity(acTable);

                // And to the transaction, which we then commit
                acTrans.AddNewlyCreatedDBObject(acTable, true);
                acTable.GenerateLayout();
                acTrans.MoveToAttachment(acTable, SettingsUser.TableAttach, prRes.Value, SettingsUser.TableXOffset, SettingsUser.TableYOffset);
                acTrans.Commit();
            }
        }
    }
}