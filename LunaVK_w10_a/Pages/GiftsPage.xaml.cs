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

namespace LunaVK.Pages
{
    public sealed partial class GiftsPage : PageBase
    {
        public GiftsPage()
        {
            this.InitializeComponent();
        }

        private GiftsViewModel VM
        {
            get { return base.DataContext as GiftsViewModel; }
        }

        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            if (pageState != null && pageState.ContainsKey("Data"))
            {
                base.DataContext = pageState["Data"];
                //this.listBoxBanned.NeedReload = false;
            }
            else
            {
                Dictionary<string, object> QueryString = navigationParameter as Dictionary<string, object>;
                uint userId = (uint)QueryString["UserId"];
                string firstName = (string)QueryString["FirstName"];
                string firstNameGen = (string)QueryString["FirstNameGen"];

                base.DataContext = new GiftsViewModel(userId, firstName, firstNameGen);
            }

            //base.Title = this.VM.Title;
        }

        protected override void SaveState(Dictionary<String, Object> pageState)
        {
            pageState["Data"] = this.VM;
        }
    }
}
