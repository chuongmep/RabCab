using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Autodesk.AutoCAD.Runtime;
using RabCab.Agents;
using RabCab.Entities.Controls;
using RabCab.Settings;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = System.Exception;

namespace RabCab.Commands.TidySuite
{
    internal class RcCleanDirectory
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_CLEANDIRECTORY",
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
        public void Cmd_CleanDirectory()
        {
            if (!LicensingAgent.Check()) return;
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            _ = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            var extensions = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
                {".bak", ".lck", ".dwl", ".dwl2", ".log"};

            //Tell the user we are going to open a dialog window to select a directory
            acCurEd.WriteMessage("\nSelect directory to clean");

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

            var files = new DirectoryInfo(fDirectory).GetFiles("*", SearchOption.AllDirectories);

            var deletableFiles = new List<FileInfo>();

            foreach (var file in files)
            {
                if (extensions.Contains(file.Extension)) deletableFiles.Add(file);

                if (file.FullName.Contains(".dwg") && file.FullName.Contains(".idw")) deletableFiles.Add(file);
            }

            using (var pWorker = new ProgressAgent("Cleaning Directory: ", deletableFiles.Count()))
            {
                foreach (var file in deletableFiles)
                {
                    if (!pWorker.Progress()) return;

                    // try/catch if you really need, but I'd recommend catching a more
                    // specific exception
                    try
                    {
                        file.Attributes = FileAttributes.Normal;
                        File.Delete(file.FullName);
                        acCurEd.WriteMessage($"\nDeleting File: {file.Name}");
                    }
                    catch (Exception)
                    {
                        acCurEd.WriteMessage($"\nFile: {file.Name} could not be deleted!");
                    }
                }
            }
        }
    }
}