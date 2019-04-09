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
using RabCab.Utilities.Engine.Enumerators;

namespace RabCab.Utilities.Settings
{
    public static class Colors
    {
        #region Layer Colors

        public static Color LayerColorRcAnno = Color.FromColorIndex(ColorMethod.ByAci, (int) Enums.CadColor.Blue);
        public static Color LayerColorRcVisible = Color.FromColorIndex(ColorMethod.ByAci, (int) Enums.CadColor.White);
        public static Color LayerColorRcHidden = Color.FromColorIndex(ColorMethod.ByAci, (int) Enums.CadColor.Green);
        public static Color LayerColorConverge = Color.FromColorIndex(ColorMethod.ByAci, (int) Enums.CadColor.Red);
        public static Color LayerColorDefpoints = Color.FromColorIndex(ColorMethod.ByAci, (int) Enums.CadColor.LightGrey);

        #endregion
    }
}