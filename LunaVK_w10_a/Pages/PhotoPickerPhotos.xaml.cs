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

using LunaVK.Framework;
using LunaVK.Common;
using Windows.Storage;

namespace LunaVK.Pages
{
    public sealed partial class PhotoPickerPhotos : PageBase
    {
        StorageFile file;

        public PhotoPickerPhotos()
        {
            this.InitializeComponent();
            this._imgEditor.ImageEditCompleted = this.ImageEditCompletedHandle;
            this.Loaded += PhotoPickerPhotos_Loaded;
        }

        private void PhotoPickerPhotos_Loaded(object sender, RoutedEventArgs e)
        {
            CustomFrame.Instance.Header.IsVisible = false;

            if(file!=null)
                this._imgEditor.LoadStorageFile(file);
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            Dictionary<string, object> QueryString = e.Parameter as Dictionary<string, object>;
            if(QueryString.ContainsKey("PickToStorageFile"))
            {
                file = QueryString["PickToStorageFile"] as StorageFile;
            }
        }

        protected override void HandleOnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            CustomFrame.Instance.Header.IsVisible = true;
        }

        public void ImageEditCompletedHandle(string path)
        {
            ParametersRepository.SetParameterForId("ChoosenPhotos", path);
            CustomFrame.Instance.GoBack();
        }
    }
}
