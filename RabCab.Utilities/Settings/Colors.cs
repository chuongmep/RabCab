// -----------------------------------------------------------------------------------
//     <copyright file="Colors.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>04/08/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using Autodesk.AutoCAD.Colors;
using RabCab.Engine.Enumerators;
using RabCab.Engine.System;
using Color = System.Drawing.Color;

namespace RabCab.Settings
{
    public static class Colors
    {
        public static Color GetCadBackColor()
        {
            return AcVars.ColorTheme == 0 ? DarkBack : LightBack;
        }

        public static Color GetCadForeColor()
        {
            return AcVars.ColorTheme == 0 ? DarkFore : LightFore;
        }

        public static Color GetCadEntryColor()
        {
            return AcVars.ColorTheme == 0 ? DarkEntry : LightEntry;
        }

        public static Color GetCadTextColor()
        {
            return AcVars.ColorTheme == 0 ? DarkText : LightText;
        }

        public static Color GetCadBorderColor()
        {
            return AcVars.ColorTheme == 0 ? DarkBorder : LightBorder;
        }

        #region Custom Colors

        public static Color DarkBorder = Color.Black;
        public static Color LightBorder = Color.DarkGray;

        public static Color Focus = Color.FromArgb(6, 150, 215);

        public static Color DarkBack = Color.FromArgb(34, 41, 51);
        public static Color DarkFore = Color.FromArgb(59, 68, 83);
        public static Color DarkEntry = Color.FromArgb(78, 90, 110);
        public static Color DarkText = Color.AntiqueWhite;

        public static Color LightBack = Color.FromArgb(217, 217, 217);
        public static Color LightFore = Color.FromArgb(245, 245, 245);
        public static Color LightEntry = Color.FromArgb(255, 255, 255);
        public static Color LightText = Color.Black;

        #endregion

        #region Layer Colors

        public static Autodesk.AutoCAD.Colors.Color LayerColorRcAnno =
            Autodesk.AutoCAD.Colors.Color.FromColorIndex(ColorMethod.ByAci, (int) Enums.CadColor.Blue);

        public static Autodesk.AutoCAD.Colors.Color LayerColorRcVisible =
            Autodesk.AutoCAD.Colors.Color.FromColorIndex(ColorMethod.ByAci, (int) Enums.CadColor.White);

        public static Autodesk.AutoCAD.Colors.Color LayerColorRcHidden =
            Autodesk.AutoCAD.Colors.Color.FromColorIndex(ColorMethod.ByAci, (int) Enums.CadColor.Green);

        public static Autodesk.AutoCAD.Colors.Color LayerColorConverge =
            Autodesk.AutoCAD.Colors.Color.FromColorIndex(ColorMethod.ByAci, (int) Enums.CadColor.Red);

        public static Autodesk.AutoCAD.Colors.Color LayerColorDefpoints =
            Autodesk.AutoCAD.Colors.Color.FromColorIndex(ColorMethod.ByAci, (int) Enums.CadColor.LightGrey);

        public static Autodesk.AutoCAD.Colors.Color LayerColorBounds =
            Autodesk.AutoCAD.Colors.Color.FromColorIndex(ColorMethod.ByAci, (int) Enums.CadColor.BoundsGreen);

        public static Autodesk.AutoCAD.Colors.Color LayerColorPreview =
            Autodesk.AutoCAD.Colors.Color.FromColorIndex(ColorMethod.ByAci, (int) Enums.CadColor.Magenta);

        public static Autodesk.AutoCAD.Colors.Color LayerColorHoles =
            Autodesk.AutoCAD.Colors.Color.FromColorIndex(ColorMethod.ByAci, (int)Enums.CadColor.Red);

        #endregion
    }
}