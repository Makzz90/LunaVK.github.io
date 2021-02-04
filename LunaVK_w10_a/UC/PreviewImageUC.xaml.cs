using LunaVK.Framework;
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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

using LunaVK.Core.Utils;
using Windows.UI.Xaml.Media.Animation;
using LunaVK.Common;

namespace LunaVK.UC
{
    public sealed partial class PreviewImageUC : UserControl
    {
        private string _previewUri;
        private bool _inAnimation;

        public PreviewImageUC()
        {
            this.InitializeComponent();
        }

        public void SetImageUri(string previewUri, BitmapImage originalImage = null)
        {
            if (this._previewUri == previewUri)
                return;
            this._previewUri = previewUri;
            //if (!ImageCache.Current.HasImageInCache(previewUri) && originalImage != null)
                this.imagePreview.Source = originalImage;
            //else
            ImageViewerLowProfileImageLoader.SetUriSource(this.imagePreview, new Uri(previewUri));//ImageLoader.SetUriSource(this.imagePreview, previewUri);
            //
            //
            //this.imagePreview.Source = new BitmapImage(new Uri(previewUri));
            //
            //
            this.HandleImageOpened();
        }


            private void HandleImageOpened()
        {
            if (!this._inAnimation)
                this.RunAnimation(this.EnsureBigPreview);
            else
                this.EnsureBigPreview();
        }

        private void EnsureBigPreview()
        {
//            if (this._previewUri == ImageLoader.GetUriSource(this.imagePreview))
//                return;
//            ImageLoader.SetUriSource(this.imagePreview, this._previewUri);
        }

        private void RunAnimation(Action callback)
        {
            this._inAnimation = true;
            //
            var renderTransform = (this.imagePreview).RenderTransform as ScaleTransform;

            var animInfoList = new List<AnimationUtils.AnimationInfo>();
            AnimationUtils.AnimationInfo animationInfo1 = new AnimationUtils.AnimationInfo();
            animationInfo1.duration = PreviewBehavior.PUSH_ANIMATION_DURATION;
            animationInfo1.easing = PreviewBehavior.PUSH_ANIMATION_EASING;
            animationInfo1.from = PreviewBehavior.PUSH_SCALE;//renderTransform.ScaleX;
            animationInfo1.to = 1.0;
            animationInfo1.target = renderTransform;
            animationInfo1.propertyPath = "ScaleX";
            animInfoList.Add(animationInfo1);


            AnimationUtils.AnimationInfo animationInfo2 = new AnimationUtils.AnimationInfo();
            animationInfo2.duration = PreviewBehavior.PUSH_ANIMATION_DURATION;
            animationInfo2.easing = PreviewBehavior.PUSH_ANIMATION_EASING;
            animationInfo2.from = PreviewBehavior.PUSH_SCALE;//renderTransform.ScaleY;
            animationInfo2.to = 1.0;
            animationInfo2.target = renderTransform;
            animationInfo2.propertyPath = "ScaleY";
            animInfoList.Add(animationInfo2);


            AnimationUtils.AnimationInfo animationInfo3 = new AnimationUtils.AnimationInfo();
            animationInfo3.duration = PreviewBehavior.PUSH_ANIMATION_DURATION;
            animationInfo3.easing = PreviewBehavior.PUSH_ANIMATION_EASING;
            animationInfo3.from = this.rect.Opacity;
            animationInfo3.to = 0.4;
            animationInfo3.target = rect;
            animationInfo3.propertyPath = "Opacity";
            animInfoList.Add(animationInfo3);

            Action completed = (() =>
            {
                this._inAnimation = false;
                callback();
            });
            AnimationUtils.AnimateSeveral(animInfoList, null, completed);
        }

        public Image imagePreview
        {
            get
            {
                return this._imagePreview;
            }
        }
        //PreviewBehavior
        //StickersAutoSuggestUC
    }
}
