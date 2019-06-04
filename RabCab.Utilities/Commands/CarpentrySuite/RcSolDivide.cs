// -----------------------------------------------------------------------------------
//     <copyright file="RcSolDivide.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>04/08/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using System;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.BoundaryRepresentation;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using RabCab.Agents;
using RabCab.Calculators;
using RabCab.Extensions;
using RabCab.Settings;

namespace RabCab.Commands.CarpentrySuite
{
    internal class RcSolDivide
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_SOLDIVIDE",
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
        public void Cmd_SolDivide()
        {
            //Get the current document utilities
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            //Call user to select an Edge
            var userSel = acCurEd.SelectSubentity(SubentityType.Edge, "\nSelect EDGE to use as division path: ");
            if (userSel == null) return;
            if (userSel.Item1 == ObjectId.Null) return;
            if (userSel.Item2 == SubentityId.Null) return;

            var divideAmt = acCurEd.GetPositiveInteger("\nEnter number of times to divide solid: ");
            if (divideAmt <= 1) return;

            //Open a transaction
            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                var acSol = acTrans.GetObject(userSel.Item1, OpenMode.ForWrite) as Solid3d;
                var subEntId = userSel.Item2;


                //Creates entity path to generate Brep object from it

                var entityPath = new FullSubentityPath(
                    new[] {acSol.ObjectId},
                    new SubentityId(SubentityType.Null, IntPtr.Zero));

                using (var brep = new Brep(entityPath))
                {
                    using (var pworker = new ProgressAgent("Chopping Solid: ", divideAmt))
                    {
                        foreach (var edge in brep.Edges)
                            if (subEntId == edge.SubentityPath.SubentId)
                            {
                                if (!pworker.Progress())
                                {
                                    acTrans.Abort();
                                    return;
                                    ;
                                }

                                var c = acSol.GetSubentity(subEntId) as Curve;

                                var length = GetLength2(c, edge.Vertex1.Point, edge.Vertex2.Point);

                                var segment = length / divideAmt;
                                var segments = segment;

                                var chopSol = acSol;

                                while (segments.RoundToTolerance() < length.RoundToTolerance())
                                {
                                    var p1 = c.GetPointAtDist(segments);
                                    var ang = c.GetFirstDerivative(c.GetParameterAtPoint(p1));
                                    var plane = new Plane(p1, ang);

                                    var obj = chopSol.Slice(plane, true);

                                    acCurDb.AppendEntity(obj);


                                    segments += segment;
                                }
                            }
                    }

                    acTrans.Commit();
                }
            }
        }

        // Make sure the pt1 and pt2 are on the Curve before calling this method.
        public static double GetLength2(Curve ent, Point3d pt1, Point3d pt2)
        {
            if (ent == null)
                throw new ArgumentNullException("The passed in Curve is null.");

            var dist1 = ent.GetDistanceAtParameter(ent.GetParameterAtPoint(ent.GetClosestPointTo(pt1, false)));
            var dist2 = ent.GetDistanceAtParameter(ent.GetParameterAtPoint(ent.GetClosestPointTo(pt2, false)));

            return Math.Abs(dist1 - dist2);
        }
    }
}