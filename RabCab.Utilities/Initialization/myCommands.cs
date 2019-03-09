// (C) Copyright 2019 by  
//

using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using RabCab.Utilities.Extensions;
using RabCab.Utilities.Initialization;
using static RabCab.Utilities.Settings.SettingsInternal;

// This line is not mandatory, but improves loading performances

[assembly: CommandClass(typeof (MyCommands))]

namespace RabCab.Utilities.Initialization
{
    // This class is instantiated by AutoCAD for each document when
    // a command is called by the user the first time in the context
    // of a given document. In other words, non static data in this class
    // is implicitly per-document!
    public class MyCommands
    {
        #region Internal & Debug Commands

        [CommandMethod(CommandGroup, "Debug_CheckPrompts", CommandFlags.Modal)]
        public void DebugListCmdName() // This method can have any name
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

        #endregion

        #region Templates

        // The CommandMethod attribute can be applied to any public  member 
        // function of any public class.
        // The function should take no arguments and return nothing.
        // If the method is an instance member then the enclosing class is 
        // instantiated for each document. If the member is a static member then
        // the enclosing class is NOT instantiated.
        //
        // NOTE: CommandMethod has overloads where you can provide helpId and
        // context menu.

        // Modal Command with localized name
        [CommandMethod("MyGroup", "MyCommand", "MyCommandLocal", CommandFlags.Modal)]
        public void MyCommand() // This method can have any name
        {
            // Put your command code here
            var doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return;

            var ed = doc.Editor;
            ed.WriteMessage("Hello, this is your first command.");
        }

        // Modal Command with pickFirst selection
        [CommandMethod("MyGroup", "MyPickFirst", "MyPickFirstLocal", CommandFlags.Modal | CommandFlags.UsePickSet)]
        public void MyPickFirst() // This method can have any name
        {
            var result = Application.DocumentManager.MdiActiveDocument.Editor.GetSelection();
            if (result.Status == PromptStatus.OK)
            {
                // There are selected entities
                // Put your command using pickFirst set code here
            }
        }

        // Application Session Command with localized name
        [CommandMethod("MyGroup", "MySessionCmd", "MySessionCmdLocal", CommandFlags.Modal | CommandFlags.Session)]
        public void MySessionCmd() // This method can have any name
        {
            // Put your command code here
        }

        // LispFunction is similar to CommandMethod but it creates a lisp 
        // callable function. Many return types are supported not just string
        // or integer.
        [LispFunction("MyLispFunction", "MyLispFunctionLocal")]
        public int MyLispFunction(ResultBuffer args) // This method can have any name
        {
            // Put your command code here

            // Return a value to the AutoCAD Lisp Interpreter
            return 1;
        }

        #endregion
    }
}
