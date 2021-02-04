using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;

namespace LunaVK.Photo
{
    public class SlideMenuItemBase
    {
        /// <summary>
        /// Название
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Ссылка на иконку
        /// </summary>
        public string IconPath { get; set; }

        public Action ClickCommand { get; set; }

        public DataTemplate SecondaryControlDataTemplate { get; set; }

        public Func<object> GetDataContextFunc;
    }
}
