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

using LunaVK.Core.DataObjects;
using LunaVK.Core.Utils;

namespace LunaVK.UC.Attachment
{
    //VideoHeader
    public sealed partial class AttachVideoUC : UserControl, ThumbnailsLayoutHelper.IThumbnailSupport
    {
        private event TappedEventHandler _clicked;
        public event TappedEventHandler Clicked
        {
            add { this._clicked += value; }
            remove { this._clicked -= value; }
        }

        private VKVideoBase VM
        {
            get { return this.DataContext as VKVideoBase; }
        }

        public AttachVideoUC()
        {
            this.InitializeComponent();
        }
        
        public AttachVideoUC(VKVideoBase a) : this()
        {
            this.DataContext = a;
        }

#region IThumbnailSupport
        /// <summary>
        /// Данные для визуализации миниатюры.
        /// </summary>
        public ThumbnailsLayoutHelper.ThumbnailSize ThumbnailSize { get; set; }

        /// <summary>
        /// Ширина исходного изображения.
        /// </summary>
        double ThumbnailsLayoutHelper.IThumbnailSupport.Width { get { return this.VM.width; } }

        /// <summary>
        /// Высота исходного изображения.
        /// </summary>
        double ThumbnailsLayoutHelper.IThumbnailSupport.Height { get { return this.VM.height; } }

        /// <summary>
        /// Источник изображения миниатюры.
        /// </summary>
        public string ThumbnailSource
        {
            get { return ThumbnailSize.Width <= 130 ? this.VM.photo_130 : this.VM.photo_320; }
        }

        /// <summary>
        /// Возвращает соотношение ширины к высоте исходного изображения.
        /// </summary>
        public double GetRatio() { return (double)this.VM.width / (double)this.VM.height; }
#endregion


        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (this._clicked == null)
            {
                Grid grid = sender as Grid;
                Library.NavigatorImpl.Instance.NavigateToVideoWithComments(this.VM.owner_id, this.VM.id, this.VM.access_key, this.VM, grid.Children[0]);
            }
            else
            {
                this._clicked(this,e);
            }
        }
    }
}
