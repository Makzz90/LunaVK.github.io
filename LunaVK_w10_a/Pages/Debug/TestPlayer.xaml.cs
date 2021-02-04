using LunaVK.Core.DataObjects;
using LunaVK.Core.ViewModels;
using LunaVK.ViewModels;
using System;
using System.Collections.Generic;
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
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class TestPlayer : Page
    {
        public TestPlayer()
        {
            this.InitializeComponent();

            AudioPlayerViewModel.Instance.FillTracks(new List<VKAudio>() { new VKAudio() { title="Test", artist = "Testovich", duration = 59 } },"test");
        }
    }
}
