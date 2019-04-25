﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.Runtime;
using RabCab.Agents;
using RabCab.Engine.Enumerators;
using RabCab.Engine.System;
using RabCab.Extensions;
using RabCab.Settings;
using Viewport = Autodesk.AutoCAD.DatabaseServices.Viewport;

namespace RabCab.Commands.AnnotationSuite
{
    internal class RcSpaceViews
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

            var keyList = new List<KeywordAgent>();
            var keyAgentSpace = new KeywordAgent(acCurEd, "SpaceEqually", "Space viewports equally between two viewports? ", TypeCode.Boolean);

            keyList.Add(keyAgentSpace);

            var viewRes = acCurEd.GetFilteredSelection(Enums.DxfNameEnum.Viewport, false, keyList,
                "\nSelect Viewports to space or: ");
            if (viewRes.Length <= 0) return;

            var equalSpace = false;

            keyAgentSpace.Set(ref equalSpace);

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                var views = viewRes.ToList();

                var firstViewAr = acCurEd.GetFilteredSelection(Enums.DxfNameEnum.Viewport, true, null,
                    "\nSelect first viewport to use as spacing criteria: ");
                if (firstViewAr.Length <= 0)
                {
                    acTrans.Abort();
                    return;
                }

                var firstView = firstViewAr[0];
                var fViewport = acTrans.GetObject(firstView, OpenMode.ForRead) as Viewport;

                if (fViewport == null)
                {
                    acTrans.Abort();
                    return;
                }

                var fCen = fViewport.CenterPoint;

                if (views.Contains(firstView)) views.Remove(firstView);

                if (equalSpace)
                {
                    var lastViewAr = acCurEd.GetFilteredSelection(Enums.DxfNameEnum.Viewport, true, null,
                        "\nSelect last viewport to use as spacing criteria: ");

                    if (lastViewAr.Length <= 0)
                    {
                        acTrans.Abort();
                        return;
                    }

                    var lastView = lastViewAr[0];
                    var lViewport = acTrans.GetObject(lastView, OpenMode.ForRead) as Viewport;

                    if (lViewport == null)
                    {
                        acTrans.Abort();
                        return;
                    }

                    var lCen = lViewport.CenterPoint;

                    if (views.Contains(lastView)) views.Remove(lastView);

                    var dist = fCen.DistanceTo(lCen);
                    var dSpace = dist / (views.Count + 1);

                    for (var index = 0; index < views.Count; index++)
                    {
                        var view = views[index];
                        var acViewport = acTrans.GetObject(view, OpenMode.ForWrite) as Viewport;
                        if (acViewport == null) continue;

                        acViewport.CenterPoint =
                            fCen.GetAlong(lCen, dSpace * (index + 1));
                    }
                }
                else
                {
                    var dist = acCurEd.GetPositiveDistance("Enter distance to space between viewports: ");
                    if (dist != 0)
                    {
                        var startPoint = fCen;                                    
               
                        //prompt for end point
                        var endPtOpts = new PromptPointOptions("\nSelect direction for spacing: ")
                        {
                            UseBasePoint = true, UseDashedLine = true, BasePoint = startPoint, AllowNone = false
                        };

                        var endPtRes = acCurEd.GetPoint(endPtOpts);

                        if (endPtRes.Status == PromptStatus.OK)
                        {
                            var endPt = endPtRes.Value;

                            if (AcVars.OrthoMode == Enums.OrthoMode.On)
                            {
                                endPt = endPt.GetOrthoPoint(startPoint);
                            }                      

                            var lastPt = fCen;

                            foreach (var view in views)
                            {
                                var acViewport = acTrans.GetObject(view, OpenMode.ForWrite) as Viewport;
                                if (acViewport == null) continue;

                                //TODO add correct spacing here to get distance across viewport bounds at the given vector
                                lastPt = lastPt.GetAlong(endPt, dist);
                                acViewport.CenterPoint = lastPt;                               
          
                            }
                        }
                    }
                }

                acTrans.Commit();
            }
        }
    }
}