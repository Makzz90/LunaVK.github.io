using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace LunaVK.Photo.ViewModels
{
    public class SelectColorControlViewModel : INotifyPropertyChanged
    {
        public TextBox textBlock { get; set; }

        public SelectColorControlViewModel(TextBox tb)
        {
            this.textBlock = tb;

            this.Colors = new ObservableCollection<SolidColorBrush>(PredefinedColorPalettes.GetSolidColorBrushProviders());

            var brush = this.textBlock.Foreground as SolidColorBrush;
            var selected = this.Colors.First(c=>c.Color== brush.Color);
            this.SelectedColor = selected;
        }

        public ObservableCollection<SolidColorBrush> Colors { get; set; }

        private SolidColorBrush _selectedColor;
        public SolidColorBrush SelectedColor
        {
            get
            {
                return this._selectedColor;
            }
            set
            {
                if (this._selectedColor == value)// if (this.textBlock.FontFamily == value)
                    return;

                this._selectedColor = value;
                this.textBlock.Foreground = value;
                this.RaisePropertyChanged(nameof(SelectedColor));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string property)
        {
            // ISSUE: reference to a compiler-generated field
            if (this.PropertyChanged == null)
                return;
            //Windows.ApplicationModel.Resources.Core.Framework.Execute.ExecuteOnUIThread(() =>
            //{
            // ISSUE: reference to a compiler-generated field
            PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if (propertyChanged == null)
                return;
            PropertyChangedEventArgs e = new PropertyChangedEventArgs(property);
            propertyChanged(this, e);
            //});
        }
    }
}
