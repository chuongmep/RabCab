// -----------------------------------------------------------------------------------
//     <copyright file="RcWeldBead.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>04/08/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using RabCab.Agents;
using RabCab.Extensions;
using RabCab.Settings;

namespace RabCab.Commands.StructuralSuite
{
    internal class RcWeldBead
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_WELDBEAD",
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
        public void Cmd_WeldBead()
        {
            if (!LicensingAgent.Check()) return;
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;


            //Prompt user to select a 3dFace
            var userSel = acCurEd.SelectSubentities(SubentityType.Edge);
            if (userSel.Count <= 0) return;

            var wSize = acCurEd.GetPositiveDouble("\nEnter weld bead size: ", SettingsUser.WeldBeadSize);
            SettingsUser.WeldBeadSize = wSize;

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                foreach (var pair in userSel)
                {
                    var acSol = acTrans.GetObject(pair.Item1, OpenMode.ForRead) as Solid3d;
                    if (acSol == null) continue;

                    foreach (var subId in pair.Item2)
                    {
                        var c = acSol.GetSubentity(subId) as Curve;

                        using (var weldBead = new Circle(c.StartPoint, Vector3d.XAxis, wSize / 2))
                        {
                            acCurDb.AppendEntity(weldBead);

                            var sOptsBuilder = new SweepOptionsBuilder
                            {
                                Align = SweepOptionsAlignOption.AlignSweepEntityToPath,
                                BasePoint = weldBead.Center
                            };

                            var dSol = new Solid3d();

                            acCurDb.AddLayer(SettingsUser.RcWelds, Colors.LayerColorWelds, SettingsUser.RcWeldsLt,
                                acTrans);

                            dSol.Layer = SettingsUser.RcWelds;
                            dSol.Linetype = SettingsUser.RcWeldsLt;
                            dSol.Transparency = new Transparency(75);

                            dSol.CreateSweptSolid(weldBead, c, sOptsBuilder.ToSweepOptions());
                            acCurDb.AppendEntity(dSol, acTrans);
                            weldBead.Erase();
                        }
                    }

                    acTrans.Commit();
                }
            }
        }
    }
}