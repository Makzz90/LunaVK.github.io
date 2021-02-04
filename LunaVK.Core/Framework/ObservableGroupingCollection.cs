using LunaVK.Core.Library;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace LunaVK.Core.Framework
{

    // Grouping class 
    // credit
    // http://motzcod.es/post/94643411707/enhancing-xamarinforms-listview-with-grouping

    public class Grouping<T> : ObservableCollection<T>
    {
        public string Key { get; set; }

        public Grouping(string key, IReadOnlyList<T> items)
        {
            this.Key = key;
            foreach (var item in items)
                base.Items.Add(item);
        }
    }

    public class ObservableGroupingCollection<T>
    {
        public ObservableCollection<Grouping<T>> Items { get; private set; }
        private ObservableCollection<T> _rootCollection;

        public ObservableGroupingCollection(ObservableCollection<T> collection)
        {
            _rootCollection = collection;
            _rootCollection.CollectionChanged += _rootCollection_CollectionChanged;

            this.Items = new ObservableCollection<Grouping<T>>();
            foreach (var item in collection)
                this.ProcessAddItem(item);
        }

        ~ObservableGroupingCollection()
        {
            _rootCollection.CollectionChanged -= _rootCollection_CollectionChanged;
        }

        private void _rootCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            HandleCollectionChanged(e);
        }

        private void ProcessAddItem(T item)
        {
            string value = ((ISupportGroup)item).Key;

            // find matching group if exists
            var existingGroup = Items.FirstOrDefault(g => g.Key.Equals(value));

            if (existingGroup == null)
            {
                var newlist = new List<T>() { item };

                this.Items.Add(new Grouping<T>(value, newlist));//this.Items.Insert(0, new Grouping<T>(value, newlist));
            }
            else
            {
                existingGroup.Add(item);//existingGroup.Insert(0, item);
            }
        }

        //todo: https://github.com/xamarin/Xamarin.Forms/issues/3788 может это поможет
        private void HandleCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                var item = (T)(e.NewItems[0]);
                if (!(item is ISupportGroup))
                    throw new Exception("Can only add ISupportGroup items.");
                
                string value = ((ISupportGroup)item).Key;

                var existingGroup = this.Items.FirstOrDefault(g => g.Key.Equals(value)); // find matching group if exists
                
                int i = e.NewStartingIndex;

                Grouping<T> newGroup = null;

                int newTruePos = -1;

                this.GetGroupAndIndex(i, out newGroup, out newTruePos);

                if (newGroup == null || ((ISupportGroup)newGroup[0]).Key != value)
                {
                    var newlist = new List<T>() { item };

                    if (e.NewStartingIndex > 0)
                        this.Items.Add(new Grouping<T>(value, newlist));
                    else
                        this.Items.Insert(0, new Grouping<T>(value, newlist));
                }
                else
                {
                    newGroup.Insert(newTruePos, item);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                var item = (T)(e.OldItems[0]);
                //string value = ((ISupportGroup)item).Key;

                Grouping<T> oldGroup = null;

                int oldTruePos = -1;

                this.GetGroupAndIndex(e.OldStartingIndex, out oldGroup, out oldTruePos);

                oldGroup.RemoveAt(oldTruePos);
                if (oldGroup.Count == 0)//Удаляем группу, если она опустела
                    this.Items.Remove(oldGroup);
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                this.Items.Clear();
            }
            else if (e.Action == NotifyCollectionChangedAction.Move)
            {
                int newPos = e.NewStartingIndex;
                int oldPos = e.OldStartingIndex;
                var item = (T)(e.OldItems[0]);

                Grouping<T> newGroup = null;
                Grouping<T> oldGroup = null;

                int newTruePos = -1;
                int oldTruePos = -1;

                this.GetGroupAndIndex(newPos, out newGroup, out newTruePos);
                this.GetGroupAndIndex(oldPos, out oldGroup, out oldTruePos);

                if (newGroup == oldGroup)
                {
                    newGroup.Move(oldTruePos, newTruePos);
                }
                else
                {
                    oldGroup.Remove(item);
                    //((ISupportGroup)item).Key = ((ISupportGroup)newGroup[0]).Key;
                    newGroup.Insert(newTruePos, item);
                }
            }
        }


        private void GetGroupAndIndex(int id, out Grouping<T> group, out int trueIndex)
        {
            int i = id;
            group = null;
            trueIndex = -1;

            foreach (var item in this.Items)
            {
                if (item.Count > i || (this.Items.Last() == item && item.Count == i))//в этой группе есть место
                {
                    group = item;
                    trueIndex = i;
                    break;
                }

                i -= item.Count;
            }
        }
    }
}
