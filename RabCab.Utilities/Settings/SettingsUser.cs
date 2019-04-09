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

using RabCab.Utilities.Engine.Enumerators;

namespace RabCab.Utilities.Settings
{
    public static class SettingsUser
    {
        public static Enums.RoundTolerance UserTol { set; get; } = Enums.RoundTolerance.ThreeDecimals;
    }
}