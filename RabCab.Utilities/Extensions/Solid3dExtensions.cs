// -----------------------------------------------------------------------------------
//     <copyright file="Solid3dExtensions.cs" company="CraterSpace">
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
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using RabCab.Agents;
using RabCab.Analysis;
using RabCab.Calculators;
using RabCab.Engine.Enumerators;
using static RabCab.Settings.Colors;
using static RabCab.Settings.SettingsUser;
using AcBr = Autodesk.AutoCAD.BoundaryRepresentation;

namespace RabCab.Extensions
{
    public static class Solid3DExtensions
    {
        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="acSol"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public static void Move(this Solid3d acSol, Point3d from, Point3d to)
        {
            acSol.TransformBy(Matrix3d.Displacement(from.GetVectorTo(to)));
        }


        public static void MinToOrigin(this Solid3d acSol)
        {
            acSol.Upgrade();
            acSol.TransformBy(Matrix3d.Displacement(acSol.GetBounds().MinPoint.GetVectorTo(Point3d.Origin)));
            acSol.Downgrade();
        }

        public static void MaxToOrigin(this Solid3d acSol)
        {
            acSol.Upgrade();
            acSol.TransformBy(Matrix3d.Displacement(acSol.GetBounds().MaxPoint.GetVectorTo(Point3d.Origin)));
            acSol.Downgrade();
        }

        public static void MinCenterToOrigin(this Solid3d acSol)
        {
            acSol.Upgrade();
            var center = acSol.MassProperties.Centroid.Flatten();
            acSol.TransformBy(Matrix3d.Displacement(center.GetVectorTo(Point3d.Origin)));
            acSol.Downgrade();
        }

        public static void MaxCenterToOrigin(this Solid3d acSol)
        {
            acSol.Upgrade();
            var center = acSol.MassProperties.Centroid;
            var maxZ = acSol.GetBounds().MaxPoint.Z;
            center = new Point3d(center.X, center.Y, maxZ);
            acSol.TransformBy(Matrix3d.Displacement(center.GetVectorTo(Point3d.Origin)));
            acSol.Downgrade();
        }

        public static void CenterToOrigin(this Solid3d acSol)
        {
            acSol.Upgrade();
            acSol.TransformBy(Matrix3d.Displacement(acSol.GetBoxCenter().GetVectorTo(Point3d.Origin)));
            acSol.Downgrade();
        }

        public static double TopLeftToOrigin(this Solid3d acSol)
        {
            acSol.Upgrade();

            var min = acSol.GetBounds().MinPoint;
            var max = acSol.GetBounds().MaxPoint;
            var yDist = Math.Abs(max.Y - min.Y);

            acSol.TransformBy(Matrix3d.Displacement(new Point3d(min.X, max.Y, min.Z).GetVectorTo(Point3d.Origin)));
            acSol.Downgrade();

            return yDist;
        }

        public static double TopLeftTo(this Solid3d acSol, Point3d to)
        {
            acSol.Upgrade();

            var min = acSol.GetBounds().MinPoint;
            var max = acSol.GetBounds().MaxPoint;
            var yDist = Math.Abs(max.Y - min.Y);

            acSol.TransformBy(Matrix3d.Displacement(new Point3d(min.X, max.Y, min.Z).GetVectorTo(to)));
            acSol.Downgrade();

            return yDist;
        }

        public static void Upgrade(this Entity ent)
        {
            if (!ent.IsWriteEnabled)
                ent.UpgradeOpen();
        }

        public static void Downgrade(this Entity ent)
        {
            if (ent.IsWriteEnabled)
                ent.DowngradeOpen();
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="acSol"></param>
        /// <returns></returns>
        public static bool CheckRotation(this Solid3d acSol)
        {
            var faceList = new List<AcBr.Face>();
            Point3d centroid;

            using (var brep = new AcBr.Brep(acSol))
            {
                foreach (var face in brep.Faces)
                    faceList.Add(face);

                centroid = acSol.MassProperties.Centroid;
            }

            var largestFace = faceList.OrderByDescending(r => r.GetArea()).First();

            foreach (var loop in largestFace.Loops)
            {
                var vList = new List<AcBr.Vertex>();
                var lType = loop.GetLoopType();


                if (lType != Enums.LoopKit.Exterior) continue;

                try
                {
                    foreach (var vtx in loop.Vertices)
                    {
                        vList.Add(vtx);
                        if (vList.Count > 1000) break;
                    }

                    var val = vList.First().Point.Z;

                    if (val.IsLessThanTol())
                        return false;

                    var allZEqual = vList.All(x => x.Point.Z.IsEqualTo(val));

                    if (!allZEqual) continue;

                    acSol.Upgrade();
                    acSol.TransformBy(Matrix3d.Rotation(CalcUnit.ConvertToRadians(180), Vector3d.XAxis, centroid));
                    var minPt = acSol.GetBounds().MinPoint;
                    acSol.TransformBy(Matrix3d.Displacement(minPt.GetVectorTo(minPt.Flatten())));
                    acSol.Downgrade();
                    return true;
                }
                catch (AcBr.Exception)
                {
                    return false;
                }
            }

            return false;
        }

        #region Methods For Getting BREP Information from Solids

        /// <summary>
        ///     Method to return the BREP of a Solid3D by using the Full Subentity Path
        /// </summary>
        /// <param name="acSol">The solid of which to find the Brep</param>
        /// <returns></returns>
        public static AcBr.Brep GetBrep(this Solid3d acSol)
        {
            var objId = acSol.ObjectId;

            if (objId.IsNull)
                return new AcBr.Brep(acSol);

            ObjectId[] idArray = {objId};

            return new AcBr.Brep(new FullSubentityPath(idArray, new SubentityId(SubentityType.Null, IntPtr.Zero)));
        }

        /// <summary>
        ///     Method for returning BREP Complexes from an input Solid3D
        /// </summary>
        /// <param name="acSol">The input Solid3D</param>
        /// <returns></returns>
        public static AcBr.BrepComplexCollection GetComplexes(this Solid3d acSol)
        {
            //Initialize BREP Collection
            AcBr.BrepComplexCollection brepCollection;

            //Get BREP Subentities
            using (var acBrep = new AcBr.Brep(acSol))
            {
                brepCollection = acBrep.Complexes;
            }

            return brepCollection;
        }

        /// <summary>
        ///     Method for returning BREP Complexes from an input Solid3D using a Full Subentity Path
        /// </summary>
        /// <param name="acSol">The input Solid3D</param>
        /// <returns></returns>
        public static AcBr.BrepComplexCollection GetComplexesByPath(this Solid3d acSol)
        {
            //Initialize BREP Collection
            AcBr.BrepComplexCollection brepCollection;

            //Get Full Subentity Path
            ObjectId[] objIds = {acSol.ObjectId};
            var fSubPath = new FullSubentityPath(objIds, new SubentityId(SubentityType.Null, IntPtr.Zero));

            //Get BREP Subentities
            using (var acBrep = new AcBr.Brep(fSubPath))
            {
                brepCollection = acBrep.Complexes;
            }

            return brepCollection;
        }

        /// <summary>
        ///     Method for returning BREP Shells from an input Solid3D
        /// </summary>
        /// <param name="acSol">The input Solid3D</param>
        /// <returns></returns>
        public static AcBr.BrepShellCollection GetShells(this Solid3d acSol)
        {
            //Initialize BREP Collection
            AcBr.BrepShellCollection brepCollection;

            //Get BREP Subentities
            using (var acBrep = new AcBr.Brep(acSol))
            {
                brepCollection = acBrep.Shells;
            }

            return brepCollection;
        }

        /// <summary>
        ///     Method for returning BREP Shells from an input Solid3D using a Full Subentity Path
        /// </summary>
        /// <param name="acSol">The input Solid3D</param>
        /// <returns></returns>
        public static AcBr.BrepShellCollection GetShellsByPath(this Solid3d acSol)
        {
            //Initialize BREP Collection
            AcBr.BrepShellCollection brepCollection;

            //Get Full Subentity Path
            ObjectId[] objIds = {acSol.ObjectId};
            var fSubPath = new FullSubentityPath(objIds, new SubentityId(SubentityType.Null, IntPtr.Zero));

            //Get BREP Subentities
            using (var acBrep = new AcBr.Brep(fSubPath))
            {
                brepCollection = acBrep.Shells;
            }

            return brepCollection;
        }

        /// <summary>
        ///     Method for returning BREP Faces from an input Solid3D
        /// </summary>
        /// <param name="acSol">The input Solid3D</param>
        /// <returns></returns>
        public static AcBr.BrepFaceCollection GetFaces(this Solid3d acSol)
        {
            //Initialize BREP Collection
            AcBr.BrepFaceCollection brepCollection;

            //Get BREP Subentities
            using (var acBrep = new AcBr.Brep(acSol))
            {
                brepCollection = acBrep.Faces;
            }

            return brepCollection;
        }

        /// <summary>
        ///     Method for returning BREP Faces from an input Solid3D using a Full Subentity Path
        /// </summary>
        /// <param name="acSol">The input Solid3D</param>
        /// <returns></returns>
        public static AcBr.BrepFaceCollection GetFacesByPath(this Solid3d acSol)
        {
            //Initialize BREP Collection
            AcBr.BrepFaceCollection brepCollection;

            //Get Full Subentity Path
            ObjectId[] objIds = {acSol.ObjectId};
            var fSubPath = new FullSubentityPath(objIds, new SubentityId(SubentityType.Null, IntPtr.Zero));

            //Get BREP Subentities
            using (var acBrep = new AcBr.Brep(fSubPath))
            {
                brepCollection = acBrep.Faces;
            }

            return brepCollection;
        }

        /// <summary>
        ///     Method to Get Subentity faces of a solid and return them as entities ina DBObject Collection
        /// </summary>
        /// <param name="acSol">The input solid to be parsed</param>
        /// <returns></returns>
        public static DBObjectCollection GetFacesByHandle(this Solid3d acSol)
        {
            var dbCollect = new DBObjectCollection();

            //Create an object ID collection to hold the Solid Object ID
            ObjectId[] solId = {acSol.ObjectId};

            //Create a handle Pointer to use to find subentity handles
            var subentIdPe = acSol.QueryX(RXObject.GetClass(typeof(AssocPersSubentityIdPE)));

            if (subentIdPe == IntPtr.Zero)
                //Entity doesn't support the subentityPE
                return dbCollect;

            var subentityIdPe = RXObject.Create(subentIdPe, false) as AssocPersSubentityIdPE;

            // ParseAndFill all faces of the Solid
            if (subentityIdPe == null) return dbCollect;

            var faceIds = subentityIdPe.GetAllSubentities(acSol, SubentityType.Face);

            foreach (var subentId in faceIds)
            {
                //Create a full subentity path for the subentity
                var path = new FullSubentityPath(solId, subentId);
                var faceEntity = acSol.GetSubentity(path);

                if (faceEntity != null) dbCollect.Add(faceEntity);
            }

            //Return the collection
            return dbCollect;
        }

        /// <summary>
        ///     Method for returning BREP Edges from an input Solid3D
        /// </summary>
        /// <param name="acSol">The input Solid3D</param>
        /// <returns></returns>
        public static AcBr.BrepEdgeCollection GetEdges(this Solid3d acSol)
        {
            //Initialize BREP Collection
            AcBr.BrepEdgeCollection brepCollection;

            //Get BREP Subentities
            using (var acBrep = new AcBr.Brep(acSol))
            {
                brepCollection = acBrep.Edges;
            }

            return brepCollection;
        }

        /// <summary>
        ///     Method for returning BREP Edges from an input Solid3D using a Full Subentity Path
        /// </summary>
        /// <param name="acSol">The input Solid3D</param>
        /// <returns></returns>
        public static AcBr.BrepEdgeCollection GetEdgesByPath(this Solid3d acSol)
        {
            //Initialize BREP Collection
            AcBr.BrepEdgeCollection brepCollection;

            //Get Full Subentity Path
            ObjectId[] objIds = {acSol.ObjectId};
            var fSubPath = new FullSubentityPath(objIds, new SubentityId(SubentityType.Null, IntPtr.Zero));

            //Get BREP Subentities
            using (var acBrep = new AcBr.Brep(fSubPath))
            {
                brepCollection = acBrep.Edges;
            }

            return brepCollection;
        }

        /// <summary>
        ///     Method to Get Subentity edges of a solid and return them as entities ina DBObject Collection
        /// </summary>
        /// <param name="acSol">The input solid to be parsed</param>
        /// <returns></returns>
        public static DBObjectCollection GetEdgesByHandle(this Solid3d acSol)
        {
            var dbCollect = new DBObjectCollection();

            //Create an object ID collection to hold the Solid Object ID
            ObjectId[] solId = {acSol.ObjectId};

            //Create a handle Pointer to use to find subentity handles
            var subentIdPe = acSol.QueryX(RXObject.GetClass(typeof(AssocPersSubentityIdPE)));

            if (subentIdPe == IntPtr.Zero)
                //Entity doesn't support the subentityPE
                return dbCollect;

            var subentityIdPe = RXObject.Create(subentIdPe, false) as AssocPersSubentityIdPE;

            // ParseAndFill all edges of the Solid
            if (subentityIdPe == null) return dbCollect;

            var edgeIds = subentityIdPe.GetAllSubentities(acSol, SubentityType.Edge);

            foreach (var subentId in edgeIds)
            {
                //Create a full subentity path for the subentity
                var path = new FullSubentityPath(solId, subentId);
                var edgeEntity = acSol.GetSubentity(path);

                if (edgeEntity != null) dbCollect.Add(edgeEntity);
            }

            //Return the collection
            return dbCollect;
        }

        /// <summary>
        ///     Method for returning BREP Vertices from an input Solid3D
        /// </summary>
        /// <param name="acSol">The input Solid3D</param>
        /// <returns></returns>
        public static AcBr.BrepVertexCollection GetVertices(this Solid3d acSol)
        {
            //Initialize BREP Collection
            AcBr.BrepVertexCollection brepCollection;

            //Get BREP Subentities
            using (var acBrep = new AcBr.Brep(acSol))
            {
                brepCollection = acBrep.Vertices;
            }

            return brepCollection;
        }

        /// <summary>
        ///     Method for returning BREP Vertices from an input Solid3D using a Full Subentity Path
        /// </summary>
        /// <param name="acSol">The input Solid3D</param>
        /// <returns></returns>
        public static AcBr.BrepVertexCollection GetVerticesByPath(this Solid3d acSol)
        {
            //Initialize BREP Collection
            AcBr.BrepVertexCollection brepCollection;

            //Get Full Subentity Path
            ObjectId[] objIds = {acSol.ObjectId};
            var fSubPath = new FullSubentityPath(objIds, new SubentityId(SubentityType.Null, IntPtr.Zero));

            //Get BREP Subentities
            using (var acBrep = new AcBr.Brep(fSubPath))
            {
                brepCollection = acBrep.Vertices;
            }

            return brepCollection;
        }

        /// <summary>
        ///     Method to Get Subentity vertices of a solid and return them as entities ina DBObject Collection
        /// </summary>
        /// <param name="acSol">The input solid to be parsed</param>
        /// <returns></returns>
        public static DBObjectCollection GetVerticesByHandle(this Solid3d acSol)
        {
            var dbCollect = new DBObjectCollection();

            //Create an object ID collection to hold the Solid Object ID
            ObjectId[] solId = {acSol.ObjectId};

            //Create a handle Pointer to use to find subentity handles
            var subentIdPe = acSol.QueryX(RXObject.GetClass(typeof(AssocPersSubentityIdPE)));

            if (subentIdPe == IntPtr.Zero)
                //Entity doesn't support the subentityPE
                return dbCollect;

            var subentityIdPe = RXObject.Create(subentIdPe, false) as AssocPersSubentityIdPE;

            // ParseAndFill all vertexs of the Solid
            if (subentityIdPe == null) return dbCollect;

            var vertexIds = subentityIdPe.GetAllSubentities(acSol, SubentityType.Vertex);

            foreach (var subentId in vertexIds)
            {
                //Create a full subentity path for the subentity
                var path = new FullSubentityPath(solId, subentId);
                var vertexEntity = acSol.GetSubentity(path);

                if (vertexEntity != null) dbCollect.Add(vertexEntity);
            }

            //Return the collection
            return dbCollect;
        }

        /// <summary>
        ///     Utility Method For Getting A Subentity as an Entity - Given the solid and SubentityID
        /// </summary>
        /// <param name="acSol">The Solid to parse for the subentity</param>
        /// <param name="subId">The Subentity ID of the subentity to be parsed</param>
        /// <returns></returns>
        public static Entity GetSubentity(this Solid3d acSol, SubentityId subId)
        {
            //If the solid is not null
            if (acSol == null)
                return null;

            var objId = acSol.ObjectId;

            if (objId == ObjectId.Null || acSol.IsErased)
                return null;

            //Create an objID array to use with FullSubEntityPath
            var objIds = new[] { objId };

            return acSol.GetSubentity(new FullSubentityPath(objIds, subId));
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="acSol"></param>
        /// <returns></returns>
        public static Point3d GetBoxCenter(this Solid3d acSol)
        {
            var massProps = acSol.MassProperties;
            var minExt = massProps.Extents.MinPoint;
            var maxExt = massProps.Extents.MaxPoint;
            return GetBoxCenter(minExt, maxExt);
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="minExt"></param>
        /// <param name="maxExt"></param>
        /// <returns></returns>
        public static Point3d GetBoxCenter(Point3d minExt, Point3d maxExt)
        {
            return minExt.GetMidPoint(maxExt).RoundToTolerance();
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="acSol"></param>
        /// <returns></returns>
        public static Point3d GetBoxSize(Solid3d acSol)
        {
            var massProps = acSol.MassProperties;
            var minExt = massProps.Extents.MinPoint;
            var maxExt = massProps.Extents.MaxPoint;
            return GetBoxSize(minExt, maxExt);
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="minExt"></param>
        /// <param name="maxExt"></param>
        /// <returns></returns>
        public static Point3d GetBoxSize(Point3d minExt, Point3d maxExt)
        {
            return new Point3d(maxExt.X - minExt.X, maxExt.Y - minExt.Y, maxExt.Z - minExt.Z);
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="acSol"></param>
        /// <returns></returns>
        public static double GetSolVolume(this Solid3d acSol)
        {
            return acSol.MassProperties.Volume.RoundToTolerance();
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="acSol"></param>
        /// <returns></returns>
        public static Extents3d GetBounds(this Solid3d acSol)
        {
            try
            {
                using (var acBrep = new AcBr.Brep(acSol))
                {
                    using (var bBlock = acBrep.BoundBlock)
                    {
                        return new Extents3d(bBlock.GetMinimumPoint(), bBlock.GetMaximumPoint());
                    }
                }
            }
            catch (AcBr.Exception)
            {
                return new Extents3d(Point3d.Origin, Point3d.Origin);
            }
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="solid"></param>
        /// <returns></returns>
        public static bool IsBox(this Solid3d solid)
        {
            return solid.MassProperties.IsBox();
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="mProps"></param>
        /// <returns></returns>
        public static bool IsBox(this Solid3dMassProperties mProps)
        {
            return mProps.Volume.IsEqualVolume(mProps.Extents.Volume());
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="acSol"></param>
        /// <returns></returns>
        public static double Volume(this Solid3d acSol)
        {
            return acSol.MassProperties.Volume.RoundToTolerance();
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="extents"></param>
        /// <returns></returns>
        public static double Volume(this Extents3d extents)
        {
            var minPoint = extents.MinPoint;
            var maxPoint = extents.MaxPoint;
            return ((maxPoint.X - minPoint.X) * (maxPoint.Y - minPoint.Y) * (maxPoint.Z - minPoint.Z))
                .RoundToTolerance();
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="acSol"></param>
        /// <returns></returns>
        public static Solid3d GetBoundingBox(this Solid3d acSol)
        {
            var extents = acSol.GeometricExtents;

            //Get geom extents of all selected
            var minX = extents.MinPoint.X;
            var maxX = extents.MaxPoint.X;
            var minY = extents.MinPoint.Y;
            var maxY = extents.MaxPoint.Y;
            var minZ = extents.MinPoint.Z;
            var maxZ = extents.MaxPoint.Z;

            var sol = new Solid3d();

            var width = Math.Abs(maxX - minX);
            var length = Math.Abs(maxY - minY);
            var height = Math.Abs(maxZ - minZ);

            sol.CreateBox(width, length, height);
            sol.TransformBy(
                Matrix3d.Displacement(sol.GeometricExtents.MinPoint.GetVectorTo(new Point3d(minX, minY, minZ))));

            return sol;
        }

        /// <summary>
        ///     Utility Method to get the Full Subentity Path of a SubEntity
        /// </summary>
        /// <param name="acSol">The parent Solid</param>
        /// <param name="subEnt">The ID of the subentity to be determined</param>
        /// <returns></returns>
        public static FullSubentityPath GetFsPath(this Solid3d acSol, SubentityId subEnt)
        {
            ObjectId[] objIds = {acSol.ObjectId};
            return new FullSubentityPath(objIds, subEnt);
        }

        #endregion


        #region Methods for Union, Subtract, Converge, & Gap

        /// <summary>
        ///     Method To Fuse Selected Solids Together and Prompt User to Keep or Delete Solids Used
        /// </summary>
        /// <param name="acCurDb">The Current Working Database</param>
        /// <param name="acTrans">The Current Working Transaction</param>
        /// <param name="objIds">The IDList to run the command on</param>
        /// <param name="deleteSols">Delete consumed solids? True or False</param>
        public static ObjectId SolidFusion(this ObjectId[] objIds, Transaction acTrans, Database acCurDb,
            bool deleteSols)
        {
            // Create A List To Store Objects To Be Fused
            var fuseList = new List<Solid3d>();

            // Get Solids from the ID's provided
            foreach (var acSolId in objIds)
            {
                var acSol = acTrans.GetObject(acSolId, OpenMode.ForWrite) as Solid3d;

                if (acSol != null) fuseList.Add(acSol);
            }

            // Get the first selected solid - it will be used as the initial fuse point
            var acMainSol = fuseList.First();

            //Create a clone of the main solid
            var acBool1 = acMainSol.Clone() as Solid3d;

            // Remove the solid from the list
            fuseList.RemoveAt(0);

            // Fuse the Solids
            foreach (var acSol in fuseList)
            {
                //Create a clone of the solid to add to the main solid
                var acBool2 = acSol.Clone() as Solid3d;

                // Fuse the solids with a Boolean Operation
                acBool1?.BooleanOperation(BooleanOperationType.BoolUnite, acBool2);
            }

            // If User specified solids should be deleted, delete the solids
            if (deleteSols)
            {
                acMainSol.Erase();

                foreach (var acSol in fuseList) acSol.Erase();
            }

            // Ensure the fused solid is cleaned
            acBool1?.CleanBody();

            // Open the Block table for read
            var acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

            // Open the Block table record Model space for write
            if (acBlkTbl != null)
            {
                var acBlkTblRec =
                    acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                //Add the Fused Boolean to the Database
                acBlkTblRec?.AppendEntity(acBool1);
            }

            acTrans.AddNewlyCreatedDBObject(acBool1, true);

            if (acBool1 != null) return acBool1.ObjectId;

            return ObjectId.Null;
        }

        /// <summary>
        ///     Method To Subtract Selected Solids Together and Prompt User to Keep or Delete Solids Used
        /// </summary>
        /// <param name="acCurDb">The Current Working Database</param>
        /// <param name="acTrans">The Current Working Transaction</param>
        /// <param name="boolId1">The ID List to subtract objects From</param>
        /// <param name="boolId2">The ID List to use as subtraction objects</param>
        /// <param name="deleteSols">Delete consumed solids? True or False</param>
        /// open
        public static void SolidSubtrahend(this ObjectId[] boolId1, ObjectId[] boolId2, Database acCurDb,
            Transaction acTrans,
            bool deleteSols)
        {
            // Create A List To Store Objects To Be Fused
            var subtFromList = new List<Solid3d>();
            var subtToList = new List<Solid3d>();

            // Get Solids from the ID's provided
            foreach (var acSolId in boolId1)
            {
                var acSol = acTrans.GetObject(acSolId, OpenMode.ForWrite) as Solid3d;
                if (acSol != null) subtFromList.Add(acSol);
            }

            // Get Solids from the ID's provided
            foreach (var acSolId in boolId2)
            {
                var acSol = acTrans.GetObject(acSolId, OpenMode.ForWrite) as Solid3d;
                if (acSol != null) subtToList.Add(acSol);
            }

            // Open the Block table record in model space for write
            var acBlkTblRec = acTrans.GetObject(acCurDb.GetBlockTable(acTrans, 0)
                [BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

            //Subtract solids from BoolId2 fromt BoolId1
            foreach (var acSol in subtFromList)
            {
                foreach (var subtSol in subtToList)
                    //Create a clone of the solid to add to the main solid
                    using (var acBool = subtSol.Clone() as Solid3d)
                    {
                        acSol.BooleanOperation(BooleanOperationType.BoolSubtract, acBool);
                    }

                //Get the layer of the solid - this will be applied to the separated part (to keep it from going to layer 0)
                var solLayer = acSol.Layer;

                // Create an array of the seperated solids
                var seperatedSolids = acSol.SeparateBody();

                // add the solids back to the database that would otherwise be deleted
                foreach (var sepSol in seperatedSolids)
                {
                    // Set the solids layer
                    sepSol.Layer = solLayer;

                    //Append it to the database
                    acBlkTblRec?.AppendEntity(sepSol);
                    acTrans.AddNewlyCreatedDBObject(sepSol, true);
                }

                // Ensure the fused solid is cleaned
                acSol.CleanBody();
            }

            // If User specified solids should be deleted, delete the solids
            if (deleteSols)
                foreach (var acSol in subtToList)
                {
                    acSol.Erase();
                    acSol.Dispose();
                }
        }

        /// <summary>
        ///     Method To Subtract Selected Solids Together and Prompt User to Keep or Delete Solids Used
        /// </summary>
        /// <param name="acCurDb">The Current Working Database</param>
        /// <param name="acTrans">The Current Working Transaction</param>
        /// <param name="boolId1">The ID List to subtract objects From</param>
        /// <param name="boolId2">The ID List to use as subtraction objects</param>
        /// <param name="deleteSols">Delete consumed solids? True or False</param>
        /// <param name="offset"></param>
        /// open
        public static void SolidGap(this ObjectId[] boolId1, ObjectId[] boolId2, Database acCurDb, Transaction acTrans,
            bool deleteSols, double offset)
        {
            // Create A List To Store Objects To Be Fused
            var subtFromList = new List<Solid3d>();
            var subtToList = new List<Solid3d>();

            // Get Solids from the ID's provided
            foreach (var acSolId in boolId1)
            {
                var acSol = acTrans.GetObject(acSolId, OpenMode.ForWrite) as Solid3d;
                if (acSol != null) subtFromList.Add(acSol);
            }

            // Get Solids from the ID's provided
            foreach (var acSolId in boolId2)
            {
                var acSol = acTrans.GetObject(acSolId, OpenMode.ForWrite) as Solid3d;
                if (acSol != null)
                    subtToList.Add(acSol);
            }

            // Create a utility object


            // Open the Block table record in model space for write
            var acBlkTblRec = acTrans.GetObject(acCurDb.GetBlockTable(acTrans, 0)
                [BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

            //Subtract solids from BoolId2 fromt BoolId1
            foreach (var acSol in subtFromList)
            {
                foreach (var subtSol in subtToList)
                {
                    //Create a clone of the solid to add to the main solid
                    var acBool = subtSol.Clone() as Solid3d;
                    acBool?.OffsetBody(offset);

                    //  Subtract the solids with a Boolean Operation
                    acSol.BooleanOperation(BooleanOperationType.BoolSubtract, acBool);
                }

                //Get the layer of the solid - this will be applied to the separated part (to keep it from going to layer 0)
                var solLayer = acSol.Layer;

                // Create an array of the seperated solids
                var seperatedSolids = acSol.SeparateBody();

                // add the solids back to the database that would otherwise be deleted
                foreach (var sepSol in seperatedSolids)
                {
                    // Set the solids layer
                    sepSol.Layer = solLayer;

                    //Append it to the database
                    acBlkTblRec?.AppendEntity(sepSol);
                    acTrans.AddNewlyCreatedDBObject(sepSol, true);
                }

                // Ensure the fused solid is cleaned
                acSol.CleanBody();
            }

            // If User specified solids should be deleted, delete the solids
            if (deleteSols)
                foreach (var acSol in subtToList)
                    acSol.Erase();
        }

        /// <summary>
        ///     Method To Find Convergence Of Selected Solids and Prompt User to Keep or Delete Solids Used
        /// </summary>
        /// <param name="acCurDb">The Current Working Database</param>
        /// <param name="acTrans">The Current Working Transaction</param>
        /// <param name="boolId1">The ID List to Find Convergence Of</param>
        /// <param name="deleteSols">Delete consumed solids? True or False</param>
        public static void SolidConverge(this ObjectId[] boolId1, Database acCurDb, Transaction acTrans,
            bool deleteSols)
        {
            // Create A List To Store Objects To Be Fused
            var convList = new List<Solid3d>();
            var ranIds = new List<ObjectId>();

            // Get Solids from the ID's provided
            foreach (var acSolId in boolId1)
            {
                var acSol = acTrans.GetObject(acSolId, OpenMode.ForWrite) as Solid3d;
                if (acSol != null) convList.Add(acSol);
            }

            // Open the Block table for read
            var acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

            // Open the Block table record Model space for write
            if (acBlkTbl != null)
            {
                var acBlkTblRec =
                    acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                acCurDb.AddLayer("RCConverge", LayerColorConverge, "CONTINUOUS", acTrans);

                // Find Convergence Of the Solids
                foreach (var acSol in convList)
                {
                    ranIds.Add(acSol.Id);

                    foreach (var checkSol in convList)
                    {
                        if (ranIds.Contains(checkSol.Id)) continue;

                        //Create a clone of the solids and add them to the Database
                        var acBool1 = acSol.Clone() as Solid3d;
                        var acBool2 = checkSol.Clone() as Solid3d;

                        //Set the color of the solids
                        if (acBool1 == null) continue;
                        acBool1.Layer = "RCConverge";

                        if (acBool2 == null) continue;
                        acBool2.Layer = "RCConverge";

                        acBlkTblRec?.AppendEntity(acBool1);
                        acBlkTblRec?.AppendEntity(acBool2);

                        acTrans.AddNewlyCreatedDBObject(acBool1, true);
                        acTrans.AddNewlyCreatedDBObject(acBool2, true);

                        // Fuse the solids with a Boolean Operation
                        acBool1.BooleanOperation(BooleanOperationType.BoolIntersect, acBool2);
                    }
                }
            }

            // If User specified solids should be deleted, delete the solids
            if (deleteSols)
                foreach (var acSol in convList)
                    acSol.Erase();
        }

        public static void Separate(this ObjectId[] objIds, Database acCurDb, Transaction acTrans)
        {
            using (var pWorker = new ProgressAgent("Separating Solids: ", objIds.Length))
            {
                foreach (var objId in objIds)
                {
                    //Tick progress bar or exit if ESC has been pressed
                    if (!pWorker.Tick()) return;

                    var acSol = acTrans.GetObject(objId, OpenMode.ForWrite) as Solid3d;
                    if (acSol == null) continue;

                    var sepSols = acSol.SeparateBody();

                    foreach (var newSol in sepSols)
                    {
                        newSol.SetPropertiesFrom(acSol);
                        acCurDb.AppendEntity(newSol);
                    }
                }
            }
        }

        public static void Clean(this ObjectId[] objIds, Database acCurDb, Transaction acTrans)
        {
            using (var pWorker = new ProgressAgent("Cleaning Solids: ", objIds.Length))
            {
                foreach (var objId in objIds)
                {
                    //Tick progress bar or exit if ESC has been pressed
                    if (!pWorker.Tick()) return;

                    var acSol = acTrans.GetObject(objId, OpenMode.ForWrite) as Solid3d;
                    if (acSol == null) continue;

                    acSol.CleanBody();
                }
            }
        }

        #endregion

        #region Methods for 2D Creation

        /// <summary>
        ///     Utility method to create a 2d representation of a 3d object
        /// </summary>
        /// <param name="acEnt"></param>
        /// <param name="acTrans"></param>
        /// <param name="acCurDb"></param>
        /// <param name="acCurEd"></param>
        /// <param name="visible"></param>
        /// <param name="hidden"></param>
        /// <param name="translate"></param>
        /// <param name="userCoordSystem"></param>
        /// <param name="pWorker"></param>
        public static void Flatten(this Entity acEnt, Transaction acTrans, Database acCurDb, Editor acCurEd,
            bool visible, bool hidden, bool translate, Matrix3d userCoordSystem)
        {
            // Open the Block currently active space for write
            var bt = (BlockTable) acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead);

            var ms = (BlockTableRecord) acTrans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

            var ptCol = new Point3dCollection {new Point3d(0, 0, 0), new Point3d(1, 0, 0)};

            const SectionType acSectionType = SectionType.Section2d;
            var acSection = new Section(ptCol, Vector3d.ZAxis) {State = SectionState.Plane};

            var geomMin = acEnt.GeometricExtents.MinPoint;
            var geomMax = acEnt.GeometricExtents.MaxPoint;

            var xLength = Math.Abs(geomMax.X - geomMin.X);
            var yLength = Math.Abs(geomMax.Y - geomMin.Y);
            var zLength = Math.Abs(geomMax.Z - geomMin.Z);

            if (xLength > zLength)
                zLength = xLength;

            if (yLength > zLength)
                zLength = yLength;

            if (zLength < 10)
                zLength = 10;

            var pt1 = Point3d.Origin;
            var pt2 = new Point3d(0, 0, zLength * 2);

            acSection.TransformBy(Matrix3d.Rotation(CalcUnit.ConvertToRadians(270), Vector3d.XAxis,
                new Point3d(0, 0, 0)));
            acSection.TransformBy(Matrix3d.Displacement(pt1.GetTransformedVector(pt2, acCurEd)));

            if (translate)
                acSection.TransformBy(userCoordSystem);

            ms.AppendEntity(acSection);

            acTrans.AddNewlyCreatedDBObject(acSection, true);

            // Set up some of its direct properties
            acSection.SetHeight(SectionHeight.HeightAboveSectionLine, 60.0);
            acSection.SetHeight(SectionHeight.HeightBelowSectionLine, 60.0);

            // ... and then its settings
            var ss = (SectionSettings) acTrans.GetObject(acSection.Settings, OpenMode.ForWrite);

            // Set our section type
            ss.CurrentSectionType = acSectionType;

            var oic = new ObjectIdCollection {acEnt.ObjectId};
            ss.SetSourceObjects(acSectionType, oic);

            // 2D-specific settings
            ss.SetVisibility(acSectionType, SectionGeometry.BackgroundGeometry, visible);
            ss.SetHiddenLine(acSectionType, SectionGeometry.BackgroundGeometry, hidden);

            ss.SetGenerationOptions(acSectionType,
                SectionGeneration.SourceSelectedObjects | SectionGeneration.DestinationFile);

            var ent = (Entity) acTrans.GetObject(acEnt.ObjectId, OpenMode.ForWrite);

            acSection.GenerateSectionGeometry(ent, out var flEnts, out var bgEnts, out var fgEnts, out _,
                out var ctEnts);

            if (hidden)
                acCurDb.AddLayer(RcHidden, LayerColorRcHidden, RcHiddenLT, acTrans);
            else
                acCurDb.AddLayer(RcVisible, LayerColorRcVisible, RcVisibleLT, acTrans);

            foreach (Entity apEnt in flEnts)
            {
                apEnt.Layer = hidden ? RcHidden : RcVisible;

                apEnt.ColorIndex = 256;

                if (translate)
                    apEnt.TransformBy(userCoordSystem.Inverse());

                apEnt.TransformBy(Matrix3d.Displacement(pt2.GetTransformedVector(pt1, acCurEd)));

                if (translate)
                    apEnt.TransformBy(userCoordSystem);

                ms.AppendEntity(apEnt);
                acTrans.AddNewlyCreatedDBObject(apEnt, true);
            }

            foreach (Entity apEnt in bgEnts)
            {
                apEnt.Layer = hidden ? RcHidden : RcVisible;

                apEnt.ColorIndex = 256;

                if (translate)
                    apEnt.TransformBy(userCoordSystem.Inverse());

                apEnt.TransformBy(Matrix3d.Displacement(pt2.GetTransformedVector(pt1, acCurEd)));


                if (translate)
                    apEnt.TransformBy(userCoordSystem);

                ms.AppendEntity(apEnt);
                acTrans.AddNewlyCreatedDBObject(apEnt, true);
            }

            foreach (Entity apEnt in fgEnts)
            {
                apEnt.Layer = hidden ? RcHidden : RcVisible;

                apEnt.ColorIndex = 256;

                if (translate)
                    apEnt.TransformBy(userCoordSystem.Inverse());

                apEnt.TransformBy(Matrix3d.Displacement(pt2.GetTransformedVector(pt1, acCurEd)));

                if (translate)
                    apEnt.TransformBy(userCoordSystem);

                ms.AppendEntity(apEnt);
                acTrans.AddNewlyCreatedDBObject(apEnt, true);
            }

            foreach (Entity apEnt in ctEnts)
            {
                apEnt.Layer = hidden ? RcHidden : RcVisible;

                apEnt.ColorIndex = 256;

                if (translate)
                    apEnt.TransformBy(userCoordSystem.Inverse());

                apEnt.TransformBy(Matrix3d.Displacement(pt2.GetTransformedVector(pt1, acCurEd)));

                if (translate)
                    apEnt.TransformBy(userCoordSystem);

                ms.AppendEntity(apEnt);
                acTrans.AddNewlyCreatedDBObject(apEnt, true);
            }

            acSection.Erase();
            acSection.Dispose();
        }

        public static bool BoundsIntersect(this Solid3d sol1, Solid3d sol2, Database acCurDb, Transaction acTrans,
            double tolerance = 0)
        {
            bool interferes;

            using (var bounds1 = sol1.GetBoundingBox())
            {
                using (var bounds2 = sol2.GetBoundingBox())
                {
                    acCurDb.AppendEntity(bounds1, acTrans);
                    acCurDb.AppendEntity(bounds2, acTrans);

                    if (!Math.Abs(tolerance).IsLessThanTol())
                        try
                        {
                            bounds1.OffsetBody(tolerance);
                        }
                        catch (AcBr.Exception e)
                        {
                            Console.WriteLine(e);
                        }

                    interferes = bounds1.CheckInterference(bounds2);

                    bounds1.Erase();
                    bounds2.Erase();
                }
            }

            return interferes;
        }

        /// <summary>
        ///     Method to flatten all sides of a solid3d
        /// </summary>
        /// <param name="acSol"></param>
        /// <param name="acCurDb"></param>
        /// <param name="acCurEd"></param>
        /// <param name="acTrans"></param>
        public static double FlattenAllSides(this Solid3d acSol, Database acCurDb, Editor acCurEd, Transaction acTrans)
        {
            if (acSol == null) return 0;

            var eInfo = new EntInfo(acSol, acCurDb, acTrans);

            var yStep = LayStep;
            var solBounds = acSol.GetBounds();
            var geomMin = solBounds.MinPoint;
            var geomMax = solBounds.MaxPoint;

            var xLength = Math.Abs(geomMax.X - geomMin.X);
            var yLength = Math.Abs(geomMax.Y - geomMin.Y);
            var zLength = Math.Abs(geomMax.Z - geomMin.Z);

            if (zLength > xLength)
                xLength = zLength;

            if (zLength > yLength)
                yLength = zLength;

            var topView = acSol.Clone() as Solid3d;
            var frontView = acSol.Clone() as Solid3d;
            var bottomView = acSol.Clone() as Solid3d;
            var rearView = acSol.Clone() as Solid3d;
            var leftView = acSol.Clone() as Solid3d;
            var rightView = acSol.Clone() as Solid3d;

            var frontY = new Point3d(geomMin.X, geomMin.Y - (yLength + yStep), geomMin.Z);
            var bottomY = new Point3d(geomMin.X, geomMin.Y - (yLength + yStep) * 2, geomMin.Z);
            var rearY = new Point3d(geomMin.X, geomMin.Y - (yLength + yStep) * 3, geomMin.Z);
            var leftX = new Point3d(geomMin.X - xLength / 2, geomMin.Y, geomMin.Z);
            var rightX = new Point3d(geomMin.X + xLength / 2, geomMin.Y, geomMin.Z);

            frontView?.TransformBy(eInfo.X90);
            bottomView?.TransformBy(eInfo.X180);
            rearView?.TransformBy(eInfo.X270);
            leftView?.TransformBy(eInfo.Y270);
            rightView?.TransformBy(eInfo.Y90);

            frontView?.TransformBy(Matrix3d.Displacement(geomMin.GetTransformedVector(frontY, acCurEd)));
            bottomView?.TransformBy(Matrix3d.Displacement(geomMin.GetTransformedVector(bottomY, acCurEd)));
            rearView?.TransformBy(Matrix3d.Displacement(geomMin.GetTransformedVector(rearY, acCurEd)));
            leftView?.TransformBy(Matrix3d.Displacement(geomMin.GetTransformedVector(leftX, acCurEd)));
            rightView?.TransformBy(Matrix3d.Displacement(geomMin.GetTransformedVector(rightX, acCurEd)));

            acCurDb.AppendEntity(topView, acTrans);
            acCurDb.AppendEntity(frontView, acTrans);
            acCurDb.AppendEntity(bottomView, acTrans);
            acCurDb.AppendEntity(rearView, acTrans);
            acCurDb.AppendEntity(leftView, acTrans);
            acCurDb.AppendEntity(rightView, acTrans);

            //Check if views interfere
            var interferes = true;

            while (interferes)
                if (frontView != null && frontView.BoundsIntersect(topView, acCurDb, acTrans, LayStep))
                {
                    frontView.TransformBy(
                        Matrix3d.Displacement(frontY.GetTransformedVector(frontY.StepYPoint(), acCurEd)));
                    bottomView?.TransformBy(
                        Matrix3d.Displacement(bottomY.GetTransformedVector(bottomY.StepYPoint(), acCurEd)));
                    rearView?.TransformBy(
                        Matrix3d.Displacement(rearY.GetTransformedVector(rearY.StepYPoint(), acCurEd)));
                }
                else
                {
                    interferes = false;
                }

            interferes = true;

            while (interferes)
                if (bottomView != null && bottomView.BoundsIntersect(frontView, acCurDb, acTrans, LayStep))
                {
                    bottomView.TransformBy(
                        Matrix3d.Displacement(bottomY.GetTransformedVector(bottomY.StepYPoint(), acCurEd)));
                    rearView?.TransformBy(
                        Matrix3d.Displacement(rearY.GetTransformedVector(rearY.StepYPoint(), acCurEd)));
                }
                else
                {
                    interferes = false;
                }

            interferes = true;

            while (interferes)
                if (rearView != null && rearView.BoundsIntersect(bottomView, acCurDb, acTrans, LayStep))
                    rearView.TransformBy(
                        Matrix3d.Displacement(rearY.GetTransformedVector(rearY.StepYPoint(), acCurEd)));
                else
                    interferes = false;

            interferes = true;

            while (interferes)
                if (leftView != null && leftView.BoundsIntersect(topView, acCurDb, acTrans, LayStep))
                    leftView.TransformBy(Matrix3d.Displacement(leftX.GetTransformedVector(leftX.StepXLeft(), acCurEd)));
                else
                    interferes = false;

            interferes = true;

            while (interferes)
                if (rightView != null && rightView.BoundsIntersect(topView, acCurDb, acTrans, LayStep))
                    rightView.TransformBy(
                        Matrix3d.Displacement(rightX.GetTransformedVector(rightX.StepXRight(), acCurEd)));
                else
                    interferes = false;

            CreateFlatShotText("TOP", topView, acCurDb, acTrans);
            CreateFlatShotText("FRONT", frontView, acCurDb, acTrans);
            CreateFlatShotText("BOTTOM", bottomView, acCurDb, acTrans);
            CreateFlatShotText("BACK", rearView, acCurDb, acTrans);
            CreateFlatShotText("LEFT", leftView, acCurDb, acTrans);
            CreateFlatShotText("RIGHT", rightView, acCurDb, acTrans);

            //Create Top Views
            topView.Flatten(acTrans, acCurDb, acCurEd, true, false, false, Matrix3d.Identity);
            topView.Flatten(acTrans, acCurDb, acCurEd, false, true, false, Matrix3d.Identity);

            //Create Front Views
            frontView.Flatten(acTrans, acCurDb, acCurEd, true, false, false, Matrix3d.Identity);
            frontView.Flatten(acTrans, acCurDb, acCurEd, false, true, false, Matrix3d.Identity);

            //Create Bottom Views
            bottomView.Flatten(acTrans, acCurDb, acCurEd, true, false, false, Matrix3d.Identity);
            bottomView.Flatten(acTrans, acCurDb, acCurEd, false, true, false, Matrix3d.Identity);

            //Create Rear Views
            rearView.Flatten(acTrans, acCurDb, acCurEd, true, false, false, Matrix3d.Identity);
            rearView.Flatten(acTrans, acCurDb, acCurEd, false, true, false, Matrix3d.Identity);

            //Create Left Views
            leftView.Flatten(acTrans, acCurDb, acCurEd, true, false, false, Matrix3d.Identity);
            leftView.Flatten(acTrans, acCurDb, acCurEd, false, true, false, Matrix3d.Identity);

            //Create Right Views
            rightView.Flatten(acTrans, acCurDb, acCurEd, true, false, false, Matrix3d.Identity);
            rightView.Flatten(acTrans, acCurDb, acCurEd, false, true, false, Matrix3d.Identity);

            acSol.Erase();
            topView?.Erase();
            frontView?.Erase();
            bottomView?.Erase();
            rearView?.Erase();
            leftView?.Erase();
            rightView?.Erase();

            acSol.Dispose();
            topView?.Dispose();
            frontView?.Dispose();
            bottomView?.Dispose();
            rearView?.Dispose();
            leftView?.Dispose();
            rightView?.Dispose();

            return (yLength + yStep) * 4;
        }

        /// <summary>
        ///     Utility method for creating flat shot text identifiers
        /// </summary>
        /// <param name="text"></param>
        /// <param name="acEnt"></param>
        /// <param name="acCurDb"></param>
        /// <param name="acTrans"></param>
        private static void CreateFlatShotText(string text, Entity acEnt, Database acCurDb, Transaction acTrans)
        {
            var geomMin = acEnt.GeometricExtents.MinPoint;
            var geomMax = acEnt.GeometricExtents.MaxPoint;

            var xLength = Math.Abs(geomMax.X - geomMin.X);

            //View Text
            using (var acText = new MText())
            {
                acCurDb.AddLayer(RcAnno, LayerColorRcAnno, RcAnnoLt,
                    acTrans);

                //Set the text height                   
                acText.TextHeight = LayTextHeight;
                acText.Layer = RcAnno;
                acText.ColorIndex = 256;

                //ParseAndFill the insertion point and text alignment
                if (LayTextLeft)
                {
                    acText.Attachment = AttachmentPoint.TopLeft;
                    acText.Location = new Point3d(geomMin.X, geomMin.Y - 1, 0);
                }
                else if (LayTextCenter)
                {
                    acText.Attachment = AttachmentPoint.TopCenter;
                    acText.Location = new Point3d(geomMin.X + xLength / 2, geomMin.Y - 1, 0);
                }

                acText.Contents = text;

                //Append the text
                acCurDb.AppendEntity(acText);
            }
        }

        #endregion
    }
}