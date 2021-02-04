using LunaVK.UC;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using LunaVK.Common;
using LunaVK.Framework;
using LunaVK.Core.Utils;
using LunaVK.ViewModels;

namespace LunaVK.Pages.Debug
{
    public sealed partial class TestLikesControl : Page
    {
        ObservableCollection<string> col = new ObservableCollection<string>();
        List<string> list = new List<string>();


        public TestLikesControl()
        {
            this.InitializeComponent();
            //this._likes.ItemsSource = this.col;
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //this.col.Add(col.Count.ToString());
            for(int i =0;i<3;i++)
            {
                this.list.Add(i.ToString());
            }
            this._likes.ItemsSource = this.list;
        }


        private void Rectangle_Tapped(object sender, TappedRoutedEventArgs e)
        {
            /*
            VideoCommentsViewModel vm = new VideoCommentsViewModel(-54162110, 456239341,"");

            PopUpService pop = new PopUpService();
            pop.Child = new VideoViewerUC() { DataContext = vm };
            pop.OverrideBackKey = true;
            pop.AnimationTypeChild = PopUpService.AnimationTypes.Fade;
            pop.BackgroundBrush = null;
            pop.Show();

            var temp = CustomFrame.Instance.VideoViewerElement;
            temp.DataContext = vm;
            CompositeTransform renderTransform = temp.RenderTransform as CompositeTransform;

            if(renderTransform==null)
            {
                ImageAnimator imageAnimator = new ImageAnimator(200, null);
                imageAnimator.AnimateIn((sender as FrameworkElement), temp);
            }
            else
            {
                renderTransform.Animate(renderTransform.TranslateX, 0, "TranslateX", 600, 0, null);
                renderTransform.Animate(renderTransform.TranslateY, 0, "TranslateY", 600, 0, null);
                renderTransform.Animate(renderTransform.ScaleX, 1, "ScaleX", 600, 0, null, null);
                renderTransform.Animate(renderTransform.ScaleY, 1, "ScaleY", 600, 0, null, null);
            }
            */
        }
    }
}
