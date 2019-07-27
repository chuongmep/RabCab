using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace wyDay.TurboActivate
{ /*
    public static class OperatingSystem
    {

        public static bool IsWindows() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public static bool IsMacOS() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        public static bool IsLinux() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    }
    */

    [Flags]
    public enum TA_Flags : uint
    {
        TA_SYSTEM = 1,
        TA_USER = 2,

        /// <summary>
        ///     Use the TA_DISALLOW_VM in UseTrial() to disallow trials in virtual machines.
        ///     If you use this flag in UseTrial() and the customer's machine is a Virtual
        ///     Machine, then UseTrial() will throw VirtualMachineException.
        /// </summary>
        TA_DISALLOW_VM = 4,

        /// <summary>
        ///     Use this flag in TA_UseTrial() to tell TurboActivate to use client-side
        ///     unverified trials. For more information about verified vs. unverified trials,
        ///     see here: https://wyday.com/limelm/help/trials/
        ///     Note: unverified trials are unsecured and can be reset by malicious customers.
        /// </summary>
        TA_UNVERIFIED_TRIAL = 16,

        /// <summary>
        ///     Use the TA_VERIFIED_TRIAL flag to use verified trials instead
        ///     of unverified trials. This means the trial is locked to a particular computer.
        ///     The customer can't reset the trial.
        /// </summary>
        TA_VERIFIED_TRIAL = 32
    }

    [Flags]
    public enum TA_DateCheckFlags : uint
    {
        /// <summary>TAHasNotExpired when passed into IsDateValid() verifies that the passed in UTC date-time has not elapsed.</summary>
        TA_HAS_NOT_EXPIRED = 1
    }


    /// <summary>Represents the method that will handle an event that is cancelable.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A StatusArgs that shows you the result of the trial change.</param>
    public delegate void TrialCallbackHandler(object sender, StatusArgs e);

    public enum TA_TrialStatus : uint
    {
        /// <summary>Callback-status value used when the trial has expired.</summary>
        TA_CB_EXPIRED = 0,

        /// <summary>Callback-status value used when the trial has expired due to date/time fraud.</summary>
        TA_CB_EXPIRED_FRAUD = 1
    }

    /// <summary>Event data for a trial callback.</summary>
    public class StatusArgs : EventArgs
    {
        /// <summary>The status of the trial.</summary>
        public TA_TrialStatus Status { get; set; }
    }


    public class TurboActivate
    {
        private const int TA_OK = 0x00;
        private const int TA_FAIL = 0x01;
        private const int TA_E_PKEY = 0x02;
        private const int TA_E_ACTIVATE = 0x03;
        private const int TA_E_INET = 0x04;
        private const int TA_E_INUSE = 0x05;
        private const int TA_E_REVOKED = 0x06;
        private const int TA_E_PDETS = 0x08;
        private const int TA_E_TRIAL = 0x09;
        private const int TA_E_COM = 0x0B;
        private const int TA_E_TRIAL_EUSED = 0x0C;
        private const int TA_E_TRIAL_EEXP = 0x0D;
        private const int TA_E_EXPIRED = 0x0D;
        private const int TA_E_INSUFFICIENT_BUFFER = 0x0E; // never should get this in our code
        private const int TA_E_PERMISSION = 0x0F;
        private const int TA_E_INVALID_FLAGS = 0x10;
        private const int TA_E_IN_VM = 0x11;
        private const int TA_E_EDATA_LONG = 0x12;
        private const int TA_E_INVALID_ARGS = 0x13;
        private const int TA_E_KEY_FOR_TURBOFLOAT = 0x14;
        private const int TA_E_INET_DELAYED = 0x15;
        private const int TA_E_FEATURES_CHANGED = 0x16;
        private const int TA_E_NO_MORE_DEACTIVATIONS = 0x18;
        private const int TA_E_ACCOUNT_CANCELED = 0x19;
        private const int TA_E_ALREADY_ACTIVATED = 0x1A;
        private const int TA_E_INVALID_HANDLE = 0x1B;
        private const int TA_E_ENABLE_NETWORK_ADAPTERS = 0x1C;
        private const int TA_E_ALREADY_VERIFIED_TRIAL = 0x1D;
        private const int TA_E_TRIAL_EXPIRED = 0x1E;
        private const int TA_E_MUST_SPECIFY_TRIAL_TYPE = 0x1F;
        private const int TA_E_MUST_USE_TRIAL = 0x20;
        private const int TA_E_NO_MORE_TRIALS_ALLOWED = 0x21;
        private const int TA_E_BROKEN_WMI = 0x22;
        private const int TA_E_INET_TIMEOUT = 0x23;
        private const int TA_E_INET_TLS = 0x24;
        private readonly uint handle;

        /// <summary>The async operation variable to manage the various thread contexts without forcing the user to do this.</summary>
        private AsyncOperation _operation;

        /// <summary>
        ///     Storing the native callback delegate instance so it doesn't get garbage collected (.NET garbage collector will
        ///     collect a local delegate if we just pass it to the native function like this TA_SetTrialCallback(TrialCallbackFn) )
        /// </summary>
        private TrialCallbackType privTrialCallback;

        /// <summary>Creates a TurboActivate object instance.</summary>
        /// <param name="vGUID">The GUID for this product version. This is found on the LimeLM site on the version overview.</param>
        /// <param name="pdetsFilename">The absolute location to the TurboActivate.dat file on the disk.</param>
        public TurboActivate(string vGUID, string pdetsFilename = null)
        {
            if (pdetsFilename != null)
            {
#if TA_BOTH_DLL
                switch (IntPtr.Size == 8
                    ? Native64.TA_PDetsFromPath(pdetsFilename)
                    : Native.TA_PDetsFromPath(pdetsFilename))
#else
                switch (Native.TA_PDetsFromPath(pdetsFilename))
#endif
                {
                    case TA_OK: // successful
                        break;
                    case TA_E_PDETS:
                        throw new ProductDetailsException();
                    case TA_FAIL:
                        // the TurboActivate.dat already loaded.
                        break;
                    default:
                        throw new TurboActivateException("The TurboActivate.dat file failed to load.");
                }
            }

            VersionGUID = vGUID;

#if TA_BOTH_DLL
            handle = IntPtr.Size == 8 ? Native64.TA_GetHandle(VersionGUID) : Native.TA_GetHandle(VersionGUID);
#else
            handle = Native.TA_GetHandle(versGUID);
#endif

            // if the handle is still unset then immediately throw an exception
            // telling the user that they need to actually load the correct
            // TurboActivate.dat and/or use the correct GUID for the TurboActivate.dat
            if (handle == 0)
                throw new ProductDetailsException();
        }

        /// <summary>Creates a TurboActivate object instance.</summary>
        /// <param name="vGUID">The GUID for this product version. This is found on the LimeLM site on the version overview.</param>
        /// <param name="pdetsData">The TurboActivate.dat file loaded into a byte array.</param>
        public TurboActivate(string vGUID, byte[] pdetsData)
        {
#if TA_BOTH_DLL
            switch (IntPtr.Size == 8
                ? Native64.TA_PDetsFromByteArray(pdetsData, pdetsData.Length)
                : Native.TA_PDetsFromByteArray(pdetsData, pdetsData.Length))
#else
            switch (Native.TA_PDetsFromByteArray(pdetsData, pdetsData.Length))
#endif
            {
                case TA_OK: // successful
                    break;
                case TA_E_PDETS:
                    throw new ProductDetailsException();
                case TA_FAIL:
                    // the TurboActivate.dat already loaded.
                    break;
                default:
                    throw new TurboActivateException("The TurboActivate.dat file failed to load.");
            }


            VersionGUID = vGUID;

#if TA_BOTH_DLL
            handle = IntPtr.Size == 8 ? Native64.TA_GetHandle(VersionGUID) : Native.TA_GetHandle(VersionGUID);
#else
            handle = Native.TA_GetHandle(versGUID);
#endif

            // if the handle is still unset then immediately throw an exception
            // telling the user that they need to actually load the correct
            // TurboActivate.dat and/or use the correct GUID for the TurboActivate.dat
            if (handle == 0)
                throw new ProductDetailsException();
        }

        /// <summary>The GUID for this product version. This is found on the LimeLM site on the version overview.</summary>
        public string VersionGUID { get; }

        /// <summary>
        ///     Holds the actual callback event, while the public version does a whole bunch of junk to initialize the
        ///     callback with native code, etc.
        /// </summary>
        private event TrialCallbackHandler privTrialChange;

        /// <summary>Event is raised when the trial status changes.</summary>
        public event TrialCallbackHandler TrialChange
        {
            add
            {
                if (privTrialChange == null)
                {
                    // prevent garbage collection of the delegate by storing it in a local variable.
                    privTrialCallback = TrialCallbackFn;

#if TA_BOTH_DLL
                    switch (IntPtr.Size == 8
                        ? Native64.TA_SetTrialCallback(handle, privTrialCallback)
                        : Native.TA_SetTrialCallback(handle, privTrialCallback))
#else
                    switch (Native.TA_SetTrialCallback(handle, privTrialCallback))
#endif
                    {
                        case TA_E_INVALID_HANDLE:
                            throw new InvalidHandleException();
                        case TA_OK: // successful
                            break;
                        default:
                            throw new TurboActivateException("Failed to save trial callback.");
                    }
                }

                privTrialChange += value;
            }
            remove
            {
                privTrialChange -= value;

                if (privTrialChange == null)
                {
                    privTrialChange += value;
                    throw new TurboActivateException("You must have at least one subscriber to the TrialChange event.");
                }
            }
        }


        private static TurboActivateException taHresultToExcep(int ret, string funcName)
        {
            switch (ret)
            {
                case TA_FAIL:
                    return new TurboActivateException(funcName + " general failure.");
                case TA_E_PKEY:
                    return new InvalidProductKeyException();
                case TA_E_ACTIVATE:
                    return new NotActivatedException();
                case TA_E_INET:
                    return new InternetException();
                case TA_E_INUSE:
                    return new PkeyMaxUsedException();
                case TA_E_REVOKED:
                    return new PkeyRevokedException();
                case TA_E_TRIAL:
                    return new TrialDateCorruptedException();
                case TA_E_COM:
                    return new COMException();
                case TA_E_TRIAL_EUSED:
                    return new TrialExtUsedException();
                case TA_E_EXPIRED:
                    return new DateTimeException();
                case TA_E_PERMISSION:
                    return new PermissionException();
                case TA_E_INVALID_FLAGS:
                    return new InvalidFlagsException();
                case TA_E_IN_VM:
                    return new VirtualMachineException();
                case TA_E_EDATA_LONG:
                    return new ExtraDataTooLongException();
                case TA_E_INVALID_ARGS:
                    return new InvalidArgsException();
                case TA_E_KEY_FOR_TURBOFLOAT:
                    return new TurboFloatKeyException();
                case TA_E_NO_MORE_DEACTIVATIONS:
                    return new NoMoreDeactivationsException();
                case TA_E_ACCOUNT_CANCELED:
                    return new AccountCanceledException();
                case TA_E_ALREADY_ACTIVATED:
                    return new AlreadyActivatedException();
                case TA_E_INVALID_HANDLE:
                    return new InvalidHandleException();
                case TA_E_ENABLE_NETWORK_ADAPTERS:
                    return new EnableNetworkAdaptersException();
                case TA_E_ALREADY_VERIFIED_TRIAL:
                    return new AlreadyVerifiedTrialException();
                case TA_E_TRIAL_EXPIRED:
                    return new TrialExpiredException();
                case TA_E_MUST_SPECIFY_TRIAL_TYPE:
                    return new MustSpecifyTrialTypeException();
                case TA_E_MUST_USE_TRIAL:
                    return new MustUseTrialException();
                case TA_E_NO_MORE_TRIALS_ALLOWED:
                    return new NoMoreTrialsAllowedException();
                case TA_E_BROKEN_WMI:
                    return new BrokenWMIException();
                case TA_E_INET_TIMEOUT:
                    return new InternetTimeoutException();
                case TA_E_INET_TLS:
                    return new InternetTLSException();
                default:

                    // Make sure you're using the latest TurboActivate.cs, we occassionally add new error codes
                    // and you need latest version of this file to get a detailed description of the error.

                    // More information about upgrading here: https://wyday.com/limelm/help/faq/#update-libs

                    // You can also view error directly from the source: TurboActivate.h
                    return new TurboActivateException(funcName + " failed with an unknown error code: " + ret);
            }
        }

        private void TrialCallbackFn(uint status)
        {
            if (_operation != null)
            {
                var lStatus = (TA_TrialStatus) status;

                // call the event from the calling thread's context
                var e = new StatusArgs {Status = lStatus};

                // finish off the operation
                _operation.PostOperationCompleted(RaiseTrialCallbackFn, e);
            }
        }

        private void RaiseTrialCallbackFn(object args)
        {
            var stArgs = (StatusArgs) args;

            // null out the _operation
            _operation = null;

            privTrialChange(null, stArgs);
        }


        /// <summary>
        ///     Activates the product on this computer. You must call <see cref="CheckAndSavePKey(string)" /> with a valid
        ///     product key or have used the TurboActivate wizard sometime before calling this function.
        /// </summary>
        /// <param name="extraData">
        ///     Extra data to pass to the LimeLM servers that will be visible for you to see and use. Maximum
        ///     size is 255 UTF-8 characters.
        /// </param>
        public void Activate(string extraData = null)
        {
            int ret;

            if (extraData != null)
            {
                var opts = new Native.ACTIVATE_OPTIONS {sExtraData = extraData};
                opts.nLength = (uint) Marshal.SizeOf(opts);

#if TA_BOTH_DLL
                ret = IntPtr.Size == 8 ? Native64.TA_Activate(handle, ref opts) : Native.TA_Activate(handle, ref opts);
#else
                ret = Native.TA_Activate(handle, ref opts);
#endif
            }
            else
            {
#if TA_BOTH_DLL
                ret = IntPtr.Size == 8
                    ? Native64.TA_Activate(handle, IntPtr.Zero)
                    : Native.TA_Activate(handle, IntPtr.Zero);
#else
                ret = Native.TA_Activate(handle, IntPtr.Zero);
#endif
            }

            if (ret != TA_OK)
                throw taHresultToExcep(ret, "Activate");
        }

        /// <summary>
        ///     Get the "activation request" file for offline activation. You must call
        ///     <see cref="CheckAndSavePKey(string)" /> with a valid product key or have used the TurboActivate wizard sometime
        ///     before calling this function.
        /// </summary>
        /// <param name="filename">The location where you want to save the activation request file.</param>
        /// <param name="extraData">
        ///     Extra data to pass to the LimeLM servers that will be visible for you to see and use. Maximum
        ///     size is 255 UTF-8 characters.
        /// </param>
        public void ActivationRequestToFile(string filename, string extraData)
        {
            int ret;

            if (extraData != null)
            {
                var opts = new Native.ACTIVATE_OPTIONS {sExtraData = extraData};
                opts.nLength = (uint) Marshal.SizeOf(opts);

#if TA_BOTH_DLL
                ret = IntPtr.Size == 8
                    ? Native64.TA_ActivationRequestToFile(handle, filename, ref opts)
                    : Native.TA_ActivationRequestToFile(handle, filename, ref opts);
#else
                ret = Native.TA_ActivationRequestToFile(handle, filename, ref opts);
#endif
            }
            else
            {
#if TA_BOTH_DLL
                ret = IntPtr.Size == 8
                    ? Native64.TA_ActivationRequestToFile(handle, filename, IntPtr.Zero)
                    : Native.TA_ActivationRequestToFile(handle, filename, IntPtr.Zero);
#else
                ret = Native.TA_ActivationRequestToFile(handle, filename, IntPtr.Zero);
#endif
            }

            if (ret != TA_OK)
                throw taHresultToExcep(ret, "ActivationRequestToFile");
        }

        /// <summary>Activate from the "activation response" file for offline activation.</summary>
        /// <param name="filename">The location of the activation response file.</param>
        public void ActivateFromFile(string filename)
        {
            int ret;

#if TA_BOTH_DLL
            ret = IntPtr.Size == 8
                ? Native64.TA_ActivateFromFile(handle, filename)
                : Native.TA_ActivateFromFile(handle, filename);
#else
            ret = Native.TA_ActivateFromFile(handle, filename);
#endif

            if (ret != TA_OK)
                throw taHresultToExcep(ret, "ActivateFromFile");
        }

        /// <summary>Checks and saves the product key.</summary>
        /// <param name="productKey">The product key you want to save.</param>
        /// <param name="flags">
        ///     Whether to create the activation either user-wide or system-wide. Valid flags are
        ///     <see cref="TA_Flags.TA_SYSTEM" /> and <see cref="TA_Flags.TA_USER" />.
        /// </param>
        /// <returns>True if the product key is valid, false if it's not</returns>
        public bool CheckAndSavePKey(string productKey, TA_Flags flags = TA_Flags.TA_SYSTEM)
        {
            int ret;

#if TA_BOTH_DLL
            ret = IntPtr.Size == 8
                ? Native64.TA_CheckAndSavePKey(handle, productKey, flags)
                : Native.TA_CheckAndSavePKey(handle, productKey, flags);
#else
            ret = Native.TA_CheckAndSavePKey(handle, productKey, flags);
#endif

            switch (ret)
            {
                case TA_OK: // successful
                    return true;
                case TA_FAIL: // not successful
                    return false;

                default:
                    throw taHresultToExcep(ret, "CheckAndSavePKey");
            }
        }

        /// <summary>Deactivates the product on this computer.</summary>
        /// <param name="eraseProductKey">
        ///     Erase the product key so the user will have to enter a new product key if they wish to
        ///     reactivate.
        /// </param>
        public void Deactivate(bool eraseProductKey = false)
        {
            int ret;

#if TA_BOTH_DLL
            ret = IntPtr.Size == 8
                ? Native64.TA_Deactivate(handle, (byte) (eraseProductKey ? 1 : 0))
                : Native.TA_Deactivate(handle, (byte) (eraseProductKey ? 1 : 0));
#else
            ret = Native.TA_Deactivate(handle, (byte)(eraseProductKey ? 1 : 0));
#endif

            if (ret != TA_OK)
                throw taHresultToExcep(ret, "Deactivate");
        }

        /// <summary>Get the "deactivation request" file for offline deactivation.</summary>
        /// <param name="filename">The location where you want to save the deactivation request file.</param>
        /// <param name="eraseProductKey">
        ///     Erase the product key so the user will have to enter a new product key if they wish to
        ///     reactivate.
        /// </param>
        public void DeactivationRequestToFile(string filename, bool eraseProductKey = false)
        {
            int ret;

#if TA_BOTH_DLL
            ret = IntPtr.Size == 8
                ? Native64.TA_DeactivationRequestToFile(handle, filename, (byte) (eraseProductKey ? 1 : 0))
                : Native.TA_DeactivationRequestToFile(handle, filename, (byte) (eraseProductKey ? 1 : 0));
#else
            ret = Native.TA_DeactivationRequestToFile(handle, filename, (byte)(eraseProductKey ? 1 : 0));
#endif

            if (ret != TA_OK)
                throw taHresultToExcep(ret, "DeactivationRequestToFile");
        }

        /// <summary>Gets the extra data value you passed in when activating.</summary>
        /// <returns>Returns the extra data if it exists, otherwise it returns null.</returns>
        public string GetExtraData()
        {
#if TA_BOTH_DLL
            var length = IntPtr.Size == 8
                ? Native64.TA_GetExtraData(handle, null, 0)
                : Native.TA_GetExtraData(handle, null, 0);
#else
            int length = Native.TA_GetExtraData(handle, null, 0);
#endif

            var sb = new StringBuilder(length);

#if TA_BOTH_DLL
            switch (IntPtr.Size == 8
                ? Native64.TA_GetExtraData(handle, sb, length)
                : Native.TA_GetExtraData(handle, sb, length))
#else
            switch (Native.TA_GetExtraData(handle, sb, length))
#endif
            {
                case TA_E_INVALID_HANDLE:
                    throw new InvalidHandleException();
                case TA_OK: // success
                    return sb.ToString();
                default:
                    return null;
            }
        }

        /// <summary>Gets the value of a feature.</summary>
        /// <param name="featureName">The name of the feature to retrieve the value for.</param>
        /// <returns>Returns the feature value.</returns>
        public string GetFeatureValue(string featureName)
        {
            var value = GetFeatureValue(featureName, null);

            if (value == null)
                throw new TurboActivateException("Failed to get feature value. The feature doesn't exist.");

            return value;
        }

        /// <summary>Gets the value of a custom license field.</summary>
        /// <param name="featureName">The name of the feature to retrieve the value for.</param>
        /// <param name="defaultValue">The default value to return if the feature doesn't exist.</param>
        /// <returns>Returns the feature value if it exists, otherwise it returns the default value.</returns>
        public string GetFeatureValue(string featureName, string defaultValue)
        {
#if TA_BOTH_DLL
            var length = IntPtr.Size == 8
                ? Native64.TA_GetFeatureValue(handle, featureName, null, 0)
                : Native.TA_GetFeatureValue(handle, featureName, null, 0);
#else
            int length = Native.TA_GetFeatureValue(handle, featureName, null, 0);
#endif

            var sb = new StringBuilder(length);

#if TA_BOTH_DLL
            switch (IntPtr.Size == 8
                ? Native64.TA_GetFeatureValue(handle, featureName, sb, length)
                : Native.TA_GetFeatureValue(handle, featureName, sb, length))
#else
            switch (Native.TA_GetFeatureValue(handle, featureName, sb, length))
#endif
            {
                case TA_E_INVALID_HANDLE:
                    throw new InvalidHandleException();
                case TA_OK: // success
                    return sb.ToString();
                default:
                    return defaultValue;
            }
        }

        /// <summary>
        ///     Gets the stored product key. NOTE: if you want to check if a product key is valid simply call
        ///     <see cref="IsProductKeyValid()" />. If you want to check if your app is locked to the computer then call
        ///     IsGenuineEx() or IsActivated().
        /// </summary>
        /// <returns>string Product key.</returns>
        public string GetPKey()
        {
            // this makes the assumption that the PKey is 34+NULL characters long.
            // This may or may not be true in the future.
            var sb = new StringBuilder(35);

#if TA_BOTH_DLL
            switch (IntPtr.Size == 8 ? Native64.TA_GetPKey(handle, sb, 35) : Native.TA_GetPKey(handle, sb, 35))
#else
            switch (Native.TA_GetPKey(handle, sb, 35))
#endif
            {
                case TA_E_PKEY:
                    throw new InvalidProductKeyException();
                case TA_E_INVALID_HANDLE:
                    throw new InvalidHandleException();
                case TA_OK: // success
                    return sb.ToString();
                default:
                    throw new TurboActivateException("Failed to get the product key.");
            }
        }

        /// <summary>Checks whether the computer has been activated.</summary>
        /// <returns>True if the computer is activated. False otherwise.</returns>
        public bool IsActivated()
        {
#if TA_BOTH_DLL
            var ret = IntPtr.Size == 8 ? Native64.TA_IsActivated(handle) : Native.TA_IsActivated(handle);
#else
            int ret = Native.TA_IsActivated(handle);
#endif

            switch (ret)
            {
                case TA_OK: // is activated
                    return true;
                case TA_FAIL: // not activated
                    return false;
                default:
                    throw taHresultToExcep(ret, "IsActivated");
            }
        }

        /// <summary>
        ///     Checks if the string in the form "YYYY-MM-DD HH:mm:ss" is a valid date/time. The date must be in UTC time and
        ///     "24-hour" format. If your date is in some other time format first convert it to UTC time before passing it into
        ///     this function.
        /// </summary>
        /// <param name="date_time">The date time string to check.</param>
        /// <param name="flags">The type of date time check. Valid flags are <see cref="TA_DateCheckFlags.TA_HAS_NOT_EXPIRED" />.</param>
        /// <returns>True if the date is valid, false if it's not</returns>
        public bool IsDateValid(string date_time, TA_DateCheckFlags flags)
        {
#if TA_BOTH_DLL
            var ret = IntPtr.Size == 8
                ? Native64.TA_IsDateValid(handle, date_time, flags)
                : Native.TA_IsDateValid(handle, date_time, flags);
#else
            int ret = Native.TA_IsDateValid(handle, date_time, flags);
#endif

            switch (ret)
            {
                case TA_OK: // date is valid and not expired
                    return true;
                case TA_FAIL: // date is invalid or not expired
                    return false;
                default:
                    throw taHresultToExcep(ret, "IsDateValid");
            }
        }

        /// <summary>Checks whether the computer is genuinely activated by verifying with the LimeLM servers.</summary>
        /// <returns>IsGenuineResult</returns>
        public IsGenuineResult IsGenuine()
        {
#if TA_BOTH_DLL
            var ret = IntPtr.Size == 8 ? Native64.TA_IsGenuine(handle) : Native.TA_IsGenuine(handle);
#else
            int ret = Native.TA_IsGenuine(handle);
#endif

            switch (ret)
            {
                case TA_OK: // is activated and/or Genuine
                    return IsGenuineResult.Genuine;

                case TA_E_FEATURES_CHANGED:
                    return IsGenuineResult.GenuineFeaturesChanged;

                case TA_E_INET:
                    return IsGenuineResult.InternetError;

                case TA_FAIL:
                case TA_E_REVOKED:
                case TA_E_ACTIVATE:
                    return IsGenuineResult.NotGenuine;

                case TA_E_IN_VM:
                    return IsGenuineResult.NotGenuineInVM;

                default:
                    throw taHresultToExcep(ret, "IsGenuine");
            }
        }

        /// <summary>
        ///     Checks whether the computer is activated, and every "daysBetweenChecks" days it check if the customer is
        ///     genuinely activated by verifying with the LimeLM servers.
        /// </summary>
        /// <param name="daysBetweenChecks">How often to contact the LimeLM servers for validation. 90 days recommended.</param>
        /// <param name="graceDaysOnInetErr">
        ///     If the call fails because of an internet error, how long, in days, should the grace period last (before returning
        ///     deactivating and returning TA_FAIL).
        ///     14 days is recommended.
        /// </param>
        /// <param name="skipOffline">
        ///     If the user activated using offline activation
        ///     (ActivateRequestToFile(), ActivateFromFile() ), then with this
        ///     option IsGenuineEx() will still try to validate with the LimeLM
        ///     servers, however instead of returning <see cref="IsGenuineResult.InternetError" /> (when within the
        ///     grace period) or <see cref="IsGenuineResult.NotGenuine" /> (when past the grace period) it will
        ///     instead only return <see cref="IsGenuineResult.Genuine" /> (if IsActivated()).
        ///     If the user activated using online activation then this option
        ///     is ignored.
        /// </param>
        /// <param name="offlineShowInetErr">
        ///     If the user activated using offline activation, and you're
        ///     using this option in tandem with skipOffline, then IsGenuineEx()
        ///     will return <see cref="IsGenuineResult.InternetError" /> on internet failure instead of
        ///     <see cref="IsGenuineResult.Genuine" />.
        ///     If the user activated using online activation then this flag
        ///     is ignored.
        /// </param>
        /// <returns>IsGenuineResult</returns>
        public IsGenuineResult IsGenuine(uint daysBetweenChecks, uint graceDaysOnInetErr, bool skipOffline = false,
            bool offlineShowInetErr = false)
        {
            var opts = new Native.GENUINE_OPTIONS
                {nDaysBetweenChecks = daysBetweenChecks, nGraceDaysOnInetErr = graceDaysOnInetErr, flags = 0};
            opts.nLength = (uint) Marshal.SizeOf(opts);

            if (skipOffline)
            {
                opts.flags = Native.GenuineFlags.TA_SKIP_OFFLINE;

                if (offlineShowInetErr)
                    opts.flags |= Native.GenuineFlags.TA_OFFLINE_SHOW_INET_ERR;
            }

            int ret;

#if TA_BOTH_DLL
            ret = IntPtr.Size == 8
                ? Native64.TA_IsGenuineEx(handle, ref opts)
                : Native.TA_IsGenuineEx(handle, ref opts);
#else
            ret = Native.TA_IsGenuineEx(handle, ref opts);
#endif
            switch (ret)
            {
                case TA_OK: // is activated and/or Genuine
                    return IsGenuineResult.Genuine;

                case TA_E_FEATURES_CHANGED:
                    return IsGenuineResult.GenuineFeaturesChanged;

                case TA_E_INET:
                case TA_E_INET_DELAYED:
                    return IsGenuineResult.InternetError;

                case TA_FAIL:
                case TA_E_REVOKED:
                case TA_E_ACTIVATE:
                    return IsGenuineResult.NotGenuine;

                case TA_E_IN_VM:
                    return IsGenuineResult.NotGenuineInVM;

                default:
                    throw taHresultToExcep(ret, "IsGenuineEx");
            }
        }

        /// <summary>
        ///     Get the number of days until the next time that the <see cref="IsGenuine(uint, uint, bool, bool)" /> function
        ///     contacts the LimeLM activation servers to reverify the activation.
        /// </summary>
        /// <param name="daysBetweenChecks">
        ///     How often to contact the LimeLM servers for validation. Use the exact same value as
        ///     used in <see cref="IsGenuine(uint, uint, bool, bool)" />.
        /// </param>
        /// <param name="graceDaysOnInetErr">
        ///     If the call fails because of an internet error, how long, in days, should the grace
        ///     period last (before returning deactivating and returning TA_FAIL). Again, use the exact same value as used in
        ///     <see cref="IsGenuine(uint, uint, bool, bool)" />.
        /// </param>
        /// <param name="inGracePeriod">Get whether the user is in the grace period.</param>
        /// <returns>
        ///     The number of days remaining. 0 days if both the days between checks and the grace period have expired. (E.g.
        ///     1 day means *at most* 1 day. That is, it could be 30 seconds.)
        /// </returns>
        public uint GenuineDays(uint daysBetweenChecks, uint graceDaysOnInetErr, ref bool inGracePeriod)
        {
            uint daysRemain = 0;
            var inGrace = (char) 0;

#if TA_BOTH_DLL
            var ret = IntPtr.Size == 8
                ? Native64.TA_GenuineDays(handle, daysBetweenChecks, graceDaysOnInetErr, ref daysRemain, ref inGrace)
                : Native.TA_GenuineDays(handle, daysBetweenChecks, graceDaysOnInetErr, ref daysRemain, ref inGrace);
#else
            int ret = Native.TA_GenuineDays(handle, daysBetweenChecks, graceDaysOnInetErr, ref daysRemain, ref inGrace);
#endif

            if (ret != TA_OK)
                throw taHresultToExcep(ret, "GenuineDays");

            // set whether we're in a grace period or not
            inGracePeriod = inGrace == (char) 1;

            return daysRemain;
        }

        /// <summary>
        ///     Checks if the product key installed for this product is valid. This does NOT check if the product key is
        ///     activated or genuine. Use <see cref="IsActivated()" /> and <see cref="IsGenuine(ref bool)" /> instead.
        /// </summary>
        /// <returns>True if the product key is valid.</returns>
        public bool IsProductKeyValid()
        {
#if TA_BOTH_DLL
            switch (IntPtr.Size == 8 ? Native64.TA_IsProductKeyValid(handle) : Native.TA_IsProductKeyValid(handle))
#else
            switch (Native.TA_IsProductKeyValid(handle))
#endif
            {
                case TA_E_INVALID_HANDLE:
                    throw new InvalidHandleException();
                case TA_OK: // is valid
                    return true;
            }

            // not valid
            return false;
        }

        /// <summary>Sets the custom proxy to be used by functions that connect to the internet.</summary>
        /// <param name="proxy">The proxy to use. Proxy must be in the form "http://username:password@host:port/".</param>
        public static void SetCustomProxy(string proxy)
        {
#if TA_BOTH_DLL
            if ((IntPtr.Size == 8 ? Native64.TA_SetCustomProxy(proxy) : Native.TA_SetCustomProxy(proxy)) != 0)
#else
            if (Native.TA_SetCustomProxy(proxy) != TA_OK)
#endif
                throw new TurboActivateException("Failed to set the custom proxy.");
        }

        /// <summary>
        ///     Get the number of trial days remaining. You must call <see cref="UseTrial()" /> at least once in the past
        ///     before calling this function.
        /// </summary>
        /// <param name="useTrialFlags">The same exact flags you passed to <see cref="UseTrial()" />.</param>
        /// <returns>
        ///     The number of days remaining. 0 days if the trial has expired. (E.g. 1 day means *at most* 1 day. That is it
        ///     could be 30 seconds.)
        /// </returns>
        public uint TrialDaysRemaining(TA_Flags useTrialFlags = TA_Flags.TA_SYSTEM | TA_Flags.TA_VERIFIED_TRIAL)
        {
            uint daysRemain = 0;

#if TA_BOTH_DLL
            var ret = IntPtr.Size == 8
                ? Native64.TA_TrialDaysRemaining(handle, useTrialFlags, ref daysRemain)
                : Native.TA_TrialDaysRemaining(handle, useTrialFlags, ref daysRemain);
#else
            int ret = Native.TA_TrialDaysRemaining(handle, useTrialFlags, ref daysRemain);
#endif
            if (ret != TA_OK)
                throw taHresultToExcep(ret, "TrialDaysRemaining");

            return daysRemain;
        }

        /// <summary>
        ///     Begins the trial the first time it's called. Calling it again will validate the trial data hasn't been
        ///     tampered with.
        /// </summary>
        /// <param name="flags">
        ///     Whether to create the trial (verified or unverified) either user-wide or system-wide and whether to
        ///     allow trials in virtual machines. Valid flags are <see cref="TA_Flags.TA_SYSTEM" />,
        ///     <see cref="TA_Flags.TA_USER" />, <see cref="TA_Flags.TA_DISALLOW_VM" />, <see cref="TA_Flags.TA_VERIFIED_TRIAL" />,
        ///     and <see cref="TA_Flags.TA_UNVERIFIED_TRIAL" />.
        /// </param>
        /// <param name="extraData">
        ///     Extra data to pass to the LimeLM servers that will be visible for you to see and use. Maximum
        ///     size is 255 UTF-8 characters.
        /// </param>
        public void UseTrial(TA_Flags flags = TA_Flags.TA_SYSTEM | TA_Flags.TA_VERIFIED_TRIAL, string extraData = null)
        {
#if TA_BOTH_DLL
            var ret = IntPtr.Size == 8
                ? Native64.TA_UseTrial(handle, flags, extraData)
                : Native.TA_UseTrial(handle, flags, extraData);
#else
            int ret = Native.TA_UseTrial(handle, flags, extraData);
#endif
            if (ret != TA_OK)
                throw taHresultToExcep(ret, "UseTrial");

            // create operation for trial callback (if not already created)
            if (_operation == null)
                _operation = AsyncOperationManager.CreateOperation(null);
        }

        /// <summary>
        ///     Generate a "verified trial" offline request file. This file will then need to be submitted to LimeLM. You will
        ///     then need to use the TA_UseTrialVerifiedFromFile() function with the response file from LimeLM to actually start
        ///     the trial.
        /// </summary>
        /// <param name="filename">The location where you want to save the trial request file.</param>
        /// <param name="extraData">
        ///     Extra data to pass to the LimeLM servers that will be visible for you to see and use. Maximum
        ///     size is 255 UTF-8 characters.
        /// </param>
        public void UseTrialVerifiedRequest(string filename, string extraData = null)
        {
#if TA_BOTH_DLL
            var ret = IntPtr.Size == 8
                ? Native64.TA_UseTrialVerifiedRequest(handle, filename, extraData)
                : Native.TA_UseTrialVerifiedRequest(handle, filename, extraData);
#else
            int ret = Native.TA_UseTrialVerifiedRequest(handle, filename, extraData);
#endif

            if (ret != TA_OK)
                throw taHresultToExcep(ret, "UseTrialVerifiedRequest");
        }

        /// <summary>Use the "verified trial response" from LimeLM to start the verified trial.</summary>
        /// <param name="filename">The location of the trial response file.</param>
        /// <param name="flags">
        ///     Whether to create the trial (verified or unverified) either user-wide or system-wide and whether to
        ///     allow trials in virtual machines. Valid flags are <see cref="TA_Flags.TA_SYSTEM" />,
        ///     <see cref="TA_Flags.TA_USER" />, <see cref="TA_Flags.TA_DISALLOW_VM" />, <see cref="TA_Flags.TA_VERIFIED_TRIAL" />,
        ///     and <see cref="TA_Flags.TA_UNVERIFIED_TRIAL" />.
        /// </param>
        public void UseTrialVerifiedFromFile(string filename,
            TA_Flags flags = TA_Flags.TA_SYSTEM | TA_Flags.TA_VERIFIED_TRIAL)
        {
#if TA_BOTH_DLL
            var ret = IntPtr.Size == 8
                ? Native64.TA_UseTrialVerifiedFromFile(handle, filename, flags)
                : Native.TA_UseTrialVerifiedFromFile(handle, filename, flags);
#else
            int ret = Native.TA_UseTrialVerifiedFromFile(handle, filename, flags);
#endif

            if (ret != TA_OK)
                throw taHresultToExcep(ret, "UseTrialVerifiedFromFile");

            // create operation for trial callback (if not already created)
            if (_operation == null)
                _operation = AsyncOperationManager.CreateOperation(null);
        }

        /// <summary>Extends the trial using a trial extension created in LimeLM.</summary>
        /// <param name="useTrialFlags">The same exact flags you passed to <see cref="UseTrial()" />.</param>
        /// <param name="trialExtension">The trial extension generated from LimeLM.</param>
        public void ExtendTrial(string trialExtension,
            TA_Flags useTrialFlags = TA_Flags.TA_SYSTEM | TA_Flags.TA_VERIFIED_TRIAL)
        {
#if TA_BOTH_DLL
            var ret = IntPtr.Size == 8
                ? Native64.TA_ExtendTrial(handle, useTrialFlags, trialExtension)
                : Native.TA_ExtendTrial(handle, useTrialFlags, trialExtension);
#else
            int ret = Native.TA_ExtendTrial(handle, useTrialFlags, trialExtension);
#endif

            if (ret != TA_OK)
                throw taHresultToExcep(ret, "ExtendTrial");

            // create operation for trial callback (if not already created)
            if (_operation == null)
                _operation = AsyncOperationManager.CreateOperation(null);
        }


        /// <summary>
        ///     This function allows you to set a custom folder to store the activation
        ///     data files. For normal use we do not recommend you use this function.
        ///     Only use this function if you absolutely must store data into a separate
        ///     folder. For example if your application runs on a USB drive and can't write
        ///     any files to the main disk, then you can use this function to save the activation
        ///     data files to a directory on the USB disk.
        ///     If you are using this function (which we only recommend for very special use-cases)
        ///     then you must call this function on every start of your program at the very top of
        ///     your app before any other functions are called.
        ///     The directory you pass in must already exist. And the process using TurboActivate
        ///     must have permission to create, write, and delete files in that directory.
        /// </summary>
        /// <param name="directory">The full directory to store the activation files.</param>
        public void SetCustomActDataPath(string directory)
        {
#if TA_BOTH_DLL
            var ret = IntPtr.Size == 8
                ? Native64.TA_SetCustomActDataPath(handle, directory)
                : Native.TA_SetCustomActDataPath(handle, directory);
#else
            int ret = Native.TA_SetCustomActDataPath(handle, directory);
#endif

            if (ret != TA_OK)
                throw taHresultToExcep(ret, "SetCustomActDataPath");
        }

        /// <summary>
        ///     Gets the version number of the currently used TurboActivate library.
        ///     This is a useful alternative for platforms which don't support file meta-data
        ///     (like Linux, FreeBSD, and other unix variants).
        /// </summary>
        /// <returns>Version class with the version number of the currently used TurboActivate library.</returns>
        public static Version GetVersion()
        {
            uint major, minor, build, rev;

            int ret;

#if TA_BOTH_DLL
            ret = IntPtr.Size == 8
                ? Native64.TA_GetVersion(out major, out minor, out build, out rev)
                : Native.TA_GetVersion(out major, out minor, out build, out rev);
#else
            ret = Native.TA_GetVersion(out major, out minor, out build, out rev);
#endif

            if (ret != TA_OK)
                throw taHresultToExcep(ret, "GetVersion");

            return new Version((int) major, (int) minor, (int) build, (int) rev);
        }


        /// <summary>The trial callback delegate.</summary>
        /// <param name="status">The status that's returned from TurboActivate to the callback function.</param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void TrialCallbackType(uint status);


        private static class Native
        {
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct ACTIVATE_OPTIONS
            {
                public uint nLength;

                [MarshalAs(UnmanagedType.LPWStr)] public string sExtraData;
            }

            [Flags]
            public enum GenuineFlags : uint
            {
                TA_SKIP_OFFLINE = 1,
                TA_OFFLINE_SHOW_INET_ERR = 2
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct GENUINE_OPTIONS
            {
                public uint nLength;
                public GenuineFlags flags;
                public uint nDaysBetweenChecks;
                public uint nGraceDaysOnInetErr;
            }

#if MACOS
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#elif UNIX
            [DllImport("libTurboActivate.so", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#else
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern uint TA_GetHandle(string versionGUID);

#if MACOS
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#elif UNIX
            [DllImport("libTurboActivate.so", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#else
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern int TA_Activate(uint handle, ref ACTIVATE_OPTIONS options);

#if MACOS
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#elif UNIX
            [DllImport("libTurboActivate.so", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#else
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern int TA_Activate(uint handle, IntPtr options);

#if MACOS
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#elif UNIX
            [DllImport("libTurboActivate.so", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#else
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern int TA_ActivationRequestToFile(uint handle, string filename,
                ref ACTIVATE_OPTIONS options);

#if MACOS
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#elif UNIX
            [DllImport("libTurboActivate.so", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#else
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern int TA_ActivationRequestToFile(uint handle, string filename, IntPtr options);

#if MACOS
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#elif UNIX
            [DllImport("libTurboActivate.so", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#else
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern int TA_ActivateFromFile(uint handle, string filename);

#if MACOS
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#elif UNIX
            [DllImport("libTurboActivate.so", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#else
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern int TA_CheckAndSavePKey(uint handle, string productKey, TA_Flags flags);

#if MACOS
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#elif UNIX
            [DllImport("libTurboActivate.so", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#else
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern int TA_Deactivate(uint handle, byte erasePkey);

#if MACOS
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#elif UNIX
            [DllImport("libTurboActivate.so", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#else
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern int TA_DeactivationRequestToFile(uint handle, string filename, byte erasePkey);

#if MACOS
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#elif UNIX
            [DllImport("libTurboActivate.so", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#else
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern int TA_GetExtraData(uint handle, StringBuilder lpValueStr, int cchValue);

#if MACOS
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#elif UNIX
            [DllImport("libTurboActivate.so", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#else
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern int TA_GetFeatureValue(uint handle, string featureName, StringBuilder lpValueStr,
                int cchValue);

#if MACOS
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#elif UNIX
            [DllImport("libTurboActivate.so", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#else
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern int TA_GetPKey(uint handle, StringBuilder lpPKeyStr, int cchPKey);

#if MACOS
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#elif UNIX
            [DllImport("libTurboActivate.so", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#else
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern int TA_IsActivated(uint handle);

#if MACOS
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#elif UNIX
            [DllImport("libTurboActivate.so", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#else
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern int TA_IsDateValid(uint handle, string date_time, TA_DateCheckFlags flags);

#if MACOS
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#elif UNIX
            [DllImport("libTurboActivate.so", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#else
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern int TA_IsGenuine(uint handle);

#if MACOS
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#elif UNIX
            [DllImport("libTurboActivate.so", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#else
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern int TA_IsGenuineEx(uint handle, ref GENUINE_OPTIONS options);

#if MACOS
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#elif UNIX
            [DllImport("libTurboActivate.so", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#else
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern int TA_GenuineDays(uint handle, uint nDaysBetweenChecks, uint nGraceDaysOnInetErr,
                ref uint DaysRemaining, ref char inGracePeriod);

#if MACOS
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#elif UNIX
            [DllImport("libTurboActivate.so", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#else
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern int TA_IsProductKeyValid(uint handle);

#if MACOS
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#elif UNIX
            [DllImport("libTurboActivate.so", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#else
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern int TA_SetCustomProxy(string proxy);

#if MACOS
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#elif UNIX
            [DllImport("libTurboActivate.so", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#else
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern int TA_TrialDaysRemaining(uint handle, TA_Flags useTrialFlags, ref uint DaysRemaining);

#if MACOS
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#elif UNIX
            [DllImport("libTurboActivate.so", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#else
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern int TA_UseTrial(uint handle, TA_Flags flags, string extra_data);

#if MACOS
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#elif UNIX
            [DllImport("libTurboActivate.so", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#else
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern int TA_UseTrialVerifiedRequest(uint handle, string filename, string extra_data);

#if MACOS
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#elif UNIX
            [DllImport("libTurboActivate.so", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#else
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern int TA_UseTrialVerifiedFromFile(uint handle, string filename, TA_Flags flags);

#if MACOS
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#elif UNIX
            [DllImport("libTurboActivate.so", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#else
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern int TA_ExtendTrial(uint handle, TA_Flags flags, string trialExtension);

#if MACOS
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#elif UNIX
            [DllImport("libTurboActivate.so", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#else
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern int TA_PDetsFromPath(string filename);

#if MACOS
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#elif UNIX
            [DllImport("libTurboActivate.so", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#else
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern int TA_PDetsFromByteArray(byte[] pArray, int nSize);

#if MACOS
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#elif UNIX
            [DllImport("libTurboActivate.so", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#else
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern int TA_SetCustomActDataPath(uint handle, string directory);

#if MACOS
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#elif UNIX
            [DllImport("libTurboActivate.so", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#else
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern int TA_SetTrialCallback(uint handle, TrialCallbackType callback);

#if MACOS
            [DllImport("libTurboActivate.dylib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#elif UNIX
            [DllImport("libTurboActivate.so", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
#else
            [DllImport("TurboActivate.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
#endif
            public static extern int TA_GetVersion(out uint MajorVersion, out uint MinorVersion, out uint BuildVersion,
                out uint RevisionVersion);
        }

        /*
         To use "AnyCPU" Target CPU type, first copy the x64 TurboActivate.dll and rename to TurboActivate64.dll
         Then in your project properties go to the Build panel, and add the TA_BOTH_DLL conditional compilation symbol.
        */

#if TA_BOTH_DLL
        private static class Native64
        {
            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern uint TA_GetHandle(string versionGUID);

            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern int TA_Activate(uint handle, ref Native.ACTIVATE_OPTIONS options);

            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern int TA_Activate(uint handle, IntPtr options);

            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern int TA_ActivationRequestToFile(uint handle, string filename,
                ref Native.ACTIVATE_OPTIONS options);

            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern int TA_ActivationRequestToFile(uint handle, string filename, IntPtr options);

            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern int TA_ActivateFromFile(uint handle, string filename);

            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern int TA_CheckAndSavePKey(uint handle, string productKey, TA_Flags flags);

            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern int TA_Deactivate(uint handle, byte erasePkey);

            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern int TA_DeactivationRequestToFile(uint handle, string filename, byte erasePkey);

            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern int TA_GetExtraData(uint handle, StringBuilder lpValueStr, int cchValue);

            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern int TA_GetFeatureValue(uint handle, string featureName, StringBuilder lpValueStr,
                int cchValue);

            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern int TA_GetPKey(uint handle, StringBuilder lpPKeyStr, int cchPKey);

            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern int TA_IsActivated(uint handle);

            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern int TA_IsDateValid(uint handle, string date_time, TA_DateCheckFlags flags);

            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern int TA_IsGenuine(uint handle);

            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern int TA_IsGenuineEx(uint handle, ref Native.GENUINE_OPTIONS options);

            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern int TA_GenuineDays(uint handle, uint nDaysBetweenChecks, uint nGraceDaysOnInetErr,
                ref uint DaysRemaining, ref char inGracePeriod);

            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern int TA_IsProductKeyValid(uint handle);

            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern int TA_SetCustomProxy(string proxy);

            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern int TA_TrialDaysRemaining(uint handle, TA_Flags useTrialFlags, ref uint DaysRemaining);

            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern int TA_UseTrial(uint handle, TA_Flags flags, string extra_data);

            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern int TA_UseTrialVerifiedRequest(uint handle, string filename, string extra_data);

            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern int TA_UseTrialVerifiedFromFile(uint handle, string filename, TA_Flags flags);

            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern int TA_ExtendTrial(uint handle, TA_Flags flags, string trialExtension);

            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern int TA_PDetsFromPath(string filename);

            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern int TA_PDetsFromByteArray(byte[] pArray, int nSize);

            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern int TA_SetCustomActDataPath(uint handle, string directory);

            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern int TA_SetTrialCallback(uint handle, TrialCallbackType callback);

            [DllImport("TurboActivate64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern int TA_GetVersion(out uint MajorVersion, out uint MinorVersion, out uint BuildVersion,
                out uint RevisionVersion);
        }
#endif
    }

    public class COMException : TurboActivateException
    {
        public COMException()
            : base(
                "CoInitializeEx failed. Re-enable Windows Management Instrumentation (WMI) service. Contact your system admin for more information.")
        {
        }
    }

    public class AccountCanceledException : TurboActivateException
    {
        public AccountCanceledException()
            : base("Can't activate because the LimeLM account is cancelled.")
        {
        }
    }

    public class PkeyRevokedException : TurboActivateException
    {
        public PkeyRevokedException()
            : base("The product key has been revoked.")
        {
        }
    }

    public class PkeyMaxUsedException : TurboActivateException
    {
        public PkeyMaxUsedException()
            : base("The product key has already been activated with the maximum number of computers.")
        {
        }
    }

    public class InternetException : TurboActivateException
    {
        // More information here: https://wyday.com/limelm/help/faq/#internet-error
        public InternetException()
            : base("Connection to the servers failed.")
        {
        }

        public InternetException(string message) : base(message)
        {
        }
    }

    public class InternetTimeoutException : InternetException
    {
        public InternetTimeoutException()
            : base(
                "The connection to the server timed out because a long period of time elapsed since the last data was sent or received.")
        {
        }
    }

    public class InternetTLSException : InternetException
    {
        // More information here: https://wyday.com/limelm/help/faq/#internet-error
        public InternetTLSException()
            : base(
                "The secure connection to the activation servers failed due to a TLS or certificate error. More information here: https://wyday.com/limelm/help/faq/#internet-error")
        {
        }
    }

    public class InvalidProductKeyException : TurboActivateException
    {
        public InvalidProductKeyException()
            : base("The product key is invalid or there's no product key.")
        {
        }
    }

    public class NotActivatedException : TurboActivateException
    {
        public NotActivatedException()
            : base("The product needs to be activated.")
        {
        }
    }

    public class ProductDetailsException : TurboActivateException
    {
        public ProductDetailsException()
            : base("The product details file \"TurboActivate.dat\" failed to load. It's either missing or corrupt.")
        {
        }
    }

    public class InvalidHandleException : TurboActivateException
    {
        public InvalidHandleException()
            : base("The handle is not valid. You must set a valid VersionGUID when constructing TurboActivate object.")
        {
        }
    }

    public class TrialDateCorruptedException : TurboActivateException
    {
        public TrialDateCorruptedException()
            : base("The trial data has been corrupted, using the oldest date possible.")
        {
        }
    }

    public class TrialExtUsedException : TurboActivateException
    {
        public TrialExtUsedException()
            : base("The trial extension has already been used.")
        {
        }
    }

    public class TrialExtExpiredException : TurboActivateException
    {
        public TrialExtExpiredException()
            : base("The trial extension has expired.")
        {
        }
    }

    public class DateTimeException : TurboActivateException
    {
        public DateTimeException()
            : base(
                "The activation has expired or the system time has been tampered with. Ensure your time, timezone, and date settings are correct. After fixing them restart your computer.")
        {
        }
    }

    public class PermissionException : TurboActivateException
    {
        public PermissionException()
            : base(
                "Insufficient system permission. Either start your process as an admin / elevated user or call the function again with the TA_USER flag.")
        {
        }
    }

    public class InvalidFlagsException : TurboActivateException
    {
        public InvalidFlagsException()
            : base(
                "The flags you passed to the function were invalid (or missing). Flags like \"TA_SYSTEM\" and \"TA_USER\" are mutually exclusive -- you can only use one or the other.")
        {
        }
    }

    public class VirtualMachineException : TurboActivateException
    {
        public VirtualMachineException()
            : base(
                "The function failed because this instance of your program is running inside a virtual machine / hypervisor and you've prevented the function from running inside a VM.")
        {
        }
    }

    public class ExtraDataTooLongException : TurboActivateException
    {
        public ExtraDataTooLongException()
            : base(
                "The \"extra data\" was too long. You're limited to 255 UTF-8 characters. Or, on Windows, a Unicode string that will convert into 255 UTF-8 characters or less.")
        {
        }
    }

    public class InvalidArgsException : TurboActivateException
    {
        public InvalidArgsException()
            : base("The arguments passed to the function are invalid. Double check your logic.")
        {
        }
    }

    public class TurboFloatKeyException : TurboActivateException
    {
        public TurboFloatKeyException()
            : base("The product key used is for TurboFloat Server, not TurboActivate.")
        {
        }
    }

    public class NoMoreDeactivationsException : TurboActivateException
    {
        public NoMoreDeactivationsException()
            : base(
                "No more deactivations are allowed for the product key. This product is still activated on this computer.")
        {
        }
    }

    public class EnableNetworkAdaptersException : TurboActivateException
    {
        // More information here: https://wyday.com/limelm/help/faq/#disabled-adapters
        public EnableNetworkAdaptersException()
            : base(
                "There are network adapters on the system that are disabled and TurboActivate couldn't read their hardware properties (even after trying and failing to enable the adapters automatically). Enable the network adapters, re-run the function, and TurboActivate will be able to \"remember\" the adapters even if the adapters are disabled in the future.")
        {
        }
    }

    public class AlreadyActivatedException : TurboActivateException
    {
        public AlreadyActivatedException()
            : base(
                "You can't use a product key because your app is already activated with a product key. To use a new product key, then first deactivate using either the Deactivate() or DeactivationRequestToFile().")
        {
        }
    }

    public class AlreadyVerifiedTrialException : TurboActivateException
    {
        public AlreadyVerifiedTrialException()
            : base(
                "The trial is already a verified trial. You need to use the \"TA_VERIFIED_TRIAL\" flag. Can't \"downgrade\" a verified trial to an unverified trial.")
        {
        }
    }

    public class TrialExpiredException : TurboActivateException
    {
        public TrialExpiredException()
            : base("The verified trial has expired. You must request a trial extension from the company.")
        {
        }
    }

    public class NoMoreTrialsAllowedException : TurboActivateException
    {
        public NoMoreTrialsAllowedException()
            : base(
                "In the LimeLM account either the trial days is set to 0, OR the account is set to not auto-upgrade and thus no more verified trials can be made.")
        {
        }
    }

    public class MustSpecifyTrialTypeException : TurboActivateException
    {
        public MustSpecifyTrialTypeException()
            : base(
                "You must specify the trial type (TA_UNVERIFIED_TRIAL or TA_VERIFIED_TRIAL). And you can't use both flags. Choose one or the other. We recommend TA_VERIFIED_TRIAL.")
        {
        }
    }

    public class BrokenWMIException : TurboActivateException
    {
        public BrokenWMIException()
            : base(
                "The WMI repository on the computer is broken. To fix the WMI repository see the instructions here: https://wyday.com/limelm/help/faq/#fix-broken-wmi")
        {
        }
    }

    public class MustUseTrialException : TurboActivateException
    {
        public MustUseTrialException()
            : base("You must call TA_UseTrial() before you can get the number of trial days remaining.")
        {
        }
    }

    public class TurboActivateException : Exception
    {
        public TurboActivateException(string message) : base(message)
        {
        }
    }

    public enum IsGenuineResult
    {
        /// <summary>Is activated and genuine.</summary>
        Genuine = 0,

        /// <summary>Is activated and genuine and the features changed.</summary>
        GenuineFeaturesChanged = 1,

        /// <summary>Not genuine (note: use this in tandem with NotGenuineInVM).</summary>
        NotGenuine = 2,

        /// <summary>Not genuine because you're in a Virtual Machine.</summary>
        NotGenuineInVM = 3,

        /// <summary>
        ///     Treat this error as a warning. That is, tell the user that the activation couldn't be validated with the
        ///     servers and that they can manually recheck with the servers immediately.
        /// </summary>
        InternetError = 4
    }
}