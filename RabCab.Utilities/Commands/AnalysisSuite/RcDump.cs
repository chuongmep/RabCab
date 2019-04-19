using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using AcAp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RabCab.Commands.AnalysisSuite
{
    class RcDump
    {
        [CommandMethod("DUMPALLPROPS")]
        public void Dump()
        {
            Document doc = AcAp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            var entRes = ed.GetEntity("\nSelect object: ");
            if (entRes.Status == PromptStatus.OK)
            {
                PrintDump(entRes.ObjectId, ed);
                AcAp.DisplayTextScreen = true;
            }
        }

        [CommandMethod("DUMPCOMPROPS")]
        public static void ListComProps()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            PromptEntityOptions peo = new PromptEntityOptions("\nSelect object: ");
            var res = ed.GetEntity(peo);
            if (res.Status != PromptStatus.OK)
                return;
            using (Transaction tr = doc.TransactionManager.StartOpenCloseTransaction())
            {
                Entity ent = (Entity)tr.GetObject(res.ObjectId, OpenMode.ForRead);
                object acadObj = ent.AcadObject;
                var props = TypeDescriptor.GetProperties(acadObj);
                foreach (PropertyDescriptor prop in props)
                {
                    object value = prop.GetValue(acadObj);
                    if (value != null)
                    {
                        ed.WriteMessage("\n{0} = {1}", prop.DisplayName, value.ToString());
                    }
                }
                tr.Commit();
            }
        }

        private void PrintDump(ObjectId id, Editor ed)
        {
            var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

            using (var tr = id.Database.TransactionManager.StartTransaction())
            {
                var dbObj = tr.GetObject(id, OpenMode.ForRead);
                var types = new List<Type>();
                types.Add(dbObj.GetType());
                while (true)
                {
                    var type = types[0].BaseType;
                    types.Insert(0, type);
                    if (type == typeof(RXObject))
                        break;
                }
                foreach (Type t in types)
                {
                    ed.WriteMessage($"\n\n - {t.Name} -");
                    foreach (var prop in t.GetProperties(flags))
                    {
                        ed.WriteMessage("\n{0,-40}: ", prop.Name);
                        try
                        {
                            ed.WriteMessage("{0}", prop.GetValue(dbObj, null));
                        }
                        catch (System.Exception e)
                        {
                            ed.WriteMessage(e.Message);
                        }
                    }
                }
                tr.Commit();
            }
        }
    }
}
