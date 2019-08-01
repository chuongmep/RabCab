// -----------------------------------------------------------------------------------
//     <copyright file="RcGenDims.cs" company="CraterSpace">
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
using RabCab.Calculators;
using RabCab.Extensions;
using RabCab.Settings;

namespace RabCab.Commands.AnnotationSuite
{
    internal class RcGenDims
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_GENDIMS",
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
        public void Cmd_GenDims()
        {
            if (!Agents.LicensingAgent.Check()) return;
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            var objIds = acCurEd.GetAllSelection(false);
            if (objIds.Length <= 0) return;

            using (var pWorker = new ProgressAgent("Generating Dimension: ", objIds.Length))
            {
                using (var acTrans = acCurDb.TransactionManager.StartTransaction())
                {
                    foreach (var obj in objIds)
                    {
                        if (!pWorker.Progress())
                        {
                            acTrans.Abort();
                            return;
                        }

                        var acEnt = acTrans.GetObject(obj, OpenMode.ForRead) as Entity;
                        if (acEnt == null) continue;

                        var extents = acEnt.GeometricExtents;

                        var minPt = extents.MinPoint;
                        var maxPt = extents.MaxPoint;

                        // Create the rotated dimension
                        using (var acXDim = new RotatedDimension())
                        {
                            acXDim.XLine1Point = new Point3d(minPt.X, minPt.Y, 0);
                            acXDim.XLine2Point = new Point3d(maxPt.X, minPt.Y, 0);
                            acXDim.DimLinePoint = new Point3d(minPt.X, minPt.Y - SettingsUser.AnnoSpacing, 0);
                            acXDim.DimensionStyle = acCurDb.Dimstyle;

                            // Add the new object to Model space and the transaction
                            acCurDb.AppendEntity(acXDim);
                        }

                        // Create the rotated dimension
                        using (var acYDim = new RotatedDimension())
                        {
                            acYDim.XLine1Point = new Point3d(minPt.X, minPt.Y, 0);
                            acYDim.XLine2Point = new Point3d(minPt.X, maxPt.Y, 0);
                            acYDim.Rotation = CalcUnit.ConvertToRadians(90);
                            acYDim.DimLinePoint = new Point3d(minPt.X - SettingsUser.AnnoSpacing, minPt.Y, 0);
                            acYDim.DimensionStyle = acCurDb.Dimstyle;

                            // Add the new object to Model space and the transaction
                            acCurDb.AppendEntity(acYDim);
                        }
                    }

                    acTrans.Commit();
                }
            }
        }
    }
}