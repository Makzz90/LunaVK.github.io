using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using System.Windows.Input;

namespace LunaVK.Framework
{
    /// <summary>
    /// Кнопка панели приложения.
    /// </summary>
    public class AppBarButton
    {
        public string Label { get;set;}
        public string Icon { get; set; }
        public ICommand Command { get; set; }
    }
}
