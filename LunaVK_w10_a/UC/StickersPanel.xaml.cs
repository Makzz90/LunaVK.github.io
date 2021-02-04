using LunaVK.Common;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Library;
using LunaVK.Core.Utils;
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

// Документацию по шаблону элемента "Пользовательский элемент управления" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234236

namespace LunaVK.UC
{
    public sealed partial class StickersPanel : UserControl
    {
        /// <summary>
        /// Происходит перед выбором стикера и перед походом в магазин
        /// </summary>
        public event EventHandler Closed;

//        List<PreviewBehavior> previewBehaviors = new List<PreviewBehavior>();

        public StickersPanel()
        {
            base.DataContext = this;
            this.Items = new ObservableCollection<object>();
            this.InitializeComponent();
            this.Loaded += StickersPanel_Loaded;
            this.Unloaded += StickersPanel_Unloaded;
        }

        private void StickersPanel_Unloaded(object sender, RoutedEventArgs e)
        {
//            this.previewBehaviors.Clear();
//            this.previewBehaviors = null;
        }

        private void StickersPanel_Loaded(object sender, RoutedEventArgs e)
        {
            this.InitPanel();
        }

        public ObservableCollection<object> Items { get; set; }

        //private bool _panelInitialized;

        /// <summary>
        /// Получаем стикеры и добавляем их в коллекцию
        /// </summary>
        private void InitPanel()
        {
            //bug: если быстро нажимать, загрузится список дважды
//            if (this._panelInitialized)
//                return;
            
            List<StoreProductFilter> l = new List<StoreProductFilter>() { StoreProductFilter.Active };
            //List<StockItem> temp = StoreService.Instance.Stickers;

            StoreService.Instance.GetStockItems(l,(result)=> {
                if(result!=null && result.error.error_code== VKErrors.None)
                {
                    foreach (var pack in result.response.items)
                    {
                        this.Items.Add(pack);
                    }
                }
                this.progBar.IsIndeterminate = false;

                //BugFix: чтобы у нижней панели был выделен первый пак
                this.itemsControl.SelectedIndex = -1;
                this.itemsControl.SelectedIndex = 0;
            });

            

            
            //            this._panelInitialized = true;
            

            
        }

        public event EventHandler<VKSticker> StickerTapped;

        private void Image_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.Closed?.Invoke(this, null);

            var vm = (sender as FrameworkElement).DataContext as VKSticker;
            //if (vm.is_allowed)
            //{
                this.StickerTapped?.Invoke(this, vm);
            //}
        }

        private void GridView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GridView gv = sender as GridView;
            var panel = (ItemsWrapGrid)gv.ItemsPanelRoot;
            panel.Orientation = Orientation.Horizontal;

            double colums = e.NewSize.Width / 90.0;

            panel.MaximumRowsOrColumns = (int)colums;

            //System.Diagnostics.Debug.WriteLine(colums + " " );

            panel.ItemHeight = panel.ItemWidth = e.NewSize.Width / (int)colums;
        }
        
        private void Store_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.Closed?.Invoke(this, null);
            (Window.Current.Content as Frame).Navigate(typeof(StickersStorePage));
        }

        private void Image_Loaded(object sender, RoutedEventArgs e)
        {
            Image img = sender as Image;
            img.Animate(0, 1, "Opacity", 600);
            img.Loaded -= Image_Loaded;
        }
        /*
        private void Border_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            ScaleTransform tr = element.RenderTransform as ScaleTransform;
            if (tr == null)
            {
                element.RenderTransform = new ScaleTransform();
                tr = element.RenderTransform as ScaleTransform;
            }

            List<AnimationUtils.AnimationInfo> animInfoList = new List<AnimationUtils.AnimationInfo>();
            animInfoList.Add(new AnimationUtils.AnimationInfo()
            {
                target = tr,
                propertyPath = "ScaleX",
                from = tr.ScaleX,
                to = 1.3,
                duration = 200
            });
            animInfoList.Add(new AnimationUtils.AnimationInfo()
            {
                target = tr,
                propertyPath = "ScaleY",
                from = tr.ScaleY,
                to = 1.3,
                duration = 200
            });

            AnimationUtils.AnimateSeveral(animInfoList);
        }

        private void Border_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            ScaleTransform tr = element.RenderTransform as ScaleTransform;
            if (tr == null)
            {
                element.RenderTransform = new ScaleTransform();
                tr = element.RenderTransform as ScaleTransform;
            }

            List<AnimationUtils.AnimationInfo> animInfoList = new List<AnimationUtils.AnimationInfo>();
            animInfoList.Add(new AnimationUtils.AnimationInfo()
            {
                target = tr,
                propertyPath = "ScaleX",
                from = tr.ScaleX,
                to = 1.0,
                duration = 200
            });
            animInfoList.Add(new AnimationUtils.AnimationInfo()
            {
                target = tr,
                propertyPath = "ScaleY",
                from = tr.ScaleY,
                to = 1.0,
                duration = 200
            });

            AnimationUtils.AnimateSeveral(animInfoList);
        }
        */
        


        private void Opened(bool status)
        {
            System.Diagnostics.Debug.WriteLine("Opened " + status.ToString());
            this.flip.IsEnabled = !status;
        }
    }
}
