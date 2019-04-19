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

                //Get XData Props
                var partName = acEnt.GetPartName();
                if (!string.IsNullOrEmpty(partName))
                    AddCheckBox(bodyPanel, "Part Name = " + partName);

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

                var entProps = GetProperties(acEnt, propFilter);
                var matchList = new List<ObjectId>();

                //Get the Current Space
                var acCurSpaceBlkTblRec = acTrans.GetObject(acCurDb.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;

                if (acCurSpaceBlkTblRec == null) return;

                var objCount = 0;

                foreach (var objId in acCurSpaceBlkTblRec)
                {
                    var compEnt = acTrans.GetObject(objId, OpenMode.ForRead) as Entity;
                    if (compEnt == null) continue;

                    var compProps = GetProperties(compEnt, propFilter);

                    if (entProps.SequenceEqual(compProps))
                    {
                        matchList.Add(objId);
                    }

                    objCount++;
                }

                if (matchList.Count > 0)
                {
                    acCurEd.SetImpliedSelection(matchList.ToArray());
                    acCurEd.WriteMessage($"\n{objCount} Objects parsed - {matchList.Count} duplicates found.");
                }
                else
                {
                    acCurEd.WriteMessage("\nNo duplicates found.");
                }

                #endregion
                acTrans.Commit();
            }

        }

        private Dictionary<string, string> GetProperties(Entity acEnt, List<string> filter)
        {
            var propDict = new Dictionary<string, string>();

            //Get Com Properties
            var acadObj = acEnt.AcadObject;
            var props = TypeDescriptor.GetProperties(acadObj);

            //Iterate through properties
            foreach (PropertyDescriptor prop in props)
            {
                if (!filter.Contains(prop.DisplayName.ToString())) continue;

                var value = prop.GetValue(acadObj);
                if (value == null) continue;

                var isNumeric = double.TryParse(value.ToString(), out var checkVal);

                if (isNumeric)
                {
                    checkVal = checkVal.RoundToTolerance();
                    propDict.Add(prop.DisplayName, checkVal.ToString());
                }
                else
                {
                    propDict.Add(prop.DisplayName, value.ToString());
                }
   
            }

            //XData Checking
            //NAME
            //

            return propDict;
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