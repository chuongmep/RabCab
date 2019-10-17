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
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using RabCab.Agents;
using RabCab.Engine.AcSystem;
using RabCab.Engine.Enumerators;
using RabCab.Extensions;
using RabCab.Settings;

namespace RabCab.Commands.AssemblySuite
{
    internal class RcShrinkView
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_SHRINKVIEW",
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
            var pso = new PromptSelectionOptions { MessageForAdding = "\nSelect viewports to shrink: " };
            var sf = new SelectionFilter(new[] { new TypedValue((int)DxfCode.Start, "VIEWPORT") });

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

                            var largestPart = ObjectId.Null;
                            var cenView = new Point2d();
                            var largestExt = new Extents2d();

                            using (var psVp = acTrans.GetObject(objId, OpenMode.ForRead) as Viewport)
                            {
                                var mObjs = objId.GetModelObjects();

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
                                                    var solExt = acSol.GeometricExtents;
                                                    
                                                   //TODO finish

                                                    break;
                                                }

                                            case BlockReference bRef:
                                                {
                                                    var ext = acTrans.GetExtents(new ObjectId[] { bRef.ObjectId }, acCurDb);
                                                    var cen = ext.MinPoint.GetMidPoint(ext.MaxPoint);


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