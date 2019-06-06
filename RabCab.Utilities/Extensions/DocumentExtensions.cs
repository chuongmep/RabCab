// -----------------------------------------------------------------------------------
//     <copyright file="DocumentExtensions.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>03/08/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Windows;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace RabCab.Extensions
{
    internal static class DocumentExtensions
    {
        public static void PurgeAll(this Document doc)
        {
            if (doc == null) return;

            object adoc = null;
            if (Application.Version.Major < 19)
            {
                // For AutoCAD 2012- (i.e. 2012, 2011, 2010, ...)
                // before 2013, AcadDocument was a property
                adoc = doc.GetType().InvokeMember("AcadDocument", BindingFlags.GetProperty, null, doc, null);
            }
            else
            {
                // For AutoCAD 2013+ (i.e. 2013, 2014, ...)
                // starting from 2013 is a method

                var ext = typeof(Menu).Assembly.GetType("Autodesk.AutoCAD.ApplicationServices.DocumentExtension", true);
                if (ext != null)
                {
                    var GetAcadDocumentMethod =
                        ext.GetMethod("GetAcadDocument", BindingFlags.Public | BindingFlags.Static);

                    if (GetAcadDocumentMethod != null)
                        adoc = GetAcadDocumentMethod.Invoke(doc, new object[1] {doc});

                    // if you don't care about previous version, this is standard method 
                    // object adoc = Application.DocumentManager.MdiActiveDocument.GetAcadDocument(); 
                }

                if (adoc != null) adoc.GetType().InvokeMember("PurgeAll", BindingFlags.InvokeMethod, null, adoc, null);
            }
        }
    }
}