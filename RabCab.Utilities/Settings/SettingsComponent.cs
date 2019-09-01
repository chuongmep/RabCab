using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RabCab.Commands.CarpentrySuite;
using RabCab.Engine.Enumerators;
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
            NumTextHeight.Value = (decimal)LayTextHeight;

            check_PromptMult.Checked = PromptForMultiplication;
            check_CreateFlat.Checked = LayFlatShot;
            check_FlatAll.Checked = LayAllSidesFlatShot;

            if (LayTextAbove && LayTextLeft)
            {
                Loc_TL.Checked = true;
            }
            else if (LayTextAbove && LayTextCenter)
            {
                Loc_TC.Checked = true;
            }
            else if (LayTextInside && LayTextLeft)
            {
                Loc_CL.Checked = true;
            }
            else if (LayTextInside && LayTextCenter)
            {
                Loc_MC.Checked = true;
            }
            else
            {
                Loc_TL.Checked = true;
            }

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
            BOM_ColWidth.Value = (decimal)TableColumnWidth;
            BOM_TextHeight.Value = (decimal)TableTextHeight;
            BOM_XOffset.Value = (decimal)TableXOffset;
            BOM_YOffset.Value = (decimal)TableYOffset;

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

        //Website Link
        private void LinkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Navigate to a URL.
            System.Diagnostics.Process.Start("http://www.rabcab.com");
        }

        //Email Link
        private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = "mailto:RabCabService@gmail.com";
            proc.Start();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            openFileDialog.Filter = "Drawing Template (*.dwt)|*.dwt";

           
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                ViewTemplateBox.Text = string.Format("{0}/{1}",
                    Path.GetDirectoryName(openFileDialog.FileName), openFileDialog.FileName);
            }
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
    }
}
