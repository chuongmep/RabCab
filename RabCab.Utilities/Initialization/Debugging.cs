// -----------------------------------------------------------------------------------
//     <copyright file="Debug.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>04/09/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using System.Diagnostics;
using Autodesk.AutoCAD.Runtime;
using RabCab.Utilities.External;
using RabCab.Utilities.External.XmlAgent;
using RabCab.Utilities.Initialization;

// This line is not mandatory, but improves loading performances

[assembly: CommandClass(typeof (Debugging))]

namespace RabCab.Utilities.Initialization
{
    // This class is instantiated by AutoCAD for each document when
    // a command is called by the user the first time in the context
    // of a given document. In other words, non static data in this class
    // is implicitly per-document!
    internal class Debugging
    {
        [CommandMethod("TESTXML")]
        public void Cmd_TestXml()
        {
            var reader = new XmlAgent();
            reader.ReadXml(@"C:\Users\zayers\Documents\GitHub\RabCab.Utilities\RabCab.Utilities\Repository\RepoFasteners.xml", "", new []{""});
        }
    }
}