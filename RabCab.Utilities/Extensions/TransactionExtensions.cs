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