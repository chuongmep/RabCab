using System;
using System.Linq;

namespace RabCab.Utilities.Calculators
{
    public static class UnitConverter
    {

        /// <summary>
        ///     Utility method to convert an angle from radians to degrees
        /// </summary>
        /// <param name="radians">The angle (in radians) to be converted</param>
        /// <returns>Returns the input angle in Radians</returns>
        public static double ConvertToDegrees(double radians)
        {
            return (180 / Math.PI) * radians;
        }

        /// <summary>
        ///     Utility method to convert an angle from degrees to radians
        /// </summary>
        /// <param name="degrees">The angle (in degrees) to be converted</param>
        /// <returns>Returns the input angle in Radians</returns>
        public static double ConvertToRadians(double degrees)
        {
            return (Math.PI / 180) * degrees;
        }

    }
}
