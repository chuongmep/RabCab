using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using RabCab.Utilities.Extensions;
using static RabCab.Utilities.Settings.SettingsInternal;
using RabCab.Utilities.External._3dsMaxAgent;
using RabCab.Utilities.Initialization;

// This line is not mandatory, but improves loading performances
[assembly: CommandClass(typeof(Debug))]

namespace RabCab.Utilities.Initialization
{
    // This class is instantiated by AutoCAD for each document when
    // a command is called by the user the first time in the context
    // of a given document. In other words, non static data in this class
    // is implicitly per-document!
    class Debug
    {
       
    }
}
