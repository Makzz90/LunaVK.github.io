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

using Windows.UI.Xaml.Media.Animation;
using LunaVK.Core.Utils;

namespace LunaVK.UC
{
    public sealed partial class NotificationsPanel : UserControl
    {
        public NotificationsPanel()
        {
            this.InitializeComponent();
        }

        public void AddAndShowNotification(string image_src, string title, string content, Action tapCallback = null)
        {
            //Ищем уже существующий элемент
            AppNotification2 exists = (AppNotification2)this.main_content.Children.FirstOrDefault((element) =>
            {
                var temp = element as AppNotification2;
                return temp.Title == title && temp.ToRemove == false;
            });

            //
            var value = UIStringFormatterHelper.CutTextGently(content, 60);
            if (value != content)
            {
                value += "...";
            }
            //

            if (exists == null)
            {
                if (this.main_content.Children.Count == 0)
                    this._backBorder.Animate(this._backBorder.Opacity, 1, "Opacity", 500);

                AppNotification2 notify = new AppNotification2(image_src, title, value, tapCallback);
                notify.TimeToDelete = (element) =>
                {
                    this.main_content.Children.Remove(element);

                    if (this.main_content.Children.Count == 0)
                        this._backBorder.Animate(this._backBorder.Opacity, 0, "Opacity", 300);
                };

                if (this.main_content.Children.Count >= 3)
                    this.main_content.Children.RemoveAt(0);

                this.main_content.Children.Add(notify);
            }
            else
            {
                exists.AddContent(value);
            }
        }

        public void Play()
        {
            this._mySong.Play();
        }
    }
}
