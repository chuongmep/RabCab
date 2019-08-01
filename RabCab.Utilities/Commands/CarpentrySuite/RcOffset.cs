// -----------------------------------------------------------------------------------
//     <copyright file="RcOffset.cs" company="CraterSpace">
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
using RabCab.Extensions;
using RabCab.Settings;
using Exception = Autodesk.AutoCAD.Runtime.Exception;

namespace RabCab.Commands.CarpentrySuite
{
    internal class RcOffset
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_RCOFFSET",
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
        public void Cmd_RcOFfset()
        {
            if (!Agents.LicensingAgent.Check()) return;
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;


            //Prompt user to select a 3dFace
            var userSel = acCurEd.SelectSubentities(SubentityType.Face);

            if (userSel.Count <= 0) return;

            //Get the offset distance from the user
            var prSelOpts = new PromptDistanceOptions("\nEnter offset distance: ")
            {
                AllowNone = false,
                AllowZero = false,
                AllowNegative = true,
                DefaultValue = SettingsUser.RcOffsetDepth
            };

            var prSelRes = acCurEd.GetDistance(prSelOpts);

            if (prSelRes.Status != PromptStatus.OK) return;

            //Set the offset variable
            SettingsUser.RcOffsetDepth = prSelRes.Value;

            try
            {
                var objList = new List<OffsetObject>();

                //Start a transaction
                using (var acTrans = acCurDb.TransactionManager.StartTransaction())
                {
                    foreach (var (objectId, subEntList) in userSel)
                        if (objList.Any(n => n.ObjId == objectId))
                        {
                            var offsetObject = objList.Find(i => i.ObjId == objectId);

                            foreach (var subentityId in subEntList) offsetObject?.SubentIds.Add(subentityId);
                        }
                        else
                        {
                            var offsetObject = new OffsetObject(objectId);

                            foreach (var subentityId in subEntList) offsetObject?.SubentIds.Add(subentityId);

                            objList.Add(offsetObject);
                        }

                    foreach (var obj in objList)
                    {
                        var acSol = acTrans.GetObject(obj.ObjId, OpenMode.ForWrite) as Solid3d;

                        if (obj.SubentIds.Count > 0)
                            //Offset the faces
                            acSol?.OffsetFaces(obj.SubentIds.ToArray(), SettingsUser.RcOffsetDepth);
                    }

                    //Commit the transaction
                    acTrans.Commit();
                }
            }
            catch (Exception e)
            {
                acCurEd.WriteMessage(e.Message);
            }
        }
    }

    internal class OffsetObject
    {
        public ObjectId ObjId;
        public List<SubentityId> SubentIds;

        public OffsetObject(ObjectId objId)
        {
            ObjId = objId;
            SubentIds = new List<SubentityId>();
        }
    }
}