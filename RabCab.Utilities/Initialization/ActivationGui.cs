using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using wyDay.TurboActivate;
using AcAp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RabCab.Initialization
{
    public partial class ActivationGui : Form
    {
        // Don't use 0 for either of these values.
        // We recommend 90, 14. But if you want to lower the values
        // we don't recommend going below 7 days for each value.
        // Anything lower and you're just punishing legit users.
        private const uint DaysBetweenChecks = 0;
        private const uint GracePeriodLength = 14;
        private readonly TurboActivate ta;

        // Set the trial flags you want to use. Here we've selected that the
        // trial data should be stored system-wide (TA_SYSTEM) and that we should
        // use un-resetable verified trials (TA_VERIFIED_TRIAL).
        private readonly TA_Flags trialFlags = TA_Flags.TA_SYSTEM | TA_Flags.TA_VERIFIED_TRIAL;
        private bool isGenuine;

        public ActivationGui()
        {
            InitializeComponent();

            try
            {
                //TODO: goto the version page at LimeLM and paste this GUID here
                ta = new TurboActivate("4jb6ccnwvp47kmfhqe6cs3jv3p5aeaa");
                // set the trial changed event handler
                ta.TrialChange += trialChange;
                // Check if we're activated, and every 90 days verify it with the activation servers
                // In this example we won't show an error if the activation was done offline
                // (see the 3rd parameter of the IsGenuine() function)
                // https://wyday.com/limelm/help/offline-activation/
                var gr = ta.IsGenuine(DaysBetweenChecks, GracePeriodLength);

                isGenuine = gr == IsGenuineResult.Genuine ||
                            gr == IsGenuineResult.GenuineFeaturesChanged ||

                            // an internet error means the user is activated but
                            // TurboActivate failed to contact the LimeLM servers
                            gr == IsGenuineResult.InternetError;

                // If IsGenuineEx() is telling us we're not activated
                // but the IsActivated() function is telling us that the activation
                // data on the computer is valid (i.e. the crypto-signed-fingerprint matches the computer)
                // then that means that the customer has passed the grace period and they must re-verify
                // with the servers to continue to use your app.

                //Note: DO NOT allow the customer to just continue to use your app indefinitely with absolutely
                //      no reverification with the servers. If you want to do that then don't use IsGenuine() or
                //      IsGenuineEx() at all -- just use IsActivated().
                if (!isGenuine && ta.IsActivated())
                {
                    // We're treating the customer as is if they aren't activated, so they can't use your app.

                    // However, we show them a dialog where they can reverify with the servers immediately.

                    var frmReverify = new ReVerifyNow(ta, DaysBetweenChecks, GracePeriodLength);

                    if (frmReverify.ShowDialog(this) == DialogResult.OK)
                    {
                        isGenuine = true;
                    }
                    else if (!frmReverify.noLongerActivated) // the user clicked cancel and the user is still activated
                    {
                        InitPlugin.Activated = false;
                        return;
                    }
                }
            }
            catch (TurboActivateException ex)
            {
                // failed to check if activated, meaning the customer screwed
                // something up so kill the app immediately
                MessageBox.Show("Failed to check if activated: " + ex.Message);
                InitPlugin.Activated = false;
                return;
            }

            ShowTrial(!isGenuine);
            InitPlugin.Activated = ta.IsActivated() && isGenuine;
        }

        private void mnuActDeact_Click(object sender, EventArgs e)
        {
            if (isGenuine)
            {
                // deactivate product without deleting the product key
                // allows the user to easily reactivate
                try
                {
                    InitPlugin.Activated = false;
                    ta.Deactivate(true);
                }
                catch (TurboActivateException ex)
                {
                    MessageBox.Show("Failed to deactivate: " + ex.Message);
                    return;
                }

                isGenuine = false;
                ShowTrial(true);
            }
            else
            {
                // Note: you can launch the TurboActivate wizard
                //       or you can create you own interface

                // launch TurboActivate.exe to get the product key from
                // the user, and activate.
                var TAProcess = new Process
                {
                    StartInfo =
                    {
                        FileName = Path.Combine(
                            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                            "TurboActivate.exe"
                        )
                    },
                    EnableRaisingEvents = true
                };

                TAProcess.Exited += p_Exited;
                TAProcess.Start();
            }
        }

        /// <summary>This event handler is called when TurboActivate.exe closes.</summary>
        private void p_Exited(object sender, EventArgs e)
        {
            // remove the event
            ((Process) sender).Exited -= p_Exited;

            // the UI thread is running asynchronous to TurboActivate closing
            // that's why we can't call CheckIfActivated(); directly
            Invoke(new IsActivatedDelegate(CheckIfActivated));
        }

        /// <summary>Rechecks if we're activated -- if so enable the app features.</summary>
        private void CheckIfActivated()
        {
            var isNowActivated = false;

            try
            {
                isNowActivated = ta.IsActivated();
            }
            catch (TurboActivateException ex)
            {
                MessageBox.Show("Failed to check if activated: " + ex.Message);
                return;
            }

            // recheck if activated
            if (isNowActivated)
            {
                isGenuine = true;
                ReEnableAppFeatures();
                ShowTrial(false);
            }
            else // maybe the user entered a trial extension
            {
                RecheckTrialLength();
            }
        }

        /// <summary>Put this app in either trial mode or "full mode"</summary>
        /// <param name="show">If true show the trial, otherwise hide the trial.</param>
        private void ShowTrial(bool show)
        {
            lblTrialMessage.Visible = show;

            mnuActDeact.Text = show ? "Activate..." : "Deactivate";

            if (show)
            {
                uint trialDaysRemaining = 0;

                try
                {
                    ta.UseTrial(trialFlags);

                    // get the number of remaining trial days
                    trialDaysRemaining = ta.TrialDaysRemaining(trialFlags);
                }
                catch (TrialExpiredException)
                {
                    // do nothing because trialDaysRemaining is already set to 0
                }
                catch (TurboActivateException ex)
                {
                    MessageBox.Show("Failed to start the trial: " + ex.Message);
                }

                // if no more trial days then disable all app features
                if (trialDaysRemaining == 0)
                    DisableAppFeatures();
                else
                {
                    InitPlugin.HasTime = true;
                    AcAp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nRabCab is in trial mode! Your trial expires in " + trialDaysRemaining + " days.");
                    lblTrialMessage.Text = "Your trial expires in " + trialDaysRemaining + " days.";
                }
                    
            }
        }

        /// <summary>Change this function to disable the features of your app.</summary>
        /// <param name="timeFraudFlag">true if the trial has expired due to date/time fraud.</param>
        private void DisableAppFeatures(bool timeFraudFlag = false)
        {
            //TODO: disable all the features of the program
            txtMain.Enabled = false;
            InitPlugin.Activated = false;

            if (!timeFraudFlag)
                lblTrialMessage.Text = "The trial has expired. Get an extension at Example.com";
            else
                lblTrialMessage.Text = "The trial has expired due to date/time fraud detected";
        }

        /// <summary>Change this function to re-enable the features of your app.</summary>
        private void ReEnableAppFeatures()
        {
            InitPlugin.Activated = true;
            //TODO: re-enable all the features of the program
            txtMain.Enabled = true;
        }

        // Recheck to see if the trial has been extended. If so, re-enable app features.
        private void RecheckTrialLength()
        {
            // get the number of remaining trial days
            uint trialDaysRemaining = 0;

            try
            {
                trialDaysRemaining = ta.TrialDaysRemaining(trialFlags);
            }
            catch (TurboActivateException ex)
            {
                MessageBox.Show("Failed to get the trial days remaining: " + ex.Message);
            }

            // if more trial days then re-enable all app features
            if (trialDaysRemaining > 0)
            {
                ReEnableAppFeatures();
                lblTrialMessage.Text = "Your trial expires in " + trialDaysRemaining + " days.";
            }
        }

        private void btnExtendTrial_Click(object sender, EventArgs e)
        {
        }

        /// <summary>
        ///     This function is called only if ta.UseTrial() has been called once in this process lifetime and if the trial has
        ///     expired since that call.
        /// </summary>
        /// <param name="sender">The sending object.</param>
        /// <param name="e">The status of the trial (expired naturally or by fraud)</param>
        private void trialChange(object sender, StatusArgs e)
        {
            // disable the features of your app
            DisableAppFeatures(e.Status == TA_TrialStatus.TA_CB_EXPIRED_FRAUD);
        }

        private delegate void IsActivatedDelegate();
    }
}