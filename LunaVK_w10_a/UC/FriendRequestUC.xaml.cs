using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using LunaVK.Core.Utils;
using LunaVK.Core;
using Windows.UI.Xaml.Shapes;
using LunaVK.Library;
using LunaVK.ViewModels;

namespace LunaVK.UC
{
    public sealed partial class FriendRequestUC : UserControl
    {
        public static readonly DependencyProperty ModelProperty = DependencyProperty.Register("Model", typeof(FriendsViewModel3.VKUserEx), typeof(FriendRequestUC), new PropertyMetadata(null,FriendRequestUC.OnModelChanged));
               
        public FriendsViewModel3.VKUserEx Model
        {
            get
            {
                return (FriendsViewModel3.VKUserEx)base.GetValue(FriendRequestUC.ModelProperty);
            }
            set
            {
                base.SetValue(FriendRequestUC.ModelProperty, value);
            }
        }
                        
        public FriendRequestUC()
        {
            //
            this.InitializeComponent();
        }

        private static void OnModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((FriendRequestUC)d).UpdateDataView();
        }

        private static void OnProfilesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((FriendRequestUC)d).UpdateDataView();
        }

        private static void OnIsSuggestedFriendChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((FriendRequestUC)d).UpdateDataView();
        }
        
        private void UpdateDataView()
        {
            if (this.Model == null )
                return;
            this.RequestOccupation.Visibility = Visibility.Collapsed;
            this.RequestMessage.Visibility = Visibility.Collapsed;
            //this.RecommenderPanel.Visibility = Visibility.Collapsed;
            this.MutualFriendsPanel.Visibility = Visibility.Collapsed;
            this.MutualFriendsPhotosPanel.Children.Clear();

            var user1 = this.Model;
                        
            this.RequestName.Text = user1.Title;
            this.RequestPhoto.ImageSource = new BitmapImage(new Uri(user1.photo_100));//ImageLoader.SetUriSource(this.RequestPhoto, user1.photo_max);
            if (user1.occupation != null && string.IsNullOrWhiteSpace(user1.occupation.name))
            {
                this.RequestOccupation.Text = user1.occupation.name;
                ((UIElement)this.RequestOccupation).Visibility = Visibility.Visible;
            }
            if (!string.IsNullOrWhiteSpace(this.Model.message))
            {
                this.RequestMessage.Text = this.Model.message;//(Extensions.ForUI(this.Model.message));
                ((UIElement)this.RequestMessage).Visibility = Visibility.Visible;
            }
            
            if (this.Model.randomMutualFriends == null || this.Model.randomMutualFriends.Count==0)
                return;
            this.MutualFriendsCountBlock.Text = UIStringFormatterHelper.FormatNumberOfSomething(this.Model.randomMutualFriends.Count, "OneCommonFriendFrm", "TwoFourCommonFriendsFrm", "FiveCommonFriendsFrm");
            this.MutualFriendsPanel.Visibility = Visibility.Visible;

            foreach(var f in this.Model.randomMutualFriends)
            {
                Ellipse ellipse = new Ellipse();
                int num1 = 0;
                ellipse.HorizontalAlignment = ((HorizontalAlignment)num1);
                int num2 = 0;
                ellipse.VerticalAlignment = ((VerticalAlignment)num2);
                ellipse.Height = 40.0;
                ellipse.Width = 40.0;


                ImageBrush brush = new ImageBrush();
                brush.ImageSource = new BitmapImage(new Uri(f.photo_max));
                ellipse.Fill = brush;

                this.MutualFriendsPhotosPanel.Children.Add(ellipse);

                if (this.MutualFriendsPhotosPanel.Children.Count == 8)
                    break;
            }

        }
        /*
        private void Request_OnTapped(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Navigator.Current.NavigateToUserProfile(this.Model.user_id, this.RequestName.Text, "", false);
        }

        private void RecommenderName_OnTapped(object sender, System.Windows.Input.GestureEventArgs e)
        {
            e.Handled = true;
            Navigator.Current.NavigateToUserProfile(this.Model.from, "", "", false);
        }
        */
        private void Button_OnClicked(object sender, RoutedEventArgs e)
        {
            /*
            if (this.Model.RequestHandledAction == null)
                return;
            string format = "\r\n\r\nvar result=API.friends.{0}({{user_id:{3}}});\r\nif (({1}&&result>0)||({2}&&result.success==1)) \r\n    return API.execute.getFriendsWithRequests({{requests_count:1,requests_offset:0,without_friends:1,requests_only:{4},suggested_only:{5}}});\r\nreturn 0;";
            object[] objArray = new object[6]
      {
        (sender == this.AddButton ? "add" : "delete"),
        (sender == this.AddButton ? "true" : "false"),
        (sender == this.AddButton ? "false" : "true"),
        this.Model.user_id,
        null,
        null
      };
            int index1 = 4;
            bool? isSuggestedFriend;
            string str1;
            if (this.NeedBottomSeparatorLine)
            {
                isSuggestedFriend = this.IsSuggestedFriend;
                bool flag = false;
                if ((isSuggestedFriend.GetValueOrDefault() == flag ? (isSuggestedFriend.HasValue ? 1 : 0) : 0) != 0)
                {
                    str1 = "1";
                    goto label_5;
                }
            }
            str1 = "0";
        label_5:
            objArray[index1] = str1;
            int index2 = 5;
            string str2;
            if (this.NeedBottomSeparatorLine)
            {
                isSuggestedFriend = this.IsSuggestedFriend;
                if (isSuggestedFriend.Value)
                {
                    str2 = "1";
                    goto label_9;
                }
            }
            str2 = "0";
        label_9:
            objArray[index2] = str2;
            string str3 = string.Format(format, objArray);
            FriendRequest model = this.Model;
            Action<BackendResult<FriendRequests, ResultCode>> action = (Action<BackendResult<FriendRequests, ResultCode>>)(result => Execute.ExecuteOnUIThread((Action)(() =>
            {
                if (result.ResultCode == ResultCode.Succeeded)
                {
                    FriendRequests resultData = result.ResultData;
                    model.RequestHandledAction(resultData);
                    CountersManager.Current.Counters.friends = resultData.menu_counter;
                    EventAggregator.Current.Publish((object)new CountersChanged(CountersManager.Current.Counters));
                }
                PageBase.SetInProgress(false);
            })));
            PageBase.SetInProgress(true);
            string str4 = "execute";
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            string key = "code";
            string str5 = str3;
            dictionary.Add(key, str5);
            // ISSUE: variable of the null type
            CancellationToken? nullable = new CancellationToken?();
            // ISSUE: variable of the null type
            VKRequestsDispatcher.DispatchRequestToVK<FriendRequests>(str4, dictionary, action, null, false, true, nullable, null);*/
        }

        public event RoutedEventHandler AddClick
        {
            add { this.AddButton.Click += value; }
            remove { this.AddButton.Click -= value; }
        }

        public event RoutedEventHandler HideClick
        {
            add { this.HideButton.Click += value; }
            remove { this.HideButton.Click -= value; }
        }

        private void Background_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            e.Handled = true;
            NavigatorImpl.Instance.NavigateToProfilePage(this.Model.Id);
        }
    }
}
