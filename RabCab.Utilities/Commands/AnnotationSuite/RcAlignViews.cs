// -----------------------------------------------------------------------------------
//     <copyright file="RcAlignViews.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>04/11/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using RabCab.Engine.Enumerators;
using RabCab.Extensions;
using RabCab.Settings;

namespace RabCab.Commands.AnnotationSuite
{
    internal class RcAlignViews
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_ALIGNVIEWS",
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
        public void Cmd_AlignViews()
        {
            //Get the current document utilities
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            var viewRes = acCurEd.GetFilteredSelection(Enums.DxfNameEnum.Viewport, false, null,
                "\nSelect Viewports to align: ");
            if (viewRes.Length <= 0) return;

            var alignRes = acCurEd.GetFilteredSelection(Enums.DxfNameEnum.Viewport, true, null,
                "\nSelect Viewport to align to: ");
            if (alignRes.Length <= 0) return;

            var boolRes = acCurEd.GetBool("Align by which orientation? ", "Horizontal", "Vertical");
            if (boolRes == null) return;

            var horizontal = boolRes.Value;

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                var mainViewport = acTrans.GetObject(alignRes[0], OpenMode.ForRead) as Viewport;
                if (mainViewport == null) return;

                var mainX = mainViewport.CenterPoint.X;
                var mainY = mainViewport.CenterPoint.Y;

                foreach (var objId in viewRes)
                {
                    var alignView = acTrans.GetObject(objId, OpenMode.ForWrite) as Viewport;
                    if (alignView == null) continue;

                    var alignX = alignView.CenterPoint.X;
                    var alignY = alignView.CenterPoint.Y;

                    if (horizontal) //Horizontal
                        mainX = alignX;
                    else //Vertical
                        mainY = alignY;

                    alignView.CenterPoint = new Point3d(mainX, mainY, 0);
                }

                acTrans.Commit();
            }
        }
    }
}