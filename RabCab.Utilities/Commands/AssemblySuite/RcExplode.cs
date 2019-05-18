// -----------------------------------------------------------------------------------
//     <copyright file="RcExplode.cs" company="CraterSpace">
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
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using RabCab.Agents;
using RabCab.Analysis;
using RabCab.Engine.Enumerators;
using RabCab.Extensions;
using RabCab.Settings;

namespace RabCab.Commands.AssemblySuite
{
    internal class RcExplode
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_RCEXPLODE",
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
        public void Cmd_RcExplode()
        {
            //Get the current document utilities
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            var objIds = acCurEd.GetFilteredSelection(Enums.DxfNameEnum._3Dsolid, false, null,
                "\nSelect solid assembly to explode: ");
            if (objIds.Length <= 0) return;

            var explodePoint = acCurEd.Get2DPoint("\nSelect insertion point for exploded view: ");

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                var extents = acTrans.GetExtents(objIds, acCurDb);
                var extCen = Solid3DExtensions.GetBoxCenter(extents.MinPoint, extents.MaxPoint);
                var centryMove = extCen.GetVectorTo(explodePoint.Convert3D());

                foreach (var obj in objIds)
                {
                    var acSol = acTrans.GetObject(obj, OpenMode.ForWrite) as Solid3d;
                    if (acSol == null) continue;

                    var cSol = acSol.Clone() as Solid3d;
                    if (cSol == null) continue;

                    var eInfo = new EntInfo(acSol, acCurDb, acTrans);
                    var cInfo = new EntInfo(cSol, acCurDb, acTrans);

                    acCurDb.AppendEntity(cSol, acTrans);

                    var pHandle = acSol.Handle;
                    var cHandle = cSol.Handle;

                    eInfo.ChildHandles.Add(cHandle);

                    cInfo.ParentHandle = pHandle;
                    cInfo.ChildHandles.Clear();

                    acSol.AddXData(eInfo, acCurDb, acTrans);
                    cSol.AddXData(cInfo, acCurDb, acTrans);

                    var cCen = cSol.MassProperties.Centroid;
                    var cPower = extCen.GetVectorTo(cCen).MultiplyBy(SettingsUser.ExplodePower);

                    cSol.TransformBy(Matrix3d.Displacement(cPower));
                    //cSol.TransformBy(Matrix3d.Displacement(centryMove));
                    
                }

                acTrans.Commit();
            }

        }
    }
}