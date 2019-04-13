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
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using Autodesk.AutoCAD.BoundaryRepresentation;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using RabCab.Calculators;
using RabCab.Extensions;
using static RabCab.Engine.Enumerators.Enums;
using Exception = System.Exception;

namespace RabCab.Analysis
{
    internal class EntInfo
    {
        #region ObjectProperties

        //Object Information
        public ObjectId ObjId;
        public int FaceCount;
        public int NumberOfChanges;

        //Subentity Information
        public SubentityId SubId;
        public double SubArea;
        public double SubPerimeter;

        //Extents
        public Extents3d Extents;
        public Point3d MinExt;
        public Point3d MaxExt;
        public Point3d Centroid;

        //Sizing
        public double Length;
        public double Width;
        public double Height;
        public double Thickness;
        public double Volume;
        public double Box;

        public double MaxArea;
        public double MaxPerimeter;


        //CNC Information
        public Matrix3d LayMatrix;

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
        public TextureDirection TxDirection;

        public ProductionType ProdType
        {
            get
            {
                if (IsBox) return ProductionType.Box;
                if (IsSweep) return ProductionType.Sweep;
                if (Has3DFaces) return ProductionType.MillingManySide;
                if (HasHoles || HasNonFlatFaces || FaceCount > 6) return ProductionType.MillingOneSide;

                return ProductionType.S4S;
            }
        }

        public EntInfo(Solid3d acSol)
        {
            ObjId = acSol.ObjectId;
            LayMatrix = Matrix3d.Identity;
            GetMeasurements(acSol);
        }

        public EntInfo(Solid3d acSol, SubentityId subId)
        {
            ObjId = acSol.ObjectId;

        }

        #endregion

        #region Comparison Methods
        /// <summary>
        ///     Method for setting EntReader to Null
        /// </summary>
        public bool IsNull =>
            Width == 0.0 && Height == 0.0 && Thickness == 0.0;

        /// <summary>
        ///     Property for determining if Entity is Box
        /// </summary>
        public bool IsBox =>
            Volume.IsEqualVolume(Box);

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="mirCheck"></param>
        /// <returns></returns>
        public bool IsMirrorOf(EntInfo mirCheck)
        {
            return Asymmetry != 0.0 && mirCheck.Asymmetry != 0.0 && !AsymmetryVector.IsEqualTo(mirCheck.AsymmetryVector,
                       CalcTol.UnitVector) && (!AsymmetryVector.X.IsEqualTo(mirCheck.AsymmetryVector.X) 
                                               || !AsymmetryVector.Y.IsEqualTo(mirCheck.AsymmetryVector.Y)
                                               || !AsymmetryVector.Z.IsEqualTo(mirCheck.AsymmetryVector.Z));
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="compEnt"></param>
        /// <returns></returns>
        public bool IsEqualSize(EntInfo compEnt)
        {
            return Height.IsEqualSize(compEnt.Height) && Width.IsEqualSize(compEnt.Width) &&
                   Thickness.IsEqualSize(compEnt.Thickness) && Volume.IsEqualVolume(compEnt.Volume) &&
                   Asymmetry.IsEqualSize(compEnt.Asymmetry);
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="compEnt"></param>
        /// <returns></returns>
        public bool Equals(EntInfo compEnt)
        {
            if (!IsEqualSize(compEnt)) return false;
            if (Asymmetry != 0.0 || compEnt.Asymmetry != 0.0)
                return Asymmetry.IsEqualSize(compEnt.Asymmetry) 
                       && AsymmetryVector.IsEqualTo(compEnt.AsymmetryVector, CalcTol.UnitVector) 
                       && Math.Sign(AsymmetryVector.X) == Math.Sign(compEnt.AsymmetryVector.X) 
                       && Math.Sign(AsymmetryVector.Y) == Math.Sign(compEnt.AsymmetryVector.Y) 
                       && Math.Sign(AsymmetryVector.Z) == Math.Sign(compEnt.AsymmetryVector.Z);
            return true;
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator == (EntInfo a, EntInfo b)
        {
            return a != null && a.Equals(b);
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator != (EntInfo a, EntInfo b)
        {
            return a != null && !a.Equals(b);
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="vect"></param>
        /// <returns></returns>
        public static string AsymVStr(Vector3d vect)
        {
            if (vect.IsLessThanTol()) return "";
            var asymStr = "";
            asymStr = !vect.X.IsLessThanTol() ? vect.X <= 0.0 ? "-" : "+" : asymStr + "0";
            asymStr = !vect.Y.IsLessThanTol() ? vect.Y <= 0.0 ? asymStr + "-" : asymStr + "+" : asymStr + "0";
            return !vect.Z.IsLessThanTol() ? vect.Z <= 0.0 ? asymStr + "-" : asymStr + "+" : asymStr + "0";
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
            var other = obj as EntInfo;
            return other != null && Equals(other);
        }

        public override string ToString()
        {
            return ObjId.ToString();
        }

        #endregion

        #region Parsing Methods

        private void GetMeasurements(Solid3d acSol)
        {
            GetLayMatrix(acSol);
            
           //Get Volume & Extents
           Extents = acSol.GetBounds();
           MinExt = Extents.MinPoint;
           MaxExt = Extents.MaxPoint;
           Centroid = acSol.MassProperties.Centroid;
           Box = acSol.GetBoxVolume();
           Volume = acSol.Volume();
        }

        private void GetLayMatrix(Solid3d acSol)
        {          
            try
            {
                var bestVerts = GetBestVerts(acSol);

                if (bestVerts == null)
                {
                    //LayMatrix = BrepExt.LayMatrixForNotFlat(solid, tr);
                }
                else if (bestVerts.Count == 0)
                {
                    LayMatrix = new Matrix3d();
                }
                else
                {
                    LayMatrix = RefineLayMatrix(bestVerts);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

           
        }

        private Matrix3d RefineLayMatrix(List<VertExt> vertList)
        {
            if (vertList == null || vertList.Count == 0)
            {
                return new Matrix3d();
            }

            VertExt item = vertList.Max<VertExt>();
            var mat1 = item.LayMatrix();

            while (true)
            {          
                var mat2 = new Matrix3d();
                if (mat1 != mat2 || vertList.Count <= 1)
                {
                    return mat1;
                }
                vertList.Remove(item);
                mat1 = vertList.Max<VertExt>().LayMatrix();
            }
        }

        private List<VertExt> GetBestVerts(Solid3d acSol)
        {
            List<VertExt> vList = new List<VertExt>();

            using (var acBrep = new Brep(acSol))
            {
                if (acBrep.IsNull)
                {
                    return vList;
                }
                else if (!acBrep.IsNull)
                {

                    double y = 0.0;

                    try
                    {
                        foreach (var face in acBrep.Faces)
                        {
                            FaceCount++;
                            var fArea = 0.0;

                            if ((SubId.Type == SubentityType.Face) && (SubId == face.SubentityPath.SubentId))
                            {
                                fArea = face.GetArea();
                                SubArea = fArea.RoundArea();
                                SubPerimeter = face.GetPerimeterLength().RoundToTolerance();
                            }

                            using (var surface = face.Surface as ExternalBoundedSurface)
                            {
                                if (!surface.IsPlane)
                                {
                                    if (!surface.IsCylinder)
                                    {
                                        Has3DFaces = true;
                                    }
                                    HasNonFlatFaces = true;
                                    continue;
                                }
                            }

                            if (fArea == 0.0)
                            {
                                fArea = face.GetArea();
                            }

                            double x = fArea;

                            try
                            {
                                foreach (var acLoop in face.Loops)
                                {
                                    var lType = acLoop.GetLoopType();

                                    if (lType == LoopKit.Interior)
                                    {
                                        HasHoles = true;
                                        continue;
                                    }

                                    if (lType != LoopKit.Error)
                                    {
                                        if (lType == LoopKit.RightAngle)
                                        {
                                            x *= 1.5;
                                        }

                                        if (!x.IsEqualArea(y))
                                        {
                                            if (x < y)
                                            {
                                                continue;
                                            }

                                            vList.Clear();
                                            y = x;
                                            MaxArea = x.RoundArea();
                                            MaxPerimeter = face.GetPerimeterLength().RoundToTolerance();
                                        }

                                        try
                                        {
                                            foreach (var vtx in acLoop.Vertices)
                                            {
                                                vList.Add(new VertExt(vtx, acLoop));
                                                if (vList.Count > 1000)
                                                {
                                                    break;
                                                }
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine(e);
                                        }
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }

                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
                else
                {
                    if (SubId.Type == SubentityType.Edge)
                    {
                        try
                        {
                            foreach (var acEdge in acBrep.Edges)
                            {
                                FullSubentityPath subentityPath = acEdge.SubentityPath;
                                if (subentityPath.SubentId == this.SubId)
                                {
                                    this.SubPerimeter = acEdge.GetLength().RoundToTolerance();
                                    break;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }
                }

                if (this.MaxArea != 0.0)
                {
                    return vList;
                }

                return null;
            }
        }


        #endregion

    }
}