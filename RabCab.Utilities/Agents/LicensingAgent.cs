// -----------------------------------------------------------------------------------
//     <copyright file="LicensingAgent.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>03/08/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using RabCab.Entities.Controls;
using RabCab.Initialization;
using AcAp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RabCab.Agents
{
    internal abstract class LicensingAgent
    {

        public static bool Check()
        {
            var actDia = new ActivationGui();

            if (!InitPlugin.Activated)
            {
                actDia.ShowDialog(new AcadMainWindow());

                if (InitPlugin.Activated)
                {
                    return true;
                }
                else
                {
                    if (InitPlugin.HasTime)
                    {                 
                        return true;
                    }
                    else
                    {
                        AcAp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nRabCab is not activated! Please enter an activation key to continue using the plugin!");
                        return false;
                    }        
                }
            }

            return InitPlugin.Activated;
        }
       
    }
}