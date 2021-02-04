using LunaVK.Core.DataObjects;
using LunaVK.Core.Framework;
using LunaVK.Core.Library;
using LunaVK.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using LunaVK.Core.Utils;
using Windows.UI.Xaml.Media.Animation;
using LunaVK.Library;
using LunaVK.Core.Network;

namespace LunaVK.UC
{
    public sealed partial class StickersPackViewUC : UserControl
    {
        private static readonly EasingFunctionBase ANIMATION_EASING;
        private static PopUpService _flyout;
        private static StickersPackViewUC _control;

        public StickersPackViewUC()
        {
            this.InitializeComponent();
        }

        static StickersPackViewUC()
        {
            CubicEase cubicEase = new CubicEase() { EasingMode = EasingMode.EaseInOut };
            StickersPackViewUC.ANIMATION_EASING = cubicEase;
        }

        private void SendAsAGift_OnTap(object sender, TappedRoutedEventArgs e)
        {
            if (this._stockItemHeader == null)
                return;
            /*
            //EventAggregator.Current.Publish(new GiftsPurchaseStepsEvent(GiftPurchaseStepsSource.stickers_present, GiftPurchaseStepsAction.store));
            //EventAggregator.Current.Publish(new StickersPurchaseFunnelEvent(StickersPurchaseFunnelAction.present_button_clicked));
            long productId = (long)this._stockItemHeader.ProductId;
            long userOrChatId = this._stockItemHeader.UserOrChatId;
            bool isChat = this._stockItemHeader.IsChat;
            if (productId == 0L || userOrChatId == 0L)
                return;
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            FullscreenLoader fullscreenLoader = new FullscreenLoader();
            fullscreenLoader.HideOnBackKeyPress = true;
            Action<FullscreenLoaderHiddenEventArgs> action = (args => cancellationTokenSource.Cancel());
            fullscreenLoader.HiddenCallback = action;
            FullscreenLoader loader = fullscreenLoader;
            loader.Show(null, true);
            GiftsService.Instance.GetGiftInfoFromStore(productId, userOrChatId, isChat, (Action<BackendResult<GiftInfoFromStoreResponse, ResultCode>>)(result =>
            {
                loader.Hide(false);
                if (result.ResultCode == ResultCode.Succeeded)
                {
                    GiftInfoFromStoreResponse resultData = result.ResultData;
                    List<long> userIds = resultData.userIds;
                    GiftsSectionItem giftItem = resultData.giftItem;
                    Gift gift = giftItem.gift;
                    if (userIds == null || userIds.Count == 0)
                    {
                        Execute.ExecuteOnUIThread((Action)(() => MessageBox.Show(isChat ? CommonResources.AllChatParticipantsHaveStickerPack : CommonResources.UserAlreadyHasStickerPack, CommonResources.StickerPack, (MessageBoxButton)0)));
                    }
                    else
                    {
                        if (giftItem == null || gift == null)
                            return;
                        NavigatorImpl.Instance.NavigateToGiftSend(gift.id, "stickers", giftItem.description, gift.thumb_256, giftItem.price, giftItem.gifts_left, userIds, true);
                    }
                }
                else
                    GenericInfoUC.ShowBasedOnResult((int)result.ResultCode, "", (VKRequestsDispatcher.Error)null);
            }), new CancellationToken?(cancellationTokenSource.Token));
            */
        }

        private void ShowStoryboard_OnCompleted(object sender, object e)
        {
            this._isAnimating = false;
        }

        private void BorderContent_OnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void BorderContent_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            //if (e.PinchManipulation != null)
            //    return;
            Point translation = e.Delta.Translation;
            //bool? handled = this.Handled;
            //if (handled.HasValue)
            //{
            //    handled = this.Handled;
            //    if (handled.Value)
            //        goto label_5;
            //}
            
            //this.Handled = new bool?(Math.Abs(((Point)@translation).Y) > Math.Abs(((Point)@translation).X));
            //handled = this.Handled;
            //if (!handled.Value)
            //    return;
            //label_5:
            e.Handled = true;
            // ISSUE: explicit reference operation
            this.HandleDragDelta(translation.Y);
        }

        private void BorderContent_OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            e.Handled = true;
            //bool? handled = this.Handled;
            //this.Handled = new bool?();
            //if (!handled.HasValue || !handled.Value)
            //    return;
            Point linearVelocity = e.Cumulative.Translation;//e.FinalVelocities.LinearVelocity;
            this.HandleDragCompleted(linearVelocity.Y);
        }

        private void HandleDragDelta(double delta)
        {
            TranslateTransform translateContent = this.translateContent;
            double num = translateContent.Y + delta;
            translateContent.Y = num;
        }

        private void HandleDragCompleted(double velocityY)
        {
            if (this._isAnimating)
                return;
            double y = this.translateContent.Y;
            bool? nullable1 = new bool?();
            if (velocityY <= -500.0)
                nullable1 = new bool?(true);
            else if (velocityY >= 500.0)
                nullable1 = new bool?(false);
            if (!nullable1.HasValue)
            {
                if (y <= -100.0)
                    nullable1 = new bool?(true);
                else if (y >= 100.0)
                    nullable1 = new bool?(false);
            }
            double num = 0.0;
            bool? nullable2 = nullable1;
            bool flag1 = true;
            if ((nullable2.GetValueOrDefault() == flag1 ? (nullable2.HasValue ? 1 : 0) : 0) != 0)
            {
//                num = -(this._height + this._contentMarginTop);
            }
            else
            {
                nullable2 = nullable1;
                bool flag2 = false;
//                if ((nullable2.GetValueOrDefault() == flag2 ? (nullable2.HasValue ? 1 : 0) : 0) != 0)
//                    num = this._pageHeight - this._contentMarginTop;
            }
            if (nullable1.HasValue)
            {
                this.Hide(nullable1.Value);
            }
            else
            {
                if (num - this.translateContent.Y == 0.0)
                    return;
                this.AnimateToInitial();
            }
        }

        private void AnimateToInitial()
        {
            this._isAnimating = true;
            this.translateContent.Animate(this.translateContent.Y, 0.0, "Y", 175, 0, StickersPackViewUC.ANIMATION_EASING, (() => this._isAnimating = false));
        }

        public static void Show(uint stickerId, string referrer)
        {
            //StickersPackViewUC.ShowWithLoader(referrer, ((callback, cancellationToken) => StoreService.Instance.GetStockItemByStickerId(stickerId, callback, new CancellationToken?(cancellationToken))));
            StickersPackViewUC.Init((callback, cancellationToken) => StoreService.Instance.GetStockItemByStickerId(stickerId, callback, new CancellationToken?(cancellationToken)));
        }

        public static void Show(string stickersPackName, string referrer)
        {
            //StickersPackViewUC.ShowWithLoader(referrer, (callback, cancellationToken) => StoreService.Instance.GetStockItemByName(stickersPackName, callback));
            StickersPackViewUC.Init((callback, cancellationToken) => StoreService.Instance.GetStockItemByName(stickersPackName, callback, new CancellationToken?(cancellationToken)));
        }

        private static void Init(Action<Action<VKResponse<StockItem>>, CancellationToken> loadAction)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            StickersPackViewUC stickersPackView = new StickersPackViewUC();
            var dialogService = new PopUpService();
            //dialogService.AnimationType = DialogService.AnimationTypes.None;
            dialogService.AnimationTypeChild = PopUpService.AnimationTypes.Slide;

            dialogService.OverrideBackKey = true;
            dialogService.Child = stickersPackView;
            
            dialogService.OnClosingAction = (callback => {

                stickersPackView.AnimateHide(false, callback);
                cancellationTokenSource.Cancel();
            });
            
            StickersPackViewUC._control = stickersPackView;
            StickersPackViewUC._flyout = dialogService;
            StickersPackViewUC._flyout.Show();
            
            loadAction(result =>
            {
                if (result.error.error_code == Core.Enums.VKErrors.None)
                    Execute.ExecuteOnUIThread((() => StickersPackViewUC.Show(result.response, "")));
                
            }, cancellationTokenSource.Token);
        }

        public static void Show(StockItem stockItemHeader, string referrer)
        {
            StickersPackViewUC stickersPackView = StickersPackViewUC._control;//new StickersPackViewUC();
            stickersPackView.brdContent.Visibility = Visibility.Visible;
            stickersPackView.loading.Visibility = Visibility.Collapsed;
            stickersPackView.Init(stockItemHeader, referrer);
            stickersPackView.AnimateShow();
            //            var dialogService = new PopUpService();
            //dialogService.AnimationType = DialogService.AnimationTypes.None;
            //            dialogService.AnimationTypeChild = PopUpService.AnimationTypes.None;
            //            dialogService.BackgroundBrush = new SolidColorBrush(Colors.Transparent);
            //            dialogService.OverrideBackKey = true;
            //dialogService.IsOverlayApplied = false;
            //dialogService.HideOnNavigation = false;
            //            Action<Action> action = (callback => stickersPackView.AnimateHide(false, callback));
            //dialogService.OnClosingAction = action;
            //            StickersPackViewUC._flyout = dialogService;
            //            StickersPackViewUC._flyout.Opened += ((sender, args) =>
            //            {
            //                stickersPackView.AnimateShow();
            //EventAggregator.Current.Publish(new StickersPurchaseFunnelEvent(StickersPurchaseFunnelAction.sticker_page));
            //            });
            //            StickersPackViewUC._flyout.Child = stickersPackView;
            //            StickersPackViewUC._flyout.Show();
        }

        private static void ShowWithLoader(string referrer, Action<Action<VKResponse<StockItem>>, CancellationToken> loadAction, int peerId = 0)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            //var fullscreenLoader = new PopUpService();
            //fullscreenLoader.OverrideBackKey = true;
            //fullscreenLoader.AnimationTypeChild = PopUpService.AnimationTypes.Slide;
            //var uc = new StickersPackViewUC();
            //fullscreenLoader.Child = uc;
            //fullscreenLoader.Show();

            //fullscreenLoader.HideOnBackKeyPress = true;
            //Action<FullscreenLoaderHiddenEventArgs> action = (Action<FullscreenLoaderHiddenEventArgs>)(args => cancellationTokenSource.Cancel());
            //fullscreenLoader.HiddenCallback = action;
            loadAction( result =>
            {
                //loader.Hide(false);
                if (result.error.error_code == Core.Enums.VKErrors.None)
                    Execute.ExecuteOnUIThread((() => StickersPackViewUC.Show(result.response, referrer)));
                //else
                //    GenericInfoUC.ShowBasedOnResult((int)result.ResultCode, "", null);
            }, cancellationTokenSource.Token);
        }

        private bool _isAnimating;
        private bool _isHidden;
//        private double _height;
//        private double _contentMarginTop;
//        private double _pageHeight;

        private void AnimateShow()
        {
            this._isAnimating = true;
            this.ShowStoryboard.Begin();
        }

        private void AnimateHide(bool up, Action callback = null)
        {
            if (this._isHidden)
            {
                if (callback == null)
                    return;
                callback();
            }
            else
            {
                this._isAnimating = true;
                if (up)
                {
                    List<AnimationUtils.AnimationInfo> animInfoList = new List<AnimationUtils.AnimationInfo>();
                    animInfoList.Add(new AnimationUtils.AnimationInfo()
                    {
                        target = this.translateContent,
                        from = this.translateContent.Y,
                        to = -(base.ActualHeight),
                        propertyPath = "Y",
                        duration = 200,
                        easing = StickersPackViewUC.ANIMATION_EASING
                    });
                    /*
                    animInfoList.Add(new AnimationUtils.AnimationInfo()
                    {
                        target = this.rectBackground,
                        from = this.rectBackground.Opacity,
                        to = 0.0,
                        propertyPath = "Opacity",
                        duration = 200,
                        easing = StickersPackViewUC.ANIMATION_EASING
                    });*/
                    int? startTime = new int?();
                    Action completed = (() =>
                    {
                        this._isAnimating = false;
                        this._isHidden = true;
                        Action action = callback;
                        if (action == null)
                            return;
                        action();
                    });
                    AnimationUtils.AnimateSeveral(animInfoList, startTime, completed);
                }
                else
                {
                    List<AnimationUtils.AnimationInfo> animInfoList = new List<AnimationUtils.AnimationInfo>();
                    animInfoList.Add(new AnimationUtils.AnimationInfo()
                    {
                        target = this.translateContent,
                        from = this.translateContent.Y,
                        to = base.ActualHeight,
                        propertyPath = "Y",
                        duration = 200,
                        easing = StickersPackViewUC.ANIMATION_EASING
                    });
                    /*
                    animInfoList.Add(new AnimationUtils.AnimationInfo()
                    {
                        target = (DependencyObject)this.rectBackground,
                        from = ((UIElement)this.rectBackground).Opacity,
                        to = 0.0,
                        propertyPath = "Opacity",
                        duration = 200,
                        easing = StickersPackViewUC.ANIMATION_EASING
                    });
                    */
                    int? startTime = new int?();
                    Action completed = (() =>
                    {
                        this._isAnimating = false;
                        this._isHidden = true;
                        if (callback == null)
                            return;
                        callback();
                    });
                    AnimationUtils.AnimateSeveral(animInfoList, startTime, completed);
                }
            }
        }

        private StockItem _stockItemHeader;
//        private double _contentMarginTopPortrait;
//        private double _pageHeightPortrait;
//        private double _pageHeightLandscape;
//        private double _contentMarginTopLandscape;

/*
        private void PrepareAnimations()
        {
            this.rectBackground.Opacity = 0.0;
            this.translateContent.Y = 96.0;
            ((DoubleKeyFrame)this.splineKeyFrameShowBegin).Value = 96.0;
            ((DoubleKeyFrame)this.splineKeyFrameShowEnd).Value = 0.0;
        }

        private void UpdateValuesForOrientation()
        {
            //if ((this._currentPage == null ? 0 : (this._currentPage.Orientation == PageOrientation.Landscape || this._currentPage.Orientation == PageOrientation.LandscapeLeft ? 1 : (this._currentPage.Orientation == PageOrientation.LandscapeRight ? 1 : 0))) != 0)
            //{
            //    this._contentMarginTop = this._contentMarginTopLandscape;
            //    this._pageHeight = this._pageHeightLandscape;
            //}
            //else
            //{
                this._contentMarginTop = this._contentMarginTopPortrait;
                this._pageHeight = this._pageHeightPortrait;
            //}
            if (this._contentMarginTop < 32.0)
            {
                this._contentMarginTop = 32.0;
                this.scrollViewerContent.VerticalScrollBarVisibility = ((ScrollBarVisibility)3);
            }
            else
                this.scrollViewerContent.VerticalScrollBarVisibility = ((ScrollBarVisibility)0);
            ((FrameworkElement)this.borderContent).MaxHeight = (this._pageHeight - this._contentMarginTop * 2.0);
            ((FrameworkElement)this.borderContent).Margin = (new Thickness(0.0, this._contentMarginTop, 0.0, 0.0));
        }
*/
        private void Init(StockItem stockItemHeader, string referrer)
        {
            this._stockItemHeader = stockItemHeader;
            base.DataContext = this._stockItemHeader;
            this.listBoxNavDots.SelectedIndex = 0;
            //if (!string.IsNullOrEmpty(referrer))
            //    this.ucStickersPackInfo.Referrer = referrer;
            //string description = this._stockItemHeader.description;
            //if (!string.IsNullOrEmpty(description))
            //{
            //    this.textBlockDescription.Visibility = Visibility.Visible;
            //    this.textBlockDescription.Text = description;
            //}
            if (this._stockItemHeader.price > 0 && (this._stockItemHeader.can_purchase /*|| this._stockItemHeader.IsChat*/))
                this.gridSendAsAGift.Visibility = Visibility.Visible;

/*
            //this._currentPage = (PhoneApplicationPage)FramePageUtils.CurrentPage;
            var content = CustomFrame.Instance.Content as Page;
            this._pageHeightPortrait = content.ActualHeight;
            this._pageHeightLandscape = content.ActualWidth;
            this._height = 0.0;
            this._height = this._height + this.gridSlideView.Height;
            Thickness margin2 = this.ucStickersPackInfo.Margin;
            double num2 = this.ucStickersPackInfo.Height + this.ucStickersPackInfo.Margin.Top + margin2.Bottom;
            this._height = this._height + num2;
            double num3 = this.textBlockDescription.ActualHeight + this.textBlockDescription.Margin.Top;
            double num4 = num3 + this.textBlockDescription.Margin.Bottom;
            this._height = this._height + num4;
            if (this.gridSendAsAGift.Visibility == Visibility.Visible)
            {
                double num6 = this.gridSendAsAGift.Height + this.gridSendAsAGift.Margin.Top + this.gridSendAsAGift.Margin.Bottom;
                this._height += num6;
            }
            
            this._height = Math.Round(this._height);
            this._contentMarginTopPortrait = Math.Round((this._pageHeightPortrait - this._height) / 2.0);
            this._contentMarginTopLandscape = Math.Round((this._pageHeightLandscape - this._height) / 2.0);
            this.UpdateValuesForOrientation();
            this.PrepareAnimations();
*/
        }

        

        private void Hide(bool up = false)
        {
            this.AnimateHide(up, (() =>
            {
                var flyout = StickersPackViewUC._flyout;
                flyout?.Hide();
            }));
        }

        private void SlideView_OnSelectionChanged(object sender, int e)
        {
            if ((sender as FrameworkElement).DataContext == null)
                return;
            if (this.listBoxNavDots.Items.Count == 0)
                return;
            //if (this._stockItemHeader == null)
             //   return;
            //List<string> demoPhotos = this._stockItemHeader.DemoPhotos;
            //if ((demoPhotos != null ? (demoPhotos.Count == 0 ? true : false) : false) != false)
            //    return;
            //if (this.listBoxNavDots.ItemsSource == null)
            //    this.listBoxNavDots.ItemsSource = this._stockItemHeader.DemoPhotos;
            this.listBoxNavDots.SelectedIndex = e;
        }
    }
}
