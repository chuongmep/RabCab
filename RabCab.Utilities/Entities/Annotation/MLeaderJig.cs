using System;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using RabCab.Settings;
using MgdAcApplication = Autodesk.AutoCAD.ApplicationServices.Application;
using MgdAcDocument = Autodesk.AutoCAD.ApplicationServices.Document;
using AcWindowsNS = Autodesk.AutoCAD.Windows;

namespace RabCab.Entities.Annotation
{
    public class MLeaderJigger : EntityJig
    {
        #region Constructors

        public MLeaderJigger(MLeader ent)
            : base(ent)
        {
            // Entity.SetDatabaseDefaults();
            Entity.MLeaderStyle = Application.DocumentManager.MdiActiveDocument.Database.MLeaderstyle;
            Entity.ContentType = ContentType.MTextContent;
            var mText = new MText();
            mText.TextStyleId = Application.DocumentManager.MdiActiveDocument.Database.Textstyle;

            Entity.EnableDogleg = false;
            Entity.EnableLanding = false;
            Entity.LandingGap = 0;

            Entity.TextAttachmentType = TextAttachmentType.AttachmentMiddle;
            Entity.SetTextAttachmentType(TextAttachmentType.AttachmentMiddleOfBottom, LeaderDirectionType.BottomLeader);
            Entity.SetTextAttachmentType(TextAttachmentType.AttachmentMiddle, LeaderDirectionType.LeftLeader);
            Entity.SetTextAttachmentType(TextAttachmentType.AttachmentMiddle, LeaderDirectionType.RightLeader);
            Entity.SetTextAttachmentType(TextAttachmentType.AttachmentMiddleOfTop, LeaderDirectionType.TopLeader);

            Entity.TextAttachmentDirection = TextAttachmentDirection.AttachmentHorizontal;
            Entity.TextAlignmentType = TextAlignmentType.CenterAlignment;

            mText.Attachment = AttachmentPoint.MiddleCenter;
            mText.BackgroundFill = true;
            mText.UseBackgroundColor = true;
            mText.TextHeight = SettingsUser.LeaderTextHeight;

            Entity.MText = mText;
            Entity.EnableFrameText = true;

            Entity.AddLeaderLine(mTextLocation);
            Entity.SetFirstVertex(0, mArrowLocation);

            Entity.TransformBy(UCS);
        }

        #endregion

        #region Methods to Call

        public static MLeader Jig()
        {
            MLeaderJigger jigger = null;
            try
            {
                jigger = new MLeaderJigger(new MLeader());
                PromptResult pr;
                do
                {
                    pr = Application.DocumentManager.MdiActiveDocument.Editor.Drag(jigger);
                    if (pr.Status == PromptStatus.Keyword)
                    {
                        // Keyword handling code
                    }
                    else
                    {
                        jigger.mCurJigFactorIndex++;
                    }
                } while (pr.Status != PromptStatus.Cancel && pr.Status != PromptStatus.Error &&
                         jigger.mCurJigFactorIndex <= 3);

                if (pr.Status == PromptStatus.Cancel || pr.Status == PromptStatus.Error)
                {
                    if (jigger != null && jigger.Entity != null)
                        jigger.Entity.Dispose();

                    return null;
                }

                var text = new MText();
                text.Contents = jigger.mMText;
                text.TransformBy(jigger.UCS);
                jigger.Entity.MText = text;
                return jigger.Entity;
            }
            catch
            {
                if (jigger != null && jigger.Entity != null)
                    jigger.Entity.Dispose();

                return null;
            }
        }

        #endregion

        #region Fields

        public int mCurJigFactorIndex = 1; // Jig Factor Index

        public Point3d mArrowLocation; // Jig Factor #1
        public Point3d mTextLocation; // Jig Factor #2
        public string mMText; // Jig Factor #3

        #endregion

        #region Properties

        private Editor Editor => Application.DocumentManager.MdiActiveDocument.Editor;

        private Matrix3d UCS => Application.DocumentManager.MdiActiveDocument.Editor.CurrentUserCoordinateSystem;

        #endregion

        #region Overrides

        public new MLeader Entity // Overload the Entity property for convenience.
            =>
                base.Entity as MLeader;

        protected override bool Update()
        {
            switch (mCurJigFactorIndex)
            {
                case 1:
                    Entity.SetFirstVertex(0, mArrowLocation);
                    Entity.SetLastVertex(0, mArrowLocation);

                    break;
                case 2:
                    Entity.SetLastVertex(0, mTextLocation);

                    break;
                case 3:
                    Entity.MText.Contents = mMText;

                    break;

                default:
                    return false;
            }

            return true;
        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            switch (mCurJigFactorIndex)
            {
                case 1:
                    var prOptions1 = new JigPromptPointOptions("\nArrow Location:");
                    // Set properties such as UseBasePoint and BasePoint of the prompt options object if necessary here.
                    prOptions1.UserInputControls = UserInputControls.Accept3dCoordinates |
                                                   UserInputControls.GovernedByOrthoMode |
                                                   UserInputControls.GovernedByUCSDetect |
                                                   UserInputControls.UseBasePointElevation;
                    var prResult1 = prompts.AcquirePoint(prOptions1);
                    if (prResult1.Status == PromptStatus.Cancel && prResult1.Status == PromptStatus.Error)
                        return SamplerStatus.Cancel;

                    if (prResult1.Value.Equals(mArrowLocation)) //Use better comparison method if necessary.
                    {
                        return SamplerStatus.NoChange;
                    }
                    else
                    {
                        mArrowLocation = prResult1.Value;
                        return SamplerStatus.OK;
                    }

                case 2:

                    var xDiff = mTextLocation.X - mArrowLocation.X;
                    var yDiff = mTextLocation.Y - mArrowLocation.Y;
                    var angle = Math.Atan2(yDiff, xDiff) * 180.0 / Math.PI;

                    if (angle > 45 && angle < 135 || angle < -45 && angle > -135)
                        Entity.TextAttachmentDirection = TextAttachmentDirection.AttachmentVertical;
                    else
                        Entity.TextAttachmentDirection = TextAttachmentDirection.AttachmentHorizontal;

                    Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n" + angle);
                    var prOptions2 = new JigPromptPointOptions("\nLanding Location:");
                    // Set properties such as UseBasePoint and BasePoint of the prompt options object if necessary here.
                    prOptions2.UseBasePoint = true;
                    prOptions2.BasePoint = mArrowLocation;
                    prOptions2.UserInputControls = UserInputControls.Accept3dCoordinates |
                                                   UserInputControls.GovernedByOrthoMode |
                                                   UserInputControls.GovernedByUCSDetect |
                                                   UserInputControls.UseBasePointElevation;
                    var prResult2 = prompts.AcquirePoint(prOptions2);
                    if (prResult2.Status == PromptStatus.Cancel && prResult2.Status == PromptStatus.Error)
                        return SamplerStatus.Cancel;

                    if (prResult2.Value.Equals(mTextLocation)) //Use better comparison method if necessary.
                    {
                        return SamplerStatus.NoChange;
                    }
                    else
                    {
                        mTextLocation = prResult2.Value;
                        return SamplerStatus.OK;
                    }

                case 3:
                    var prOptions3 = new JigPromptStringOptions("\nText Content:");
                    // Set properties such as UseBasePoint and BasePoint of the prompt options object if necessary here.
                    prOptions3.UserInputControls = UserInputControls.AcceptOtherInputString;
                    var prResult3 = prompts.AcquireString(prOptions3);
                    if (prResult3.Status == PromptStatus.Cancel && prResult3.Status == PromptStatus.Error)
                        return SamplerStatus.Cancel;

                    if (prResult3.StringResult.Equals(mMText)) //Use better comparison method if necessary.
                    {
                        return SamplerStatus.NoChange;
                    }
                    else
                    {
                        mMText = prResult3.StringResult;
                        return SamplerStatus.OK;
                    }
            }

            return SamplerStatus.OK;
        }

        #endregion
    }
}