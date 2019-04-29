// -----------------------------------------------------------------------------------
//     <copyright file="RcFlatten.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>04/08/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using RabCab.Agents;
using RabCab.Engine.Enumerators;
using RabCab.Extensions;
using RabCab.Settings;

namespace RabCab.Commands.CNCSuite
{
    internal class RcFlatten
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_RCFLAT",
            CommandFlags.Modal
            //| CommandFlags.Transparent
            //| CommandFlags.UsePickSet
            //| CommandFlags.Redraw
            //| CommandFlags.NoPerspective
            //| CommandFlags.NoMultiple
            //| CommandFlags.NoTileMode
            | CommandFlags.NoPaperSpace
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
        public void Cmd_RcFlatten()
        {
            //Get the current document utilities
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            var keys = new List<KeywordAgent>();

            var keyFlatAssembly = new KeywordAgent(acCurEd, "assemBly", "Flatten Assembly? ", TypeCode.Boolean,
                SettingsUser.FlattenAssembly.ToSpecified());

            var keyFlatAllSides = new KeywordAgent(acCurEd, "allSides", "Flatten All Sides? ", TypeCode.Boolean,
                SettingsUser.FlattenAllSides.ToSpecified());

            var keyRetainHidden = new KeywordAgent(acCurEd, "retainHidden", "Retain Hidden Lines? ", TypeCode.Boolean,
                SettingsUser.RetainHiddenLines.ToSpecified());

            keys.Add(keyFlatAssembly);
            keys.Add(keyFlatAllSides);
            keys.Add(keyRetainHidden);

            var objIds =
                acCurEd.GetFilteredSelection(Enums.DxfNameEnum._3Dsolid, false, keys, "\nSelect solids to flatten: ");
            if (objIds.Length <= 0) return;

            keyFlatAssembly.Set(ref SettingsUser.FlattenAssembly);
            keyFlatAllSides.Set(ref SettingsUser.FlattenAllSides);
            keyRetainHidden.Set(ref SettingsUser.RetainHiddenLines);

            using (var pWorker = new ProgressAgent("Flattening Solids: ", objIds.Length))
            {
                using (var acTrans = acCurDb.TransactionManager.StartTransaction())
                {

                    //Set the UCS to World - save the user UCS
                    var userCoordSystem = acCurEd.CurrentUserCoordinateSystem;
                    acCurEd.CurrentUserCoordinateSystem = Matrix3d.Identity;

                    if (SettingsUser.FlattenAssembly || SettingsUser.FlattenAllSides)
                    {
                        var objId = objIds.SolidFusion(acTrans, acCurDb, true);
                        objIds = new[] { objId };
                    }

                    foreach (var obj in objIds)
                    {
                        if (!pWorker.Tick())
                        {
                            acTrans.Abort();
                        }

                        using (var acSol = acTrans.GetObject(obj, OpenMode.ForWrite) as Solid3d)
                        {
                            if (acSol != null)
                            {
                                if (SettingsUser.FlattenAllSides)
                                {
                                    acSol.FlattenAllSides(acCurDb, acCurEd, acTrans);                    
                                }
                                else
                                {
                                    acSol.Flatten(acTrans, acCurDb, acCurEd, true, false, true, userCoordSystem);
                                    if (SettingsUser.RetainHiddenLines)
                                        acSol.Flatten(acTrans, acCurDb, acCurEd, false, true, true, userCoordSystem);

                                    acSol.Erase();
                                    acSol.Dispose();
                                }
                            
                            }
                    
                        }
                    }

                    acCurEd.CurrentUserCoordinateSystem = userCoordSystem;
                    acTrans.Commit();
                }


            }
        }

        
    }
}