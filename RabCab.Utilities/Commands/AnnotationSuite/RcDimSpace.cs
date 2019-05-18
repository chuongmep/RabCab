// -----------------------------------------------------------------------------------
//     <copyright file="RcDimArrange.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>04/11/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using RabCab.Engine.Enumerators;
using RabCab.Extensions;
using RabCab.Settings;

namespace RabCab.Commands.AnnotationSuite
{
    internal class RcDimSpace
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_DIMSP",
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
        public void Cmd_DimSpace()
        {
            //TODO Add option to space baseline dimension
            
            //Get the current document utilities
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            var pId = acCurEd.GetFilteredSelection(Enums.DxfNameEnum.Dimension, true, null,
                "\nSelect baseline dimension: ");
            if (pId.Length <= 0) return;

            var objIds = acCurEd.GetFilteredSelection(Enums.DxfNameEnum.Dimension, false, null,
                "\nSelect dimensions to space: ");
            if (objIds.Length <= 0) return;

            var pDim = SelectionSet.FromObjectIds(pId);
            var dims = SelectionSet.FromObjectIds(objIds);

            acCurEd.Command("_.Dimspace", pDim, dims, "", SettingsUser.AnnoSpacing);
        }
    }
}