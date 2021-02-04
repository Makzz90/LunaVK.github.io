using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace LunaVK.UC
{
    public sealed partial class DragItemUC : UserControl
    {
        public Action<FrameworkElement> Close;

        public DragItemUC()
        {
            this.InitializeComponent();

            this.Unloaded += this.DragItemUC_Unloaded;
        }

        private void DragItemUC_Unloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 1);
        }

        public void SetContent(UIElement content)
        {
            //this._content.Content = content;
            this._rootGrid.Children.Insert(0, content);
            content.PointerCanceled += this.Item_PointerExited;
            content.PointerExited += this.Item_PointerExited;
            content.PointerEntered += this.Item_PointerEntered;
        }

        public void RemoveContent()
        {
            UIElement content = this._rootGrid.Children[0];
            
            content.PointerCanceled -= this.Item_PointerExited;
            content.PointerExited -= this.Item_PointerExited;
            content.PointerEntered -= this.Item_PointerEntered;

            this._rootGrid.Children.Remove(content);
        }

        private void Close_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.Close?.Invoke(this);
        }

        private void Item_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 1);
        }

        private void Item_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.SizeAll, 1);
        }
    }
}
