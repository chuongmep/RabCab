// -----------------------------------------------------------------------------------
//     <copyright file="RegionExtensions.cs" company="CraterSpace">
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
    ///     Provides extension methods for the Region type.
    /// </summary>
    public static class RegionExtensions
    {
        /// <summary>
        ///     Gets the centroid of the region.
        /// </summary>
        /// <param name="reg">The instance to which the method applies.</param>
        /// <returns>The centroid of the region (WCS coordinates).</returns>
        public static Point3d Centroid(this Region reg)
        {
            using (var sol = new Solid3d())
            {
                sol.Extrude(reg, 2.0, 0.0);
                return sol.MassProperties.Centroid - reg.Normal;
            }
        }

        public static Solid3d Extrude(this Region reg)
        {
            var sol = new Solid3d();
            sol.Extrude(reg, 2.0, 0.0);
            return sol;
        }
    }
}