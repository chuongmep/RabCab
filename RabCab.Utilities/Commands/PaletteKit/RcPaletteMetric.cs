// -----------------------------------------------------------------------------------
//     <copyright file="RcMainPalette.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>04/08/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using System;
using System.Drawing;
using System.Windows.Forms;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using RabCab.Agents;
using RabCab.Settings;

namespace RabCab.Commands.PaletteKit
{
    internal class RcPaletteMetric
    {
        private readonly string _palName = "Metrics";
        private UserControl _palPanel;
        private PaletteSet _rcPal;
        private const int ctrlHeight = 25;
        private const int labColumn = 0;
        private const int infoColumn = 1;

        private TableLayoutPanel tbLayout;

        private Label _rcNameLab,
            _rcInfoLab,
            _rcLengthLab,
            _rcWidthLab,
            _rcThickLab,
            _rcVolLab,
            _rcAreaLab,
            _rcPerimLab,
            _rcAsymLab,
            _rcAsymStrLab,
            _rcQtyOfLab,
            _rcQtyTotalLab,
            _rcNumChangesLab,
            _rcParentLab,
            _rcChildLab,
            _rcTxDirLab,
            _prodTypLab;

        private TextBox
            _rcNameTxt,
            _rcInfoTxt,
            _rcLengthTxt,
            _rcWidthTxt,
            _rcThickTxt,
            _rcVolTxt,
            _rcAreaTxt,
            _rcPerimTxt,
            _rcAsymTxt,
            _rcAsymStrTxt,
            _rcQtyOfTxt,
            _rcQtyTotalTxt,
            _rcNumChangesTxt,
            _rcParentTxt;

        private ListBox _rcChildList;

        private CheckBox _rcIsSweepChk,
            _rcIsiMirChk,
            _rcHasHolesChk,
            _txDirUnknown,
            _txDirNone,
            _txDirHor,
            _txDirVer,
            _prodUnkown,
            _prodS4S,
            _prodMOne,
            _prodMMany;

        private Button _selParent, _selChildren, _updChildren, _upSiblings;

        private StatusStrip _stStrip;

        private ToolStripLabel _stText;

        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_RCMETPAL",
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
        public void Cmd_RcMetPal()
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
                _rcPal = new PaletteSet(_palName, new Guid())
                {
                    Style = PaletteSetStyles.ShowPropertiesMenu
                            | PaletteSetStyles.ShowAutoHideButton
                            | PaletteSetStyles.ShowCloseButton
                };

                _palPanel = new UserControl();

                PopulatePal();
                _palPanel.UpdateTheme();
                _rcPal.Add(_palName, _palPanel);
            }

            _rcPal.Visible = true;
        }

        /// <summary>
        ///     TODO
        /// </summary>
        private void PopulatePal()
        {
            var rowCount = 0;

            var backColor = Colors.GetCadBackColor();
            var foreColor = Colors.GetCadForeColor();
            var textColor = Colors.GetCadTextColor();

            tbLayout = new TableLayoutPanel
            {
                AutoScroll = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = backColor,
                ForeColor = foreColor,
                ColumnCount = 3,
                Dock = DockStyle.Fill
            };

            tbLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80F));
            tbLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tbLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 5F));

            _rcNameLab = new Label
            {
                Text = "Name:", TextAlign = ContentAlignment.MiddleLeft, Anchor = AnchorStyles.None,
                BackColor = backColor, ForeColor = textColor
            };
            _rcInfoLab = new Label
            {
                Text = "Info:", TextAlign = ContentAlignment.MiddleLeft, Anchor = AnchorStyles.None,
                BackColor = backColor, ForeColor = textColor
            };
            _rcLengthLab = new Label
            {
                Text = "Length:", TextAlign = ContentAlignment.MiddleLeft, Anchor = AnchorStyles.None,
                BackColor = backColor, ForeColor = textColor
            };
            _rcWidthLab = new Label
            {
                Text = "Width:", TextAlign = ContentAlignment.MiddleLeft, Anchor = AnchorStyles.None,
                BackColor = backColor, ForeColor = textColor
            };
            _rcThickLab = new Label
            {
                Text = "Thickness:", TextAlign = ContentAlignment.MiddleLeft, Anchor = AnchorStyles.None,
                BackColor = backColor, ForeColor = textColor
            };
            _rcVolLab = new Label
            {
                Text = "Volume:", TextAlign = ContentAlignment.MiddleLeft, Anchor = AnchorStyles.None,
                BackColor = backColor, ForeColor = textColor
            };
            _rcAreaLab = new Label
            {
                Text = "Area:", TextAlign = ContentAlignment.MiddleLeft, Anchor = AnchorStyles.None,
                BackColor = backColor, ForeColor = textColor
            };
            _rcPerimLab = new Label
            {
                Text = "Perimeter:", TextAlign = ContentAlignment.MiddleLeft, Anchor = AnchorStyles.None,
                BackColor = backColor, ForeColor = textColor
            };
            _rcAsymLab = new Label
            {
                Text = "Asym:", TextAlign = ContentAlignment.MiddleLeft, Anchor = AnchorStyles.None,
                BackColor = backColor, ForeColor = textColor
            };
            _rcAsymStrLab = new Label
            {
                Text = "Asym V:", TextAlign = ContentAlignment.MiddleLeft, Anchor = AnchorStyles.None,
                BackColor = backColor, ForeColor = textColor
            };
            _rcQtyOfLab = new Label
            {
                Text = "Qty Of:", TextAlign = ContentAlignment.MiddleLeft, Anchor = AnchorStyles.None,
                BackColor = backColor, ForeColor = textColor
            };
            _rcQtyTotalLab = new Label
            {
                Text = "Qty Total:", TextAlign = ContentAlignment.MiddleLeft, Anchor = AnchorStyles.None,
                BackColor = backColor, ForeColor = textColor
            };
            _rcNumChangesLab = new Label
            {
                Text = "Changes:", TextAlign = ContentAlignment.MiddleLeft, Anchor = AnchorStyles.None,
                BackColor = backColor, ForeColor = textColor
            };
            _rcParentLab = new Label
            {
                Text = "Parent: ", TextAlign = ContentAlignment.MiddleLeft, Anchor = AnchorStyles.None,
                BackColor = backColor, ForeColor = textColor
            };
            _rcChildLab = new Label
            {
                Text = "Children: ", TextAlign = ContentAlignment.TopLeft,
                Anchor = AnchorStyles.None,
                BackColor = backColor, ForeColor = textColor
            };
            _rcTxDirLab = new Label
            {
                Text = "Texture: ", TextAlign = ContentAlignment.TopLeft,
                Anchor = AnchorStyles.None,
                BackColor = backColor, ForeColor = textColor
            };
            _prodTypLab = new Label
            {
                Text = "Production: ", TextAlign = ContentAlignment.TopLeft,
                Anchor = AnchorStyles.None,
                BackColor = backColor, ForeColor = textColor
            };

            _stText = new ToolStripLabel {Text = "Status", BackColor = foreColor, ForeColor = textColor};
            _rcNameTxt = new TextBox {Dock = DockStyle.Fill, BackColor = backColor, ForeColor = textColor};
            _rcInfoTxt = new TextBox {Dock = DockStyle.Fill, BackColor = backColor, ForeColor = textColor};
            _rcLengthTxt = new TextBox {Dock = DockStyle.Fill, ReadOnly = true, BackColor = backColor, ForeColor = textColor};
            _rcWidthTxt = new TextBox {Dock = DockStyle.Fill, ReadOnly = true, BackColor = backColor, ForeColor = textColor};
            _rcThickTxt = new TextBox {Dock = DockStyle.Fill, ReadOnly = true, BackColor = backColor, ForeColor = textColor};
            _rcVolTxt = new TextBox {Dock = DockStyle.Fill, ReadOnly = true, BackColor = backColor, ForeColor = textColor};
            _rcAreaTxt = new TextBox {Dock = DockStyle.Fill, ReadOnly = true, BackColor = backColor, ForeColor = textColor};
            _rcPerimTxt = new TextBox {Dock = DockStyle.Fill, ReadOnly = true, BackColor = backColor, ForeColor = textColor};
            _rcAsymTxt = new TextBox {Dock = DockStyle.Fill, ReadOnly = true, BackColor = backColor, ForeColor = textColor};
            _rcAsymStrTxt = new TextBox {Dock = DockStyle.Fill, ReadOnly = true, BackColor = backColor, ForeColor = textColor};
            _rcQtyOfTxt = new TextBox {Dock = DockStyle.Fill, ReadOnly = true, BackColor = backColor, ForeColor = textColor};
            _rcQtyTotalTxt = new TextBox {Dock = DockStyle.Fill, ReadOnly = true, BackColor = backColor, ForeColor = textColor};
            _rcNumChangesTxt = new TextBox {Dock = DockStyle.Fill, ReadOnly = true, BackColor = backColor, ForeColor = textColor};
            _rcParentTxt = new TextBox {Dock = DockStyle.Fill, ReadOnly = true, BackColor = backColor, ForeColor = textColor};


            _rcChildList = new ListBox {Dock = DockStyle.Fill, BackColor = backColor, ForeColor = textColor};

            _rcIsSweepChk = new CheckBox
            {
                Text = "Is Sweep", Dock = DockStyle.Fill, AutoSize = false, BackColor = backColor, ForeColor = textColor
            };
            _rcIsiMirChk = new CheckBox
            {
                Text = "Is Mirror", Dock = DockStyle.Fill, AutoSize = false, BackColor = backColor,
                ForeColor = textColor
            };
            _rcHasHolesChk = new CheckBox
            {
                Text = "Has Holes", Dock = DockStyle.Fill, AutoSize = false, BackColor = backColor,
                ForeColor = textColor
            };


            _txDirUnknown = new CheckBox
            {
                Text = "UN", Appearance = Appearance.Button, Dock = DockStyle.Fill, BackColor = foreColor,
                ForeColor = textColor
            };
            _txDirNone = new CheckBox
            {
                Text = "NO", Appearance = Appearance.Button, Dock = DockStyle.Fill, BackColor = foreColor,
                ForeColor = textColor
            };
            _txDirHor = new CheckBox
            {
                Text = "HZ", Appearance = Appearance.Button, Dock = DockStyle.Fill, BackColor = foreColor,
                ForeColor = textColor
            };
            _txDirVer = new CheckBox
            {
                Text = "VT", Appearance = Appearance.Button, Dock = DockStyle.Fill, BackColor = foreColor,
                ForeColor = textColor
            };
            var txLayout = new TableLayoutPanel
            {
                AutoScroll = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = backColor,
                ForeColor = foreColor,
                ColumnCount = 4,
                RowCount = 1,
                Dock = DockStyle.Fill
            };

            txLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            txLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            txLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            txLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            txLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            txLayout.Controls.Add(_txDirUnknown, 0, 0);
            txLayout.Controls.Add(_txDirNone, 1, 0);
            txLayout.Controls.Add(_txDirHor, 2, 0);
            txLayout.Controls.Add(_txDirVer, 3, 0);


            _prodUnkown = new CheckBox
            {
                Text = "UN", Appearance = Appearance.Button, Dock = DockStyle.Fill, BackColor = foreColor,
                ForeColor = textColor
            };
            _prodS4S = new CheckBox
            {
                Text = "S4", Dock = DockStyle.Fill, Appearance = Appearance.Button, BackColor = foreColor,
                ForeColor = textColor
            };
            _prodMOne = new CheckBox
            {
                Text = "OS", Appearance = Appearance.Button, Dock = DockStyle.Fill, BackColor = foreColor,
                ForeColor = textColor
            };
            _prodMMany = new CheckBox
            {
                Text = "MS", Appearance = Appearance.Button, Dock = DockStyle.Fill, BackColor = foreColor,
                ForeColor = textColor
            };
            var prodLayout = new TableLayoutPanel
            {
                AutoScroll = false,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = backColor,
                ForeColor = foreColor,
                ColumnCount = 5,
                RowCount = 1,
                Dock = DockStyle.Fill
            };

            prodLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            prodLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            prodLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            prodLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            prodLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            prodLayout.Controls.Add(_prodUnkown, 0, 0);
            prodLayout.Controls.Add(_prodS4S, 1, 0);
            prodLayout.Controls.Add(_prodMOne, 2, 0);
            prodLayout.Controls.Add(_prodMMany, 3, 0);

            _selParent = new Button
                {Text = "Select Parent", Dock = DockStyle.Fill, BackColor = foreColor, ForeColor = textColor};
            _selChildren = new Button
                {Text = "Select Children", Dock = DockStyle.Fill, BackColor = foreColor, ForeColor = textColor};
            _updChildren = new Button
                {Text = "Update Children", Dock = DockStyle.Fill, BackColor = foreColor, ForeColor = textColor};
            _upSiblings = new Button
                {Text = "Update Siblings", Dock = DockStyle.Fill, BackColor = foreColor, ForeColor = textColor};

            _stStrip = new StatusStrip {Dock = DockStyle.Bottom, BackColor = foreColor, ForeColor = textColor};
            _stStrip.Items.Add(_stText);

            #region AddInfoToTable

            AddToTable(_rcNameLab, _rcNameTxt, ctrlHeight, ref rowCount);
            AddToTable(_rcInfoLab, _rcInfoTxt, ctrlHeight, ref rowCount);
            AddToTable(_rcQtyOfLab, _rcQtyOfTxt, ctrlHeight, ref rowCount);
            AddToTable(_rcQtyTotalLab, _rcQtyTotalTxt, ctrlHeight, ref rowCount);
            AddToTable(_rcLengthLab, _rcLengthTxt, ctrlHeight, ref rowCount);
            AddToTable(_rcWidthLab, _rcWidthTxt, ctrlHeight, ref rowCount);
            AddToTable(_rcThickLab, _rcThickTxt, ctrlHeight, ref rowCount);
            AddToTable(_rcVolLab, _rcVolTxt, ctrlHeight, ref rowCount);
            AddToTable(_rcAreaLab, _rcAreaTxt, ctrlHeight, ref rowCount);
            AddToTable(_rcPerimLab, _rcPerimTxt, ctrlHeight, ref rowCount);
            AddToTable(_rcAsymLab, _rcAsymTxt, ctrlHeight, ref rowCount);
            AddToTable(_rcAsymStrLab, _rcAsymStrTxt, ctrlHeight, ref rowCount);
            AddToTable(_rcNumChangesLab, _rcNumChangesTxt, ctrlHeight, ref rowCount);

            AddToTable(_rcParentLab, _rcParentTxt, ctrlHeight, ref rowCount);
            AddToTable(_rcChildLab, _rcChildList, ctrlHeight * 5, ref rowCount);

            AddToTable(_rcTxDirLab, txLayout, ctrlHeight + 10, ref rowCount);
            AddToTable(_prodTypLab, prodLayout, ctrlHeight + 10, ref rowCount);

            AddToTable(new Label(), _rcIsSweepChk, ctrlHeight, ref rowCount);
            AddToTable(new Label(), _rcIsiMirChk, ctrlHeight, ref rowCount);
            AddToTable(new Label(), _rcHasHolesChk, ctrlHeight, ref rowCount);

            AddToTable(new Label(), new Label(), ctrlHeight, ref rowCount);

            #endregion

            _palPanel.Controls.Add(tbLayout);
            _palPanel.Controls.Add(_stStrip);
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="ctrl1"></param>
        /// <param name="ctr2"></param>
        /// <param name="ctHeight"></param>
        /// <param name="rowCount"></param>
        private void AddToTable(Control ctrl1, Control ctr2, float ctHeight, ref int rowCount)
        {
            if (ctrl1 == null) throw new ArgumentNullException(nameof(ctrl1));
            if (ctr2 == null) throw new ArgumentNullException(nameof(ctr2));

            tbLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, ctHeight));
            tbLayout.Controls.Add(ctrl1, labColumn, rowCount);
            tbLayout.Controls.Add(ctr2, infoColumn, rowCount);
            rowCount++;
        }

        #endregion
    }
}