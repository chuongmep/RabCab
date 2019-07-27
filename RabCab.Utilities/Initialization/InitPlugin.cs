// -----------------------------------------------------------------------------------
//     <copyright file="InitPlugin.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>03/08/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using RabCab.Engine.AcSystem;
using RabCab.Initialization;
using RabCab.Settings;

// This line is not mandatory, but improves loading performances

[assembly: ExtensionApplication(typeof(InitPlugin))]

namespace RabCab.Initialization
{
    // This class is instantiated by AutoCAD once and kept alive for the 
    // duration of the session. If you don't do any one time initialization 
    // then you should remove this class.
    public class InitPlugin : IExtensionApplication
    {
        public static bool Activated = false;

        void IExtensionApplication.Initialize()
        {
            // Add one time initialization here
            // One common scenario is to setup a callback function here that 
            // unmanaged code can call. 
            // To do this:
            // 1. Export a function from unmanaged code that takes a function
            //    pointer and stores the passed in value in a global variable.
            // 2. Call this exported function in this function passing delegate.
            // 3. When unmanaged code needs the services of this managed module
            //    you simply call acrxLoadApp() and by the time acrxLoadApp 
            //    returns  global function pointer is initialized to point to
            //    the C# delegate.
            // For more info see: 
            // http://msdn2.microsoft.com/en-US/library/5zwkzwf4(VS.80).aspx
            // http://msdn2.microsoft.com/en-us/library/44ey4b32(VS.80).aspx
            // http://msdn2.microsoft.com/en-US/library/7esfatk4.aspx
            // as well as some of the existing AutoCAD managed apps.


            // Initialize your plug-in application here
            Application.DisplayingOptionDialog += Application_DisplayingOptionDialog;
            DocumentHandlers.AddDocEvents();
        }

        void IExtensionApplication.Terminate()
        {
            if (Activated)
            {
                Application.DisplayingOptionDialog -= Application_DisplayingOptionDialog;
                DocumentHandlers.RemoveDocEvents();
            }
        }

        #region Options Panel Addition

        private static SettingsGui _gSettings;

        private static void Application_DisplayingOptionDialog(object sender, TabbedDialogEventArgs e)
        {
            if (_gSettings == null)
                _gSettings = new SettingsGui();

            var tde = new TabbedDialogExtension(_gSettings, OnOK, OnCancel, OnHelp, OnApply);

            e.AddTab(SettingsInternal.CommandGroup, tde);
        }

        private static void OnOK()
        {
        }

        private static void OnCancel()
        {
        }

        private static void OnHelp()
        {
        }

        private static void OnApply()
        {
        }

        #endregion
    }
}