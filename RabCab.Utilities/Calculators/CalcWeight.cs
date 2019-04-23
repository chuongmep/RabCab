// -----------------------------------------------------------------------------------
//     <copyright file="CalcWeight.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>03/08/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

namespace RabCab.Calculators
{
    internal static class CalcWeight
    {
        public static double GetWeightPerCuFt(this double mass, double weightPerCuFt) => mass * weightPerCuFt;
    }
}