using LunaVK.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

using LunaVK.Core.DataObjects;

namespace LunaVK.UC
{
    public sealed partial class StickersAutoSuggestUC : UserControl
    {
        public class StickersAutoSuggestViewModel : ViewModelBase
        {
            public ObservableCollection<VKSticker> AutoSuggestCollection { get; private set; }
            
            public void SetItems(IEnumerable<VKSticker> items)
            {
                this.AutoSuggestCollection.Clear();
                foreach (var stickersAutoSuggestItem in items)
                    this.AutoSuggestCollection.Add(stickersAutoSuggestItem);
            }

            public StickersAutoSuggestViewModel()
            {
                this.AutoSuggestCollection = new ObservableCollection<VKSticker>();
            }
        }

        public StickersAutoSuggestUC()
        {
            this.InitializeComponent();
            this.LayoutRoot.Visibility = Visibility.Collapsed;
            base.DataContext = new StickersAutoSuggestViewModel();
        }
        
        private DateTime _lastHiddenTime;

        public event EventHandler<VKSticker> StickerTapped;

        private StickersAutoSuggestViewModel VM
        {
            get { return base.DataContext as StickersAutoSuggestViewModel; }
        }

        public void ShowHide(bool show)
        {
            if (show)
                this._inHideState = false;

            VisualStateManager.GoToState(this, show ? "Visible" : "NotVisible", true);
        }

        private bool _inHideState;

        public void SetData(IEnumerable<VKSticker> items, string keyword)
        {
            if(items.Count()==0)
            {
                this._inHideState = true;
                return;
            }
            else
                this._inHideState = false;
            this.VM.SetItems(items);
        }
        
        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var vm = (sender as FrameworkElement).DataContext as VKSticker;
//            if (vm.is_allowed)
//            {
                this.StickerTapped?.Invoke(this, vm);
//            }
//            else
//            {
                //string referrer = !string.IsNullOrEmpty(this.VM.Keyword) ? StickerReferrer.FromKeyword(this.VM.Keyword) : "store";
                //CurrentStickersPurchaseFunnelSource.Source = StickersPurchaseFunnelSource.keyboard;
                //this.currentStickerId = dataContext.StickerData.StickerId;
                //StickersPackView.ShowAndReloadStickers((long)this.currentStickerId, referrer);
//            }
            e.Handled = true;
        }

        public bool HasItemsToShow
        {
            get
            {
                if (this._inHideState)
                    return false;

                return Enumerable.Any(this.VM.AutoSuggestCollection);
            }
        }
    }
}
