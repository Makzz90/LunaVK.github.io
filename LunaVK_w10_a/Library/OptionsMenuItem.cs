using System;

namespace LunaVK.Library
{
    public class OptionsMenuItem
    {
        public string Icon { get; set; }

        /// <summary>
        /// При нажатии на элемент интерфейса
        /// </summary>
        public Action<object> Clicked;
    }
}
