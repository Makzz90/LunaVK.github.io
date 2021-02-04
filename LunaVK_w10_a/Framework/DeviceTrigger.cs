using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using static Windows.ApplicationModel.Resources.Core.ResourceContext;
using static Windows.ApplicationModel.DesignMode;
using Windows.UI.ViewManagement;
//https://github.com/dotMorten/WindowsStateTriggers/blob/master/src/WindowsStateTriggers/DeviceFamilyStateTrigger.cs
//https://www.codeproject.com/Articles/896974/Advanced-View-States-for-Windows-apps
namespace LunaVK.Framework
{
    public class DeviceTrigger : StateTriggerBase
    {
        public enum Families { Mobile, Desktop }

#region Familiy
        public Families Family
        {
            get { return (Families)GetValue(FamilyProperty); }
            set { SetValue(FamilyProperty, value); }
        }
        public static readonly DependencyProperty FamilyProperty = DependencyProperty.Register(nameof(Family), typeof(Families), typeof(DeviceTrigger), new PropertyMetadata(Families.Desktop));
#endregion

#region Orientation
        public ApplicationViewOrientation Orientation
        {
            get { return (ApplicationViewOrientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(nameof(Orientation), typeof(ApplicationViewOrientation), typeof(DeviceTrigger), new PropertyMetadata(ApplicationViewOrientation.Landscape));
#endregion

        public static Frame DisplayFrame => Window.Current.Content == null ? null : Window.Current.Content as Frame;

        public class DeviceInformation
        {
            public static ApplicationViewOrientation Orientation =>
                DisplayInformation.GetForCurrentView().CurrentOrientation.ToString().Contains("Landscape") ? ApplicationViewOrientation.Landscape : ApplicationViewOrientation.Portrait;


            public static Families Family => GetForCurrentView().QualifierValues["DeviceFamily"] == "Mobile" ? Families.Mobile : Families.Desktop;


            public static DisplayInformation DisplayInformation => DisplayInformation.GetForCurrentView();


            //public static Frame DisplayFrame => Window.Current.Content == null ? null : Window.Current.Content as Frame;
        }

        public double MinWindowWidth
        {
            get { return (double)GetValue(MinWindowWidthProperty); }
            set { SetValue(MinWindowWidthProperty, value); }
        }

        public static readonly DependencyProperty MinWindowWidthProperty = DependencyProperty.Register("MinWindowWidth", typeof(double), typeof(DeviceTrigger), new PropertyMetadata(0.0));

        public DeviceTrigger()
        {
            this.Initialize();
        }

        /// <summary>
        /// Initial Trigger
        /// </summary>
        private void Initialize()
        {
            NavigatedEventHandler framenavigated = null;
            framenavigated = (s, e) =>
            {
                DisplayFrame.Navigated -= framenavigated;
                this.SetTrigger(DisplayFrame.ActualWidth);
            };
            DisplayFrame.Navigated += framenavigated;

            //Orientation Trigger
            DeviceInformation.DisplayInformation.OrientationChanged += (s, e) => this.SetTrigger(DisplayFrame.ActualWidth);

            //Orientation Trigger
            Window.Current.SizeChanged += (s, e) =>
            {
                this.SetTrigger(e.Size.Width);
            };

            (Window.Current.Content as FrameworkElement).Loaded += (s, e) =>
            {
                this.SetTrigger(DisplayFrame.ActualWidth);
            };
        }
        /*
        private void SetTrigger()
        {
            base.SetActive(Orientation == DeviceInformation.Orientation && Family == DeviceInformation.Family);
        }
        */
        private void SetTrigger(double width)
        {
            base.SetActive(width >= MinWindowWidth && this.Orientation == DeviceInformation.Orientation && this.Family == DeviceInformation.Family);
        }
    }
}
