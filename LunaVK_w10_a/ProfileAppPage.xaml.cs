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
using Windows.UI.Xaml.Navigation;

using LunaVK.Core.Network;
using LunaVK.Core.Library;

namespace LunaVK
{
    public sealed partial class ProfileAppPage : PageBase
    {
        private string AppName;

        public ProfileAppPage()
        {
            this.InitializeComponent();
            this.Loaded += (s, e) =>
            {
                base.Title = AppName;
            };

        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            Dictionary<string, object> QueryString = e.Parameter as Dictionary<string, object>;
            uint appId =(uint) QueryString["AppId"];
            int ownerId = 0;
            if (QueryString.ContainsKey("OwnerId"))
                ownerId = (int)QueryString["OwnerId"];
            string utmParams = "";
            if (QueryString.ContainsKey("UtmParams"))
            {
                utmParams = (string)QueryString["UtmParams"];

                var request = new Windows.Web.Http.HttpRequestMessage();
                request.RequestUri = new Uri(utmParams);
                request.Headers.Add("User-Agent", "Mozilla/5.0 (Android; Mobile; rv:13.0) Gecko/13.0 Firefox/13.0");//BugFix: нам нужен телефон, чтобы вк не отправлял меню
                //this.WebView.NavigateWithHttpRequestMessage(request);
                this.WebView.Navigate(new Uri(utmParams));

                this.WebView.NavigationStarting += WebView_NavigationStarting;
                this.WebView.NewWindowRequested += WebView_NewWindowRequested;
                this.WebView.LoadCompleted += WebView_LoadCompleted;
            }

            this.AppName = (string)QueryString["AppName"];



            AppsService.Instance.GetEmbeddedUrl(appId, ownerId, (result) =>
            {
                if(result.error.error_code == Core.Enums.VKErrors.None)
                {

                }
            });
        }
        
        private void WebView_LoadCompleted(object sender, NavigationEventArgs e)
        {
            int i = 0;
        }

        private void WebView_NewWindowRequested(WebView sender, WebViewNewWindowRequestedEventArgs args)
        {
            int i = 0;
        }

        private void WebView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            int i = 0;
        }

        protected override void HandleOnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (this.WebView.CanGoBack)
            {
                this.WebView.GoBack();
                e.Cancel = true;
            }
        }
        /*
        public async void GetEmbeddedUrl(long appId, long ownerId)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["app_id"] = appId.ToString();
            if (ownerId != 0)
                parameters["owner_id"] = ownerId.ToString();
            RequestsDispatcher.GetResponse<EmbeddedUrlResponse>("apps.getEmbeddedUrl", parameters);
        }*/
    }
}
