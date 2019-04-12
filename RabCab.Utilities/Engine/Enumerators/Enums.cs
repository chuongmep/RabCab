// -----------------------------------------------------------------------------------
//     <copyright file="Enums.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>04/08/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;

namespace RabCab.Engine.Enumerators
{
    public static class Enums
    {
        /// <summary>
        /// Color codes for use in Cad Colors
        /// </summary>
        public enum CadColor
        {
            Red = 1,
            Yellow = 2,
            Green = 3,
            Cyan = 4,
            Blue = 5,
            Magenta = 6,
            White = 7,
            DarkGrey = 8,
            LightGrey = 9
        }

        /// <summary>
        ///     DXF Names for autoCAD entities
        /// </summary>
        [SuppressMessage("ReSharper", "IdentifierTypo")]
        public enum DxfNameEnum
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
        ///     Enum for setting round tolerance
        /// </summary>
        public enum RoundTolerance
        {
            NoDecimals = 0,
            OneDecimal = 1,
            TwoDecimals = 2,
            ThreeDecimals = 3,
            FourDecimals = 4,
            FiveDecimals = 5,
            SixDecimals = 6,
            SevenDecimals = 7,
            EightDecimals = 8,
            NineDecimals = 9,
            TenDecimals = 10
        }

        /// <summary>
        ///     Sub Object Selection Modes
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
        ///     Tile Mode Enum
        /// </summary>
        public enum TileModeEnum
        {
            Paperspace = 0,
            Modelspace = 1
        }

        /// <summary>
        /// Enum for determining texture direction in solids
        /// </summary>
        public enum TextureDirection
        {
            Unknown = -1,
            None = 0,
            Horizontal = 1,
            Vertical = 2,
            Diagonal = 3
        }

        /// <summary>
        /// Enum for determining production type
        /// </summary>
        public enum ProductionType
        {
            Unknown = -1,
            Box = 0,
            Sweep = 1,
            S4S = 2,
            MillingOneSide = 3,
            MillingManySide = 4          
        }

    }
}