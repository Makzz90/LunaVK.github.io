using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaVK.Photo.ViewModels
{
    public class BlurEffectViewModel// : INotifyPropertyChanged
    {

        public BlurEffectViewModel(double min, double max)
        {
            this._min = min;
            this._max = max;
        }
        private double _min;
        private double _max;

        public double Min
        {
            get { return this._min; }
        }

        public double Max
        {
            get { return this._max; }
        }

        private double _amount;
        public double Amount
        {
            get { return this._amount; }
            set
            {
                this._amount = value;
                //this.RaisePropertyChanged(nameof(this.Amount));
                this.PropertyChanged?.Invoke();
            }
        }

        //public event PropertyChangedEventHandler PropertyChanged;

        public Action PropertyChanged;
            /*
        private void RaisePropertyChanged(string property)
        {
            if (this.PropertyChanged == null)
                return;
            //Надо вызывать на ветке интерфейса
            //Execute.ExecuteOnUIThread(() =>
            //{
                if (this.PropertyChanged == null)
                    return;//В оригинале есть эта перепроверка
                this.PropertyChanged(this, new PropertyChangedEventArgs(property));
            //});
        }*/
    }
}
