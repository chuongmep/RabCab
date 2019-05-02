// -----------------------------------------------------------------------------------
//     <copyright file="TransientAgent.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>03/08/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;

namespace RabCab.Agents
{
    internal static class TransientAgent
    {
        private static readonly List<Drawable> _transients = new List<Drawable>();

        /// <summary>
        ///     TODO
        /// </summary>
        public static void Clear()
        {
            var acCurTm = TransientManager.CurrentTransientManager;
            var intCol = new IntegerCollection();

            foreach (var tempGraphic in _transients) acCurTm.EraseTransient(tempGraphic, intCol);

            _transients.Clear();
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="ents"></param>
        public static void Add(Entity[] ents)
        {
            Clear();

            foreach (var ent in ents)
                if (ent != null)
                    _transients.Add(ent);
        }

        public static void Update(Point3d curPt, Point3d moveToPt)
        {
            // Displace each of our drawables
            var mat = Matrix3d.Displacement(curPt.GetVectorTo(moveToPt));

            // Update their graphics
            foreach (var d in _transients)
            {
                var e = d as Entity;
                e.TransformBy(mat);
                TransientManager.CurrentTransientManager.UpdateTransient(
                    d, new IntegerCollection()
                );
            }
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="ents"></param>
        public static void Add(Entity ent)
        {
            Clear();

            if (ent != null) _transients.Add(ent);
        }

        /// <summary>
        ///     TODO
        /// </summary>
        public static void Draw()
        {
            var acCurTm = TransientManager.CurrentTransientManager;
            var intCol = new IntegerCollection();

            foreach (var transient in _transients)
                acCurTm.AddTransient(transient, TransientDrawingMode.DirectTopmost, 0, intCol);
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <returns></returns>
        public static double GetPenWidth()
        {
            return (double) Application.GetSystemVariable("VIEWSIZE") /
                   250.0;
        }
    }
}