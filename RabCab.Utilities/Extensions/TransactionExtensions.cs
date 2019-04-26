﻿// -----------------------------------------------------------------------------------
//     <copyright file="TransactionExtensions.cs" company="CraterSpace">
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
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace RabCab.Extensions

{
    public static class TransactionExtensions

    {
        // A simple extension method that aggregates the extents of any entities
        // passed in (via their ObjectIds)
        public static Extents3d GetExtents(this Transaction tr, ObjectId[] ids)
        {
            var ext = new Extents3d();
            foreach (var id in ids)
            {
                var ent = tr.GetObject(id, OpenMode.ForRead) as Entity;
                if (ent != null) ext.AddExtents(ent.GeometricExtents);
            }

            return ext;
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="acTrans"></param>
        /// <param name="acCurDb"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static Extents3d GetExtents(this Transaction acTrans, ObjectId[] ids, Database acCurDb)
        {
            //Add all selected objects to a temporary group
            var grDict = (DBDictionary)acTrans.GetObject(acCurDb.GroupDictionaryId, OpenMode.ForWrite);

            var anonyGroup = new Group();

            grDict.SetAt("*", anonyGroup);
            foreach (ObjectId objId in ids)
            {
                anonyGroup.Append(objId);
            }

            acTrans.AddNewlyCreatedDBObject(anonyGroup, true);

            var extents = acTrans.GetExtents(anonyGroup.GetAllEntityIds());

            foreach (ObjectId objId in ids)
            {
                anonyGroup.Remove(objId);
            }

            anonyGroup.Dispose();

            return extents;
        }

        public static Solid3d GetBoundingBox(this Transaction acTrans, ObjectId[] ids, Database acCurDb)
        {
            var extents = acTrans.GetExtents(ids, acCurDb);

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
            sol.TransformBy( Matrix3d.Displacement(sol.GeometricExtents.MinPoint.GetVectorTo(new Point3d(minX, minY, minZ))));

            return sol;
        }
    }
}