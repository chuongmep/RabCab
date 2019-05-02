// -----------------------------------------------------------------------------------
//     <copyright file="RcTraverse.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>04/11/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using RabCab.Agents;
using RabCab.Analysis;
using RabCab.Engine.Enumerators;
using RabCab.Extensions;
using RabCab.Settings;

namespace RabCab.Commands.AnalysisSuite
{
    internal class RcTraverse
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_TRAVERSE",
            CommandFlags.Modal
            //| CommandFlags.Transparent
            | CommandFlags.UsePickSet
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
            | CommandFlags.NoActionRecording
            //| CommandFlags.ActionMacro
            //| CommandFlags.NoInferConstraint 
        )]
        public void Cmd_Traverse()
        {
            //Get the current document utilities
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            //Start a selection set
            SelectionSet acSet;

            //Check for pick-first selection -> if none, get selection
            if (!acCurEd.CheckForPickFirst(out acSet))
                acSet = SelectionSet.FromObjectIds(acCurEd.GetFilteredSelection(Enums.DxfNameEnum._3Dsolid, false));

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                using (var pWorker = new ProgressAgent("Parsing Solids: ", acSet.Count))
                {
                    var parseCount = 1;

                    foreach (var objId in acSet.GetObjectIds())
                    {
                        //Tick progress bar or exit if ESC has been pressed
                        if (!pWorker.Tick())
                        {
                            acTrans.Abort();
                            return;
                        }

                        var acSol = acTrans.GetObject(objId, OpenMode.ForRead) as Solid3d;

                        if (acSol == null) continue;

                        var entInfo = new EntInfo(acSol, acCurDb, acTrans);
                        acSol.AddXData(entInfo, acCurDb, acTrans);
                        acCurEd.WriteMessage("\n" + entInfo.PrintInfo(false, parseCount));
                        parseCount++;

                        #region Debug Testing

                        //acSol.UpdateXData("TestName", Enums.XDataCode.Name, acCurDb, acTrans); //1
                        //acSol.UpdateXData("TestInfo", Enums.XDataCode.Info, acCurDb, acTrans); //2
                        //acSol.UpdateXData(10, Enums.XDataCode.Length, acCurDb, acTrans); //3
                        //acSol.UpdateXData(20, Enums.XDataCode.Width, acCurDb, acTrans); //4
                        //acSol.UpdateXData(30, Enums.XDataCode.Thickness, acCurDb, acTrans); //5
                        //acSol.UpdateXData(40, Enums.XDataCode.Volume, acCurDb, acTrans); //6
                        //acSol.UpdateXData(50, Enums.XDataCode.MaxArea, acCurDb, acTrans); //7
                        //acSol.UpdateXData(60, Enums.XDataCode.MaxPerimeter, acCurDb, acTrans); //8
                        //acSol.UpdateXData(70, Enums.XDataCode.Asymmetry, acCurDb, acTrans); //9
                        //acSol.UpdateXData("AxymV", Enums.XDataCode.AsymmetryVector, acCurDb, acTrans); //10
                        //acSol.UpdateXData(1, Enums.XDataCode.PartOf, acCurDb, acTrans); //11
                        //acSol.UpdateXData(2, Enums.XDataCode.PartTotal, acCurDb, acTrans); //12
                        //acSol.UpdateXData(3, Enums.XDataCode.NumChanges, acCurDb, acTrans); //13
                        //acSol.UpdateXData(false, Enums.XDataCode.IsSweep, acCurDb, acTrans); //14
                        //acSol.UpdateXData(true, Enums.XDataCode.IsMirror, acCurDb, acTrans); //15
                        //acSol.UpdateXData(false, Enums.XDataCode.HasHoles, acCurDb, acTrans); //16
                        //acSol.UpdateXData(Enums.TextureDirection.Vertical, Enums.XDataCode.TextureDirection, acCurDb, acTrans); //17
                        //acSol.UpdateXData(Enums.ProductionType.MillingManySide, Enums.XDataCode.ProductionType, acCurDb, acTrans); //18
                        //acSol.UpdateXData(acSol.Handle, Enums.XDataCode.ParentObject, acCurDb, acTrans); //19
                        //acSol.UpdateXData(new List<Handle>() {acSol.Handle, acSol.Handle , acSol.Handle}, 
                        //Enums.XDataCode.ChildObjects, acCurDb, acTrans); //20

                        //acCurEd.WriteMessage("\n Testing XData Read");
                        //acCurEd.WriteMessage("\n" + acSol.GetAppName());
                        //acCurEd.WriteMessage("\n" + acSol.GetPartName());
                        //acCurEd.WriteMessage("\n" + acSol.GetPartInfo());
                        //acCurEd.WriteMessage("\n" + acSol.GetPartLength());
                        //acCurEd.WriteMessage("\n" + acSol.GetPartWidth());
                        //acCurEd.WriteMessage("\n" + acSol.GetPartThickness());
                        //acCurEd.WriteMessage("\n" + acSol.GetPartVolume());
                        //acCurEd.WriteMessage("\n" + acSol.GetPartArea());
                        //acCurEd.WriteMessage("\n" + acSol.GetPartPerimeter());
                        //acCurEd.WriteMessage("\n" + acSol.GetPartAsymmetry());
                        //acCurEd.WriteMessage("\n" + acSol.GetAsymVector());
                        //acCurEd.WriteMessage("\n" + acSol.GetQtyOf());
                        //acCurEd.WriteMessage("\n" + acSol.GetQtyTotal());
                        //acCurEd.WriteMessage("\n" + acSol.GetNumChanges());
                        //acCurEd.WriteMessage("\n" + acSol.GetIsSweep());
                        //acCurEd.WriteMessage("\n" + acSol.GetIsMirror());
                        //acCurEd.WriteMessage("\n" + acSol.GetHasHoles());
                        //acCurEd.WriteMessage("\n" + acSol.GetTextureDirection());
                        //acCurEd.WriteMessage("\n" + acSol.GetProductionType());
                        //acCurEd.WriteMessage("\n" + acSol.GetParent());
                        //acCurEd.WriteMessage("\n" + String.Join(",",acSol.GetChildren()));

                        #endregion
                    }
                }

                acTrans.Commit();
            }
        }
    }
}