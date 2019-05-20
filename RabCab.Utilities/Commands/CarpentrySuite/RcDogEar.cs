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
using System.Windows.Media.Media3D;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.BoundaryRepresentation;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using RabCab.Analysis;
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
        public void Cmd_DogEar()
        {
            //Get the current document utilities
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            //Call user to select a face
            var userSel = acCurEd.SelectSubentity(SubentityType.Edge);

            if (userSel == null) return;


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


                                //TODO change base point by selecting next

                                var sol = new Solid3d();
                                sol.CreateSweptSolid(acCirc, acLine, sOptsBuilder.ToSweepOptions());

                                acCurDb.AppendEntity(sol, acTrans);

                                var closestPt = new Point3d(double.MaxValue, double.MaxValue, double.MaxValue);
                                double maxDis = closestPt.DistanceTo(startPt);
                                
                                using (var solRep = new Brep(sol))
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

                                Vector3d rotVector;

                                if (closestPt != new Point3d(double.MaxValue, double.MaxValue, double.MaxValue))
                                {
                                    rotVector = closestPt.GetVectorTo(startPt);
                                    sol.TransformBy(Matrix3d.Displacement(closestPt.GetVectorTo(startPt)));
                                }
                                else
                                {
                                    acTrans.Abort();
                                    return;
                                }

                                var cont = false;

                                while (cont == false)
                                {
                                    var prKeyOpts = new PromptKeywordOptions("Enter option: ");
                                    prKeyOpts.Keywords.Add("Rotate");
                                    prKeyOpts.Keywords.Add("Continue");
                                    prKeyOpts.Keywords.Add("Exit");
                                    prKeyOpts.AllowNone = false;

                                    var prKeyRes = acCurEd.GetKeywords(prKeyOpts);

                                    if (prKeyRes.Status == PromptStatus.OK)
                                    {
                                        var strResult = prKeyRes.StringResult;

                                        switch (strResult)
                                        {
                                            case "Rotate":

                                                sol.TransformBy(Matrix3d.Rotation(CalcUnit.ConvertToRadians(90),
                                                    rotVector, startPt));

                                                acCurDb.TransactionManager.QueueForGraphicsFlush();

                                                break;
                                            case "Continue":
                                                cont = true;
                                                break;
                                            default:
                                                acTrans.Abort();
                                                return;
                                        }
                                    }
                                    else
                                    {
                                        acTrans.Abort();
                                        return;
                                    }

                                }
                                
                                //TODO cut solid

                            }

                        }
                }

                acTrans.Commit();
            }
        }
    }
}