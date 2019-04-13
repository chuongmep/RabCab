// -----------------------------------------------------------------------------------
//     <copyright file="XmlDocumentExtensions.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>04/10/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using System;
using System.Xml;

namespace RabCab.External.XmlAgent
{
    public static class XmlDocumentExtensions
    {
        public static void IterateThroughAllNodes(
            this XmlDocument doc,
            Action<XmlNode> elementVisitor)
        {
            if (doc != null && elementVisitor != null)
                foreach (XmlNode node in doc.ChildNodes)
                    DoIterateNode(node, elementVisitor);
        }

        private static void DoIterateNode(
            XmlNode node,
            Action<XmlNode> elementVisitor)
        {
            elementVisitor(node);

            foreach (XmlNode childNode in node.ChildNodes) DoIterateNode(childNode, elementVisitor);
        }
    }
}