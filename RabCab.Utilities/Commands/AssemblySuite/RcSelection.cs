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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using RabCab.Agents;
using RabCab.Analysis;
using RabCab.Calculators;
using RabCab.Engine.System;
using RabCab.Exceptions;
using RabCab.Extensions;
using RabCab.Settings;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using Button = System.Windows.Forms.Button;
using CheckBox = System.Windows.Forms.CheckBox;
using Control = System.Windows.Forms.Control;
using FlowDirection = System.Windows.Forms.FlowDirection;
using Label = System.Windows.Forms.Label;

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
            var peo = new PromptEntityOptions("\nSelect object: ");
            var res = acCurEd.GetEntity(peo);

            if (res.Status != PromptStatus.OK) return;

            using (var acTrans = acCurDoc.TransactionManager.StartTransaction())
            {
                var acEnt = acTrans.GetObject(res.ObjectId, OpenMode.ForRead) as Entity;
                if (acEnt == null) return;

                #region Form Creation

                //Create a dialog form to hold the property information
                var propF = new Form();
                propF.Text = "Select Same Objects";

                propF.Width = 300;
                propF.Height = 450;
                propF.StartPosition = FormStartPosition.CenterParent;
                propF.MinimizeBox = false;
                propF.MaximizeBox = false;
                propF.FormBorderStyle = FormBorderStyle.FixedDialog;

                var headPanel = new FlowLayoutPanel();
                var bodyPanel = new FlowLayoutPanel();
                var footPanel = new FlowLayoutPanel();

                var buttonOk = new Button();
                var buttonCancel = new Button();

                buttonOk.Text = "OK";
                buttonCancel.Text = "Cancel";

                propF.Controls.Add(bodyPanel);
                propF.Controls.Add(headPanel);
                propF.Controls.Add(footPanel);

                bodyPanel.Dock = DockStyle.Fill;
                headPanel.Dock = DockStyle.Top;
                footPanel.Dock = DockStyle.Bottom;

                headPanel.BackColor = Colors.GetCadBackColor();
                bodyPanel.BackColor = Colors.GetCadForeColor();
                footPanel.BackColor = Colors.GetCadBackColor();

                headPanel.ForeColor = Colors.GetCadTextColor();
                bodyPanel.ForeColor = Colors.GetCadTextColor();
                footPanel.ForeColor = Colors.GetCadTextColor();

                headPanel.FlowDirection = FlowDirection.LeftToRight;
                bodyPanel.FlowDirection = FlowDirection.TopDown;
                footPanel.FlowDirection = FlowDirection.LeftToRight;

                headPanel.AutoScroll = false;
                bodyPanel.AutoScroll = true;
                footPanel.AutoScroll = false;

                headPanel.WrapContents = false;
                bodyPanel.WrapContents = false;
                footPanel.WrapContents = false;

                headPanel.Height = 25;
                footPanel.Height = 25;

                buttonOk.Click += (sender, e) =>
                {
                    propF.DialogResult = DialogResult.OK;
                    propF.Close();
                };
                buttonCancel.Click += (sender, e) =>
                {
                    propF.DialogResult = DialogResult.Cancel;
                    propF.Close();
                };

                footPanel.Controls.Add(buttonOk);
                footPanel.Controls.Add(buttonCancel);

                buttonOk.AutoSize = true;
                buttonCancel.AutoSize = true;

                //NAME
                AddCheckBox(bodyPanel, "Part Name = " + acEnt.GetPartName());
                //LENGTH
                AddCheckBox(bodyPanel, "Length = " + acEnt.GetPartLength());
                //WIDTH
                AddCheckBox(bodyPanel, "Width = " + acEnt.GetPartWidth());
                //THICKNESS
                AddCheckBox(bodyPanel, "Thickness = " + acEnt.GetPartThickness());
                //VOLUME
                AddCheckBox(bodyPanel, "Volume = " + acEnt.GetPartVolume());
                //ISSWEEP
                AddCheckBox(bodyPanel, "Is Sweep = " + acEnt.GetIsSweep());
                //ISMIRROR
                AddCheckBox(bodyPanel, "Is Mirror = " + acEnt.GetIsMirror());
                //HASHOLES
                AddCheckBox(bodyPanel, "Has Holes = " + acEnt.GetHasHoles());
                //TXDIRECTION
                AddCheckBox(bodyPanel, "Texture Direction = " + acEnt.GetTextureDirection());
                //PRODTYPE
                AddCheckBox(bodyPanel, "Production Type = " + acEnt.GetProductionType());
                //BASEHANDLE
                AddCheckBox(bodyPanel, "Parent Handle = " + acEnt.GetParent());

                var divider = new Label();
                divider.Text = "";
                divider.BorderStyle = BorderStyle.Fixed3D;
                divider.AutoSize = false;
                divider.Height = 2;

                bodyPanel.Controls.Add(divider);

                //Get Com Properties
                var acadObj = acEnt.AcadObject;
                var props = TypeDescriptor.GetProperties(acadObj);

                //Iterate through properties
                foreach (PropertyDescriptor prop in props)
                {
                    var value = prop.GetValue(acadObj);
                    if (value == null) continue;
                    if (value.ToString().Contains("System.")) continue;
                    AddCheckBox(bodyPanel, prop.DisplayName + " = " + value);
                }

                propF.ShowDialog();

                if (propF.DialogResult != DialogResult.OK) return;

                var checkedProps = new List<CheckBox>();

                foreach (Control c in bodyPanel.Controls)
                {
                    if (!(c is CheckBox cBox)) continue;

                    if (cBox.Checked)
                        checkedProps.Add(cBox);
                }

                if (checkedProps.Count == 0) return;

                var propFilter = new List<string>();

                foreach (var cBox in checkedProps)
                {
                    var contents = cBox.Text.Split('=');
                    var propName = contents[0].Replace(" ", "");
                    var propValue = contents[1].Replace(" ", "");

                    propFilter.Add(propName);
                }

                //Dispose the form
                propF.Dispose();

                acEnt.SelectSimilar(propFilter, acCurEd, acCurDb, acTrans, true);
                #endregion
                acTrans.Commit();
            }

        }
      

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="panel"></param>
        /// <param name="text"></param>
        private void AddCheckBox(FlowLayoutPanel panel, string text)
        {
            var cBox = new CheckBox();
            cBox.AutoSize = true;
            cBox.Text = text;
            cBox.ForeColor = Colors.GetCadTextColor();
            cBox.CheckState = CheckState.Unchecked;
            panel.Controls.Add(cBox);
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