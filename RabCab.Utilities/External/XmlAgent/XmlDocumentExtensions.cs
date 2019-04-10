using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RabCab.Utilities.External.XmlAgent
{
    public static class XmlDocumentExtensions
    {
        public static void IterateThroughAllNodes(
       this XmlDocument doc,
       Action<XmlNode> elementVisitor)
        {
            if (doc != null && elementVisitor != null)
            {
                foreach (XmlNode node in doc.ChildNodes)
                {
                    DoIterateNode(node, elementVisitor);
                }
            }
        }

        private static void DoIterateNode(
            XmlNode node,
            Action<XmlNode> elementVisitor)
        {
            elementVisitor(node);

            foreach (XmlNode childNode in node.ChildNodes)
            {
                DoIterateNode(childNode, elementVisitor);
            }
        }
    }
}
