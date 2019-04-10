// -----------------------------------------------------------------------------------
//     <copyright file="EllipseExtensions.cs" company="CraterSpace">
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
using RabCab.Entities.Linework;

namespace RabCab.Extensions
{
    /// <summary>
    ///     Provides extension methods for the Ellipse type.
    /// </summary>
    public static class EllipseExtensions
    {
        /// <summary>
        ///     Generates a polyline to approximate an ellipse.
        /// </summary>
        /// <param name="ellipse">The ellipse to be approximated</param>
        /// <returns>A new Polyline instance</returns>
        public static Polyline ToPolyline(this Ellipse ellipse)
        {
            var pline = new PolylineSegmentCollection(ellipse).ToPolyline();
            pline.Closed = ellipse.Closed;
            pline.Normal = ellipse.Normal;
            pline.Elevation =
                ellipse.Center.TransformBy(Matrix3d.WorldToPlane(new Plane(Point3d.Origin, ellipse.Normal))).Z;
            return pline;
        }
    }
}