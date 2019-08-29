// -----------------------------------------------------------------------------------
//     <copyright file="Debugging.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>04/09/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using System;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using RabCab.Agents;
using RabCab.Extensions;
using RabCab.External.XmlAgent;
using RabCab.Settings;

// This line is not mandatory, but improves loading performances

namespace RabCab.Initialization
{
    // This class is instantiated by AutoCAD for each document when
    // a command is called by the user the first time in the context
    // of a given document. In other words, non static data in this class
    // is implicitly per-document!
    public class Debugging
    {
        public void Cmd_TestXml()
        {
            var reader = new XmlAgent();
            var cols =
                reader.GetXmlAttributes(
                    @"C:\Users\zayers\Documents\GitHub\RabCab.Utilities\RabCab.Utilities\Repository\RepoFasteners.xml",
                    "bolt");

            Sandbox.WriteLine("");
            Sandbox.WriteLine("Parsing Collection...");

            foreach (var attCol in cols)
            foreach (var att in attCol)
                Sandbox.WriteLine("  " + att.LocalName + " - " + att.Value);
        }

        public void Cmd_TestMats()
        {
            var reader = new XmlAgent();
            var cols =
                reader.GetXmlAttributes(
                    @"C:\Users\zayers\Documents\GitHub\RabCab.Utilities\RabCab.Utilities\Repository\RepoMaterials.xml",
                    "machMaterial");

            Sandbox.WriteLine("");
            Sandbox.WriteLine("Parsing Collection...");

            foreach (var attCol in cols)
            foreach (var att in attCol)
                Sandbox.WriteLine("  " + att.LocalName + " - " + att.Value);
        }

        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_DEBUGBOXES",
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
            | CommandFlags.NoBlockEditor
            | CommandFlags.NoActionRecording
            | CommandFlags.ActionMacro
            //| CommandFlags.NoInferConstraint 
        )]
        public void Cmd_DEBUGBOXES()
        {
            if (!LicensingAgent.Check()) return;

            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            var boxCount = 500;
            var maxSize = 40;
            var minSize = 5;

            var minX = -500;
            var minY = -500;
            var minZ = -500;

            var maxX = 500;
            var maxY = 500;
            var maxZ = 500;
            var random = new Random();

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                for (var i = 0; i < boxCount; i++)
                {
                    var acSol = new Solid3d();

                    var length = random.Next(minSize, maxSize);
                    var width = random.Next(minSize, maxSize);

                    var height = random.Next(minSize, maxSize);

                    acSol.CreateBox(length, width, height);

                    var insertPoint = new Point3d(random.Next(minX, maxX), random.Next(minY, maxY),
                        random.Next(minZ, maxZ));

                    acCurDb.AppendEntity(acSol);

                    acSol.Move(acSol.GetBoxCenter(), insertPoint);
                    acSol.CleanBody();
                }

                acTrans.Commit();
            }
        }
    }
}