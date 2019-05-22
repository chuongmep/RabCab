// -----------------------------------------------------------------------------------
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
using Autodesk.AutoCAD.Geometry;
using RabCab.Engine.Enumerators;

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
        ///     TODO
        /// </summary>
        /// <param name="acTrans"></param>
        /// <param name="acCurDb"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static Extents3d GetExtents(this Transaction acTrans, ObjectId[] ids, Database acCurDb)
        {
            //Add all selected objects to a temporary group
            var grDict = (DBDictionary) acTrans.GetObject(acCurDb.GroupDictionaryId, OpenMode.ForWrite);

            var anonyGroup = new Group();

            grDict.SetAt("*", anonyGroup);
            foreach (var objId in ids) anonyGroup.Append(objId);

            acTrans.AddNewlyCreatedDBObject(anonyGroup, true);

            var extents = acTrans.GetExtents(anonyGroup.GetAllEntityIds());

            foreach (var objId in ids) anonyGroup.Remove(objId);

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
            sol.TransformBy(
                Matrix3d.Displacement(sol.GeometricExtents.MinPoint.GetVectorTo(new Point3d(minX, minY, minZ))));

            return sol;
        }


        public static void MoveToAttachment(this Transaction acTrans, Entity acEnt,
            Enums.AttachmentPoint attachmentPoint, Point3d initPoint, double xOffset = 0, double yOffset = 0)
        {
            var ext = acEnt.GeometricExtents;
            var extMin = ext.MinPoint;
            var extMax = ext.MaxPoint;

            var fromPoint = new Point3d();

            switch (attachmentPoint)
            {
                case Enums.AttachmentPoint.TopLeft:
                    fromPoint = new Point3d(extMin.X - xOffset, extMax.Y + yOffset, 0);
                    break;
                case Enums.AttachmentPoint.TopRight:
                    fromPoint = new Point3d(extMax.X + xOffset, extMax.Y + yOffset, 0);
                    break;
                case Enums.AttachmentPoint.BottomLeft:
                    fromPoint = new Point3d(extMin.X - xOffset, extMin.Y - yOffset, 0);
                    break;
                case Enums.AttachmentPoint.BottomRight:
                    fromPoint = new Point3d(extMax.X + xOffset, extMin.Y - yOffset, 0);
                    break;
                case Enums.AttachmentPoint.TopCenter:
                    var leftTc = new Point3d(extMin.X, extMax.Y + yOffset, 0);
                    var rightTc = new Point3d(extMax.X, extMax.Y + yOffset, 0);
                    fromPoint = leftTc.GetMidPoint(rightTc);
                    break;
                case Enums.AttachmentPoint.BottomCenter:
                    var leftBc = new Point3d(extMin.X, extMin.Y - yOffset, 0);
                    var rightBc = new Point3d(extMax.X, extMin.Y - yOffset, 0);
                    fromPoint = leftBc.GetMidPoint(rightBc);
                    break;
                case Enums.AttachmentPoint.LeftCenter:
                    var botLc = new Point3d(extMin.X - xOffset, extMin.Y, 0);
                    var topLc = new Point3d(extMin.X - xOffset, extMax.Y, 0);
                    fromPoint = botLc.GetMidPoint(topLc);
                    break;
                case Enums.AttachmentPoint.RightCenter:
                    var botRc = new Point3d(extMax.X - xOffset, extMin.Y, 0);
                    var topRc = new Point3d(extMax.X - xOffset, extMax.Y, 0);
                    fromPoint = botRc.GetMidPoint(topRc);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(attachmentPoint), attachmentPoint, null);
            }

            acEnt.Upgrade();
            acEnt.TransformBy(Matrix3d.Displacement(fromPoint.GetVectorTo(initPoint)));
        }
    }
}