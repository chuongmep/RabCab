// -----------------------------------------------------------------------------------
//     <copyright file="RcSelection.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>04/08/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using RabCab.Analysis;
using RabCab.Exceptions;
using RabCab.Extensions;
using RabCab.Settings;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using FlowDirection = System.Windows.Forms.FlowDirection;

namespace RabCab.Commands.AssemblySuite
{
    internal class RcSelection
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_SELECTSAME",
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
        public void Cmd_SelectSame()
        {
            //Get the current document utilities
            var acCurDoc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            //Get a single selected object from the user
            var objIds = acCurEd.GetAllSelection(true);

            if (objIds.Count() == 0) return;

            using (var acTrans = acCurDoc.TransactionManager.StartTransaction())
            {
                var acSol = acTrans.GetObject(objIds[0], OpenMode.ForRead) as Solid3d;
                if (acSol == null) throw new NullException("3DSolid", acCurEd, acTrans);

                var props = acSol.GetProps();
                var eInfo = new EntInfo(acSol, acCurDb, acTrans);

                #region Form Creation

                //Create a dialog form to hold the property information
                var propF = new Form();
                propF.Text = "Select Same Objects";

                var headPanel = new FlowLayoutPanel();
                var bodyPanel = new FlowLayoutPanel();
                var footPanel = new FlowLayoutPanel();

                var buttonOk = new Button();
                var buttonCancel = new Button();

                buttonOk.Text = "OK";
                buttonCancel.Text = "Cancel";

                propF.Controls.Add(headPanel);
                propF.Controls.Add(bodyPanel);
                propF.Controls.Add(footPanel);

                headPanel.Dock = DockStyle.Top;
                bodyPanel.Dock = DockStyle.Fill;
                footPanel.Dock = DockStyle.Bottom;

                headPanel.FlowDirection = FlowDirection.LeftToRight;
                bodyPanel.FlowDirection = FlowDirection.TopDown;
                footPanel.FlowDirection = FlowDirection.LeftToRight;

                headPanel.AutoScroll = false;
                bodyPanel.AutoScroll = true;
                footPanel.AutoScroll = false;

                headPanel.Height = 30;
                footPanel.Height = 30;

                buttonOk.Click += (sender, e) => { propF.DialogResult = DialogResult.OK; propF.Close();};
                buttonCancel.Click += (sender, e) => { propF.DialogResult = DialogResult.Cancel; propF.Close(); };

                //First Add XData Properties
                //TODO
                //Then Add Com Properties
                //TODO

                propF.ShowDialog();

                if (propF.DialogResult == DialogResult.OK)
                {

                }

                propF.Close();
                propF.Dispose();

                //Get the selected properties to match


                #endregion


            }
        }

        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_SELECTUNNAMED",
            CommandFlags.Modal
            //| CommandFlags.Transparent
            | CommandFlags.UsePickSet
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
        public void Cmd_SelectUnNamed()
        {
            //Get the current document utilities
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            var acSol = acCurEd.GetAllSelection(false);
        }
    }
}