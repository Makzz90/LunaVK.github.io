using LunaVK.Core.DataObjects;
using LunaVK.Core.Framework;
using LunaVK.Core.Library;
using LunaVK.Core.Network;
using LunaVK.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaVK.ViewModels
{
    public class GroupStatsViewModel : ViewModelBase
    {
        private readonly uint Id;
        public bool IsLoaded;

        public ObservableCollection<double> Visitors { get; private set; }
        public ObservableCollection<double> Subscribers { get; private set; }
        public ObservableCollection<double> Views { get; private set; }

        public double TotalVisitors { get; private set; }
        public double TotalSubscribers { get; private set; }
        public double TotalViews { get; private set; }

        public GroupStatsViewModel(uint id)
        {
            this.Id = id;
//#if DEBUG
//            this.Id = 154148777;
//#endif
            this.Visitors = new ObservableCollection<double>();
            this.Subscribers = new ObservableCollection<double>();
            this.Views = new ObservableCollection<double>();
        }

        private void Clear()
        {
            this.IsLoaded = false;

            this.Visitors.Clear();
            this.Views.Clear();
            this.Subscribers.Clear();

            this.TotalSubscribers=0;
            this.TotalVisitors =0;
            this.TotalViews =0;

            base.NotifyPropertyChanged(nameof(this.TotalVisitors));
            base.NotifyPropertyChanged(nameof(this.TotalSubscribers));
            base.NotifyPropertyChanged(nameof(this.TotalViews));
        }

        public void LoadData(uint period)
        {
            this.Clear();

//#if DEBUG
//            RequestsDispatcher.GetResponseFromDump<List<VKStatsResponse>>(300, "stats154148777.json", (result) =>
//#else

            string temp = "day";
            uint intervalCount = 12;
            if (period == 1)
            {
                temp = "week";
                intervalCount = 14;
            }
            else if (period == 2)
            {
                temp = "month";
                intervalCount = 50;
            }
            else if (period == 3)
            {
                temp = "year";
                intervalCount = 24;
            }

            StatsService.Instance.GetStats(this.Id, 0, null, null, temp, intervalCount, "", false, (result) =>
//#endif
            {
                if(result.error.error_code== Core.Enums.VKErrors.None)
                {
                    this.IsLoaded = true;

                    Execute.ExecuteOnUIThread(() => {

                        foreach(var item in result.response)
                        {
                            this.Visitors.Add(item.visitors.visitors);
                            this.Views.Add(item.visitors.views);
                            if (item.activity == null)
                                this.Subscribers.Add(0);
                            else
                            {
                                this.Subscribers.Add(item.activity.subscribed);
                                this.TotalSubscribers += item.activity.subscribed;
                            }


                            this.TotalVisitors += item.visitors.visitors;
                            this.TotalViews += item.visitors.views;
                        }

                        base.NotifyPropertyChanged(nameof(this.TotalVisitors));
                        base.NotifyPropertyChanged(nameof(this.TotalSubscribers));
                        base.NotifyPropertyChanged(nameof(this.TotalViews));
                    });
                }
            });
        }
    }
}
