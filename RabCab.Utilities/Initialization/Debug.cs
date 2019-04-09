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
        [CommandMethod(CommandGroup, "Debug_CheckPrompts", CommandFlags.Modal)]
        public void DebugListCmdName()
        {
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            #region Prompt Angle Testing

            //Angle Prompt Testing
            var angleRadTest = acCurEd.GetRadian("\nEnter angle to test prompt: ");
            acCurEd.WriteMessage("\n" + angleRadTest);

            var angleDegTest = acCurEd.GetDegree("\nEnter angle to test prompt: ");
            acCurEd.WriteMessage("\n" + angleDegTest);

            //Angle Prompt Testing with Default Value
            var angleRadTest2 = acCurEd.GetRadian("\nEnter angle to test prompt: ", 90);
            acCurEd.WriteMessage("\n" + angleRadTest2);

            var angleDegTest2 = acCurEd.GetDegree("\nEnter angle to test prompt: ", 90);
            acCurEd.WriteMessage("\n" + angleDegTest2);

            #endregion

            #region Prompt Distance Testing

            //3D Distance Prompt Testing
            var distTest1 = acCurEd.GetAnyDistance("\nEnter any 3D distance: ");
            acCurEd.WriteMessage("\n" + distTest1);

            var distTest2 = acCurEd.GetAnyDistance(Point3d.Origin, "\nEnter any 3D distance: ");
            acCurEd.WriteMessage("\n" + distTest2);

            var distTest3 = acCurEd.GetPositiveDistance("\nEnter any positive 3D distance: ");
            acCurEd.WriteMessage("\n" + distTest3);

            var distTest4 = acCurEd.GetPositiveDistance(Point3d.Origin, "\nEnter any positive 3D distance: ");
            acCurEd.WriteMessage("\n" + distTest4);

            //2D Distance Prompt Testing
            var distTest5 = acCurEd.GetAny2DDistance("\nEnter any 2D distance: ");
            acCurEd.WriteMessage("\n" + distTest5);

            var distTest6 = acCurEd.GetAny2DDistance(Point3d.Origin, "\nEnter any 2D distance: ");
            acCurEd.WriteMessage("\n" + distTest6);

            var distTest7 = acCurEd.GetPositive2DDistance("\nEnter any positive 2D distance: ");
            acCurEd.WriteMessage("\n" + distTest7);

            var distTest8 = acCurEd.GetPositive2DDistance(Point3d.Origin, "\nEnter any positive 2D distance: ");
            acCurEd.WriteMessage("\n" + distTest8);

            //3D Distance Prompt Testing with Default Value
            var distTest9 = acCurEd.GetAnyDistance("\nEnter any 3D distance: ", 1);
            acCurEd.WriteMessage("\n" + distTest9);

            var distTest10 = acCurEd.GetAnyDistance(Point3d.Origin, "\nEnter any 3D distance: ", 1);
            acCurEd.WriteMessage("\n" + distTest10);

            var distTest11 = acCurEd.GetPositiveDistance("\nEnter any positive 3D distance: ", 1);
            acCurEd.WriteMessage("\n" + distTest11);

            var distTest12 = acCurEd.GetPositiveDistance(Point3d.Origin, "\nEnter any positive 3D distance: ", 1);
            acCurEd.WriteMessage("\n" + distTest12);

            //2D Distance Prompt Testing with Default Value
            var distTest13 = acCurEd.GetAny2DDistance("\nEnter any 2D distance: ", 1);
            acCurEd.WriteMessage("\n" + distTest13);

            var distTest14 = acCurEd.GetAny2DDistance(Point3d.Origin, "\nEnter any 2D distance: ", 1);
            acCurEd.WriteMessage("\n" + distTest14);

            var distTest15 = acCurEd.GetPositive2DDistance("\nEnter any positive 2D distance: ", 1);
            acCurEd.WriteMessage("\n" + distTest15);

            var distTest16 = acCurEd.GetPositive2DDistance(Point3d.Origin, "\nEnter any positive 2D distance: ", 1);
            acCurEd.WriteMessage("\n" + distTest16);

            #endregion
        }


        [CommandMethod(CommandGroup, "Debug_Try3ds", CommandFlags.Modal)]
        public void Try3ds() 
        {
            var proc = new _3DsMaxAgent();
            proc.Start3DsMax();
        }
    }
}
