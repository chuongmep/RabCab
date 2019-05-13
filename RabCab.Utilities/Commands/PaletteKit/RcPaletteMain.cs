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
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace RabCab.Commands.PaletteKit
{
    internal class RcPaletteMain
    {
        private readonly string _palName = "RabCab";
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

        private TextBox _statusInfo,
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

        private CheckBox _rcIsSweepChk, _rcIsiMirChk, _rcHasHolesChk;

        private GroupBox _rcTxDirGrp, _rcProdTypGrp;

        private RadioButton _txDirUnknown,
            _txDirNone,
            _txDirHor,
            _txDirVer,
            _prodUnkown,
            _prodBox,
            _prodSweep,
            _prodS4S,
            _prodMOne,
            _prodMMany;

        private Button _selParent, _selChildren, _updChildren, _upSiblings;

        private StatusStrip _stStrip;

        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_RCMAINPAL",
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
        public void Cmd_RcMainPal()
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

            tbLayout.MouseEnter += (s, e) => tbLayout.Focus();

            tbLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50F));
            tbLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tbLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 5F));

            _rcNameLab = new Label
            {
                Text = "Name:", TextAlign = ContentAlignment.MiddleLeft, Anchor = AnchorStyles.None,
                BackColor = foreColor, ForeColor = textColor
            };
            _rcInfoLab = new Label
            {
                Text = "Info:", TextAlign = ContentAlignment.MiddleLeft, Anchor = AnchorStyles.None,
                BackColor = foreColor, ForeColor = textColor
            };
            _rcLengthLab = new Label
            {
                Text = "Length:", TextAlign = ContentAlignment.MiddleLeft, Anchor = AnchorStyles.None,
                BackColor = foreColor, ForeColor = textColor
            };
            _rcWidthLab = new Label
            {
                Text = "Width:", TextAlign = ContentAlignment.MiddleLeft, Anchor = AnchorStyles.None,
                BackColor = foreColor, ForeColor = textColor
            };
            _rcThickLab = new Label
            {
                Text = "Thickness:", TextAlign = ContentAlignment.MiddleLeft, Anchor = AnchorStyles.None,
                BackColor = foreColor, ForeColor = textColor
            };
            _rcVolLab = new Label
            {
                Text = "Volume:", TextAlign = ContentAlignment.MiddleLeft, Anchor = AnchorStyles.None,
                BackColor = foreColor, ForeColor = textColor
            };
            _rcAreaLab = new Label
            {
                Text = "Area:", TextAlign = ContentAlignment.MiddleLeft, Anchor = AnchorStyles.None,
                BackColor = foreColor, ForeColor = textColor
            };
            _rcPerimLab = new Label
            {
                Text = "Perimeter:", TextAlign = ContentAlignment.MiddleLeft, Anchor = AnchorStyles.None,
                BackColor = foreColor, ForeColor = textColor
            };
            _rcAsymLab = new Label
            {
                Text = "Asymmetry:", TextAlign = ContentAlignment.MiddleLeft, Anchor = AnchorStyles.None,
                BackColor = foreColor, ForeColor = textColor
            };
            _rcAsymStrLab = new Label
            {
                Text = "Asym Vector:", TextAlign = ContentAlignment.MiddleLeft, Anchor = AnchorStyles.None,
                BackColor = foreColor, ForeColor = textColor
            };
            _rcQtyOfLab = new Label
            {
                Text = "Qty Of:", TextAlign = ContentAlignment.MiddleLeft, Anchor = AnchorStyles.None,
                BackColor = foreColor, ForeColor = textColor
            };
            _rcQtyTotalLab = new Label
            {
                Text = "Qty Total:", TextAlign = ContentAlignment.MiddleLeft, Anchor = AnchorStyles.None,
                BackColor = foreColor, ForeColor = textColor
            };
            _rcNumChangesLab = new Label
            {
                Text = "Num Changes:", TextAlign = ContentAlignment.MiddleLeft, Anchor = AnchorStyles.None,
                BackColor = foreColor, ForeColor = textColor
            };
            _rcParentLab = new Label
            {
                Text = "Parent: ", TextAlign = ContentAlignment.MiddleLeft, Anchor = AnchorStyles.None,
                BackColor = foreColor, ForeColor = textColor
            };
            _rcChildLab = new Label
            {
                Text = "Children: ", TextAlign = ContentAlignment.TopLeft, Anchor = AnchorStyles.None,
                BackColor = foreColor, ForeColor = textColor
            };
            _rcTxDirLab = new Label
            {
                Text = "Texture: ", TextAlign = ContentAlignment.TopLeft, Anchor = AnchorStyles.None,
                BackColor = foreColor, ForeColor = textColor
            };
            _prodTypLab = new Label
            {
                Text = "Production: ", TextAlign = ContentAlignment.TopLeft, Anchor = AnchorStyles.None,
                BackColor = foreColor, ForeColor = textColor
            };

            _statusInfo = new TextBox {Dock = DockStyle.Fill, BackColor = foreColor, ForeColor = textColor};
            _rcNameTxt = new TextBox {Dock = DockStyle.Fill, BackColor = foreColor, ForeColor = textColor};
            _rcInfoTxt = new TextBox {Dock = DockStyle.Fill, BackColor = foreColor, ForeColor = textColor};
            _rcLengthTxt = new TextBox {Dock = DockStyle.Fill, BackColor = foreColor, ForeColor = textColor};
            _rcWidthTxt = new TextBox {Dock = DockStyle.Fill, BackColor = foreColor, ForeColor = textColor};
            _rcThickTxt = new TextBox {Dock = DockStyle.Fill, BackColor = foreColor, ForeColor = textColor};
            _rcVolTxt = new TextBox {Dock = DockStyle.Fill, BackColor = foreColor, ForeColor = textColor};
            _rcAreaTxt = new TextBox {Dock = DockStyle.Fill, BackColor = foreColor, ForeColor = textColor};
            _rcPerimTxt = new TextBox {Dock = DockStyle.Fill, BackColor = foreColor, ForeColor = textColor};
            _rcAsymTxt = new TextBox {Dock = DockStyle.Fill, BackColor = foreColor, ForeColor = textColor};
            _rcAsymStrTxt = new TextBox {Dock = DockStyle.Fill, BackColor = foreColor, ForeColor = textColor};
            _rcQtyOfTxt = new TextBox {Dock = DockStyle.Fill, BackColor = foreColor, ForeColor = textColor};
            _rcQtyTotalTxt = new TextBox {Dock = DockStyle.Fill, BackColor = foreColor, ForeColor = textColor};
            _rcNumChangesTxt = new TextBox {Dock = DockStyle.Fill, BackColor = foreColor, ForeColor = textColor};
            _rcParentTxt = new TextBox {Dock = DockStyle.Fill, BackColor = foreColor, ForeColor = textColor};


            _rcChildList = new ListBox {Dock = DockStyle.Fill, BackColor = foreColor, ForeColor = textColor};

            _rcIsSweepChk = new CheckBox
                {Text = "Is Sweep", Dock = DockStyle.Fill, BackColor = foreColor, ForeColor = textColor};
            _rcIsiMirChk = new CheckBox
                {Text = "Is Mirror", Dock = DockStyle.Fill, BackColor = foreColor, ForeColor = textColor};
            _rcHasHolesChk = new CheckBox
                {Text = "Has Holes", Dock = DockStyle.Fill, BackColor = foreColor, ForeColor = textColor};

            _rcTxDirGrp = new GroupBox {Text = "", BackColor = foreColor, ForeColor = textColor};
            _rcProdTypGrp = new GroupBox {Text = "", BackColor = foreColor, ForeColor = textColor};

            _txDirUnknown = new RadioButton {Text = "Unknown", BackColor = foreColor, ForeColor = textColor};
            _txDirNone = new RadioButton {Text = "None", BackColor = foreColor, ForeColor = textColor};
            _txDirHor = new RadioButton {Text = "Horizontal", BackColor = foreColor, ForeColor = textColor};
            _txDirVer = new RadioButton {Text = "Vertical", BackColor = foreColor, ForeColor = textColor};

            _rcTxDirGrp.Controls.Add(new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown, Dock = DockStyle.Fill, BackColor = foreColor,
                ForeColor = textColor
            });
            _rcTxDirGrp.Controls.Add(_txDirUnknown);
            _rcTxDirGrp.Controls.Add(_txDirNone);
            _rcTxDirGrp.Controls.Add(_txDirHor);
            _rcTxDirGrp.Controls.Add(_txDirVer);

            _prodUnkown = new RadioButton {Text = "Unknown", BackColor = foreColor, ForeColor = textColor};
            _prodBox = new RadioButton {Text = "Box", BackColor = foreColor, ForeColor = textColor};
            _prodSweep = new RadioButton {Text = "Sweep", BackColor = foreColor, ForeColor = textColor};
            _prodS4S = new RadioButton {Text = "S4S", BackColor = foreColor, ForeColor = textColor};
            _prodMOne = new RadioButton {Text = "Milling - One Side", BackColor = foreColor, ForeColor = textColor};
            _prodMMany = new RadioButton {Text = "Milling - Many Side", BackColor = foreColor, ForeColor = textColor};

            _rcProdTypGrp.Controls.Add(new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown, Dock = DockStyle.Fill, BackColor = foreColor,
                ForeColor = textColor
            });
            _rcProdTypGrp.Controls.Add(_prodUnkown);
            _rcProdTypGrp.Controls.Add(_prodBox);
            _rcProdTypGrp.Controls.Add(_prodSweep);
            _rcProdTypGrp.Controls.Add(_prodS4S);
            _rcProdTypGrp.Controls.Add(_prodMOne);
            _rcProdTypGrp.Controls.Add(_prodMMany);

            _selParent = new Button
                {Text = "Select Parent", Dock = DockStyle.Fill, BackColor = foreColor, ForeColor = textColor};
            _selChildren = new Button
                {Text = "Select Children", Dock = DockStyle.Fill, BackColor = foreColor, ForeColor = textColor};
            _updChildren = new Button
                {Text = "Update Children", Dock = DockStyle.Fill, BackColor = foreColor, ForeColor = textColor};
            _upSiblings = new Button
                {Text = "Update Siblings", Dock = DockStyle.Fill, BackColor = foreColor, ForeColor = textColor};

            _stStrip = new StatusStrip {Dock = DockStyle.Bottom, BackColor = foreColor, ForeColor = textColor};
            _stStrip.Controls.Add(_statusInfo);

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

            AddToTable(new Label(), _rcIsSweepChk, ctrlHeight, ref rowCount);
            AddToTable(new Label(), _rcIsiMirChk, ctrlHeight, ref rowCount);
            AddToTable(new Label(), _rcHasHolesChk, ctrlHeight, ref rowCount);

            AddToTable(_rcParentLab, _rcParentTxt, ctrlHeight, ref rowCount);
            AddToTable(_rcChildLab, _rcChildList, ctrlHeight * 6, ref rowCount);

            AddToTable(_rcTxDirLab, _rcTxDirGrp, ctrlHeight * 4, ref rowCount);
            AddToTable(_prodTypLab, _rcProdTypGrp, ctrlHeight * 6, ref rowCount);

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
            tbLayout.Controls.Add(_rcNameLab, labColumn, rowCount);
            tbLayout.Controls.Add(_rcNameTxt, infoColumn, rowCount);
            rowCount++;
        }

        #endregion
    }
}