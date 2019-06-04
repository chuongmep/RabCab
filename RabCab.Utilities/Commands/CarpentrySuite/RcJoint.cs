// -----------------------------------------------------------------------------------
//     <copyright file="RcJoint.cs" company="CraterSpace">
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
using System.Diagnostics;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.BoundaryRepresentation;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using RabCab.Agents;
using RabCab.Engine.Enumerators;
using RabCab.Extensions;
using RabCab.Settings;
using Exception = Autodesk.AutoCAD.Runtime.Exception;

namespace RabCab.Commands.CarpentrySuite
{
    internal class RcJoint
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_RCJOINT",
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
            //| CommandFlags.NoBlockEditor
            //| CommandFlags.NoActionRecording
            //| CommandFlags.ActionMacro
            //| CommandFlags.NoInferConstraint 
        )]
        public void Cmd_RcJoint()
        {
            //Get the current document utilities
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            //Prompt user to select solids to use as joint materials
            var userRes1 = acCurEd.GetFilteredSelection(Enums.DxfNameEnum._3Dsolid, false, null,
                "\nSelect 3DSOLIDS to extend: ");

            if (userRes1.Length <= 0) return;

            var userRes2 = acCurEd.GetFilteredSelection(Enums.DxfNameEnum._3Dsolid, false, null,
                "\nSelect 3DSOLIDS to cut joints into: ");

            if (userRes2.Length <= 0) return;

            //Get the Joint depth from the user
            var userDistOpt = new PromptDistanceOptions("")
            {
                DefaultValue = SettingsUser.RcJointDepth,
                Message = "\n Enter joint depth: ",
                AllowNone = false,
                AllowNegative = false,
                AllowZero = false
            };

            //Set the join depth
            var distRes = acCurEd.GetDistance(userDistOpt);

            if (distRes.Status != PromptStatus.OK) return;

            SettingsUser.RcJointDepth = distRes.Value;

            //Create lists to hold solids
            var joinerSols = new List<Solid3d>();
            var joinToSols = new List<Solid3d>();
            var joinerGroup = new List<Tuple<Solid3d, List<SubentityId>>>();

            try
            {
                //Open a transaction
                using (var acTrans = acCurDb.TransactionManager.StartTransaction())
                {
                    //Get the object Ids of all of the selected solids
                    var joinerIds = userRes1;
                    var joinToIds = userRes2;

                    //Iterate through the joiner IDs
                    joinerSols.AddRange(
                        joinerIds.Select(joinId => acTrans.GetObject(joinId, OpenMode.ForWrite) as Solid3d));

                    //Iterate through the joiner IDs
                    joinToSols.AddRange(
                        joinToIds.Select(joinToId => acTrans.GetObject(joinToId, OpenMode.ForWrite) as Solid3d));
                    var joinToIds2 = joinToIds.ToList();

                    try
                    {
                        //Iterate through the joining solids and find if they touch the join to solids
                        foreach (var joinSol in joinerSols)
                        {
                            var subIds = new List<SubentityId>();

                            //Get Full Subentity Path
                            ObjectId[] objIds = {joinSol.ObjectId};
                            var fSubPath = new FullSubentityPath(objIds,
                                new SubentityId(SubentityType.Null, IntPtr.Zero));

                            //Get BREP Subentities
                            using (var acBrep = new Brep(fSubPath))
                            {
                                var faceCollection = acBrep.Faces.ToList();

                                Debug.WriteLine(faceCollection.Count());

                                using (
                                    var pWorker = new ProgressAgent("Cutting Joints: ",
                                        faceCollection.Count()))
                                {
                                    //Offset each face and see if it interferes with a join to solid
                                    foreach (var jointFace in faceCollection)
                                    {
                                        //Progress progress bar or exit if ESC has been pressed
                                        if (!pWorker.Progress())
                                        {
                                            acTrans.Abort();
                                            return;
                                        }

                                        using (var tempTrans = new OpenCloseTransaction())
                                        {
                                            var checkJoiner =
                                                tempTrans.GetObject(joinSol.ObjectId, OpenMode.ForWrite) as Solid3d;

                                            SubentityId[] subentId =
                                            {
                                                checkJoiner.GetFsPath(jointFace.SubentityPath.SubentId).SubentId
                                            };
                                            checkJoiner?.OffsetFaces(subentId, 0.001);

                                            foreach (var checkSol in joinToSols)
                                            {
                                                var tempCheckSol =
                                                    tempTrans.GetObject(checkSol.ObjectId, OpenMode.ForRead) as Solid3d;
                                                if (checkJoiner != null && checkJoiner.CheckInterference(tempCheckSol))
                                                    subIds.Add(subentId[0]);
                                            }

                                            //Do not commit the transaction
                                            tempTrans.Abort();
                                        }
                                    }
                                }
                            }

                            //If touching faces found, add them to the group list
                            if (subIds.Count > 0) joinerGroup.Add(Tuple.Create(joinSol, subIds));
                        }
                    }
                    catch (Exception e)
                    {
                        acCurEd.WriteMessage("\n" + e.Message);
                        acTrans.Abort();
                    }

                    var joinerIds2 = new List<ObjectId>();

                    //Offset all joiner faces
                    try
                    {
                        foreach (var group in joinerGroup)
                        {
                            if (!group.Item1.IsWriteEnabled)
                                group.Item1.UpgradeOpen();

                            try
                            {
                                group.Item1.OffsetFaces(group.Item2.ToArray(), SettingsUser.RcJointDepth);
                                joinerIds2.Add(group.Item1.ObjectId);
                            }
                            catch (Exception e)
                            {
                                acCurEd.WriteMessage("\n" + e.Message);
                            }
                        }

                        //Open a transaction for subtracting solids
                        if (joinerIds2.Count > 0)
                            //Subtract the joint material from the join to parts
                            joinToIds2.ToArray().SolidSubtrahend(joinerIds2.ToArray(), acCurDb, acTrans, false);
                    }
                    catch (Exception e)
                    {
                        acCurEd.WriteMessage("\n" + e.Message);
                        acTrans.Abort();
                    }

                    //Commit the transaction
                    acTrans.Commit();
                }
            }
            catch (Exception e)
            {
                acCurEd.WriteMessage("\n" + e.Message);
            }
        }
    }
}