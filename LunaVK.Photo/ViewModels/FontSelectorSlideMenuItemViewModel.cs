using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace LunaVK.Photo.ViewModels
{
    public class FontSelectorSlideMenuItemViewModel : INotifyPropertyChanged
    {
        public FontSelectorSlideMenuItemViewModel(TextBox tb)
        {
            this.textBlock = tb;
            this.FontFamilyGroup = new ObservableCollection<FontFamily>();
            
            this.FontFamilyGroup.Add(new FontFamily("Arial"));
            this.FontFamilyGroup.Add(new FontFamily("Calibri"));
            this.FontFamilyGroup.Add(new FontFamily("Impact"));
            this.FontFamilyGroup.Add(new FontFamily("Segoe UI"));
            this.FontFamilyGroup.Add(new FontFamily("Times New Roman"));

            var selected = this.FontFamilyGroup.First(f => f.Source == tb.FontFamily.Source);

            this.SelectedFont = selected;
        }

        public TextBox textBlock { get; set; }

        public ObservableCollection<FontFamily> FontFamilyGroup { get; set; }

        private FontFamily _selectedFont;
        public FontFamily SelectedFont
        {
            get
            {
                return this._selectedFont;//return this.textBlock.FontFamily;
            }
            set
            {
                if (this._selectedFont == value)// if (this.textBlock.FontFamily == value)
                    return;

                this._selectedFont = value;
                this.textBlock.FontFamily = value;
                this.RaisePropertyChanged(nameof(SelectedFont));
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
