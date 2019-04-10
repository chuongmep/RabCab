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
            get { return (Enums.SubObjEnum) (short) Application.GetSystemVariable("SUBOBJSELECTIONMODE"); }
            set { Application.SetSystemVariable("SUBOBJSELECTIONMODE", (short) value); }
        }

        #endregion

        #region Tile Mode

        /// <summary>
        ///     Specifies whether the tileMode is model or paper space
        /// </summary>
        public static Enums.TileModeEnum TileMode
        {
            get { return (Enums.TileModeEnum) (short) Application.GetSystemVariable("TILEMODE"); }
            set { Application.SetSystemVariable("TILEMODE", (short) value); }
        }

        #endregion
    }
}