using System.Linq;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using RabCab.Agents;
using RabCab.Settings;
using Exception = System.Exception;

namespace RabCab.Commands.AnnotationSuite
{
    internal class RcDimEdit
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_DIMVALUE",
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
        public void Cmd_DimValue()
        {
            if (!LicensingAgent.Check()) return;
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurEd = acCurDoc.Editor;
            var acCurDb = acCurDoc.Database;

            var pKeyOpts = new PromptKeywordOptions(string.Empty)
                {Message = "\nSelect value to append to dimension text: "};
            pKeyOpts.Keywords.Add("OD");
            pKeyOpts.Keywords.Add("ID");
            pKeyOpts.Keywords.Add("Typ");
            pKeyOpts.Keywords.Add("Reset");
            pKeyOpts.AllowNone = false;

            var pKeyRes = acCurEd.GetKeywords(pKeyOpts);

            if (pKeyRes.Status != PromptStatus.OK) return;

            var values = new[] {new TypedValue(0, "DIMENSION")};
            var filter = new SelectionFilter(values);
            var opts = new PromptSelectionOptions
            {
                MessageForRemoval = "\nSelect dimensions to remove from selection: ",
                MessageForAdding = "\nSelect dimensions to edit: ",
                PrepareOptionalDetails = false,
                SingleOnly = false,
                SinglePickInSpace = false,
                AllowDuplicates = true
            };

            var result = acCurEd.GetSelection(opts, filter);
            if (result.Status != PromptStatus.OK) return;
            try
            {
                using (var tr = acCurDb.TransactionManager.StartTransaction())
                {
                    var sset = result.Value;
                    var transaction = tr;

                    foreach (var dim in (from SelectedObject selobj in sset
                        where transaction != null
                        select transaction.GetObject(selobj.ObjectId, OpenMode.ForWrite, false)
                        into obj
                        select obj).OfType<Dimension>())
                        switch (pKeyRes.StringResult)
                        {
                            case "OD":
                                dim.DimensionText = "<> O.D.";
                                break;
                            case "ID":
                                dim.DimensionText = "<> I.D.";
                                break;
                            case "Typ":
                                dim.DimensionText = "<> TYP.";
                                break;
                            case "Reset":
                                dim.DimensionText = "<>";
                                break;
                            default:
                                return;
                        }

                    tr.Commit();
                }
            }
            catch (Exception ex)
            {
                acCurEd.WriteMessage("\nProblem updating dimensions.\n");
                acCurEd.WriteMessage(ex.Message);
            }
        }
    }
}