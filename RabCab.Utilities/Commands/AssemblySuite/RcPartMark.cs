﻿// -----------------------------------------------------------------------------------
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
using RabCab.Engine.AcSystem;
using RabCab.Engine.Enumerators;
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
            if (!LicensingAgent.Check()) return;
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            _ = AcVars.CmdEcho;
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
                            if (!pWorker.Progress())
                            {
                                acTrans.Abort();
                                return;
                            }

                            using (var psVp = acTrans.GetObject(objId, OpenMode.ForRead) as Viewport)
                            {
                                var pObjs = objId.GetPaperObjects();
                                var mObjs = objId.GetModelObjects();

                                if (pObjs.Count > 0 && SettingsUser.DeleteExistingMarks)
                                    foreach (ObjectId id in pObjs)
                                    {
                                        var acEnt = acTrans.GetObject(id, OpenMode.ForWrite) as Entity;
                                        if (acEnt == null) continue;

                                        if (!acEnt.HasXData()) continue;
                                        var txt = acEnt.GetXData<string>(Enums.XDataCode.Info);

                                        if (txt != "RCMARK") continue;
                                        acEnt.Erase();
                                        acEnt.Dispose();
                                    }

                                if (mObjs.Count <= 0) continue;
                                {
                                    //Finished finding objects, now create tags
                                    var xform = psVp.Ms2Ps();

                                    foreach (ObjectId id in mObjs)
                                    {
                                        var dbObj = acTrans.GetObject(id, OpenMode.ForWrite);
                                        if (dbObj == null) continue;

                                        switch (dbObj)
                                        {
                                            case Solid3d acSol:
                                            {
                                                var solCen = acSol.MassProperties.Centroid;
                                                var solName = acSol.GetPartName();
                                                var insPt = solCen.TransformBy(xform).Flatten();

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
                                                    acText.UpdateXData("RCMARK", Enums.XDataCode.Info, acCurDb, acTrans);
                                                }

                                                break;
                                            }

                                            case BlockReference bRef:
                                            {
                                                var ext = acTrans.GetExtents(new ObjectId[] {bRef.ObjectId}, acCurDb);
                                                var cen = ext.MinPoint.GetMidPoint(ext.MaxPoint);

                                                var bName = bRef.Name;
                                                var insPt = cen.TransformBy(xform).Flatten();

                                                using (var acText = new MText())
                                                {
                                                    acText.TextHeight = SettingsUser.MarkTextHeight;
                                                    acText.Contents = bName;
                                                    acText.BackgroundFill = true;
                                                    acText.UseBackgroundColor = true;
                                                    acText.ShowBorders = true;
                                                    acText.Location = insPt;
                                                    acText.Attachment = AttachmentPoint.MiddleCenter;
                                                    //acText.Layer = ;
                                                    //acText.ColorIndex = ;                           

                                                    //Append the text
                                                    acCurDb.AppendEntity(acText, acTrans);
                                                    acText.UpdateXData("RCMARK", Enums.XDataCode.Info, acCurDb, acTrans);
                                                }

                                                break;
                                            }
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
}