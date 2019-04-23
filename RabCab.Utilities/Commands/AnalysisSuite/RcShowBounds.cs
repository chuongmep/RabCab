using Autodesk.AutoCAD.ApplicationServices.Core;
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
            | CommandFlags.Transparent
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

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                var extents = acTrans.GetExtents(objIds, acCurDb);

                // now create a boundary polyline to hilite my structure that need attention:
                Polyline3d poly = new Polyline3d(Poly3dType.SimplePoly, new Point3dCollection(){extents.MinPoint, extents.MaxPoint},false );

                TransientAgent.Add(poly);
                TransientAgent.Draw();

                var exit = acCurEd.GetString("");

                acTrans.Commit();
            }
        }
    }
}