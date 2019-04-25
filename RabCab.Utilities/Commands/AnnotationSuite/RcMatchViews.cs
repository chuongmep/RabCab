using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using RabCab.Engine.Enumerators;
using RabCab.Extensions;
using RabCab.Settings;

namespace RabCab.Commands.AnnotationSuite
{
    class RcMatchViews
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_MATCHVIEWS",
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
        public void Cmd_MatchViews()
        {
            //Get the current document utilities
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            var viewRes = acCurEd.GetFilteredSelection(Enums.DxfNameEnum.Viewport, false, null, "\nSelect Viewports to resize: ");
            if (viewRes.Length <= 0) return;

            var alignRes = acCurEd.GetFilteredSelection(Enums.DxfNameEnum.Viewport, true, null, "\nSelect Viewport to match: ");
            if (alignRes.Length <= 0) return;

            const string key1 = "Height";
            const string key2 = "Width";
            const string key3 = "Both";


            var keyRes = acCurEd.GetSimpleKeyword("Match which size: ", new[] { key1, key2, key3});
            if (string.IsNullOrEmpty(keyRes)) return;


            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {

                var mainViewport = acTrans.GetObject(alignRes[0], OpenMode.ForRead) as Viewport;
                if (mainViewport == null) return;

                var mainHeight = mainViewport.Height;
                var mainWidth = mainViewport.Width;
                var mainBounds = mainViewport.Bounds;
                if (mainBounds == null) return;

                var topY = mainBounds.Value.MaxPoint.Y;
                var botY = mainBounds.Value.MinPoint.Y;
                var leftX = mainBounds.Value.MinPoint.X;
                var rightX = mainBounds.Value.MaxPoint.X;

                foreach (var objId in viewRes)
                {
                    var alignView = acTrans.GetObject(objId, OpenMode.ForWrite) as Viewport;
                    if (alignView == null) continue;

                    switch (keyRes)
                    {
                        case key1://Height
                            alignView.Height = mainHeight;
                            break;

                        case key2://Width
                            alignView.Width = mainWidth;
                            break;

                        case key3: //Both
                            alignView.Height = mainHeight;
                            alignView.Width = mainWidth;
                            break;

                    }

                }

                acTrans.Commit();

            }

        }
    }
}
