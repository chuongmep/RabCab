using Autodesk.AutoCAD.ApplicationServices.Core;
using RabCab.Utilities.Engine.Enumerators;
// ReSharper disable StringLiteralTypo

namespace RabCab.Utilities.Engine.System
{
    internal static class AcVars
    {
        #region Sub Object Selection Mode

        /// <summary>
        ///     Filters whether faces, edges, vertices or solid history sub-objects are highlighted when you roll over them.
        /// </summary>
        public static Enums.SubObjEnum SubObjSelMode
        {
            get { return (Enums.SubObjEnum)(short)Application.GetSystemVariable("SUBOBJSELECTIONMODE"); }
            set { Application.SetSystemVariable("SUBOBJSELECTIONMODE", (short)value); }
        }

        #endregion

        #region Tile Mode

        /// <summary>
        ///     Specifies whether the tileMode is model or paper space
        /// </summary>
        public static Enums.TileModeEnum TileMode
        {
            get { return (Enums.TileModeEnum)(short)Application.GetSystemVariable("TILEMODE"); }
            set { Application.SetSystemVariable("TILEMODE", (short)value); }
        }

        #endregion

    }
}
