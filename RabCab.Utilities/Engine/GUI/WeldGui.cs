using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using RabCab.Agents;
using RabCab.Calculators;
using RabCab.Extensions;
using RabCab.Settings;
using UCImageCombo;
using static Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace RabCab.Engine.GUI
{
    public partial class WeldGui : Form
    {
        public List<Entity> drawnEnts = new List<Entity>();
        public bool LeftFacing = false;
        public Point3d SymEnd;
        public Point3d SymStart;

        public WeldGui(Point3d sStart, Point3d sEnd)
        {
            InitializeComponent();

            SymStart = sStart;
            SymEnd = sEnd;

            foreach (Control c in groupBox1.Controls)
                switch (c)
                {
                    case CheckBox box:
                        box.CheckedChanged += c_ControlChanged;
                        break;
                    case ComboBox combo:
                        combo.SelectedIndexChanged += c_ControlChanged;
                        break;
                    default:
                        c.TextChanged += c_ControlChanged;
                        break;
                }
        }

        private void WeldGui_Load(object sender, EventArgs e)
        {
            WeldType_T.Items.Add(new ImageComboItem("None", 0));
            WeldType_T.Items.Add(new ImageComboItem("Fillet", 1));
            WeldType_T.Items.Add(new ImageComboItem("Plug", 2));
            WeldType_T.Items.Add(new ImageComboItem("Spot", 3));
            WeldType_T.Items.Add(new ImageComboItem("Seam", 4));
            WeldType_T.Items.Add(new ImageComboItem("Backing", 5));
            //WeldType_T.Items.Add(new ImageComboItem("Melt Thru", 6));
            //WeldType_T.Items.Add(new ImageComboItem("Flange Edge", 7));
            //WeldType_T.Items.Add(new ImageComboItem("Flange Corner", 8));
            //WeldType_T.Items.Add(new ImageComboItem("Square Groove", 9));
            //WeldType_T.Items.Add(new ImageComboItem("V Groove", 10));

            WeldType_B.Items.Add(new ImageComboItem("None", 0));
            WeldType_B.Items.Add(new ImageComboItem("Fillet", 1));
            WeldType_B.Items.Add(new ImageComboItem("Plug", 2));
            WeldType_B.Items.Add(new ImageComboItem("Spot", 3));
            WeldType_B.Items.Add(new ImageComboItem("Seam", 4));
            WeldType_B.Items.Add(new ImageComboItem("Backing", 5));
            //WeldType_B.Items.Add(new ImageComboItem("Melt Thru", 6));
            //WeldType_B.Items.Add(new ImageComboItem("Flange Edge", 7));
            //WeldType_B.Items.Add(new ImageComboItem("Flange Corner", 8));
            //WeldType_B.Items.Add(new ImageComboItem("Square Groove", 9));
            //WeldType_B.Items.Add(new ImageComboItem("V Groove", 10));

            Contour_B.Items.Add(new ImageComboItem("None", 0));
            Contour_B.Items.Add(new ImageComboItem("Concave", 1));
            Contour_B.Items.Add(new ImageComboItem("Flush", 2));
            Contour_B.Items.Add(new ImageComboItem("Convex", 3));

            Contour_T.Items.Add(new ImageComboItem("None", 0));
            Contour_T.Items.Add(new ImageComboItem("Concave", 1));
            Contour_T.Items.Add(new ImageComboItem("Flush", 2));
            Contour_T.Items.Add(new ImageComboItem("Convex", 3));

            //IdCombo.Items.Add(new ImageComboItem("No ID", 0));
            //IdCombo.Items.Add(new ImageComboItem("ID on Top", 1));
            //IdCombo.Items.Add(new ImageComboItem("ID on Bottom", 2));

            //StaggerCombo.Items.Add(new ImageComboItem("No Stagger", 0));
            //StaggerCombo.Items.Add(new ImageComboItem("Move", 1));
            //StaggerCombo.Items.Add(new ImageComboItem("Mirror", 2));

            Method_B.Items.Add(new ImageComboItem("None", 0));
            Method_B.Items.Add(new ImageComboItem("Chipping", 1));
            Method_B.Items.Add(new ImageComboItem("Grinding", 2));
            Method_B.Items.Add(new ImageComboItem("Hammering", 3));
            Method_B.Items.Add(new ImageComboItem("Machining", 4));
            Method_B.Items.Add(new ImageComboItem("Rolling", 5));

            Method_T.Items.Add(new ImageComboItem("None", 0));
            Method_T.Items.Add(new ImageComboItem("Chipping", 1));
            Method_T.Items.Add(new ImageComboItem("Grinding", 2));
            Method_T.Items.Add(new ImageComboItem("Hammering", 3));
            Method_T.Items.Add(new ImageComboItem("Machining", 4));
            Method_T.Items.Add(new ImageComboItem("Rolling", 5));

            WeldType_T.SelectedIndex = 0;
            WeldType_B.SelectedIndex = 0;
            //IdCombo.SelectedIndex = 0;
            //StaggerCombo.SelectedIndex = 0;
            Focus();
        }

        #region Draw Transients

        private void c_ControlChanged(object sender, EventArgs e)
        {
            TransientAgent.Clear();

            foreach (var ent in drawnEnts) ent.Dispose();

            drawnEnts.Clear();

            //TODO add all drawables here

            //Draw weld symbol

            #region TopWeldSymbol

            var index_T = WeldType_T.SelectedIndex;
            var cIndex_T = Contour_T.SelectedIndex;
            var mIndex_T = Method_T.SelectedIndex;

            var index_B = WeldType_B.SelectedIndex;
            var cIndex_B = Contour_B.SelectedIndex;
            var mIndex_B = Method_B.SelectedIndex;

            var dDimScale = (double) GetSystemVariable("DIMSCALE");
            var symbolLength = SettingsUser.WeldSymbolLength * dDimScale;

            //Draw Symbol
            var mPoint = SymStart.GetMidPoint(SymEnd);
            var fLineLength = CalcUnit.GetProportion(.2, 1, symbolLength);
            var fLineHalf = fLineLength / 2;
            var arcRad = CalcUnit.GetProportion(.2, 1, symbolLength);

            switch (index_T)
            {
                case 1: //Fillet
                    var fLinePt1 = new Point2d(mPoint.X, mPoint.Y);
                    var fLinePt2 = new Point2d(fLinePt1.X, fLinePt1.Y + fLineLength);
                    var fLinePt3 = new Point2d(fLinePt1.X + fLineLength, fLinePt1.Y);

                    var fLine = new Polyline(3);
                    fLine.AddVertexAt(0, fLinePt1, 0, 0, 0);
                    fLine.AddVertexAt(0, fLinePt2, 0, 0, 0);
                    fLine.AddVertexAt(0, fLinePt3, 0, 0, 0);
                    fLine.Closed = false;

                    drawnEnts.Add(fLine);

                    if (Contour_T.SelectedIndex > 0)
                    {
                        var lVector = fLinePt2.GetVectorTo(fLinePt3);
                        var pVector = lVector.GetPerpendicularVector();

                        var cenPt = fLinePt2.GetMidPoint(fLinePt3);
                        var cenArc = cenPt.GetAlong(pVector, arcRad);
                        var acArc = new Arc(cenArc.Convert3D(), arcRad * .9, CalcUnit.ConvertToRadians(185),
                            CalcUnit.ConvertToRadians(270));
                        var flLine = new Line(acArc.StartPoint, acArc.EndPoint);

                        switch (cIndex_T)
                        {
                            case 1: //Concave
                                flLine.Dispose();
                                drawnEnts.Add(acArc);
                                break;
                            case 2: //Flush
                                acArc.Dispose();
                                drawnEnts.Add(flLine);
                                break;
                            case 3: //Convex
                                flLine.Dispose();
                                var mirLine = new Line3d(acArc.StartPoint, acArc.EndPoint);
                                var mirMat = Matrix3d.Mirroring(mirLine);
                                acArc.TransformBy(mirMat);
                                mirLine.Dispose();
                                drawnEnts.Add(acArc);
                                break;
                        }

                        if (Method_T.SelectedIndex > 0)
                        {
                            var insertionPoint = cenArc.GetAlong(pVector, fLineHalf / 2);
                            var methodText = "";

                            switch (mIndex_T)
                            {
                                case 1: //Chipping
                                    methodText = "C";
                                    break;
                                case 2: //Grinding
                                    methodText = "G";
                                    break;
                                case 3: //Hammering
                                    methodText = "H";
                                    break;
                                case 4: //Machining
                                    methodText = "M";
                                    break;
                                case 5: //Rolling
                                    methodText = "R";
                                    break;
                            }

                            var dbText = new MText();
                            dbText.Attachment = AttachmentPoint.MiddleCenter;
                            dbText.Location = insertionPoint.Convert3D();
                            dbText.Contents = methodText;
                            dbText.TextStyleId = DocumentManager.CurrentDocument.Database.Textstyle;
                            dbText.Height = CalcUnit.GetProportion(SettingsUser.LeaderTextHeight, 1, symbolLength);

                            drawnEnts.Add(dbText);
                        }
                    }

                    break;
                case 2: //Plug

                    //Draw Symbol
                    var pLinePt1 = new Point2d(mPoint.X - fLineLength, mPoint.Y);
                    var pLinePt2 = new Point2d(pLinePt1.X, pLinePt1.Y + fLineLength);
                    var pLinePt3 = new Point2d(mPoint.X + fLineLength, pLinePt2.Y);
                    var pLinePt4 = new Point2d(pLinePt3.X, mPoint.Y);

                    var pLine = new Polyline(4);
                    pLine.AddVertexAt(0, pLinePt1, 0, 0, 0);
                    pLine.AddVertexAt(1, pLinePt2, 0, 0, 0);
                    pLine.AddVertexAt(2, pLinePt3, 0, 0, 0);
                    pLine.AddVertexAt(3, pLinePt4, 0, 0, 0);
                    pLine.Closed = false;

                    drawnEnts.Add(pLine);
                    if (Contour_T.SelectedIndex > 0)
                    {
                        var lVector = pLinePt2.GetVectorTo(pLinePt3);
                        var pVector = lVector.GetPerpendicularVector();
                        var cenPt = pLinePt2.GetMidPoint(pLinePt3);
                        var cenArc = cenPt.GetAlong(pVector, arcRad);
                        var acArc = new Arc(cenArc.Convert3D(), arcRad * .85, CalcUnit.ConvertToRadians(210),
                            CalcUnit.ConvertToRadians(330));
                        var flLine = new Line(acArc.StartPoint, acArc.EndPoint);

                        switch (cIndex_T)
                        {
                            case 1: //Concave
                                flLine.Dispose();
                                drawnEnts.Add(acArc);
                                break;
                            case 2: //Flush
                                acArc.Dispose();
                                drawnEnts.Add(flLine);
                                break;
                            case 3: //Convex
                                flLine.Dispose();
                                var mirLine = new Line3d(acArc.StartPoint, acArc.EndPoint);
                                var mirMat = Matrix3d.Mirroring(mirLine);
                                acArc.TransformBy(mirMat);
                                mirLine.Dispose();
                                drawnEnts.Add(acArc);
                                break;
                        }

                        if (Method_T.SelectedIndex > 0)
                        {
                            var insertionPoint = cenArc.GetAlong(pVector, fLineHalf / 2);
                            var methodText = "";

                            switch (mIndex_T)
                            {
                                case 1: //Chipping
                                    methodText = "C";
                                    break;
                                case 2: //Grinding
                                    methodText = "G";
                                    break;
                                case 3: //Hammering
                                    methodText = "H";
                                    break;
                                case 4: //Machining
                                    methodText = "M";
                                    break;
                                case 5: //Rolling
                                    methodText = "R";
                                    break;
                            }

                            var dbText = new MText();
                            dbText.Attachment = AttachmentPoint.MiddleCenter;
                            dbText.Location = insertionPoint.Convert3D();
                            dbText.Contents = methodText;
                            dbText.TextStyleId = DocumentManager.CurrentDocument.Database.Textstyle;
                            dbText.Height = CalcUnit.GetProportion(SettingsUser.LeaderTextHeight, 1, symbolLength);

                            drawnEnts.Add(dbText);
                        }
                    }

                    break;
                case 3: //Spot

                    //Draw Symbol
                    var sVec = SymStart.GetVectorTo(SymEnd);
                    var pVec = sVec.GetPerpendicularVector();
                    var sCen = mPoint.GetAlong(pVec, fLineHalf);

                    var acCirc = new Circle(sCen, Vector3d.ZAxis, fLineHalf);

                    drawnEnts.Add(acCirc);

                    if (Contour_T.SelectedIndex > 0)
                    {
                        var cenArc = mPoint.GetAlong(pVec, fLineLength + arcRad);
                        var acArc = new Arc(cenArc, arcRad * .85, CalcUnit.ConvertToRadians(210),
                            CalcUnit.ConvertToRadians(330));
                        var flLine = new Line(acArc.StartPoint, acArc.EndPoint);

                        switch (cIndex_T)
                        {
                            case 1: //Concave
                                flLine.Dispose();
                                drawnEnts.Add(acArc);
                                break;
                            case 2: //Flush
                                acArc.Dispose();
                                drawnEnts.Add(flLine);
                                break;
                            case 3: //Convex
                                flLine.Dispose();
                                var mirLine = new Line3d(acArc.StartPoint, acArc.EndPoint);
                                var mirMat = Matrix3d.Mirroring(mirLine);
                                acArc.TransformBy(mirMat);
                                mirLine.Dispose();
                                drawnEnts.Add(acArc);
                                break;
                        }

                        if (Method_T.SelectedIndex > 0)
                        {
                            var insertionPoint = cenArc.GetAlong(pVec, fLineHalf / 2);
                            var methodText = "";

                            switch (mIndex_T)
                            {
                                case 1: //Chipping
                                    methodText = "C";
                                    break;
                                case 2: //Grinding
                                    methodText = "G";
                                    break;
                                case 3: //Hammering
                                    methodText = "H";
                                    break;
                                case 4: //Machining
                                    methodText = "M";
                                    break;
                                case 5: //Rolling
                                    methodText = "R";
                                    break;
                            }

                            var dbText = new MText();
                            dbText.Attachment = AttachmentPoint.MiddleCenter;
                            dbText.Location = insertionPoint;
                            dbText.Contents = methodText;
                            dbText.TextStyleId = DocumentManager.CurrentDocument.Database.Textstyle;
                            dbText.Height = CalcUnit.GetProportion(SettingsUser.LeaderTextHeight, 1, symbolLength);

                            drawnEnts.Add(dbText);
                        }
                    }

                    break;
                case 4: //Seam

                    //Draw Symbol
                    var mVec = SymStart.GetVectorTo(SymEnd);
                    var mpVec = mVec.GetPerpendicularVector();
                    var mCen = mPoint.GetAlong(mpVec, fLineHalf);

                    var mCirc = new Circle(mCen, Vector3d.ZAxis, fLineHalf);

                    drawnEnts.Add(mCirc);

                    var oSet = CalcUnit.GetProportion(.025, 1, symbolLength);

                    var mLine1Pt = mCen.GetAlong(mpVec, oSet);
                    var mLine2Pt = mCen.GetAlong(mpVec, -oSet);

                    var mLine1 = new Line(new Point3d(mLine1Pt.X - fLineLength, mLine1Pt.Y, mLine1Pt.Z),
                        new Point3d(mLine1Pt.X + fLineLength, mLine1Pt.Y, mLine1Pt.Z));
                    var mLine2 = new Line(new Point3d(mLine2Pt.X - fLineLength, mLine2Pt.Y, mLine2Pt.Z),
                        new Point3d(mLine2Pt.X + fLineLength, mLine2Pt.Y, mLine2Pt.Z));

                    drawnEnts.Add(mLine1);
                    drawnEnts.Add(mLine2);

                    if (Contour_T.SelectedIndex > 0)
                    {
                        var cenArc = mPoint.GetAlong(mpVec, fLineLength + arcRad);
                        var acArc = new Arc(cenArc, arcRad * .85, CalcUnit.ConvertToRadians(210),
                            CalcUnit.ConvertToRadians(330));
                        var flLine = new Line(acArc.StartPoint, acArc.EndPoint);

                        switch (cIndex_T)
                        {
                            case 1: //Concave
                                flLine.Dispose();
                                drawnEnts.Add(acArc);
                                break;
                            case 2: //Flush
                                acArc.Dispose();
                                drawnEnts.Add(flLine);
                                break;
                            case 3: //Convex
                                flLine.Dispose();
                                var mirLine = new Line3d(acArc.StartPoint, acArc.EndPoint);
                                var mirMat = Matrix3d.Mirroring(mirLine);
                                acArc.TransformBy(mirMat);
                                mirLine.Dispose();
                                drawnEnts.Add(acArc);
                                break;
                        }

                        if (Method_T.SelectedIndex > 0)
                        {
                            var insertionPoint = cenArc.GetAlong(mpVec, fLineHalf / 2);
                            var methodText = "";

                            switch (mIndex_T)
                            {
                                case 1: //Chipping
                                    methodText = "C";
                                    break;
                                case 2: //Grinding
                                    methodText = "G";
                                    break;
                                case 3: //Hammering
                                    methodText = "H";
                                    break;
                                case 4: //Machining
                                    methodText = "M";
                                    break;
                                case 5: //Rolling
                                    methodText = "R";
                                    break;
                            }

                            var dbText = new MText();
                            dbText.Attachment = AttachmentPoint.MiddleCenter;
                            dbText.Location = insertionPoint;
                            dbText.Contents = methodText;
                            dbText.TextStyleId = DocumentManager.CurrentDocument.Database.Textstyle;
                            dbText.Height = CalcUnit.GetProportion(SettingsUser.LeaderTextHeight, 1, symbolLength);

                            drawnEnts.Add(dbText);
                        }
                    }

                    break;
                case 5: //Backing

                    //Draw Symbol
                    var bVec = SymStart.GetVectorTo(SymEnd);
                    var bpVec = bVec.GetPerpendicularVector();

                    var bcArc = new Arc(mPoint, fLineLength * 0.75, CalcUnit.ConvertToRadians(0),
                        CalcUnit.ConvertToRadians(180));
                    drawnEnts.Add(bcArc);

                    if (Contour_T.SelectedIndex > 0)
                    {
                        var cenArc = mPoint.GetAlong(bpVec, fLineLength * 0.75 + arcRad);
                        var acArc = new Arc(cenArc, arcRad * .85, CalcUnit.ConvertToRadians(210),
                            CalcUnit.ConvertToRadians(330));
                        var flLine = new Line(acArc.StartPoint, acArc.EndPoint);

                        switch (cIndex_T)
                        {
                            case 1: //Concave
                                flLine.Dispose();
                                drawnEnts.Add(acArc);
                                break;
                            case 2: //Flush
                                acArc.Dispose();
                                drawnEnts.Add(flLine);
                                break;
                            case 3: //Convex
                                flLine.Dispose();
                                var mirLine = new Line3d(acArc.StartPoint, acArc.EndPoint);
                                var mirMat = Matrix3d.Mirroring(mirLine);
                                acArc.TransformBy(mirMat);
                                mirLine.Dispose();
                                drawnEnts.Add(acArc);
                                break;
                        }

                        if (Method_T.SelectedIndex > 0)
                        {
                            var insertionPoint = cenArc.GetAlong(bpVec, fLineHalf / 2);
                            var methodText = "";

                            switch (mIndex_T)
                            {
                                case 1: //Chipping
                                    methodText = "C";
                                    break;
                                case 2: //Grinding
                                    methodText = "G";
                                    break;
                                case 3: //Hammering
                                    methodText = "H";
                                    break;
                                case 4: //Machining
                                    methodText = "M";
                                    break;
                                case 5: //Rolling
                                    methodText = "R";
                                    break;
                            }

                            var dbText = new MText();
                            dbText.Attachment = AttachmentPoint.MiddleCenter;
                            dbText.Location = insertionPoint;
                            dbText.Contents = methodText;
                            dbText.TextStyleId = DocumentManager.CurrentDocument.Database.Textstyle;
                            dbText.Height = CalcUnit.GetProportion(SettingsUser.LeaderTextHeight, 1, symbolLength);

                            drawnEnts.Add(dbText);
                        }
                    }

                    break;
            }

            #endregion

            var mirrorLine = new Line3d(SymStart, SymEnd);
            var mm = Matrix3d.Mirroring(mirrorLine);

            #region BottomWeldSymbol

            switch (index_B)
            {
                case 1: //Fillet
                    var fLinePt1 = new Point2d(mPoint.X, mPoint.Y);
                    var fLinePt2 = new Point2d(fLinePt1.X, fLinePt1.Y + fLineLength);
                    var fLinePt3 = new Point2d(fLinePt1.X + fLineLength, fLinePt1.Y);

                    var fLine = new Polyline(3);
                    fLine.AddVertexAt(0, fLinePt1, 0, 0, 0);
                    fLine.AddVertexAt(0, fLinePt2, 0, 0, 0);
                    fLine.AddVertexAt(0, fLinePt3, 0, 0, 0);
                    fLine.Closed = false;

                    fLine.TransformBy(mm);
                    drawnEnts.Add(fLine);

                    if (Contour_B.SelectedIndex > 0)
                    {
                        var lVector = fLinePt2.GetVectorTo(fLinePt3);
                        var pVector = lVector.GetPerpendicularVector();

                        var cenPt = fLinePt2.GetMidPoint(fLinePt3);
                        var cenArc = cenPt.GetAlong(pVector, arcRad);
                        var acArc = new Arc(cenArc.Convert3D(), arcRad * .9, CalcUnit.ConvertToRadians(185),
                            CalcUnit.ConvertToRadians(270));
                        var flLine = new Line(acArc.StartPoint, acArc.EndPoint);

                        switch (cIndex_B)
                        {
                            case 1: //Concave
                                flLine.Dispose();
                                acArc.TransformBy(mm);
                                drawnEnts.Add(acArc);
                                break;
                            case 2: //Flush
                                acArc.Dispose();
                                flLine.TransformBy(mm);
                                drawnEnts.Add(flLine);
                                break;
                            case 3: //Convex
                                flLine.Dispose();
                                var mirLine = new Line3d(acArc.StartPoint, acArc.EndPoint);
                                var mirMat = Matrix3d.Mirroring(mirLine);
                                acArc.TransformBy(mirMat);
                                mirLine.Dispose();
                                acArc.TransformBy(mm);
                                drawnEnts.Add(acArc);
                                break;
                        }

                        if (Method_B.SelectedIndex > 0)
                        {
                            var insertionPoint = cenArc.GetAlong(pVector, fLineHalf / 2);
                            insertionPoint = insertionPoint.Convert3D().TransformBy(mm).Convert2D();
                            var methodText = "";

                            switch (mIndex_B)
                            {
                                case 1: //Chipping
                                    methodText = "C";
                                    break;
                                case 2: //Grinding
                                    methodText = "G";
                                    break;
                                case 3: //Hammering
                                    methodText = "H";
                                    break;
                                case 4: //Machining
                                    methodText = "M";
                                    break;
                                case 5: //Rolling
                                    methodText = "R";
                                    break;
                            }

                            var dbText = new MText();
                            dbText.Attachment = AttachmentPoint.MiddleCenter;
                            dbText.Location = insertionPoint.Convert3D();
                            dbText.Contents = methodText;
                            dbText.TextStyleId = DocumentManager.CurrentDocument.Database.Textstyle;
                            dbText.Height = CalcUnit.GetProportion(SettingsUser.LeaderTextHeight, 1, symbolLength);

                            drawnEnts.Add(dbText);
                        }
                    }

                    break;
                case 2: //Plug

                    //Draw Symbol
                    var pLinePt1 = new Point2d(mPoint.X - fLineLength, mPoint.Y);
                    var pLinePt2 = new Point2d(pLinePt1.X, pLinePt1.Y + fLineLength);
                    var pLinePt3 = new Point2d(mPoint.X + fLineLength, pLinePt2.Y);
                    var pLinePt4 = new Point2d(pLinePt3.X, mPoint.Y);

                    var pLine = new Polyline(4);
                    pLine.AddVertexAt(0, pLinePt1, 0, 0, 0);
                    pLine.AddVertexAt(1, pLinePt2, 0, 0, 0);
                    pLine.AddVertexAt(2, pLinePt3, 0, 0, 0);
                    pLine.AddVertexAt(3, pLinePt4, 0, 0, 0);
                    pLine.Closed = false;
                    pLine.TransformBy(mm);
                    drawnEnts.Add(pLine);
                    if (Contour_B.SelectedIndex > 0)
                    {
                        var lVector = pLinePt2.GetVectorTo(pLinePt3);
                        var pVector = lVector.GetPerpendicularVector();
                        var cenPt = pLinePt2.GetMidPoint(pLinePt3);
                        var cenArc = cenPt.GetAlong(pVector, arcRad);
                        var acArc = new Arc(cenArc.Convert3D(), arcRad * .85, CalcUnit.ConvertToRadians(210),
                            CalcUnit.ConvertToRadians(330));
                        var flLine = new Line(acArc.StartPoint, acArc.EndPoint);

                        switch (cIndex_B)
                        {
                            case 1: //Concave
                                flLine.Dispose();
                                acArc.TransformBy(mm);
                                drawnEnts.Add(acArc);
                                break;
                            case 2: //Flush
                                acArc.Dispose();
                                flLine.TransformBy(mm);
                                drawnEnts.Add(flLine);
                                break;
                            case 3: //Convex
                                flLine.Dispose();
                                var mirLine = new Line3d(acArc.StartPoint, acArc.EndPoint);
                                var mirMat = Matrix3d.Mirroring(mirLine);
                                acArc.TransformBy(mirMat);
                                mirLine.Dispose();
                                acArc.TransformBy(mm);
                                drawnEnts.Add(acArc);
                                break;
                        }

                        if (Method_B.SelectedIndex > 0)
                        {
                            var insertionPoint = cenArc.GetAlong(pVector, fLineHalf / 2);
                            insertionPoint = insertionPoint.Convert3D().TransformBy(mm).Convert2D();
                            var methodText = "";

                            switch (mIndex_B)
                            {
                                case 1: //Chipping
                                    methodText = "C";
                                    break;
                                case 2: //Grinding
                                    methodText = "G";
                                    break;
                                case 3: //Hammering
                                    methodText = "H";
                                    break;
                                case 4: //Machining
                                    methodText = "M";
                                    break;
                                case 5: //Rolling
                                    methodText = "R";
                                    break;
                            }

                            var dbText = new MText();
                            dbText.Attachment = AttachmentPoint.MiddleCenter;
                            dbText.Location = insertionPoint.Convert3D();
                            dbText.Contents = methodText;
                            dbText.TextStyleId = DocumentManager.CurrentDocument.Database.Textstyle;
                            dbText.Height = CalcUnit.GetProportion(SettingsUser.LeaderTextHeight, 1, symbolLength);

                            drawnEnts.Add(dbText);
                        }
                    }

                    break;
                case 3: //Spot

                    //Draw Symbol
                    var sVec = SymStart.GetVectorTo(SymEnd);
                    var pVec = sVec.GetPerpendicularVector();
                    var sCen = mPoint.GetAlong(pVec, fLineHalf);

                    var acCirc = new Circle(sCen, Vector3d.ZAxis, fLineHalf);
                    acCirc.TransformBy(mm);
                    drawnEnts.Add(acCirc);

                    if (Contour_B.SelectedIndex > 0)
                    {
                        var cenArc = mPoint.GetAlong(pVec, fLineLength + arcRad);
                        var acArc = new Arc(cenArc, arcRad * .85, CalcUnit.ConvertToRadians(210),
                            CalcUnit.ConvertToRadians(330));
                        var flLine = new Line(acArc.StartPoint, acArc.EndPoint);

                        switch (cIndex_B)
                        {
                            case 1: //Concave
                                flLine.Dispose();
                                acArc.TransformBy(mm);
                                drawnEnts.Add(acArc);
                                break;
                            case 2: //Flush
                                acArc.Dispose();
                                flLine.TransformBy(mm);
                                drawnEnts.Add(flLine);
                                break;
                            case 3: //Convex
                                flLine.Dispose();
                                var mirLine = new Line3d(acArc.StartPoint, acArc.EndPoint);
                                var mirMat = Matrix3d.Mirroring(mirLine);
                                acArc.TransformBy(mirMat);
                                mirLine.Dispose();
                                acArc.TransformBy(mm);
                                drawnEnts.Add(acArc);
                                break;
                        }

                        if (Method_B.SelectedIndex > 0)
                        {
                            var insertionPoint = cenArc.GetAlong(pVec, fLineHalf / 2);
                            insertionPoint = insertionPoint.TransformBy(mm);
                            var methodText = "";

                            switch (mIndex_B)
                            {
                                case 1: //Chipping
                                    methodText = "C";
                                    break;
                                case 2: //Grinding
                                    methodText = "G";
                                    break;
                                case 3: //Hammering
                                    methodText = "H";
                                    break;
                                case 4: //Machining
                                    methodText = "M";
                                    break;
                                case 5: //Rolling
                                    methodText = "R";
                                    break;
                            }

                            var dbText = new MText();
                            dbText.Attachment = AttachmentPoint.MiddleCenter;
                            dbText.Location = insertionPoint;
                            dbText.Contents = methodText;
                            dbText.TextStyleId = DocumentManager.CurrentDocument.Database.Textstyle;
                            dbText.Height = CalcUnit.GetProportion(SettingsUser.LeaderTextHeight, 1, symbolLength);

                            drawnEnts.Add(dbText);
                        }
                    }

                    break;
                case 4: //Seam

                    //Draw Symbol
                    var mVec = SymStart.GetVectorTo(SymEnd);
                    var mpVec = mVec.GetPerpendicularVector();
                    var mCen = mPoint.GetAlong(mpVec, fLineHalf);

                    var mCirc = new Circle(mCen, Vector3d.ZAxis, fLineHalf);
                    mCirc.TransformBy(mm);
                    drawnEnts.Add(mCirc);

                    var oSet = CalcUnit.GetProportion(.025, 1, symbolLength);

                    var mLine1Pt = mCen.GetAlong(mpVec, oSet);
                    var mLine2Pt = mCen.GetAlong(mpVec, -oSet);

                    var mLine1 = new Line(new Point3d(mLine1Pt.X - fLineLength, mLine1Pt.Y, mLine1Pt.Z),
                        new Point3d(mLine1Pt.X + fLineLength, mLine1Pt.Y, mLine1Pt.Z));
                    var mLine2 = new Line(new Point3d(mLine2Pt.X - fLineLength, mLine2Pt.Y, mLine2Pt.Z),
                        new Point3d(mLine2Pt.X + fLineLength, mLine2Pt.Y, mLine2Pt.Z));
                    mLine1.TransformBy(mm);
                    mLine2.TransformBy(mm);
                    drawnEnts.Add(mLine1);
                    drawnEnts.Add(mLine2);

                    if (Contour_B.SelectedIndex > 0)
                    {
                        var cenArc = mPoint.GetAlong(mpVec, fLineLength + arcRad);
                        var acArc = new Arc(cenArc, arcRad * .85, CalcUnit.ConvertToRadians(210),
                            CalcUnit.ConvertToRadians(330));
                        var flLine = new Line(acArc.StartPoint, acArc.EndPoint);

                        switch (cIndex_B)
                        {
                            case 1: //Concave
                                flLine.Dispose();
                                acArc.TransformBy(mm);
                                drawnEnts.Add(acArc);
                                break;
                            case 2: //Flush
                                acArc.Dispose();
                                flLine.TransformBy(mm);
                                drawnEnts.Add(flLine);
                                break;
                            case 3: //Convex
                                flLine.Dispose();
                                var mirLine = new Line3d(acArc.StartPoint, acArc.EndPoint);
                                var mirMat = Matrix3d.Mirroring(mirLine);
                                acArc.TransformBy(mirMat);
                                mirLine.Dispose();
                                acArc.TransformBy(mm);
                                drawnEnts.Add(acArc);
                                break;
                        }

                        if (Method_B.SelectedIndex > 0)
                        {
                            var insertionPoint = cenArc.GetAlong(mpVec, fLineHalf / 2);
                            insertionPoint = insertionPoint.TransformBy(mm);
                            var methodText = "";

                            switch (mIndex_B)
                            {
                                case 1: //Chipping
                                    methodText = "C";
                                    break;
                                case 2: //Grinding
                                    methodText = "G";
                                    break;
                                case 3: //Hammering
                                    methodText = "H";
                                    break;
                                case 4: //Machining
                                    methodText = "M";
                                    break;
                                case 5: //Rolling
                                    methodText = "R";
                                    break;
                            }

                            var dbText = new MText();
                            dbText.Attachment = AttachmentPoint.MiddleCenter;
                            dbText.Location = insertionPoint;
                            dbText.Contents = methodText;
                            dbText.TextStyleId = DocumentManager.CurrentDocument.Database.Textstyle;
                            dbText.Height = CalcUnit.GetProportion(SettingsUser.LeaderTextHeight, 1, symbolLength);

                            drawnEnts.Add(dbText);
                        }
                    }

                    break;
                case 5: //Backing

                    //Draw Symbol
                    var bVec = SymStart.GetVectorTo(SymEnd);
                    var bpVec = bVec.GetPerpendicularVector();

                    var bcArc = new Arc(mPoint, fLineLength * 0.75, CalcUnit.ConvertToRadians(0),
                        CalcUnit.ConvertToRadians(180));
                    bcArc.TransformBy(mm);
                    drawnEnts.Add(bcArc);

                    if (Contour_B.SelectedIndex > 0)
                    {
                        var cenArc = mPoint.GetAlong(bpVec, fLineLength * 0.75 + arcRad);
                        var acArc = new Arc(cenArc, arcRad * .85, CalcUnit.ConvertToRadians(210),
                            CalcUnit.ConvertToRadians(330));
                        var flLine = new Line(acArc.StartPoint, acArc.EndPoint);

                        switch (cIndex_B)
                        {
                            case 1: //Concave
                                flLine.Dispose();
                                acArc.TransformBy(mm);
                                drawnEnts.Add(acArc);
                                break;
                            case 2: //Flush
                                acArc.Dispose();
                                flLine.TransformBy(mm);
                                drawnEnts.Add(flLine);
                                break;
                            case 3: //Convex
                                flLine.Dispose();
                                var mirLine = new Line3d(acArc.StartPoint, acArc.EndPoint);
                                var mirMat = Matrix3d.Mirroring(mirLine);
                                acArc.TransformBy(mirMat);
                                mirLine.Dispose();
                                acArc.TransformBy(mm);
                                drawnEnts.Add(acArc);
                                break;
                        }

                        if (Method_B.SelectedIndex > 0)
                        {
                            var insertionPoint = cenArc.GetAlong(bpVec, fLineHalf / 2);
                            insertionPoint = insertionPoint.TransformBy(mm);
                            var methodText = "";

                            switch (mIndex_B)
                            {
                                case 1: //Chipping
                                    methodText = "C";
                                    break;
                                case 2: //Grinding
                                    methodText = "G";
                                    break;
                                case 3: //Hammering
                                    methodText = "H";
                                    break;
                                case 4: //Machining
                                    methodText = "M";
                                    break;
                                case 5: //Rolling
                                    methodText = "R";
                                    break;
                            }

                            var dbText = new MText();
                            dbText.Attachment = AttachmentPoint.MiddleCenter;
                            dbText.Location = insertionPoint;
                            dbText.Contents = methodText;
                            dbText.TextStyleId = DocumentManager.CurrentDocument.Database.Textstyle;
                            dbText.Height = CalcUnit.GetProportion(SettingsUser.LeaderTextHeight, 1, symbolLength);

                            drawnEnts.Add(dbText);
                        }
                    }

                    break;
            }

            #endregion

            if (WeldAllAround.Checked)
            {
                var acCirc = new Circle(SymStart, Vector3d.ZAxis, fLineHalf / 2);
                drawnEnts.Add(acCirc);
            }

            if (WeldFlag.Checked)
            {
                var mLine1 = new Point3d(SymStart.X, SymStart.Y + fLineLength * 2, 0);
                var mLine2 = new Point3d(mLine1.X + fLineHalf, mLine1.Y - fLineHalf, 0);
                var mLine3 = new Point3d(mLine1.X, mLine2.Y - fLineHalf, 0);

                var pLine = new Polyline(4);

                pLine.AddVertexAt(0, SymStart.Convert2D(), 0, 0, 0);
                pLine.AddVertexAt(1, mLine1.Convert2D(), 0, 0, 0);
                pLine.AddVertexAt(2, mLine2.Convert2D(), 0, 0, 0);
                pLine.AddVertexAt(3, mLine3.Convert2D(), 0, 0, 0);

                drawnEnts.Add(pLine);
            }

            if (TailNote.Text != "" && !string.IsNullOrEmpty(TailNote.Text))
            {
                if (!LeftFacing)
                {
                    var t1 = new Line(SymEnd, new Point3d(SymEnd.X + fLineLength * 2, SymStart.Y + fLineLength * 2, 0));
                    var t2 = new Line(SymEnd, new Point3d(SymEnd.X + fLineLength * 2, SymStart.Y - fLineLength * 2, 0));

                    drawnEnts.Add(t1);
                    drawnEnts.Add(t2);

                    var dbText = new MText();
                    dbText.Attachment = AttachmentPoint.MiddleLeft;
                    dbText.Location = new Point3d(SymEnd.X + fLineLength, SymEnd.Y, 0);
                    dbText.Contents = TailNote.Text;
                    dbText.TextStyleId = DocumentManager.CurrentDocument.Database.Textstyle;
                    dbText.Height = CalcUnit.GetProportion(SettingsUser.LeaderTextHeight / 2, 1, symbolLength);
                    drawnEnts.Add(dbText);
                }
                else
                {
                    var t1 = new Line(SymEnd, new Point3d(SymEnd.X - fLineLength * 2, SymEnd.Y + fLineLength * 2, 0));
                    var t2 = new Line(SymEnd, new Point3d(SymEnd.X - fLineLength * 2, SymEnd.Y - fLineLength * 2, 0));

                    drawnEnts.Add(t1);
                    drawnEnts.Add(t2);

                    var dbText = new MText();
                    dbText.Attachment = AttachmentPoint.MiddleRight;
                    dbText.Location = new Point3d(SymEnd.X - fLineLength, SymEnd.Y, 0);
                    dbText.Contents = TailNote.Text;
                    dbText.TextStyleId = DocumentManager.CurrentDocument.Database.Textstyle;
                    dbText.Height = CalcUnit.GetProportion(SettingsUser.LeaderTextHeight / 2, 1, symbolLength);
                    drawnEnts.Add(dbText);
                }
            }

            TransientAgent.Add(drawnEnts.ToArray());
            TransientAgent.Draw();
            DocumentManager.MdiActiveDocument.Editor.Regen();
        }

        #endregion

        #region Visibility Handlers

        private void WeldFlag_CheckedChanged(object sender, EventArgs e)
        {
            if (WeldFlag.Checked)
                WeldFlag.Image = Properties.Resources.Weld_Flag;
            else
                WeldFlag.Image = Properties.Resources.Weld_NoFlag;
        }

        private void WeldAllAround_CheckedChanged(object sender, EventArgs e)
        {
            if (WeldAllAround.Checked)
                WeldAllAround.Image = Properties.Resources.Weld_AllAround;
            else
                WeldAllAround.Image = Properties.Resources.Weld_Single;
        }

        private void FlipSyms_Click(object sender, EventArgs e)
        {
            //TODO
            //Implement method for swapping
        }

        private void WeldType_T_SelectedIndexChanged(object sender, EventArgs e)
        {
            SuspendLayout();

            var index = WeldType_T.SelectedIndex;

            Contour_T.Visible = false;
            Method_T.Visible = false;

            switch (index)
            {
                case 0:
                    break;
                case 1: //Fillet

                    Contour_T.Visible = true;

                    if (Contour_T.SelectedIndex > 0) Method_T.Visible = true;

                    break;
                case 2: //Plug

                    Contour_T.Visible = true;


                    if (Contour_T.SelectedIndex > 0) Method_T.Visible = true;

                    break;
                case 3: //Spot

                    Contour_T.Visible = true;

                    if (Contour_T.SelectedIndex > 0) Method_T.Visible = true;

                    break;
                case 4: //Seam


                    Contour_T.Visible = true;


                    if (Contour_T.SelectedIndex > 0) Method_T.Visible = true;

                    break;
                case 5: //Backing

                    Contour_T.Visible = true;


                    if (Contour_T.SelectedIndex > 0) Method_T.Visible = true;

                    break;
            }

            ResumeLayout();
        }

        private void WeldType_B_SelectedIndexChanged(object sender, EventArgs e)
        {
            SuspendLayout();

            var index = WeldType_B.SelectedIndex;


            Contour_B.Visible = false;
            Method_B.Visible = false;


            switch (index)
            {
                case 0:
                    break;
                case 1: //Fillet

                    Contour_B.Visible = true;

                    if (Contour_B.SelectedIndex > 0) Method_B.Visible = true;

                    break;
                case 2: //Plug

                    Contour_B.Visible = true;

                    if (Contour_B.SelectedIndex > 0) Method_B.Visible = true;

                    break;
                case 3: //Spot

                    Contour_B.Visible = true;

                    if (Contour_B.SelectedIndex > 0) Method_B.Visible = true;

                    break;
                case 4: //Seam

                    Contour_B.Visible = true;

                    if (Contour_B.SelectedIndex > 0) Method_B.Visible = true;

                    break;
                case 5: //Backing

                    Contour_B.Visible = true;

                    if (Contour_B.SelectedIndex > 0) Method_B.Visible = true;

                    break;
                //case 6: //Melt

                //    Prefix_B.Visible = true;
                //    Size_B.Visible = true;
                //    Contour_B.Visible = true;
                //    Angle_B.Visible = true;

                //    if (Contour_B.SelectedIndex > 0) Method_B.Visible = true;

                //    break;
                //case 7: //Flange Edge

                //    Prefix_B.Visible = true;
                //    Leg1_B.Visible = true;
                //    Leg2_B.Visible = true;
                //    Length_B.Visible = true;
                //    Pitch_B.Visible = true;
                //    Contour_B.Visible = true;
                //    Depth_B.Visible = true;

                //    Plus_B.Visible = true;
                //    Minus_B.Visible = true;

                //    if (Contour_B.SelectedIndex > 0) Method_B.Visible = true;

                //    break;
                //case 8: //Flange Corner

                //    Prefix_B.Visible = true;
                //    Leg1_B.Visible = true;
                //    Leg2_B.Visible = true;
                //    Length_B.Visible = true;
                //    Pitch_B.Visible = true;
                //    Contour_B.Visible = true;
                //    Depth_B.Visible = true;

                //    Plus_B.Visible = true;
                //    Minus_B.Visible = true;

                //    if (Contour_B.SelectedIndex > 0) Method_B.Visible = true;

                //    break;
                //case 9: //Square Groove

                //    Prefix_B.Visible = true;
                //    Size_B.Visible = true;
                //    Length_B.Visible = true;
                //    Pitch_B.Visible = true;
                //    Contour_B.Visible = true;
                //    Angle_B.Visible = true;

                //    Minus_B.Visible = true;

                //    if (Contour_B.SelectedIndex > 0) Method_B.Visible = true;

                //    break;
                //case 10: //V Groove

                //    Prefix_B.Visible = true;
                //    Size_B.Visible = true;
                //    Length_B.Visible = true;
                //    Pitch_B.Visible = true;
                //    Contour_B.Visible = true;
                //    Depth_B.Visible = true;
                //    Angle_B.Visible = true;

                //    Minus_B.Visible = true;

                //    if (Contour_B.SelectedIndex > 0) Method_B.Visible = true;

                //    break;
            }

            ResumeLayout();
        }

        private void Contour_T_SelectedIndexChanged(object sender, EventArgs e)
        {
            Method_T.Visible = Contour_T.SelectedIndex > 0;
        }

        private void Contour_B_SelectedIndexChanged(object sender, EventArgs e)
        {
            Method_B.Visible = Contour_B.SelectedIndex > 0;
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Visible = false;
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Visible = false;
        }

        #endregion
    }
}