using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using RabCab.Agents;
using RabCab.Engine.Enumerators;
using RabCab.Engine.System;
using RabCab.Entities.Controls;
using RabCab.Extensions;
using RabCab.Settings;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Color = Autodesk.AutoCAD.Colors.Color;
using Exception = Autodesk.AutoCAD.BoundaryRepresentation.Exception;

namespace RabCab.Commands.PaletteKit
{
    internal class RcPaletteLayer
    {
        private PaletteSet _rcPal;
        private UserControl _palPanel;
        private readonly string _palName = "Layers";

        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_RCLAYERPAL",
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
        public void Cmd_RcLayerPal()
        {
            //Get the current document utilities
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

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

        private void PopulatePal()
        {
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            const int imageColumn = 0;
            const int buttonColumn = 1;
            const int barOffset = 10;
            const int buttonOffset = 1;
            const int buttonHeight = 25;

            var backColor = Colors.GetCadBackColor();
            var foreColor = Colors.GetCadForeColor();
            var textColor = Colors.GetCadTextColor();

            var rowCounter = 0;
            try
            {
                using (acCurDoc.LockDocument())
                {
                    _palPanel.Controls.Clear();
                    _palPanel.AutoScroll = false;

                    #region Table Layout

                    var palLayout = new TableLayoutPanel
                    {
                        AutoScroll = true,
                        AutoSizeMode = AutoSizeMode.GrowAndShrink,
                        BackColor = Colors.GetCadBackColor(),
                        ForeColor = Colors.GetCadForeColor(),
                        ColumnCount = 3,
                        Dock = DockStyle.Fill,
                        Location = new Point(0, 0),
                        Name = "PalLayout"
                    };

                    palLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 25F));
                    palLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                    palLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 5F));

                    #endregion

                    using (var acTrans = acCurDb.TransactionManager.StartTransaction())
                    {
                        var layerGroups = acCurDb.GetLayerTableRecord(acTrans, 0)
                            .GroupBy(layer => layer.Name.Split(SettingsUser.LayerDelimiter)[0])
                            .OrderBy(layer => layer.Key);

                        foreach (var group in layerGroups)
                        {
                            palLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, buttonHeight + 5));

                            if (group.Count() > 1)
                            {
                                var spButton = new SplitButton
                                {
                                    ShowSplit = true,
                                    BackColor = foreColor,
                                    ForeColor = textColor,
                                    Dock = DockStyle.Fill,
                                    Height = buttonHeight,
                                    ContextMenuStrip = new ContextMenuStrip(),
                                    FlatStyle = FlatStyle.Flat
                                };

                                spButton.FlatAppearance.BorderColor = SystemColors.WindowFrame;
                                spButton.FlatAppearance.BorderSize = 1;
                                spButton.ContextMenuStrip.BackColor = foreColor;
                                spButton.ContextMenuStrip.ForeColor = textColor;
                                spButton.ContextMenuStrip.ShowImageMargin = false;
                                spButton.ContextMenuStrip.ShowCheckMargin = false;

                                Color spColor = null;
                                var firstParse = true;

                                foreach (var layer in group)
                                {
                                    if (firstParse)
                                    {
                                        spButton.Text = layer.Name;
                                        spColor = layer.Color;
                                        firstParse = false;
                                    }

                                    var tsButton = new ToolStripButton(layer.Name, GetLayerImage(layer.Color),
                                        contextItem_Click)
                                    {
                                        ImageAlign = ContentAlignment.TopLeft,
                                        TextAlign = ContentAlignment.MiddleLeft,
                                        BackColor = foreColor,
                                        ForeColor = textColor
                                    };

                                    spButton.ContextMenuStrip.Items.Add(tsButton);
                                }

                                var picBox = new PictureBox
                                {
                                    Height = buttonHeight,
                                    Image = GetLayerImage(spColor),
                                    Anchor = AnchorStyles.None
                                };

                                palLayout.Controls.Add(spButton, buttonColumn, rowCounter);
                                palLayout.Controls.Add(picBox, imageColumn, rowCounter);
                                rowCounter++;
                            }
                            else
                            {
                                var layer = group.First();

                                var button = new Button
                                {
                                    Text = layer.Name,
                                    BackColor = foreColor,
                                    ForeColor = textColor,
                                    Dock = DockStyle.Fill,
                                    Height = buttonHeight,
                                    FlatStyle = FlatStyle.Flat
                                };

                                button.FlatAppearance.BorderColor = SystemColors.WindowFrame;
                                button.FlatAppearance.BorderSize = 1;

                                var picBox = new PictureBox
                                {
                                    Height = buttonHeight,
                                    Image = GetLayerImage(layer.Color),
                                    Anchor = AnchorStyles.None
                                };


                                palLayout.Controls.Add(button, buttonColumn, rowCounter);
                                palLayout.Controls.Add(picBox, imageColumn, rowCounter);
                                rowCounter++;
                            }
                        }

                        //Add a blank label to the final row to keep from having a giant row at the bottom
                        var blankLabel = new Label {Height = buttonHeight};
                        palLayout.Controls.Add(blankLabel, buttonColumn, rowCounter + 1);
                        palLayout.RowCount++;

                        acTrans.Commit();
                    }

                    palLayout.AutoScroll = true;
                    palLayout.AutoSize = true;
                    palLayout.Refresh();

                    if (palLayout.VerticalScroll.Visible)
                    {
                        palLayout.ColumnStyles[2].SizeType = SizeType.Absolute;
                        palLayout.ColumnStyles[2].Width = barOffset;
                    }
                    else
                    {
                        palLayout.ColumnStyles[2].SizeType = SizeType.Absolute;
                        palLayout.ColumnStyles[2].Width = buttonOffset;
                    }

                    palLayout.Refresh();

                    _palPanel.Controls.Add(palLayout);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void contextItem_Click(object sender, EventArgs e)
        {
            var item = (ToolStripItem) sender;
            if (item == null) return;

            //TODO
        }



        #endregion

        /// <summary>
        ///     Method to create a colored square for displaying layer color
        /// </summary>
        /// <param name="acColor"></param>
        /// <returns></returns>
        private Image GetLayerImage(Color acColor)
        {
            var squareSize = new Size(18, 18);
            var brush = new SolidBrush(AcadColorAciToDrawingColor(acColor));
            var outlineColor = System.Drawing.Color.Black;

            if (AcVars.ColorTheme == Enums.ColorTheme.Dark) outlineColor = System.Drawing.Color.White;

            var squareImage = new Bitmap(squareSize.Width, squareSize.Height);
            using (var graphics = Graphics.FromImage(squareImage))
            {
                var pen = new Pen(outlineColor, 1) {Alignment = PenAlignment.Center};
                graphics.FillRectangle(brush, 3, 3, squareSize.Width, squareSize.Height);
                graphics.DrawRectangle(pen, 3, 3, squareSize.Width - 4, squareSize.Height - 4);
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
            }

            return squareImage;
        }

        /// <summary>
        ///     Method to parse an Autocad COLOR and convert it to System Color
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        private System.Drawing.Color AcadColorAciToDrawingColor(Color color)
        {
            var aci = Convert.ToByte(color.ColorIndex);
            var aRgb = EntityColor.LookUpRgb(aci);
            var ch = BitConverter.GetBytes(aRgb);
            if (!BitConverter.IsLittleEndian) Array.Reverse(ch);
            int r = ch[2];
            int g = ch[1];
            int b = ch[0];

            return System.Drawing.Color.FromArgb(r, g, b);
        }
    }
}