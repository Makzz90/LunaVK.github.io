using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Linq.Expressions;
using LunaVK.Core.Framework;

namespace LunaVK.Core.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            if (propertyExpression.Body.NodeType != ExpressionType.MemberAccess)
                return;
            this.RaisePropertyChanged((propertyExpression.Body as MemberExpression).Member.Name);
        }

        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.RaisePropertyChanged(propertyName);
        }
        
        private void RaisePropertyChanged(string property)
        {
            if (this.PropertyChanged == null)
                return;
            //Надо вызывать на ветке интерфейса
            Execute.ExecuteOnUIThread(() =>
            {
                if (this.PropertyChanged == null)
                    return;//В оригинале есть эта перепроверка
                this.PropertyChanged(this, new PropertyChangedEventArgs(property));
            });
        }

        private bool _isInProgress;
        public bool IsInProgress
        {
            get
            {
                return this._isInProgress;
            }
            private set
            {
                this._isInProgress = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof( this.IsInProgressVisibility));
            }
        }

        public Visibility IsInProgressVisibility
        {
            get
            {
                return this.IsInProgress ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public void SetInProgress(bool isInProgress, string inProgressText = "")
        {
            this.IsInProgress = isInProgress;
            //this.InProgressText = this.IsInProgress ? inProgressText : "";
        }
    }
}
