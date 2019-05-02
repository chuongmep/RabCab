using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using RabCab.Settings;
using System;
using System.Linq;

namespace RabCab.Commands.AnnotationSuite
{
    class RcDimLt
    {

        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_DIMLT",
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
        public void Cmd_DimLt()
        {
            //Get the current document utilities
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            var prEntOpt = new PromptEntityOptions("\nSelect an extension line to convert to line-type <" + SettingsUser.RcDimLt + "> : ");

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
                {
                    try
                    {

                        // Open the Linetype table for read
                        LinetypeTable acLineTypTbl;
                        acLineTypTbl = acTrans.GetObject(acCurDb.LinetypeTableId,
                            OpenMode.ForRead) as LinetypeTable;

                        if (acLineTypTbl != null)
                        {
                            var sLineTypName = SettingsUser.RcDimLt;

                            if (acLineTypTbl.Has(sLineTypName) == false)
                            {
                                acCurDb.LoadLineTypeFile(sLineTypName, "acad.lin");
                            }

                            Point3d xPt1;
                            Point3d xPt2;

                            var ltId = acLineTypTbl[sLineTypName];

                            switch (acDim)
                            {
                                case AlignedDimension alDim:
                                    {
                                        xPt1 = alDim.XLine1Point;
                                        xPt2 = alDim.XLine2Point;

                                        if (prPickPoint.DistanceTo(xPt1) < prPickPoint.DistanceTo(xPt2))
                                        {
                                            alDim.Dimltex1 = ltId;
                                        }
                                        else
                                        {
                                            alDim.Dimltex2 = ltId;
                                        }

                                        break;
                                    }

                                case RotatedDimension roDim:
                                    {
                                        xPt1 = roDim.XLine1Point;
                                        xPt2 = roDim.XLine2Point;

                                        if (prPickPoint.DistanceTo(xPt1) < prPickPoint.DistanceTo(xPt2))
                                        {
                                            roDim.Dimltex1 = ltId;
                                        }
                                        else
                                        {
                                            roDim.Dimltex2 = ltId;
                                        }

                                        break;
                                    }

                                case ArcDimension arDim:
                                    {
                                        xPt1 = arDim.XLine1Point;
                                        xPt2 = arDim.XLine2Point;

                                        if (prPickPoint.DistanceTo(xPt1) < prPickPoint.DistanceTo(xPt2))
                                        {
                                            arDim.Dimltex1 = ltId;
                                        }
                                        else
                                        {
                                            arDim.Dimltex2 = ltId;
                                        }

                                        break;
                                    }

                            }

                        }

                    }
                    catch (Autodesk.AutoCAD.Runtime.Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }

                acTrans.Commit();
            }

        }
    }
}
