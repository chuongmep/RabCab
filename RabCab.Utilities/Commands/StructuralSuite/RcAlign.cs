// -----------------------------------------------------------------------------------
//     <copyright file="RcAlignEnts.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>04/10/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using RabCab.Agents;
using RabCab.Extensions;
using RabCab.Settings;

namespace RabCab.Commands.StructuralSuite
{
    internal class RcAlign
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_RCALIGN",
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
        public void Cmd_RcAlign()
        {
            if (!LicensingAgent.Check()) return;
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            var objIds = acCurEd.GetAllSelection(false);
            if (objIds.Length <= 0) return;

            var alId = acCurEd.GetPoint("\nSelect point to align objects to: ");
            var alignPt = alId.Value;
            var alignX = alignPt.X;

            var alignType = acCurEd.GetSimpleKeyword("\nEnter alignment type: ", new[] {"Left", "Center", "Right"});

            if (string.IsNullOrEmpty(alignType)) return;
            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                foreach (var obj in objIds)
                {
                    var acEnt = acTrans.GetObject(obj, OpenMode.ForWrite) as Entity;
                    if (acEnt == null) continue;
                    var geomExt = acEnt.GeometricExtents;
                    var cenPt = geomExt.MinPoint.GetMidPoint(geomExt.MaxPoint);

                    switch (alignType)
                    {
                        case "Left":
                            acEnt.TransformBy(Matrix3d.Displacement(
                                geomExt.MinPoint.GetVectorTo(new Point3d(alignX, geomExt.MinPoint.Y,
                                    geomExt.MinPoint.Z))));
                            break;
                        case "Center":
                            acEnt.TransformBy(Matrix3d.Displacement(
                                cenPt.GetVectorTo(new Point3d(alignX, cenPt.Y,
                                    cenPt.Z))));
                            break;
                        case "Right":
                            acEnt.TransformBy(Matrix3d.Displacement(
                                geomExt.MaxPoint.GetVectorTo(new Point3d(alignX, geomExt.MaxPoint.Y,
                                    geomExt.MaxPoint.Z))));
                            break;
                    }
                }

                acTrans.Commit();
            }
        }
    }
}