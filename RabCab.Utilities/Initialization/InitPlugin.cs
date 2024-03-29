﻿// -----------------------------------------------------------------------------------
//     <copyright file="InitPlugin.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>03/08/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using RabCab.Engine.AcSystem;
using RabCab.Entities.Controls;
using RabCab.Initialization;
using RabCab.Settings;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

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
        public static bool HasTime = false;
        public static bool FirstRun = true;

        private Splash _ss;

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

            ShowSplash();

            // Initialize your plug-in application here
            var actDia = new ActivationGui();
            actDia.Dispose();

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

        private void ShowSplash()
        {
            _ss = new Splash();

            // Rather than trusting these properties to be set
            // at design-time, let's set them here

            _ss.StartPosition = FormStartPosition.CenterScreen;
            _ss.FormBorderStyle = FormBorderStyle.None;
            _ss.Opacity = 0.8;
            _ss.TopMost = true;
            _ss.ShowInTaskbar = false;

            _ss.Opacity = 0;

            // Now let's disply the splash-screen
            Application.ShowModelessDialog(new AcadMainWindow(), _ss, false);
            _ss.Update();

            var opacityStep = 0.1;

            while (_ss.Opacity < 1)
            {
                _ss.Opacity += opacityStep;
                Thread.Sleep(10);
            }


            while (_ss.pBar.Value < _ss.pBar.Maximum)
            {
                _ss.pBar.Value += 1;
                Thread.Sleep(20);
            }

            Thread.Sleep(500);

            while (_ss.Opacity > 0)
            {
                _ss.Opacity -= opacityStep;
                Thread.Sleep(10);
            }

            // This is where your application should initialise,
            // but in our case let's take a 3-second nap

            _ss.Close();
        }

        #region Options Panel Addition

        private static SettingsGui _gSettings;

        private static void Application_DisplayingOptionDialog(object sender, TabbedDialogEventArgs e)
        {
            if (_gSettings == null)
                _gSettings = new SettingsGui();

            _gSettings.SetComp.UpdateGui();

            var tde = new TabbedDialogExtension(_gSettings, OnOK, OnCancel, OnHelp, OnApply);

            e.AddTab(SettingsInternal.CommandGroup, tde);
        }

        private static void OnOK()
        {
            _gSettings.SetComp.UpdateSettings();
        }

        private static void OnCancel()
        {
            //Do Nothing
        }

        private static void OnHelp()
        {
            Process.Start("http://www.rabcab.com/help");
        }

        private static void OnApply()
        {
            _gSettings.SetComp.UpdateSettings();
        }

        #endregion
    }
}