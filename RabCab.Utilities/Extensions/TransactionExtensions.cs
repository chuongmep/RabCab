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

using Autodesk.AutoCAD.DatabaseServices;

namespace RabCab.Utilities.Extensions

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
    }
}