using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Enums;
using LunaVK.Core.Framework;
using LunaVK.Core.Library;
using LunaVK.Core.Utils;
using LunaVK.Framework;
using LunaVK.Library;
using LunaVK.UC;
using LunaVK.UC.AttachmentPickers;
using LunaVK.UC.PopUp;
using LunaVK.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace LunaVK.Pages
{
    /// <summary>
    /// NewPost
    /// </summary>
    public sealed partial class NewPostPage : PageBase
    {
        private PopUpService _flyout;

        public NewPostPage()
        {
            this.InitializeComponent();
            this.Loaded += NewPostPage_Loaded;
        }

        private void NewPostPage_Loaded(object sender, RoutedEventArgs e)
        {
            CustomFrame.Instance.Header.IsVisible = false;
            CustomFrame.Instance.Header.HideSandwitchButton = true;
            

            base.InitializeProgressIndicator();
        }
        
        private WallPostViewModel VM
        {
            get { return base.DataContext as WallPostViewModel; }
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            Dictionary<string, object> QueryString = e.Parameter as Dictionary<string, object>;
            //WallPost parameterForIdAndReset2 = ParametersRepository.GetParameterForIdAndReset("EditWallPost") as WallPost;


            WallPostViewModel.Mode mode = (WallPostViewModel.Mode)QueryString["Mode"];
            int UserOrGroupId = (int)QueryString["UserOrGroupId"];
            VKAdminLevel adminLevel = (VKAdminLevel)QueryString["AdminLevel"];
            bool isPublicPage = (bool)QueryString["IsPublicPage"];



            WallPostViewModel wallPostViewModel = null;//new WallPostViewModel(mode, UserOrGroupId, adminLevel,isPublicPage);
            

            if(QueryString.ContainsKey("Data"))
            {
                VKWallPost data = (VKWallPost)QueryString["Data"];
                wallPostViewModel = new WallPostViewModel(data, adminLevel);
                wallPostViewModel.WMMode = WallPostViewModel.Mode.PublishWallPost;
                wallPostViewModel.IsPublishSuggestedSuppressed = true;
            }
            else
            {
                wallPostViewModel = new WallPostViewModel(mode, UserOrGroupId, adminLevel, isPublicPage);
            }

            base.DataContext = wallPostViewModel;


            //base.Title = this.VM.Title;
            base.InitializeProgressIndicator();
        }

        protected override void HandleOnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            CustomFrame.Instance.Header.IsVisible = true;
            CustomFrame.Instance.Header.HideSandwitchButton = false;
        }

        private void AddAttachmentTap(object sender, TappedRoutedEventArgs e)
        {

        }

        private void Attachment_Loaded(object sender, RoutedEventArgs e)
        {//rubberBand
            FrameworkElement element = sender as FrameworkElement;

            CompositeTransform scaleTransform = element.RenderTransform as CompositeTransform;

            scaleTransform.ScaleX = scaleTransform.ScaleY = 0;
            scaleTransform.CenterX = element.ActualWidth / 2.0;
            scaleTransform.CenterY = element.ActualHeight / 2.0;

            ElasticEase ease = new ElasticEase();
            ease.Oscillations = 2;
            ease.Springiness = 10;
            ease.EasingMode = EasingMode.EaseOut;



            List<AnimationUtils.AnimationInfo> animInfoList = new List<AnimationUtils.AnimationInfo>();
            animInfoList.Add(new AnimationUtils.AnimationInfo()
            {
                target = scaleTransform,
                propertyPath = "ScaleX",
                from = scaleTransform.ScaleX,
                to = 1,
                duration = 1000,
                easing = ease
            });
            animInfoList.Add(new AnimationUtils.AnimationInfo()
            {
                target = scaleTransform,
                propertyPath = "ScaleY",
                from = scaleTransform.ScaleY,
                to = 1,
                duration = 1000,
                easing = ease
            });
            AnimationUtils.AnimateSeveral(animInfoList);
        }

        private void Delete_Tapped(object sender, TappedRoutedEventArgs e)
        {
            IOutboundAttachment a = (sender as FrameworkElement).DataContext as IOutboundAttachment;





            FrameworkElement elementBorder = sender as FrameworkElement;
            FrameworkElement element = elementBorder.Parent as FrameworkElement;
            element.IsHitTestVisible = false;

            CompositeTransform scaleTransform = element.RenderTransform as CompositeTransform;

            //scaleTransform.ScaleX = scaleTransform.ScaleY = 0;
            scaleTransform.CenterX = element.ActualWidth / 2.0;
            scaleTransform.CenterY = element.ActualHeight / 2.0;

            //ElasticEase ease = new ElasticEase();
            //ease.Oscillations = 2;
            //ease.Springiness = 10;
            //ease.EasingMode = EasingMode.EaseOut;



            List<AnimationUtils.AnimationInfo> animInfoList = new List<AnimationUtils.AnimationInfo>();
            animInfoList.Add(new AnimationUtils.AnimationInfo()
            {
                target = scaleTransform,
                propertyPath = "ScaleX",
                from = scaleTransform.ScaleX,
                to = 0,
                duration = 200,
                //easing = ease
            });
            animInfoList.Add(new AnimationUtils.AnimationInfo()
            {
                target = scaleTransform,
                propertyPath = "ScaleY",
                from = scaleTransform.ScaleY,
                to = 0,
                duration = 200,
                //easing = ease
            });
            animInfoList.Add(new AnimationUtils.AnimationInfo()
            {
                target = scaleTransform,
                propertyPath = "Rotation",
                from = scaleTransform.Rotation,
                to = 90,
                duration = 200,
                //easing = ease
            });
            animInfoList.Add(new AnimationUtils.AnimationInfo()
            {
                target = element,
                propertyPath = "Opacity",
                from = element.Opacity,
                to = 0,
                duration = 150,
                //easing = ease
            });
            AnimationUtils.AnimateSeveral(animInfoList, null, () => {
                //this.OnImageDeleteTap?.Invoke(a);
                this.VM.Attachments.Remove(a);
            });

        }

        private void Photo_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this._flyout = new PopUpService();
            this._flyout.OverrideBackKey = true;
            this._flyout.AnimationTypeChild = PopUpService.AnimationTypes.Slide;

            AttachmentPickerUC uc = new AttachmentPickerUC((byte)this.VM.Attachments.Count, 10, AttachmentPickerUC.NamedAttachmentEnum.PhotoVideo);
            uc.AttachmentsAction = this.HandleAttachmentsAction;
            this._flyout.Child = uc;
            this._flyout.Show();
        }

        private void HandleAttachmentsAction(IReadOnlyList<IOutboundAttachment> list)
        {
            foreach (var attach in list)
            {
                if (attach is OutboundPhotoAttachment outboundPhoto)
                    outboundPhoto.IsForWallPost = true;
                this.VM.Attachments.Add(attach);
            }
            this._flyout.Hide();
        }

        private void HandleDocumentAction(VKDocument doc)
        {
            OutboundDocumentAttachment o = new OutboundDocumentAttachment(doc);
            this.VM.Attachments.Add(o);
            this._flyout.Hide();
        }

        private void Document_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this._flyout = new PopUpService();
            this._flyout.OverrideBackKey = true;
            this._flyout.AnimationTypeChild = PopUpService.AnimationTypes.Slide;

            DocumentsPickerUC uc = new DocumentsPickerUC();
            uc.AttachmentsAction = this.HandleAttachmentsAction;
            uc.DocumentAction = this.HandleDocumentAction;
            this._flyout.Child = uc;
            this._flyout.Show();
        }
        /*
         * attachmentTypes = new List<NamedAttachmentType>((IEnumerable<NamedAttachmentType>) AttachmentTypes.AttachmentTypesWithPhotoFromGalleryAndLocation);
        if (this.WallPostVM.CanAddPollAttachment)
          attachmentTypes.Add(AttachmentTypes.PollAttachmentType);
        if (this.WallPostVM.CannAddTimerAttachment)
          attachmentTypes.Add(AttachmentTypes.TimerAttachmentType);
        maxCount = this.WallPostVM.NumberOfAttAllowedToAdd;
        */

        private void Poll_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this._flyout = new PopUpService();
            this._flyout.OverrideBackKey = true;
            this._flyout.AnimationTypeChild = PopUpService.AnimationTypes.Slide;

            CreateEditPollUC uc = new CreateEditPollUC();
            uc.CancelClick = () => {
                this._flyout.Hide();
            };
            //uc.AttachmentsAction = this.HandleAttachmentsAction;
            //uc.DocumentAction = this.HandleDocumentAction;
            this._flyout.Child = uc;
            this._flyout.Show();
        }

        private void TextBoxPost_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.VM.Text = (sender as TextBox).Text;
        }
        
        private bool _autorsPanelOpened;
        private void Header_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (this.VM.Authors.Count == 1)
                return;

            if (this._autorsPanelOpened)
                this.HideAutors.Begin();
            else
                this.ShowAutors.Begin();
            this._autorsPanelOpened = !this._autorsPanelOpened;
        }

        private void PostScheduleUC_CancelClick(object sender, RoutedEventArgs e)
        {
            this._flyTime.Hide();
        }

        private void PostScheduleUC_SaveClick(object sender, RoutedEventArgs e)
        {
            this.VM.Time = this._ucSchedule.Data;
            this._flyTime.Hide();
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.HideAutors.Begin();
            this._autorsPanelOpened = false;
        }


        private void _appBarButtonSend_Click(object sender, RoutedEventArgs e)
        {
            AppBarButton btn = sender as AppBarButton;
            btn.IsEnabled = false;
            //this._isPublishing = true;
            //this.UpdateViewState();
            this.VM.Publish((res)=>
            {
                Execute.ExecuteOnUIThread(() =>
                {
                    if (res.error_code == VKErrors.None)
                    {
                    
                            NavigatorImpl.GoBack();
                    }
                    else
                    {
                    
                        if (res.error_code != VKErrors.AccessToAddingPostDenied)
                        {
                            //this.UpdateViewState();
                            GenericInfoUC.ShowBasedOnResult("", res);
                            return;
                        }


                        //IEnumerable<IOutboundAttachment> arg_EF_0 = this.WallPostVM.OutboundAttachments;
                        //Func<IOutboundAttachment, bool> arg_EF_1 = new Func<IOutboundAttachment, bool>((a) => { return a.AttachmentId == "timestamp"; });

                        if (this.VM.Time != DateTime.MinValue)
                        {
                            //this.UpdateViewState();
                            new GenericInfoUC(3000).ShowAndHideLater(LocalizedStrings.GetString("ScheduledForExistingTime"));
                            return;
                        }
                        //this.UpdateViewState();
                        new GenericInfoUC(3000).ShowAndHideLater(LocalizedStrings.GetString("PostsLimitReached"));
                    
                }
                });
            });
        }

        private void Image_Tap(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            var vm = (sender as FrameworkElement).DataContext as IOutboundAttachment;
            if (vm.UploadState != OutboundAttachmentUploadState.Uploading)
                vm.Upload(null);
        }

        private void ListView_Loaded(object sender, RoutedEventArgs e)
        {
            ListView lv = sender as ListView;
            lv.SelectionChanged += this.ListView_SelectionChanged;
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            NavigatorImpl.GoBack();
        }
    }
}
