using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using RabCab.Engine.Enumerators;
using RabCab.Engine.System;
using RabCab.Extensions;
using RabCab.Settings;

namespace RabCab.Commands.StructuralSuite
{
    internal class RcFloor
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_FLOOR",
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
        public void Cmd_Floor()
        {
            //Get the current document utilities
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            try
            {
                // Begin Transaction
                using (var acTrans = acCurDb.TransactionManager.StartTransaction())
                {
                    var userGridMode = AcVars.GridMode;

                    if (AcVars.GridMode != Enums.GridMode.On) AcVars.GridMode = Enums.GridMode.Off;

                    acCurEd.Regen();

                    var acSelOpts = new PromptSelectionOptions
                    {
                        MessageForAdding = "\nSelect objects to floor to the current XY plane: ",
                        MessageForRemoval = "\nSelect objects to remove from selection: ",
                        RejectPaperspaceViewport = true,
                        RejectObjectsOnLockedLayers = true,
                        RejectObjectsFromNonCurrentSpace = true
                    };

                    // Get object selection from user
                    var selRes = acCurEd.GetSelection(acSelOpts);

                    // Exit if selection error
                    if (selRes.Status != PromptStatus.OK) return;

                    // Set selection set to user selection
                    var acSSet = selRes.Value;

                    //Regenerate the User Space
                    acCurEd.Regen();

                    // Get point from user to floor
                    var ptOpt = new PromptPointOptions("\nSelect point to align with XY plane: ")
                    {
                        AllowNone = false
                    };
                    var ptRes = acCurEd.GetPoint(ptOpt);

                    // Exit if selection error
                    if (ptRes.Status != PromptStatus.OK) return;

                    // Create  matrix to move the objects from the current Z value to a '0' Z value
                    var acPt3D = ptRes.Value;
                    var transVec3D = acPt3D.GetTransformedVector(new Point3d(acPt3D.X, acPt3D.Y, 0), acCurEd);

                    // Floor selected objects
                    foreach (SelectedObject acSsObj in acSSet) acSsObj.DisplaceByVector(acTrans, transVec3D);

                    // Return the Grid Mode to the Users Setting
                    AcVars.GridMode = userGridMode;

                    acCurEd.Regen();

                    // Commit Transaction
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