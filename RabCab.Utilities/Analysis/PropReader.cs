using System.Collections.Generic;
using System.ComponentModel;
using Autodesk.AutoCAD.DatabaseServices;

namespace RabCab.Analysis
{
    public static class PropReader
    {
        public static List<PropertyDescriptor> GetProps(this Entity acEnt)
        {
            var propList = new List<PropertyDescriptor>();

            var acadObj = acEnt.AcadObject;
            var props = TypeDescriptor.GetProperties(acadObj);

            foreach (PropertyDescriptor prop in props)
            {
                var value = prop.GetValue(acadObj);
                if (value != null) propList.Add(prop);
            }

            return propList;
        }
    }
}