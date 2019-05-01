using System;
using System.Globalization;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.Runtime;
using RabCab.Entities.Annotation;
using RabCab.Settings;

namespace RabCab.Commands.AnnotationSuite
{
    internal class RcDimSettings
    {
        /// <summary>
        /// </summary>
        [CommandMethod(SettingsInternal.CommandGroup, "_DIMSETTINGS",
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
        public void Cmd_DimSettings()
        {
            var dimSetSettings = DimSystemSettings.GetDimSystemSettings();

            var myDialog = new Gui.DimSystem.DimSystemSettingsGui();

            myDialog.TboxTolerance.Text = dimSetSettings.EqPoint.ToString(CultureInfo.CurrentCulture);
            myDialog.ChbOriginalDimRemoveOverride.Checked =
                Convert.ToBoolean(dimSetSettings.OriginalDimRemoveTextOverride);
            myDialog.CmbOriginalTextPosition.SelectedIndex = dimSetSettings.OriginalDimTextPosition;
            myDialog.ChbNewDimRemoveOverride.Checked = Convert.ToBoolean(dimSetSettings.NewDimRemoveTextOverride);
            myDialog.CmbNewTextPosition.SelectedIndex = dimSetSettings.NewDimTextPosition;
            var color = Color.FromColorIndex(ColorMethod.ByAci, dimSetSettings.DynPreviewColor);
            var button = myDialog.BtnDynColor;
            var r = color.ColorValue.R;
            var g = color.ColorValue.G;
            var colorValue = color.ColorValue;
            button.BackColor = System.Drawing.Color.FromArgb(r, g, colorValue.B);
            Application.ShowModalDialog(null, myDialog, false);
            if (!myDialog.ClickedOk) return;
            dimSetSettings.EqPoint = Convert.ToDouble(myDialog.TboxTolerance.Text);
            var num = myDialog.BtnDynColor.BackColor.R;
            var g1 = myDialog.BtnDynColor.BackColor.G;
            var backColor = myDialog.BtnDynColor.BackColor;
            var color1 = Color.FromRgb(num, g1, backColor.B);
            dimSetSettings.DynPreviewColor = color1.ColorIndex;
            dimSetSettings.OriginalDimRemoveTextOverride =
                Convert.ToInt32(myDialog.ChbOriginalDimRemoveOverride.Checked);
            dimSetSettings.OriginalDimTextPosition = myDialog.CmbOriginalTextPosition.SelectedIndex;
            dimSetSettings.NewDimRemoveTextOverride = Convert.ToInt32(myDialog.ChbNewDimRemoveOverride.Checked);
            dimSetSettings.NewDimTextPosition = myDialog.CmbNewTextPosition.SelectedIndex;
            DimSystemSettings.WriteDimSettings(dimSetSettings);
        }
    }
}