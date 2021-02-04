namespace LunaVK.Core.Library
{
    public interface ISupportReload
    {
        /// <summary>
        /// this._totalCount = null;
        /// this._nextFrom = null;
        /// this.Items.Clear();
        /// 
        /// this.LoadDownAsync(true);
        /// </summary>
        void Reload();
    }
}
