using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.DataExtraction;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using RabCab.Extensions;
using RabCab.Settings;

namespace RabCab.Commands.AssemblySuite
{
    class RcDataExtract
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
            // Copy the attached "MyBlock.dwg" to C:\Temp for testing

            // The Dxe file will be created at runtime in C:\Temp if not found
            //Ask user to select the data extraction file to use
            var dwgFolder = Path.GetDirectoryName(acCurDoc.Name);
            var dwgName = Path.GetFileName(acCurDoc.Name);
            var dxeName = Path.GetFileNameWithoutExtension(acCurDoc.Name) + ".dxe";
            var dxePath = dwgFolder + "\\" + dxeName;

            if (System.IO.File.Exists(dxePath))
            {
                File.Delete(dxePath);
            }

            // Create the DXE file with the information that we want to extract
                DxExtractionSettings setting = new DxExtractionSettings();

               IDxFileReference dxFileReference = new DxFileReference(dwgFolder, dwgFolder + "\\" + dwgName);

                setting.DrawingDataExtractor.Settings.DrawingList.AddFile(dxFileReference);
                setting.DrawingDataExtractor.DiscoverTypesAndProperties(dwgFolder + dwgName);
                setting.WizardSettings.DisplayOptions = DisplayOptions.DisplayUsedTypesOnly | DisplayOptions.Block;

                List<IDxTypeDescriptor> types = setting.DrawingDataExtractor.DiscoveredTypesAndProperties;
                List<string> selectedTypes = new List<string>();
                List<string> selectedProps = new List<string>();

                foreach (IDxTypeDescriptor td in types)
                {
                    var globalName = td.GlobalName;
                    var displayName = td.DisplayName;
                    Debug.WriteLine(globalName + " - " + td.DisplayName);
                    

                    if (globalName.Equals("BlockReferenceTypeDescriptor." + displayName))
                        selectedTypes.Add(td.GlobalName);

                    foreach (IDxPropertyDescriptor pd in td.Properties)
                    {
                        if (pd.GlobalName.Equals("AcDxObjectTypeGlobalName") ||
                            pd.GlobalName.Equals("AcDxObjectTypeName"))
                        {
                            if (!selectedProps.Contains(pd.GlobalName))
                                selectedProps.Add(pd.GlobalName);
                        }

                        //TODO check for user defined properties for blocks here

                    }
                }

                setting.DrawingDataExtractor.Settings.ExtractFlags
                    = ExtractFlags.Nested | 
                      ExtractFlags.Xref | 
                      ExtractFlags.ExtractBlockOnly | 
                      ExtractFlags.ModelSpaceOnly;

                setting.DrawingDataExtractor.Settings.SetSelectedTypesAndProperties
                    (types, selectedTypes, selectedProps);

            
                setting.OutputSettings.DataCellStyle = "Data";
                setting.OutputSettings.HeaderCellStyle = "Header";
                setting.OutputSettings.ManuallySetupTable = true;
                setting.OutputSettings.OuputFlags = DxOuputFlags.Table;
                setting.OutputSettings.TableStyleId = acCurDb.Tablestyle;
                setting.OutputSettings.TableStyleName = "Standard";
                setting.OutputSettings.TitleCellStyle = "Title";
                setting.OutputSettings.UsePropertyNameAsColumnHeader = false;
                setting.OutputSettings. = DxCombineMode.Sum;

                setting.Save(dxePath);
                
            //Create a DataLink
            ObjectId dlId = ObjectId.Null;
            DataLinkManager dlm = acCurDb.DataLinkManager;
            using (DataLink dl = new DataLink())
            {
                String dataLinkName = "TestLink";
                dlId = dlm.GetDataLink(dataLinkName);

                if (dlId.IsNull)
                {
                    // create a datalink
                    dl.ConnectionString = dxePath;
                    dl.ToolTip = "TestLink";
                    dl.Name = dataLinkName;
                    
                    DataAdapter da = DataAdapterManager.GetDataAdapter("Autodesk.AutoCAD.DataExtraction.DxDataLinkAdapter");

                    if (da != null)
                        dl.DataAdapterId = da.DataAdapterId;

                    dlId = dlm.AddDataLink(dl);
                }
            }

            // Ask for the table insertion point
            PromptPointResult pr = acCurEd.GetPoint("\nEnter table insertion point: ");
            if (pr.Status == PromptStatus.OK)
            {
                // Create a table
                ObjectId tableId = ObjectId.Null;
                using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
                {
                    Table acTable = new Table();
                    acTable.Position = pr.Value;
                    acTable.TableStyle = acCurDb.Tablestyle;
                    acTable.SetSize(2, 1);
                    acTable.SetRowHeight(rowHeight);
                    acTable.SetColumnWidth(colWidth);
                    
                    var dl = acTrans.GetObject(dlId, OpenMode.ForRead) as DataLink;
                    var tabName = dl.Name;

                    var header = acTable.Cells[0, 0];
                    header.Value = tabName.ToUpper();
                    header.Alignment = CellAlignment.MiddleCenter;
                    header.TextHeight = textHeight;

                    acTable.Cells[1, 0].DataLink = dlId;
                    acCurDb.AppendEntity(acTable, acTrans);

                    //Generate the layout
                    acTable.GenerateLayout();
                    acTrans.MoveToAttachment(acTable, SettingsUser.TableAttach, pr.Value, SettingsUser.TableXOffset, SettingsUser.TableYOffset);
                    acTrans.Commit();
                }
            }
        }
    }
}
