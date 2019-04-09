using System;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;

namespace RabCab.Utilities.Calculators
{
    public static class UnitConverter
    {
        /// <summary>
        ///     Method to convert input units to DWG units
        /// </summary>
        /// <param name="acCurDb">The current working database</param>
        /// <param name="val">The value to be converted</param>
        /// <returns></returns>
        public static string ConvertToDwgUnits(this Database acCurDb, double val)
        {
            return Converter.DistanceToString(val, DistanceUnitFormat.Current, acCurDb.Luprec);
        }

        /// <summary>
        ///     Utility method to convert an angle from radians to degrees
        /// </summary>
        /// <param name="radians">The angle (in radians) to be converted</param>
        /// <returns>Returns the input angle in Radians</returns>
        public static double ConvertToDegrees(double radians)
        {
            return 180/Math.PI*radians;
        }

        /// <summary>
        ///     Utility method to convert an angle from degrees to radians
        /// </summary>
        /// <param name="degrees">The angle (in degrees) to be converted</param>
        /// <returns>Returns the input angle in Radians</returns>
        public static double ConvertToRadians(double degrees)
        {
            return Math.PI/180*degrees;
        }
    }
}