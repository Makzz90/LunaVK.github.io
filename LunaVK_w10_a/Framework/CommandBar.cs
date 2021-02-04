using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media;

using Windows.UI.Xaml.Input;
using System.Collections.ObjectModel;
using LunaVK.Core;

namespace LunaVK.Framework
{
    //[TemplateVisualState(GroupName = "CommonStates", Name = "Normal")]
    public class CommandBar : ContentControl
    {
        public static readonly DependencyProperty PrimaryCommandsProperty = DependencyProperty.Register("PrimaryCommands", typeof(ObservableCollection<AppBarButton>), typeof(CommandBar), new PropertyMetadata(new ObservableCollection<AppBarButton>()));
        public static readonly DependencyProperty SecondaryCommandsProperty = DependencyProperty.Register("SecondaryCommands", typeof(ObservableCollection<AppBarButton>), typeof(CommandBar), new PropertyMetadata(new ObservableCollection<AppBarButton>()));


        //public static readonly DependencyProperty TappedProperty = DependencyProperty.Register("Tapped", typeof(TappedEventHandler), typeof(CommandBar), new PropertyMetadata(null));

        public CommandBar()
        {
            this.DefaultStyleKey = typeof(CommandBar);

            this.PrimaryCommands.CollectionChanged += PrimaryCommands_CollectionChanged;
            this.SecondaryCommands.CollectionChanged += SecondaryCommands_CollectionChanged;
            this.PrimaryCommands.Clear();
            this.SecondaryCommands.Clear();
        }

        public ObservableCollection<AppBarButton> PrimaryCommands
        {
            get { return (ObservableCollection<AppBarButton>)base.GetValue(PrimaryCommandsProperty); }
            set { base.SetValue(PrimaryCommandsProperty, value); }
        }

        public ObservableCollection<AppBarButton> SecondaryCommands
        {
            get { return (ObservableCollection<AppBarButton>)base.GetValue(SecondaryCommandsProperty); }
            set { base.SetValue(SecondaryCommandsProperty, value); }
        }
        /*
        /// <summary>
        /// Базовое состояние панели. Меняется при нажатии на точки.
        /// </summary>
        public bool IsMenuOpened
        {
            get { return (bool)GetValue(IsMenuOpenedProperty); }
            set { SetValue(IsMenuOpenedProperty, value); }
        }
        */
        private bool _isMenuOpened;
        public bool IsMenuOpened
        {
            get { return this._isMenuOpened; }
            private set
            {
                this._isMenuOpened = value;
                //if (this.SecondaryCommands.Count > 0)
                //{
                //    VisualStateManager.GoToState(this, "Minimized", true);
                //    return;
                //}
                if (this.PrimaryCommands.Count == 0 && this.SecondaryCommands.Count == 0)
                {
                    VisualStateManager.GoToState(this, "Empty", true);
                    return;
                }
                VisualStateManager.GoToState(this, value == true ? "MenuOpened" : (this.PrimaryCommands.Count == 0 && this.SecondaryCommands.Count > 0 ? "Minimized" : "Normal"), true);

            }
        }

        private bool _isMenuHiden;
        public bool IsMenuHiden
        {
            get { return this._isMenuHiden; }
            set
            {
                this._isMenuHiden = value;
                if (value == true)
                {
                    VisualStateManager.GoToState(this, "Empty", true);
                }
                else
                {
                    this.IsMenuOpened = this.IsMenuOpened;
                }
            }
        }


        /// <summary>
        /// Сетка с точками внутри
        /// </summary>
        protected FrameworkElement menuButtonBorder;


        //protected Storyboard openMenuStoryboard;

        //protected Storyboard hideMenuStoryboard;
        //protected Storyboard unHideMenuStoryboard;

        protected Grid menuPanel;

        protected Grid primaryPanel;

        /// <summary>
        /// ItemsControl с основными кнопками
        /// </summary>
        protected ItemsControl buttonsControl;

        //        protected FrameworkElement rootCanvas;
        protected FrameworkElement _Rect;

        protected override void OnApplyTemplate()
        {
            this.Width = ((FrameworkElement)Window.Current.Content).ActualWidth;

            base.OnApplyTemplate();

            this.menuButtonBorder = GetTemplateChild("MenuButtonBorder") as FrameworkElement;

            this.primaryPanel = GetTemplateChild("PrimaryComandsPanel") as Grid;
            this.menuPanel = GetTemplateChild("SecondaryCommandsPanel") as Grid;
            //this.openMenuStoryboard = GetTemplateChild("OpenMenuStoryboard") as Storyboard;
            //this.hideMenuStoryboard = GetTemplateChild("HideMenuStoryboard") as Storyboard;
            //this.unHideMenuStoryboard = GetTemplateChild("UnHideMenuStoryboard") as Storyboard;
            this.buttonsControl = GetTemplateChild("ButtonsControl") as ItemsControl;

            if (Settings.CmdBarDivider == false)
            {
                this._Rect = GetTemplateChild("_Rect") as FrameworkElement;
                this._Rect.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }


            //            this.rootCanvas = GetTemplateChild("RootCanvas") as FrameworkElement;


            this.menuButtonBorder.PointerEntered += delegate { VisualStateManager.GoToState(this, "PressedButton", true); };
            this.menuButtonBorder.PointerExited += delegate { VisualStateManager.GoToState(this, "NormalButton", true); };


            ((FrameworkElement)Window.Current.Content).SizeChanged += (s, e) =>
            {
                this.Width = e.NewSize.Width;
            };

            this.menuButtonBorder.Tapped += (s, e) =>
            {
                IsMenuOpened = !IsMenuOpened;
                e.Handled = true;
            };

            VisualStateManager.GoToState(this, "Empty", false);
#if WINDOWS_UWP
            /*
            LunaVK.Framework.JuniperPhotonAcrylicBrush.AcrylicBrush br = new LunaVK.Framework.JuniperPhotonAcrylicBrush.AcrylicBrush();
            LunaVK.Framework.JuniperPhotonAcrylicBrush.AcrylicBrush br2 = new LunaVK.Framework.JuniperPhotonAcrylicBrush.AcrylicBrush();

            if (Settings.BackgroundType == Library.Threelen.On)
            {
                br.TintColor = Windows.UI.Colors.Black;
                br2.TintColor = Windows.UI.Colors.Black;
            }
            else
            {
                br.TintColor = Windows.UI.Colors.White;
                br2.TintColor = Windows.UI.Colors.White;
            }


            br.BlurAmount = 25;
            br2.BlurAmount = 25;
            br.BackdropFactor = 0.3f;
            br2.BackdropFactor = 0.4f;

            this.primaryPanel.Background = br;
            this.menuPanel.Background = br2;*/
#endif
        }

        void temp4_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            e.Handled = true;
        }
        /*
        void buttonsControl_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            int i = 0;
        }
        */
        void SecondaryCommands_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                if (this.PrimaryCommands.Count == 0)
                    VisualStateManager.GoToState(this, "Minimized", false);
            }
        }

        void PrimaryCommands_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems == null)
            {

                VisualStateManager.GoToState(this, "Empty", false);
            }
            else
            {
                VisualStateManager.GoToState(this, "Normal", true);
            }
        }
    }
}
