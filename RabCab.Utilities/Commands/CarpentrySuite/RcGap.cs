// -----------------------------------------------------------------------------------
//     <copyright file="RcGap.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>04/08/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using RabCab.Agents;
using RabCab.Engine.Enumerators;
using RabCab.Extensions;
using RabCab.Settings;

namespace RabCab.Commands.CarpentrySuite
{
    internal class RcGap
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_RCGAP",
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
        public void Cmd_RcGap()
        {
            if (!LicensingAgent.Check()) return;
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            try
            {
                // Begin Transaction
                using (var acTrans = acCurDb.TransactionManager.StartTransaction())
                {
                    //Prompt To Select Solids to perform Boolean Operations On
                    var boolRes1 = acCurEd.GetFilteredSelection(Enums.DxfNameEnum._3Dsolid, false, null,
                        "\nSelect 3DSOLIDS to be subtracted from: ");

                    if (boolRes1.Length <= 0) return;

                    //Prompt To Select Solids to perform Boolean Operations On
                    var boolRes2 = acCurEd.GetFilteredSelection(Enums.DxfNameEnum._3Dsolid, false, null,
                        "\nSelect 3DSOLIDS to be used as gap criteria: ");

                    if (boolRes2.Length <= 0) return;

                    var prSelOpts = new PromptDistanceOptions("\nEnter gap distance: ")
                    {
                        AllowNone = false,
                        AllowZero = true,
                        AllowNegative = true,
                        DefaultValue = SettingsUser.RcGapDepth
                    };

                    //Get the offset distance
                    var prSelRes = acCurEd.GetDistance(prSelOpts);

                    if (prSelRes.Status != PromptStatus.OK) return;

                    SettingsUser.RcGapDepth = prSelRes.Value;

                    boolRes1.SolidGap(boolRes2, acCurDb, acTrans, false, SettingsUser.RcGapDepth);

                    // Commit Transaction
                    acTrans.Commit();
                }
            }
            catch (Exception e)
            {
                acCurEd.WriteMessage(e.Message);
                MailAgent.Report(e.Message);
            }
        }
    }
}