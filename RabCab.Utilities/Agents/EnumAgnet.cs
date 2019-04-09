// -----------------------------------------------------------------------------------
//     <copyright file="EnumAgnet.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>03/08/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace RabCab.Utilities.Agents
{
    /// <summary>
    ///     Static class used to get enums by their string name or int location
    /// </summary>
    public static class EnumAgnet
    {
        public static IEnumerable<T> GetValues<T>()
        {
            return Enum.GetValues(typeof (T)).Cast<T>();
        }

        /// Method to get enumeration value from string value.
        public static T GetEnumValue<T>(string str) where T : struct, IConvertible
        {
            if (!typeof (T).IsEnum) throw new Exception("T must be an Enumeration type.");
            var val = ((T[]) Enum.GetValues(typeof (T)))[0];
            if (!string.IsNullOrEmpty(str))
                foreach (var enumValue in (T[]) Enum.GetValues(typeof (T)))
                    if (enumValue.ToString().ToUpper().Equals(str.ToUpper()))
                    {
                        val = enumValue;
                        break;
                    }

            return val;
        }

        /// Method to get enumeration value from int value.
        public static T GetEnumValue<T>(int intValue) where T : struct, IConvertible
        {
            if (!typeof (T).IsEnum) throw new Exception("T must be an Enumeration type.");
            var val = ((T[]) Enum.GetValues(typeof (T)))[0];

            foreach (var enumValue in (T[]) Enum.GetValues(typeof (T)))
                if (Convert.ToInt32(enumValue).Equals(intValue))
                {
                    val = enumValue;
                    break;
                }

            return val;
        }
    }
}