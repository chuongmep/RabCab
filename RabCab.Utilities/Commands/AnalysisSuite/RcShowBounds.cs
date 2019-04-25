using System;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using RabCab.Agents;
using RabCab.Extensions;
using RabCab.Settings;

namespace RabCab.Commands.AnalysisSuite
{
    internal class RcShowBounds
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_SHOWBOUNDS",
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
        public void Cmd_ShowBounds()
        {
            //Get the current document utilities
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            var objIds = acCurEd.GetAllSelection(false);
            var boolRes = acCurEd.GetBool("Save bounds? ");
            if (boolRes == null)
               return;

            var append = boolRes.Value;

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                var extents = acTrans.GetExtents(objIds, acCurDb);

                //Get geom extents of all selected
                var minX = extents.MinPoint.X;
                var maxX = extents.MaxPoint.X;
                var minY = extents.MinPoint.Y;
                var maxY = extents.MaxPoint.Y;
                var minZ = extents.MinPoint.Z;
                var maxZ = extents.MaxPoint.Z;

                var sol = new Solid3d();
                
                    var width = Math.Abs(maxX - minX);
                    var length = Math.Abs(maxY - minY);
                    var height = Math.Abs(maxZ - minZ);

                    sol.CreateBox(width, length, height);
                    sol.TransformBy(
                        Matrix3d.Displacement(sol.GeometricExtents.MinPoint.GetVectorTo(new Point3d(minX, minY, minZ))));
                    sol.Transparency = new Transparency(75);
                    sol.Color = Colors.LayerColorBounds;

                    TransientAgent.Add(sol);
                    TransientAgent.Draw();               

                    if (append)
                    {
                        acCurDb.AppendEntity(sol);
                    }
                    else
                    {
                        sol.Dispose();
                    }

                    TransientAgent.Clear();


                acTrans.Commit();
            }
        }
    }
}