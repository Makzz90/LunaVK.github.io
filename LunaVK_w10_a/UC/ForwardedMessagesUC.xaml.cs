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

using LunaVK.Core.DataObjects;
using LunaVK.Framework;
using LunaVK.Core.Library;
using LunaVK.Core;
using System.Diagnostics;
using LunaVK.Core.Framework;
using LunaVK.Core.Enums;
using LunaVK.Library;

namespace LunaVK.UC
{
    public sealed partial class ForwardedMessagesUC : UserControl
    {
        public ForwardedMessagesUC()
        {
            this.InitializeComponent();
        }

        public ForwardedMessagesUC(VKComment comment, FrameworkElement uc)
            : this()
        {
            ScrollableTextBlock t = new ScrollableTextBlock();
            t.Text = comment.text;
            t.SelectionEnabled = true;
            t.Margin = new Thickness(10, 0, 10, 0);

            //this.Do(post);

            this.MainContent.Children.Add(t);
            Debug.Assert(comment.from_id > 0);
            VKBaseDataForGroupOrUser u = UsersService.Instance.GetCachedUser((uint)comment.from_id);
            //если профиль удалёнён то photo_50 хотябы есть
            //bug: в пересылаемом сообщении пользователь, которого нет в кеше :(
            if (u != null)
            {
                string temp = u.MinPhoto;
                if (string.IsNullOrEmpty(temp))
                    temp = VKConstants.AVATAR_DEACTIVATED;
                
                this.img.ImageSource = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri(temp));//Source
                this.text.Text = u.Title;
            }
            if (comment.attachments != null)
            {
                AttachmentsPresenter ap = new AttachmentsPresenter();
                ap.Attachments = comment.attachments;
                ap.Margin = new Thickness(10, 0, 10, 0);
                this.MainContent.Children.Add(ap);
            }

            base.Tapped += delegate { NavigatorImpl.Instance.NavigateToWallPostComments(comment.owner_id,comment.post_id, comment.id); };
        }
        
        public ForwardedMessagesUC(VKWallPost post, bool forMsg = false)
            : this()
        {
            this.Do(post);
            //
            if (post.copy_history != null)
            {
                for (int i = 0; i < post.copy_history.Count; i++)
                {
                    VKWallPost j = post.copy_history[i];
                    //if (j.owner_id < 0 && groups != null)
                    //    j.Owner = groups.Find(ow => ow.id == (-j.owner_id));
                    //else
                    //    j.Owner = profiles.Find(ow => ow.id == j.owner_id);

                    if (post.attachments == null)
                        post.attachments = new List<VKAttachment>();
                    post.attachments.Add(new VKAttachment() { wall = j, type = VKAttachmentType.Wall });
                }
                //
                post.copy_history = null;
            }
            //
            AttachmentsPresenter ap = new AttachmentsPresenter();
            ap.ForceMediaPresenterMargin = true;
            ap.Text = post.text;
            ap.Attachments = post.attachments;
            
            //ap.Margin = new Thickness(10, 0, 0, 0);//almost
            this.MainContent.Children.Add(ap);

            if (forMsg)
            {
                Border brd = new Border();
                brd.Margin = new Thickness(10, 10, 10, 0);
                brd.CornerRadius = new CornerRadius(12);
                brd.Background = new SolidColorBrush(Windows.UI.Colors.Transparent);
                brd.BorderThickness = new Thickness(2);
                brd.BorderBrush = new SolidColorBrush(Windows.UI.Colors.CadetBlue);
                brd.Tapped += (s, e) =>
                {
                    NavigatorImpl.Instance.NavigateToWallPostComments(post.OwnerId, post.PostId);
                };
                TextBlock tb = new TextBlock() { Text = LocalizedStrings.GetString( "OpenPost") };
                tb.Padding = new Thickness(10,5,10,5);
                brd.Child = tb;

                this.MainContent.Children.Add(brd);
            }
        }

        public ForwardedMessagesUC(VKMessage msg, FrameworkElement uc, double diff)
            : this()
        {
            this.DataContext = msg;
            
            VKBaseDataForGroupOrUser u = null;
            if (msg.from_id > 0)
                u = UsersService.Instance.GetCachedUser((uint)msg.from_id);
            else
                u = GroupsService.Instance.GetCachedGroup((uint)-msg.from_id);
            //если профиль удалёнён то photo_50 хотябы есть
            //bug: в пересылаемом сообщении пользователь, которого нет в кеше :(
            Debug.Assert(u!=null);
            if (u != null)
            {
                this.img.ImageSource = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri(u.MinPhoto));//Source
                this.text.Text = u.Title;
            }
            
            AttachmentsPresenter ap = new AttachmentsPresenter() { IsMessage = true };
            ap.Text = msg.text;
            ap.Attachments = msg.attachments;
            //ap.Margin = new Thickness(10, 0, 0, 0);//NO
            this.MainContent.Children.Add(ap);

            if (msg.fwd_messages != null)
            {
                foreach (VKMessage m in msg.fwd_messages)
                {
                    ForwardedMessagesUC fwd = new ForwardedMessagesUC(m, uc, diff);
                    Thickness margin = fwd.Margin;
                    margin.Left = 10;
                    fwd.Margin = margin;
                    this.MainContent.Children.Add(fwd);
                }
            }

            //
            this._brd.Tapped += (s, e) =>
            {
                int id = u is VKGroup ? (-u.Id) : u.Id;
                NavigatorImpl.Instance.NavigateToProfilePage(id);
                e.Handled = true;
            };
            //
        }
        
        private void Do(VKWallPost post)
        {
            int owner = 0;

            if (post.from_id != 0)
                owner = post.from_id;
            else
                owner = post.owner_id;
           //
            this._brd.Tapped += (s, e) =>
            {
                Library.NavigatorImpl.Instance.NavigateToProfilePage(owner);
                e.Handled = true;
            };
            //
            
            if(post.Owner==null)
            {
                if (owner > 0)
                {
                    List<uint> userIds = new List<uint>() { (uint)owner };
                    UsersService.Instance.GetUsers(userIds, (result) =>
                    {
                        if (result != null)
                        {
                            post.Owner = result[0];

                            Execute.ExecuteOnUIThread(() => {
                                this.img.ImageSource = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri(post.Owner.MinPhoto));
                                this.text.Text = post.Owner.Title;
                            });
                        }

                    });
                }
                else
                {
                    var group = GroupsService.Instance.GetCachedGroup((uint)(-owner));
                    if (group == null)
                    {
                        GroupsService.Instance.GetCommunity((uint)(-owner), "photo_50", (result) => {
                            if (result != null)
                            {
                                post.Owner = result;

                                Execute.ExecuteOnUIThread(() =>
                                {
                                    this.img.ImageSource = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri(post.Owner.MinPhoto));
                                    this.text.Text = post.Owner.Title;
                                });

                            }
                        });
                    }
                    else
                    {
                        post.Owner = group;
                    }
                }
            }
            
            if(post.Owner!=null)
            {
                Execute.ExecuteOnUIThread(() => {
                    this.img.ImageSource = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri(post.Owner.MinPhoto));
                    this.text.Text = post.Owner.Title;
                });
            }
            
        }
    }
}
