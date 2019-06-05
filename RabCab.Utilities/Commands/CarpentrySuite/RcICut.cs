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
    internal class RcICut
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_ICUT",
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
        public void Cmd_ICut()
        {
            //Get the current document utilities
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            //Call user to select a face
            var userSel = acCurEd.SelectSubentity(SubentityType.Face, "\nSelect a FACE to use as cutting criteria: ");

            if (userSel == null) return;
            if (userSel.Item1 == ObjectId.Null) return;
            if (userSel.Item2 == SubentityId.Null) return;

            var insetOpts = new PromptDistanceOptions("\nEnter inset distance: ")
            {
                AllowNone = false,
                AllowZero = false,
                AllowNegative = false,
                DefaultValue = SettingsUser.RcICutInset
            };

            //Get the offset distance
            var insetRes = acCurEd.GetDistance(insetOpts);

            if (insetRes.Status != PromptStatus.OK) return;
            
            SettingsUser.RcICutInset = insetRes.Value;

            var prSelOpts2 = new PromptDistanceOptions("\nEnter cut depth: ")
            {
                AllowNone = false,
                AllowZero = false,
                AllowNegative = false,
                DefaultValue = SettingsUser.RcICutDepth
            };

            //Get the offset distance
            var prDepthRes = acCurEd.GetDistance(prSelOpts2);

            if (prDepthRes.Status != PromptStatus.OK) return;

            SettingsUser.RcICutDepth = prDepthRes.Value;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (SettingsUser.RcICutDepth <= 0 || SettingsUser.RcICutInset <= 0) return;
            
            Entity faceEnt = null;
            Surface tempSurf = null;
            Solid3d tempSol = null;

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
                        var thickness = SettingsUser.RcICutDepth + SettingsUser.RcICutInset;

                        using (tempSol = tempSurf.Thicken(-(thickness * 2), true))
                        {
                            tempSol.OffsetBody(-SettingsUser.RcICutInset);

                            var cutSol = tempSol.Slice(tempSurf, true);

                            acSol.BooleanOperation(BooleanOperationType.BoolSubtract, tempSol);
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
                if (faceEnt != null) faceEnt.Dispose();
                if (tempSurf != null) tempSurf.Dispose();
                if (tempSol != null) tempSol.Dispose();
            }
        }
    }
    }
