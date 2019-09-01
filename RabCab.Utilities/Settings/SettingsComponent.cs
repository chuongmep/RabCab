using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using RabCab.Commands.PaletteKit;
using RabCab.Engine.Enumerators;
using RabCab.Entities.Controls;
using RabCab.Initialization;
using static RabCab.Settings.SettingsUser;

namespace RabCab.Settings
{
    public partial class SettingsComponent : UserControl
    {
        public SettingsComponent()
        {
            InitializeComponent();
        }

        public void UpdateGui()
        {
            //Common Tab
            TCounter.Value = (int) UserTol;

            Check_RightAnglePriority.Checked = PrioritizeRightAngles;
            Check_MetPal.Checked = EnableSelectionParse;

            NameConv.Text = NamingConvention;
            LayerDelim.Text = LayerDelimiter.ToString();
            Check_ResetPartCount.Checked = ResetPartCount;

            SortLayer.Checked = SortByLayer;
            SortColor.Checked = SortByColor;
            SortThickness.Checked = SortByThickness;
            SortName.Checked = SortByName;
            SortMix.Checked = MixS4S;

            FlatAssembly.Checked = FlattenAssembly;
            FlatAllSides.Checked = FlattenAllSides;
            RetainHidden.Checked = RetainHiddenLines;

            NumLStep.Value = LayStep;
            NumTextHeight.Value = (decimal) LayTextHeight;

            check_PromptMult.Checked = PromptForMultiplication;
            check_CreateFlat.Checked = LayFlatShot;
            check_FlatAll.Checked = LayAllSidesFlatShot;

            if (LayTextAbove && LayTextLeft)
                Loc_TL.Checked = true;
            else if (LayTextAbove && LayTextCenter)
                Loc_TC.Checked = true;
            else if (LayTextInside && LayTextLeft)
                Loc_CL.Checked = true;
            else if (LayTextInside && LayTextCenter)
                Loc_MC.Checked = true;
            else
                Loc_TL.Checked = true;

            Carp_JointDepth.Value = (decimal) RcJointDepth;
            Carp_OffDepth.Value = (decimal) RcOffsetDepth;
            Carp_SliceDepth.Value = (decimal) RcSliceDepth;
            Carp_GapDepth.Value = (decimal) RcGapDepth;
            Carp_DogEar.Value = (decimal) DogEarDiam;
            Carp_ChopDepth.Value = (decimal) RcChopDepth;
            Carp_ICutDepth.Value = (decimal) RcICutDepth;
            Carp_ICutInset.Value = (decimal) RcICutInset;
            Carp_Lam.Value = (decimal) LaminateThickness;
            Carp_EdgeBand.Value = (decimal) EdgeBandThickness;
            Carp_Explode.Value = (decimal) ExplodePower;

            LabelActivate.Text = InitPlugin.Activated ? "Activated" : "Trial Version";

            Lay_Visible.Text = RcVisible;
            Lay_Hidden.Text = RcHidden;
            Lay_Anno.Text = RcAnno;
            Lay_Holes.Text = RcHoles;

            Check_AutoLayer.Checked = AutoLayerEnabled;

            AutoLay_Commands.Text = string.Join(",", LayerCommandList);

            BOM_Title.Text = BomTitle;
            Bom_Layer.Checked = BomLayer;
            BOM_Color.Checked = BomColor;
            BOM_Name.Checked = BomName;
            BOM_Width.Checked = BomWidth;
            BOM_Length.Checked = BomLength;
            BOM_Thickness.Checked = BomThickness;
            BOM_Volume.Checked = BomVolume;
            BOM_TextureDirection.Checked = BomTextureDirection;
            BOM_ProductionType.Checked = BomLayer;
            BOM_Qty.Checked = BomQty;

            BOM_RowHeight.Value = (decimal) TableRowHeight;
            BOM_ColWidth.Value = (decimal) TableColumnWidth;
            BOM_TextHeight.Value = (decimal) TableTextHeight;
            BOM_XOffset.Value = (decimal) TableXOffset;
            BOM_YOffset.Value = (decimal) TableYOffset;

            switch (TableAttach)
            {
                case Enums.AttachmentPoint.TopLeft:
                    A_TL.Checked = true;
                    break;
                case Enums.AttachmentPoint.TopRight:
                    A_TR.Checked = true;
                    break;
                case Enums.AttachmentPoint.BottomLeft:
                    A_BL.Checked = true;
                    break;
                case Enums.AttachmentPoint.BottomRight:
                    A_BR.Checked = true;
                    break;
                case Enums.AttachmentPoint.TopCenter:
                    A_TC.Checked = true;
                    break;
                case Enums.AttachmentPoint.BottomCenter:
                    A_BC.Checked = true;
                    break;
                case Enums.AttachmentPoint.LeftCenter:
                    A_LC.Checked = true;
                    break;
                case Enums.AttachmentPoint.RightCenter:
                    A_RC.Checked = true;
                    break;
                default:
                    A_TR.Checked = true;
                    break;
            }

            Check_PartLeader.Checked = PartLeaderEnabled;
            PartTextHeight.Value = (decimal) LeaderTextHeight;

            Check_DeleteMark.Checked = DeleteExistingMarks;
            MarkHeight.Value = (decimal) MarkTextHeight;

            CPage.Text = PageNoOf;
            TPage.Text = PageNoTotal;

            VSpace.Value = (decimal) ViewSpacing;
            DSpace.Value = (decimal) AnnoSpacing;

            ViewTemplateBox.Text = ViewTemplatePath;
        }

        public void UpdateSettings()
        {
            //Common Tab
            UserTol = (Enums.RoundTolerance) TCounter.Value;

            PrioritizeRightAngles = Check_RightAnglePriority.Checked;
            EnableSelectionParse = Check_MetPal.Checked;

            if (EnableSelectionParse == false)
                RcPaletteMetric.DisablePal();
            else
                RcPaletteMetric.EnablePal();

            NamingConvention = NameConv.Text;
            LayerDelimiter = Convert.ToChar(LayerDelim.Text);
            ResetPartCount = Check_ResetPartCount.Checked;

            SortByLayer = SortLayer.Checked;
            SortByColor = SortColor.Checked;
            SortThickness.Checked = SortByThickness;
            SortByName = SortName.Checked;
            MixS4S = SortMix.Checked;

            FlattenAssembly = FlatAssembly.Checked;
            FlattenAllSides = FlatAllSides.Checked;
            RetainHiddenLines = RetainHidden.Checked;

            LayStep = (int) NumLStep.Value;
            LayTextHeight = (double) NumTextHeight.Value;

            PromptForMultiplication = check_PromptMult.Checked;
            LayFlatShot = check_CreateFlat.Checked;
            LayAllSidesFlatShot = check_FlatAll.Checked;


            PrioritizeRightAngles = Check_RightAnglePriority.Checked;
            EnableSelectionParse = Check_MetPal.Checked;

            NamingConvention = NameConv.Text;
            LayerDelim.Text = LayerDelimiter.ToString();
            ResetPartCount = Check_ResetPartCount.Checked;

            SortByLayer = SortLayer.Checked;
            SortByColor = SortColor.Checked;
            SortByThickness = SortThickness.Checked;
            SortByName = SortName.Checked;
            MixS4S = SortMix.Checked;

            FlattenAssembly = FlatAssembly.Checked;
            FlattenAllSides = FlatAllSides.Checked;
            RetainHiddenLines = RetainHidden.Checked;

            LayStep = (int) NumLStep.Value;
            LayTextHeight = (double) NumTextHeight.Value;

            PromptForMultiplication = check_PromptMult.Checked;
            LayFlatShot = check_CreateFlat.Checked;
            check_FlatAll.Checked = LayAllSidesFlatShot;

            if (Loc_TL.Checked)
            {
                LayTextAbove = true;
                LayTextLeft = true;
            }
            else if (Loc_TC.Checked)
            {
                LayTextAbove = true;
                LayTextCenter = true;
            }
            else if (Loc_CL.Checked)
            {
                LayTextInside = true;
                LayTextLeft = true;
            }
            else if (Loc_MC.Checked)
            {
                LayTextInside = true;
                LayTextCenter = true;
            }

            RcJointDepth = (double) Carp_JointDepth.Value;
            RcOffsetDepth = (double) Carp_OffDepth.Value;
            RcSliceDepth = (double) Carp_SliceDepth.Value;
            RcGapDepth = (double) Carp_GapDepth.Value;
            DogEarDiam = (double) Carp_DogEar.Value;
            RcChopDepth = (double) Carp_ChopDepth.Value;
            RcICutDepth = (double) Carp_ICutDepth.Value;
            RcICutInset = (double) Carp_ICutInset.Value;
            LaminateThickness = (double) Carp_Lam.Value;
            EdgeBandThickness = (double) Carp_EdgeBand.Value;
            ExplodePower = (double) Carp_Explode.Value;

            RcVisible = Lay_Visible.Text;
            RcHidden = Lay_Hidden.Text;
            RcAnno = Lay_Anno.Text;
            RcHoles = Lay_Holes.Text;

            AutoLayerEnabled = Check_AutoLayer.Checked;

            try
            {
                var cmdList = AutoLay_Commands.Text.Split(',');
                LayerCommandList.Clear();
                LayerCommandList = cmdList.ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }


            BomTitle = BOM_Title.Text;
            BomLayer = Bom_Layer.Checked;
            BomColor = BOM_Color.Checked;
            BomName = BOM_Name.Checked;
            BomWidth = BOM_Width.Checked;
            BomLength = BOM_Length.Checked;
            BomThickness = BOM_Thickness.Checked;
            BomVolume = BOM_Volume.Checked;
            BomTextureDirection = BOM_TextureDirection.Checked;
            BomLayer = BOM_ProductionType.Checked;
            BomQty = BOM_Qty.Checked;

            TableRowHeight = (double) BOM_RowHeight.Value;
            TableColumnWidth = (double) BOM_ColWidth.Value;
            TableTextHeight = (double) BOM_TextHeight.Value;
            TableXOffset = (double) BOM_XOffset.Value;
            TableYOffset = (double) BOM_YOffset.Value;

            if (A_TL.Checked)
                TableAttach = Enums.AttachmentPoint.TopLeft;
            else if (A_TR.Checked)
                TableAttach = Enums.AttachmentPoint.TopRight;
            else if (A_BL.Checked)
                TableAttach = Enums.AttachmentPoint.BottomLeft;
            else if (A_BR.Checked)
                TableAttach = Enums.AttachmentPoint.BottomRight;
            else if (A_TC.Checked)
                TableAttach = Enums.AttachmentPoint.TopCenter;
            else if (A_BC.Checked)
                TableAttach = Enums.AttachmentPoint.BottomCenter;
            else if (A_LC.Checked)
                TableAttach = Enums.AttachmentPoint.LeftCenter;
            else if (A_RC.Checked) TableAttach = Enums.AttachmentPoint.RightCenter;

            PartLeaderEnabled = Check_PartLeader.Checked;
            LeaderTextHeight = (double) PartTextHeight.Value;

            DeleteExistingMarks = Check_DeleteMark.Checked;
            MarkTextHeight = (double) MarkHeight.Value;

            PageNoOf = CPage.Text;
            PageNoTotal = TPage.Text;

            ViewSpacing = (double) VSpace.Value;
            AnnoSpacing = (double) DSpace.Value;

            ViewTemplatePath = ViewTemplateBox.Text;
        }

        //Website Link
        private void LinkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Navigate to a URL.
            Process.Start("http://www.rabcab.com");
        }

        //Email Link
        private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var proc = new Process();
            proc.StartInfo.FileName = "mailto:RabCabService@gmail.com";
            proc.Start();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            openFileDialog.Filter = "Drawing Template (*.dwt)|*.dwt";


            if (openFileDialog.ShowDialog() == DialogResult.OK)
                ViewTemplateBox.Text = string.Format("{0}/{1}",
                    Path.GetDirectoryName(openFileDialog.FileName), openFileDialog.FileName);
        }

        private void Check_FlatAll_CheckedChanged(object sender, EventArgs e)
        {
            if (check_CreateFlat.Checked == false && check_FlatAll.Checked)
                check_CreateFlat.Checked = true;
        }

        private void Check_CreateFlat_CheckedChanged(object sender, EventArgs e)
        {
            if (check_FlatAll.Checked && check_CreateFlat.Checked == false)
                check_FlatAll.Checked = false;
        }

        private void LabelActivate_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var actDia = new ActivationGui();
            actDia.ShowDialog(new AcadMainWindow());
        }
    }
}