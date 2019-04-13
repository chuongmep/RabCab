// -----------------------------------------------------------------------------------
//     <copyright file="Polyline3dExtensions.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>03/08/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace RabCab.Extensions
{
    /// <summary>
    ///     Provides extension methods for the Polyline3d type.
    /// </summary>
    public static class Polyline3dExtensions
    {
        /// <summary>
        ///     Creates a new Polyline that is the result of projecting the Polyline3d parallel to 'direction' onto 'plane' and
        ///     returns it.
        /// </summary>
        /// <param name="pline">The polyline to project.</param>
        /// <param name="plane">The plane onto which the curve is to be projected.</param>
        /// <param name="direction">Direction (in WCS coordinates) of the projection.</param>
        /// <returns>The projected Polyline.</returns>
        public static Polyline GetProjectedPolyline(this Polyline3d pline, Plane plane, Vector3d direction)
        {
            if (plane.Normal.IsPerpendicularTo(direction, new Tolerance(1e-9, 1e-9)))
                return null;

            return GeomExt.ProjectPolyline(pline, plane, direction);
        }

        /// <summary>
        ///     Creates a new Polyline that is the result of projecting the Polyline3d along the given plane.
        /// </summary>
        /// <param name="pline">The polyline to project.</param>
        /// <param name="plane">The plane onto which the curve is to be projected.</param>
        /// <returns>The projected polyline</returns>
        public static Polyline GetOrthoProjectedPolyline(this Polyline3d pline, Plane plane)
        {
            return pline.GetProjectedPolyline(plane, plane.Normal);
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public static bool IsNull(this Point3d pt)
        {
            return pt == new Point3d();
        }
    }
}