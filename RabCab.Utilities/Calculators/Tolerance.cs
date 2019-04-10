// -----------------------------------------------------------------------------------
//     <copyright file="Tolerance.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>03/13/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using System;
using Autodesk.AutoCAD.Geometry;
using RabCab.Engine.Enumerators;
using RabCab.Settings;

namespace RabCab.Calculators
{
    public static class Tolerance
    {
        /// <summary>
        ///     Method to round an input based on the given tolerance
        /// </summary>
        /// <param name="input">The number to be rounded</param>
        /// <param name="tolerance">The specified tolerance</param>
        /// <returns></returns>
        public static double RoundToTolerance(double input, Enums.RoundTolerance tolerance)
        {
            //var floorRound = FloorNum(input, (int) tolerance);
            return Math.Round(input, (int) tolerance);
        }

        public static double RoundToTolerance(double input)
        {
            //var floorRound = FloorNum(input, (int) PrefsUser.UserTol);
            return Math.Round(input, (int) SettingsUser.UserTol);
        }

        /// <summary>
        ///     Method to check the comparison of two double to see if they are equal
        /// </summary>
        /// <param name="input1">The first input to compare</param>
        /// <param name="input2">The second input to compare</param>
        /// <returns></returns>
        public static bool CompareTolerance(double input1, double input2)
        {
            //Create a tolerance variable
            float tolerance = 1;

            switch (SettingsUser.UserTol)
            {
                case Enums.RoundTolerance.NoDecimals:
                    tolerance = 1;
                    break;
                case Enums.RoundTolerance.OneDecimal:
                    tolerance = 0.1f;
                    break;
                case Enums.RoundTolerance.TwoDecimals:
                    tolerance = 0.01f;
                    break;
                case Enums.RoundTolerance.ThreeDecimals:
                    tolerance = 0.001f;
                    break;
                case Enums.RoundTolerance.FourDecimals:
                    tolerance = 0.0001f;
                    break;
                case Enums.RoundTolerance.FiveDecimals:
                    tolerance = 0.00001f;
                    break;
                case Enums.RoundTolerance.SixDecimals:
                    tolerance = 0.000001f;
                    break;
                case Enums.RoundTolerance.SevenDecimals:
                    tolerance = 0.0000001f;
                    break;
                case Enums.RoundTolerance.EightDecimals:
                    tolerance = 0.00000001f;
                    break;
                case Enums.RoundTolerance.NineDecimals:
                    tolerance = 0.000000001f;
                    break;
                case Enums.RoundTolerance.TenDecimals:
                    tolerance = 0.0000000001f;
                    break;
            }

            var tol1 = (float) RoundToTolerance(input1, SettingsUser.UserTol);
            var tol2 = (float) RoundToTolerance(input2, SettingsUser.UserTol);

            var tolCheck = Math.Abs(tol1 - tol2);

            //Compare difference of values - return false if a difference is found    
            if (tolCheck > tolerance)
                return false;

            //if not, return true
            return true;
        }

        /// <summary>
        ///     Method for returning a tolerance based on the users current setting
        /// </summary>
        /// <returns></returns>
        public static float ReturnTolerance()
        {
            //Create a tolerance variable
            float tolerance = 1;

            switch (SettingsUser.UserTol)
            {
                case Enums.RoundTolerance.NoDecimals:
                    tolerance = 1;
                    break;
                case Enums.RoundTolerance.OneDecimal:
                    tolerance = 0.1f;
                    break;
                case Enums.RoundTolerance.TwoDecimals:
                    tolerance = 0.01f;
                    break;
                case Enums.RoundTolerance.ThreeDecimals:
                    tolerance = 0.001f;
                    break;
                case Enums.RoundTolerance.FourDecimals:
                    tolerance = 0.0001f;
                    break;
                case Enums.RoundTolerance.FiveDecimals:
                    tolerance = 0.00001f;
                    break;
                case Enums.RoundTolerance.SixDecimals:
                    tolerance = 0.000001f;
                    break;
                case Enums.RoundTolerance.SevenDecimals:
                    tolerance = 0.0000001f;
                    break;
                case Enums.RoundTolerance.EightDecimals:
                    tolerance = 0.00000001f;
                    break;
                case Enums.RoundTolerance.NineDecimals:
                    tolerance = 0.000000001f;
                    break;
                case Enums.RoundTolerance.TenDecimals:
                    tolerance = 0.0000000001f;
                    break;
            }

            return tolerance;
        }

        /// <summary>
        ///     Utility method to return a Point3d with coordinates that adhere to tolerance
        /// </summary>
        /// <param name="inputPt">The point to be normalized</param>
        /// <param name="roundTol">The tolerance to round the vertex points with</param>
        /// <returns></returns>
        public static Point3d GetPointTolerance(Point3d inputPt, Enums.RoundTolerance roundTol)
        {
            return new Point3d(RoundToTolerance(inputPt.X, roundTol), RoundToTolerance(inputPt.Y, roundTol),
                RoundToTolerance(inputPt.Z, roundTol));
        }
    }
}