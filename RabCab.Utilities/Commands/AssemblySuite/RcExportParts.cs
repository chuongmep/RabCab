// -----------------------------------------------------------------------------------
//     <copyright file="RcExportParts.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>04/11/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using RabCab.Agents;
using RabCab.Analysis;
using RabCab.Engine.Enumerators;
using RabCab.Entities.Controls;
using RabCab.Extensions;
using RabCab.External;
using RabCab.Settings;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = Autodesk.AutoCAD.Runtime.Exception;

namespace RabCab.Commands.AssemblySuite
{
    internal class RcExportParts
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_EXPORTPARTS",
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
            // | CommandFlags.Session
            //| CommandFlags.Interruptible
            //| CommandFlags.NoHistory
            //| CommandFlags.NoUndoMarker
            | CommandFlags.NoBlockEditor
            //| CommandFlags.NoActionRecording
            //| CommandFlags.ActionMacro
            //| CommandFlags.NoInferConstraint 
        )]
        public void Cmd_OutputParts()
        {
            if (!LicensingAgent.Check()) return;
            // Get the current document
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;

            if (acCurDoc == null) return;

            using (acCurDoc.LockDocument())
            {
                //Get the current database and editor
                var acCurDb = acCurDoc.Database;
                var acCurEd = acCurDoc.Editor;

                var objIds = acCurEd.GetFilteredSelection(Enums.DxfNameEnum._3Dsolid, false, null,
                    "\nSelect 3D Solids to save out into separate files: ");
                if (objIds.Length <= 0) return;


                //Tell the user we are going to open a dialog window to select a directory
                acCurEd.WriteMessage("\nSelect directory to export parts to");

                var dirSelected = false;

                // Get the directory to save to
                var fDirectory = string.Empty;

                //While user has not chosen a directory, or until user cancels - show a dialog window for directory selection
                while (dirSelected == false)
                {
                    //Create the folder browser
                    var fBrowser = new FolderBrowser();
                    var result = fBrowser.ShowDialog(new AcadMainWindow());

                    //If the folder browser selection worked:
                    if (result != DialogResult.OK)
                    {
                        if (result == DialogResult.Cancel)
                        {
                            acCurEd.WriteMessage("\nOperation cancelled by user.");
                            return;
                        }

                        acCurEd.WriteMessage("\nError in selection - Aborting operation.");
                        return;
                    }

                    fDirectory = fBrowser.DirectoryPath;
                    dirSelected = true;
                }

                #region Old File Handling

                if (!new PathFinder().IsDirectoryEmpty(fDirectory))
                {
                    var subPath = "\\OldVersions" + DateTime.Now.ToString(" MM.dd [HH.mm.ss]");

                    var exists = Directory.Exists(fDirectory + subPath);

                    if (exists) return;

                    try
                    {
                        // Ensure the source directory exists
                        if (Directory.Exists(fDirectory))
                            // Ensure the destination directory doesn't already exist
                            if (Directory.Exists(fDirectory + subPath) == false)
                            {
                                var destDir = fDirectory + subPath;

                                Directory.CreateDirectory(destDir);

                                var files = Directory.EnumerateFiles(fDirectory, "*")
                                    .Select(path => new FileInfo(path));

                                var folders = Directory.EnumerateDirectories(fDirectory, "*")
                                    .Select(path => new DirectoryInfo(path));

                                foreach (var file in files)
                                    try
                                    {
                                        file.MoveTo(Path.Combine(destDir, file.Name));
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine(e);
                                        MailAgent.Report(e.Message);
                                    }

                                foreach (var folder in folders)
                                    try
                                    {
                                        if (!folder.Name.Contains("OldVersions"))
                                            folder.MoveTo(Path.Combine(destDir, folder.Name));
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine(e);
                                        MailAgent.Report(e.Message);
                                    }
                            }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                        return;
                    }
                }

                #endregion

                acCurEd.WriteMessage("\nDirectory selected: " + fDirectory);
                var eList = new List<EntInfo>();

                var multAmount = 1;

                if (SettingsUser.PromptForMultiplication)
                    multAmount = acCurEd.GetPositiveInteger("\nEnter number to multiply parts by: ", 1);

                using (var pWorker = new ProgressAgent("Parsing Solids: ", objIds.Length))
                {
                    using (var acTrans = acCurDb.TransactionManager.StartTransaction())
                    {
                        foreach (var objId in objIds)
                        {
                            //Progress progress bar or exit if ESC has been pressed
                            if (!pWorker.Progress())
                            {
                                acTrans.Abort();
                                return;
                            }

                            var acSol = acTrans.GetObject(objId, OpenMode.ForRead) as Solid3d;

                            if (acSol == null) continue;

                            eList.Add(new EntInfo(acSol, acCurDb, acTrans));
                        }

                        eList.SortAndExport(fDirectory, pWorker, acCurDb, acCurEd, acTrans, multAmount);

                        acTrans.Commit();
                    }
                }
            }
        }
    }
}