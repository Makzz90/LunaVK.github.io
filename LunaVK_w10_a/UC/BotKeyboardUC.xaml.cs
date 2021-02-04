using LunaVK.Core.DataObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
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

// Документацию по шаблону элемента "Пользовательский элемент управления" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234236

namespace LunaVK.UC
{
    public sealed partial class BotKeyboardUC : UserControl
    {
        public BotKeyboardUC()
        {
            this.InitializeComponent();
        }

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IList<List<VKBotKeyboard.KeyboardButton>>), typeof(BotKeyboardUC), new PropertyMetadata(null,OnItemsSourcePropertyChanged));
        public IList ItemsSource
        {
            get { return (IList)base.GetValue(BotKeyboardUC.ItemsSourceProperty); }
            set { base.SetValue(BotKeyboardUC.ItemsSourceProperty, value); }
        }

        private static void OnItemsSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BotKeyboardUC virtualizingPanel2 = d as BotKeyboardUC;
            if (virtualizingPanel2 == null)
                return;

            INotifyCollectionChanged oldValue = e.OldValue as INotifyCollectionChanged;
            if (oldValue != null)
                virtualizingPanel2.UnhookCollectionChanged(oldValue);

            INotifyCollectionChanged newValue = e.NewValue as INotifyCollectionChanged;
            if (newValue != null)
            {
                virtualizingPanel2._botKeyboard.ItemsSource = newValue;
                IEnumerator enumerator = (newValue as ICollection).GetEnumerator();
                try
                {
                    while (enumerator.MoveNext())
                    {
                        object current = enumerator.Current;
                    }
                }
                finally
                {

                }

                virtualizingPanel2.HookUpCollectionChanged(newValue);
            }
        }

        public void HookUpCollectionChanged(INotifyCollectionChanged collection)
        {
            collection.CollectionChanged += this.collection_CollectionChanged;
        }

        public void UnhookCollectionChanged(INotifyCollectionChanged collection)
        {
            collection.CollectionChanged -= this.collection_CollectionChanged;
        }

        private void collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            BotKeyboardUC virtualizingPanel2 = this;
            //List<IVirtualizable> itemsToInsert = new List<IVirtualizable>();
            if (e.NewItems != null)
            {

            }

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
    }
}
