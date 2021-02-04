using LunaVK.Photo.UC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace LunaVK.Photo
{
    public abstract class ContainedShapeControlContainer
    {
        /// <summary>
        /// То что внутри AdornerElementBaseUC
        /// </summary>
        public FrameworkElement Control { get; private set; }

        /// <summary>
        /// AdornerElementBaseUC FrameworkElement
        /// </summary>
        public ICompositeShape ParentContainer { get; private set; }

        public virtual bool IsInEditMode { get; set; }

        public ContainedShapeControlContainer(/*ICompositeShape parentContainer,*/ FrameworkElement content, double initialWidth, double initialHeight)
        {
            AdornerElementBaseUC uc = new AdornerElementBaseUC(initialWidth, initialHeight, content);
            uc._scaleChanged = this.ScaleChanged;
            this.ParentContainer = uc;
            this.Control = content;
        }

        public virtual void ScaleChanged(double p) { }

        public virtual List<SlideMenuItemBase> MenuItems { get; }

        public void FadeIn()
        {
            /*
            FrameworkElement control = this.Control;
            double opacity = this.State.Opacity;
            TimeSpan timeSpan = TimeSpan.FromMilliseconds((double)GlobalConstants.CollageAnimationTimeInMillis);
            ExponentialEase exponentialEase1 = new ExponentialEase();
            ((EasingFunctionBase)exponentialEase1).set_EasingMode((EasingMode)1);
            ExponentialEase exponentialEase2 = exponentialEase1;
            control.Fade(opacity, timeSpan, (IEasingFunction)exponentialEase2);*/
            if(this.Control!=null)
                this.Control.Opacity = 1.0;
        }

        public void FadeOut()
        {
            /*
            FrameworkElement control = this.Control;
            TimeSpan timeSpan = TimeSpan.FromMilliseconds((double)GlobalConstants.CollageAnimationTimeInMillis);
            ExponentialEase exponentialEase1 = new ExponentialEase();
            ((EasingFunctionBase)exponentialEase1).set_EasingMode((EasingMode)1);
            ExponentialEase exponentialEase2 = exponentialEase1;
            control.Fade(0.4, timeSpan, (IEasingFunction)exponentialEase2);*/
            if (this.Control != null)
                this.Control.Opacity = 0.4;
        }
    }
}
