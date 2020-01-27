﻿using System;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace RabCab.Extensions

{
    public static class LayoutExtensions

    {
        /// <summary>
        ///     Reverses the order of the X and Y properties of a Point2d.
        /// </summary>
        /// <param name="flip">Boolean indicating whether to reverse or not.</param>
        /// <returns>The original Point2d or the reversed version.</returns>
        public static Point2d SwapL(this Point2d pt, bool flip = true)

        {
            return flip ? new Point2d(pt.Y, pt.X) : pt;
        }


        /// <summary>
        ///     Pads a Point2d with a zero Z value, returning a Point3d.
        /// </summary>
        /// <param name="pt">The Point2d to pad.</param>
        /// <returns>The padded Point3d.</returns>
        public static Point3d Pad(this Point2d pt)

        {
            return new Point3d(pt.X, pt.Y, 0);
        }


        /// <summary>
        ///     Strips a Point3d down to a Point2d by simply ignoring the Z ordinate.
        /// </summary>
        /// <param name="pt">The Point3d to strip.</param>
        /// <returns>The stripped Point2d.</returns>
        public static Point2d Strip(this Point3d pt)

        {
            return new Point2d(pt.X, pt.Y);
        }


        /// <summary>
        ///     Creates a layout with the specified name and optionally makes it current.
        /// </summary>
        /// <param name="name">The name of the viewport.</param>
        /// <param name="select">Whether to select it.</param>
        /// <returns>The ObjectId of the newly created viewport.</returns>
        public static ObjectId CreateAndMakeLayoutCurrent(
            this LayoutManager lm, string name, bool select = true)
        {
            // First try to get the layout
            var id = lm.GetLayoutId(name);

            // If it doesn't exist, we create it
            if (!id.IsValid) id = lm.CreateLayout(name);
            
            // And finally we select it
            if (select) lm.CurrentLayout = name;

            return id;
        }

        /// <summary>
        ///     Creates a layout with the specified name and optionally makes it current.
        /// </summary>
        /// <param name="name">The name of the viewport.</param>
        /// <param name="select">Whether to select it.</param>
        /// <returns>The ObjectId of the newly created viewport.</returns>
        public static ObjectId CreateAndMakeLayoutCurrentByAddition(
            this LayoutManager lm, string name, bool select = true)
        {
            // First try to get the layout
            ObjectId id;
            var count = 0;
            var idValid = true;
            
            while (idValid)
            {
               
                id = lm.GetLayoutId(name + count);
                if (id.IsValid)
                {
                    count++;
                    continue;
                }

                idValid = false;
                name += count;
            }

            // If it doesn't exist, we create it
            id = lm.CreateLayout(name);

            // And finally we select it
            if (select) lm.CurrentLayout = name;

            return id;
        }


        /// <summary>
        ///     Applies an action to the specified viewport from this layout.
        ///     Creates a new viewport if none is found withthat number.
        /// </summary>
        /// <param name="tr">The transaction to use to open the viewports.</param>
        /// <param name="vpNum">The number of the target viewport.</param>
        /// <param name="f">The action to apply to each of the viewports.</param>
        public static void ApplyToViewport(
            this Layout lay, Transaction tr, int vpNum, Action<Viewport> f
        )

        {
            var vpIds = lay.GetViewports();

            Viewport vp = null;


            foreach (ObjectId vpId in vpIds)

            {
                var vp2 = tr.GetObject(vpId, OpenMode.ForWrite) as Viewport;

                if (vp2 != null && vp2.Number == vpNum)

                {
                    // We have found our viewport, so call the action


                    vp = vp2;

                    break;
                }
            }


            if (vp == null)

            {
                // We have not found our viewport, so create one


                var btr =
                    (BlockTableRecord) tr.GetObject(
                        lay.BlockTableRecordId, OpenMode.ForWrite
                    );


                vp = new Viewport();


                // Add it to the database


                btr.AppendEntity(vp);

                tr.AddNewlyCreatedDBObject(vp, true);


                // Turn it - and its grid - on


                vp.On = true;

                vp.GridOn = true;
            }


            // Finally we call our function on it


            f(vp);
        }


        /// <summary>
        ///     Apply plot settings to the provided layout.
        /// </summary>
        /// <param name="pageSize">The canonical media name for our page size.</param>
        /// <param name="styleSheet">The pen settings file (ctb or stb).</param>
        /// <param name="devices">The name of the output device.</param>
        public static void SetPlotSettings(
            this Layout lay, string pageSize, string styleSheet, string device
        )

        {
            using (var ps = new PlotSettings(lay.ModelType))

            {
                ps.CopyFrom(lay);


                var psv = PlotSettingsValidator.Current;


                // Set the device


                var devs = psv.GetPlotDeviceList();

                if (devs.Contains(device))

                {
                    psv.SetPlotConfigurationName(ps, device, null);

                    psv.RefreshLists(ps);
                }


                // Set the media name/size


                var mns = psv.GetCanonicalMediaNameList(ps);

                if (mns.Contains(pageSize)) psv.SetCanonicalMediaName(ps, pageSize);


                // Set the pen settings


                var ssl = psv.GetPlotStyleSheetList();

                if (ssl.Contains(styleSheet)) psv.SetCurrentStyleSheet(ps, styleSheet);


                // Copy the PlotSettings data back to the Layout


                var upgraded = false;

                if (!lay.IsWriteEnabled)

                {
                    lay.UpgradeOpen();

                    upgraded = true;
                }


                lay.CopyFrom(ps);


                if (upgraded) lay.DowngradeOpen();
            }
        }


        /// <summary>
        ///     Determine the maximum possible size for this layout.
        /// </summary>
        /// <returns>The maximum extents of the viewport on this layout.</returns>
        public static Extents2d GetMaximumExtents(this Layout lay)

        {
            // If the drawing template is imperial, we need to divide by

            // 1" in mm (25.4)


            var div = lay.PlotPaperUnits == PlotPaperUnit.Inches ? 25.4 : 1.0;


            // We need to flip the axes if the plot is rotated by 90 or 270 deg


            var doIt =
                lay.PlotRotation == PlotRotation.Degrees090 ||
                lay.PlotRotation == PlotRotation.Degrees270;


            // Get the extents in the correct units and orientation


            var min = SwapL(lay.PlotPaperMargins.MinPoint, doIt) / div;

            var max =
                (SwapL(lay.PlotPaperSize, doIt) -
                 SwapL(lay.PlotPaperMargins.MaxPoint, doIt).GetAsVector()) / div;


            return new Extents2d(min, max);
        }


        /// <summary>
        ///     Sets the size of the viewport according to the provided extents.
        /// </summary>
        /// <param name="ext">The extents of the viewport on the page.</param>
        /// <param name="fac">Optional factor to provide padding.</param>
        public static void ResizeViewport(
            this Viewport vp, Extents2d ext, double fac = 1.0
        )

        {
            vp.Width = (ext.MaxPoint.X - ext.MinPoint.X) * fac;

            vp.Height = (ext.MaxPoint.Y - ext.MinPoint.Y) * fac;

            vp.CenterPoint =
                (Point2d.Origin + (ext.MaxPoint - ext.MinPoint) * 0.5).Pad();
        }


        /// <summary>
        ///     Sets the view in a viewport to contain the specified model extents.
        /// </summary>
        /// <param name="ext">The extents of the content to fit the viewport.</param>
        /// <param name="fac">Optional factor to provide padding.</param>
        public static void FitContentToViewport(
            this Viewport vp, Extents3d ext, double fac = 1.0
        )

        {
            // Let's zoom to just larger than the extents


            vp.ViewCenter =
                (ext.MinPoint + (ext.MaxPoint - ext.MinPoint) * 0.5).Strip();


            // Get the dimensions of our view from the database extents


            var hgt = ext.MaxPoint.Y - ext.MinPoint.Y;

            var wid = ext.MaxPoint.X - ext.MinPoint.X;


            // We'll compare with the aspect ratio of the viewport itself

            // (which is derived from the page size)


            var aspect = vp.Width / vp.Height;


            // If our content is wider than the aspect ratio, make sure we

            // set the proposed height to be larger to accommodate the

            // content


            if (wid / hgt > aspect) hgt = wid / aspect;


            // Set the height so we're exactly at the extents


            vp.ViewHeight = hgt;


            // Set a custom scale to zoom out slightly (could also

            // vp.ViewHeight *= 1.1, for instance)


            vp.CustomScale *= fac;
        }

        /// <summary>
        ///     Sets the view in a viewport to contain the specified model extents.
        /// </summary>
        /// <param name="ext">The extents of the content to fit the viewport.</param>
        /// <param name="fac">Optional factor to provide padding.</param>
        public static void FitViewportToContent(
            this Viewport vp, Extents3d ext)
        {

            var scale = vp.CustomScale;

            // Get the dimensions of our view from the database extents
            var hgt = (ext.MaxPoint.Y - ext.MinPoint.Y) * vp.CustomScale;
            var wid = (ext.MaxPoint.X - ext.MinPoint.X) * vp.CustomScale;

            vp.Height = hgt;
            vp.Width = wid;

            vp.UpdateDisplay();

            vp.FindBestScale(ext);

            //vp.ViewHeight = 1.1;
        }
    }
}