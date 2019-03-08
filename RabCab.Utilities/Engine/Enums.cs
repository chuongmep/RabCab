using System.Diagnostics.CodeAnalysis;

namespace RabCab.Utilities.Engine
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class Enums
    {
        /// <summary>
        /// DXF Names for autoCAD entities
        /// </summary>       
        public enum DxfName
        {
            _3DFACE,
            _3DSOLID,
            ACAD_PROXY_ENTITY,
            ARC,
            ATTDEF,
            ATTRIB,
            BODY,
            CIRCLE,
            DIMENSION,
            ELLIPSE,
            HATCH,
            HELIX,
            IMAGE,
            INSERT,
            LEADER,
            LIGHT,
            LINE,
            LWPOLYLINE,
            MESH,
            MLINE,
            MLEADERSTYLE,
            MLEADER,
            MTEXT,
            OLEFRAME,
            OLE2FRAME,
            POINT,
            POLYLINE,
            RAY,
            REGION,
            SECTION,
            SEQEND,
            SHAPE,
            SOLID,
            SPLINE,
            SUN,
            SURFACE,
            TABLE,
            TEXT,
            TOLERANCE,
            TRACE,
            UNDERLAY,
            VERTEX,
            VIEWPORT,
            WIPEOUT,
            XLINE
        }
    }
}
