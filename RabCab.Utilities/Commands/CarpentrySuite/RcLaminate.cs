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
    internal class RcLaminate
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_LAMINATE",
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
        public void Cmd_RcLaminate()
        {
            if (!Agents.LicensingAgent.Check()) return;
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;


            //Prompt user to select a 3dFace
            var userSel = acCurEd.SelectSubentities(SubentityType.Face);

            if (userSel.Count <= 0) return;

            //Get the offset distance from the user
            var prSelOpts = new PromptDistanceOptions("\nEnter laminate thickness: ")
            {
                AllowNone = false,
                AllowZero = false,
                AllowNegative = false,
                DefaultValue = SettingsUser.LaminateThickness
            };

            var prSelRes = acCurEd.GetDistance(prSelOpts);

            if (prSelRes.Status != PromptStatus.OK) return;

            //Set the offset variable
            SettingsUser.LaminateThickness = prSelRes.Value;

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

                            foreach (var subentityId in subEntList) offsetObject.SubentIds.Add(subentityId);

                            objList.Add(offsetObject);
                        }

                    var fuseList = new List<ObjectId>();

                    var layer = acCurDb.GetCLayer(acTrans);

                    foreach (var obj in objList)
                    {
                        var acSol = acTrans.GetObject(obj.ObjId, OpenMode.ForWrite) as Solid3d;
                        if (acSol != null)
                        {
                            var subtSol = acSol.Clone() as Solid3d;
                            if (subtSol != null)
                            {
                                subtSol.SetPropertiesFrom(acSol);
                                acCurDb.AppendEntity(subtSol, acTrans);

                                if (obj.SubentIds.Count > 0)
                                {
                                    acSol.Layer = layer;
                                    acSol.OffsetFaces(obj.SubentIds.ToArray(), SettingsUser.LaminateThickness);
                                    acSol.Downgrade();

                                    var acBool1 = new[] {acSol.ObjectId};
                                    var acBool2 = new[] {subtSol.ObjectId};

                                    acBool1.SolidSubtrahend(acBool2, acCurDb, acTrans, false);

                                    fuseList.Add(obj.ObjId);
                                }
                            }
                        }
                    }

                    var fusedObjId = fuseList.ToArray().SolidFusion(acTrans, acCurDb, true);

                    var fusedObj = acTrans.GetObject(fusedObjId, OpenMode.ForWrite) as Solid3d;
                    if (fusedObj != null)
                    {
                        var sepObj = fusedObj.SeparateBody();

                        foreach (var o in sepObj)
                        {
                            o.SetPropertiesFrom(fusedObj);
                            acCurDb.AppendEntity(o, acTrans);
                        }
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
}

internal class OffsetObject
{
    private ObjectId _objId;
    private List<SubentityId> _subentIds;

    public OffsetObject(ObjectId objId)
    {
        _objId = objId;
        _subentIds = new List<SubentityId>();
    }
}