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
using Windows.UI;
using Windows.Foundation.Metadata;
using LunaVK.Core;

namespace LunaVK
{
    public sealed partial class SettingsPersonalizationPage : PageBase
    {
        //PopUpService dialogService;
        //UC.ColorPickerUC picker;
        //GridView gv;
        //StackPanel colorStack;

        public SettingsPersonalizationPage()
        {
            this.InitializeComponent();

            for (int i = 0; i < (double)Application.Current.Resources["TotalAccentColors"]; i++)
            {
                GridViewItem item1 = new GridViewItem();
                item1.Background = new SolidColorBrush((Color)Application.Current.Resources["AccentColor" + i]);
                this._gridView.Items.Add(item1);
            }

            if (/*!ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.XamlCompositionBrushBase") ||*/ !ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 4))
            {
                this._fluentPanel.Visibility = Visibility.Collapsed;
            }

            base.DataContext = new ViewModels.SettingsViewModel();

            base.Title = LocalizedStrings.GetString("NewSettings_Personalization/Title");
        }

        /*
        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (this._scaleInterface != null)
                this._scaleInterface.Text = "Масштаб интерфейса (" + e.NewValue + "%)";
        }

        private void Slider_ValueChanged2(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (this._roundAvatar != null)
                this._roundAvatar.Text = "Скругление аватарок (" + e.NewValue + "%)";
        }
        */
        /*
        private void Rectangle_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (dialogService == null)
                dialogService = new PopUpService();

            if (gv != null)
            {
                gv.SelectedItems.Clear();
            }

            if (colorStack==null)
            {
                colorStack = new StackPanel();
                colorStack.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center;
                colorStack.Background = (SolidColorBrush)Application.Current.Resources["ItemBackgroundBrush"];
                gv = new GridView();
                gv.Style = (Style)this.Resources["GridViewStyle"];
                gv.Margin = new Thickness(0, 10, 0, 10);
                gv.SelectionMode = ListViewSelectionMode.Multiple;
                gv.SizeChanged += gv_SizeChanged;
                //gv.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center;

                for(int i =0;i<8;i++)
                {
                    GridViewItem item1 = new GridViewItem();
                    item1.Background = new SolidColorBrush((Windows.UI.Color)Application.Current.Resources["AccentColor"+i]);
                    item1.IsSelected = i == Settings.AccentColor;
                    item1.Tag = i;
                    gv.Items.Add(item1);
                }

                
                colorStack.Children.Add(gv);
            }

            gv.SelectedIndex = Settings.AccentColor;
            gv.SelectionChanged += GV_SelectionChanged;

            dialogService.Child = colorStack;


            dialogService.Show();
        }
        
        void gv_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GridView gv = sender as GridView;
            var panel = (ItemsWrapGrid)gv.ItemsPanelRoot;
            double max = e.NewSize.Width;
            panel.ItemHeight = panel.ItemWidth = max / 4;
        }

        void GV_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GridView gv = sender as GridView;
            gv.SelectionChanged -= GV_SelectionChanged;

            if (gv.SelectedIndex == -1)
                return;

            foreach(var item in  gv.SelectedItems)
            {
                GridViewItem gi = item as GridViewItem;
                if ((int)gi.Tag == Settings.AccentColor)
                    continue;

                Settings.AccentColor = (byte)((int)gi.Tag);
                this._rectAccent.Fill = new SolidColorBrush((Windows.UI.Color)Application.Current.Resources["AccentColor" + Settings.AccentColor]);
                break;
            }

            dialogService.Hide();
            
        }
        

        /// <summary>
        /// Вернуть цвет
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Border_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Settings.AccentColor = 0;//new Library.RGB(72, 119, 203);
            this._rectAccent.Fill = new SolidColorBrush(Windows.UI.Color.FromArgb(byte.MaxValue, 72, 119, 203));
        }
        */
    }
}
