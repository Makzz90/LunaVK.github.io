using LunaVK.Library;
using LunaVK.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

namespace LunaVK.Pages
{
    public sealed partial class ProductPage : PageBase
    {
        public ProductPage()
        {
            this.InitializeComponent();
        }

        private ProductViewModel VM
        {
            get { return base.DataContext as ProductViewModel; }
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            IDictionary<string, object> QueryString = e.Parameter as IDictionary<string, object>;
            int ownerId = (int)QueryString["OwnerId"];
            uint productId = (uint)QueryString["ProductId"];

            base.DataContext = new ProductViewModel(ownerId, productId);

            base.Title = "Товар";
        }

        private void SlideView_OnSelectionChanged(object sender, int e)
        {
            if ((sender as FrameworkElement).DataContext == null)
                return;
            if (this.listBoxNavDots.Items.Count == 0)
                return;
            //if (this._stockItemHeader == null)
            //   return;
            //List<string> demoPhotos = this._stockItemHeader.DemoPhotos;
            //if ((demoPhotos != null ? (demoPhotos.Count == 0 ? true : false) : false) != false)
            //    return;
            //if (this.listBoxNavDots.ItemsSource == null)
            //    this.listBoxNavDots.ItemsSource = this._stockItemHeader.DemoPhotos;
            this.listBoxNavDots.SelectedIndex = e;
        }

        private void ContactSellerButton_OnClick(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            this.VM.ContactSeller();
        }

        private void MetaData_OnTap(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            this.VM.NavigateToGroup();
        }
    }
}
