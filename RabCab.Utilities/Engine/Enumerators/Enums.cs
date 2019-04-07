using System.Diagnostics.CodeAnalysis;

namespace RabCab.Utilities.Engine.Enumerators
{
    public static class Enums
    {
        /// <summary>
        /// DXF Names for autoCAD entities
        /// </summary>       
        [SuppressMessage("ReSharper", "IdentifierTypo")]
        public enum DxfName
        {
            _3Dface,
            _3Dsolid,
            AcadProxyEntity,
            Arc,
            Attdef,
            Attrib,
            Body,
            Circle,
            Dimension,
            Ellipse,
            Hatch,
            Helix,
            Image,
            Insert,
            Leader,
            Light,
            Line,
            Lwpolyline,
            Mesh,
            Mline,
            Mleaderstyle,
            Mleader,
            Mtext,
            Oleframe,
            Ole2Frame,
            Point,
            Polyline,
            Ray,
            Region,
            Section,
            Seqend,
            Shape,
            Solid,
            Spline,
            Sun,
            Surface,
            Table,
            Text,
            Tolerance,
            Trace,
            Underlay,
            Vertex,
            Viewport,
            Wipeout,
            Xline
        }

        /// <summary>
        /// Sub Object Selection Modes
        /// </summary>
        public enum SubObjEnum
        {
            NoFilter = 0,
            Vertex = 1,
            Edge = 2,
            Face = 3,
            History = 4,
            View = 5
        }

        /// <summary>
        /// Tile Mode Enum
        /// </summary>
        public enum TileModeEnum
        {
            Paperspace = 0,
            Modelspace = 1
        }
    }
}
