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
using System.Drawing;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using RabCab.Agents;
using RabCab.Extensions;
using RabCab.Settings;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using FlowDirection = System.Windows.Forms.FlowDirection;

namespace RabCab.Commands.AssemblySuite
{
    internal class RcSelection
    {
        private readonly List<string> _lastChecked = new List<string>();
        private bool showOnlySelected = true;

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
        public void Cmd_SelectSame()
        {
            //Get the current document utilities
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
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
                var footPanel = new Panel();

                var buttonOk = new Button();
                var buttonCancel = new Button();
                var showButton = new CheckBox();
                showButton.Appearance = Appearance.Button;

                var divider = new Label
                {
                    Text = "",
                    BorderStyle = BorderStyle.Fixed3D,
                    AutoSize = false,
                    Height = 1,
                    Width = bodyPanel.Width - 40
                };

                buttonOk.Text = "OK";
                buttonCancel.Text = "Cancel";
                showButton.Text = "Showing only selected";

                showButton.TextAlign = ContentAlignment.MiddleCenter;

                propF.Controls.Add(bodyPanel);
                propF.Controls.Add(headPanel);
                propF.Controls.Add(footPanel);

                bodyPanel.Dock = DockStyle.Fill;
                headPanel.Dock = DockStyle.Top;
                footPanel.Dock = DockStyle.Bottom;
                buttonOk.Dock = DockStyle.Left;
                buttonCancel.Dock = DockStyle.Right;
                showButton.Dock = DockStyle.Fill;

                buttonOk.BackColor = Colors.GetCadForeColor();
                buttonCancel.BackColor = Colors.GetCadForeColor();
                showButton.BackColor = Colors.GetCadForeColor();

                buttonOk.ForeColor = Colors.GetCadTextColor();
                buttonCancel.ForeColor = Colors.GetCadTextColor();
                showButton.ForeColor = Colors.GetCadTextColor();

                headPanel.BackColor = Colors.GetCadBackColor();
                bodyPanel.BackColor = Colors.GetCadForeColor();
                footPanel.BackColor = Colors.GetCadBackColor();

                headPanel.ForeColor = Colors.GetCadTextColor();
                bodyPanel.ForeColor = Colors.GetCadTextColor();
                footPanel.ForeColor = Colors.GetCadTextColor();

                headPanel.FlowDirection = FlowDirection.LeftToRight;
                bodyPanel.FlowDirection = FlowDirection.TopDown;

                headPanel.AutoScroll = false;
                bodyPanel.AutoScroll = true;
                footPanel.AutoScroll = false;

                headPanel.WrapContents = false;
                bodyPanel.WrapContents = false;

                headPanel.Height = 25;
                footPanel.Height = 25;

                var allBox = new CheckBox
                {
                    AutoSize = true,
                    Text = "Deselect All",
                    ForeColor = Colors.GetCadTextColor(),
                    CheckState = CheckState.Checked
                };

                headPanel.Controls.Add(allBox);

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

                showButton.CheckStateChanged += (sender, e) =>
                {
                    if (showButton.Checked)
                    {
                        showOnlySelected = false;
                        showButton.Text = "Showing all properties";
                        RunPanel(bodyPanel, divider, true);
                    }
                    else
                    {
                        showOnlySelected = true;
                        showButton.Text = "Showing only selected";
                        RunPanel(bodyPanel, divider, false);
                    }
                };

                footPanel.Controls.Add(showButton);
                footPanel.Controls.Add(buttonOk);
                footPanel.Controls.Add(buttonCancel);

                buttonOk.AutoSize = true;
                buttonCancel.AutoSize = true;
                showButton.AutoSize = true;

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
                    if (prop.DisplayName == "Volume") continue;
                    AddCheckBox(bodyPanel, prop.DisplayName + " = " + value);
                }

                foreach (Control c in bodyPanel.Controls)
                {
                    if (!(c is CheckBox cBox)) continue;

                    if (cBox.Checked) continue;

                    allBox.Checked = false;
                    allBox.Text = "Select All";
                }

                allBox.CheckStateChanged += (sender, e) =>
                {
                    allBox.Text = allBox.Checked ? "Deselect All" : "Select All";

                    foreach (Control c in bodyPanel.Controls)
                    {
                        if (!(c is CheckBox cBox)) continue;

                        cBox.Checked = allBox.Checked;
                    }

                    if (allBox.Checked) showButton.Checked = true;
                };

                _lastChecked.Clear();
                RunPanel(bodyPanel, divider, false);

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

                    propFilter.Add(propName);
                    _lastChecked.Add(propName);
                }

                //Dispose the form
                propF.Dispose();

                #endregion

                acEnt.SelectSimilar(propFilter, acCurEd, acCurDb, acTrans, true);

                acTrans.Commit();
            }
        }


        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="panel"></param>
        /// <param name="text"></param>
        private void AddCheckBox(FlowLayoutPanel panel, string text)
        {
            var cBox = new CheckBox();
            cBox.AutoSize = true;
            cBox.Text = text;
            cBox.ForeColor = Colors.GetCadTextColor();

            var contents = text.Split('=');
            var propName = contents[0].Replace(" ", "");

            cBox.Checked = _lastChecked.Contains(propName);

            cBox.CheckStateChanged += (sender, e) =>
            {
                if (cBox.Checked) return;

                if (showOnlySelected) cBox.Visible = false;

                panel.Update();
            };

            panel.Controls.Add(cBox);
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="panel"></param>
        /// <param name="showAll"></param>
        private void RunPanel(FlowLayoutPanel panel, Label divider, bool showAll)
        {
            panel.SuspendLayout();

            var hiddenCount = 0;

            foreach (Control c in panel.Controls)
            {
                if (!(c is CheckBox cBox)) continue;


                if (!showAll)
                {
                    if (!cBox.Checked)
                    {
                        cBox.Visible = false;
                        hiddenCount++;
                    }
                }
                else
                {
                    cBox.Visible = true;
                }
            }

            divider.Visible = hiddenCount != panel.Controls.Count - 1;

            panel.ResumeLayout();
            panel.Update();
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