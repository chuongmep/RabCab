// -----------------------------------------------------------------------------------
//     <copyright file="RcPaletteNotebook.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>04/11/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using System;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using RabCab.Agents;
using RabCab.Settings;
using Exception = Autodesk.AutoCAD.Runtime.Exception;

namespace RabCab.Commands.PaletteKit
{
    internal class RcPaletteNotebook
    {
        private PaletteSet _rcPal;
        private UserControl _palPanel;
        private const string PalName = "Notebook";
        private TextBox _noteBox;

        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_RCNOTEPAL",
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
        public void Cmd_RcNotePal()
        {
            CreatePal();
        }

        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_UPDNOTEPAL",
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
        public void Cmd_UpdNotePal()
        {
            UpdatePal();
        }

        #region Pal Initialization

        /// <summary>
        ///     TODO
        /// </summary>
        private void CreatePal()
        {
            if (_rcPal == null)
            {
                _rcPal = new PaletteSet(PalName, new Guid())
                {
                    Style = PaletteSetStyles.ShowPropertiesMenu
                            | PaletteSetStyles.ShowAutoHideButton
                            | PaletteSetStyles.ShowCloseButton
                };

                _palPanel = new UserControl();

                PopulatePal();
                _palPanel.UpdateTheme();
                _rcPal.Add(PalName, _palPanel);
            }

            _rcPal.Visible = true;
        }

        /// <summary>
        ///     TODO
        /// </summary>
        private void PopulatePal()
        {
            // Create an instance of a TextBox control.

            _noteBox = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                AcceptsReturn = true,
                AcceptsTab = true,
                WordWrap = true,
                Dock = DockStyle.Fill
            };

            var resBuf = XDataAgent.GetXrecord(SettingsInternal.CommandGroup, PalName);
            var contents = (string) resBuf.AsArray()[0].Value;
            _noteBox.Text = contents;

            _noteBox.TextChanged += text_TextChanged;

            _palPanel.Controls.Add(_noteBox);
        }

        /// <summary>
        ///     TODO
        /// </summary>
        private void UpdatePal()
        {
            var resBuf = XDataAgent.GetXrecord(SettingsInternal.CommandGroup, PalName);
            var contents = (string) resBuf.AsArray()[0].Value;
            _noteBox.Text = contents;
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void text_TextChanged(object sender, EventArgs e)
        {
            if (!(sender is TextBox tBox)) return;

            try
            {
                var data = new ResultBuffer(new TypedValue((int) DxfCode.Text, tBox.Text));
                XDataAgent.SetXrecord(SettingsInternal.CommandGroup, PalName, data);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion
    }
}