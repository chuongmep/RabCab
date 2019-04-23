// -----------------------------------------------------------------------------------
//     <copyright file="XmlAgent.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>03/08/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Xml;
using RabCab.Initialization;

namespace RabCab.External.XmlAgent
{
    internal class XmlAgent
    {
        public List<XmlAttribute[]> GetXmlAttributes(string xmlPath, string nodeName)
        {
            //Create a list to hold all attribute groups
            var xmlAtts = new List<XmlAttribute[]>();

            //Tell Console we are reading the XML
            Sandbox.WriteLine("Reading XML: " + Path.GetFileName(xmlPath));

            //Create a new XmlDocument for reading - and load it
            var doc = new XmlDocument();
            doc.Load(xmlPath);

            //Iterate through all nodes of the XML to find the information we are looking for
            doc.IterateThroughAllNodes(delegate(XmlNode node)
            {
                //Check if the node has attributes - we only want nodes with attributes
                if (node.Attributes == null) return;
                if (node.LocalName != nodeName) return;

                //If we have found a node with attributes, write the nodes name to the console
                //Sandbox.WriteLine(node.LocalName);

                //Create a list to hold the attribute values
                var attributeList = new List<XmlAttribute>();

                //Iterate through the attributes - if any match the specified 'attributesToGet' - replace that att with its value
                foreach (XmlAttribute att in node.Attributes)
                {
                    attributeList.Add(att);
                    //Sandbox.WriteLine("  " + att.LocalName + " - " + att.Value);
                }

                //Add the attribute collection to the returnable collection
                xmlAtts.Add(attributeList.ToArray());
            });

            //Return the attribute collections
            return xmlAtts;
        }
    }
}