using Convnet.Properties;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Convnet.PageViews
{
    public partial class TrainPageView : UserControl
    {
        private bool Zoomout = false;
        private ScrollViewer ScrollViewerListViewTrainingResult = null;

        public TrainPageView()
        {
            InitializeComponent();
        }

        private void ListViewTrainingResult_LayoutUpdated(object sender, System.EventArgs e)
        {
            if (ScrollViewerListViewTrainingResult == null)
                ScrollViewerListViewTrainingResult = (VisualTreeHelper.GetChild(listViewTrainingResult, 0) as Decorator).Child as ScrollViewer;

            if (ScrollViewerListViewTrainingResult != null)
                ScrollViewerHeader.VerticalScrollBarVisibility = ScrollViewerListViewTrainingResult.ComputedVerticalScrollBarVisibility == Visibility.Visible ? ScrollBarVisibility.Visible : ScrollBarVisibility.Hidden;
        }

        private void ListViewTrainingResult_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewerHeader.ScrollToHorizontalOffset(e.HorizontalOffset);
        }

        private void ListViewTrainingResult_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && listViewTrainingResult.SelectedIndex > -1)
                Settings.Default.TrainingLog.RemoveAt(listViewTrainingResult.SelectedIndex);
        }

        private void ListViewTrainingResult_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listViewTrainingResult.SelectedIndex == (listViewTrainingResult.Items.Count - 1) && listViewTrainingResult.Items.Count > 0)
            {
                listViewTrainingResult.Dispatcher.Invoke(() =>
                {
                    listViewTrainingResult.SelectedItem = listViewTrainingResult.Items.GetItemAt(listViewTrainingResult.Items.Count - 1);
                    listViewTrainingResult.ScrollIntoView(listViewTrainingResult.SelectedItem);
                    ListViewItem item = listViewTrainingResult.ItemContainerGenerator.ContainerFromItem(listViewTrainingResult.SelectedItem) as ListViewItem;
                    item.BringIntoView();
                    item.Focus();
                }, System.Windows.Threading.DispatcherPriority.Render);
            }
        }

        private void SnapShot_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (!Zoomout)
                {
                    BorderSnapShot.SetValue(Grid.ColumnProperty, 0);
                    BorderSnapShot.SetValue(Grid.ColumnSpanProperty, 2);
                    BorderTrainingPlot.SetValue(Grid.ColumnProperty, 0);
                    BorderTrainingPlot.SetValue(Grid.ColumnSpanProperty, 2);

                    gridMain.RowDefinitions[0].Height = new GridLength(1.0, GridUnitType.Star);
                    gridMain.RowDefinitions[1].Height = new GridLength(0.0, GridUnitType.Pixel);
                    gridMain.RowDefinitions[2].Height = new GridLength(0.0, GridUnitType.Pixel);
                    Zoomout = true;
                }
                else
                {
                    BorderSnapShot.SetValue(Grid.ColumnProperty, 1);
                    BorderSnapShot.SetValue(Grid.ColumnSpanProperty, 1);
                    BorderTrainingPlot.SetValue(Grid.ColumnProperty, 1);
                    BorderTrainingPlot.SetValue(Grid.ColumnSpanProperty, 1);

                    gridMain.RowDefinitions[0].Height = new GridLength(1.0, GridUnitType.Auto);
                    gridMain.RowDefinitions[1].Height = new GridLength(20.0, GridUnitType.Pixel);
                    gridMain.RowDefinitions[2].Height = new GridLength(1.0, GridUnitType.Star);

                    BorderSnapShot.MaxHeight = 0.0;
                    BorderSnapShot.UpdateLayout();

                    Binding binding = new Binding
                    {
                        Path = new PropertyPath("ActualHeight"),
                        Source = BorderWeightsMinMax
                    };
                    BorderSnapShot.SetBinding(Border.MaxHeightProperty, binding);

                    BorderTrainingPlot.MaxHeight = 0.0;
                    BorderTrainingPlot.UpdateLayout();

                    binding = new Binding
                    {
                        Path = new PropertyPath("ActualHeight"),
                        Source = BorderWeightsMinMax
                    };
                    BorderTrainingPlot.SetBinding(Border.MaxHeightProperty, binding);

                    Zoomout = false;
                }

                e.Handled = true;
            }
        }

        private void TrainingPlot_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (!Zoomout)
                {
                    BorderSnapShot.SetValue(Grid.ColumnProperty, 0);
                    BorderSnapShot.SetValue(Grid.ColumnSpanProperty, 2);
                    BorderTrainingPlot.SetValue(Grid.ColumnProperty, 0);
                    BorderTrainingPlot.SetValue(Grid.ColumnSpanProperty, 2);

                    gridMain.RowDefinitions[0].Height = new GridLength(1.0, GridUnitType.Star);
                    gridMain.RowDefinitions[1].Height = new GridLength(0.0, GridUnitType.Pixel);
                    gridMain.RowDefinitions[2].Height = new GridLength(0.0, GridUnitType.Pixel);
                    Zoomout = true;
                }
                else
                {
                    BorderSnapShot.SetValue(Grid.ColumnProperty, 1);
                    BorderSnapShot.SetValue(Grid.ColumnSpanProperty, 1);
                    BorderTrainingPlot.SetValue(Grid.ColumnProperty, 1);
                    BorderTrainingPlot.SetValue(Grid.ColumnSpanProperty, 1);

                    gridMain.RowDefinitions[0].Height = new GridLength(1.0, GridUnitType.Auto);
                    gridMain.RowDefinitions[1].Height = new GridLength(20.0, GridUnitType.Pixel);
                    gridMain.RowDefinitions[2].Height = new GridLength(1.0, GridUnitType.Star);

                    BorderSnapShot.MaxHeight = 0.0;
                    BorderSnapShot.UpdateLayout();

                    Binding binding = new Binding
                    {
                        Path = new PropertyPath("ActualHeight"),
                        Source = BorderWeightsMinMax
                    };
                    BorderSnapShot.SetBinding(Border.MaxHeightProperty, binding);

                    BorderTrainingPlot.MaxHeight = 0.0;
                    BorderTrainingPlot.UpdateLayout();

                    binding = new Binding
                    {
                        Path = new PropertyPath("ActualHeight"),
                        Source = BorderWeightsMinMax
                    };
                    BorderTrainingPlot.SetBinding(Border.MaxHeightProperty, binding);
                    Zoomout = false;
                }

                e.Handled = true;
            }
        }
    }
}