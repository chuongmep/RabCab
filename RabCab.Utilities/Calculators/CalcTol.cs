// -----------------------------------------------------------------------------------
//     <copyright file="CalcTol.cs" company="CraterSpace">
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
using RabCab.Engine.System;
using RabCab.Settings;
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace RabCab.Calculators
{
    public static class CalcTol
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

        #region Double Extensions For Determining Tolerance
        public static double EqVector = 0.0017453292519943296;
        private static double EqPoint = 0.0;   

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Point2d Approx(this Point2d point) =>
            new Point2d(point.X.ApproxSize(), point.Y.ApproxSize());

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Point3d Approx(this Point3d point) =>
            new Point3d(point.X.ApproxSize(), point.Y.ApproxSize(), point.Z.ApproxSize());

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector3d Approx(this Vector3d v) =>
            new Vector3d(v.X.ApproxSize(), v.Y.ApproxSize(), v.Z.ApproxSize());

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static bool ApproxZero(this double size) =>
            (Math.Abs(size) < ZeroSize);

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static bool IsZero(this Vector3d v) =>
            ((Math.Abs(v.X) < ZeroSize) && ((Math.Abs(v.Y) < ZeroSize) && (Math.Abs(v.Z) < ZeroSize)));

        /// <summary>
        /// TODO
        /// </summary>
        public static Tolerance Liner =>
            new Tolerance(EqVector, EqPoint);

        /// <summary>
        /// TODO
        /// </summary>
        public static Tolerance UnitVector =>
            new Tolerance(EqVector, ZeroSize);

        /// <summary>
        /// TODO
        /// </summary>
        public static double ZeroSize =>
            1E-07;

        /// <summary>
        /// TODO
        /// </summary>
        public static double ZeroArea =>
            Math.Pow(ZeroSize, 2.0);

        /// <summary>
        /// TODO
        /// </summary>
        public static double ZeroVolume =>
            Math.Pow(ZeroSize, 3.0);

        /// <summary>
        /// TODO
        /// </summary>
        private static double DefEqPoint =>
            (AcVars.IsAppInch ? 0.004 : 0.1);

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double ApproxArea(this double x)
        {
            if ((double.IsNaN(x) || double.IsInfinity(x)) || (x == 0.0))
            {
                return x;
            }
            return ((Math.Abs(x) >= ZeroArea) ? x.Approx((byte) SettingsUser.UserTol) : 0.0);
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool ApproxEqArea(this double x, double y)
        {
            if ((double.IsNaN(x) || (double.IsNaN(y) || double.IsInfinity(x))) || double.IsInfinity(y))
            {
                return (x == y);
            }
            return ((Math.Abs(x - y) >= ZeroArea) ? x.ApproxEq(y, (byte) SettingsUser.UserTol) : true);
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool ApproxEqSize(this double x, double y)
        {
            if ((double.IsNaN(x) || (double.IsNaN(y) || double.IsInfinity(x))) || double.IsInfinity(y))
            {
                return (x == y);
            }
            return (!(x - y).ApproxZero() ? x.ApproxEq(y, (byte) SettingsUser.UserTol) : true);
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool ApproxEqVol(this double x, double y)
        {
            if ((double.IsNaN(x) || (double.IsNaN(y) || double.IsInfinity(x))) || double.IsInfinity(y))
            {
                return (x == y);
            }
            return ((!(Math.Abs(x - y) >= ZeroVolume)) || x.ApproxEq(y, (byte) SettingsUser.UserTol));
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="x"></param>
        /// <param name="tol"></param>
        /// <returns></returns>
        public static double ApproxLike(this double x, Tolerance tol)
        {
            if ((double.IsNaN(x) || double.IsInfinity(x)) || (x == 0.0))
            {
                return x;
            }
            if (x.ApproxZero())
            {
                return 0.0;
            }
            byte digits = (byte)Math.Ceiling(-Math.Log10(tol.EqualPoint));
            return Math.Round(x, digits);
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double ApproxSize(this double x)
        {
            if ((double.IsNaN(x) || double.IsInfinity(x)) || (x == 0.0))
            {
                return x;
            }
            return (!x.ApproxZero() ? x.Approx((byte) SettingsUser.UserTol) : 0.0);
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double ApproxVol(this double x)
        {
            if ((double.IsNaN(x) || double.IsInfinity(x)) || (x == 0.0))
            {
                return x;
            }
            return ((Math.Abs(x) >= ZeroVolume) ? x.Approx((byte) SettingsUser.UserTol) : 0.0);
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="d"></param>
        /// <param name="digs"></param>
        /// <returns></returns>
        public static double Approx(this double d, byte digs)
        {
            if ((double.IsNaN(d) || double.IsInfinity(d)) || (d == 0.0))
            {
                return d;
            }
            if ((digs == 0) || (digs > 0x10))
            {
                throw new ArgumentOutOfRangeException();
            }
            double num1 = Math.Log10(Math.Abs(d));
            int num = (int)Math.Floor(num1);
            if (((int)Math.Ceiling(num1)) == digs)
            {
                return Math.Round(d);
            }
            double num2 = Math.Pow(10.0, num);
            double num3 = Math.Pow(10.0, digs - 1);
            return ((Math.Round((d / num2) * num3) / num3) * num2);
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="digs"></param>
        /// <returns></returns>
        public static bool ApproxEq(this double from, double to, byte digs)
        {
            if ((double.IsNaN(from) || (double.IsNaN(to) || double.IsInfinity(from))) || double.IsInfinity(to))
            {
                return (from == to);
            }
            if ((from == 0.0) && (to == 0.0))
            {
                return true;
            }
            if ((digs == 0) || (digs > 0x10))
            {
                throw new ArgumentOutOfRangeException();
            }
            int num = -1;
            if ((from != 0.0) && (to != 0.0))
            {
                num = (int)Math.Floor(Math.Log10(Math.Abs(from)));
            }
            return (Math.Abs(@from - to) < Math.Pow(10.0, (num + 1) - digs));
        }
        #endregion
    }
}