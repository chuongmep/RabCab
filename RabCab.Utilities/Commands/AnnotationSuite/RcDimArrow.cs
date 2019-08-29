using System;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using RabCab.Agents;
using RabCab.Engine.Enumerators;
using RabCab.Entities.Annotation;
using RabCab.Settings;
using Exception = Autodesk.AutoCAD.Runtime.Exception;

namespace RabCab.Commands.AnnotationSuite
{
    internal class RcDimArrow
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_DIMARROW",
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
        public void Cmd_DimArrow()
        {
            if (!LicensingAgent.Check()) return;
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            var prEntOpt = new PromptEntityOptions("\nSelect a dimensions arrow to convert to arrow-type <" +
                                                   EnumAgent.GetNameOf(SettingsUser.ArwHead) + "> : ");

            prEntOpt.SetRejectMessage("\nOnly dimensions may be selected.");
            prEntOpt.AllowNone = false;
            prEntOpt.AddAllowedClass(typeof(AlignedDimension), false);
            prEntOpt.AddAllowedClass(typeof(RotatedDimension), false);
            prEntOpt.AddAllowedClass(typeof(ArcDimension), false);

            var prEntRes = acCurEd.GetEntity(prEntOpt);
            var prPickPoint = prEntRes.PickedPoint;

            if (prEntRes.Status != PromptStatus.OK) return;

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                var acDim = acTrans.GetObject(prEntRes.ObjectId, OpenMode.ForWrite) as Entity;
                if (acDim != null)
                    try
                    {
                        Point3d xPt1;
                        Point3d xPt2;
                        var arwString = EnumAgent.GetNameOf(SettingsUser.ArwHead);

                        var arwId = DimSystem.GetArrowId(arwString);

                        switch (acDim)
                        {
                            case AlignedDimension alDim:
                            {
                                alDim.Dimsah = true;
                                xPt1 = alDim.XLine1Point;
                                xPt2 = alDim.XLine2Point;

                                if (prPickPoint.DistanceTo(xPt1) < prPickPoint.DistanceTo(xPt2))
                                    alDim.Dimblk1 = arwId;
                                else
                                    alDim.Dimblk2 = arwId;
                                alDim.RecomputeDimensionBlock(true);
                                break;
                            }

                            case RotatedDimension roDim:
                            {
                                roDim.Dimsah = true;
                                xPt1 = roDim.XLine1Point;
                                xPt2 = roDim.XLine2Point;

                                if (prPickPoint.DistanceTo(xPt1) < prPickPoint.DistanceTo(xPt2))
                                    roDim.Dimblk1 = arwId;
                                else
                                    roDim.Dimblk2 = arwId;
                                roDim.RecomputeDimensionBlock(true);
                                break;
                            }

                            case ArcDimension arDim:
                            {
                                arDim.Dimsah = true;
                                xPt1 = arDim.XLine1Point;
                                xPt2 = arDim.XLine2Point;

                                if (prPickPoint.DistanceTo(xPt1) < prPickPoint.DistanceTo(xPt2))
                                    arDim.Dimblk1 = arwId;
                                else
                                    arDim.Dimblk2 = arwId;

                                arDim.RecomputeDimensionBlock(true);
                                break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        MailAgent.Report(e.Message);
                    }

                acTrans.Commit();
            }
        }
    }
}