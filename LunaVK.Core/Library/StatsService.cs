using LunaVK.Core.DataObjects;
using LunaVK.Core.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaVK.Core.Library
{
    public class StatsService
    {
        private static StatsService _instance;
        public static StatsService Instance
        {
            get
            {
                if (StatsService._instance == null)
                    StatsService._instance = new StatsService();
                return StatsService._instance;
            }
        }

        public void GetStats(uint gId, uint appid, DateTime? from, DateTime? to, string interval, uint intervalCount, string statsGroups, bool extended, Action<VKResponse<List<VKStatsResponse>>> calback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if(gId!=0)
                parameters["group_id"] = gId.ToString();//идентификатор сообщества
            else
                parameters["app_id"] = appid.ToString();//идентификатор приложения

            if(from!=null)
                parameters["timestamp_from"] = from.Value.Ticks.ToString();//начало периода статистики в Unixtime
            if(to!=null)
                parameters["timestamp_to"] = to.Value.ToString();//окончание периода статистики в Unixtime
            parameters["interval"] = interval;//временные интервалы. Возможные значения: day, week, month, year, all. По умолчанию: day
            parameters["intervals_count"] = intervalCount.ToString();//количество интервалов времени
            //parameters["filters"] = gId.ToString();//список слов, разделенных через запятую
            parameters["stats_groups"] = statsGroups;//фильтр для получения данных по конкретному блоку статистики сообщества. Возможные значения: visitors, reach, activity

            if(extended)
                parameters["extended"] = "1";//возвращать дополнительно агрегированные данные в результатах

            VKRequestsDispatcher.DispatchRequestToVK<List<VKStatsResponse>>("stats.get", parameters, calback);
        }
    }
}
