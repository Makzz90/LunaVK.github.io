using System.Threading.Tasks;

namespace LunaVK.Core.Library
{
    public interface ISupportLoadMore
    {
        Task LoadData(bool reload = false);

        bool HasMoreItems { get; }
    }

    public interface ISupportLoadBack
    {
        Task LoadDataBack();

        bool HasBackItems { get; }
    }
}
