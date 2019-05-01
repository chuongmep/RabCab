using System;
using System.Globalization;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.Runtime;
using RabCab.Calculators;
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
            var myDialog = new Gui.DimSystem.DimSystemSettingsGui();

            myDialog.TboxTolerance.Text = CalcTol.ReturnCurrentTolerance().ToString();
            var color = SettingsUser.DynPreviewColor;
            var button = myDialog.BtnDynColor;
            var r = color.ColorValue.R;
            var g = color.ColorValue.G;
            var colorValue = color.ColorValue;
            button.BackColor = System.Drawing.Color.FromArgb(r, g, colorValue.B);
            Application.ShowModalDialog(null, myDialog, false);
            if (!myDialog.ClickedOk) return;
            var num = myDialog.BtnDynColor.BackColor.R;
            var g1 = myDialog.BtnDynColor.BackColor.G;
            var backColor = myDialog.BtnDynColor.BackColor;
            var color1 = Color.FromRgb(num, g1, backColor.B);
            SettingsUser.DynPreviewColor = color1;


        }
    }
}