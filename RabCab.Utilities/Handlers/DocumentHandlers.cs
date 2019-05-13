// -----------------------------------------------------------------------------------
//     <copyright file="DocumentHandlers.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>03/08/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using Autodesk.AutoCAD.ApplicationServices;
using RabCab.Commands.PaletteKit;
using RabCab.Settings;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace RabCab.Handlers
{
    /// <summary>
    /// TODO
    /// </summary>
    internal static class DocumentHandlers
    {
        /// <summary>
        /// TODO
        /// </summary>
        internal static void AddDocEvents()
        {
            // Get the current document
            var acDocMan = Application.DocumentManager;

            acDocMan.DocumentToBeDeactivated +=  BeginDocClose;
            acDocMan.DocumentActivated += DocActivated;
            
        }

        /// <summary>
        /// TODO
        /// </summary>
        internal static void RemoveDocEvents()
        {
            // Get the current document
            var acDocMan = Application.DocumentManager;

            acDocMan.DocumentToBeDeactivated -= BeginDocClose;
            acDocMan.DocumentActivated -= DocActivated;
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="senderObj"></param>
        /// <param name="docBegClsEvtArgs"></param>
        private static void BeginDocClose(object senderObj,
            DocumentCollectionEventArgs docBegClsEvtArgs)
        {
           //TODO
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="senderObj"></param>
        /// <param name="docActEvent"></param>
        private static void DocActivated(object senderObj,
            DocumentCollectionEventArgs docActEvent)
        {
            //Notebook Handlers
            if (SettingsInternal.EnNotePal)
            {
                RcPaletteNotebook.UpdNotePal();
            }
        }
    }
}