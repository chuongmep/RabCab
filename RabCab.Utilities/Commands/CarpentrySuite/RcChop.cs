using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using RabCab.Agents;
using RabCab.Extensions;
using RabCab.Settings;

namespace RabCab.Commands.CarpentrySuite
{
    internal class RcChop
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_RCCHOP",
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
        public void Cmd_RCChop()
        {
            //Get the current document utilities
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            //Call user to select a face
            var userSel = acCurEd.SelectSubentity(SubentityType.Face, "\nSelect a FACE to use as chopping criteria: ");

            if (userSel == null) return;
            if (userSel.Item1 == ObjectId.Null) return;
            if (userSel.Item2 == SubentityId.Null) return;

            var prSelOpts = new PromptDistanceOptions("\nEnter chop distance: ")
            {
                AllowNone = false,
                AllowZero = false,
                AllowNegative = false,
                DefaultValue = SettingsUser.RcChopDepth
            };

            //Get the offset distance
            var prSelRes = acCurEd.GetDistance(prSelOpts);


            if (prSelRes.Status != PromptStatus.OK) return;

            SettingsUser.RcChopDepth = prSelRes.Value;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (SettingsUser.RcChopDepth == 0) return;
            var sList = new List<Surface>();
            Entity faceEnt = null;
            Surface tempSurf = null;
            try
            {
                //Open a transaction
                using (var acTrans = acCurDb.TransactionManager.StartTransaction())
                {
                    var acSol = acTrans.GetObject(userSel.Item1, OpenMode.ForWrite) as Solid3d;
                    if (acSol == null)
                    {
                        acTrans.Abort();
                        return;
                    }

                    faceEnt = acSol.GetSubentity(userSel.Item2);

                    using (tempSurf = faceEnt.CreateSurfaceFromFace(acCurDb, acTrans, false))
                    {
                        var canSlice = true;
                        var multiplier = 1;

                        using (var pWorker = new ProgressAgent("Testing Chops: ", 100))
                        {
                            while (canSlice)
                            {
                                if (!pWorker.Progress()) break;

                                var sliceSurf =
                                    Surface.CreateOffsetSurface(tempSurf,
                                        -SettingsUser.RcChopDepth * multiplier) as Surface;

                                if (sliceSurf == null) break;

                                canSlice = CanSlice(acSol, sliceSurf);

                                if (!canSlice)
                                {
                                    sliceSurf.Dispose();
                                    continue;
                                }

                                sList.Add(sliceSurf);

                                multiplier++;
                            }

                            if (sList.Count > 0)
                            {
                                pWorker.SetTotalOperations(sList.Count);
                                pWorker.Reset("Chopping Solid: ");

                                foreach (var su in sList)
                                {
                                    if (!pWorker.Progress())
                                    {
                                        acTrans.Abort();
                                        break;
                                    }

                                    var obj = acSol.Slice(su, true);
                                    obj.SetPropertiesFrom(acSol);
                                    acCurDb.AppendEntity(obj);
                                }
                            }
                            else
                            {
                                acCurEd.WriteMessage("\nNo Chopping Criteria found!");
                            }
                        }
                    }

                    acTrans.Commit();
                }
            }
            catch (Exception e)
            {
                acCurEd.WriteMessage(e.Message);
            }
            finally
            {
                foreach (var suf in sList) suf.Dispose();

                if (faceEnt != null) faceEnt.Dispose();
                if (tempSurf != null) tempSurf.Dispose();
            }
        }

        private bool CanSlice(Solid3d acSol, Surface acSurf)
        {
            var canSlice = false;

            using (var testSol = acSol.Clone() as Solid3d)
            {
                using (var testSurf = acSurf.Clone() as Surface)
                {
                    if (testSol != null && testSurf != null)
                    {
                        using (var thSol = testSurf.Thicken(-.01, false))
                        {
                            if (testSol.CheckInterference(thSol)) canSlice = true;

                            thSol.Dispose();
                        }

                        testSurf.Dispose();
                        testSol.Dispose();
                    }
                }
            }

            return canSlice;
        }
    }
}