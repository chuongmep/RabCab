// -----------------------------------------------------------------------------------
//     <copyright file="RcSimulate.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>04/09/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using System;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.BoundaryRepresentation;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using RabCab.Extensions;
using RabCab.Settings;

namespace RabCab.Commands.CarpentrySuite
{
    internal class RcDogEar
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_DOGEAR",
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
        public void Cmd_DogEar()
        {
            //Get the current document utilities
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            //Call user to select a face
            var sweepSel = acCurEd.SelectSubentity(SubentityType.Edge, "\nSelect EDGE to use as cutting path: ");
            if (sweepSel == null) return;
            if (sweepSel.Item1 == ObjectId.Null) return;
            if (sweepSel.Item2 == SubentityId.Null) return;

            //Call user to select a face
            var dirSel = acCurEd.SelectSubentity(SubentityType.Edge, "\nSelect EDGE to cut dog ear into: ");
            if (dirSel == null) return;
            if (dirSel.Item1 == ObjectId.Null) return;
            if (dirSel.Item2 == SubentityId.Null) return;

            //Open a transaction
            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                var acSol = acTrans.GetObject(sweepSel.Item1, OpenMode.ForWrite) as Solid3d;

                var solPath = new FullSubentityPath(new[] {acSol.ObjectId},
                    new SubentityId(SubentityType.Null, IntPtr.Zero));

                var sweepSubId = sweepSel.Item2;
                var dirSubId = dirSel.Item2;
                VertExt sweepInfo1 = null;
                VertExt sweepInfo2 = null;
                VertExt dirInfo1 = null;
                VertExt dirInfo2 = null;

                using (var brep = new Brep(solPath))
                {
                    foreach (var face in brep.Faces)
                    foreach (var loop in face.Loops)
                    foreach (var edge in loop.Edges)
                    {
                        var eId = edge.SubentityPath.SubentId;

                        if (sweepSubId == eId)
                        {
                            sweepInfo1 = new VertExt(edge.Vertex1, loop);
                            sweepInfo2 = new VertExt(edge.Vertex2, loop);
                        }
                        else if (dirSubId == eId)
                        {
                            dirInfo1 = new VertExt(edge.Vertex1, loop);
                            dirInfo2 = new VertExt(edge.Vertex2, loop);
                        }
                    }
                }

                if (sweepInfo1 != null && sweepInfo2 != null && dirInfo1 != null && dirInfo2 != null)
                {
                    var cutDiam = SettingsUser.DogEarDiam;
                    var cutRad = cutDiam / 2;
                    var sw1 = sweepInfo1.VertPoint;
                    var sw2 = sweepInfo2.VertPoint;
                    var dr1 = dirInfo1.VertPoint;
                    var dr2 = dirInfo2.VertPoint;

                    var rotAxis = sw1.GetVectorTo(sw2);

                    var vertsMatch = false;

                    while (!vertsMatch)
                    {
                        if (sw1 == dr1)
                        {
                            vertsMatch = true;
                            continue;
                        }

                        if (sw1 == dr2)
                        {
                            var tempExt = dr1;
                            dr1 = dr2;
                            dr2 = tempExt;

                            continue;
                        }

                        if (sw2 == dr1)
                        {
                            var tempExt = sw1;
                            sw1 = sw2;
                            sw2 = tempExt;
                            continue;
                        }

                        if (sw2 == dr2)
                        {
                            var tempExt = dr1;
                            dr1 = dr2;
                            dr2 = tempExt;

                            tempExt = sw1;
                            sw1 = sw2;
                            sw2 = tempExt;

                            continue;
                        }

                        acTrans.Abort();
                        return;
                    }

                    var dr3 = dr1.GetAlong(dr2, cutDiam);

                    using (var pathLine = new Line(sw1, sw2))
                    {
                        using (var cutCirc = new Circle(dr1.GetMidPoint(dr3), rotAxis, cutRad))
                        {
                            acCurDb.AppendEntity(pathLine);
                            acCurDb.AppendEntity(cutCirc);

                            var sOptsBuilder = new SweepOptionsBuilder
                            {
                                Align = SweepOptionsAlignOption.NoAlignment,
                                BasePoint = cutCirc.Center,
                                Bank = true
                            };

                            using (var dSol = new Solid3d())
                            {
                                dSol.CreateSweptSolid(cutCirc, pathLine, sOptsBuilder.ToSweepOptions());
                                acSol.BooleanOperation(BooleanOperationType.BoolSubtract, dSol);
                            }

                            pathLine.Erase();
                            cutCirc.Erase();
                        }
                    }
                }

                acTrans.Commit();
            }
        }
    }
}

#region Old Style

//var startPt = edge.Curve.StartPoint;
//var endPt = edge.Curve.EndPoint;

//using (var acLine = new Line(startPt, endPt))
//{
//    acCurDb.AppendEntity(acLine, acTrans);

//    using (var acCirc = new Circle())
//    {
//        acCirc.Center = acLine.StartPoint;
//        acCirc.Diameter = SettingsUser.DogEarDiam;

//        var sOptsBuilder = new SweepOptionsBuilder
//        {
//            Align = SweepOptionsAlignOption.AlignSweepEntityToPath,
//            BasePoint = acLine.StartPoint,
//            Bank = true
//        };


//        acCurDb.AddLayer(SettingsUser.RcHoles, Colors.LayerColorHoles, SettingsUser.RcHolesLt,
//            acTrans);

//        var dSol = new Solid3d();
//        dSol.CreateSweptSolid(acCirc, acLine, sOptsBuilder.ToSweepOptions());
//        dSol.Layer = SettingsUser.RcHoles;
//        dSol.Transparency = new Transparency(75);

//        acCurDb.AppendEntity(dSol, acTrans);

//        var closestPt = new Point3d(double.MaxValue, double.MaxValue, double.MaxValue);
//        var maxDis = closestPt.DistanceTo(startPt);

//        using (var solRep = new Brep(dSol))
//        {
//            foreach (var vtx in solRep.Vertices)
//            {
//                var dist = vtx.Point.DistanceTo(startPt);
//                if (dist < maxDis)
//                {
//                    maxDis = dist;
//                    closestPt = vtx.Point;
//                }
//            }
//        }

//        if (closestPt != new Point3d(double.MaxValue, double.MaxValue, double.MaxValue))
//        {
//            dSol.TransformBy(Matrix3d.Displacement(closestPt.GetVectorTo(startPt)));
//        }
//        else
//        {
//            acTrans.Abort();
//            acLine.Erase();
//            return;
//        }

//        var cont = false;

//        while (cont == false)
//        {
//            var prKeyOpts = new PromptKeywordOptions("Define placement: ");
//            prKeyOpts.Keywords.Add("Next");
//            prKeyOpts.Keywords.Add("Back");
//            prKeyOpts.Keywords.Add("Continue");
//            prKeyOpts.Keywords.Add("Exit");
//            prKeyOpts.AllowNone = false;

//            var prKeyRes = acCurEd.GetKeywords(prKeyOpts);

//            if (prKeyRes.Status == PromptStatus.OK)
//            {
//                var strResult = prKeyRes.StringResult;
//                var xAxis = startPt.GetVectorTo(endPt);

//                switch (strResult)
//                {
//                    case "Next":
//                        dSol.TransformBy(Matrix3d.Rotation(CalcUnit.ConvertToRadians(90),
//                            xAxis, startPt));
//                        break;
//                    case "Back":
//                        dSol.TransformBy(Matrix3d.Rotation(CalcUnit.ConvertToRadians(-90),
//                            xAxis, startPt));
//                        break;
//                    case "Continue":
//                        var mainIds = new[] {acSol.ObjectId};
//                        var dIds = new[] {dSol.ObjectId};
//                        mainIds.SolidSubtrahend(dIds, acCurDb, acTrans, true);
//                        cont = true;
//                        break;
//                    default:
//                        acTrans.Abort();
//                        acLine.Erase();
//                        return;
//                }
//            }
//            else
//            {
//                acTrans.Abort();
//                acLine.Erase();
//                return;
//            }

//            acCurDb.TransactionManager.QueueForGraphicsFlush();
//        }
//    }

//    acLine.Erase();
//}

#endregion