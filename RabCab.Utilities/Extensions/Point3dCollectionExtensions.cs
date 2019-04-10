// -----------------------------------------------------------------------------------
//     <copyright file="Point3dCollectionExtensions.cs" company="CraterSpace">
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
using Autodesk.AutoCAD.Geometry;

namespace RabCab.Extensions
{
    /// <summary>
    ///     Provides extension methods for the Point3dCollection type.
    /// </summary>
    public static class Point3dCollectionExtensions
    {
        /// <summary>
        ///     Removes duplicated points in the collection.
        /// </summary>
        /// <param name="pts">The instance to which the method applies.</param>
        public static void RemoveDuplicate(this Point3dCollection pts)
        {
            pts.RemoveDuplicate(Tolerance.Global);
        }

        /// <summary>
        ///     Removes duplicated points in the collection.
        /// </summary>
        /// <param name="pts">The instance to which the method applies.</param>
        /// <param name="tol">The tolerance to use in comparisons.</param>
        public static void RemoveDuplicate(this Point3dCollection pts, Tolerance tol)
        {
            var ptlst = new List<Point3d>();
            for (var i = 0; i < pts.Count; i++) ptlst.Add(pts[i]);
            ptlst.Sort((p1, p2) => p1.X.CompareTo(p2.X));
            for (var i = 0; i < ptlst.Count - 1; i++)
                for (var j = i + 1; j < ptlst.Count;)
                {
                    if (ptlst[j].X - ptlst[i].X > tol.EqualPoint)
                        break;
                    if (ptlst[i].IsEqualTo(ptlst[j], tol))
                    {
                        pts.Remove(ptlst[j]);
                        ptlst.RemoveAt(j);
                    }
                    else
                    {
                        j++;
                    }
                }
        }

        /// <summary>
        ///     Gets a value indicating whether the specified point belongs to the collection.
        /// </summary>
        /// <param name="pts">The instance to which the method applies.</param>
        /// <param name="pt">The point to search.</param>
        /// <returns>true if the point is found; otherwise, false.</returns>
        public static bool Contains(this Point3dCollection pts, Point3d pt)
        {
            return pts.Contains(pt, Tolerance.Global);
        }

        /// <summary>
        ///     Gets a value indicating whether the specified point belongs to the collection.
        /// </summary>
        /// <param name="pts">The instance to which the method applies.</param>
        /// <param name="pt">The point to search.</param>
        /// <param name="tol">The tolerance to use in comparisons.</param>
        /// <returns>true if the point is found; otherwise, false.</returns>
        public static bool Contains(this Point3dCollection pts, Point3d pt, Tolerance tol)
        {
            for (var i = 0; i < pts.Count; i++)
                if (pt.IsEqualTo(pts[i], tol))
                    return true;
            return false;
        }

        /// <summary>
        ///     Gets the extents 3d for the point collection.
        /// </summary>
        /// <param name="pts">The instance to which the method applies.</param>
        /// <returns>An Extents3d instance.</returns>
        /// <exception cref="ArgumentException">
        ///     ArgumentException is thrown if the collection is null or empty.
        /// </exception>
        public static Extents3d ToExtents3d(this Point3dCollection pts)
        {
            return pts.Cast<Point3d>().ToExtents3d();
        }

        /// <summary>
        ///     Gets the extents 3d for the point collection.
        /// </summary>
        /// <param name="pts">The instance to which the method applies.</param>
        /// <returns>An Extents3d instance.</returns>
        /// <exception cref="ArgumentException">
        ///     ArgumentException is thrown if the sequence is null or empty.
        /// </exception>
        public static Extents3d ToExtents3d(this IEnumerable<Point3d> pts)
        {
            if (pts == null || !pts.Any())
                throw new ArgumentException("Null or empty sequence");
            var pt = pts.First();
            return pts.Aggregate(new Extents3d(pt, pt), (e, p) =>
            {
                e.AddPoint(p);
                return e;
            });
        }
    }
}