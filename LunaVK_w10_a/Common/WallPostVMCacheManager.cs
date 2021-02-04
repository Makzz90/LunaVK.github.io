using LunaVK.Core;
using LunaVK.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaVK.Common
{
    public static class WallPostVMCacheManager
    {
        private static readonly string _wallPostKey = "WallPost";
        private static WallPostViewModel _instance;

        public static void RegisterForDelayedSerialization(WallPostViewModel vm)
        {
            WallPostVMCacheManager._instance = vm;
        }

        public static void TrySerializeVM(WallPostViewModel vm)
        {
            string key = WallPostVMCacheManager.GetKey(vm);
            CacheManager.TrySerialize(vm, key, false);
        }

        public static void TryDeserializeVM(WallPostViewModel vm)
        {
            string key = WallPostVMCacheManager.GetKey(vm);
            CacheManager.TryDeserialize(vm, key, false);
        }

        private static string GetKey(WallPostViewModel vm)
        {
            return "WallPost_" + Settings.UserId + "_" + vm.UniqueId;
        }

        public static void ResetVM(WallPostViewModel vm)
        {
            CacheManager.TryDelete(WallPostVMCacheManager.GetKey(vm), false);
        }

        public static void TryDeserializeInstance(WallPostViewModel vm)
        {
            CacheManager.TryDeserialize(vm, WallPostVMCacheManager._wallPostKey, false);
        }

        public static void TrySerializeInstance()
        {
            if (WallPostVMCacheManager._instance == null)
                return;
            CacheManager.TrySerialize(WallPostVMCacheManager._instance, WallPostVMCacheManager._wallPostKey, false);
        }

        public static void ResetInstance()
        {
            WallPostVMCacheManager._instance = null;
            CacheManager.TryDelete(WallPostVMCacheManager._wallPostKey, false);
        }
    }
}
