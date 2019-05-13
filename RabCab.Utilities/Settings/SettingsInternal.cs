// -----------------------------------------------------------------------------------
//     <copyright file="SettingsInternal.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>03/08/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

namespace RabCab.Settings
{
    internal class SettingsInternal
    {
        //Setting For Command Methods
        internal const string CommandGroup = "RABCAB";
        internal const string VariesTxt = "*VARIES*";

        //Settings for Tolerance
        internal static double TolVector = 0.0017453292519943296;

        //Palette Init
        internal static bool EnDwgPal = true;
        internal static bool EnLayerPal = true;
        internal static bool EnMetPal = true;
        internal static bool EnNotePal = true;
        internal static bool EnSnapPal = true;

    }
}