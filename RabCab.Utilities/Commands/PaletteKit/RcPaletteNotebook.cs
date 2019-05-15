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
        private const string PalName = "Notebook";
        private const string Name = "NoteBox";
        private static UserControl _palPanel;
        private static bool _reWriteData = true;
        private static TextBox _noteBox;
        private static PaletteSet _rcPal;


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

        internal static void UpdNotePal()
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
            var foreColor = Colors.GetCadForeColor();
            var textColor = Colors.GetCadTextColor();

            _noteBox = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                AcceptsReturn = true,
                AcceptsTab = true,
                WordWrap = true,
                Dock = DockStyle.Fill,
                Name = Name,
                BackColor = foreColor,
                ForeColor = textColor
            };

            var resBuf = XDataAgent.GetXrecord(SettingsInternal.CommandGroup, PalName);

            if (resBuf != null && resBuf.AsArray().Length > 0)
            {
                var contents = (string) resBuf.AsArray()[0].Value;
                _noteBox.Text = contents;
            }

            _noteBox.TextChanged += text_TextChanged;

            _palPanel.Controls.Add(_noteBox);
        }

        /// <summary>
        ///     TODO
        /// </summary>
        private static void UpdatePal()
        {
            if (_rcPal == null) return;

            _reWriteData = false;

            var resBuf = XDataAgent.GetXrecord(SettingsInternal.CommandGroup, PalName);

            if (resBuf != null && resBuf.AsArray().Length > 0)
            {
                var contents = (string) resBuf.AsArray()[0].Value;
                _noteBox.Text = contents;
            }
            else
            {
                _noteBox.Text = "";
            }

            _reWriteData = true;
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void text_TextChanged(object sender, EventArgs e)
        {
            if (!_reWriteData) return;
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