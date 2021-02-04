using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;
using LunaVK.Core.Enums;
using Windows.UI.Xaml.Media;
using LunaVK.Framework;

namespace LunaVK.ViewModels
{
    public class MenuItemViewModel : LunaVK.Core.ViewModels.ViewModelBase
    {
        public readonly MenuSectionName _attachedSection;

        private int _count = 0;
        public int Count
        {
            get
            {
                return this._count;
            }
            set
            {
                if (this._count == value)
                    return;

                this._count = value;

                base.NotifyPropertyChanged("CountVisibility");
                base.NotifyPropertyChanged("CountString");
            }
        }

        public MenuItemViewModel(MenuSectionName attachedSection)
        {
            this._attachedSection = attachedSection;
        }

        public Visibility CountVisibility
        {
            get
            {
                if (this.Count <= 0)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public string CountString
        {
            get { return this.Count.ToString(); }
        }
        
        public bool IsCurrent { get; private set; }

        /// <summary>
        /// Обновляет выделение пункта
        /// </summary>
        /// <param name="selectedSection"></param>
        public void UpdateSelectionState(MenuSectionName selectedSection)
        {
            this.IsCurrent = selectedSection == this._attachedSection;
            base.NotifyPropertyChanged("IsCurrent");
        }

        public string WideState { get; set; }

        public void UpdateWideState(CustomFrame.MenuStates new_state)
        {
            this.WideState = new_state.ToString();
            base.NotifyPropertyChanged("WideState");
        }
    }
}
