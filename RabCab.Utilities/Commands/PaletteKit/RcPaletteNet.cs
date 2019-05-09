// -----------------------------------------------------------------------------------
//     <copyright file="RcPaletteNet.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>04/11/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

#region CefSharp License

// Copyright © The CefSharp Authors. All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are
// met:
//
//    * Redistributions of source code must retain the above copyright
//      notice, this list of conditions and the following disclaimer.
//
//    * Redistributions in binary form must reproduce the above
//      copyright notice, this list of conditions and the following disclaimer
//      in the documentation and/or other materials provided with the
//      distribution.
//
//    * Neither the name of Google Inc. nor the name Chromium Embedded
//      Framework nor the name CefSharp nor the names of its contributors
//      may be used to endorse or promote products derived from this software
//      without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

#endregion

using System;
using System.Windows.Forms;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using CefSharp;
using CefSharp.WinForms;
using RabCab.Agents;
using RabCab.Settings;

namespace RabCab.Commands.PaletteKit
{
    internal class RcPaletteNet
    {
        private PaletteSet _rcPal;
        private UserControl _palPanel;
        private const string PalName = "Web Browser";
        private readonly double _zoomIncrement = 0.5;

        #region Variables

        private ChromiumWebBrowser _wBrowser;

        private ToolStrip _menuStrip;

        private ToolStripButton
            _searchButton,
            _refreshButton,
            _homeButton,
            _zoomIn,
            _zoomOut;

        #endregion

        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_RCNETPAL",
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
        public void Cmd_RcNetPal()
        {
            CreatePal();
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

                Cef.Initialize(new CefSettings());
                _wBrowser = new ChromiumWebBrowser(SettingsUser.NetHomePage) {Dock = DockStyle.Fill};
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

            _menuStrip = new ToolStrip();
            _searchButton = new ToolStripButton();
            _refreshButton = new ToolStripButton();
            _homeButton = new ToolStripButton();
            _zoomIn = new ToolStripButton();
            _zoomOut = new ToolStripButton();

            _searchButton.BackColor = foreColor;
            _searchButton.ForeColor = textColor;

            _refreshButton.BackColor = foreColor;
            _refreshButton.ForeColor = textColor;

            _homeButton.BackColor = foreColor;
            _homeButton.ForeColor = textColor;

            _zoomIn.BackColor = foreColor;
            _zoomIn.ForeColor = textColor;

            _zoomOut.BackColor = foreColor;
            _zoomOut.ForeColor = textColor;

            _menuStrip.Items.AddRange(new ToolStripItem[]
            {
                _searchButton, _homeButton,
                _refreshButton, _zoomIn, _zoomOut
            });

            _refreshButton.Text = "Refresh";
            _searchButton.Text = "Search";
            _homeButton.Text = "Home";
            _zoomIn.Text = "In";
            _zoomOut.Text = "Out";

            _refreshButton.Click += refreshButton_Click;
            _homeButton.Click += homeButton_Click;
            _searchButton.Click += searchButton_Click;
            _zoomIn.Click += zoomIn_Click;
            _zoomOut.Click += zoomOut_Click;


            _menuStrip.BackColor = foreColor;
            _menuStrip.ForeColor = textColor;

            _palPanel.Controls.AddRange(new Control[]
            {
                _wBrowser, _menuStrip
            });
        }

        #endregion

        #region Handlers

        private void zoomIn_Click(object sender, EventArgs e)
        {
            // zoom in
            _wBrowser.SetZoomLevel(_wBrowser.GetZoomLevelAsync().Result + _zoomIncrement);
        }

        private void zoomOut_Click(object sender, EventArgs e)
        {
            // zoom out
            _wBrowser.SetZoomLevel(_wBrowser.GetZoomLevelAsync().Result - _zoomIncrement);
        }

        // Reloads the current page.
        private void refreshButton_Click(object sender, EventArgs e)
        {
            _wBrowser.Refresh();
        }

        // Navigates wBrowser to the home page of the current user.
        private void searchButton_Click(object sender, EventArgs e)
        {
            _wBrowser.Load("www.google.com");
        }

        // Navigates wBrowser to the home page of the current user.
        private void homeButton_Click(object sender, EventArgs e)
        {
            _wBrowser.Load(SettingsUser.NetHomePage);
        }

        #endregion
    }
}