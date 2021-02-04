using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI;
using Windows.UI.Xaml;
using LunaVK.Core.Utils;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Media;
using System.Linq;

namespace LunaVK.ViewModels
{
    public class GraffitiDrawViewModel
    {
        public ObservableCollection<ColorViewModel> Colors { get; private set; }
        public ObservableCollection<BrushThicknessViewModel> ThicknessItems { get; private set; }

        public GraffitiDrawViewModel()
        {
            this.Colors = new ObservableCollection<ColorViewModel>();
            this.ThicknessItems = new ObservableCollection<BrushThicknessViewModel>();

            this.Colors.Add(new ColorViewModel("#ffe64646") { IsSelected = true });
            this.Colors.Add(new ColorViewModel("#fffe8d49"));
            this.Colors.Add(new ColorViewModel("#fff8d825"));
            this.Colors.Add(new ColorViewModel("#ff2cb946"));
            this.Colors.Add(new ColorViewModel("#ff4089e7"));
            this.Colors.Add(new ColorViewModel("#ff9b4beb"));

            this.Colors.Add(new ColorViewModel("#ff68e4b2"));
            this.Colors.Add(new ColorViewModel("#ffff84a0"));
            this.Colors.Add(new ColorViewModel("#fff6a877"));
            this.Colors.Add(new ColorViewModel("#fff7796b"));
            this.Colors.Add(new ColorViewModel("#ff8a3231"));
            this.Colors.Add(new ColorViewModel("#ff92b656"));
            this.Colors.Add(new ColorViewModel("#ff556e34"));
            this.Colors.Add(new ColorViewModel("#ff5a448f"));

            this.Colors.Add(new ColorViewModel("#ff000000"));
            //this.Colors.Add(new ColorViewModel("#ff333333"));
            this.Colors.Add(new ColorViewModel("#ff4d4d4d"));
            this.Colors.Add(new ColorViewModel("#ff666666"));
            this.Colors.Add(new ColorViewModel("#ff808080"));
            this.Colors.Add(new ColorViewModel("#ff999999"));
            this.Colors.Add(new ColorViewModel("#ffb3b3b3"));
            this.Colors.Add(new ColorViewModel("#ffffffff"));

            this.ThicknessItems.Add(new BrushThicknessViewModel(100, 48));
            this.ThicknessItems.Add(new BrushThicknessViewModel(70, 40));
            this.ThicknessItems.Add(new BrushThicknessViewModel(40, 32));
            this.ThicknessItems.Add(new BrushThicknessViewModel(20, 24) { IsSelected = true });
            this.ThicknessItems.Add(new BrushThicknessViewModel(10, 16));
        }

        public int CurrentThickness
        {
            get
            {
                return (Enumerable.First<BrushThicknessViewModel>(this.ThicknessItems, (Func<BrushThicknessViewModel, bool>)(item => item.IsSelected))).Thickness;
            }
        }

        public Color CurrentColor
        {
            get
            {
                return (Enumerable.First<ColorViewModel>(this.Colors, (Func<ColorViewModel, bool>)(color => color.IsSelected))).Color;
            }
        }

        public class ColorViewModel : LunaVK.Core.ViewModels.ViewModelBase
        {
            public Color Color { get; private set; }
            public string ColorHex { get; private set; }

            public ColorViewModel(string color)
            {
                this.ColorHex = color;
                this.Color = color.ToColor();
            }

            public Visibility SelectedVisibility
            {
                get { return this._isSelected ? Visibility.Visible : Visibility.Collapsed; }
            }

            private bool _isSelected;
            public bool IsSelected
            {
                get
                {
                    return this._isSelected;
                }
                set
                {
                    this._isSelected = value;
                    base.NotifyPropertyChanged<Visibility>(() => this.SelectedVisibility);
                }
            }
        }

        public class BrushThicknessViewModel : LunaVK.Core.ViewModels.ViewModelBase
        {
            public int Thickness { get; private set; }
            public int ViewThickness { get; private set; }
            private bool _isSelected;

            public bool IsSelected
            {
                get
                {
                    return this._isSelected;
                }
                set
                {
                    this._isSelected = value;
                    this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.IsSelected));
                    this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.SelectedVisibility));
                }
            }

            public Visibility SelectedVisibility
            {
                get
                {
                    return this._isSelected ? Visibility.Visible : Visibility.Collapsed;
                }
            }

            public int StrokeThickness
            {
                get
                {
                    return this.ViewThickness + 12;
                }
            }

            public BrushThicknessViewModel(int thickness, int viewThickness)
            {
                this.Thickness = thickness;
                this.ViewThickness = viewThickness;
            }

            private Brush _fillBrush;
            public Brush FillBrush
            {
                get
                {
                    //if (DesignerProperties.IsInDesignTool)
                    //    return (Brush)new SolidColorBrush("#ffe64646".ToColor());
                    return this._fillBrush;
                }
                set
                {
                    this._fillBrush = value;
                    this.NotifyPropertyChanged("FillBrush");
                    //this.NotifyPropertyChanged<int>((System.Linq.Expressions.Expression<Func<int>>)(() => this.ExtraStroke));
                }
            }


        }
    }
}
