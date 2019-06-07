using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using RabCab.Analysis;
using RabCab.Extensions;
using RabCab.Settings;
using Exception = Autodesk.AutoCAD.Runtime.Exception;

namespace RabCab.Commands.CarpentrySuite
{
    class RcEdgeBand
    {

        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_EDGEBAND",
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
        public void Cmd_EdgeBand()
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

            var insetOpts = new PromptDistanceOptions("\nEnter edge banding thickness: ")
            {
                AllowNone = false,
                AllowZero = false,
                AllowNegative = false,
                DefaultValue = SettingsUser.EdgeBandThickness
            };

            //Get the offset distance
            var insetRes = acCurEd.GetDistance(insetOpts);

            if (insetRes.Status != PromptStatus.OK) return;

            SettingsUser.EdgeBandThickness = insetRes.Value;


            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (SettingsUser.EdgeBandThickness <= 0) return;

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

                    var innerSol = acSol.Clone() as Solid3d;

                    if (innerSol == null)
                    {
                        acTrans.Abort();
                        return;
                    }

                    acSol.Layer = acCurDb.GetCLayer(acTrans);
                    acCurDb.AppendEntity(innerSol);

                    faceEnt = acSol.GetSubentity(userSel.Item2);

                    var eInfo = new EntInfo(acSol, acCurDb, acTrans);
                    var largestM = eInfo.GetLargestMeasurement();

                    using (tempSurf = faceEnt.CreateSurfaceFromFace(acCurDb, acTrans, false))
                    {
                        var thickness = largestM + SettingsUser.EdgeBandThickness;

                        using (tempSol = tempSurf.Thicken(-(thickness * 2), true))
                        {
                            tempSol.OffsetBody(-SettingsUser.EdgeBandThickness);

                            var cutSol = tempSol.Slice(tempSurf, true);
                            cutSol.Dispose();

                            acSol.BooleanOperation(BooleanOperationType.BoolSubtract, tempSol);
                        }
                    }

                    var acBool1 = new[] {innerSol.ObjectId};
                    var acBool2 = new[] {acSol.ObjectId};

                    acBool1.SolidSubtrahend(acBool2, acCurDb, acTrans, false);

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
