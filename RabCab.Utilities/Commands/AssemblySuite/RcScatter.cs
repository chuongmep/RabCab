// -----------------------------------------------------------------------------------
//     <copyright file="RcScatter.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>04/08/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using System;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using RabCab.Extensions;
using RabCab.Settings;

namespace RabCab.Commands.AssemblySuite
{
    internal class RcScatter
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_SCATTER",
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
        public void Cmd_Scatter()
        {
            //Get the current document utilities
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            var objIds = acCurEd.GetAllSelection(false);
            if (objIds.Length <= 0) return;

            var random = new Random();

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                var extents = acTrans.GetExtents(objIds, acCurDb);
                var extCen = Solid3DExtensions.GetBoxCenter(extents.MinPoint, extents.MaxPoint);

                foreach (var obj in objIds)
                {
                    var acEnt = acTrans.GetObject(obj, OpenMode.ForWrite) as Entity;
                    if (acEnt == null) continue;

                    var entExt = acTrans.GetExtents(new[] {acEnt.ObjectId}, acCurDb);
                    var entCen = Solid3DExtensions.GetBoxCenter(entExt.MinPoint, entExt.MaxPoint);

                    var cPower = extCen.GetVectorTo(entCen).MultiplyBy(random.NextDouble());

                    acEnt.TransformBy(Matrix3d.Displacement(cPower));
                }

                acTrans.Commit();
            }
        }
    }
}