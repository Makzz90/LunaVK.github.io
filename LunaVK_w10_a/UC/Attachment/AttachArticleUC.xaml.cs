using LunaVK.Core.DataObjects;
using LunaVK.Core.Framework;
using LunaVK.Core.Library;
using LunaVK.Framework;
using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace LunaVK.UC.Attachment
{
    public sealed partial class AttachArticleUC : UserControl
    {
        private PopUpService _flyout;

        public AttachArticleUC()
        {
            this.InitializeComponent();
        }

        private VKLink VM
        {
            get { return base.DataContext as VKLink; }
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            FrameworkElement fr = sender as FrameworkElement;
            fr.Clip = new RectangleGeometry() { Rect = new Rect(0, 0, e.NewSize.Width, e.NewSize.Height) };
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (this.VM == null)
                return;//todo: ARTICLE!

            this.VM.button.action.url = this.VM.button.action.url.Replace("m.vk.com/", "vk.com/");


            this._flyout = new PopUpService();
            this._flyout.AnimationTypeChild = PopUpService.AnimationTypes.Swivel;
            this._flyout.BackgroundBrush = null;
            this._flyout.OverrideBackKey = true;
            WebView w = new WebView();
            w.NavigationCompleted += this.w_NavigationCompleted;

            w.Navigate(new Uri(this.VM.button.action.url));
            this._flyout.Child = w;
            this._flyout.Show();
        }

        private void w_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            sender.NavigationStarting += w_NavigationStarting;
        }

        private void w_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            if(!args.Uri.AbsoluteUri.StartsWith("https://m.vk.com/@") && !args.Uri.AbsoluteUri.StartsWith("https://vk.com/@"))
            {
                args.Cancel = true;
                this._flyout.Hide();
            }
        }

        private void AppBarButton_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            FavoritesService.Instance.AddArticle(this.VM.url, (result)=> {
                if(result.error.error_code == Core.Enums.VKErrors.None && result.response == 1)
                {
                    //Execute.ExecuteOnUIThread(() => { });
                }
            });
        }
    }
}
