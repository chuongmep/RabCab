using Autodesk.AutoCAD.Geometry;

namespace RabCab.Extensions
{
    /// <summary>
    ///     TODO
    /// </summary>
    public static class Vector2DExtensions
    {
        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static Point2d ConvertToPoint(this Vector2d vector)
        {
            return new Point2d(vector.X, vector.Y);
        }
    }
}