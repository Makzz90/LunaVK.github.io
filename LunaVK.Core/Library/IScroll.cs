namespace LunaVK.Core.Library
{
    public interface IScroll
    {
        //bool IsHorizontalOrientation { get; }

        double VerticalOffset { get; }

        //bool IsManipulating { get; }

        void ScrollToBottom(bool animated = true, bool onlyIfInTheBottom = false);

        void ScrollToUnreadItem();

        void ScrollToMessageId(uint messageId);

        void ScrollToOffset(double offset);
    }
}
