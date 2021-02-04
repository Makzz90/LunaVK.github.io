using System;
using System.Collections.Generic;
using System.Text;

using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml;

namespace LunaVK.Core.Utils
{
    public static class AnimationUtils
    {
        public static void Animate(double to, DependencyObject target, string propertyName, double durationSeconds)
        {
            Storyboard storyboard = new Storyboard();
            DoubleAnimation doubleAnimation = new DoubleAnimation();
            doubleAnimation.To = new double?(to);
            doubleAnimation.AutoReverse = false;
            doubleAnimation.Duration = TimeSpan.FromSeconds(durationSeconds);
            doubleAnimation.EasingFunction = new CubicEase();
            Storyboard.SetTargetProperty(doubleAnimation, propertyName);
            Storyboard.SetTarget(doubleAnimation, target);
            storyboard.Children.Add(doubleAnimation);
            storyboard.Begin();
        }

        public static Storyboard Animate(this DependencyObject target, double from, double to, string propertyPath, int duration, int startTime = 0, EasingFunctionBase easing = null, Action completed = null, bool autoReverse = false)
        {
            
            DoubleAnimation doubleAnimation = new DoubleAnimation();
            doubleAnimation.To = (new double?(to));
            doubleAnimation.From = (new double?(from));
            doubleAnimation.AutoReverse = autoReverse;
            doubleAnimation.EasingFunction = easing;
            doubleAnimation.Duration = TimeSpan.FromMilliseconds(duration);
            Storyboard.SetTarget(doubleAnimation, target);
            Storyboard.SetTargetProperty(doubleAnimation, propertyPath);
            Storyboard storyboard = new Storyboard();
            storyboard.BeginTime = new TimeSpan?(TimeSpan.FromMilliseconds(startTime));
            if (completed != null)
                storyboard.Completed += (s, e) => completed();
            storyboard.Children.Add(doubleAnimation);
            storyboard.Begin();
            return storyboard;
        }

        public static Storyboard AnimateSeveral(List<AnimationInfo> animInfoList, int? startTime = null, Action completed = null)
        {
            List<DoubleAnimation> doubleAnimationList = new List<DoubleAnimation>();
            foreach (AnimationInfo animInfo in animInfoList)
            {
                DoubleAnimation doubleAnimation = new DoubleAnimation();
                doubleAnimation.To = (new double?(animInfo.to));
                doubleAnimation.From = (new double?(animInfo.from));
                doubleAnimation.EasingFunction = animInfo.easing;
                doubleAnimation.Duration = TimeSpan.FromMilliseconds(animInfo.duration);
                Storyboard.SetTarget(doubleAnimation, animInfo.target);
                Storyboard.SetTargetProperty(doubleAnimation, animInfo.propertyPath);
                doubleAnimationList.Add(doubleAnimation);
            }
            Storyboard storyboard = new Storyboard();
            if (startTime.HasValue)
                storyboard.BeginTime = (new TimeSpan?(TimeSpan.FromMilliseconds(startTime.Value)));
            else
                storyboard.BeginTime = (new TimeSpan?());
            if (completed != null)
                storyboard.Completed += ((s, e) => completed());
            using (List<DoubleAnimation>.Enumerator enumerator = doubleAnimationList.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    storyboard.Children.Add(enumerator.Current);
                }
            }
            storyboard.Begin();
            return storyboard;
        }

        public class AnimationInfo
        {
            public DependencyObject target { get; set; }
            public double from { get; set; }
            public double to { get; set; }
            public string propertyPath { get; set; }
            public int duration { get; set; }
            public EasingFunctionBase easing { get; set; }
        }
    }
}
