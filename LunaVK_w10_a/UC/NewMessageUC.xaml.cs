using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Windows.UI.ViewManagement;
using System.Collections.ObjectModel;
using LunaVK.Core.Utils;
using Windows.Media.Capture;
using LunaVK.Core.Enums;

using Windows.UI.Core;
using Windows.System;
using LunaVK.Core.Emoji;
using LunaVK.Core.Library;
using LunaVK.ViewModels;
using LunaVK.Core.Framework;
using LunaVK.Core;
using LunaVK.Core.DataObjects;
using Windows.UI.Xaml.Media.Animation;
using LunaVK.Framework;



using Windows.UI.Core.AnimationMetrics;
using System.Diagnostics;
using LunaVK.Common;
using System.Linq;
using System.Threading.Tasks;

namespace LunaVK.UC
{
    public sealed partial class NewMessageUC : UserControl
    {
        private bool IsFocused = false;
        private bool _panelInitialized;
        private bool _isEmojiOpened;
        private DispatcherTimer _auioRecordHoldTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(200.0) };
        public EventHandler<double> IsOpenedChanged;
        DispatcherTimer timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(500) };

        private bool _isVoiceMessageButtonEnabled;
        public bool IsVoiceMessageButtonEnabled
        {
            get { return this._isVoiceMessageButtonEnabled; }
            set
            {
                this._isVoiceMessageButtonEnabled = value;
                this.UpdateVoiceMessageAvailability();
            }
        }

        /// <summary>
        /// Нужная высота панели
        /// </summary>
        private double _trueKeyboardHeight = 347;

        /// <summary>
        /// Высота клавиатуры, которую мы узнаем после её открытии
        /// Однако, если клавиатура отсоединена, то высота получается 0
        /// </summary>
        private double _height_keyboard = 0;

        private bool _canRecordVoiceMessage;
        
        

        /// <summary>
        /// Событие нажатия кнопки "отправить сообщение"
        /// </summary>
        public Action OnSendTap;

        public Action<string,string> OnSendPayloadTap;

        public Action ReplyNameTapped;
        
        public bool IsFromGroupChecked { get; private set; }

        /// <summary>
        /// Событие нажатия кнопки "прикрепить файл"
        /// </summary>
        public Action<FrameworkElement> OnAddAttachTap { get; set; }

        public Action<IOutboundAttachment> OnImageDeleteTap { get; set; }

        public AudioRecorderUC AudioRecorder
        {
            get { return this.ucAudioRecorder; }
        }
        
        public NewMessageUC()
        {
            this.InitializeComponent();
            this.SetAdminLevel(0);
            this._borderSend.Tapped += _borderSend_Tapped;

            this.textBoxPost.Paste += textBoxPost_Paste;
            
            this.textBoxPost.GotFocus += TextBoxPost_GotFocus;
            this.textBoxPost.LostFocus += TextBoxPost_LostFocus;            
            InputPane.GetForCurrentView().Showing += Keyboard_Showing;
            InputPane.GetForCurrentView().Hiding += Keyboard_Hiding;

            this.Unloaded += NewMessageUC_Unloaded;

            this._auioRecordHoldTimer.Tick += _auioRecordHoldTimer_Tick;
            timer.Tick += Timer_Tick;

            this.UpdateAutoSuggestVisibility();

            if (Settings.StickersAutoSuggestEnabled)
            {
                this.textBoxPost.TextChanged += TextBoxPost_TextChanged;
                this.ucStickersAutoSuggest.StickerTapped += UcStickersAutoSuggest_StickerTapped;
            }

            this._botKeyboard.SizeChanged += this._botKeyboard_SizeChanged;
        }

        private void _botKeyboard_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if(e.NewSize.Height != e.PreviousSize.Height)
                this.UpdateVisibilityState();
        }

        private void UcStickersAutoSuggest_StickerTapped(object sender, VKSticker e)
        {
            this.StickerTapped?.Invoke(sender, e);
            this.textBoxPost.Text = String.Empty;
            this.ForceFocusIfNeeded();
        }

        private void TextBoxPost_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.UpdateAutoSuggest(false);
        }
        /*
        private void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            if (e.Handled == true)
                return;

            e.Handled = HidePanel();
            int i = 0;
        }
        */
        async void textBoxPost_Paste(object sender, TextControlPasteEventArgs e)
        {
            var view = dataPackage.GetView();
            if (view.Contains("Text"))
            {
                e.Handled = true;

                string s = await view.GetTextAsync();

                TextBox tb = sender as TextBox;
                tb.Text += s;

                dataPackage = new Windows.ApplicationModel.DataTransfer.DataPackage();
            }
        }

        Windows.ApplicationModel.DataTransfer.DataPackage dataPackage = new Windows.ApplicationModel.DataTransfer.DataPackage();

        public void SetClipboard(string s)
        {
            dataPackage.SetText(s);
        }
       
        private int _adminLevel;
        private bool HaveRightsToPostOnBehalfOfCommunity
        {
            get { return this._adminLevel > 1; }
        }

        public void SetAdminLevel(int adminLevel)
        {
            this._adminLevel = adminLevel;
            if (this.HaveRightsToPostOnBehalfOfCommunity)
            {
                this.panelReply.Visibility = Visibility.Visible;
                this.checkBoxAsCommunity.Visibility = Visibility.Visible;
                this.textBlockReply.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.checkBoxAsCommunity.Visibility = Visibility.Collapsed;
                this.textBlockReply.Visibility = Visibility.Visible;
                if (string.IsNullOrEmpty(this.textBlockTitle.Text))
                {
                    this.panelReply.Visibility = Visibility.Collapsed;
                    this.ucReplyUser.Visibility = Visibility.Collapsed;
                }
            }
        }

        public string ReplyToUserName
        {
            get
            {
                return this.textBlockTitle.Text;
            }
            set
            {
                this.textBlockTitle.Text = value;
                if (!string.IsNullOrEmpty(value))
                {
                    //Имя есть!
                    this.ucReplyUser.Visibility = Visibility.Visible;
                    this.panelReply.Visibility = Visibility.Visible;
                    this.textBlockReply.Visibility = Visibility.Visible;//(this.HaveRightsToPostOnBehalfOfCommunity ? Visibility.Collapsed : Visibility.Visible);
                }
                else
                {
                    this.ucReplyUser.Visibility = Visibility.Collapsed;

                    if (!this.HaveRightsToPostOnBehalfOfCommunity)
                    {
                        this.panelReply.Visibility = Visibility.Collapsed;
                        this.textBlockReply.Visibility = Visibility.Visible;
                    }
                    else
                        this.textBlockReply.Visibility = Visibility.Collapsed;
                }
            }
        }

        void NewMessageUC_Unloaded(object sender, RoutedEventArgs e)
        {
            this.textBoxPost.GotFocus -= TextBoxPost_GotFocus;
            this.textBoxPost.LostFocus -= TextBoxPost_LostFocus;
            InputPane.GetForCurrentView().Showing -= Keyboard_Showing;
            InputPane.GetForCurrentView().Hiding -= Keyboard_Hiding;
        }

        

        public enum Mode : byte
        {
            NotInitialized,
            NewMessageEmpty,
            NewMessageReadyToSend,
            VoiceRecord,
            EditMessage
        }

        private Mode _controlMode;
        public Mode ControlMode
        {
            get
            {
                return this._controlMode;
            }
            set
            {
                this._borderVoice.IsHitTestVisible = value == Mode.VoiceRecord;
                this._controlMode = value;
                this.UpdateIcon();
            }
        }

        private void UpdateIcon()
        {
            switch (this._controlMode)
            {
                case Mode.NewMessageReadyToSend:
                case Mode.NewMessageEmpty:
                    {
                        this._cIcon.Glyph = "\xE725";
                        break;
                    }
                case Mode.VoiceRecord:
                    {
                        this._cIcon.Glyph = "\xE720";
                        break;
                    }
                case Mode.EditMessage:
                    {
                        this._cIcon.Glyph = "\xE73E";
                        break;
                    }
            }
        }

        private DialogHistoryViewModel VM
        {
            get { return base.DataContext as DialogHistoryViewModel; }
        }

        public void UpdateVoiceMessageAvailability()
        {
            Task.Run( async () =>// делаем на фоновой ветке, чтобы убрать фризы интерфейса
            {
                this._canRecordVoiceMessage = false;

                if (this.IsVoiceMessageButtonEnabled)
                {
                    this._canRecordVoiceMessage = await this.ucAudioRecorder.CanRecord();
                }

                Execute.ExecuteOnUIThread(() =>
                { 
                    bool attachExists = this.itemsControlAttachments.Items.Count > 0;

                    if (attachExists || !string.IsNullOrEmpty(this.textBoxPost.Text))
                    {
                        this.ControlMode = Mode.NewMessageReadyToSend;
                    }
                    else if (this._canRecordVoiceMessage)
                    {
                        this.ControlMode = Mode.VoiceRecord;
                    }
                    else
                    {
                        this.ControlMode = Mode.NewMessageEmpty;
                    }
                });
            });
            
        }

        /// <summary>
        /// Скрывает панель со стикерами
        /// </summary>
        /// <returns>Возвращает Тру если панель была открыта</returns>
        public bool HidePanel()
        {
            if (this._isEmojiOpened)
            {
                this._isEmojiOpened = false;
                this.UpdateVisibilityState();
                return true;
            }

            return false;
        }
                
        void TextBoxPost_LostFocus(object sender, RoutedEventArgs e)
        {
#if DEBUG
            //System.Diagnostics.Debug.WriteLine("TextBoxPost_LostFocus");
#endif
            this.IsFocused = false;

            this.timer.Start();//this.UpdateVisibilityState();

            this.UpdateAutoSuggest();
        }
        

        /// <summary>
        /// Сначала происходит фркус, а потом показ клавиатуры
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TextBoxPost_GotFocus(object sender, RoutedEventArgs e)
        {
#if DEBUG

            //System.Diagnostics.Debug.WriteLine("TextBoxPost_GotFocus");
#endif
            this.IsFocused = true;
            this._isEmojiOpened = false;

            this.timer.Start();//this.UpdateVisibilityState();
            this.UpdateAutoSuggest();
        }

        //Stopwatch sw = new Stopwatch();

        private void Timer_Tick(object sender, object e)
        {
#if DEBUG
            //System.Diagnostics.Debug.WriteLine("Timer_Tick");
#endif
            this.timer.Stop();
            this.UpdateVisibilityState();
        }

        private void Keyboard_Hiding(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            if (this.timer.IsEnabled)
                this.timer.Stop();

            this._height_keyboard = 0;
            this.UpdateVisibilityState();
        }
        
        private void Keyboard_Showing(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            if ((Window.Current.Content as CustomFrame).OverlayGrid.Children.Count > 0)
            {
                //BugFix: мы в комментах, нажали на репост
                return;
            }

            if (this.IsFocused == false)
                return;//BugFix:откуда это гавно лезит если мы не в фокусе?

            args.EnsuredFocusedElementInView = true;

            //System.Diagnostics.Debug.WriteLine("Keyboard_Showing trY:" + trNewMessage.Y + "   h:" + sender.OccludedRect.Height);

            //347,727294921875 И БЫВАЕТ 300
            if (this._height_keyboard < sender.OccludedRect.Height)
                this._height_keyboard = sender.OccludedRect.Height;

            if (this._height_keyboard > 0 && this._height_keyboard > this._trueKeyboardHeight)
                this._trueKeyboardHeight = this._height_keyboard;

            this.panelControl.Height = this._height_keyboard;

            

            if (this._height_keyboard != 0 && this._height_keyboard > sender.OccludedRect.Height)
                return;

            

            if (this.timer.IsEnabled)
                this.timer.Stop();

            this._isEmojiOpened = false;

            this.UpdateVisibilityState();
        }
        
        public event EventHandler<VKSticker> StickerTapped;

        private void Smiles_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if(Settings.StickerPanelAsPopup)
            {
                PopUpService pop = new PopUpService();
                StickersPanel panel = new StickersPanel();
                panel.MaxWidth = 600;
                panel.VerticalAlignment = VerticalAlignment.Bottom;
                panel.Margin = new Thickness(40,0,10,60);
                panel.StickerTapped += (s,vm) => {
                    this.StickerTapped?.Invoke(s, vm);
                };
                panel.Closed += (s, a) => {
                    pop.Hide();
                };
                pop.Closed += (s, a) => {
                    panel.StickerTapped -= this.StickerTapped;
                };
                //todo: полностью удалять, а то оно реагирует на кнопку назад
                pop.BackgroundBrush = new SolidColorBrush(Windows.UI.Colors.Transparent);
                pop.OverrideBackKey = true;
                pop.Child = panel;
                pop.Show();
            }
            else
            {
                this._isEmojiOpened = !this._isEmojiOpened;
                this.InitPanel();

                if (this._isEmojiOpened == false)//cpc
                    this.textBoxPost.Focus(FocusState.Keyboard);//cpc
                else
                    this.UpdateVisibilityState();
            }
        }
        
        /// <summary>
        /// И так, показываем панель если поле ввода в фокусе или эмоджи открыты
        /// нажатие на смайли
        /// </summary>
        public void UpdateVisibilityState()
        {
            //System.Diagnostics.Debug.WriteLine(string.Format("{0} {1} {2}", this._isShowingBotKeyboard, this.IsFocused, this._isEmojiOpened));
            //System.Diagnostics.Debug.WriteLine(string.Format("{0} {1} {2}", this.panelControl.Height, this._botKeyboard.ActualHeight, this._height_keyboard));

            bool needOpen = this.IsFocused || this._isEmojiOpened || (this._isShowingBotKeyboard && this._botKeyboard.ActualHeight > 0) || this._height_keyboard > 0;

            if (this.IsFocused && this._botKeyboard.ActualHeight > 0 && this._height_keyboard == 0)
                needOpen = false;
            //if (!Settings.StickerPanelAsPopup)
            //{
            this._botKeyboard.Visibility = (this._isEmojiOpened == false) ? Visibility.Visible : Visibility.Collapsed;
            //this.IsOpenedChanged?.Invoke(this, this.panelControl.Height);

            if (needOpen)
            {
                double value = this.panelControl.Height;

                if (this._isEmojiOpened)
                    value = 0;
                else if (this._height_keyboard > 0)
                {
                    value = this._botKeyboard.ActualHeight;
                }
                else if(this._botKeyboard.ActualHeight > 0 )
                {
                    if (this._isShowingBotKeyboard)
                        value = this.panelControl.Height;
                    //else
                    

                }

                //
                //

                if (CustomFrame.Instance.BottomPlayer.Visibility == Visibility.Visible && this._height_keyboard > 0)
                    value = CustomFrame.Instance.BottomPlayer.ActualHeight;
                //
                //

                //System.Diagnostics.Debug.WriteLine("open: "+ value);
                this.ShowingMoveSpline.Value = value;
                //this.MoveMiddleOnShowing.Stop();
                this.MoveMiddleOnShowing.Begin();







                if (this._isEmojiOpened)
                {
                    value = this.panelControl.Height;
                }
                else if (this._height_keyboard > 0)
                {
                    value = this._height_keyboard;
                }
                else
                {
                    if (this.VM!=null && this.VM.BotKeyboardButtons.Count > 0 && this._botKeyboard.ActualHeight == 0)//сразу после загрузки высота равна нулю
                        value = this.VM.BotKeyboardButtons.Count * 44;
                    else
                        value = this._botKeyboard.ActualHeight;
                }
                
                //System.Diagnostics.Debug.WriteLine("open: "+ value);
                this.IsOpenedChanged?.Invoke(this, value);
            }
            else
            {
                
                if (this._botKeyboard.ActualHeight > 0)
                {
                    this.HidingMoveSpline.Value = this.panelControl.Height + this._botKeyboard.ActualHeight;
                }
                else
                {
                    this.HidingMoveSpline.Value = this.panelControl.Height;
                }

               // System.Diagnostics.Debug.WriteLine("hide: " + this.HidingMoveSpline.Value);

                //System.Diagnostics.Debug.WriteLine("hide: " + 0);
                this.IsOpenedChanged?.Invoke(this, 0);
                //this.MoveMiddleOnShowing.Stop();
                this.MoveMiddleOnHiding.Begin();
            }
            //}

            this._stickerIcon.Glyph = this._isEmojiOpened ? "\xE765" : "\xED54";
            this.UpdateAutoSuggest();
        }

        public void ClosePanel()
        {
            this.IsFocused = false;
            this._isEmojiOpened = false;
            this.UpdateVisibilityState();
        }

        /*
        /// <summary>
        /// Панель со стикерами
        /// </summary>
        public SwipeThroughControl PanelControl
        {
            get { return this.panelControl; }
        }
        */
        void _borderSend_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (this._controlMode == Mode.VoiceRecord)
                return;
            
            this.ForceFocusIfNeeded();

            if ((this._controlMode == Mode.EditMessage || this._controlMode == Mode.NewMessageReadyToSend) && this.OnSendTap != null)
                this.OnSendTap();
            //e.Handled = true;
        }

        public void ForceFocusIfNeeded()
        {
            if (!this.IsFocused)
                return;

            this.textBoxPost.Focus(FocusState.Programmatic);
        }

        public TextBox TextBoxNewComment
        {
            get { return this.textBoxPost; }
        }

        /// <summary>
        /// Вызывается из вне
        /// </summary>
        /// <param name="status">тру, если есть вложения или текст</param>
        public void ActivateSendButton(bool status)
        {
            VisualStateManager.GoToState(this, status ? "Active" : "Idle", true);

            if (this.ControlMode == Mode.EditMessage)
            {
                return;
            }

            if (status)
            {
                this.ControlMode = Mode.NewMessageReadyToSend;
            }
            else
            {
                if (this._canRecordVoiceMessage)
                    this.ControlMode = Mode.VoiceRecord;
                else
                    this.ControlMode = Mode.NewMessageEmpty;
            }
        }




        /// <summary>
        /// Получаем стикеры и добавляем их в коллекцию
        /// </summary>
        private void InitPanel()
        {
            //bug: если быстро нажимать, загрузится список дважды
            if (this._panelInitialized)
                return;

            //StoreProduct storeProduct = null;
            //if (keepPosition)
            //    storeProduct = this.GetCurrentSelectedProduct();

            this.progBar.IsIndeterminate = true;

            List<StoreProductFilter> l = new List<StoreProductFilter>() { StoreProductFilter.Active };

            this.panelControl.DataContext = null;
            this.panelControl.Items = new ObservableCollection<object>();
            //List<SpriteListItemData> spriteListItemDataList1 = new List<SpriteListItemData>();
            //spriteListItemDataList1.Add(new SpriteListItemData() { IsStore = true });
            //spriteListItemDataList1.Add(new SpriteListItemData() { IsEmoji = true });
            //            spriteListItemDataList1.Add(new SpriteListItemData() { IsRecentStickers = true });
            //spriteListItemDataList1.Add(new SpriteListItemData() { IsSettings = true });
            //
            var emoji = new SpriteListItemData() { IsEmoji = true };
            this.panelControl.Items.Add(emoji);

            //
            StoreService.Instance.GetStockItems(l, (result) =>
            {
                if (result != null && result.error.error_code == VKErrors.None)
                {
                    Execute.ExecuteOnUIThread(() =>
                    {
                        foreach(var item in result.response.items)
                        {
                            //spriteListItemDataList1.Add(new SpriteListItemData() { StickerStockItemHeader = item });
                            this.panelControl.Items.Add(new SpriteListItemData() { StickerStockItemHeader = item });
                        }

                        //this.panelControl.Items = new ObservableCollection<object>(spriteListItemDataList1);
                        
                        //this.panelControl.FooterItems = new ObservableCollection<object>(spriteListItemDataList1);
                    });
                    //this.panelControl.SelectedIndex = 1;
                    this._panelInitialized = true;
                }
                //todo: error
                Execute.ExecuteOnUIThread(() =>
                {
                    this.progBar.IsIndeterminate = false;
                });
            });


            
            
        }
        
        

        

        public ListView MentionPicker
        {
            get { return this.mentionPicker; }
        }

        private void ucReplyUser_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.ReplyNameTapped?.Invoke();
        }

        private void textBoxPost_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            /*
            if (e.Key == VirtualKey.Enter)
            {
                if (Settings.SendByEnter)
                {
                    this.OnSendTap?.Invoke();
                }
                else
                {
                    if ((Window.Current.CoreWindow.GetKeyState(VirtualKey.Control) & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down)
                    {
                        e.Handled = true;
                        this.OnSendTap?.Invoke();
                    }
                }
            }
            */
        }

        private void TextBoxPost_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            
            bool enter = (Window.Current.CoreWindow.GetKeyState(VirtualKey.Enter) & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
            bool ctrl = (Window.Current.CoreWindow.GetKeyState(VirtualKey.Control) & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
            
            if (Settings.SendByEnter)
            {
                if(enter && !ctrl)
                    this.OnSendTap?.Invoke();
            }
            else
            {
                if (enter && ctrl)
                    this.OnSendTap?.Invoke();
            }
            
        }

        private void checkBoxAsCommunity_Checked(object sender, RoutedEventArgs e)
        {
            this.IsFromGroupChecked = (sender as CheckBox).IsChecked.Value;
        }

        //mentionPicker_OnItemSelected
        private void Mention_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var vm = (sender as FrameworkElement).DataContext as VKBaseDataForGroupOrUser;
            this.textBoxPost.Text += this.GetUserMention(vm.Id, vm.Title);
        }

        private string GetUserMention(long id, string name)
        {
            return string.Format("id{0} ({1})", id, name);
            //return string.Format("[id{0}|{1}]", id, name);
        }

        private void AddAttachTapped(object sender, TappedRoutedEventArgs e)
        {
            this.OnAddAttachTap?.Invoke(sender as FrameworkElement);
        }
        
        private void _borderVoice_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            this.ucAudioRecorder.HandleManipulationDelta(e);
        }

        private void _borderVoice_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            this.ForceFocusIfNeeded();
            this.ucAudioRecorder.HandleManipulationCompleted();
        }

        private void _borderVoice_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            this._auioRecordHoldTimer.Start();
        }

        private void _borderVoice_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            //мы отжали кнопку мыши

            if (this._auioRecordHoldTimer.IsEnabled)//если таймер ещё включен, значит записываем как для компьютера
            {
                this._auioRecordHoldTimer.Stop();
                //this.ShowHoldToRecord();
                this.ucAudioRecorder.IsHoldMode = false;
                this.ucAudioRecorder.IsOpened = true;
            }
            else
            {
                if (this.ucAudioRecorder.InRecord && this.ucAudioRecorder.IsOpened)
                    this.ucAudioRecorder.HandleManipulationCompleted();
            }
        }

        private void _auioRecordHoldTimer_Tick(object sender, object e)
        {
            this._auioRecordHoldTimer.Stop();
            this.ucAudioRecorder.IsHoldMode = true;
            this.ucAudioRecorder.HandleManipulationStarted();
        }

        private string _lastKeyForAutoSuggest;

        /// <summary>
        /// Проверяем текст на наличие подсказки к стикерам
        /// </summary>
        /// <param name="force"></param>
        private void UpdateAutoSuggest(bool force = false)
        {
            string text = this.TextBoxNewComment.Text;
            //if (!string.IsNullOrEmpty(this._replyAutoForm) && text.StartsWith(this._replyAutoForm))
            //    text = text.Substring(this._replyAutoForm.Length);
            string str = StickersAutoSuggestDictionary.Instance.PrepareTextForLookup(text);
            if (str != this._lastKeyForAutoSuggest || force)
            {
                this.ucStickersAutoSuggest.SetData(StickersAutoSuggestDictionary.Instance.GetAutoSuggestItemsFor(str), str);
                this._lastKeyForAutoSuggest = str;
            }
            this.UpdateAutoSuggestVisibility();
        }

        private void UpdateAutoSuggestVisibility()
        {
            this.ucStickersAutoSuggest.ShowHide((this.textBoxPost.FocusState != FocusState.Unfocused || this._isEmojiOpened) && this.ucStickersAutoSuggest.HasItemsToShow);
        }
        
        private void Attachment_Loaded(object sender, RoutedEventArgs e)
        {//rubberBand
            FrameworkElement element = sender as FrameworkElement;

            CompositeTransform scaleTransform = element.RenderTransform as CompositeTransform;

            scaleTransform.ScaleX = scaleTransform.ScaleY = 0;
            scaleTransform.CenterX = element.ActualWidth / 2.0;
            scaleTransform.CenterY = element.ActualHeight / 2.0;

            ElasticEase ease = new ElasticEase() { Oscillations = 2, Springiness = 8, EasingMode = EasingMode.EaseOut };

            List<AnimationUtils.AnimationInfo> animInfoList = new List<AnimationUtils.AnimationInfo>();
            animInfoList.Add(new AnimationUtils.AnimationInfo()
            {
                target = scaleTransform,
                propertyPath = "ScaleX",
                from = scaleTransform.ScaleX,
                to = 1,
                duration = 1000,
                easing = ease
            });
            animInfoList.Add(new AnimationUtils.AnimationInfo()
            {
                target = scaleTransform,
                propertyPath = "ScaleY",
                from = scaleTransform.ScaleY,
                to = 1,
                duration = 1000,
                easing = ease
            });
            AnimationUtils.AnimateSeveral(animInfoList);
        }

        private void Delete_Tapped(object sender, TappedRoutedEventArgs e)
        {
            IOutboundAttachment a = (sender as FrameworkElement).DataContext as IOutboundAttachment;

            



            FrameworkElement elementBorder = sender as FrameworkElement;
            FrameworkElement element = elementBorder.Parent as FrameworkElement;
            element.IsHitTestVisible = false;

            CompositeTransform scaleTransform = element.RenderTransform as CompositeTransform;

            //scaleTransform.ScaleX = scaleTransform.ScaleY = 0;
            scaleTransform.CenterX = element.ActualWidth / 2.0;
            scaleTransform.CenterY = element.ActualHeight / 2.0;

            //ElasticEase ease = new ElasticEase();
            //ease.Oscillations = 2;
            //ease.Springiness = 10;
            //ease.EasingMode = EasingMode.EaseOut;



            List<AnimationUtils.AnimationInfo> animInfoList = new List<AnimationUtils.AnimationInfo>();
            animInfoList.Add(new AnimationUtils.AnimationInfo()
            {
                target = scaleTransform,
                propertyPath = "ScaleX",
                from = scaleTransform.ScaleX,
                to = 0,
                duration = 200,
                //easing = ease
            });
            animInfoList.Add(new AnimationUtils.AnimationInfo()
            {
                target = scaleTransform,
                propertyPath = "ScaleY",
                from = scaleTransform.ScaleY,
                to = 0,
                duration = 200,
                //easing = ease
            });
            animInfoList.Add(new AnimationUtils.AnimationInfo()
            {
                target = scaleTransform,
                propertyPath = "Rotation",
                from = scaleTransform.Rotation,
                to = 90,
                duration = 200,
                //easing = ease
            });
            animInfoList.Add(new AnimationUtils.AnimationInfo()
            {
                target = element,
                propertyPath = "Opacity",
                from = element.Opacity,
                to = 0,
                duration = 150,
                //easing = ease
            });
            AnimationUtils.AnimateSeveral(animInfoList,null,()=> {
                this.OnImageDeleteTap?.Invoke(a);
            });

        }

        bool _isShowingBotKeyboard = true;

        private void BotKeyboard_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.IsFocused = false;
            
            this._isShowingBotKeyboard = !this._isShowingBotKeyboard;
            if (this._isEmojiOpened)
                this._isShowingBotKeyboard = true;

            this._isEmojiOpened = false;

            this.UpdateVisibilityState();
        }

        private void BotKeyboardButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var vm = (sender as FrameworkElement).DataContext as VKBotKeyboard.KeyboardButton;
            //this.textBoxPost.Text = vm.action.label;
            //this.OnSendTap();
            this.OnSendPayloadTap(vm.action.label,vm.action.payload);
        }

        private void SpriteListControl_ItemClick(object sender, RoutedEventArgs e)
        {
            var vm = (sender as FrameworkElement).DataContext as VKSticker;
            this.StickerTapped?.Invoke(sender, vm);
        }

        public ItemsControl ItemsControlAttachments
        {
            get { return this.itemsControlAttachments; }
        }

        private void Image_Tap(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            var vm = (sender as FrameworkElement).DataContext as IOutboundAttachment;
            if(vm.UploadState!= OutboundAttachmentUploadState.Uploading)
                vm.Upload(null);
        }

        private void SpriteListControl_EmojiClick(object sender, RoutedEventArgs e)
        {
            var vm = (sender as FrameworkElement).DataContext as LunaVK.UC.EmojiControlUC.EmojiControlViewModel.EmojiDataItem;
            int selectionStart = this.textBoxPost.SelectionStart;
            this.textBoxPost.Text = (this.textBoxPost.Text.Insert(selectionStart, vm.ElementCode));
            this.textBoxPost.Select(selectionStart + vm.ElementCode.Length, 0);
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









        /*


        /// <summary>
        /// Retrieves the specified metrics and displays them in textual form.
        /// </summary>
        /// <param name="effect">The AnimationEffect whose metrics are to be displayed.</param>
        /// <param name="target">The AnimationEffecTarget whose metrics are to be displayed.</param>
        private void DisplayMetrics(AnimationEffect effect, AnimationEffectTarget target)
        {
            var s = new System.Text.StringBuilder();
            AnimationDescription animationDescription = new AnimationDescription(effect, target);
            s.AppendFormat("Stagger delay = {0}ms", animationDescription.StaggerDelay.TotalMilliseconds);
            s.AppendLine();
            s.AppendFormat("Stagger delay factor = {0}", animationDescription.StaggerDelayFactor);
            s.AppendLine();
            s.AppendFormat("Delay limit = {0}ms", animationDescription.DelayLimit.TotalMilliseconds);
            s.AppendLine();
            s.AppendFormat("ZOrder = {0}", animationDescription.ZOrder);
            s.AppendLine();
            s.AppendLine();

            int animationIndex = 0;
            foreach (var animation in animationDescription.Animations)
            {
                s.AppendFormat("Animation #{0}:", ++animationIndex);
                s.AppendLine();

                switch (animation.Type)
                {
                    case PropertyAnimationType.Scale:
                        {
                            ScaleAnimation scale = animation as ScaleAnimation;
                            s.AppendLine("Type = Scale");
                            if (scale.InitialScaleX.HasValue)
                            {
                                s.AppendFormat("InitialScaleX = {0}", scale.InitialScaleX.Value);
                                s.AppendLine();
                            }
                            if (scale.InitialScaleY.HasValue)
                            {
                                s.AppendFormat("InitialScaleY = {0}", scale.InitialScaleY.Value);
                                s.AppendLine();
                            }
                            s.AppendFormat("FinalScaleX = {0}", scale.FinalScaleX);
                            s.AppendLine();
                            s.AppendFormat("FinalScaleY = {0}", scale.FinalScaleY);
                            s.AppendLine();
                            s.AppendFormat("Origin = {0}, {1}", scale.NormalizedOrigin.X, scale.NormalizedOrigin.Y);
                            s.AppendLine();
                        }
                        break;
                    case PropertyAnimationType.Translation:
                        s.AppendLine("Type = Translation");
                        break;
                    case PropertyAnimationType.Opacity:
                        {
                            OpacityAnimation opacity = animation as OpacityAnimation;
                            s.AppendLine("Type = Opacity");
                            if (opacity.InitialOpacity.HasValue)
                            {
                                s.AppendFormat("InitialOpacity = {0}", opacity.InitialOpacity.Value);
                                s.AppendLine();
                            }
                            s.AppendFormat("FinalOpacity = {0}", opacity.FinalOpacity);
                            s.AppendLine();
                        }
                        break;
                }

                s.AppendFormat("Delay = {0}ms", animation.Delay.TotalMilliseconds);
                s.AppendLine();
                s.AppendFormat("Duration = {0}ms", animation.Duration.TotalMilliseconds);
                s.AppendLine();
                s.AppendFormat("Cubic Bezier control points");
                s.AppendLine();
                s.AppendFormat("    X1 = {0}, Y1 = {1}", animation.Control1.X, animation.Control1.Y);
                s.AppendLine();
                s.AppendFormat("    X2 = {0}, Y2 = {1}", animation.Control2.X, animation.Control2.Y);
                s.AppendLine();
                s.AppendLine();
            }

            Metrics.Text = s.ToString();
        }
        */
    }
}
