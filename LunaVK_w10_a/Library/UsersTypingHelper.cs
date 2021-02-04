using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Library;
using LunaVK.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace LunaVK.Library
{
    public sealed class UsersTypingHelper
    {
        public interface ISupportUsersTyping
        {
            bool IsChat { get; }
            void UpdateTypingInUI();
        }

        private readonly List<int> _typingNowUserIds = new List<int>();
        private readonly Dictionary<int, DispatcherTimer> _typingNowTimers = new Dictionary<int, DispatcherTimer>();
        private string _typingStringValue = "";//Кто-то печатает (без точек)
        private DispatcherTimer _dotsAnimationTimer;
        private byte _currentDotsNumber;
        private string _typingString;
        private ISupportUsersTyping _vm;

        public string TypingString
        {
            get
            {
                return this._typingString;
            }
            private set
            {
                this._typingString = value;
                this._vm.UpdateTypingInUI();
            }
        }

        private bool TypingVisibility;

        public bool AnyTypingNow
        {
            get
            {
                return this._typingNowUserIds.Any();
            }
        }

        public UsersTypingHelper(ISupportUsersTyping conversationHeader)
        {
            this._vm = conversationHeader;
        }

        public void SetUserIsTypingWithDelayedReset(int userId)
        {
            if (this._typingNowUserIds.Contains(userId))
                this.UpdateTypingState(userId, false);
            else
            {
                DispatcherTimer dispatcherTimer = new DispatcherTimer();
                dispatcherTimer.Interval = TimeSpan.FromSeconds(5.0);
                DispatcherTimer timer = dispatcherTimer;
                timer.Tick += (o, e) =>
                {
                    timer.Stop();
                    this.UpdateTypingState(userId, true);
                };
                timer.Start();
                this._typingNowUserIds.Insert(0, userId);
                this._typingNowTimers.Add(userId, timer);
                this.UpdateTypingView();
            }
        }

        private void UpdateTypingState(int userId, bool typingIsOver)
        {
            if (!typingIsOver)
            {
                DispatcherTimer typingNowTimer = this._typingNowTimers[userId];
                typingNowTimer.Stop();
                typingNowTimer.Start();
            }
            else
            {
                this._typingNowUserIds.Remove(userId);
                this._typingNowTimers.Remove(userId);
                this.UpdateTypingView();
            }
        }

        private void UpdateTypingView()
        {
            Core.Framework.Execute.ExecuteOnUIThread(() =>
            {
                if (!this.AnyTypingNow)
                {
                    this.TypingString = string.Empty;
                    this.TypingVisibility = false;
                    DispatcherTimer dotsAnimationTimer = this._dotsAnimationTimer;
                    if (dotsAnimationTimer == null)
                        return;
                    dotsAnimationTimer.Stop();
                }
                else
                {
                    
                    if (this._vm.IsChat)
                    {
                        var associatedUser = this.GetAssociatedUser(this._typingNowUserIds.First());
                        string str = associatedUser != null ? associatedUser.FirstNameLastNameShort : null;
                        if(this._typingNowUserIds.Count==1)
                        {
                            this._typingStringValue = string.Format("{0} {1}", str, LocalizedStrings.GetString("Conversation_IsTyping"));//Тест Т. печататет 
                        }
                        else if (this._typingNowUserIds.Count == 2)
                        {
                            var associatedUser2 = this.GetAssociatedUser(this._typingNowUserIds[1]);
                            string str2 = associatedUser2 != null ? associatedUser2.FirstNameLastNameShort : null;
                            this._typingStringValue = string.Format("{0}, {1} {2}", str, str2, LocalizedStrings.GetString("Conversation_IsTypingUsers"));//Тест Т., Кто-то печатают
                        }
                        else
                        {
                            this._typingStringValue = string.Format(LocalizedStrings.GetString("Conversation_FewUsersAreTypingFrm"), str, this._typingNowUserIds.Count - 1);//несколько людей печатают
                        }
                        //this._typingString = this._typingNowUserIds.Count != 1 ? string.Format(LocalizedStrings.GetString("Conversation_FewUsersAreTypingFrm"), str, this._typingNowUserIds.Count - 1) : string.Format(LocalizedStrings.GetString("Conversation_UserIsTypingFrm"), str);
                    }
                    else
                    {
                        this._typingStringValue = LocalizedStrings.GetString("Conversation_IsTyping");
                    }


                    if (this.TypingVisibility == false)
                    {
                        this.TypingVisibility = true;
                        DispatcherTimer dispatcherTimer = new DispatcherTimer();
                        dispatcherTimer.Interval = TimeSpan.FromSeconds(0.25);
                        this._dotsAnimationTimer = dispatcherTimer;
                        this._currentDotsNumber = 1;
                        this._dotsAnimationTimer.Tick += (o, e) =>
                        {
                            this._currentDotsNumber++;
                            if (this._currentDotsNumber > 3)
                            {
                                this._currentDotsNumber = 0;
                            }
                            else
                            {
                                string typingString = this._typingStringValue;
                                for (int index = 0; index < this._currentDotsNumber; ++index)
                                    typingString += ".";
                                this.TypingString = typingString;
                            }
                        };
                        this.TypingString = this._typingStringValue + ".";
                        this._dotsAnimationTimer.Start();
                    }
                    else
                    {
                        string typingString = this._typingStringValue;
                        for (int index = 0; index < this._currentDotsNumber; ++index)
                            typingString += ".";
                        this.TypingString = typingString;
                    }
                }
            });
        }

        private VKUser GetAssociatedUser(int id)
        {/*
            VKUser user;
            if (this._conversationHeader == null)
            {
                user = null;
            }
            else
            {
                List<VKUser> associatedUsers = this._conversationHeader._associatedUsers;
                if (associatedUsers == null)
                {
                    user = null;
                }
                else
                {
                    Func<VKUser, bool> predicate = (Func<User, bool>)(u => u.id == id);
                    user = associatedUsers.FirstOrDefault<User>(predicate);
                }
            }
            return user ?? (VKUser)UsersService.Instance.GetCachedUser(id);*/
            Debug.Assert(id > 0);
            return UsersService.Instance.GetCachedUser((uint)id);
        }

        public void SetUserIsNotTyping(int userId)
        {
            if (!this._typingNowUserIds.Contains(userId))
                return;
            this._typingNowTimers[userId].Stop();
            this.UpdateTypingState(userId, true);
        }
    }
}
