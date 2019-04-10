// -----------------------------------------------------------------------------------
//     <copyright file="XmlReader.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>03/08/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using System.Xml.Linq;

namespace RabCab.Utilities.External.XmlAgent
{
    internal class XmlAgent
    {
        public string[] ReadXml(string xmlPath, string nodeName, string[] nodeAttributes)
        {
            XmlTextReader reader = new XmlTextReader(xmlPath);

            XmlDocument doc = new XmlDocument();
            XmlNode node = doc.ReadNode(reader);

            foreach (XmlNode item in node.ChildNodes)
            {
                Debug.WriteLine(item.Value);

                foreach (XmlAttribute att in item.Attributes)
                {
                    Debug.WriteLine(att.Value);
                }
            }

            return new string[0];
        }
    }
}