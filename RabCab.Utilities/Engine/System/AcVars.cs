// -----------------------------------------------------------------------------------
//     <copyright file="AcVars.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>04/08/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using System;
using Autodesk.AutoCAD.ApplicationServices.Core;
using RabCab.Engine.Enumerators;

// ReSharper disable StringLiteralTypo

namespace RabCab.Engine.System
{
    internal static class AcVars
    {
        #region Sub Object Selection Mode

        /// <summary>
        ///     Filters whether faces, edges, vertices or solid history sub-objects are highlighted when you roll over them.
        /// </summary>
        public static Enums.SubObjEnum SubObjSelMode
        {
            get => (Enums.SubObjEnum) (short) Application.GetSystemVariable("SUBOBJSELECTIONMODE");
            set => Application.SetSystemVariable("SUBOBJSELECTIONMODE", (short) value);
        }

        #endregion

        #region GRIDMODE

        /// <summary>
        ///     Specifies whether the grid is turned on or off.
        /// </summary>
        public static Enums.GridMode GridMode
        {
            get => (Enums.GridMode) (short) Application.GetSystemVariable("GRIDMODE");
            set => Application.SetSystemVariable("GRIDMODE", (short) value);
        }

        #endregion

        #region OrthoMode

        /// <summary>
        ///     Specifies whether the grid is turned on or off.
        /// </summary>
        public static Enums.OrthoMode OrthoMode
        {
            get => (Enums.OrthoMode) (short) Application.GetSystemVariable("ORTHOMODE");
            set => Application.SetSystemVariable("ORTHOMODE", (short) value);
        }

        #endregion

        #region Tile Mode

        /// <summary>
        ///     Specifies whether the tileMode is model or paper space
        /// </summary>
        public static Enums.TileModeEnum TileMode
        {
            get => (Enums.TileModeEnum) (short) Application.GetSystemVariable("TILEMODE");
            set => Application.SetSystemVariable("TILEMODE", (short) value);
        }

        #endregion

        /// <summary>
        ///     Controls the behavior of the Action Recorder panel when recording and playing back macros.
        /// </summary>
        public static Enums.ColorTheme ColorTheme
        {
            get => (Enums.ColorTheme) (short) Application.GetSystemVariable("COLORTHEME");
            set => Application.SetSystemVariable("COLORTHEME", (short) value);
        }

        #region Unit Checker

        /// <summary>
        ///     Checks if current app units are in inches
        ///     ///TODO USE GET FOR THESE
        /// </summary>
        public static bool IsAppInch
        {
            get
            {
                try
                {
                    int systemVariable = (short) Application.GetSystemVariable("INSUNITS");
                    if (systemVariable == 0)
                        return Convert.ToInt16(Application.GetSystemVariable("LUNITS")) > 2 ||
                               (short) Application.GetSystemVariable("INSUNITSDEFTARGET") == 1;
                    return systemVariable == 1;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        ///     Checks if current app units are in millimeters
        ///     ///TODO USE GET FOR THESE
        /// </summary>
        public static bool IsAppMm
        {
            get
            {
                try
                {
                    int systemVariable = (short) Application.GetSystemVariable("INSUNITS");
                    if (systemVariable == 0)
                        return Convert.ToInt16(Application.GetSystemVariable("LUNITS")) <= 2 &&
                               (short) Application.GetSystemVariable("INSUNITSDEFTARGET") == 4;
                    return systemVariable == 4;
                }
                catch
                {
                    return false;
                }
            }
        }

        #endregion
    }
}