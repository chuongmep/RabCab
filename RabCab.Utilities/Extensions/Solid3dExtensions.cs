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
using Autodesk.AutoCAD.Runtime;
using RabCab.Settings;
using AcBr = Autodesk.AutoCAD.BoundaryRepresentation;

namespace RabCab.Extensions
{
    public static class Solid3DExtensions
    {
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
            var subentIdPe = acSol.QueryX(RXObject.GetClass(typeof (AssocPersSubentityIdPE)));

            if (subentIdPe == IntPtr.Zero)
                //Entity doesn't support the subentityPE
                return dbCollect;

            var subentityIdPe = RXObject.Create(subentIdPe, false) as AssocPersSubentityIdPE;

            // Parse all faces of the Solid
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
            var subentIdPe = acSol.QueryX(RXObject.GetClass(typeof (AssocPersSubentityIdPE)));

            if (subentIdPe == IntPtr.Zero)
                //Entity doesn't support the subentityPE
                return dbCollect;

            var subentityIdPe = RXObject.Create(subentIdPe, false) as AssocPersSubentityIdPE;

            // Parse all edges of the Solid
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
            var subentIdPe = acSol.QueryX(RXObject.GetClass(typeof (AssocPersSubentityIdPE)));

            if (subentIdPe == IntPtr.Zero)
                //Entity doesn't support the subentityPE
                return dbCollect;

            var subentityIdPe = RXObject.Create(subentIdPe, false) as AssocPersSubentityIdPE;

            // Parse all vertexs of the Solid
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

        #endregion

        #region Methods for Union, Subtract, Converge, & Gap

        /// <summary>
        ///     Method To Fuse Selected Solids Together and Prompt User to Keep or Delete Solids Used
        /// </summary>
        /// <param name="acCurDb">The Current Working Database</param>
        /// <param name="acTrans">The Current Working Transaction</param>
        /// <param name="boolId1">The IDList to run the command on</param>
        /// <param name="deleteSols">Delete consumed solids? True or False</param>
        public static ObjectId SolidFusion(Database acCurDb, Transaction acTrans, ObjectId[] boolId1, bool deleteSols)
        {
            // Create A List To Store Objects To Be Fused
            var fuseList = new List<Solid3d>();

            // Get Solids from the ID's provided
            foreach (var acSolId in boolId1)
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
            var acBlkTblRec =
                acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

            //Add the Fused Boolean to the Database
            acBlkTblRec?.AppendEntity(acBool1);
            acTrans.AddNewlyCreatedDBObject(acBool1, true);

            return acBool1.ObjectId;
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
        public static void SolidSubtrahend(Database acCurDb, Transaction acTrans, ObjectId[] boolId1,
            ObjectId[] boolId2,
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
        public static void SolidGap(Database acCurDb, Transaction acTrans, ObjectId[] boolId1, ObjectId[] boolId2,
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
        public static void SolidConverge(Database acCurDb, Transaction acTrans, ObjectId[] boolId1, bool deleteSols)
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
            var acBlkTblRec =
                acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

            acCurDb.AddLayer("RCConverge", Colors.LayerColorConverge, "CONTINUOUS", acTrans);

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

            // If User specified solids should be deleted, delete the solids
            if (deleteSols)
                foreach (var acSol in convList)
                    acSol.Erase();
        }

        #endregion
    }
}