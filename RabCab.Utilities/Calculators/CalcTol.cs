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
using RabCab.Settings;

// ReSharper disable CompareOfFloatsByEqualityOperator

namespace RabCab.Calculators
{
    public static class CalcTol
    {
        /// <summary>
        ///     Method for returning a tolerance based on the users current setting
        /// </summary>
        /// <returns></returns>
        public static float ReturnCurrentTolerance()
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
            }

            return tolerance;
        }

        #region Extensions For Determining Tolerance

        private static byte DecimalPlace => (byte) SettingsUser.UserTol;
        private static double CurrentTolerance => ReturnCurrentTolerance();
        public static double TolSquare => Math.Pow(CurrentTolerance, 2.0);
        public static double TolCube => Math.Pow(CurrentTolerance, 3.0);

        public static Tolerance CadTolerance => new Tolerance(SettingsInternal.TolVector, SettingsUser.TolPoint);
        public static Tolerance UnitVector => new Tolerance(SettingsInternal.TolVector, CurrentTolerance);

        #region Double Extensions

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double RoundToTolerance(this double x)
        {
            try
            {
                return x.IsLessThanTol() ? 0.0 : Math.Round(x, DecimalPlace);
            }
            catch (Exception)
            {
                return x;
            }
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static bool IsLessThanTol(this double size)
        {
            try
            {
                return Math.Abs(size) < CurrentTolerance;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static bool IsGreaterThanTol(this double size)
        {
            try
            {
                return Math.Abs(size) > CurrentTolerance;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double RoundArea(this double x)
        {
            try
            {
                return Math.Abs(x) >= TolSquare ? x.RoundToTolerance() : 0.0;
            }
            catch (Exception)
            {
                return x;
            }
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double RoundVolume(this double x)
        {
            try
            {
                return Math.Abs(x) >= TolCube ? x.RoundToTolerance() : 0.0;
            }
            catch (Exception)
            {
                return x;
            }
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static bool IsEqualTo(this double from, double to)
        {
            try
            {
                return Math.Abs(from - to).IsLessThanTol();
            }
            catch (Exception)
            {
                return from == to;
            }
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool IsEqualSize(this double x, double y)
        {
            try
            {
                if (!(x - y).IsLessThanTol())
                    return x.IsEqualTo(y);
                return true;
            }
            catch (Exception)
            {
                return x == y;
            }
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool IsEqualVolume(this double x, double y)
        {
            try
            {
                return !(Math.Abs(x - y) >= TolSquare) || x.IsEqualTo(y);
            }
            catch (Exception)
            {
                return x == y;
            }
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool IsEqualArea(this double x, double y)
        {
            try
            {
                return !(Math.Abs(x - y) >= TolCube) || x.IsEqualTo(y);
            }
            catch (Exception)
            {
                return x == y;
            }
        }

        #endregion

        #region Point2D Extensions

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public static Point2d RoundToTolerance(this Point2d pt)
        {
            return new Point2d(pt.X.RoundToTolerance(), pt.Y.RoundToTolerance());
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public static bool IsLessThanTol(this Point2d pt)
        {
            return pt.X.IsLessThanTol() && pt.Y.IsLessThanTol();
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static bool IsEqualTo(this Point2d from, Point2d to)
        {
            try
            {
                return Math.Abs(from.GetDistanceTo(to)).IsLessThanTol();
            }
            catch (Exception)
            {
                return from == to;
            }
        }

        #endregion

        #region Point3D Extensions

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public static Point3d RoundToTolerance(this Point3d pt)
        {
            return new Point3d(pt.X.RoundToTolerance(), pt.Y.RoundToTolerance(), pt.Z.RoundToTolerance());
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public static bool IsLessThanTol(this Point3d pt)
        {
            return pt.X.IsLessThanTol() && pt.Y.IsLessThanTol() && pt.Z.IsLessThanTol();
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static bool IsEqualTo(this Point3d from, Point3d to)
        {
            try
            {
                return Math.Abs(from.DistanceTo(to)).IsLessThanTol();
            }
            catch (Exception)
            {
                return from == to;
            }
        }

        #endregion

        #region Vector2D Extensions

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        public static Vector2d RoundToTolerance(this Vector2d vec)
        {
            return new Vector2d(vec.X.RoundToTolerance(), vec.Y.RoundToTolerance());
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        public static bool IsLessThanTol(this Vector2d vec)
        {
            return vec.X.IsLessThanTol() && vec.Y.IsLessThanTol();
        }

        #endregion

        #region Vector3D Extensions

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        public static Vector3d RoundToTolerance(this Vector3d vec)
        {
            return new Vector3d(vec.X.RoundToTolerance(), vec.Y.RoundToTolerance(), vec.Z.RoundToTolerance());
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        public static bool IsLessThanTol(this Vector3d vec)
        {
            return vec.X.IsLessThanTol() && vec.Y.IsLessThanTol() && vec.Z.IsLessThanTol();
        }

        #endregion

        #endregion
    }
}