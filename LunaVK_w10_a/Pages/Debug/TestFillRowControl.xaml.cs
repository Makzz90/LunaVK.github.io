using LunaVK.Core.DataObjects;
using LunaVK.UC.Controls;
using LunaVK.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace LunaVK.Pages.Debug
{
    public sealed partial class TestFillRowControl : Page
    {
        public TestFillRowControl()
        {
            this.InitializeComponent();
            base.DataContext = new TempClass();
            this.Loaded += TestFillRowControl_Loaded;
        }

        private void TestFillRowControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.VM.Load();
        }

        public TempClass VM
        {
            get { return base.DataContext as TempClass; }
        }

        private void FillRowView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            int rows = (int)e.NewSize.Width / 125;
            this.VM.UpdateRowItemsCount(rows);
        }

        public class ClassImgItem
        {
            public List<VKPhoto> Items { get; private set; }
            public double Height
            {
                get
                {
                    double total = this.Items.Select((i => i.height)).Max();
                    double ratio = (Window.Current.Content as FrameworkElement).ActualHeight / total;
                    total *= ratio;
                    
                    return total;
                }
            }

            public ClassImgItem(List<VKPhoto> items)
            {
                this.Items = items;
            }
        }

        public class TempClass
        {
            //public ObservableCollection<ClassImgItem> SortedItems { get; private set; }
            public ObservableCollection<List<VKAttachment>> SortedItems { get; private set; }
            public int RowItemsCount { get; private set; }
            public ObservableCollection<VKPhoto> Items { get; private set; }

            public TempClass()
            {
                this.Items = new ObservableCollection<VKPhoto>();
                this.RowItemsCount = 4;
                this.SortedItems = new ObservableCollection<List<VKAttachment>>();
            }

            public void Load()
            {
                for(int i =0;i<40;i++)
                {
                    VKPhoto photo = new VKPhoto()
                    {
                        height = 400,
                        width = 800,
                        id = (uint)i,
                        sizes = new Dictionary<char, VKImageWithSize>()
                    };
                    photo.sizes.Add('x',new VKImageWithSize() { url = "https://vk.com", type = 'x' });
                    this.Items.Add(photo);
                   
                }
                this.UpdateRowItems();
            }

            public void UpdateRowItemsCount(int rowItemsCount)
            {
                if (rowItemsCount != this.RowItemsCount)
                {
                    System.Diagnostics.Debug.WriteLine("UpdateRowItemsCount: "+rowItemsCount);
                    this.RowItemsCount = rowItemsCount;
                    this.UpdateRowItems();
                }
            }

            private void UpdateRowItems()
            {
                /*
                int i = 0;
                var rowItems = this.Items.Take(RowItemsCount);
                while (rowItems != null && rowItems.Count() != 0)
                {
                    var rowItemsCount = rowItems.Count();
                    var item = this.SortedItems.ElementAtOrDefault(i);
                    if (item == null)
                    {
                        item = new List<VKPhoto>();
                        this.SortedItems.Insert(i, item);
                    }

                    for (int j = 0; j < rowItemsCount; j++)
                    {
                        var rowItem = rowItems.ElementAt(j);
                        var temp = item.ElementAtOrDefault(j);
                        if (temp == null || !temp.Equals(rowItem))
                        {
                            item.Insert(j, rowItem);
                        }
                    }

                    while (item.Count > rowItemsCount)
                    {
                        item.RemoveAt(item.Count - 1);
                    }
                    i++;
                    rowItems = this.Items.Skip(i * this.RowItemsCount).Take(this.RowItemsCount);
                }

                int rowCount = this.Items.Count / this.RowItemsCount + 1;
                while (this.SortedItems.Count > rowCount)
                {
                    this.SortedItems.RemoveAt(this.SortedItems.Count - 1);
                }
                */
                this.SortedItems.Clear();
                int i = 0;
                while(true)
                {
                    var rowItems = this.Items.Skip(i * this.RowItemsCount).Take(this.RowItemsCount);
                    if (rowItems.Count() == 0)
                        break;
                    //this.SortedItems.Add(new ClassImgItem(rowItems.ToList()));
                    this.SortedItems.Add(rowItems.Select((p => new VKAttachment() { type = Core.Enums.VKAttachmentType.Photo, photo = p })).ToList());
                    i++;
                }
            }
        }
    }
}
