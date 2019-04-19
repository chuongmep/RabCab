using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;

namespace RabCab.Analysis
{
    public static class PropReader
    {
        public static List<PropertyDescriptor> GetProps(this Entity acEnt)
        {
            var propList = new List<PropertyDescriptor>();

            object acadObj = acEnt.AcadObject;
            var props = TypeDescriptor.GetProperties(acadObj);

            foreach (PropertyDescriptor prop in props)
            {
                object value = prop.GetValue(acadObj);
                if (value != null)
                {
                    propList.Add(prop);
                }
            }

            return propList;
        }
    }
}
