using System;
using System.Runtime.InteropServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using RabCab.Extensions;
using RabCab.Settings;

namespace RabCab.Commands.AnnotationSuite
{
    internal class RcShrinkWrap
    {

      

        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_SHRINKWRAP",
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
        public void Cmd_ShrinkWrap()
        {
            //Get the current document utilities
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                var objects = acCurEd.SelectAllOfType("*TEXT", acTrans);
                
                foreach (var obj in objects)
                {
                    var mText = acTrans.GetObject(obj, OpenMode.ForRead) as MText;
                    if (mText == null) continue;

                    mText.Upgrade();
                    var width = mText.ActualWidth;
                    var height = mText.ActualHeight;

                    switch (mText.ColumnType)
                    {
                        case ColumnType.DynamicColumns:
                            mText.ColumnWidth = width / mText.ColumnCount;
                            break;
                        case ColumnType.NoColumns:
                            mText.Width = width;
                            break;
                        case ColumnType.StaticColumns:
                            mText.ColumnWidth = width / mText.ColumnCount;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    mText.Height = height;

                    mText.Downgrade();
                }
                
                acTrans.Commit();
            }
            
        }
    }
}