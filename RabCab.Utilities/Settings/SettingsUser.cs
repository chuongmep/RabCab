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

using Autodesk.AutoCAD.Colors;
using RabCab.Engine.Enumerators;
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
        public static Enums.Arrowhead ArwHead = Arrowhead._None;

        //Flatshot Options
        public static bool FlattenAssembly = false;
        public static bool FlattenAllSides = false;
        public static bool RetainHiddenLines = true;

        //Layers
        public static string RcVisible = "RCVisible";
        public static string RcHidden = "RcHidden";
        public static string RcAnno = "RcAnno";

        //Linetypes
        public static string RcVisibleLT = "CONTINUOUS";
        public static string RcHiddenLT = "HIDDEN";
        public static string RcAnnoLt = "CONTINUOUS";
        public static string RcDimLt = "CENTER";

        public static RoundTolerance UserTol { set; get; } = RoundTolerance.SixDecimals;
    }
}