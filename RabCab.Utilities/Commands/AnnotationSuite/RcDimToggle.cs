using System;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using RabCab.Settings;
using Exception = Autodesk.AutoCAD.Runtime.Exception;

namespace RabCab.Commands.AnnotationSuite
{
    internal class RcDimToggle
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_DIMTOGGLE",
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
        public void Cmd_DimToggle()
        {
            if (!Agents.LicensingAgent.Check()) return;
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            var prEntOpt = new PromptEntityOptions("\nSelect an extension line to toggle: ");

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

                        switch (acDim)
                        {
                            case AlignedDimension alDim:
                            {
                                xPt1 = alDim.XLine1Point;
                                xPt2 = alDim.XLine2Point;

                                if (prPickPoint.DistanceTo(xPt1) < prPickPoint.DistanceTo(xPt2))
                                {
                                    var curToggle = alDim.Dimse1;
                                    alDim.Dimse1 = !curToggle;
                                }
                                else
                                {
                                    var curToggle = alDim.Dimse2;
                                    alDim.Dimse2 = !curToggle;
                                }

                                break;
                            }

                            case RotatedDimension roDim:
                            {
                                xPt1 = roDim.XLine1Point;
                                xPt2 = roDim.XLine2Point;

                                if (prPickPoint.DistanceTo(xPt1) < prPickPoint.DistanceTo(xPt2))
                                {
                                    var curToggle = roDim.Dimse1;
                                    roDim.Dimse1 = !curToggle;
                                }
                                else
                                {
                                    var curToggle = roDim.Dimse2;
                                    roDim.Dimse2 = !curToggle;
                                }

                                break;
                            }

                            case ArcDimension arDim:
                            {
                                xPt1 = arDim.XLine1Point;
                                xPt2 = arDim.XLine2Point;

                                if (prPickPoint.DistanceTo(xPt1) < prPickPoint.DistanceTo(xPt2))
                                {
                                    var curToggle = arDim.Dimse1;
                                    arDim.Dimse1 = !curToggle;
                                }
                                else
                                {
                                    var curToggle = arDim.Dimse2;
                                    arDim.Dimse2 = !curToggle;
                                }

                                break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                acTrans.Commit();
            }
        }
    }
}