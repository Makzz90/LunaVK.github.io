using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Media.Animation;

namespace LunaVK.UC.LiveTiles
{
    public partial class LiveTileType3 : UserControl
    {
        public LiveTileType3()
        {
            InitializeComponent();
            Storyboard anim = (Storyboard)FindName("liveTileAnim1_Part1");
            anim.Begin();

        }

        private void liveTileAnim1_Part1_Completed_1(object sender, object e)
        {
            Storyboard anim = (Storyboard)FindName("liveTileAnim1_Part2");
            anim.Begin();
        }

        private void liveTileAnim1_Part2_Completed_1(object sender, object e)
        {
            Storyboard anim = (Storyboard)FindName("liveTileAnim2_Part1");
            anim.Begin();
        }
        private void liveTileAnim2_Part1_Completed_1(object sender, object e)
        {
            Storyboard anim = (Storyboard)FindName("liveTileAnim2_Part2");
            anim.Begin();
        }

        private void liveTileAnim2_Part2_Completed_1(object sender, object e)
        {
            Storyboard anim = (Storyboard)FindName("liveTileAnim1_Part1");
            anim.Begin();
        }
    }
}
