// -----------------------------------------------------------------------------------
//     <copyright file="EntReader.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>03/08/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using System;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using RabCab.Calculators;
using RabCab.Engine.Enumerators;
using RabCab.Settings;

namespace RabCab.Analysis
{
    internal class EntMetric
    {
        #region ObjectProperties

        //Object Information
        public ObjectId ObjId;
        public int FaceCount;
        public int NumberOfChanges;

        //Sizing
        public double Length;
        public double Width;
        public double Height;
        public double Thickness;
        public double Volume;
        public double Box;

        public double MaxArea;
        public double SubArea;
        public double MaxPerimeter;
        public double SubPerimeter;

        //Asymmetry
        public double Asymmetry;
        public Vector3d AsymmetryVector;

        //Ent Information
        public string EntLayer;
        public string EntColor;
        public string EntMaterial;

        //XData Information
        public string RcName;
        public bool IsSweep;
        public bool IsMirror;
        public string RcInfo;
        public string RcQty;
        public string RcQtyInSelection;
        public bool HasNonFlatFaces;
        public bool HasHoles;
        public bool Has3DFaces;
        public Enums.TextureDirection TxDirection;

        public Enums.ProductionType ProdType
        {
            get
            {
                if (IsBox) return Enums.ProductionType.Box;
                if (IsSweep) return Enums.ProductionType.Sweep;
                if (Has3DFaces) return Enums.ProductionType.MillingManySide;
                if (HasHoles || HasNonFlatFaces || FaceCount > 6) return Enums.ProductionType.MillingOneSide;

                return Enums.ProductionType.S4S;
            }
        }

        /// <summary>
        ///     Method for setting EntReader to Null
        /// </summary>
        public bool IsNull =>
            Width == 0.0 && Height == 0.0 && Thickness == 0.0;

        /// <summary>
        ///     Property for determining if Entity is Box
        /// </summary>
        public bool IsBox =>
            Volume.ApproxEqVol(Box);

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool MirrorOf(EntMetric y)
        {
            return Asymmetry != 0.0 && y.Asymmetry != 0.0 && !AsymmetryVector.IsEqualTo(y.AsymmetryVector,
                       CalcTol.UnitVector) && (!AsymmetryVector.X.ApproxEq(y.AsymmetryVector.X,
                                                   (byte) SettingsUser.UserTol) || !AsymmetryVector.Y.ApproxEq(
                                                   y.AsymmetryVector.Y,
                                                   (byte) SettingsUser.UserTol) || !AsymmetryVector.Z.ApproxEq(
                                                   y.AsymmetryVector.Z,
                                                   (byte) SettingsUser.UserTol));
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool EqualSize(EntMetric other)
        {
            return Height.ApproxEqSize(other.Height) && Width.ApproxEqSize(other.Width) &&
                   Thickness.ApproxEqSize(other.Thickness) && Volume.ApproxEqVol(other.Volume) &&
                   Asymmetry.ApproxEqSize(other.Asymmetry);
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(EntMetric other)
        {
            if (!EqualSize(other)) return false;
            if (Asymmetry != 0.0 || other.Asymmetry != 0.0)
                return Asymmetry.ApproxEqSize(other.Asymmetry) && AsymmetryVector.IsEqualTo(other.AsymmetryVector,
                           CalcTol.UnitVector) && Math.Sign(AsymmetryVector.X) == Math.Sign(other.AsymmetryVector.X) &&
                       Math.Sign(AsymmetryVector.Y) == Math.Sign(other.AsymmetryVector.Y) &&
                       Math.Sign(AsymmetryVector.Z) == Math.Sign(other.AsymmetryVector.Z);
            return true;
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(EntMetric a, EntMetric b)
        {
            return a != null && a.Equals(b);
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(EntMetric a, EntMetric b)
        {
            return a != null && !a.Equals(b);
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static string AsymVStr(Vector3d v)
        {
            if (v.IsZero()) return "";
            var str = "";
            str = !v.X.ApproxZero() ? v.X <= 0.0 ? "<" : ">" : str + "_";
            str = !v.Y.ApproxZero() ? v.Y <= 0.0 ? str + "<" : str + ">" : str + "_";
            return !v.Z.ApproxZero() ? v.Z <= 0.0 ? str + "<" : str + ">" : str + "_";
        }

        #endregion

        #region Overrides

        /// <summary>
        ///     TODO
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Height.GetHashCode() + Width.GetHashCode() + Thickness.GetHashCode() +
                   Volume.GetHashCode() + Asymmetry.GetHashCode() +
                   (Asymmetry == 0.0 ? 0 : AsymmetryVector.GetHashCode());
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var other = obj as EntMetric;
            return other != null && Equals(other);
        }

        #endregion
    }
}