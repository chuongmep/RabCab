// -----------------------------------------------------------------------------------
//     <copyright file="Point2dCollectionExtensions.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>03/08/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using System.Collections.Generic;
using Autodesk.AutoCAD.Geometry;

namespace RabCab.Extensions
{
    /// <summary>
    ///     Provides extension methods for the Point2dCollection type.
    /// </summary>
    public static class Point2dCollectionExtensions
    {
        /// <summary>
        ///     Removes duplicated points in the collection.
        /// </summary>
        /// <param name="pts">The instance to which the method applies.</param>
        public static void RemoveDuplicate(this Point2dCollection pts)
        {
            pts.RemoveDuplicate(Tolerance.Global);
        }

        /// <summary>
        ///     Removes duplicated points in the collection.
        /// </summary>
        /// <param name="pts">The instance to which the method applies.</param>
        /// <param name="tol">The tolerance to use in comparisons.</param>
        public static void RemoveDuplicate(this Point2dCollection pts, Tolerance tol)
        {
            var ptlst = new List<Point2d>();
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
        public static bool Contains(this Point2dCollection pts, Point2d pt)
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
        public static bool Contains(this Point2dCollection pts, Point2d pt, Tolerance tol)
        {
            for (var i = 0; i < pts.Count; i++)
                if (pt.IsEqualTo(pts[i], tol))
                    return true;
            return false;
        }
    }
}