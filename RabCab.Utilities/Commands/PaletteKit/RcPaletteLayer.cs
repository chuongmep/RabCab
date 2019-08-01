using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Internal;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using RabCab.Agents;
using RabCab.Engine.AcSystem;
using RabCab.Engine.Enumerators;
using RabCab.Entities.Controls;
using RabCab.Extensions;
using RabCab.Settings;
using static System.Drawing.ContentAlignment;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Color = Autodesk.AutoCAD.Colors.Color;
using Exception = Autodesk.AutoCAD.BoundaryRepresentation.Exception;
using Image = System.Drawing.Image;

namespace RabCab.Commands.PaletteKit
{
    internal class RcPaletteLayer
    {
        private readonly string _palName = "Layers";
        private readonly Size _squareSize = new Size(18, 18);
        private UserControl _palPanel;
        private PaletteSet _rcPal;

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
            CreatePal();
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="acColor"></param>
        /// <returns></returns>
        private Image GetLayerImage(Color acColor)
        {
            var brush = new SolidBrush(AcadColorAciToDrawingColor(acColor));
            var outlineColor = System.Drawing.Color.Black;

            if (AcVars.ColorTheme == Enums.ColorTheme.Dark) outlineColor = System.Drawing.Color.White;

            var squareImage = new Bitmap(_squareSize.Width, _squareSize.Height);
            using (var graphics = Graphics.FromImage(squareImage))
            {
                var pen = new Pen(outlineColor, 1) {Alignment = PenAlignment.Center};
                graphics.FillRectangle(brush, 3, 3, _squareSize.Width, _squareSize.Height);
                graphics.DrawRectangle(pen, 3, 3, _squareSize.Width - 4, _squareSize.Height - 4);
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
            }

            return squareImage;
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="acColor"></param>
        /// <returns></returns>
        private Image GetLayerImage(System.Drawing.Color acColor)
        {
            var brush = new SolidBrush(acColor);
            var outlineColor = System.Drawing.Color.Black;

            if (AcVars.ColorTheme == Enums.ColorTheme.Dark) outlineColor = System.Drawing.Color.White;

            var squareImage = new Bitmap(_squareSize.Width, _squareSize.Height);
            using (var graphics = Graphics.FromImage(squareImage))
            {
                var pen = new Pen(outlineColor, 1) {Alignment = PenAlignment.Center};
                graphics.FillRectangle(brush, 3, 3, _squareSize.Width, _squareSize.Height);
                graphics.DrawRectangle(pen, 3, 3, _squareSize.Width - 4, _squareSize.Height - 4);
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
            }

            return squareImage;
        }

        /// <summary>
        ///     TODO
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

        #region Pal Initialization

        /// <summary>
        ///     TODO
        /// </summary>
        private void CreatePal()
        {
            if (!Agents.LicensingAgent.Check()) return;
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
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            if (acCurDoc == null) return;

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
                    _palPanel.SuspendLayout();
                    _palPanel.BackColor = backColor;
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
                        Dock = DockStyle.Fill
                    };

                    palLayout.MouseEnter += (s, e) => palLayout.Focus();

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

                                spButton.Click += Button_Click;
                                spButton.FlatAppearance.BorderColor = Colors.GetCadBorderColor();
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
                                        ContextItem_Click)
                                    {
                                        ImageAlign = TopLeft,
                                        TextAlign = BottomLeft,
                                        BackColor = foreColor,
                                        ForeColor = textColor
                                    };

                                    spButton.ContextMenuStrip.Items.Add(tsButton);
                                }

                                var picBox = new PictureBox
                                {
                                    Height = buttonHeight,
                                    Image = GetLayerImage(spColor),
                                    Dock = DockStyle.Fill
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

                                button.Click += Button_Click;
                                button.FlatAppearance.BorderColor = Colors.GetCadBorderColor();
                                button.FlatAppearance.BorderSize = 1;

                                var picBox = new PictureBox
                                {
                                    Height = buttonHeight,
                                    Image = GetLayerImage(layer.Color),
                                    Dock = DockStyle.Fill
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

                    var bLayout = new TableLayoutPanel
                    {
                        BackColor = Colors.GetCadBackColor(),
                        ForeColor = Colors.GetCadForeColor(),
                        ColumnCount = 2,
                        Height = 30,
                        Dock = DockStyle.Bottom
                    };

                    bLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 80F));
                    bLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
                    bLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f));


                    var loadButton = new Button();
                    loadButton.Click += Load_Click;
                    loadButton.Text = "Load From Dwg";
                    loadButton.BackColor = foreColor;
                    loadButton.ForeColor = textColor;
                    loadButton.Dock = DockStyle.Fill;
                    loadButton.Height = 30;
                    loadButton.FlatStyle = FlatStyle.Flat;
                    loadButton.FlatAppearance.BorderColor = Colors.GetCadBorderColor();
                    loadButton.FlatAppearance.BorderSize = 1;

                    var updButton = new Button();
                    updButton.Click += Update_Click;
                    updButton.Text = "Update";
                    updButton.BackColor = foreColor;
                    updButton.ForeColor = textColor;
                    updButton.Dock = DockStyle.Fill;
                    loadButton.Height = 30;
                    updButton.FlatStyle = FlatStyle.Flat;
                    updButton.FlatAppearance.BorderColor = Colors.GetCadBorderColor();
                    updButton.FlatAppearance.BorderSize = 1;

                    bLayout.Controls.Add(loadButton, 0, 0);
                    bLayout.Controls.Add(updButton, 1, 0);

                    _palPanel.Controls.Add(palLayout);
                    _palPanel.Controls.Add(bLayout);

                    _palPanel.ResumeLayout();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContextItem_Click(object sender, EventArgs e)
        {
            try
            {
                var item = (ToolStripItem) sender;
                if (item == null) return;

                if (!(item.Owner is ContextMenuStrip owner)) return;
                if (!(owner.SourceControl is Button spButton)) return;
                if (!(spButton.Parent is TableLayoutPanel tLayout)) return;

                spButton.Text = item.Text;

                var bPos = tLayout.GetCellPosition(spButton);
                var image = item.Image;

                var b = new Bitmap(image);
                var layColor = b.GetPixel(image.Width / 2, image.Height / 2);
                b.Dispose();

                var newPicBox = new PictureBox
                {
                    Height = image.Height,
                    Image = GetLayerImage(layColor),
                    Dock = DockStyle.Fill
                };

                var curPicBox = tLayout.GetControlFromPosition(bPos.Column - 1, bPos.Row);
                tLayout.Controls.Remove(curPicBox);
                tLayout.Controls.Add(newPicBox, bPos.Column - 1, bPos.Row);
                tLayout.Refresh();

                SetLayer(spButton.Text, Color.FromRgb(layColor.R, layColor.G, layColor.B));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, EventArgs e)
        {
            try
            {
                var item = (Button) sender;
                if (item == null) return;

                if (!(item.Parent is TableLayoutPanel tLayout)) return;
                var bPos = tLayout.GetCellPosition(item);
                var picBox = tLayout.GetControlFromPosition(bPos.Column - 1, bPos.Row);

                if (picBox == null) return;

                var b = new Bitmap(picBox.ClientSize.Width, picBox.Height);
                picBox.DrawToBitmap(b, picBox.ClientRectangle);
                var layColor = b.GetPixel(_squareSize.Width / 2, _squareSize.Height / 2);
                b.Dispose();

                SetLayer(item.Text, Color.FromRgb(layColor.R, layColor.G, layColor.B));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Update_Click(object sender, EventArgs e)
        {
            PopulatePal();
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Load_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Agents.LicensingAgent.Check()) return;
                var acCurDoc = Application.DocumentManager.MdiActiveDocument;
                if (acCurDoc == null) return;

                var acCurDb = acCurDoc.Database;
                var acCurEd = acCurDoc.Editor;

                using (acCurDoc.LockDocument())
                {
                    using (var extDb = acCurEd.GetExternalDatabase())
                    {
                        if (extDb == null) return;

                        using (var acTrans = acCurDb.TransactionManager.StartTransaction())
                        {
                            acCurDb.CopyAllLayers(acCurEd, extDb, acTrans);
                            acTrans.Commit();
                        }

                        PopulatePal();
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="layerName"></param>
        /// <param name="acColor"></param>
        private void SetLayer(string layerName, Color acColor)
        {
            try
            {
                if (!Agents.LicensingAgent.Check()) return;
                var acCurDoc = Application.DocumentManager.MdiActiveDocument;
                if (acCurDoc == null) return;

                var acCurDb = acCurDoc.Database;
                var acCurEd = acCurDoc.Editor;

                using (acCurDoc.LockDocument())
                {
                    Utils.SetFocusToDwgView();
                    var objIds = acCurEd.GetAllSelection(false);

                    using (var acTrans = acCurDb.TransactionManager.StartTransaction())
                    {
                        acCurDb.AddLayer(layerName, acColor, SettingsUser.RcVisibleLT, acTrans);

                        foreach (var obj in objIds)
                        {
                            var acEnt = acTrans.GetObject(obj, OpenMode.ForWrite) as Entity;
                            if (acEnt == null) continue;

                            acEnt.Layer = layerName;
                            acEnt.Color = Color.FromColorIndex(ColorMethod.ByLayer, 256);
                        }

                        acTrans.Commit();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion
    }
}