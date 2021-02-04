using LunaVK.Core.Framework;
using LunaVK.Core.Utils;
using LunaVK.Framework;
using LunaVK.UC;
using LunaVK.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace LunaVK.Pages
{
    public sealed partial class RegistrationPage : PageBase
    {
        private readonly DelayedExecutor _de;

        public RegistrationPage()
        {
            this.InitializeComponent();
            this.Loaded += RegistrationPage_Loaded;

            this._de = new DelayedExecutor(300);
        }

        private void RegistrationPage_Loaded(object sender, RoutedEventArgs e)
        {
            CustomFrame.Instance.SuppressMenu = true;
        }

        public RegistrationViewModel RegistrationVM
        {
            get { return base.DataContext as RegistrationViewModel; }
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            base.DataContext = new RegistrationViewModel();
            base.Title = this.RegistrationVM.Title;
            this.RegistrationVM.OnMovedForward = this.HandleMoveBackOrForward;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            CustomFrame.Instance.SuppressMenu = false;
        }

        private void HandleMoveBackOrForward()
        {
            int num1 = this.RegistrationVM.CurrentStep - 1;
            bool flag = num1 <= 3;
//            if (!flag)
//                ((Page)this).NavigationService.ClearBackStack();
            this.rectProgress.Width = (flag ? 120.0 : 240.0);
            double num2 = flag ? (120 * num1) : (240 * (num1 - 4));
            TranslateTransform renderTransform = this.rectProgress.RenderTransform as TranslateTransform;
            TranslateTransform translateTransform = renderTransform;
            double x = renderTransform.X;
            double to = num2;
            int duration = 250;
            
            CubicEase cubicEase = new CubicEase();
            int num3 = 2;
            cubicEase.EasingMode = ((EasingMode)num3);

            translateTransform.Animate(x, to, "X", duration, 0, cubicEase);
            switch (num1)
            {
                case 0:
                    this._de.AddToDelayedExecution((() => Execute.ExecuteOnUIThread((() =>
                    {
                        if (string.IsNullOrEmpty(this.textBoxFirstName.Text))
                        {
                            this.textBoxFirstName.Focus( FocusState.Keyboard);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(this.textBoxLastName.Text))
                                return;
                            this.textBoxLastName.Focus(FocusState.Keyboard);
                        }
                    }))));
                    break;
                case 1:
                    this._de.AddToDelayedExecution(() => Execute.ExecuteOnUIThread((() =>
                    {
                        if (!string.IsNullOrEmpty(this.textBoxPhoneNumber.Text))
                            return;
                        this.textBoxPhoneNumber.Focus(FocusState.Keyboard);
                    })));
                    break;
                case 2:
                    this._de.AddToDelayedExecution((() => Execute.ExecuteOnUIThread((() =>
                    {
                        if (!string.IsNullOrEmpty(this.textBoxConfirmationCode.Text))
                            return;
                        (this.textBoxConfirmationCode).Focus(FocusState.Keyboard);
                    }))));
                    break;
                case 3:
                    this._de.AddToDelayedExecution((() => Execute.ExecuteOnUIThread((() =>
                    {
                        if (!string.IsNullOrEmpty(this.passwordBox.Password))
                            return;
                        (this.passwordBox).Focus(FocusState.Keyboard);
                    }))));
                    break;
            }
        }

        private void _appBarButtonCheck_Click(object sender, TappedRoutedEventArgs e)
        {
            switch (this.RegistrationVM.CurrentStep)
            {
                case 1:
                    if (this.textBoxFirstName.Text.Length < 2 || this.textBoxLastName.Text.Length < 2)
                    {
                        new GenericInfoUC().ShowAndHideLater("Registration_WrongName");
                        return;
                    }
                    break;
                case 4:
                    if (this.passwordBox.Password.Length < 6)
                    {
                        new GenericInfoUC().ShowAndHideLater("Registration_ShortPassword");
                        return;
                    }
                    break;
            }
            this.RegistrationVM.CompleteCurrentStep();
        }
    }
}
