using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using System.Collections.Specialized;
using Windows.Foundation.Collections;
using Windows.Foundation;
using Windows.Foundation.Metadata;

namespace LunaVK.UC.Controls
{
    [MarshalingBehavior(MarshalingType.Agile)]
    [Threading(ThreadingModel.Both)]
    [Version(100859904)]
    [WebHostHidden]
    public class Pivot : ItemsControl
    {
        private FlipView _flip;
        private ListView _listView;
        private List<object> _headers;
        private List<object> _items;

        public event SelectionChangedEventHandler SelectionChanged;
        public int SelectedIndex
        {
            get
            {
                return this._flip.SelectedIndex;
            }
            set
            {
                this._flip.SelectedIndex = value;
            }
        }

        public Pivot()
        {
            base.Items.VectorChanged += Items_VectorChanged;
            this._headers = new List<object>();
            this._items = new List<object>();
            base.Loaded += Pivot_Loaded;
        }

        void Pivot_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.SelectionChanged != null)
                this.SelectionChanged(this, new SelectionChangedEventArgs(new List<object>(), new List<object>()));
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this._flip = GetTemplateChild("_flip") as FlipView;
            this._listView = GetTemplateChild("_listView") as ListView;

            this._listView.ItemsSource = this._headers;
            this._flip.ItemsSource = this._items;

            this._flip.SelectionChanged += _flip_SelectionChanged;
            //this._flip.PointerWheelChanged += (s, e) => { e.Handled = true; };
        }
        
        void _flip_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(this.SelectionChanged!=null)
                this.SelectionChanged(this, e);
        }

        void Items_VectorChanged(IObservableVector<object> sender, IVectorChangedEventArgs @event)
        {
            if(@event.CollectionChange == CollectionChange.ItemInserted)
            {
                PivotItem temp = sender[(int)@event.Index] as PivotItem;

                this._headers.Add(temp.Header);
                this._items.Add(temp.Content);
            }
        }
    }
}
