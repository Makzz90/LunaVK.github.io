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

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LunaVK.Core.DataObjects;

namespace LunaVK.UC
{
    public sealed partial class SwipeThroughControl : UserControl, INotifyPropertyChanged
    {
        public event EventHandler<VKSticker> StickerTapped;
        public event PropertyChangedEventHandler PropertyChanged;

        public SwipeThroughControl()
        {
            this.InitializeComponent();
            base.DataContext = this;
            this._items_control.SizeChanged+=_items_control_SizeChanged;
        }

        

        /// <summary>
        /// LunaVK.Core.DataObjects.StockItem
        /// </summary>
        private ObservableCollection<object> _items;
        public ObservableCollection<object> Items
        {
            get
            {
                return this._items;
            }
            set
            {
                this._items = value;
                this.OnPropertyChanged("Items");
            }
        }
        
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (this.PropertyChanged == null)
                return;
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        
        private void GridView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GridView gv = sender as GridView;
            var panel = (ItemsWrapGrid) gv.ItemsPanelRoot;
            panel.Orientation = Orientation.Horizontal;

            double colums = e.NewSize.Width / 100.0;

            panel.MaximumRowsOrColumns = (int)colums;
            
            //System.Diagnostics.Debug.WriteLine(colums + " " );

            panel.ItemHeight = panel.ItemWidth = e.NewSize.Width / (int)colums;
        }

        private void Image_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var vm = (sender as FrameworkElement).DataContext as VKSticker;
            if (vm.is_allowed)
            {
                this.StickerTapped?.Invoke(this, vm);
            }
        }

        private void ScrollViewer_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            ScrollViewer sv = sender as ScrollViewer;

            double offs = e.NextView.VerticalOffset - sv.VerticalOffset;
            //System.Diagnostics.Debug.WriteLine(offs);
            if(offs>0)
            {
                tr.Y += offs;
                if (tr.Y > 64)
                    tr.Y = 64;
            }
            else
            {
                tr.Y += offs;
                if (tr.Y < 0)
                    tr.Y = 0;
            }
        }

        private void FlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            tr.Y = 0;

            FlipView fv = sender as FlipView;

            if (fv.SelectedIndex < 0 || this._items_control.Items.Count == 0)
                return;
            
            for(int i =0;i<this._items.Count;i++)
            {
                StockItem temp = this._items_control.Items[i] as StockItem;
                temp.IsSelected = i == fv.SelectedIndex;
            }
        }

        
        private void Tab_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            StockItem s = element.DataContext as StockItem;
            flip.SelectedItem = s;
        }

        /// <summary>
        /// BugFix
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _items_control_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ItemsControl ic = sender as ItemsControl;
            if(ic.Items.Count>0)
            {
                StockItem temp = ic.Items[0] as StockItem;
                temp.IsSelected = true;
                this._items_control.SizeChanged -= _items_control_SizeChanged;
            }
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            (Window.Current.Content as Frame).Navigate(typeof(StickersStorePage));
        }        
    }
}
