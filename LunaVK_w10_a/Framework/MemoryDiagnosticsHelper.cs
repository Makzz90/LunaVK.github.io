using System;
using System.Collections.Generic;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace LunaVK.Framework
{
    public static class MemoryDiagnosticsHelper
    {
        //private static int lastSafetyBand = -1;
        //private static bool alreadyFailedPeak = false;
        private static Popup popup;
        private static TextBlock currentMemoryBlock;
        private static TextBlock peakMemoryBlock;
        private static DispatcherTimer timer;
        private static bool forceGc;
        private const long MAX_MEMORY = 209715200;

        /*
        private const long MAX_CHECKPOINTS = 10;
        private static Queue<MemoryCheckpoint> recentCheckpoints;

        public static IEnumerable<MemoryCheckpoint> RecentCheckpoints
        {
            get
            {
                if (MemoryDiagnosticsHelper.recentCheckpoints != null)
                {
                    foreach (MemoryCheckpoint recentCheckpoint in MemoryDiagnosticsHelper.recentCheckpoints)
                        yield return recentCheckpoint;
                }
            }
        }
        */
        public static void Start(TimeSpan timespan, bool forceGc)
        {
            if (MemoryDiagnosticsHelper.timer != null)
                throw new InvalidOperationException("Diagnostics already running");
            MemoryDiagnosticsHelper.forceGc = forceGc;
            //MemoryDiagnosticsHelper.recentCheckpoints = new Queue<MemoryCheckpoint>();
            MemoryDiagnosticsHelper.StartTimer(timespan);
            MemoryDiagnosticsHelper.ShowPopup();
        }

        public static void Stop()
        {
            MemoryDiagnosticsHelper.HidePopup();
            MemoryDiagnosticsHelper.StopTimer();
            //MemoryDiagnosticsHelper.recentCheckpoints = null;
        }
        /*
        public static void Checkpoint(string text)
        {
            if (MemoryDiagnosticsHelper.recentCheckpoints == null)
                return;
            if (MemoryDiagnosticsHelper.recentCheckpoints.Count >= 9)
                MemoryDiagnosticsHelper.recentCheckpoints.Dequeue();
            MemoryDiagnosticsHelper.recentCheckpoints.Enqueue(new MemoryCheckpoint(text, MemoryDiagnosticsHelper.GetCurrentMemoryUsage()));
        }
        */
        public static ulong GetCurrentMemoryUsage()
        {
            return MemoryManager.AppMemoryUsage;
        }
        /*
        public static long GetPeakMemoryUsage
        {
            get { return 0; }
        }
        */
        private static void ShowPopup()
        {
            MemoryDiagnosticsHelper.popup = new Popup() { Height = 20, Margin = new Thickness(100,0,0,0) };
            double num1 = 12;
            Brush brush1 = new SolidColorBrush(Colors.Black);

            StackPanel stackPanel1 = new StackPanel();
            stackPanel1.Orientation = Orientation.Horizontal;
            stackPanel1.Background = new SolidColorBrush(Colors.White);
            stackPanel1.IsHitTestVisible = false;
            TextBlock textBlock1 = new TextBlock();
            textBlock1.Text = "---";
            textBlock1.FontSize = num1;
            textBlock1.Foreground = brush1;
            textBlock1.Margin = new Thickness(5.0, 0.0, 5.0, 0.0);
            MemoryDiagnosticsHelper.currentMemoryBlock = textBlock1;

            TextBlock textBlock2 = new TextBlock();
            textBlock2.Text = "";
            textBlock2.FontSize = num1;
            textBlock2.Foreground = brush1;
            
            MemoryDiagnosticsHelper.peakMemoryBlock = textBlock2;

            stackPanel1.Children.Add(MemoryDiagnosticsHelper.currentMemoryBlock);
            TextBlock textBlock3 = new TextBlock();
            textBlock3.Text = " kb";
            textBlock3.FontSize = num1;
            textBlock3.Foreground = brush1;
            textBlock3.Margin = new Thickness(0, 0, 5, 0);
            stackPanel1.Children.Add(textBlock3);
            stackPanel1.Children.Add(MemoryDiagnosticsHelper.peakMemoryBlock);

            //CompositeTransform compositeTransform = new CompositeTransform();
            //compositeTransform.Rotation = 90;
            //compositeTransform.TranslateX = 50;
            //compositeTransform.TranslateY = 60;
            //stackPanel1.RenderTransform = compositeTransform;
            MemoryDiagnosticsHelper.popup.Child = stackPanel1;
            MemoryDiagnosticsHelper.popup.IsHitTestVisible = false;
            MemoryDiagnosticsHelper.popup.IsOpen = true;
        }

        private static void StartTimer(TimeSpan timespan)
        {
            MemoryDiagnosticsHelper.timer = new DispatcherTimer();
            MemoryDiagnosticsHelper.timer.Interval = timespan;
            MemoryDiagnosticsHelper.timer.Tick += MemoryDiagnosticsHelper.timer_Tick;
            MemoryDiagnosticsHelper.timer.Start();
        }

        private static void timer_Tick(object sender, object e)
        {
            if (MemoryDiagnosticsHelper.forceGc)
                GC.Collect();
            MemoryDiagnosticsHelper.UpdateCurrentMemoryUsage();
            //MemoryDiagnosticsHelper.UpdatePeakMemoryUsage();
        }

        /*
        private static void UpdatePeakMemoryUsage()
        {
            if (MemoryDiagnosticsHelper.alreadyFailedPeak || MemoryDiagnosticsHelper.GetPeakMemoryUsage < MAX_MEMORY)
                return;
            MemoryDiagnosticsHelper.alreadyFailedPeak = true;
            MemoryDiagnosticsHelper.Checkpoint("*MEMORY USAGE FAIL*");
            MemoryDiagnosticsHelper.peakMemoryBlock.Text = "FAIL!";
            MemoryDiagnosticsHelper.peakMemoryBlock.Foreground = new SolidColorBrush(Colors.Red);
        }
        */
        private static void UpdateCurrentMemoryUsage()
        {
            ulong currentMemoryUsage = MemoryDiagnosticsHelper.GetCurrentMemoryUsage();
            MemoryDiagnosticsHelper.currentMemoryBlock.Text = string.Format("{0:N}", (currentMemoryUsage / 1024));
            //int safetyBand = MemoryDiagnosticsHelper.GetSafetyBand(currentMemoryUsage);
            //if (safetyBand == MemoryDiagnosticsHelper.lastSafetyBand)
            //    return;
            //MemoryDiagnosticsHelper.currentMemoryBlock.Foreground = MemoryDiagnosticsHelper.GetBrushForSafetyBand(safetyBand);
            //MemoryDiagnosticsHelper.lastSafetyBand = safetyBand;
            MemoryDiagnosticsHelper.currentMemoryBlock.Foreground = MemoryDiagnosticsHelper.GetBrushForMemoryUsage(currentMemoryUsage);
        }

        private static Brush GetBrushForMemoryUsage(ulong mem)
        {

            double percent = (double)mem / (double)MemoryDiagnosticsHelper.MAX_MEMORY;

            double red = byte.MaxValue * percent;
            double green = byte.MaxValue * (1 - percent);
            byte blue = 0;

            if(percent>1)
            {
                red = 128;
                green = 0;
                blue = 56;
                
                MemoryDiagnosticsHelper.peakMemoryBlock.Text = "*MEMORY USAGE FAIL*";
            }
            else
            {
                if (!string.IsNullOrEmpty(MemoryDiagnosticsHelper.peakMemoryBlock.Text))
                    MemoryDiagnosticsHelper.peakMemoryBlock.Text = "";
            }

            return new SolidColorBrush(Color.FromArgb(byte.MaxValue, (byte)(red), (byte)(green), blue));
        }
        /*
        private static Brush GetBrushForSafetyBand(int safetyBand)
        {
            if (safetyBand == 0)
                return new SolidColorBrush(Colors.Green);
            if (safetyBand == 1)
                return new SolidColorBrush(Colors.Orange);
            return new SolidColorBrush(Colors.Red);
        }
        
        private static int GetSafetyBand(long mem)
        {
            double num = mem / MAX_MEMORY;
            if (num <= 0.75)
                return 0;
            return num <= 0.9 ? 1 : 2;
        }
        */
        private static void StopTimer()
        {
            MemoryDiagnosticsHelper.timer.Stop();
            MemoryDiagnosticsHelper.timer = null;
        }

        private static void HidePopup()
        {
            MemoryDiagnosticsHelper.popup.IsOpen = false;
            MemoryDiagnosticsHelper.popup = null;
        }

        public static bool IsLowMemDevice
        {
            get { return MemoryManager.AppMemoryUsageLimit < 200000000; }
        }


        public class MemoryCheckpoint
        {
            public string Text { get; private set; }

            public long MemoryUsage { get; private set; }

            internal MemoryCheckpoint(string text, long memoryUsage)
            {
                this.Text = text;
                this.MemoryUsage = memoryUsage;
            }
        }
    }
}