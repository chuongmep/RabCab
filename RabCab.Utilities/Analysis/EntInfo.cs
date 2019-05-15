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
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.BoundaryRepresentation;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using RabCab.Agents;
using RabCab.Calculators;
using RabCab.Extensions;
using static RabCab.Engine.Enumerators.Enums;
using static RabCab.Extensions.Solid3DExtensions;
using Exception = System.Exception;

namespace RabCab.Analysis
{
    public class EntInfo
    {
        #region ObjectProperties

        //Assembly & Reference Information
        public Handle ParentHandle;
        public List<Handle> ChildHandles;


        //Object Information

        public ObjectId ObjId;
        public Handle Hndl;
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
        public string AsymString;

        //Ent Information
        public string EntLayer;
        public Color EntColor;
        public string EntMaterial;

        //XData Information
        public string RcName;
        public bool IsSweep;
        public bool IsMirror;
        public bool IsChild;
        public string RcInfo;
        public int RcQtyOf;
        public int RcQtyTotal;
        public bool HasNonFlatFaces;
        public bool HasHoles;
        public bool Has3DFaces;
        public TextureDirection TxDirection;

        //Rotation Matrices
        public Matrix3d X90;
        public Matrix3d Y90;
        public Matrix3d Z90;
        public Matrix3d X180;
        public Matrix3d Y180;
        public Matrix3d Z180;
        public Matrix3d X270;
        public Matrix3d Y270;
        public Matrix3d Z270;

        public ProductionType ProdType
        {
            get
            {
                if (IsBox) return ProductionType.S4S;
                if (IsSweep) return ProductionType.Sweep;
                if (Has3DFaces) return ProductionType.MillingManySide;
                if (!HasHoles && !HasNonFlatFaces && FaceCount <= 6) return ProductionType.S4S;

                //TODO check for double side milling
                return ProductionType.MillingOneSide;
            }
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="acSol"></param>
        public EntInfo(Solid3d acSol, Database acCurDb, Transaction acTrans)
        {
            ObjId = acSol.ObjectId;
            Hndl = acSol.Handle;
            EntLayer = acSol.Layer;
            EntColor = acSol.Color;
            EntMaterial = acSol.Material;
            TxDirection = TextureDirection.Unknown;
            RcName = "";
            IsSweep = false;
            IsMirror = false;
            IsChild = false;
            RcInfo = "";
            RcQtyOf = 0;
            RcQtyTotal = 0;
            LayMatrix = Matrix3d.Identity;
            AsymmetryVector = new Vector3d();
            NumberOfChanges = acSol.NumChanges;
            ParentHandle = acSol.Handle;
            ChildHandles = new List<Handle>();
            ReadEntity(acSol, acCurDb, acTrans);
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="acSol"></param>
        /// <param name="subId"></param>
        public EntInfo(Solid3d acSol, SubentityId subId, Database acCurDb, Transaction acTrans)
        {
            ObjId = acSol.ObjectId;
            Hndl = acSol.Handle;
            SubId = subId;
            EntLayer = acSol.Layer;
            EntColor = acSol.Color;
            EntMaterial = acSol.Material;
            TxDirection = TextureDirection.Unknown;
            RcName = "";
            IsSweep = false;
            IsMirror = false;
            IsChild = false;
            RcInfo = "";
            RcQtyOf = 0;
            RcQtyTotal = 0;
            LayMatrix = Matrix3d.Identity;
            AsymmetryVector = new Vector3d();
            NumberOfChanges = acSol.NumChanges;
            ParentHandle = acSol.Handle;
            ChildHandles = new List<Handle>();
            ReadEntity(acSol, acCurDb, acTrans);
        }

        #endregion

        #region Comparison Methods

        /// <summary>
        ///     Method for setting EntReader to Null
        /// </summary>
        public bool IsNull =>
            Width == 0.0 && Length == 0.0 && Thickness == 0.0;

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
        public bool IsMirrorOf(EntInfo y)
        {
            if (Asymmetry == 0 || y.Asymmetry == 0) return false;

            if (AsymmetryVector.IsEqualTo(y.AsymmetryVector, CalcTol.UnitVector))
            {
                if (!AsymmetryVector.X.IsEqualTo(y.AsymmetryVector.X)) return true;
                if (!AsymmetryVector.Y.IsEqualTo(y.AsymmetryVector.Y)) return true;
                if (!AsymmetryVector.Z.IsEqualTo(y.AsymmetryVector.Z)) return true;

                return false;
            }

            if (!AsymmetryVector.X.IsEqualTo(y.AsymmetryVector.X)) return true;
            if (!AsymmetryVector.Y.IsEqualTo(y.AsymmetryVector.Y)) return true;
            if (!AsymmetryVector.Z.IsEqualTo(y.AsymmetryVector.Z)) return true;

            return false;
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="compEnt"></param>
        /// <returns></returns>
        public bool IsEqualSize(EntInfo compEnt)
        {
            return Length.IsEqualSize(compEnt.Length) && Width.IsEqualSize(compEnt.Width) &&
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
        public static bool operator ==(EntInfo a, EntInfo b)
        {
            return a != null && a.Equals(b);
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(EntInfo a, EntInfo b)
        {
            return a != null && !a.Equals(b);
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="vect"></param>
        /// <returns></returns>
        public string AsymVStr(Vector3d vect)
        {
            if (vect.IsLessThanTol())
            {
                AsymString = "";
                return AsymString;
            }

            AsymString = "";
            AsymString = !vect.X.IsLessThanTol() ? vect.X <= 0.0 ? "-" : "+" : AsymString + "0";
            AsymString = !vect.Y.IsLessThanTol()
                ? vect.Y <= 0.0 ? AsymString + "-" : AsymString + "+"
                : AsymString + "0";
            return !vect.Z.IsLessThanTol() ? vect.Z <= 0.0 ? AsymString + "-" : AsymString + "+" : AsymString + "0";
        }

        #endregion

        #region Overrides

        /// <summary>
        ///     TODO
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Length.GetHashCode() + Width.GetHashCode() + Thickness.GetHashCode() +
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

        /// <summary>
        ///     TODO
        /// </summary>
        /// <returns></returns>
        public string PrintInfo(bool supressPartName, int count = 0)
        {
            var acCurDb = Application.DocumentManager.MdiActiveDocument.Database;

            //Get variables to strings
            var countStr = count.ToString();
            var objIdStr = ObjId.ToString();
            var lengthStr = acCurDb.ConvertToDwgUnits(Length);
            var widthStr = acCurDb.ConvertToDwgUnits(Width);
            var thickStr = acCurDb.ConvertToDwgUnits(Thickness);
            var volStr = acCurDb.ConvertToDwgUnits(Volume);
            var asymStr = Asymmetry.RoundToTolerance();

            //Remove parenthesis from ObjIds
            objIdStr = objIdStr.Replace("(", "");
            objIdStr = objIdStr.Replace(")", "");

            if (count < 10) countStr = "0" + countStr;

            var prntStr = "";

            if (count > 0) prntStr += count + ":";

            if (!supressPartName)
            {
                if (!string.IsNullOrEmpty(RcName))
                    prntStr += RcName;

                else
                    prntStr += " #" + objIdStr;
            }

            prntStr += " - L:" + lengthStr +
                       " W:" + widthStr +
                       " T:" + thickStr +
                       " V:" + volStr +
                       " A:" + asymStr + " [" +
                       AsymVStr(AsymmetryVector)
                       + "] P:" + ProdType;

            //Print to the current editor
            return prntStr;
        }

        #endregion

        #region Parsing Methods

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="acSol"></param>
        private void ReadEntity(Solid3d acSol, Database acCurDb, Transaction acTrans)
        {
            RcName = acSol.GetPartName();
            ParentHandle = acSol.GetParent();
            ChildHandles = acSol.GetChildren();
            IsSweep = acSol.GetIsSweep();
            IsMirror = acSol.GetIsMirror();
            RcInfo = acSol.GetPartInfo();
            RcQtyOf = acSol.GetQtyOf();
            RcQtyTotal = acSol.GetQtyTotal();
            TxDirection = acSol.GetTextureDirection();

            GetRotationMatrices(acSol);
            GetLayMatrix(acSol);

            if (LayMatrix == new Matrix3d()) LayMatrix = GetAbstractMatrix(acSol);

            using (var solCopy = acSol.Clone() as Solid3d)
            {
                if (solCopy != null)
                {
                    if (LayMatrix != Matrix3d.Identity) solCopy.TransformBy(LayMatrix);

                    //Get Volume & Extents
                    Extents = solCopy.GetBounds();
                    MinExt = Extents.MinPoint;
                    MaxExt = Extents.MaxPoint;
                    Centroid = solCopy.MassProperties.Centroid;
                    Box = Extents.Volume();
                    Volume = acSol.Volume();
                }
            }

            var identity = Matrix3d.Identity;

            if ((MaxExt.Z + MinExt.Z) / 2 < 0)
            {
                var vector3D = new Vector3d(0, 1, 0);
                identity = Matrix3d.Rotation(3.14159265358979, vector3D, new Point3d());
                LayMatrix *= identity;
            }

            if (IsBox)
            {
                Asymmetry = 0;
            }
            else
            {
                var boxCen = GetBoxCenter(MinExt, MaxExt).RoundToTolerance();

                AsymmetryVector = boxCen.GetVectorTo(Centroid.RoundToTolerance());
                AsymmetryVector = AsymmetryVector.TransformBy(identity);
                Asymmetry = AsymmetryVector.Length;

                if (!Asymmetry.IsLessThanTol())
                    Asymmetry = Asymmetry.RoundToTolerance();
                else
                    Asymmetry = 0;

                AsymmetryVector = AsymmetryVector.RoundToTolerance();

                if (Asymmetry > 0) FixMatrix(boxCen);

                AsymString = AsymVStr(AsymmetryVector);
            }

            //Get length, width, thickness

            if (IsSweep)
            {
                //TODO
            }
            else
            {
                var measures = new List<double>(3)
                {
                    MaxExt.X - MinExt.X,
                    MaxExt.Y - MinExt.Y,
                    MaxExt.Z - MinExt.Z
                };

                measures.Sort();

                if (TxDirection == TextureDirection.Vertical)
                {
                    Width = measures[2].RoundToTolerance();
                    Length = measures[1].RoundToTolerance();
                }
                else
                {
                    Length = measures[2].RoundToTolerance();
                    Width = measures[1].RoundToTolerance();
                }

                Thickness = measures[0].RoundToTolerance();
            }

            //Add the XData
            acSol.AddXData(this, acCurDb, acTrans);
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="acSol"></param>
        private void GetLayMatrix(Solid3d acSol)
        {
            try
            {
                var bestVerts = GetBestVerts(acSol);

                if (bestVerts == null)
                    LayMatrix = GetAbstractMatrix(acSol);
                else if (bestVerts.Count == 0)
                    LayMatrix = new Matrix3d();
                else
                    LayMatrix = RefineLayMatrix(bestVerts);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="vertList"></param>
        /// <returns></returns>
        private Matrix3d RefineLayMatrix(List<VertExt> vertList)
        {
            if (vertList == null || vertList.Count == 0) return new Matrix3d();

            var item = vertList.Max();
            var mat1 = item.LayMatrix();

            while (true)
            {
                var mat2 = new Matrix3d();
                if (!(mat1 == mat2) || vertList.Count <= 1) return mat1;
                vertList.Remove(item);
                mat1 = vertList.Max().LayMatrix();
            }
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="acSol"></param>
        /// <returns></returns>
        private Matrix3d GetAbstractMatrix(Solid3d acSol)
        {
            var bestMatrix = new Matrix3d();

            using (var acBrep = acSol.GetBrep())
            {
                try
                {
                    if (acBrep.Faces.Any())
                    {
                        double largest = 0;

                        foreach (var acFace in acBrep.Faces)
                        {
                            var fArea = acFace.GetArea();

                            if (fArea.IsEqualArea(largest) || fArea < largest) continue;

                            largest = fArea;
                            bestMatrix = acFace.GetLayMatrix();
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return bestMatrix;
                }
            }

            return bestMatrix;
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="boxCenter"></param>
        /// <returns></returns>
        private bool FixMatrix(Point3d boxCenter)
        {
            var flag = false;
            if (AsymmetryVector.Z.IsLessThanTol() &&
                (AsymmetryVector.X + AsymmetryVector.Y > 0 ||
                 (AsymmetryVector.X + AsymmetryVector.Y).IsLessThanTol()
                 && AsymmetryVector.X > 0))
            {
                var matrix3D = Matrix3d.Rotation(3.14159265358979, new Vector3d(0, 1, 0), boxCenter);
                LayMatrix = matrix3D * LayMatrix;
                AsymmetryVector = AsymmetryVector.TransformBy(matrix3D);
                flag = true;
            }

            if (AsymmetryVector.Y.IsLessThanTol())
            {
                if (!AsymmetryVector.X.IsLessThanTol() && AsymmetryVector.X > 0)
                {
                    var matrix3D1 = Matrix3d.Rotation(3.14159265358979, new Vector3d(0, 0, 1), boxCenter);
                    LayMatrix = matrix3D1 * LayMatrix;
                    AsymmetryVector = AsymmetryVector.TransformBy(matrix3D1);
                    flag = true;
                }
            }
            else if (AsymmetryVector.Y > 0)
            {
                var matrix3D2 = Matrix3d.Rotation(3.14159265358979, new Vector3d(0, 0, 1), boxCenter);
                LayMatrix = matrix3D2 * LayMatrix;
                AsymmetryVector = AsymmetryVector.TransformBy(matrix3D2);
                flag = true;
            }

            return flag;
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="acSol"></param>
        /// <returns></returns>
        private List<VertExt> GetBestVerts(Solid3d acSol)
        {
            var vList = new List<VertExt>();

            using (var acBrep = new Brep(acSol))
            {
                if (acBrep.IsNull) return vList;

                if (!acBrep.IsNull)
                {
                    var y = 0.0;

                    try
                    {
                        foreach (var face in acBrep.Faces)
                        {
                            FaceCount++;
                            var fArea = 0.0;

                            if (SubId.Type == SubentityType.Face && SubId == face.SubentityPath.SubentId)
                            {
                                fArea = face.GetArea();
                                SubArea = fArea.RoundArea();
                                SubPerimeter = face.GetPerimeterLength().RoundToTolerance();
                            }

                            using (var surface = face.Surface as ExternalBoundedSurface)
                            {
                                if (!surface.IsPlane)
                                {
                                    if (!surface.IsCylinder) Has3DFaces = true;
                                    HasNonFlatFaces = true;
                                    continue;
                                }
                            }

                            if (fArea == 0.0) fArea = face.GetArea();

                            var x = fArea;

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
                                        if (lType == LoopKit.RightAngle) x *= 1.5;

                                        if (!x.IsEqualArea(y))
                                        {
                                            if (x < y) continue;

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
                                                if (vList.Count > 1000) break;
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
                        try
                        {
                            foreach (var acEdge in acBrep.Edges)
                            {
                                var subentityPath = acEdge.SubentityPath;
                                if (subentityPath.SubentId == SubId)
                                {
                                    SubPerimeter = acEdge.GetLength().RoundToTolerance();
                                    break;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                }

                return MaxArea != 0.0 ? vList : null;
            }
        }

        /// <summary>
        ///     Method to get the rotation matrices of a solid, based on its Centroid
        /// </summary>
        /// <param name="acSol">The solid to be rotated</param>
        private void GetRotationMatrices(Solid3d acSol)
        {
            var center = acSol.GetBoxCenter();

            using (var brep = new Brep(acSol))
            {
                var centroid = brep.GetMassProperties().Centroid.RoundToTolerance();

                if (centroid != center)
                    center = brep.GetMassProperties().Centroid;
            }

            //Find the 90 degree rotation matrices
            X90 = Matrix3d.Rotation(CalcUnit.ConvertToRadians(90), Vector3d.XAxis, center);
            Y90 = Matrix3d.Rotation(CalcUnit.ConvertToRadians(90), Vector3d.YAxis, center);
            Z90 = Matrix3d.Rotation(CalcUnit.ConvertToRadians(90), Vector3d.ZAxis, center);

            //Find the 180 degree rotation matrices
            X180 = Matrix3d.Rotation(CalcUnit.ConvertToRadians(180), Vector3d.XAxis, center);
            Y180 = Matrix3d.Rotation(CalcUnit.ConvertToRadians(180), Vector3d.YAxis, center);
            Z180 = Matrix3d.Rotation(CalcUnit.ConvertToRadians(180), Vector3d.ZAxis, center);

            //Find the 180 degree rotation matrices
            X270 = Matrix3d.Rotation(CalcUnit.ConvertToRadians(270), Vector3d.XAxis, center);
            Y270 = Matrix3d.Rotation(CalcUnit.ConvertToRadians(270), Vector3d.YAxis, center);
            Z270 = Matrix3d.Rotation(CalcUnit.ConvertToRadians(270), Vector3d.ZAxis, center);
        }

        #endregion
    }
}