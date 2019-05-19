// -----------------------------------------------------------------------------------
//     <copyright file="RcAutoLayer.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>04/08/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using RabCab.Extensions;
using RabCab.Settings;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = Autodesk.AutoCAD.Runtime.Exception;

namespace RabCab.Commands.AutomationSuite
{
    internal class RcAutoLayer
    {
        private static ObjectId _userLayer = ObjectId.Null;

        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_AUTOLAYER",
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
        public void Cmd_AutoLayer()
        {
            //Get the current document utilities
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            var enable = acCurEd.GetBool("\nSet AutoLayer variable: ", "On", "Off");
            if (enable != null) return;

            try
            {
                SettingsUser.AutoLayerEnabled = enable.Value;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static void autoLayer_CommandWillStart(object sender, CommandEventArgs e)
        {
            if (!SettingsUser.AutoLayerEnabled) return;

            var acCurDoc = (Document) sender;
            if (acCurDoc == null || acCurDoc.IsDisposed || !acCurDoc.IsActive) return;

            var acCurDb = acCurDoc.Database;

            if (!SettingsUser.LayerCommandList.Contains(e.GlobalCommandName)) return;
            {
                try
                {
                    _userLayer = acCurDb.Clayer;
                    using (var acTrans = acCurDb.TransactionManager.StartTransaction())
                    {
                        acCurDb.AddLayer(SettingsUser.RcAnno, Colors.LayerColorRcAnno, SettingsUser.RcAnnoLt, acTrans);

                        // Open the Layer table for read
                        var acLyrTbl = acTrans.GetObject(acCurDb.LayerTableId,
                            OpenMode.ForRead) as LayerTable;

                        string sLayerName = SettingsUser.RcAnno;

                        if (acLyrTbl != null && acLyrTbl.Has(sLayerName) == true)
                        {
                            // Set the layer Center current
                            acCurDb.Clayer = acLyrTbl[sLayerName];

                            // Save the changes
                            acTrans.Commit();
                        }
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
            }

        }

        /// <summary>
        ///     Method to run when commands are ended
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void autoLayer_CommandEnded(object sender, CommandEventArgs e)
        {
            if (!SettingsUser.AutoLayerEnabled) return;

            var acCurDoc = (Document) sender;
            if (acCurDoc == null || acCurDoc.IsDisposed || !acCurDoc.IsActive) return;

            var acCurDb = acCurDoc.Database;

            if (!SettingsUser.LayerCommandList.Contains(e.GlobalCommandName) || _userLayer == ObjectId.Null) return;
            {
                acCurDb.Clayer = _userLayer;
                _userLayer = ObjectId.Null;
            }
        }
    }
}