using LunaVK.Core;
using LunaVK.Core.Framework;
using LunaVK.Core.Network;
using LunaVK.Core.Utils;
using LunaVK.Framework;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace LunaVK.UC
{
    public sealed partial class GenericInfoUC : UserControl
    {
        private readonly DelayedExecutor _deHide;
        private const int DEFAULT_DELAY = 1000;

        public GenericInfoUC(int delayToHide = GenericInfoUC.DEFAULT_DELAY)
        {
            this.InitializeComponent();
            this._deHide = new DelayedExecutor(delayToHide);
        }

        public void ShowAndHideLater(string text, Grid elementToFadeout = null)
        {
            PopUpService ds = new PopUpService();
            this.textBlockInfo.Text = text;
            ds.BackgroundBrush = null;
            ds.Child = this;
            //ds.KeepAppBar = true;
            if (elementToFadeout != null)
                ds.OverlayGrid = elementToFadeout;
            ds.Show();

            this._deHide.AddToDelayedExecution(() => Execute.ExecuteOnUIThread(() => ds.Hide()));

        }

        public static void ShowBasedOnResult(string successString = "", VKError error = null)
        {
            if (error != null)
            {
                if (error.error_code == Core.Enums.VKErrors.None)
                {
                    if (string.IsNullOrWhiteSpace(successString))
                        return;
                    new GenericInfoUC().ShowAndHideLater(successString, null);
                }

                else
                {
                    int delayToHide = 3000;
                    //string text = "Error";
                    /*
                    switch (error.error_code)
                    {
                        case ResultCode.Processing:
                            text = "Registration_TryAgainLater";
                            break;
                        case ResultCode.ProductNotFound:
                            text = "CannotLoadProduct";
                            delayToHide = 2000;
                            break;
                        case ResultCode.VideoNotFound:
                            text = "CannotLoadVideo";
                            delayToHide = 2000;
                            break;
                        case ResultCode.WrongPhoneNumberFormat:
                            text = "Registration_InvalidPhoneNumber";
                            break;
                        case ResultCode.PhoneAlreadyRegistered:
                            text = "Registration_PhoneNumberIsBusy";
                            break;
                        case ResultCode.InvalidCode:
                            text = "Registration_WrongCode;
                            break;
                        case ResultCode.InvalidAudioFormat:
                            text = "InvalidAudioFormatError";
                            delayToHide = 3000;
                            break;
                        case ResultCode.AudioIsExcludedByRightholder:
                            text = "AudioIsExcludedByRightholderError";
                            delayToHide = 3000;
                            break;
                        case ResultCode.MaximumLimitReached:
                            text = "AudioFileSizeLimitReachedError";
                            delayToHide = 3000;
                            break;
                        case ResultCode.UploadingFailed:
                            text = "FailedToConnectError";
                            delayToHide = 3000;
                            break;
                        case ResultCode.CommunicationFailed:
                            text = "Error_Connection";
                            delayToHide = 3000;
                            break;
                    }
                    */

                    //string str = error1 != null ? error1.error_text : null;
                    //if (!string.IsNullOrWhiteSpace(str))
                    //    text = str;
                    if (error.error_code == Core.Enums.VKErrors.NoNetwork)
                        error.error_msg = LocalizedStrings.GetString("FailedToConnectError").Replace("\\r\\n", Environment.NewLine);

                    new GenericInfoUC(delayToHide).ShowAndHideLater(error.error_msg, null);
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(successString))
                    return;
                new GenericInfoUC().ShowAndHideLater(successString, null);
            }
        }
    }
}
