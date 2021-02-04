using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaVK.Core.Library
{
    public class TileManager
    {
        private static TileManager _instance;
        public static TileManager Instance
        {
            get
            {
                if (TileManager._instance == null)
                    TileManager._instance = new TileManager();
                return TileManager._instance;
            }
        }

        public void SetContentAndCount(string content1, string content2, string content3, uint count, Action callback=null)
        {/*
            Execute.ExecuteOnUIThread((Action)(() =>
            {
                IconicTileData data = new IconicTileData();
                data.Count = (new int?(count));
                data.WideContent1 = content1;
                data.WideContent2 = content2;
                data.WideContent3 = content3;
                this.SetColor(data);
                this.UpdatePrimaryTile(data);
                callback();
            }));*/
        }
    }
}
