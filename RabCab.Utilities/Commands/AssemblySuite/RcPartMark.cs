// -----------------------------------------------------------------------------------
//     <copyright file="RcPartMark.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>04/09/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using RabCab.Agents;
using RabCab.Engine.Enumerators;
using RabCab.Engine.System;
using RabCab.Extensions;
using RabCab.Settings;

namespace RabCab.Commands.AssemblySuite
{
    internal class RcPartMark
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_RCMARK",
            CommandFlags.Modal
            //| CommandFlags.Transparent
            //| CommandFlags.UsePickSet
            //| CommandFlags.Redraw
            //| CommandFlags.NoPerspective
            //| CommandFlags.NoMultiple
            | CommandFlags.NoTileMode
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
            | CommandFlags.NoBlockEditor
            //| CommandFlags.NoActionRecording
            //| CommandFlags.ActionMacro
            //| CommandFlags.NoInferConstraint 
        )]
        public void Cmd_RcMark()
        {
            //Get the current document utilities
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            var userCmdEcho = AcVars.CmdEcho;
            AcVars.CmdEcho = Enums.CmdEcho.Off;

            // Set up our selection to only select 3D solids
            var pso = new PromptSelectionOptions {MessageForAdding = "\nSelect viewports to mark: "};
            var sf = new SelectionFilter(new[] {new TypedValue((int) DxfCode.Start, "VIEWPORT")});

            //Get the 3d Solid Selection
            var res = acCurEd.GetSelection(pso, sf);

            if (res.Status == PromptStatus.OK)
                // extract the viewport points
                using (var acTrans = acCurDb.TransactionManager.StartTransaction())
                {
                    acCurDb.TransactionManager.QueueForGraphicsFlush();

                    using (
                        var pWorker = new ProgressAgent("Marking Viewports: ", res.Value.Count))
                    {
                        foreach (var objId in res.Value.GetObjectIds())
                        {
                            if (!pWorker.Tick())
                            {
                                acTrans.Abort();
                                return;
                            }

                            using (var psVp = acTrans.GetObject(objId, OpenMode.ForRead) as Viewport)
                            {
                                var pObjs = objId.GetPaperObjects();
                                var mObjs = objId.GetModelObjects();

                                if (pObjs.Count > 0)
                                    foreach (ObjectId id in pObjs)
                                    {
                                        var acEnt = acTrans.GetObject(id, OpenMode.ForWrite) as Entity;
                                        if (acEnt == null) continue;

                                        //Todo only delete if is mark

                                        acEnt.Erase();
                                        acEnt.Dispose();
                                    }

                                if (mObjs.Count > 0)
                                {
                                    //Finished finding objects, now create tags
                                    var xform = psVp.Ms2Ps();

                                    foreach (ObjectId id in mObjs)
                                    {
                                        var dbObj = acTrans.GetObject(id, OpenMode.ForWrite);
                                        if (dbObj == null) continue;

                                        if (dbObj is Solid3d acSol)
                                        {
                                            var solCen = acSol.MassProperties.Centroid;
                                            var solName = acSol.GetPartName();
                                            var insPt = solCen.TransformBy(xform).Flatten();

                                            //TODO add marking block

                                            using (var acText = new MText())
                                            {
                                                acText.TextHeight = SettingsUser.MarkTextHeight;
                                                acText.Contents = solName;
                                                acText.BackgroundFill = true;
                                                acText.UseBackgroundColor = true;
                                                acText.ShowBorders = true;
                                                acText.Location = insPt;
                                                acText.Attachment = AttachmentPoint.MiddleCenter;
                                                //acText.Layer = ;
                                                //acText.ColorIndex = ;                           

                                                //Append the text
                                                acCurDb.AppendEntity(acText, acTrans);
                                            }
                                        }

                                        //Todo add block and insert parse
                                        //else if (dbObj is Circle)
                                        //{

                                        //}
                                        //else if (dbObj is Arc)
                                        //{

                                        //}
                                    }
                                }
                            }
                        }
                    }

                    acTrans.Commit();
                }
        }
    }
}