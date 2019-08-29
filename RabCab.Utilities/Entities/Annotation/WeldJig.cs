using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace RabCab.Entities.Annotation
{
    public class WeldJig : EntityJig
    {
        #region Constructors

        public WeldJig(MLeader ent)
            : base(ent)
        {
            Entity.SetDatabaseDefaults();

            Entity.ContentType = ContentType.MTextContent;
            Entity.MText = new MText();
            Entity.MText.SetDatabaseDefaults();

            Entity.EnableDogleg = false;
            Entity.EnableLanding = false;
            Entity.EnableFrameText = false;
            Entity.LandingGap = 0;

            Entity.AddLeaderLine(symLocationStart);
            Entity.SetFirstVertex(0, mArrowLocation);

            Entity.TransformBy(UCS);
        }

        #endregion

        #region Properties

        private Matrix3d UCS => Application.DocumentManager.MdiActiveDocument.Editor.CurrentUserCoordinateSystem;

        #endregion

        #region Methods to Call

        public static MLeader Jig(out Point3d arrowStart, out Point3d symPoint)
        {
            WeldJig jigger = null;
            try
            {
                jigger = new WeldJig(new MLeader());
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

                    arrowStart = new Point3d();
                    symPoint = new Point3d();
                    return null;
                }

                arrowStart = mArrowLocation;
                symPoint = symLocationStart;
                return jigger.Entity;
            }
            catch
            {
                if (jigger != null && jigger.Entity != null)
                    jigger.Entity.Dispose();

                arrowStart = new Point3d();
                symPoint = new Point3d();
                return null;
            }
        }

        #endregion

        #region Fields

        public int mCurJigFactorIndex = 1; // Jig Factor Index

        public static Point3d mArrowLocation; // Jig Factor #1
        public static Point3d symLocationStart; // Jig Factor #2

        #endregion

        #region Overrides

        public new MLeader Entity // Overload the Entity property for convenience.
            => base.Entity as MLeader;

        protected override bool Update()
        {
            switch (mCurJigFactorIndex)
            {
                case 1:
                    Entity.SetFirstVertex(0, mArrowLocation);
                    Entity.SetLastVertex(0, mArrowLocation);

                    break;
                case 2:
                    Entity.SetLastVertex(0, symLocationStart);
                    break;
                case 3:
                    Entity.MText.Contents = string.Empty;
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

                    if (prResult2.Value.Equals(symLocationStart)) //Use better comparison method if necessary.
                    {
                        return SamplerStatus.NoChange;
                    }
                    else
                    {
                        symLocationStart = prResult2.Value;
                        return SamplerStatus.OK;
                    }
            }

            return SamplerStatus.OK;
        }

        #endregion
    }
}