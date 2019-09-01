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
        public static string ViewTemplatePath = string.Empty;

        public static string ExportTemplatePath =
            @"T:\Construction\Construction Standards\STAK Machining Standards\_Templates\MachineTemplate.dwt";

        //CSV Options
        public static string NamedPartsFileName = "PartList";

        //Save Version
        public static DwgVersion SaveVersion = DwgVersion.AC1800;

        //Palette Enablers
        public static bool EnableSelectionParse = true;
        public static double LeaderTextHeight = 0.09;

        public static string NetHomePage = "www.google.com";

        //BOM options
        public static string BomTitle = "Bill Of Materials";
        public static bool BomLayer = true;
        public static bool BomColor = true;
        public static bool BomName = true;
        public static bool BomWidth = true;
        public static bool BomLength = true;
        public static bool BomThickness = true;
        public static bool BomVolume = true;
        public static bool BomTextureDirection = true;
        public static bool BomProductionType = true;
        public static bool BomQty = true;

        //SortingOptions
        public static bool ResetPartCount = true;
        public static string NamingConvention = string.Empty;
        public static bool SortByLayer = false;
        public static bool SortByColor = false;
        public static bool SortByThickness = true;
        public static bool SortByName = true;
        public static bool MixS4S = false;

        //Explode Options
        public static double ExplodePower = 2;

        //Mark Options
        public static bool DeleteExistingMarks = true;
        public static double MarkTextHeight = 0.09;

        //Laying Options
        public static int LayStep = 10;
        public static double LayTextHeight = 2;
        public static bool LayTextAbove = true;
        public static bool LayTextInside = false;
        public static bool LayTextLeft = true;
        public static bool LayTextCenter = false;
        public static bool LayFlatShot = true;
        public static bool LayAllSidesFlatShot = true;
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
        public static string RcWelds = "RCWelds";

        //Linetypes
        public static string RcVisibleLT = "CONTINUOUS";
        public static string RcHiddenLT = "HIDDEN";
        public static string RcAnnoLt = "CONTINUOUS";
        public static string RcDimLt = "CENTER";
        public static string RcWeldsLt = "HIDDEN";

        //Carpentry
        public static double RcJointDepth = 0;
        public static double RcOffsetDepth = 0;
        public static double RcSliceDepth = 0;
        public static double RcGapDepth = 0;
        public static double DogEarDiam = 0.50;
        public static double RcChopDepth = 0;
        public static double RcICutDepth = 0;
        public static double RcICutInset = 0;
        public static double WeldBeadSize = 0;
        public static double LaminateThickness = 0;
        public static double EdgeBandThickness = 0;

        //Page Number Options
        public static string PageNoOf = "PAGE";
        public static string PageNoTotal = "PAGECOUNT";


        //Weld Symbol Settings
        public static double WeldSymbolLength = 1;

        //AutoLayer Options
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
            "GENDIMS",
            "PARTLEADER"
        };

        //Rounding Options
        public static Enums.RoundTolerance UserTol { set; get; } = Enums.RoundTolerance.SixDecimals;
    }
}