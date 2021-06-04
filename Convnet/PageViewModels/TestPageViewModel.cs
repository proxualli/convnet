using Convnet.Common;
using Convnet.Dialogs;
using Convnet.Properties;
using dnncore;
using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Float = System.Single;
using UInt = System.UInt64;

namespace Convnet.PageViewModels
{
    public class TestPageViewModel : PageViewModelBase, IDisposable
    {
        private string progressText;
        private bool showProgress;
        private string label;
        private bool showSample;
        private DataTable confusionDataTable;
        private BitmapSource inputSnapShot;
        private readonly StringBuilder sb;
        private ComboBox dataProviderComboBox;
        private ComboBox costLayersComboBox;

        public Timer RefreshTimer;
        public event EventHandler Open;

        public DNNTrainingRate TestRate
        {
            get
            {
                if (Settings.Default.TestRate == null)
                    Settings.Default.TestRate = new DNNTrainingRate(DNNOptimizers.NAG, 0.9f, 0.0005f, 0.999f, 0.000001f, 128, 1, 200, 1, 0.005f, 0.00001f, 0.1f, 0.003f, 1, 1, false, false, 0, 0, 0, 0, 0, 0, DNNInterpolations.Cubic, 10, 12);
               

                return Settings.Default.TestRate;
            }
            private set
            {
                if (value == Settings.Default.TestRate)
                    return;

                Settings.Default.TestRate = value;
                OnPropertyChanged(nameof(TestRate));
            }
        }

        public TestPageViewModel(Model model) : base(model)
        {
            AddCommandButtons();

            sb = new StringBuilder();

            showProgress = false;
            showSample = false;
            if (Model != null)
                Model.TestProgress += TestProgress;
            Modelhanged += TestPageViewModel_ModelChanged;

            Application.Current.Dispatcher.Invoke(() => LayerIndexChanged(this, null), DispatcherPriority.Render);
        }

        private void AddCommandButtons()
        {
            Button startButton = new Button
            {
                Name = "ButtonStart",
                ToolTip = "Start Testing",
                Content = new BitmapToImage(Resources.Play),
                ClickMode = ClickMode.Release
            };
            startButton.Click += new RoutedEventHandler(StartButtonClick);

            Button stopButton = new Button
            {
                Name = "ButtonStop",
                ToolTip = "Stop Testing",
                Content = new BitmapToImage(Resources.Stop),
                ClickMode = ClickMode.Release,
                Visibility = Visibility.Collapsed
            };
            stopButton.Click += new RoutedEventHandler(StopButtonClick);
           
            Button pauseButton = new Button
            {
                Name = "ButtonPause",
                ToolTip = "Pause Testing",
                Content = new BitmapToImage(Resources.Pause),
                ClickMode = ClickMode.Release,
                Visibility = Visibility.Collapsed
            };
            pauseButton.Click += new RoutedEventHandler(PauseButtonClick);
          
            dataProviderComboBox = new ComboBox
            {
                Name = "ComboBoxDataSet",
                ItemsSource = Enum.GetValues(typeof(DNNDatasets)).Cast<Enum>().ToList(),
                SelectedIndex = (int)Dataset,
                ToolTip = "Dataset",
                IsEnabled = false
            };

            costLayersComboBox = new ComboBox
            {
                Name = "ComboBoxCostLayers"
            };
            costLayersComboBox.Items.Clear();
            for (uint layer = 0u; layer < Model.CostLayersCount; layer++)
            {
                ComboBoxItem item = new ComboBoxItem
                {
                    Name = "CostLayer" + layer.ToString(),
                    Content = Model.CostLayers[layer].Name,
                    Tag = layer
                };
                costLayersComboBox.Items.Add(item);
            }
            costLayersComboBox.ToolTip = "Cost Layer";
            costLayersComboBox.SelectedIndex = (int)Model.CostIndex;
            costLayersComboBox.SelectionChanged += CostLayersComboBox_SelectionChanged;
            costLayersComboBox.IsEnabled = Model.CostLayersCount > 1;

            CommandToolBar.Add(startButton);
            CommandToolBar.Add(stopButton);
            CommandToolBar.Add(pauseButton);
            CommandToolBar.Add(new Separator());
            CommandToolBar.Add(dataProviderComboBox);
            CommandToolBar.Add(costLayersComboBox);
        }

        public void CostLayersComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (costLayersComboBox.SelectedIndex >= 0)
            {
                uint costIndex = (uint)costLayersComboBox.SelectedIndex;
                Model.SetCostIndex(costIndex);
                if (Model.TaskState != DNNTaskStates.Running && ConfusionDataTable != null)
                {
                    Model.GetConfusionMatrix();
                    ConfusionDataTable = GetConfusionDataTable();
                    Model.UpdateCostInfo(costIndex);
                    sb.Length = 0;
                    sb.AppendFormat("Loss:\t\t{0:N7}\nErrors:\t{1:G}\nError:\t\t{2:N2} %\nAccuracy:\t{3:N2} %", Model.CostLayers[costIndex].AvgTestLoss, Model.CostLayers[costIndex].TestErrors, Model.CostLayers[costIndex].TestErrorPercentage, (Float)100 - Model.CostLayers[costIndex].TestErrorPercentage);
                    ProgressText = sb.ToString();
                }
            }
        }

        private void TestPageViewModel_ModelChanged(object sender, EventArgs e)
        {
            Model.TestProgress += TestProgress;
            ShowProgress = false;
            ShowSample = false;
            ConfusionDataTable = null;

            costLayersComboBox.Items.Clear();
            for (uint layer = 0u; layer < Model.CostLayersCount; layer++)
            {
                ComboBoxItem item = new ComboBoxItem
                {
                    Name = "CostLayer" + layer.ToString(),
                    Content = Model.CostLayers[layer].Name,
                    Tag = layer
                };
                costLayersComboBox.Items.Add(item);
            }
            costLayersComboBox.SelectedIndex = (int)Model.CostIndex;
            costLayersComboBox.IsEnabled = Model.CostLayersCount > 1;

            dataProviderComboBox.SelectedIndex = (int)Dataset;

            Application.Current.Dispatcher.Invoke(() => LayerIndexChanged(this, null), DispatcherPriority.Render);
        }

        void TestProgress(UInt BatchSize, UInt SampleIndex, Float AvgTestLoss, Float TestErrorPercentage, Float TestAccuracy, UInt TestErrors, DNNStates State, DNNTaskStates TaskState)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (State != DNNStates.Completed)
                {
                    sb.Length = 0;
                    sb.AppendFormat("Sample:\t\t{0:G}\nLoss:\t\t{1:N7}\nErrors:\t\t{2:G}\nError:\t\t{3:N2} %\nAccuracy:\t{4:N2} %", SampleIndex, AvgTestLoss, TestErrors, TestErrorPercentage, TestAccuracy);
                    ProgressText = sb.ToString();

                    Model.UpdateLayerInfo(0ul, true);
                    InputSnapShot = Model.InputSnapshot;
                    Label = Model.Label;
                }
                else
                {
                    Mouse.OverrideCursor = Cursors.Wait;

                    sb.Length = 0;
                    sb.AppendFormat("Loss:\t\t{0:N7}\nErrors:\t\t{1:G}\nError:\t\t{2:N2} %\nAccuracy:\t{3:N2} %", AvgTestLoss, TestErrors, TestErrorPercentage, TestAccuracy);
                    ProgressText = sb.ToString();
                    Mouse.OverrideCursor = Cursors.Wait;

                    RefreshTimer.Stop();
                    RefreshTimer.Elapsed -= new ElapsedEventHandler(RefreshTimer_Elapsed);
                    RefreshTimer.Dispose();

                    Model.Stop();
                    Model.SetCostIndex((uint)costLayersComboBox.SelectedIndex);
                    Model.GetConfusionMatrix();
                    ConfusionDataTable = GetConfusionDataTable();

                    CommandToolBar[0].ToolTip = "Start Testing";
                    CommandToolBar[0].Visibility = Visibility.Visible;
                    CommandToolBar[1].Visibility = Visibility.Collapsed;
                    CommandToolBar[2].Visibility = Visibility.Collapsed;

                    IsValid = true;
                    ShowSample = false;
                    Mouse.OverrideCursor = null;
                }
            }, DispatcherPriority.Render);
        }

        public DataTable ConfusionDataTable
        {
            get { return confusionDataTable; }
            set
            {
                if (value == confusionDataTable)
                    return;

                confusionDataTable = value;
                OnPropertyChanged(nameof(ConfusionDataTable));
            }
        }

        DataTable GetConfusionDataTable()
        {
            DataTable table = null;

            if (Model.ConfusionMatrix != null)
            {
                table = new DataTable("ConfusionTable");
                uint classCount = (uint)Model.ClassCount;
                uint labelIndex = (uint)Model.LabelIndex;

                table.BeginInit();
                table.Columns.Add("RowHeader", typeof(string));
                for (uint c = 0; c < classCount; c++)
                    table.Columns.Add(Model.LabelsCollection[labelIndex][c], typeof(uint));
                table.EndInit();

                table.BeginLoadData();

                for (uint r = 0; r < classCount; r++)
                {
                    DataRow row = table.NewRow();
                    row.BeginEdit();
                    object[] rowCollection = new object[classCount + 1];
                    rowCollection[0] = Model.LabelsCollection[labelIndex][r].ToString().Replace("_", "__");
                    for (uint c = 0; c < classCount; c++)
                        rowCollection[c + 1] = Model.ConfusionMatrix[r][c];
                    row.ItemArray = rowCollection;
                    row.EndEdit();
                    table.Rows.Add(row);
                }
                table.EndLoadData();
            }

            return table;
        }

        public bool ShowProgress
        {
            get { return showProgress; }
            set
            {
                if (value == showProgress)
                    return;

                showProgress = value;
                OnPropertyChanged(nameof(ShowProgress));
            }
        }

        public bool ShowSample
        {
            get { return showSample; }
            set
            {
                if (value == showSample)
                    return;

                showSample = value;

                OnPropertyChanged(nameof(ShowSample));
            }
        }

        public BitmapSource InputSnapShot
        {
            get { return inputSnapShot; }
            set
            {
                if (value == inputSnapShot)
                    return;

                inputSnapShot = value;
                OnPropertyChanged(nameof(InputSnapShot));
            }
        }

        public string Label
        {
            get { return label; }
            set
            {
                if (value == label)
                    return;

                label = value;
                OnPropertyChanged(nameof(Label));
            }
        }

        public override void Reset()
        {
            ProgressText = string.Empty;
            Label = string.Empty;
        }

        public string ProgressText
        {
            get
            {
                return progressText;
            }
            set
            {
                if (value == progressText)
                    return;

                progressText = value;
                OnPropertyChanged(nameof(ProgressText));
            }
        }

        public override string DisplayName => "Test";

        private void RefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() => LayerIndexChanged(sender, null), DispatcherPriority.Render);
        }

        private void LayerIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Model != null)
            {
                ShowSample = Model.TaskState == DNNTaskStates.Running;

                if (ShowSample)
                {
                    Model.UpdateLayerInfo(0ul, true);
                    InputSnapShot = Model.InputSnapshot;
                    Label = Model.Label;
                }
            }
        }

        private void StartButtonClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (Model.TaskState == DNNTaskStates.Running)
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("You must stop training first.", "Information", MessageBoxButton.OK);
                    return;
                }

                if (Model.TaskState == DNNTaskStates.Stopped)
                {
                    TestParameters dialog = new TestParameters
                    {
                        Owner = Application.Current.MainWindow,
                        Model = this.Model,
                        Path = DefinitionsDirectory,
                        IsEnabled = true,
                        Rate = TestRate
                    };

                    if (dialog.ShowDialog() ?? false)
                    {
                        IsValid = false;

                        TestRate = dialog.Rate;
                        Settings.Default.Save();

                        Model.AddLearningRate(true, 1, Model.TrainingSamples, new DNNTrainingRate(dialog.Rate.Optimizer, dialog.Rate.Momentum, dialog.Rate.L2Penalty, dialog.Rate.Beta2,dialog.Rate.Eps, dialog.Rate.BatchSize, 1, 1, dialog.Rate.EpochMultiplier, dialog.Rate.MaximumRate, dialog.Rate.MinimumRate, dialog.Rate.FinalRate, dialog.Rate.Gamma, dialog.Rate.DecayAfterEpochs, dialog.Rate.DecayFactor, dialog.Rate.HorizontalFlip, dialog.Rate.VerticalFlip, dialog.Rate.Dropout, dialog.Rate.Cutout, dialog.Rate.AutoAugment, dialog.Rate.ColorCast, dialog.Rate.ColorAngle, dialog.Rate.Distortion, dialog.Rate.Interpolation, dialog.Rate.Scaling, dialog.Rate.Rotation)); 
                        Model.SetCostIndex((uint)costLayersComboBox.SelectedIndex);
                        Model.Start(false);

                        RefreshTimer = new Timer(1000.0);
                        RefreshTimer.Elapsed += RefreshTimer_Elapsed;

                        CommandToolBar[0].Visibility = Visibility.Collapsed;
                        CommandToolBar[1].Visibility = Visibility.Visible;
                        CommandToolBar[2].Visibility = Visibility.Visible;

                        ShowProgress = true;
                        ShowSample = true;
                    }
                }
                else
                {
                    if (Model.TaskState == DNNTaskStates.Paused)
                    {
                        Model.Resume();

                        CommandToolBar[0].Visibility = Visibility.Collapsed;
                        CommandToolBar[1].Visibility = Visibility.Visible;
                        CommandToolBar[2].Visibility = Visibility.Visible;
                    }
                }
            }, DispatcherPriority.Normal);
        }

        private void StopButtonClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (Model.TaskState != DNNTaskStates.Stopped)
                {
                    if (Xceed.Wpf.Toolkit.MessageBox.Show("Do you really want stop?", "Stop Test", MessageBoxButton.YesNo, MessageBoxImage.None, MessageBoxResult.No) == MessageBoxResult.Yes)
                    {
                        Mouse.OverrideCursor = Cursors.Wait;

                        RefreshTimer.Stop();
                        RefreshTimer.Elapsed -= new ElapsedEventHandler(RefreshTimer_Elapsed);
                        RefreshTimer.Dispose();

                        Model.Stop();
                        ConfusionDataTable = null;

                        CommandToolBar[0].ToolTip = "Start Testing";
                        CommandToolBar[0].Visibility = Visibility.Visible;
                        CommandToolBar[1].Visibility = Visibility.Collapsed;
                        CommandToolBar[2].Visibility = Visibility.Collapsed;

                        IsValid = true;
                        ShowProgress = false;
                        ShowSample = false;
                        Mouse.OverrideCursor = null;
                    }
                }
            }, DispatcherPriority.Normal);
        }

        private void PauseButtonClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (Model.TaskState == DNNTaskStates.Running)
                {
                    Model.Pause();

                    CommandToolBar[0].ToolTip = "Resume Testing";
                    CommandToolBar[0].Visibility = Visibility.Visible;
                    CommandToolBar[1].Visibility = Visibility.Visible;
                    CommandToolBar[2].Visibility = Visibility.Collapsed;
                }
            }, DispatcherPriority.Normal);
        }

        private void OpenButtonClick(object sender, RoutedEventArgs e)
        {
            Open?.Invoke(this, EventArgs.Empty);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    RefreshTimer.Dispose();
                    confusionDataTable.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~TestPageViewModel()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
