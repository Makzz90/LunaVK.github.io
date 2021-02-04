using LunaVK.Core.Framework;
using LunaVK.Core.Library;
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

namespace LunaVK.Pages.Debug
{
    public sealed partial class TestStickersKeywords : Page
    {
        public ObservableCollection<StickersAutoSuggestDictionary.StickerKeywordItem> Items { get; private set; }

        public TestStickersKeywords()
        {
            this.InitializeComponent();
            base.Loaded += TestStickersKeywords_Loaded;
            this.Items = new ObservableCollection<StickersAutoSuggestDictionary.StickerKeywordItem>();
            base.DataContext = this;
        }

        private void TestStickersKeywords_Loaded(object sender, RoutedEventArgs e)
        {
            StoreService.Instance.GetStickersKeywords((result) => {

                if(result.error.error_code== Core.Enums.VKErrors.None)
                {
                    Execute.ExecuteOnUIThread(() => {
                        foreach(var item in result.response.dictionary)
                        {
                            this.Items.Add(item);
                        }
                        this._progBar.Visibility = Visibility.Collapsed;
                    });

                }
            });
        }
    }
}
