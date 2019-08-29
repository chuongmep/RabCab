using System.Collections.Generic;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using RabCab.Agents;
using RabCab.Analysis;
using RabCab.Engine.Enumerators;
using RabCab.Extensions;
using RabCab.Settings;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace RabCab.Commands.CNCSuite
{
    internal class RcNest
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_RCNEST",
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
        public void Cmd_Nest()
        {
            if (!LicensingAgent.Check()) return;
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            var polyId = acCurEd.SelectClosedPolyline();
            if (polyId == ObjectId.Null) return;

            var nestIds = acCurEd.GetFilteredSelection(Enums.DxfNameEnum._3Dsolid, false, null,
                "\nSelect 3DSolids to nest inside of polyline.");
            if (nestIds.Length <= 0) return;

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                var poly = acTrans.GetObject(polyId, OpenMode.ForRead) as Polyline;
                if (poly != null)
                {
                    var solList = new List<Solid3d>();
                    var tooLargeList = new List<Solid3d>();
                    var pArea = poly.Area;


                    var layParts = true;
                    var rotateParts = false;

                    //TODO PackTo
                    //TODO OffsetFromEdge
                    //TODO GapBetween

                    foreach (var objId in nestIds)
                    {
                        var acSol = acTrans.GetObject(objId, OpenMode.ForRead) as Solid3d;
                        if (acSol == null) continue;
                        var eInfo = new EntInfo(acSol, acCurDb, acTrans);

                        var solClone = acSol.Clone() as Solid3d;

                        if (solClone == null) continue;

                        if (layParts) solClone.TransformBy(eInfo.LayMatrix);

                        acCurDb.AppendEntity(solClone);
                        solClone.TopLeftToOrigin();

                        var geomExt = solClone.GeometricExtents;
                        var geomMin = geomExt.MinPoint.Flatten();
                        var geomMax = geomExt.MaxPoint.Flatten();
                        var geomArea = (geomMax.X - geomMin.X) * (geomMax.Y - geomMin.Y);

                        if (geomArea > pArea)
                            //TODO check every angle
                            tooLargeList.Add(solClone);
                        else
                            solList.Add(solClone);
                    }


                    MessageBox.Show(tooLargeList.Count + " Solids are too large!");
                }

                acTrans.Commit();
            }
        }
    }
}