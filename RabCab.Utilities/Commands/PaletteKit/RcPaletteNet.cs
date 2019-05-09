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

using System;
using System.Windows.Forms;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using RabCab.Agents;
using RabCab.Settings;

namespace RabCab.Commands.PaletteKit
{
    internal class RcPaletteNet
    {
        private PaletteSet _rcPal;
        private UserControl _palPanel;
        private const string PalName = "Web Browser";

        #region Variables

        private WebBrowser _wBrowser;

        private ToolStripMenuItem _fileToolStripMenuItem,
            _saveAsToolStripMenuItem,
            _printToolStripMenuItem,
            _printPreviewToolStripMenuItem,
            _exitToolStripMenuItem,
            _pageSetupToolStripMenuItem,
            _propertiesToolStripMenuItem;

        private ToolStripSeparator _toolStripSeparator1, _toolStripSeparator2;

        private ToolStrip _menuStrip;

        private ToolStripButton _goButton,
            _backButton,
            _forwardButton,
            _stopButton,
            _refreshButton,
            _homeButton,
            _searchButton,
            _printButton,
            _zoomIn,
            _zoomOut;

        private TextBox _addressBox;

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

                PopulatePal();
                _palPanel.UpdateTheme();
                _rcPal.Add(PalName, _palPanel);

                // The following events are not visible in the designer, so 
                // you must associate them with their event-handlers in code.
                _wBrowser.CanGoBackChanged +=
                    webBrowser1_CanGoBackChanged;
                _wBrowser.CanGoForwardChanged +=
                    webBrowser1_CanGoForwardChanged;

                // Load the user's home page.
                _wBrowser.GoHome();
            }

            _rcPal.Visible = true;
        }

        /// <summary>
        ///     TODO
        /// </summary>
        private void PopulatePal()
        {
            var backColor = Colors.GetCadBackColor();
            var foreColor = Colors.GetCadForeColor();
            var textColor = Colors.GetCadTextColor();

            _wBrowser = new WebBrowser();

            _fileToolStripMenuItem = new ToolStripMenuItem();
            _saveAsToolStripMenuItem = new ToolStripMenuItem();
            _toolStripSeparator1 = new ToolStripSeparator();
            _printToolStripMenuItem = new ToolStripMenuItem();
            _printPreviewToolStripMenuItem = new ToolStripMenuItem();
            _toolStripSeparator2 = new ToolStripSeparator();
            _exitToolStripMenuItem = new ToolStripMenuItem();
            _pageSetupToolStripMenuItem = new ToolStripMenuItem();
            _propertiesToolStripMenuItem = new ToolStripMenuItem();

            _menuStrip = new ToolStrip();
            _goButton = new ToolStripButton();
            _backButton = new ToolStripButton();
            _forwardButton = new ToolStripButton();
            _stopButton = new ToolStripButton();
            _refreshButton = new ToolStripButton();
            _homeButton = new ToolStripButton();
            _searchButton = new ToolStripButton();
            _printButton = new ToolStripButton();
            _zoomIn = new ToolStripButton();
            _zoomOut = new ToolStripButton();

            _addressBox = new TextBox();

            _fileToolStripMenuItem.BackColor = foreColor;
            _fileToolStripMenuItem.ForeColor = textColor;

            _fileToolStripMenuItem.DropDownItems.AddRange(
                new ToolStripItem[]
                {
                    _saveAsToolStripMenuItem, _toolStripSeparator1,
                    _pageSetupToolStripMenuItem, _printToolStripMenuItem,
                    _printPreviewToolStripMenuItem, _toolStripSeparator2,
                    _propertiesToolStripMenuItem, _exitToolStripMenuItem
                });

            _fileToolStripMenuItem.Text = "File";
            _saveAsToolStripMenuItem.Text = "Save As...";
            _pageSetupToolStripMenuItem.Text = "Page Setup...";
            _printToolStripMenuItem.Text = "Print...";
            _printPreviewToolStripMenuItem.Text = "Print Preview...";
            _propertiesToolStripMenuItem.Text = "Properties";
            _exitToolStripMenuItem.Text = "Close";

            _printToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.P;

            _saveAsToolStripMenuItem.Click +=
                saveAsToolStripMenuItem_Click;
            _pageSetupToolStripMenuItem.Click +=
                pageSetupToolStripMenuItem_Click;
            _printToolStripMenuItem.Click +=
                printToolStripMenuItem_Click;
            _printPreviewToolStripMenuItem.Click +=
                printPreviewToolStripMenuItem_Click;
            _propertiesToolStripMenuItem.Click +=
                propertiesToolStripMenuItem_Click;
            _exitToolStripMenuItem.Click +=
                exitToolStripMenuItem_Click;

            _goButton.BackColor = foreColor;
            _goButton.ForeColor = textColor;

            _backButton.BackColor = foreColor;
            _backButton.ForeColor = textColor;

            _forwardButton.BackColor = foreColor;
            _forwardButton.ForeColor = textColor;

            _stopButton.BackColor = foreColor;
            _stopButton.ForeColor = textColor;

            _refreshButton.BackColor = foreColor;
            _refreshButton.ForeColor = textColor;

            _homeButton.BackColor = foreColor;
            _homeButton.ForeColor = textColor;

            _searchButton.BackColor = foreColor;
            _searchButton.ForeColor = textColor;

            _printButton.BackColor = foreColor;
            _printButton.ForeColor = textColor;

            _zoomIn.BackColor = foreColor;
            _zoomIn.ForeColor = textColor;

            _zoomOut.BackColor = foreColor;
            _zoomOut.ForeColor = textColor;

            _menuStrip.Items.AddRange(new ToolStripItem[]
            {
                _fileToolStripMenuItem, _homeButton, _searchButton, _goButton, _backButton, _forwardButton, _stopButton,
                _refreshButton, _printButton, _zoomIn, _zoomOut
            });

            _goButton.Text = "Go";
            _backButton.Text = "Back";
            _forwardButton.Text = "Forward";
            _stopButton.Text = "Stop";
            _refreshButton.Text = "Refresh";
            _homeButton.Text = "Home";
            _searchButton.Text = "Search";
            _printButton.Text = "Print";
            _zoomIn.Text = "Zoom In";
            _zoomOut.Text = "Zoom Out";

            _backButton.Enabled = false;
            _forwardButton.Enabled = false;

            _goButton.Click += goButton_Click;
            _backButton.Click += backButton_Click;
            _forwardButton.Click += forwardButton_Click;
            _stopButton.Click += stopButton_Click;
            _refreshButton.Click += refreshButton_Click;
            _homeButton.Click += homeButton_Click;
            _searchButton.Click += searchButton_Click;
            _printButton.Click += printButton_Click;
            _zoomIn.Click += zoomIn_Click;
            _zoomOut.Click += zoomOut_Click;

            _addressBox.Dock = DockStyle.Bottom;
            _addressBox.Height = 25;
            _addressBox.KeyDown +=
                AddressBoxKeyDown;
            _addressBox.Click +=
                AddressBoxClick;

            _wBrowser.Dock = DockStyle.Fill;
            _wBrowser.Navigated +=
                webBrowser1_Navigated;

            _wBrowser.BackColor = backColor;
            _wBrowser.ForeColor = foreColor;

            _menuStrip.BackColor = foreColor;
            _menuStrip.ForeColor = textColor;

            _wBrowser.ScriptErrorsSuppressed = true;

            _palPanel.Controls.AddRange(new Control[]
            {
                _wBrowser, _menuStrip,
                _addressBox
            });
        }

        #endregion

        #region Handlers

        // Displays the Save dialog box.
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _wBrowser.ShowSaveAsDialog();
        }

        private void zoomIn_Click(object sender, EventArgs e)
        {
            // zoom in
            _wBrowser.Focus();
            SendKeys.Send("^{ADD}");
        }

        private void zoomOut_Click(object sender, EventArgs e)
        {
            // zoom out
            _wBrowser.Focus();
            SendKeys.Send("^{SUBTRACT}");
        }

        // Displays the Page Setup dialog box.
        private void pageSetupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _wBrowser.ShowPageSetupDialog();
        }

        // Displays the Print dialog box.
        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _wBrowser.ShowPrintDialog();
        }

        // Displays the Print Preview dialog box.
        private void printPreviewToolStripMenuItem_Click(
            object sender, EventArgs e)
        {
            _wBrowser.ShowPrintPreviewDialog();
        }

        // Displays the Properties dialog box.
        private void propertiesToolStripMenuItem_Click(
            object sender, EventArgs e)
        {
            _wBrowser.ShowPropertiesDialog();
        }

        // Selects all the text in the text box when the user clicks it. 
        private void AddressBoxClick(object sender, EventArgs e)
        {
            _addressBox.SelectAll();
        }

        // Navigates to the URL in the address box when 
        // the ENTER key is pressed while the ToolStripTextBox has focus.
        private void AddressBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) Navigate(_addressBox.Text);
        }

        // Navigates to the URL in the address box when 
        // the Go button is clicked.
        private void goButton_Click(object sender, EventArgs e)
        {
            Navigate(_addressBox.Text);
        }

        // Navigates to the given URL if it is valid.
        private void Navigate(string address)
        {
            if (string.IsNullOrEmpty(address)) return;
            if (address.Equals("about:blank")) return;
            if (!address.StartsWith("http://") &&
                !address.StartsWith("https://"))
                address = "http://" + address;
            try
            {
                _wBrowser.Navigate(new Uri(address));
            }
            catch (UriFormatException)
            {
            }
        }

        // Updates the URL in TextBoxAddress upon navigation.
        private void webBrowser1_Navigated(object sender,
            WebBrowserNavigatedEventArgs e)
        {
            _addressBox.Text = _wBrowser.Url.ToString();
        }

        // Navigates wBrowser to the previous page in the history.
        private void backButton_Click(object sender, EventArgs e)
        {
            _wBrowser.GoBack();
        }

        // Disables the Back button at the beginning of the navigation history.
        private void webBrowser1_CanGoBackChanged(object sender, EventArgs e)
        {
            _backButton.Enabled = _wBrowser.CanGoBack;
        }

        // Navigates wBrowser to the next page in history.
        private void forwardButton_Click(object sender, EventArgs e)
        {
            _wBrowser.GoForward();
        }

        // Disables the Forward button at the end of navigation history.
        private void webBrowser1_CanGoForwardChanged(object sender, EventArgs e)
        {
            _forwardButton.Enabled = _wBrowser.CanGoForward;
        }

        // Halts the current navigation and any sounds or animations on 
        // the page.
        private void stopButton_Click(object sender, EventArgs e)
        {
            _wBrowser.Stop();
        }

        // Reloads the current page.
        private void refreshButton_Click(object sender, EventArgs e)
        {
            // Skip refresh if about:blank is loaded to avoid removing
            // content specified by the DocumentText property.
            if (!_wBrowser.Url.Equals("about:blank")) _wBrowser.Refresh();
        }

        // Navigates wBrowser to the home page of the current user.
        private void homeButton_Click(object sender, EventArgs e)
        {
            _wBrowser.GoHome();
        }

        // Navigates wBrowser to the search page of the current user.
        private void searchButton_Click(object sender, EventArgs e)
        {
            _wBrowser.GoSearch();
        }

        // Prints the current document using the current print settings.
        private void printButton_Click(object sender, EventArgs e)
        {
            _wBrowser.Print();
        }

        // Exits the application.
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _rcPal.Visible = false;
        }

        #endregion
    }
}