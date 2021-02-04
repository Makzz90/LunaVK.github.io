using LunaVK.Common;
using LunaVK.Core.Framework;
using LunaVK.Core.Library;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
//http://www.unicode.org/emoji/charts/full-emoji-list.html
namespace LunaVK.UC
{
    public sealed partial class EmojiControlUC : UserControl
    {
        private Action<object> _emojiTapped;
        public EmojiControlUC()
        {
            base.DataContext = new EmojiControlViewModel();
            this.InitializeComponent();
            this.Unloaded += EmojiControlUC_Unloaded;
            /*
            // 1.You need a data list
            ObservableCollection<Author> collection = new ObservableCollection<Author>();
            // and fill the list with data: collection.Add(new Author() { ... ] );

            // 2.You have to group data by a rule
            var groupedAuthors = from author in collection
                                 group author by author.Name.First() into firstLetter
                                 orderby firstLetter.Key
                                 select firstLetter;

            // 3.Link the new grouped data
            this.EmojiSource.Source = groupedAuthors;
            */
            this.AuthorsKeys.ItemsSource = this.EmojiSource.View.CollectionGroups;
        }

        private EmojiControlViewModel VM
        {
            get { return base.DataContext as EmojiControlViewModel; }
        }

        private void EmojiControlUC_Unloaded(object sender, RoutedEventArgs e)
        {
            this._emojiTapped = null;
        }

        public EmojiControlUC(Action<object> callback):this()
        {
            this._emojiTapped = callback;
        }




        public class EmojiControlViewModel
        {
            public EmojiControlViewModel()
            {
                this.Items = new ObservableCollection<EmojiDataItem>();
                this.GroupedItems = new ObservableGroupingCollection<EmojiDataItem>(this.Items);
                
                foreach (var index in Smiles.EmotionIndex)
                {
                    EmojiDataItem emojiDataItem = new EmojiDataItem(index, "Emotion");
                    this.Items.Add(emojiDataItem);
                }

                foreach(var index in Smiles.GestureIndex)
                {
                    EmojiDataItem emojiDataItem = new EmojiDataItem(index, "Gestures");
                    this.Items.Add(emojiDataItem);
                }

                foreach (var index in Smiles.AnimalIndex)
                {
                    EmojiDataItem emojiDataItem = new EmojiDataItem(index, "Animals");
                    this.Items.Add(emojiDataItem);
                }

                foreach (var index in Smiles.PlantIndex)
                {
                    EmojiDataItem emojiDataItem = new EmojiDataItem(index, "Plants");
                    this.Items.Add(emojiDataItem);
                }

                foreach (var index in Smiles.LoveIndex)
                {
                    EmojiDataItem emojiDataItem = new EmojiDataItem(index, "Love");
                    this.Items.Add(emojiDataItem);
                }

                foreach (var index in Smiles.TransportIndex)
                {
                    EmojiDataItem emojiDataItem = new EmojiDataItem(index, "Transport");
                    this.Items.Add(emojiDataItem);
                }

                foreach (var index in Smiles.PeopleIndex)
                {
                    EmojiDataItem emojiDataItem = new EmojiDataItem(index, "People");
                    this.Items.Add(emojiDataItem);
                }

                foreach (var index in Smiles.FoodIndex)
                {
                    EmojiDataItem emojiDataItem = new EmojiDataItem(index, "Food");
                    this.Items.Add(emojiDataItem);
                }

                foreach (var index in Smiles.MusicIndex)
                {
                    EmojiDataItem emojiDataItem = new EmojiDataItem(index, "Music");
                    this.Items.Add(emojiDataItem);
                }

                foreach (var index in Smiles.EventIndex)
                {
                    EmojiDataItem emojiDataItem = new EmojiDataItem(index, "Events");
                    this.Items.Add(emojiDataItem);
                }

                foreach (var index in Smiles.SportIndex)
                {
                    EmojiDataItem emojiDataItem = new EmojiDataItem(index, "Sport");
                    this.Items.Add(emojiDataItem);
                }

                foreach (var index in Smiles.GadgetIndex)
                {
                    EmojiDataItem emojiDataItem = new EmojiDataItem(index, "Gadgets");
                    this.Items.Add(emojiDataItem);
                }

                foreach (var index in Smiles.SignIndex)
                {
                    EmojiDataItem emojiDataItem = new EmojiDataItem(index, "Signs");
                    this.Items.Add(emojiDataItem);
                }

                foreach (var index in Smiles.NumberIndex)
                {
                    EmojiDataItem emojiDataItem = new EmojiDataItem(index, "Numbers");
                    this.Items.Add(emojiDataItem);
                }

                foreach (var index in Smiles.ArrowIndex)
                {
                    EmojiDataItem emojiDataItem = new EmojiDataItem(index, "Arrow");
                    this.Items.Add(emojiDataItem);
                }

                foreach (var index in Smiles.ZodiacIndex)
                {
                    EmojiDataItem emojiDataItem = new EmojiDataItem(index, "Zodiac");
                    this.Items.Add(emojiDataItem);
                }

                foreach (var index in Smiles.OfficeIndex)
                {
                    EmojiDataItem emojiDataItem = new EmojiDataItem(index, "Office");
                    this.Items.Add(emojiDataItem);
                }
                
                foreach (var index in Smiles.FlagIndex)
                {
                    EmojiDataItem emojiDataItem = new EmojiDataItem(index, "Flags");
                    this.Items.Add(emojiDataItem);
                }
                
                foreach (var index in Smiles.WearIndex)
                {
                    EmojiDataItem emojiDataItem = new EmojiDataItem(index, "Wear");
                    this.Items.Add(emojiDataItem);
                }

                foreach (var index in Smiles.SpaceIndex)
                {
                    EmojiDataItem emojiDataItem = new EmojiDataItem(index, "Space");
                    this.Items.Add(emojiDataItem);
                }

                foreach (var index in Smiles.BuildingIndex)
                {
                    EmojiDataItem emojiDataItem = new EmojiDataItem(index, "Building");
                    this.Items.Add(emojiDataItem);
                }

                foreach (var index in Smiles.WeatherIndex)
                {
                    EmojiDataItem emojiDataItem = new EmojiDataItem(index, "Weather");
                    this.Items.Add(emojiDataItem);
                }

                foreach (var index in Smiles.GamesIndex)
                {
                    EmojiDataItem emojiDataItem = new EmojiDataItem(index, "Games");
                    this.Items.Add(emojiDataItem);
                }

                foreach (var index in Smiles.PictureIndex)
                {
                    EmojiDataItem emojiDataItem = new EmojiDataItem(index, "Pictures");
                    this.Items.Add(emojiDataItem);
                }
            }

            public class EmojiDataItem : ISupportGroup
            {
                public string Code
                {
                    get { return Emoji.Dict[(int)this.Index]; }
                }

                public string ElementCode
                {
                    get
                    {
                        return this.ToStringFromUnicodeCodePoints(this.Code);
                    }
                }

                private string ToStringFromUnicodeCodePoints(string input)
                {
                    var result = new StringBuilder();

                    while (true)
                    {
                        string temp = input.Substring(0, 4);

                        char character = (char)Convert.ToInt16(temp, 16);
                        result.Append(character);

                        if (input.Length > 4)
                            input = input.Substring(4, input.Length - 4);
                        else
                            break;
                    }

                    return result.ToString();
                }




                public Uri Uri { get; private set; }

                public uint Index { get; private set; }

                public EmojiDataItem(uint index, string key)
                {
                    this.Index = index;
                    this.Uri = new Uri("ms-appx:///Assets/Emoji/" + this.Code + ".png");
                    this.Key = key;
                }


                public string Key { get; private set; }
            }
            /*
            /// <summary>
            /// ms-appx:///Assets/Emoji/{0:X8}.png
            /// </summary>
            /// <param name="str"></param>
            /// <returns></returns>
            private Uri BuildUri(string str)
            {
                switch (str.Length)
                {
                    case 2:
                        return new Uri(string.Format("ms-appx:///Assets/Emoji/{0:X8}.png", ((uint)(0 | str[0] << 16) | str[1])));
                    case 4:
                        return new Uri(string.Format("ms-appx:///Assets/Emoji/{0:X}{1:X}.png", ((uint)(0 | (int)str[0] << 16) | (uint)str[1]), (0 | (ulong)((uint)str[2] << 16) | (ulong)str[3])));
                    default:
                        return new Uri(string.Format("ms-appx:///Assets/Emoji/{0:X}.png", (short)str[0]));
                }
            }
            */
            private ObservableCollection<EmojiDataItem> Items;
            public ObservableGroupingCollection<EmojiDataItem> GroupedItems { get; private set; }
        }

        private void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var vm = e.AddedItems[0] as EmojiControlViewModel.EmojiDataItem;
            int i = 0;
        }

        private void Border_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //var vm = (sender as FrameworkElement).DataContext as EmojiControlViewModel.EmojiDataItem;
            this._emojiTapped?.Invoke(sender);
            e.Handled = true;
        }

        private void GridView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GridView gv = sender as GridView;
            var panel = (ItemsWrapGrid)gv.ItemsPanelRoot;

            //     panel.Orientation = Orientation.Horizontal;

            double colums = e.NewSize.Width / 64;

            panel.MaximumRowsOrColumns = (int)colums;

            panel.ItemHeight = panel.ItemWidth = e.NewSize.Width / (int)colums;
        }


        /*
         * public void Handle(SpriteElementTapEvent data)
        {
            if (!this._isCurrent)
                return;
            base.Dispatcher.BeginInvoke((Action)(() =>
            {
                int selectionStart = this.textBoxNewMessage.SelectionStart;
                this.textBoxNewMessage.Text = (this.textBoxNewMessage.Text.Insert(selectionStart, data.Data.ElementCode));
                this.textBoxNewMessage.Select(selectionStart + data.Data.ElementCode.Length, 0);
            }));
        }
        */
    }
}
