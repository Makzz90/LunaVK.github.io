using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using LunaVK.Core.DataObjects;
using LunaVK.UC.Attachment;
using LunaVK.Core.Enums;
using LunaVK.Core.Library;
using System.Diagnostics;
using LunaVK.Core.Utils;

namespace LunaVK.UC
{
    public sealed partial class ItemMessageUC : UserControl
    {
        public ItemMessageUC()
        {
            this.InitializeComponent();
#if DEBUG
            if (Debugger.IsAttached)
            {
                //this._ap.Background = new SolidColorBrush(Windows.UI.Colors.Green);
            }
#endif
        }
        
        public static readonly DependencyProperty DataProperty = DependencyProperty.Register("Data", typeof(VKMessage), typeof(ItemMessageUC), new PropertyMetadata(null, OnDataChanged));

        /// <summary>
        /// Данные.
        /// </summary>
        public VKMessage Data
        {
            get { return (VKMessage)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        private static void OnDataChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ((ItemMessageUC)obj).ProcessData();
        }
        
        

        private void ProcessData()
        {
            if (this.Data == null)
                return;

//#if DEBUG
//            if (Debugger.IsAttached)
//                this._ap.IsMessage = false;
//#endif

            //if (this.Data.OutboundMessageVM != null)
            //{
            //    this.Data.PropertyChanged -= _mvm_PropertyChanged;
            //    this.Data.PropertyChanged += _mvm_PropertyChanged;
            //}

            if (this.Data.action != null)
            {
                Debug.Assert(this.Data.from_id>0);
                VKUser u1 = UsersService.Instance.GetCachedUser((uint)this.Data.from_id);
                VKUser u2 = UsersService.Instance.GetCachedUser((uint)this.Data.action.member_id);
                
         
                //string temp = LunaVK.ViewModels.DialogsViewModel.GenerateText(m, u1, u2, true);
                //this.Footer.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                //m.@out = VKMessageType.Sent;
                //ScrollableTextBlock t = new ScrollableTextBlock();
                //t.SelectionEnabled = false;
                //t.FullOnly = true;
                //t.Text = temp;
                //t.FontSize = (double)Application.Current.Resources["FontSizeContent"];
                //t.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center;
                //this.MainContent.Children.Add(t);
                return;
            }
            /*
            if (!string.IsNullOrEmpty(this.Data.text))
            {
                this._text.Text = this.Data.text;
                this.MainContent.Children.Add(this._text);
            }

            if (this.Data.fwd_messages!=null)
            {
                foreach (VKMessage msg in this.Data.fwd_messages)
                {
                    //
                    if (msg.text == "" && msg.fwd_messages == null && msg.attachments == null && msg.geo == null)
                        msg.text = "(контент удалён)";
                    //
                    double offs = 0;
                    if (this.Data.UserThumbVisibility == Visibility.Visible)
                        offs = 64;
                    ForwardedMessagesUC fwd = new ForwardedMessagesUC(msg, this.MainContent, offs);//content_width - offs
                    //
                    fwd.Margin = new Thickness(10);
                    //
                    this.MainContent.Children.Add(fwd);
                }
            }
            
            if (!this.Data.attachments.IsNullOrEmpty())
            {
                this._ap.Attachments = this.Data.attachments;
                this.MainContent.Children.Add(this._ap);
            }
            */
            this._ap.Text = this.Data.text;
            this._ap.ForwardedMessages = this.Data.fwd_messages;
            this._ap.Attachments = this.Data.attachments;

  //          this.MainContent.Children.Add(this._ap);

  //          if (this.Data.geo != null)
  //          {
  //              MapAttachmentUC map = new MapAttachmentUC(this.Data.geo);
  //              this.MainContent.Children.Add(map);
  //          }

            this.UpdateMargin();
        }

        private void _mvm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "attachments")
            {
                this._ap.Attachments = null;
                this._ap.Attachments = this.Data.attachments;
            }
        }

        private void UpdateMargin()
        {
            /*
            * текст + влож = отступы со всех сторон
            * только текст = отступы со всех сторон
            * картинка вложена = отступов нет
            * статья/ссылка/документ = отступы со всех сторон
            */
            /*
            if (!string.IsNullOrEmpty(this.Data.text))//Есть текст
            {
                if (this.Data.attachments.IsNullOrEmpty()==false)//Есть вложения
                {
                    this._ap.Margin = new Thickness();//10
//                    this._text.Margin = new Thickness(0, 0, 0, 10);
                }
                else if (this.Data.fwd_messages.IsNullOrEmpty() == false)
                {
                    this._ap.Margin = new Thickness();
//                    this._text.Margin = new Thickness(10, 5, 10, 0);
                }
                else
                {
                    this._ap.Margin = new Thickness();
//                    this._text.Margin = new Thickness(14, 7, 14, 7);
                }

                
            }
            else
            {
                //Нет текста
//                this._text.Margin = new Thickness();

                if (this.Data.attachments.IsNullOrEmpty() == false)//есть вложения
                {
                    //base.Margin = new Thickness(10);


                    if (this._ap.IsOnlyImages == false)//if (this._ap.imagesExists == false)
                        this._ap.Margin = new Thickness(10);
                    else
                        this._ap.Margin = new Thickness();
                }
                else if (this.Data.fwd_messages.IsNullOrEmpty() == false)
                {
                    this._ap.Margin = new Thickness(10);
                }
                else
                {
                    this._ap.Margin = new Thickness();
                }
            }*/
        }


        private void Image_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Library.NavigatorImpl.Instance.NavigateToProfilePage(this.Data.from_id);
        }

        private void hb_Click(object sender, RoutedEventArgs e)
        {
            this.Data.Send();
        }

        private void ListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ItemsControl lv = sender as ItemsControl;
            var panel = (ItemsWrapGrid)lv.ItemsPanelRoot;

            int count = lv.Items.Count;
            if (count == 0)
                return;


            panel.MaximumRowsOrColumns = (int)count;

            panel.ItemWidth = e.NewSize.Width / count;
        }

        private void BotKeyboardButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var vm = (sender as FrameworkElement).DataContext as VKBotKeyboard.KeyboardButton;
            
            //this.OnSendPayloadTap(vm.action.label, vm.action.payload);
        }
    }
}
