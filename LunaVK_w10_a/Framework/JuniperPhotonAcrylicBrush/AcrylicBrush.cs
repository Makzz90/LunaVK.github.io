﻿#if WINDOWS_UWP
namespace LunaVK.Framework.JuniperPhotonAcrylicBrush
{
    public class AcrylicBrush : AcrylicBrushBase
    {
        public AcrylicBrush() { }

        protected override BackdropBrushType GetBrushType()
        {
            return BackdropBrushType.Backdrop;
        }
    }
}
#endif