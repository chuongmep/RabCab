using System.IO;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.DataExtraction;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using RabCab.Extensions;
using RabCab.Settings;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = System.Exception;
using OpenFileDialog = Autodesk.AutoCAD.Windows.OpenFileDialog;

namespace RabCab.Commands.AssemblySuite
{
    internal class RcDataExtract
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_RCDATAEXTRACT",
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
            | CommandFlags.NoBlockEditor
        //| CommandFlags.NoActionRecording
        //| CommandFlags.ActionMacro
        //| CommandFlags.NoInferConstraint 
        )]
        public void Cmd_DataExtract()
        {
            var rowHeight = SettingsUser.TableRowHeight;
            var colWidth = SettingsUser.TableColumnWidth;
            var textHeight = SettingsUser.TableTextHeight;

            //Get the current document utilities
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            var dwgFolder = Path.GetDirectoryName(acCurDoc.Name);
            var dwgName = Path.GetFileName(acCurDoc.Name);

            DxExtractionSettings extractionSettings = null;

            if (SelectTemplateFileAndLoad(ref extractionSettings, out var templatePath))
            {
                var xmlName = Path.GetFileNameWithoutExtension(templatePath);
                var xmlPath = dwgFolder + "\\" + xmlName + ".csv";

                var destPath = dwgFolder + "\\" + Path.GetFileName(templatePath);
                File.Copy(templatePath, destPath, true);

                extractionSettings = DxExtractionSettings.FromFile(destPath) as DxExtractionSettings;

                var dwgList = extractionSettings.DrawingDataExtractor.Settings.DrawingList as DxFileList;

                foreach (var dwg in dwgList.Files)
                    extractionSettings.DrawingDataExtractor.Settings.DrawingList.RemoveFile(dwg);


                var dxFileReference = new DxFileReference(dwgFolder, dwgFolder + "\\" + dwgName);
                extractionSettings.DrawingDataExtractor.Settings.DrawingList.AddFile(dxFileReference);
                dwgList.CurrentFile = dxFileReference;

                extractionSettings.Save(destPath);

                //Create a DataLink
                ObjectId dlId;
                var dlm = acCurDb.DataLinkManager;
                using (var dl = new DataLink())
                {
                    var dataLinkName = "MyDataLink2";
                    dlId = dlm.GetDataLink(dataLinkName);

                    if (dlId.IsNull)
                    {
                        // create a datalink
                        dl.ConnectionString = destPath;
                        dl.ToolTip = "My Data Link";
                        dl.Name = dataLinkName;

                        var da = DataAdapterManager.GetDataAdapter("Autodesk.AutoCAD.DataExtraction.DxDataLinkAdapter");

                        if (da != null)
                            dl.DataAdapterId = da.DataAdapterId;

                        dlId = dlm.AddDataLink(dl);
                    }
                }

                // Ask for the table insertion point
                var pr = acCurEd.GetPoint("\nEnter table insertion point: ");
                if (pr.Status == PromptStatus.OK)
                {
                    // Create a table
                    var tableId = ObjectId.Null;
                    using (var acTrans = acCurDb.TransactionManager.StartTransaction())
                    {
                        var acTable = new Table();
                        acTable.Position = pr.Value;
                        acTable.TableStyle = acCurDb.Tablestyle;
                        acTable.SetSize(2, 1);
                        acTable.SetRowHeight(rowHeight);
                        acTable.SetColumnWidth(colWidth);

                        var header = acTable.Cells[0, 0];
                        header.Value = "";
                        header.Alignment = CellAlignment.MiddleCenter;
                        header.TextHeight = textHeight;

                        acTable.Cells[1, 0].DataLink = dlId;

                        acCurDb.AppendEntity(acTable, acTrans);

                        //Generate the layout
                        acTable.GenerateLayout();
                        acTrans.MoveToAttachment(acTable, SettingsUser.TableAttach, pr.Value, SettingsUser.TableXOffset,
                            SettingsUser.TableYOffset);
                        acTrans.Commit();
                    }
                }
            }
        }

        private bool SelectTemplateFileAndLoad(ref DxExtractionSettings extractionSettings, out string fileName)
        {
            // set no urls or ftp sites
            var flags = OpenFileDialog.OpenFileDialogFlags.NoUrls | OpenFileDialog.OpenFileDialogFlags.NoFtpSites;
            // create a new select dialog
            var ofd = new OpenFileDialog("Select Template file", "", "dxe", "BrowseTemplateFile", flags);

            var bCheckFile = true;
            while (bCheckFile)
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    fileName = ofd.Filename;

                    if (LoadTemplateFromFile(ofd.Filename, ref extractionSettings))
                    {
                        bCheckFile = false;
                        return true;
                    }

                    MessageBox.Show("Failed to open that template file, please try again...");
                }
                else
                {
                    bCheckFile = false;
                }

            fileName = null;

            return false;
        }

        private bool LoadTemplateFromFile(string fileName, ref DxExtractionSettings extractionSettings)
        {
            try
            {
                extractionSettings = (DxExtractionSettings)DxExtractionSettings.FromFile(fileName);
                return extractionSettings != null;
            }
            catch (Exception e)
            {
                var ed = Application.DocumentManager.MdiActiveDocument.Editor;
                ed.WriteMessage("\nProblem reading template " + fileName + "\n" + e.Message);
            }

            return false;
        }
    }
}