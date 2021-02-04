using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

using LunaVK.Core.DataObjects;
using LunaVK.Library;
using LunaVK.Framework;

namespace LunaVK.UC.Attachment
{
    //NewsLinkMediumUC
    //MessagesLinkMediumUC
    //MessagesLinkUC
    //NewsLinkUC
    public sealed partial class AttachLinkUC : UserControl
    {
        private PopUpService _flyout;
        public static readonly DependencyProperty DataProperty = DependencyProperty.Register("Data", typeof(object), typeof(AttachLinkUC), new PropertyMetadata(default(object), OnDataChanged));

        /// <summary>
        /// Данные.
        /// </summary>
        public object Data
        {
            get { return (object)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        private static void OnDataChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ((AttachLinkUC)obj).DataContext = e.NewValue;
        }
        
        public AttachLinkUC()
        {
            this.InitializeComponent();
            this.Loaded += this.AttachLinkUC_Loaded;
        }

        void AttachLinkUC_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.VM == null)
                return;

            if (this.VM.button == null)
                this.btnAction.Visibility = Visibility.Collapsed;//BugFix: VM не срабатывает :(
        }

        private VKLink VM
        {
            get { return this.DataContext as VKLink; }
        }

        //public AttachLinkUC(VKLink a):this()
        //{
        //    this.DataContext = a;
        //}

        private void Border_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //todo: навигация в группу
            if (this.VM.button==null)
            {
                NavigatorImpl.Instance.NavigateToWebUri(this.VM.url);
            }
            else
            {
                if(this.VM.IsAMP)
                {
                    this.VM.button.action.url = this.VM.button.action.url.Replace("m.vk.com/", "vk.com/");

                    this._flyout = new PopUpService();
                    this._flyout.AnimationTypeChild = PopUpService.AnimationTypes.Swivel;
                    this._flyout.BackgroundBrush = null;
                    this._flyout.OverrideBackKey = true;
                    WebView w = new WebView();
                    w.NavigationCompleted += w_NavigationCompleted;
                    
                    w.Navigate(new Uri(this.VM.button.action.url));
                    this._flyout.Child = w;
                    this._flyout.Show();
                }
                else
                {
                    NavigatorImpl.Instance.NavigateToWebUri(this.VM.button.action.url);
                }
            }

        }

        void w_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            sender.NavigationStarting += w_NavigationStarting;
        }

        void w_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            if(args.Uri.AbsoluteUri!=this.VM.button.action.url)
            {
                args.Cancel = true;
                this._flyout.Hide();
            }
            int i = 0;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //string url = this._link.button.url ?? this._link.url;
     //       NavigatorImpl.Instance.NavigateToWebUri(this.VM.button.action.url);
        }
    }
}
/*
"type": "link",
"link": {
"url": "http://itun.es/i6xC69N",
"title": "Подкаст Radio Record от www.radiorecord.ru в Apple Podcasts",
"caption": "itunes.apple.com",
"description": "",
"photo": {
"id": 456245525,
"album_id": -2,
"owner_id": 429550,
"photo_75": "https://sun9-3.us...f9f/JYke8wJa1Yg.jpg",
"photo_130": "https://sun9-3.us...fa0/Dkd9SwTlx7E.jpg",
"photo_604": "https://sun9-3.us...fa1/J5W-76c9NeE.jpg",
"photo_807": "https://sun9-3.us...fa2/hWpWZH5wNCs.jpg",
"photo_1280": "https://sun9-3.us...fa3/6pVuNit__sc.jpg",
"width": 1067,
"height": 600,
"text": "",
"date": 1510341950
}
*/