// -----------------------------------------------------------------------------------
//     <copyright file="SettingsUser.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>03/08/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using System.Collections.Generic;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using RabCab.Engine.Enumerators;

namespace RabCab.Settings
{
    public static class SettingsUser
    {
        //User Options
        public static double TolPoint = 0.004;

        public static bool ManageLayers = false;
        public static bool AllowSymbols = false;
        public static bool KeepSelection = false;
        public static bool PrioritizeRightAngles = false;
        public static bool UseFields = false;

        //PartFinder
        public static bool PartLeaderEnabled = true;

        //Automation
        public static bool AutoLayerEnabled = true;

        //TableOptions
        public static double TableRowHeight = .035;
        public static double TableColumnWidth = 0.5;
        public static double TableTextHeight = 0.09;
        public static double TableXOffset = 0.125;
        public static double TableYOffset = 0.125;
        public static Enums.AttachmentPoint TableAttach = Enums.AttachmentPoint.TopRight;

        //External Paths
        public static string ViewTemplatePath = "";

        public static string ExportTemplatePath =
            @"T:\Construction\Construction Standards\STAK Machining Standards\_Templates\MachineTemplate.dwt";

        //CSV Options
        public static string NamedPartsFileName = "PartList";

        //Save Version
        public static DwgVersion SaveVersion = DwgVersion.AC1800;

        //Palette Enablers
        public static bool EnableSelectionParse = true;
        public static string NetHomePage = "www.google.com";

        //SortingOptions
        public static bool ResetPartCount = true;
        public static string NamingConvention = "";
        public static bool SortByLayer = false;
        public static bool SortByColor = false;
        public static bool SortByThickness = true;
        public static bool SortByName = true;
        public static bool GroupSame = true;
        public static bool SplitByLayer = true;
        public static bool MixS4S = false;

        //Explode Options
        public static double ExplodePower = 2;

        //Mark Options
        public static double MarkTextHeight = 0.09;

        //Laying Options
        public static int LayStep = 10;
        public static double LayTextHeight = 2;
        public static bool LayTextAbove = true;
        public static bool LayTextInside = false;
        public static bool LayTextLeft = true;
        public static bool LayTextCenter = false;
        public static bool LayFlatShot = false;
        public static bool LayAllSidesFlatShot = false;
        public static bool PromptForMultiplication = true;

        //Annotation Options
        public static double ViewSpacing = 0;
        public static Color DynPreviewColor = Colors.LayerColorPreview;
        public static Enums.Arrowhead ArwHead = Enums.Arrowhead._None;
        public static double AnnoSpacing = 0.3125;

        //Flatshot Options
        public static bool FlattenAssembly = false;
        public static bool FlattenAllSides = false;
        public static bool RetainHiddenLines = true;

        //Layers
        public static char LayerDelimiter = '-';
        public static string RcVisible = "RCVisible";
        public static string RcHidden = "RCHidden";
        public static string RcAnno = "RCAnno";
        public static string RcHoles = "RCHoles";

        //Linetypes
        public static string RcVisibleLT = "CONTINUOUS";
        public static string RcHiddenLT = "HIDDEN";
        public static string RcAnnoLt = "CONTINUOUS";
        public static string RcDimLt = "CENTER";
        public static string RcHolesLt = "HIDDEN";

        //Carpentry
        public static double RcJointDepth = 0;
        public static double RcOffsetDepth = 0;
        public static double RcSliceDepth = 0;
        public static double RcGapDepth = 0;
        public static double DogEarDiam = 0.50;

        public static List<string> LayerCommandList { get; set; } = new List<string>
        {
            "TEXT",
            "DTEXT",
            "MTEXT",
            "MLEADER",
            "DIM",
            "DIMLINEAR",
            "DIMALIGNED",
            "DIMANGULAR",
            "DIMARC",
            "DIMRADIUS",
            "DIMDIAMETER",
            "DIMJOGGED",
            "DIMORDINATE",
            "QDIM",
            "DIMCONTINUE",
            "DIMBASELINE",
            "TABLE",
            "GENDIMS"
        };

        public static Enums.RoundTolerance UserTol { set; get; } = Enums.RoundTolerance.SixDecimals;
    }
}