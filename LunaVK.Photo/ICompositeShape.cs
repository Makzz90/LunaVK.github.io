using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LunaVK.Photo
{
    public interface ICompositeShape
    {
        FrameworkElement Control { get; }

        Action<ICompositeShape> OnTap { get; set; }

        /// <summary>
        /// AdornerElementBaseUC
        /// </summary>
        Action<ICompositeShape> OnDelete { get; set; }

        bool IsSelected { get; set; }
    }
}
