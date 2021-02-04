using LunaVK.Framework;
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
using Windows.UI.Xaml.Navigation;

namespace LunaVK.Pages.Group
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class PostsSearchPage : PageBase
    {
        
        public PostsSearchPage()
        {
            this.InitializeComponent();
            
        }

        private void PostsSearchPage_Loaded(object sender, RoutedEventArgs e)
        {
            //if (CustomFrame.Instance.HeaderWithMenu.IsVisible == true)
            //    CustomFrame.Instance.HeaderWithMenu.IsVisible = false;
            if(!string.IsNullOrEmpty(this.VM._query))
            {
                this.mainScroll.NeedReload = true;
                this.mainScroll.Reload();
            }
        }

        private PostsSearchViewModel VM
        {
            get { return base.DataContext as PostsSearchViewModel; }
        }

        private void TextBoxSearch_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (e.Key != Windows.System.VirtualKey.Enter || tb.Text == string.Empty)
                return;
            //((Control)this.scrollNews).Focus();
            //this.VM.Search(tb.Text);
            this.VM._query = tb.Text;
            this.VM.Items.Clear();
            this.mainScroll.NeedReload = true;
            this.mainScroll.Reload();
        }

        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            if (pageState != null && pageState.ContainsKey("Data"))
            {
                base.DataContext = pageState["Data"];
            }
            else
            {
                Dictionary<string, object> QueryString = navigationParameter as Dictionary<string, object>;
                int OwnerId = (int)QueryString["OwnerId"];
                string NameGen = (string)QueryString["NameGen"];
                string q = (string)QueryString["Query"];
                string domain = (string)QueryString["Domain"];
                
                this.DataContext = new PostsSearchViewModel(OwnerId, domain);
                if (!string.IsNullOrEmpty(q))
                {
                    this.VM._query = q;
                    this.textBoxSearch.Text = q;
                }

                string temp = "Поиск по записям";
                temp += (" " + NameGen);
                base.Title = temp;

                this.Loaded += PostsSearchPage_Loaded;
            }
        }

        protected override void SaveState(Dictionary<String, Object> pageState)
        {
            //CustomFrame.Instance.HeaderWithMenu.IsVisible = true;
        }
    }
}
