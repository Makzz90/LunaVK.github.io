using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using LunaVK.Photo.ViewModels;

namespace LunaVK.Photo.UC
{
    public class TextOverlayShape : ContainedShapeControlContainer
    {
        private List<SlideMenuItemBase> _menuItems;
        public override List<SlideMenuItemBase> MenuItems
        {
            get { return this._menuItems; }
        }
        
        public TextBox TextBox
        {
            get { return base.Control as TextBox; }
        }

        public TextOverlayShape()
            : base(new TextBox(), 50,48)
        {
            this.TextBox.FontSize = this._fontSize;
            this.TextBox.BorderThickness = new Thickness();
            this.TextBox.AcceptsReturn = true;
            this.TextBox.TextWrapping = TextWrapping.NoWrap;
            this.TextBox.Padding = new Thickness();
            this.TextBox.IsSpellCheckEnabled = false;
            this.TextBox.Foreground = new SolidColorBrush(Windows.UI.Colors.White);
            //this.TextBox.IsHitTestVisible = false;//с этим не работает размер

            this.TextBox.TextChanged += TextBox_TextChanged;
            
            //this.TextBox.IsReadOnly = true;
            this.TextBox.GotFocus += TextBox_GotFocus;
            this.TextBox.LostFocus += TextBox_LostFocus;
            this.TextBox.Background = new SolidColorBrush(Windows.UI.Colors.Transparent);
            this.TextBox.Text = "Text";
            this.TextBox.PlaceholderText = "Enter text";

            //this.UpdateText();
            //this.UpdateFontSize();
            //this.UpdateFontFamily();
            //this.UpdateForegroundBrush();
//            this.UpdateIsInEditMode();
            this.ToggleEditMode(false);
            //this.UpdateFontWeight();

            this._menuItems = new List<SlideMenuItemBase>();
            this._menuItems.Add(new SlideMenuItemBase()
            {
                Name = "Edit text",
                IconPath = "ms-appx:///Assets/PhotoEditor/text-edit.png",
                ClickCommand = ()=> { this.ToggleEditMode(true); }
            });
            this._menuItems.Add(new SlideMenuItemBase()
            {
                Name = "Font",
                IconPath = "ms-appx:///Assets/PhotoEditor/text-fonts.png",
                SecondaryControlDataTemplate = (DataTemplate)Application.Current.Resources["FontSelectorTemplate"],
                GetDataContextFunc = (() => new FontSelectorSlideMenuItemViewModel(this.TextBox))
            });
            this._menuItems.Add(new SlideMenuItemBase()
            {
                Name = "Color",
                IconPath = "ms-appx:///Assets/PhotoEditor/text-foreground.png",
                SecondaryControlDataTemplate = (DataTemplate)Application.Current.Resources["SelectColorTemplate"],
                GetDataContextFunc = (() => new SelectColorControlViewModel(this.TextBox))
            });
        }

        private double _fontSize = 30;

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            textBox.Measure(new Size(500, 500));
            (this.ParentContainer as AdornerElementBaseUC).UpdateSize(textBox.DesiredSize.Width/* + textBox.Padding.Left + textBox.Padding.Right*/, textBox.DesiredSize.Height/* + textBox.Padding.Bottom + textBox.Padding.Top*/);
        }

        public override void ScaleChanged(double p)
        {
            this.TextBox.FontSize = this._fontSize * p;
        }

        private bool HasFocus { get; set; }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            this.HasFocus = false;
            this.IsInEditMode = false;
            //
            this.ToggleEditMode(false);
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (this.IsInEditMode)
                return;

            this.HasFocus = true;
            //this.TextBox.Focus(FocusState.Unfocused);
        }

        /*
        private bool _isInEditMode;
        public override bool IsInEditMode
        {
            get
            {
                return this._isInEditMode;
            }
            set
            {
                if (this._isInEditMode != value)
                    this._isInEditMode = value;
                this.UpdateIsInEditMode();
                //this.RaiseChangedEvent();
            }
        }
        
        private void UpdateIsInEditMode()
        {
            if (this.IsInEditMode)
            {
                this.TextBox.IsReadOnly=false;
                this.TextBox.IsHitTestVisible = true;
                if (this.HasFocus)
                    return;
                this.TextBox.Focus(Windows.UI.Xaml.FocusState.Keyboard);
                this.TextBox.SelectAll();
            }
            else
            {
                this.TextBox.IsHitTestVisible = false;
                this.TextBox.IsReadOnly = true;
            }
        }
        */
        private void ToggleEditMode(bool status)
        {
            if (status)
            {
                this.TextBox.IsReadOnly = false;
                this.TextBox.IsHitTestVisible = true;
                this.TextBox.Focus(FocusState.Keyboard);
                this.TextBox.Background = new SolidColorBrush(Windows.UI.Colors.Green);
                this.TextBox.SelectAll();
            }
            else
            {
                this.TextBox.Background = new SolidColorBrush(Windows.UI.Colors.Transparent);
                this.TextBox.IsHitTestVisible = false;
                this.TextBox.IsReadOnly = true;
            }
        }
    }
}
