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
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using RabCab.Calculators;
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
            var userSel = acCurEd.SelectSubentity(SubentityType.Edge);
            if (userSel == null) return;
            if (userSel.Item1 == ObjectId.Null) return;
            if (userSel.Item2 == SubentityId.Null) return;

            //Open a transaction
            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                var acSol = acTrans.GetObject(userSel.Item1, OpenMode.ForWrite) as Solid3d;

                var subId = userSel.Item2;

                var entityPath = new FullSubentityPath(new[] {acSol.ObjectId},
                    new SubentityId(SubentityType.Null, IntPtr.Zero));

                using (var brep = new Brep(entityPath))
                {
                    foreach (var edge in brep.Edges)
                        if (subId == edge.SubentityPath.SubentId)
                        {
                            var startPt = edge.Curve.StartPoint;
                            var endPt = edge.Curve.EndPoint;

                            var acLine = new Line(startPt, endPt);
                            acCurDb.AppendEntity(acLine, acTrans);

                            using (var acCirc = new Circle())
                            {
                                acCirc.Center = acLine.StartPoint;
                                acCirc.Diameter = SettingsUser.DogEarDiam;

                                var sOptsBuilder = new SweepOptionsBuilder
                                {
                                    Align = SweepOptionsAlignOption.AlignSweepEntityToPath,
                                    BasePoint = acLine.StartPoint,
                                    Bank = true
                                };


                                acCurDb.AddLayer(SettingsUser.RcHoles, Colors.LayerColorHoles, SettingsUser.RcHolesLt,
                                    acTrans);

                                var dSol = new Solid3d();
                                dSol.CreateSweptSolid(acCirc, acLine, sOptsBuilder.ToSweepOptions());
                                dSol.Layer = SettingsUser.RcHoles;
                                dSol.Transparency = new Transparency(75);

                                acCurDb.AppendEntity(dSol, acTrans);

                                var closestPt = new Point3d(double.MaxValue, double.MaxValue, double.MaxValue);
                                var maxDis = closestPt.DistanceTo(startPt);

                                using (var solRep = new Brep(dSol))
                                {
                                    foreach (var vtx in solRep.Vertices)
                                    {
                                        var dist = vtx.Point.DistanceTo(startPt);
                                        if (dist < maxDis)
                                        {
                                            maxDis = dist;
                                            closestPt = vtx.Point;
                                        }
                                    }
                                }

                                if (closestPt != new Point3d(double.MaxValue, double.MaxValue, double.MaxValue))
                                {
                                    dSol.TransformBy(Matrix3d.Displacement(closestPt.GetVectorTo(startPt)));
                                }
                                else
                                {
                                    acTrans.Abort();
                                    acLine.Erase();
                                    return;
                                }

                                var cont = false;

                                while (cont == false)
                                {
                                    var prKeyOpts = new PromptKeywordOptions("Define placement: ");
                                    prKeyOpts.Keywords.Add("Next");
                                    prKeyOpts.Keywords.Add("Back");
                                    prKeyOpts.Keywords.Add("Continue");
                                    prKeyOpts.Keywords.Add("Exit");
                                    prKeyOpts.AllowNone = false;

                                    var prKeyRes = acCurEd.GetKeywords(prKeyOpts);

                                    if (prKeyRes.Status == PromptStatus.OK)
                                    {
                                        var strResult = prKeyRes.StringResult;
                                        var xAxis = startPt.GetVectorTo(endPt);

                                        switch (strResult)
                                        {
                                            case "Next":
                                                dSol.TransformBy(Matrix3d.Rotation(CalcUnit.ConvertToRadians(90),
                                                    xAxis, startPt));
                                                break;
                                            case "Back":
                                                dSol.TransformBy(Matrix3d.Rotation(CalcUnit.ConvertToRadians(-90),
                                                    xAxis, startPt));
                                                break;
                                            case "Continue":
                                                var mainIds = new[] {acSol.ObjectId};
                                                var dIds = new[] {dSol.ObjectId};
                                                mainIds.SolidSubtrahend(dIds, acCurDb, acTrans, true);
                                                cont = true;
                                                break;
                                            default:
                                                acTrans.Abort();
                                                acLine.Erase();
                                                return;
                                        }
                                    }
                                    else
                                    {
                                        acTrans.Abort();
                                        acLine.Erase();
                                        return;
                                    }

                                    acCurDb.TransactionManager.QueueForGraphicsFlush();
                                }
                            }

                            acLine.Erase();
                        }
                }
                
                acTrans.Commit();
            }
        }
    }
}