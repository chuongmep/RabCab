// -----------------------------------------------------------------------------------
//     <copyright file="RcFilter.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>04/11/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using System.Linq;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.Runtime;
using RabCab.Extensions;
using RabCab.Settings;

namespace RabCab.Commands.AnalysisSuite
{
    internal class RcFilter
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_QUICKFILTER",
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
        public void Cmd_QuickFilter()
        {
            //Get the current document utilities
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            //Create strings for use in keywords and filter testing
            var objKey1 = "2dObjects";
            var objKey2 = "3dObjects";
            var objKey3 = "Annotations";
            var objKey4 = "Blocks";
            var objKey5 = "Dimensions";
            var objKey6 = "Hatches";
            var objKey7 = "Text";

            var keys = new[]
            {
                objKey1, objKey2, objKey3, objKey4,
                objKey5, objKey6, objKey7
            };

            var userKey = acCurEd.GetSimpleKeyword("Which objects would you like to select?", keys);

            // Determine the type of selection filter to use, base on user input
            var dxfKey = "";

            if (userKey == objKey1) //2dObjects
                dxfKey = "*LINE,CIRCLE,ARC,ELLIPSE,POINT,RAY";
            else if (userKey == objKey2) //3dObjects
                dxfKey = "3DSOLID, SURFACE, MESH";
            else if (userKey == objKey3) //Annotations
                dxfKey = "*DIMENSION,*TEXT,*LEADER,*TABLE";
            else if (userKey == objKey4) //Blocks
                dxfKey = "INSERT";
            else if (userKey == objKey5) //Dimensions
                dxfKey = "*DIMENSION";
            else if (userKey == objKey6) //Hatches
                dxfKey = "HATCH";
            else if (userKey == objKey7) //Text
                dxfKey = "*TEXT";

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                var objIds = acCurEd.SelectAllOfType(dxfKey, acTrans);

                if (objIds.Any())
                    acCurEd.SetImpliedSelection(objIds);
                else
                    acCurEd.WriteMessage("\nNone exist in drawing.");

                acTrans.Commit();
            }
        }
    }
}