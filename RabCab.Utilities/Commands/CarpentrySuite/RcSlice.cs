// -----------------------------------------------------------------------------------
//     <copyright file="RcSlice.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>04/08/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using System.Diagnostics;
using System.Threading;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using RabCab.Extensions;
using RabCab.Settings;

namespace RabCab.Commands.CarpentrySuite
{
    internal class RcSlice
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_RcSlice",
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
        public void Cmd_RcSlice()
        {
            //Get the current document utilities
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            //Call user to select a face
            var userSel = acCurEd.SelectSubentity(SubentityType.Face);

            if (userSel == null) return;

            var prSelOpts = new PromptDistanceOptions("\nEnter slice distance: ")
            {
                AllowNone = false,
                AllowZero = false,
                AllowNegative = false,
                DefaultValue = SettingsUser.RcSliceDepth
            };

            //Get the offset distance
            var prSelRes = acCurEd.GetDistance(prSelOpts);


            if (prSelRes.Status != PromptStatus.OK) return;

            SettingsUser.RcSliceDepth = prSelRes.Value;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (SettingsUser.RcSliceDepth == 0) return;

            try
            {
                //Open a transaction
                using (var acTrans = acCurDb.TransactionManager.StartTransaction())
                {
                    var acSol = acTrans.GetObject(userSel.Item1, OpenMode.ForWrite) as Solid3d;

                    var faceEnt = acSol.GetSubentity(userSel.Item2);

                    using (var tempSurf = faceEnt.CreateSurfaceFromFace(acCurDb, acTrans, false))
                    {
                        var sliceSurf = Surface.CreateOffsetSurface(tempSurf, -SettingsUser.RcSliceDepth) as Surface;

                        //Sleep for 100 milliseconds to bypass an error caused by appending too qickly
                        Thread.Sleep(100);

                        Solid3d sliceSol = null;

                        try
                        {
                            sliceSol = acSol?.Slice(sliceSurf, true);
                            sliceSol?.SetPropertiesFrom(acSol);
                            acCurDb.AppendEntity(sliceSol,acTrans);
                            sliceSurf?.Dispose();
                        }
                        catch (Exception)
                        {
                            try //If the slice by surface method failed, use the slice by offset method
                            {
                                //Dispose of the unused surface and solids
                                sliceSol?.Dispose();
                                sliceSurf?.Dispose();

                                //Clone the input solid
                                var subtSol = acSol?.Clone() as Solid3d;
                                SubentityId[] subIds = { userSel.Item2 };

                                //Offset the cloned solids face and append it to the database
                                subtSol?.OffsetFaces(subIds, -SettingsUser.RcSliceDepth);

                                acCurDb.AppendEntity(subtSol,  acTrans);

                                //Subtract the offset solid from the input solid, but don't delete it
                                Debug.Assert(acSol != null, "acSol != null");
                                Debug.Assert(subtSol != null, "subtSol != null");
                                new[] { acSol.ObjectId }.SolidSubtrahend( new[] { subtSol.ObjectId }, acCurDb, acTrans,
                                    false);
                            }
                            catch (Exception e)
                            {
                                //If nothing worked, dispose of everything and inform the user
                                sliceSol?.Dispose();
                                sliceSurf?.Dispose();
                                faceEnt.Dispose();
                                acCurEd.WriteMessage(e.Message);
                                acTrans.Abort();
                            }
                        }
                    }

                    faceEnt.Dispose();
                    acTrans.Commit();
                }
            }
            catch (Exception e)
            {
                acCurEd.WriteMessage(e.Message);
            }
        }
    }
}