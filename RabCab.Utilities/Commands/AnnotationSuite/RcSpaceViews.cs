using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using RabCab.Agents;
using RabCab.Engine.Enumerators;
using RabCab.Extensions;
using RabCab.Settings;

namespace RabCab.Commands.AnnotationSuite
{
    class RcSpaceViews
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_SPACEVIEWS",
            CommandFlags.Modal
            //| CommandFlags.Transparent
            //| CommandFlags.UsePickSet
            //| CommandFlags.Redraw
            //| CommandFlags.NoPerspective
            //| CommandFlags.NoMultiple
            | CommandFlags.NoTileMode
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
            | CommandFlags.NoBlockEditor
            | CommandFlags.NoActionRecording
            | CommandFlags.ActionMacro
            //| CommandFlags.NoInferConstraint 
        )]
        public void Cmd_SpaceViews()
        {
            //Get the current document utilities
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            var viewRes = acCurEd.GetFilteredSelection(Enums.DxfNameEnum.Viewport, false, null, "\nSelect Viewports to align: ");
            if (viewRes.Length <= 0) return;

            var spaceBool = acCurEd.GetBool("Space Equally? ");
            if (spaceBool == null) return;

            var equalSpace = spaceBool.Value;

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                if (equalSpace)
                {
                    if (viewRes.Length < 3)
                    {
                        acCurEd.WriteMessage("At least 3 Viewports must be selected to space equally.");
                        acTrans.Abort();
                        return;
                    }

                    var groupExt = acTrans.GetExtents(viewRes, acCurDb);
                    var groupMax = groupExt.MaxPoint;
                    var groupMin = groupExt.MinPoint;

                    var closestIdToMax = ObjectId.Null;
                    var closestIdToMin = ObjectId.Null;
                    double closestMaxDist = double.MaxValue;
                    double closestMinDist = double.MaxValue;

                    foreach (var obj in viewRes)
                    {
                        var acView = acTrans.GetObject(obj, OpenMode.ForRead) as Viewport;
                        if (acView == null) continue;
                        var acBounds = acView.Bounds;
                        if (acBounds == null) continue;

                        var acViewMax = acBounds.Value.MaxPoint;
                        var acViewMin = acBounds.Value.MinPoint;
                        var distToMax = acViewMax.DistanceTo(groupMax);
                        var distToMin = acViewMin.DistanceTo(groupMin);

                        if (distToMax < closestMaxDist)
                        {
                            closestIdToMax = obj;
                            closestMaxDist = distToMax;
                        }

                        if (distToMin < closestMinDist)
                        {
                            closestIdToMin = obj;
                            closestMinDist = distToMin;
                        }
                    }

                    if (closestIdToMax == ObjectId.Null || closestIdToMin == ObjectId.Null)
                    {
                        acTrans.Abort();
                        return;
                    }

                    var maxView = acTrans.GetObject(closestIdToMax, OpenMode.ForRead) as Viewport;
                    var minView = acTrans.GetObject(closestIdToMin, OpenMode.ForRead) as Viewport;
                    if (maxView == null || minView == null)
                    {
                        acTrans.Abort();
                        return;
                    }

                    var viewList = viewRes.ToList();
                    var divCount = viewList.Count - 1;

                    viewList.Remove(closestIdToMax);
                    viewList.Remove(closestIdToMin);

                    var maxCen = maxView.CenterPoint;
                    var minCen = minView.CenterPoint;

                    var fullDist = minCen.DistanceTo(maxCen);
                    var divDist = fullDist / divCount;

                    for (var index = 0; index < viewList.Count; index++)
                    {
                        var viewId = viewList[index];
                        var acView = acTrans.GetObject(viewId, OpenMode.ForWrite) as Viewport;
                        if (acView == null) continue;

                        acView.CenterPoint = minCen.GetAlong(maxCen, divDist * (index + 1));
                    }
                }
                else
                {
                   
                }

                acTrans.Commit();
            }


        }
    }
}
