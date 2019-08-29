using System.Linq;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using RabCab.Agents;
using RabCab.Analysis;
using RabCab.Engine.Enumerators;
using RabCab.Extensions;
using RabCab.Settings;

namespace RabCab.Commands.CarpentrySuite
{
    internal class RcFlip
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_RCFLIP",
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
        public void Cmd_RcFlip()
        {
            if (!LicensingAgent.Check()) return;
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            //Prompt To Select Solids to perform Lay Command On
            var objIds = acCurEd.GetFilteredSelection(Enums.DxfNameEnum._3Dsolid, false, null,
                "\nSelect 3DSOLIDS to flip on an axis: ");

            // Exit if selection error
            if (objIds.Length <= 0) return;

            // Prompt User to Select the Axis to flip on
            var flipAxisOpt = new PromptKeywordOptions("\nChoose axis to rotate around: ");
            flipAxisOpt.Keywords.Add("X");
            flipAxisOpt.Keywords.Add("Y");
            flipAxisOpt.Keywords.Add("Z");
            flipAxisOpt.AllowNone = false;

            var flipAxisRes = acCurEd.GetKeywords(flipAxisOpt);

            if (flipAxisRes.Status != PromptStatus.OK) return;

            // Prompt User to Select the Axis to flip on
            var flipDegOpt = new PromptKeywordOptions("\nChoose degree to rotate around axis: ");
            flipDegOpt.Keywords.Add("90");
            flipDegOpt.Keywords.Add("180");
            flipDegOpt.Keywords.Add("270");
            flipDegOpt.AllowNone = false;

            var flipDegRes = acCurEd.GetKeywords(flipDegOpt);

            if (flipDegRes.Status != PromptStatus.OK) return;

            // Get the selected keyword as a string
            var flipAxisKey = flipAxisRes.StringResult;
            var flipDegKey = flipDegRes.StringResult;

            var parseCount = 1;

            // Set selection set to user selection
            var solObjIds = objIds;
            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                using (
                    var pWorker = new ProgressAgent("Flipping Solids:",
                        solObjIds.Count()))
                {
                    //Parse Data for each solid in the list
                    foreach (var acSolId in solObjIds)
                    {
                        //Progress progress bar or exit if ESC has been pressed
                        if (!pWorker.Progress())
                        {
                            acTrans.Abort();
                            return;
                        }

                        if (acSolId == ObjectId.Null) continue;

                        // Get the Solid Information To Create a Lay Matrix
                        var acSol = acTrans.GetObject(acSolId, OpenMode.ForWrite) as Solid3d;

                        //Populate the solid
                        var eInfo = new EntInfo(acSol, acCurDb, acTrans);
                        parseCount++;

                        //Flip the solid
                        switch (flipAxisKey)
                        {
                            case "X":

                                switch (flipDegKey)
                                {
                                    case "90":
                                        acSol?.TransformBy(eInfo.X90);
                                        break;
                                    case "180":
                                        acSol?.TransformBy(eInfo.X180);
                                        break;
                                    case "270":
                                        acSol?.TransformBy(eInfo.X270);
                                        break;
                                }

                                break;
                            case "Y":
                                switch (flipDegKey)
                                {
                                    case "90":
                                        acSol?.TransformBy(eInfo.Y90);
                                        break;
                                    case "180":
                                        acSol?.TransformBy(eInfo.Y180);
                                        break;
                                    case "270":
                                        acSol?.TransformBy(eInfo.Y270);
                                        break;
                                }

                                break;
                            case "Z":
                                switch (flipDegKey)
                                {
                                    case "90":
                                        acSol?.TransformBy(eInfo.Z90);
                                        break;
                                    case "180":
                                        acSol?.TransformBy(eInfo.Z180);
                                        break;
                                    case "270":
                                        acSol?.TransformBy(eInfo.Z270);
                                        break;
                                }

                                break;
                        }
                    }
                }

                acTrans.Commit();
            }
        }
    }
}