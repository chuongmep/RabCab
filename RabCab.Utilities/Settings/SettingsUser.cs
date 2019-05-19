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
using System.Windows.Documents;
using Autodesk.AutoCAD.Colors;
using static RabCab.Engine.Enumerators.Enums;

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

        public static List<string> LayerCommandList { get; set; } = new List<string>()
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
            "TABLE"
        };

        //External Paths
        public static string ViewTemplatePath = "";

        //Palette Enablers
        public static bool EnableSelectionParse = true;
        public static string NetHomePage = "www.google.com";

        //SortingOptions
        public static bool ResetPartCount = true;
        public static string NamingConvention = "";
        public static bool SortByLayer = true;
        public static bool SortByColor = false;
        public static bool SortByThickness = true;
        public static bool SortByName = true;
        public static bool GroupSame = true;
        public static bool SplitByLayer = true;
        public static bool MixS4S = false;

        //Explode Options
        public static double ExplodePower = 2;

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
        public static Arrowhead ArwHead = Arrowhead._None;
        public static double AnnoSpacing = 0.3125;

        //Flatshot Options
        public static bool FlattenAssembly = false;
        public static bool FlattenAllSides = false;
        public static bool RetainHiddenLines = true;

        //Layers
        public static char LayerDelimiter = '-';
        public static string RcVisible = "RCVisible";
        public static string RcHidden = "RCHidden";
        public static string RcAnno = "RCAnnotation";

        //Linetypes
        public static string RcVisibleLT = "CONTINUOUS";
        public static string RcHiddenLT = "HIDDEN";
        public static string RcAnnoLt = "CONTINUOUS";
        public static string RcDimLt = "CENTER";

        //Carpentry
        public static double RcJointDepth = 0;
        public static double RcOffsetDepth = 0;
        public static double RcSliceDepth = 0;
        public static double RcGapDepth = 0;

        public static RoundTolerance UserTol { set; get; } = RoundTolerance.SixDecimals;
    }
}