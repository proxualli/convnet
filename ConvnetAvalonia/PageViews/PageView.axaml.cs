using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ConvnetAvalonia.PageViews
{
    public partial class PageView : UserControl
    {
        public PageView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}