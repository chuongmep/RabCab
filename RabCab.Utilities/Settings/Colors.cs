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

using System.Windows.Controls;
using Autodesk.AutoCAD.Colors;
using RabCab.Engine.Enumerators;
using RabCab.Engine.System;

namespace RabCab.Settings
{
    public static class Colors
    {
        #region Custom Colors

        public static System.Drawing.Color DarkBack = System.Drawing.Color.FromArgb(34, 41, 51);
        public static System.Drawing.Color DarkFore = System.Drawing.Color.FromArgb(59, 68, 83);
        public static System.Drawing.Color DarkText = System.Drawing.Color.AntiqueWhite;

        public static System.Drawing.Color LightBack = System.Drawing.Color.FromArgb(217, 217, 217);
        public static System.Drawing.Color LightFore = System.Drawing.Color.FromArgb(245, 245, 245);
        public static System.Drawing.Color LightText = System.Drawing.Color.Black;

        #endregion

        public static System.Drawing.Color GetCadBackColor() => AcVars.ColorTheme == 0 ? DarkBack : LightBack;
        public static System.Drawing.Color GetCadForeColor() => AcVars.ColorTheme == 0 ? DarkFore : LightFore;
        public static System.Drawing.Color GetCadTextColor() => AcVars.ColorTheme == 0 ? DarkText : LightText;

        #region Layer Colors

        public static Color LayerColorRcAnno = Color.FromColorIndex(ColorMethod.ByAci, (int) Enums.CadColor.Blue);
        public static Color LayerColorRcVisible = Color.FromColorIndex(ColorMethod.ByAci, (int) Enums.CadColor.White);
        public static Color LayerColorRcHidden = Color.FromColorIndex(ColorMethod.ByAci, (int) Enums.CadColor.Green);
        public static Color LayerColorConverge = Color.FromColorIndex(ColorMethod.ByAci, (int) Enums.CadColor.Red);

        public static Color LayerColorDefpoints =
            Color.FromColorIndex(ColorMethod.ByAci, (int) Enums.CadColor.LightGrey);

        #endregion
    }
}