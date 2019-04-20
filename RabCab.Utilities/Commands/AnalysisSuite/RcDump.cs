using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using AcAp = Autodesk.AutoCAD.ApplicationServices.Application;
using Exception = System.Exception;

namespace RabCab.Commands.AnalysisSuite
{
    internal class RcDump
    {
        [CommandMethod("DUMPALLPROPS")]
        public void Dump()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            var ed = doc.Editor;
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
            var doc = Application.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var peo = new PromptEntityOptions("\nSelect object: ");
            var res = ed.GetEntity(peo);
            if (res.Status != PromptStatus.OK)
                return;
            using (Transaction tr = doc.TransactionManager.StartOpenCloseTransaction())
            {
                var ent = (Entity) tr.GetObject(res.ObjectId, OpenMode.ForRead);
                var acadObj = ent.AcadObject;
                var props = TypeDescriptor.GetProperties(acadObj);
                foreach (PropertyDescriptor prop in props)
                {
                    var value = prop.GetValue(acadObj);
                    if (value != null) ed.WriteMessage("\n{0} = {1}", prop.DisplayName, value.ToString());
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

                foreach (var t in types)
                {
                    ed.WriteMessage($"\n\n - {t.Name} -");
                    foreach (var prop in t.GetProperties(flags))
                    {
                        ed.WriteMessage("\n{0,-40}: ", prop.Name);
                        try
                        {
                            ed.WriteMessage("{0}", prop.GetValue(dbObj, null));
                        }
                        catch (Exception e)
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