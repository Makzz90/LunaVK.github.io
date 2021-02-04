using LunaVK.Core;
using LunaVK.Core.DataObjects;
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

//CreateEditAlbumViewModel

namespace LunaVK.UC.PopUp
{
    public sealed partial class CreateAlbumUC : UserControl
    {
        private Action _notifyOnCompletion;

        public CreateAlbumUC(Action notifyOnCompletion)
        {
            this.InitializeComponent();
            this._notifyOnCompletion = notifyOnCompletion;
        }
        /*
        private CreateEditAlbumViewModel VM
        {
            get
            {
                return base.DataContext as CreateEditAlbumViewModel;
            }
        }
        */
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this._notifyOnCompletion();
        }

    }
}
