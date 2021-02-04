using LunaVK.Core.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaVK.ViewModels
{
    public abstract class GenericSearchViewModelBase<T> : GenericCollectionViewModel<T>
         where T : class
    {
        public abstract string SearchString { get; set; }
    }
}
