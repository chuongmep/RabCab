// -----------------------------------------------------------------------------------
//     <copyright file="Debugging.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>04/09/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using Autodesk.AutoCAD.Runtime;
using RabCab.External.XmlAgent;
using RabCab.Initialization;

// This line is not mandatory, but improves loading performances

[assembly: CommandClass(typeof(Debugging))]

namespace RabCab.Initialization
{
    // This class is instantiated by AutoCAD for each document when
    // a command is called by the user the first time in the context
    // of a given document. In other words, non static data in this class
    // is implicitly per-document!
    public static class Debugging
    {
        public static void Cmd_TestXml()
        {
            var reader = new XmlAgent();
            var cols =
                reader.GetXmlAttributes(
                    @"C:\Users\zayers\Documents\GitHub\RabCab.Utilities\RabCab.Utilities\Repository\RepoFasteners.xml",
                    "bolt");

            Sandbox.WriteLine("");
            Sandbox.WriteLine("Parsing Collection...");

            foreach (var attCol in cols)
            foreach (var att in attCol)
                Sandbox.WriteLine("  " + att.LocalName + " - " + att.Value);
        }
    }
}