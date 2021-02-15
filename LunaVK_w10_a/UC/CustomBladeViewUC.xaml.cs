using Windows.UI.Xaml.Controls;

namespace LunaVK.UC
{
	public sealed partial class CustomBladeViewUC : UserControl
	{
		public object MainContent { get; set; }
		public CustomBladeViewUC() => InitializeComponent();
	}
}
