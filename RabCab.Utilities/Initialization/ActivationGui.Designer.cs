namespace RabCab.Initialization
{
    partial class ActivationGui
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.mainMenu1 = new System.Windows.Forms.MainMenu(this.components);
            this.lblTrialMessage = new System.Windows.Forms.Label();
            this.ActDtBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblTrialMessage
            // 
            this.lblTrialMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblTrialMessage.AutoSize = true;
            this.lblTrialMessage.Location = new System.Drawing.Point(9, 253);
            this.lblTrialMessage.Name = "lblTrialMessage";
            this.lblTrialMessage.Size = new System.Drawing.Size(95, 13);
            this.lblTrialMessage.TabIndex = 1;
            this.lblTrialMessage.Text = "Your trial expires in";
            this.lblTrialMessage.Visible = false;
            // 
            // ActDtBtn
            // 
            this.ActDtBtn.Location = new System.Drawing.Point(12, 200);
            this.ActDtBtn.Name = "ActDtBtn";
            this.ActDtBtn.Size = new System.Drawing.Size(310, 44);
            this.ActDtBtn.TabIndex = 2;
            this.ActDtBtn.Text = "Deactivate...";
            this.ActDtBtn.UseVisualStyleBackColor = true;
            this.ActDtBtn.Click += new System.EventHandler(this.Button1_Click);
            // 
            // ActivationGui
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(334, 283);
            this.Controls.Add(this.ActDtBtn);
            this.Controls.Add(this.lblTrialMessage);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.Menu = this.mainMenu1;
            this.Name = "ActivationGui";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "RabCab Activation";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MainMenu mainMenu1;
        private System.Windows.Forms.Label lblTrialMessage;
        private System.Windows.Forms.Button ActDtBtn;
    }
}

