using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using LunaVK.Core.Network;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using System.ComponentModel;
using LunaVK.Core.ViewModels;

namespace LunaVK.Core.DataObjects
{
    public class VKDialogsGetObject : VKCountedItemsObject<ConversationWithLastMsg>
    {
        /// <summary>
        /// число непрочитанных бесед. 
        /// </summary>
        public int unread_count { get; set; }
    }

    public class ConversationWithLastMsg : INotifyPropertyChanged
    {
        public VKConversation conversation { get; set; }
        public VKMessage last_message { get; set; }



        

        #region VM
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public LunaVK.Core.ViewModels.ConversationAvatarViewModel ConversationAvatarVM { get; set; }

        private string _uibody = "";
        public string UIBody
        {
            get { return this._uibody; }
            set
            {
                this._uibody = value;
                this.NotifyPropertyChanged("UIBody");
            }
        }

        private string _title = "";
        public string Title
        {
            get { return this._title; }
            set
            {
                this._title = value;
                this.NotifyPropertyChanged("Title");
            }
        }

        /// <summary>
        /// Показать ли фон для нового сообщения
        /// </summary>
        public bool IsAccentBrushVisible
        {
            get
            {
                if (this.last_message.@out == Enums.VKMessageType.Received && this.conversation.in_read != this.last_message.id)
                    return true;
                return false;
            }
        }

        public bool UserVerified
        {
            get
            {
                VKBaseDataForGroupOrUser u = Library.UsersService.Instance.GetCachedUser(this.last_message.from_id);
                if (u == null)//todo: bug
                {
                    return false;
                }
                return u.verified;
            }
        }

        public void UpdateUI()
        {
            this.NotifyPropertyChanged("AreNotificationsDisabled");
        }

        public bool AreNotificationsDisabled
        {
            get
            {
                if (this.conversation.push_settings != null)
                {
                    return this.conversation.push_settings.sound == false;
                }
                
                return false;
            }
        }

        /// <summary>
        /// Мини-аватарка слева от текста внизу
        /// </summary>
        public Visibility UserThumbVisibility
        {
            get
            {
                if (this.conversation.peer.type != "chat" && this.last_message.@out != Enums.VKMessageType.Sent || this.IsTypingVisible)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        private Visibility _typingVisibility = Visibility.Collapsed;
        public bool IsTypingVisible
        {
            get
            {
                return this._typingVisibility == Visibility.Visible;
            }
        }

        public bool IsTextBackgroundBrushVisible
        {
            get
            {
                if (this.conversation.out_read != this.last_message.id)
                    return true;
                return false;
            }
        }

        public Visibility CounterVisibility
        {
            get
            {
                //return this.conversation.in_read != this.last_message.id ? Visibility.Visible : Visibility.Collapsed;
                return this.conversation.unread_count > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public string Unread
        {
            get
            {
                if (this.conversation.unread_count > 99)
                    return "99+";
                return this.conversation.unread_count.ToString();
            }
        }

        public void RefreshUIProperties()
        {
//            this.NotifyPropertyChanged("MainBackgroundBrush");
//            this.NotifyPropertyChanged("TextBackgroundBrush");
            this.NotifyPropertyChanged("Unread");
            this.NotifyPropertyChanged("UserThumbVisibility");
            this.NotifyPropertyChanged("CounterVisibility");
            this.NotifyPropertyChanged("UserThumb");
            this.NotifyPropertyChanged("IsAccentBrushVisible");
            this.NotifyPropertyChanged("IsTextBackgroundBrushVisible");

        }

        private Visibility _isCurrentDialogVisibility = Visibility.Collapsed;
        public Visibility IsCurrentDialogVisibility
        {
            get { return this._isCurrentDialogVisibility; }
            set
            {
                this._isCurrentDialogVisibility = value;
                this.NotifyPropertyChanged("IsCurrentDialogVisibility");
            }
        }

        public string UserThumb
        {
            get
            {
                if (this.last_message.@out == Enums.VKMessageType.Sent)
                {
                    return Settings.LoggedInUserPhoto;
                }
                if (this.conversation.peer.type == "chat")
                {
                    VKBaseDataForGroupOrUser author = Library.UsersService.Instance.GetCachedUser(this.last_message.from_id);

                    if(author!=null)
                    {
                        //todo: искать в интернете
                        string str = author.photo_100;
                        if (string.IsNullOrEmpty(str))
                            str = author.photo_50;
                        return str;
                    }
                }
                return null;
            }
        }

        private string _typingStr;
        public string TypingStr
        {
            get
            {
                return this._typingStr;
            }
            set
            {
                this._typingStr = value;
                this.NotifyPropertyChanged("TypingStr");
            }
        }

        public SolidColorBrush HaveUnreadMessagesBackground
        {
            get
            {
                return this.AreNotificationsDisabled ? new SolidColorBrush(Windows.UI.Colors.Gray) : (SolidColorBrush)Application.Current.Resources["AccentBrushHigh"];
            }
        }

        public Visibility TypingVisibility
        {
            get
            {
                return this._typingVisibility;
            }
            set
            {
                this._typingVisibility = value;
                this.NotifyPropertyChanged("TypingVisibility");
                this.NotifyPropertyChanged("IsTypingVisible");
                this.NotifyPropertyChanged("UserThumbVisibility");
            }
        }

        DispatcherTimer dotsAnimationTimer;
        DispatcherTimer stopAnimationTimer;
        private int _currentDotsNumber = 0;
        private bool _timerInitialized = false;

        public void AnimTyping(long userId)
        {
            this.TypingVisibility = Visibility.Visible;

            if (!_timerInitialized)
            {
                this.dotsAnimationTimer = new DispatcherTimer();
                this.stopAnimationTimer = new DispatcherTimer();

                this.dotsAnimationTimer.Interval = TimeSpan.FromSeconds(0.25);
                this.stopAnimationTimer.Interval = TimeSpan.FromSeconds(5.0);

                this.dotsAnimationTimer.Tick += ((o, e) =>
                {
                    this._currentDotsNumber++;
                    if (this._currentDotsNumber > 3)
                    {
                        this._currentDotsNumber = 0;
                    }
                    else
                    {
                        string typingString = "";
                        if (this.conversation.peer.type == "chat")
                        {
                            VKBaseDataForGroupOrUser u = Library.UsersService.Instance.GetCachedUser(userId);
                            if (u != null)
                            {
                                typingString = (u.Title + " ");
                            }
                        }
                        typingString += LocalizedStrings.GetString("IsTyping");
                        for (int index = 0; index < this._currentDotsNumber; ++index)
                            typingString += ".";
                        this.TypingStr = typingString;
                    }
                });

                this.stopAnimationTimer.Tick += ((o, e) =>
                {
                    this.dotsAnimationTimer.Stop();
                    this._currentDotsNumber = 0;
                    this._typingStr = "";
                    this.TypingVisibility = Visibility.Collapsed;
                });

                _timerInitialized = true;
            }

            this.dotsAnimationTimer.Stop();
            this.stopAnimationTimer.Stop();
            this.dotsAnimationTimer.Start();
            this.stopAnimationTimer.Start();
        }

        public void StopAnimTyping()
        {
            if (this.dotsAnimationTimer != null)
                this.dotsAnimationTimer.Stop();

            if (this.stopAnimationTimer != null)
                this.stopAnimationTimer.Stop();

            this._currentDotsNumber = 0;
            this._typingStr = "";
            this.TypingVisibility = Visibility.Collapsed;
        }
#endregion
    }
}
