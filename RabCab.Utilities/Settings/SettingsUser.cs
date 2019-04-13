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
        public static Enums.RoundTolerance UserTol { set; get; } = Enums.RoundTolerance.SixDecimals;
    }
}