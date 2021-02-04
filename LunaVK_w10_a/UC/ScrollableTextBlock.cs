using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;
using LunaVK.Core.Utils;
using LunaVK.Core;
using LunaVK.Common;
using System.Net;

namespace LunaVK.UC
{
    //NewsTextItem
    //BrowserNavigationService
    public class ScrollableTextBlock : StackPanel
    {
#region Text
        public string Text
        {
            get { return (string)base.GetValue(ScrollableTextBlock.TextProperty); }
            set { base.SetValue(ScrollableTextBlock.TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(ScrollableTextBlock), new PropertyMetadata("", new PropertyChangedCallback(ScrollableTextBlock.OnTextPropertyChanged)));

        private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //((ScrollableTextBlock)d).ParseText((string)e.NewValue, false);
            ((ScrollableTextBlock)d).OnTextChanged((string)e.NewValue, false);
        }
#endregion

#region Foreground
        public Brush Foreground
        {
            get { return (Brush)base.GetValue(ScrollableTextBlock.BrushProperty); }
            set { base.SetValue(ScrollableTextBlock.BrushProperty, value); }
        }

        public static readonly DependencyProperty BrushProperty = DependencyProperty.Register(nameof(Foreground), typeof(Brush), typeof(ScrollableTextBlock), new PropertyMetadata(null, new PropertyChangedCallback(ScrollableTextBlock.OnForegroundPropertyChanged)));

        private static void OnForegroundPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (((ScrollableTextBlock)d).Children.Count == 0)
                return;

            (((ScrollableTextBlock)d).Children[0] as RichTextBlock).Foreground = (Brush)e.NewValue;
        }
#endregion

        #region FullOnly
        public bool FullOnly
        {
            get { return (bool)GetValue(FullProperty); }
            set { base.SetValue(FullProperty, value); }
        }

        public static readonly DependencyProperty FullProperty = DependencyProperty.Register("FullOnly", typeof(bool), typeof(ScrollableTextBlock), new PropertyMetadata(null));
#endregion

#region DisableHyperlinks
        public bool DisableHyperlinks
        {
            get { return (bool)GetValue(DisableHyperlinksProperty); }
            set { base.SetValue(DisableHyperlinksProperty, value); }
        }

        public static readonly DependencyProperty DisableHyperlinksProperty = DependencyProperty.Register("DisableHyperlinks", typeof(bool), typeof(ScrollableTextBlock), new PropertyMetadata(null));
#endregion

#region SelectionEnabled
        public bool SelectionEnabled
        {
            get { return (bool)GetValue(SelectionEnabledProperty); }
            set { base.SetValue(SelectionEnabledProperty, value); }
        }

        public static readonly DependencyProperty SelectionEnabledProperty = DependencyProperty.Register("SelectionEnabled", typeof(bool), typeof(ScrollableTextBlock), new PropertyMetadata(null));
#endregion

        /// <summary>
        /// Размер шрифта
        /// Поумолчанию это FontSizeContent
        /// </summary>
        public double FontSize = (double)Application.Current.Resources["FontSizeContent"];









        //public ScrollableTextBlock()
        //{
        //}

        //private static ushort FontWeight = (ushort)Settings.UI_FontWeight;



        private void OnTextChanged(string value, bool show_full)
        {
            base.Children.Clear();//((PresentationFrameworkCollection<Block>)text_block.Blocks).Clear();
            
            if (string.IsNullOrEmpty(value))
                return;

            //
            if (this.FullOnly)
                show_full = true;

            bool _showReadFull = false;

            if (!show_full)
                value = UIStringFormatterHelper.CutTextGently(value, 300);

            if (value != this.Text)
            {
                value += "...";
                _showReadFull = true;
            }
            //Medium 500
            //Normal 400
            //Light 300
            
            RichTextBlock text_block = new RichTextBlock() { IsTextSelectionEnabled = this.SelectionEnabled, FontSize = this.FontSize/*, FontWeight = new Windows.UI.Text.FontWeight(){ Weight = ScrollableTextBlock.FontWeight }*/ };
            if (this.Foreground != null)
                text_block.Foreground = this.Foreground;
            else
            {
                text_block.Style = (Style)Application.Current.Resources["RichTextBlockTheme"];
                text_block.ContextMenuOpening += Text_block_ContextMenuOpening;
            }//RichTextBox text_block = d as RichTextBox;

            //if (text_block == null)
            //    return;
            bool disableHyperlinks = this.DisableHyperlinks;//BrowserNavigationService.GetDisableHyperlinks((DependencyObject)text_block);
                                           //string textId = BrowserNavigationService.GetTextId((DependencyObject)text_block);
                                           //bool hyperlinksForeground = BrowserNavigationService.GetHideHyperlinksForeground((DependencyObject)text_block);

            Paragraph par = new Paragraph();

            foreach (string str1 in BrowserNavigationService.ParseText(BrowserNavigationService.PreprocessTextForGroupBoardMentions(value)))
            {
                string[] innerSplit = str1.Split('\b');
                if (innerSplit.Length == 1)
                    BrowserNavigationService.AddRawText(text_block, par, innerSplit[0]);
                else if (innerSplit.Length > 1)
                {
                    if (disableHyperlinks)
                    {
                        BrowserNavigationService.AddRawText(text_block, par, innerSplit[1]);
                    }
                    else
                    {
                        if (innerSplit[0].Contains(BrowserNavigationService._searchFeedPrefix))
                        {
                            int num = innerSplit[0].IndexOf(BrowserNavigationService._searchFeedPrefix) + BrowserNavigationService._searchFeedPrefix.Length;
                            string str2 = innerSplit[0].Substring(num);
                            //innerSplit[0] = innerSplit[0].Substring(0, num) + WebUtility.UrlEncode(str2);
                            innerSplit[0] = innerSplit[0].Substring(0, num) + str2;
                        }
                        
                        Hyperlink hyperlink = BrowserNavigationService.GenerateHyperlink(innerSplit[1], innerSplit[0], ((h, navstr) =>
                        {
                            /*
                            EventAggregator.Current.Publish(new HyperlinkClickedEvent() { HyperlinkOwnerId = textId });
                            
                            if (!string.IsNullOrEmpty(textId))
                            {
                                string str = navstr;
                                if (innerSplit.Length > 2)
                                    str = str.Replace("https://", "vkontakte://");
                                
                                EventAggregator.Current.Publish(new PostInteractionEvent() { PostId = textId, Action = PostInteractionAction.link_click, Link = str });
                                
                            }
                            */
                            BrowserNavigationService.NavigateOnHyperlink(navstr);
                        }), text_block.Foreground/*, hyperlinksForeground ? HyperlinkState.MatchForeground : HyperlinkState.Normal*/);

                        ToolTip toolTip = new ToolTip();
                        toolTip.Content = innerSplit[0];
                        ToolTipService.SetToolTip(hyperlink, toolTip);

                        par.Inlines.Add(hyperlink);
                    }
                }
            }
            text_block.Blocks.Add(par);
            base.Children.Add(text_block);




            if (!show_full)
            {
                if (_showReadFull)
                {
                    Border border1 = new Border();

                    TextBlock textBlock1 = new TextBlock();
                    textBlock1.FontWeight = Windows.UI.Text.FontWeights.Medium;
                    textBlock1.Text = LocalizedStrings.GetString("ExpandText");
                    //textBlock1.Style = (Style)Application.Current.Resources["TextBlockThemeHigh"];
                    textBlock1.Foreground = (SolidColorBrush)Application.Current.Resources["SystemControlHighlightAccentBrush"];
                    textBlock1.FontSize = this.FontSize;

                    border1.Child = textBlock1;
                    border1.Tapped += TextBlockReadFull_OnTap;
                    base.Children.Add(border1);
                }
            }
        }

        private void Text_block_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            RichTextBlock tb = sender as RichTextBlock;
            
            e.Handled = string.IsNullOrEmpty(tb.SelectedText);
        }

        /*
private void ParseText(string value, bool show_full)
{
   if (this.FullOnly)
       show_full = true;

   bool _showReadFull = false;
   //
   //Windows.UI.Xaml.Hosting.XamlUIPresenter.SetHost( )
   if (value == null)
       value = "";
   this.Children.Clear();

   if (!show_full)
       value = UIStringFormatterHelper.CutTextGently(value, 300);

   if (value != this.Text)
   {
       value += "...";
       _showReadFull = true;
   }

   RichTextBlock richTextBox = new RichTextBlock() { IsTextSelectionEnabled = this.SelectionEnabled, FontSize = this.FontSize };
   if (this.Foreground != null)
       richTextBox.Foreground = this.Foreground;
   else
   {
       richTextBox.Style = (Style)Application.Current.Resources["RichTextBlockTheme"];
   }

   Paragraph paragraph = new Paragraph();
   string[] splitResult = linksRegex.Split(value);//_regex_Uri
   foreach (string block in splitResult)
   {
       if (String.IsNullOrEmpty(block)) continue;
       if (block.StartsWith("http", StringComparison.OrdinalIgnoreCase))
       {
           Uri temp = null;
           if (Uri.TryCreate(block, UriKind.Absolute, out temp))
           {
               Hyperlink hp = new Hyperlink();
               hp.Click += (sender, arg) =>
               {
                   Library.NavigatorImpl.Instance.NavigateToWebUri(block);
               };

               hp.Foreground = new SolidColorBrush((Windows.UI.Color)Application.Current.Resources["PhoneAccentColor"]);

               string str = block;
               if (str.Length > 60)
               {
                   str = block.Substring(0, 60);
                   str += "...";
               }
               hp.Inlines.Add(new Run { Text = str });
               paragraph.Inlines.Add(hp);
           }
           else
           {
               paragraph.Inlines.Add(new Run { Text = block });
           }

       }
       else if (block.StartsWith("[", StringComparison.OrdinalIgnoreCase) && block.EndsWith("]", StringComparison.OrdinalIgnoreCase))
       {
           string part = block.Replace("[", "").Replace("]", "");
           //paragraph.Inlines.Add(new Run { Text = part.Split(new char[] { '|' })[1], Foreground = (SolidColorBrush)Application.Current.Resources["AccentBrushHigh"] });

           Hyperlink hp = new Hyperlink();
           hp.Click += (sender, arg) =>
           {
               string temp = part.Split(new char[] { '|' })[0];
               if (temp.Contains("club"))
               {
                   int id = int.Parse(temp.Replace("club", ""));
                   Library.NavigatorImpl.Instance.NavigateToProfilePage(-id);
               }
               else if (temp.Contains("id"))
               {
                   int id = int.Parse(temp.Replace("id", ""));
                   Library.NavigatorImpl.Instance.NavigateToProfilePage(id);
               }
               //Library.NavigatorImpl.Instance.NavigateToWebUri(block);
           };

           hp.Foreground = new SolidColorBrush((Windows.UI.Color)Application.Current.Resources["PhoneAccentColor"]);

           string[] temp2 = part.Split(new char[] { '|' });
           if (temp2.Length > 1)
               hp.Inlines.Add(new Run { Text = temp2[1] });
           else
               hp.Inlines.Add(new Run { Text = part });
           paragraph.Inlines.Add(hp);
       }
       else if (block.StartsWith("#"))
       {
           //paragraph.Inlines.Add(new Run { Text = block, Foreground = (SolidColorBrush)Application.Current.Resources["AccentBrushHigh"] });
           Hyperlink hp = new Hyperlink();
           hp.Click += (sender, arg) =>
           {
               Library.NavigatorImpl.Instance.NavigateToWebUri("vk.com/feed?section=search&q=" + block);
           };

           hp.Foreground = new SolidColorBrush((Windows.UI.Color)Application.Current.Resources["PhoneAccentColor"]);
           hp.Inlines.Add(new Run { Text = block });
           paragraph.Inlines.Add(hp);
       }
       else if (block.StartsWith("vk.me", StringComparison.OrdinalIgnoreCase) || block.StartsWith("vk.cc", StringComparison.OrdinalIgnoreCase))
       {
           Hyperlink hp = new Hyperlink();
           hp.Click += (sender, arg) =>
           {
               Library.NavigatorImpl.Instance.NavigateToWebUri(block);
           };

           hp.Foreground = new SolidColorBrush((Windows.UI.Color)Application.Current.Resources["PhoneAccentColor"]);
           hp.Inlines.Add(new Run { Text = block });
           paragraph.Inlines.Add(hp);
       }
       else
       {

           //switch (Settings.EmojiType)
           //{
           //    case 1://Skype
           //        this.SetSkypeEmoji(paragraph, block);
           //        continue;
           //    case 2://Apple
           //        this.SetAppleEmoji(paragraph, block);
           //        continue;
           //    default:
           //        paragraph.Inlines.Add(new Run { Text = block });
           //        continue;
           //}
       }
   }
   richTextBox.Blocks.Add(paragraph);
   //
   base.Children.Add(richTextBox);

   if (!show_full)
   {
       if (_showReadFull)
       {
           Border border1 = new Border();

           string str = string.Format("{0}...", "Показать полностью");//CommonResources.ExpandText
           TextBlock textBlock1 = new TextBlock();
           textBlock1.FontWeight = Windows.UI.Text.FontWeights.Bold;
           textBlock1.Text = str;
           //textBlock1.Foreground = (SolidColorBrush)Application.Current.Resources["AccentBrushHigh"];
           textBlock1.Style = (Style)Application.Current.Resources["TextBlockThemeHigh"];
           textBlock1.FontSize = this.FontSize;

           border1.Child = textBlock1;
           border1.Tapped += TextBlockReadFull_OnTap;
           base.Children.Add(border1);
       }
   }
}
*/
        /*
        

        /// <summary>
        /// Вставляет в параграф Apple Emoji.
        /// </summary>
        /// <param name="paragraph">Параграф для добавления.</param>
        /// <param name="block">Блок текста для добавления.</param>
        private void SetAppleEmoji(Paragraph paragraph, string block)
        {
            TextElementEnumerator elementEnumerator = StringInfo.GetTextElementEnumerator(block);
            bool flag1 = elementEnumerator.MoveNext();
            while (flag1)
            {
                string textElement1 = elementEnumerator.GetTextElement();
                
                string hexString1 = this.ConvertToHexString(Encoding.BigEndianUnicode.GetBytes(textElement1));
                if (hexString1 == "")
                {
                    flag1 = elementEnumerator.MoveNext();
                    paragraph.Inlines.Add(new Run { Text = textElement1 });
                }
                else
                {
                        if(Smiles.Gestures.ContainsValue(hexString1))
                    {
                        string path = String.Format("ms-appx:///Assets/Emoji/{0:x}.png", hexString1);
                        InlineUIContainer cont = new InlineUIContainer();
                        Image img = new Image() { Height = 18, Margin = new Thickness(0, 0, 0, -2), Stretch = Stretch.Uniform, Source = new BitmapImage(new Uri(path)) };
                        cont.Child = img;

                        paragraph.Inlines.Add(cont);
                    }
                    flag1 = elementEnumerator.MoveNext();
                }
            }
        }

        /// <summary>
        /// Вставляет в параграф Apple Emoji.
        /// </summary>
        /// <param name="paragraph">Параграф для добавления.</param>
        /// <param name="block">Блок текста для добавления.</param>
        private void SetSkypeEmoji(Paragraph paragraph, string block)
        {
            var r = emojiRegex.Split(block);
            foreach (string s in r)
            {
                if (emojiRegex.IsMatch(s))
                {
                    string path = null;
                    for (int i = 0; i < s.Length; i += Char.IsSurrogatePair(s, i) ? 2 : 1)
                    {
                        try
                        {
                            int x = Char.ConvertToUtf32(s, i);
                            path = String.Format("{0:x}", x);
                        }
                        catch (Exception) { }
                    }

                    if (path == null)
                    {
                        paragraph.Inlines.Add(new Run { Text = s });
                        continue;
                    }

                    var cont = new InlineUIContainer();
                    AnimatedEmojiUC img = new AnimatedEmojiUC(path);
                    //var img = new Image() { Stretch = Stretch.Uniform, Source = new BitmapImage(new Uri(path)) };
                    img.Width = img.Height = 18;
                    img.Margin = new Thickness(0, 0, 0, -2);
                    cont.Child = img;

                    paragraph.Inlines.Add(cont);
                }
                else
                    paragraph.Inlines.Add(new Run { Text = s });
            }
        }
        */


        void TextBlockReadFull_OnTap(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            //this.ParseText(this.Text, true);
            this.OnTextChanged(this.Text, true);
        }
    }
}

/*
 * Если Вы копировали Символ (😊), то после отправки/сохранения попробуйте обновить страницу. Если после обновления страницы смайл не появился, то, видимо, Ваше устройство просто не скопировало символ — воспользуйтесь Кодом.

Если Вы копировали Код и после отправки видны символы и цифры, значит, Вы что-то забыли. Код должен начинаться с "&#" и заканчиваться на ";", а знак подчёркивания ( _ ) нужно удалять перед отправкой!

Если смайл не появился, а на его месте отображается символ, то обратите внимание на вопрос-ответ выше. Смайлы преобразовываются в графический вид только в определённых местах.

Изменение цвета смайлов
ВКонтакте поддерживает изменение оттенков кожи у "человечных" смайлов, например: 👍🏻 👍🏼 👍🏽 👍🏾 👍🏿

Для изменения существуют добавочные символы, которые нужно вставлять сразу после смайла (без пробела), например: 👍🏿 | &#_128077;&#_127999;

    */
