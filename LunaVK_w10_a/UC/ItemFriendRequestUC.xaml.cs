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
using Windows.UI.Xaml.Navigation;

using LunaVK.Network.DataVM;
using LunaVK.Network;
using System.Windows.Input;

namespace LunaVK.UC
{
    public sealed partial class ItemFriendRequestUC : UserControl
    {
        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register("CommandParameter", typeof(object), typeof(ItemFriendRequestUC), new PropertyMetadata(null, CommandParameterChanged));

        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        private static void CommandParameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ItemFriendRequestUC lv = (ItemFriendRequestUC)d;
            int i = 0;
            lv.Temp.CommandParameter = e.NewValue;
        }

        public static readonly DependencyProperty ChildButtonCommandProperty = DependencyProperty.Register("ChildButtonCommand", typeof(ICommand), typeof(ItemFriendRequestUC), new PropertyMetadata(null, ChildButtonCommandChanged));

        private static void ChildButtonCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ItemFriendRequestUC lv = (ItemFriendRequestUC)d;
            int i = 0;
            lv.Temp.Command = (ICommand)e.NewValue;
        }

        public ICommand ChildButtonCommand
        {
            get { return (ICommand)GetValue(ChildButtonCommandProperty); }
            set { SetValue(ChildButtonCommandProperty, value); }
        }

        public ItemFriendRequestUC()
        {
             
            this.InitializeComponent();
            //this.Temp.Command = ChildButtonCommand;
        }

        private void AddFriend_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            VKFriendVM vm = element.DataContext as VKFriendVM;

           // Dictionary<string, string> parameters = new Dictionary<string, string>();

           // parameters["user_id"] = vm.id.ToString();

            //VKResponse<TempLikes> temp = await RequestsDispatcher.GetResponse<TempLikes>(this.DataVM.likes.user_likes == true ? "likes.delete" : "likes.add", parameters);
     //       this.ActionAdd(vm.id);
       //     this.ChildButtonCommand();
        }

        private void CancelFriend_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            VKFriendVM vm = element.DataContext as VKFriendVM;

            Dictionary<string, string> parameters = new Dictionary<string, string>();

            parameters["user_id"] = vm.id.ToString();
        }
    }
}
