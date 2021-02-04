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
using LunaVK.Core.Enums;
using LunaVK.Framework;
using LunaVK.Core;

namespace LunaVK.UC
{
    public sealed partial class MenuItemUC : UserControl
    {
        public MenuItemUC()
        {
            this.InitializeComponent();
            if (Settings.MenuDivider == false)
                this.root.Margin = new Thickness();
            
            if (CustomFrame.Instance!=null)
                CustomFrame.Instance.MenuOpenChanged += this.CFrame_MenuOpenChanged;

#if WINDOWS_UWP
            this.root.Background = new SolidColorBrush(Windows.UI.Colors.Transparent);//Чтобы видеть прозрачность за элементом
#endif
        }

        private void CFrame_MenuOpenChanged(object sender, bool e)
        {
            VisualStateManager.GoToState(this, e ? "Opened" :"Closed", true);
        }

        void MenuStateChanged(object sender, CustomFrame.MenuStates e)
        {
            if (e == CustomFrame.MenuStates.StateMenuFixedContentFixed)
            {
                this.brd.Visibility = Visibility.Visible;
//                this.MiniCounter.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            else if (e == CustomFrame.MenuStates.StateMenuNarrowContentStretch)
            {
                this.brd.Visibility = Visibility.Collapsed;
 //               this.MiniCounter.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            else
            {
                this.brd.Visibility = Visibility.Visible;
 //               this.MiniCounter.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
        }

        public string Icon
        {
            get {return this.fIcon.Glyph;}
            set { this.fIcon.Glyph = value; }
        }

        public string Title
        {
            get { return this.tTitle.Text; }
            set { this.tTitle.Text = value; }
        }
        
        private void rect_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (args.NewValue == null)
                return;
            bool value = (bool)args.NewValue;
            VisualStateManager.GoToState(this, value ? "SelectedState" : "NormalState", true);
        }
    }
}
