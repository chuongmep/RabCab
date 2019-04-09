// -----------------------------------------------------------------------------------
//     <copyright file="3DsMaxAgent.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>03/28/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using System;
using System.Diagnostics;

namespace RabCab.Utilities.External._3dsMaxAgent
{
    internal class _3DsMaxAgent
    {
        public bool Start3DsMax()
        {
            var pFinder = new PathFinder();

            var _3dsPath = pFinder.GetAppPath("3ds Max 2018");

            var startInfo = new ProcessStartInfo(_3dsPath + "3dsmax.exe");
            startInfo.WindowStyle = ProcessWindowStyle.Maximized;
            startInfo.Arguments = "C:\\Drawing1.dwg";

            try
            {
                var _3dsProc = Process.Start(startInfo);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }
    }
}