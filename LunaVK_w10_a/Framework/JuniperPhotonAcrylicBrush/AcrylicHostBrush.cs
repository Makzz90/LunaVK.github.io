#if WINDOWS_UWP

namespace LunaVK.Framework.JuniperPhotonAcrylicBrush
{
    public class AcrylicHostBrush : AcrylicBrushBase
    {
        public AcrylicHostBrush() { }

        protected override BackdropBrushType GetBrushType()
        {
            return BackdropBrushType.HostBackdrop;
        }
    }
}
#endif