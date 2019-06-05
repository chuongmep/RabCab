// -----------------------------------------------------------------------------------
//     <copyright file="RcDrill.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>04/08/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Windows.Documents;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using RabCab.Extensions;
using RabCab.Settings;
using static RabCab.Engine.Enumerators.Enums;

namespace RabCab.Commands.CarpentrySuite
{
    internal class RcDrill
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_RCDrill",
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
        public void Cmd_Default()
        {
            //Get the current document utilities
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            var filterArgs = new[] {DxfNameEnum.Insert, DxfNameEnum._3Dsolid};

            var userSel = acCurEd.GetFilteredSelection(filterArgs, false, null);
            if (userSel.Length <= 0) return;

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                CutDrills(userSel, acCurDb, acTrans);
                acTrans.Commit();
            }
        }

        private void CutDrills(ObjectId[] objIds, Database acCurDb, Transaction acTrans)
        {
            List<Solid3d> dSols = new List<Solid3d>();
            List<Solid3d> bSols = new List<Solid3d>();
            List<BlockReference> bRefs = new List<BlockReference>();

            foreach (var obj in objIds)
            {
                var ent = acTrans.GetObject(obj, OpenMode.ForRead) as Entity;

                if (ent is BlockReference bRef)
                {
                    bRefs.Add(bRef);
                }
                else if (ent is Solid3d acSol)
                {
                    dSols.Add(acSol);
                }
            }

            foreach (var bRef in bRefs)
            {
               var objCol = bRef.ExplodeBlock(acTrans, acCurDb, false);

               foreach (ObjectId obj in objCol)
               {
                   var ent = acTrans.GetObject(obj, OpenMode.ForWrite) as Entity;
                   if (ent == null) continue;

                   if (ent is Solid3d acSol)
                   {
                       bSols.Add(acSol);
                   }
                   else
                   {
                      ent.Erase();
                      ent.Dispose();
                   }
               }
            }

            var drillCrits = new List<Solid3d>();

            //Find drills in dSols
            foreach (var d in dSols)
            {
                if (d.Layer == SettingsUser.RcHoles)
                {
                    drillCrits.Add(d);
                }
            }

            //Find drills in bSols
            foreach (var b in bSols)
            {
                if (b.Layer == SettingsUser.RcHoles)
                {
                    drillCrits.Add(b);
                }
            }

            var drillIds = new List<ObjectId>();

            //Remove drills from dSols and bSols
            foreach (var drill in drillCrits)
            {
                if (dSols.Contains(drill))
                    dSols.Remove(drill);

                if (bSols.Contains(drill))
                    bSols.Remove(drill);

                drillIds.Add(drill.ObjectId);
            }

            //Dispose of bSols
            foreach (var b in bSols)
            {
                b.Erase();
                b.Dispose();
            }

            bSols.Clear();

            foreach (var d in dSols)
            {
                var dId = new[] {d.ObjectId};

                dId.SolidSubtrahend(drillIds.ToArray(), acCurDb, acTrans, true);
            }

        }
    }
}