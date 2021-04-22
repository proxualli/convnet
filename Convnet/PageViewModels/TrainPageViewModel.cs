using Convnet.Common;
using Convnet.Dialogs;
using Convnet.Properties;
using dnncore;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Float = System.Single;
using UInt = System.UInt64;

namespace Convnet.PageViewModels
{
    [Serializable]
    public enum PlotType
    {
        Accuracy = 0,
        Error = 1,
        Loss = 2
    };

    public class TrainPageViewModel : PageViewModelBase
    {
        private string progressText;
        private bool showProgress;
        private string layerInfo;
        private string weightsMinMax;
        private string label;
        private bool showSample;
        private ObservableCollection<DNNTrainingRate> trainRates;
        private int selectedIndex = -1;
        private bool sgdr;
        private uint gotoCycle = 1;
        private uint gotoEpoch = 1;
        private int selectedCostIndex = 0;
        private ComboBox optimizerComboBox;
        private ComboBox costLayersComboBox;
        private ComboBox dataProviderComboBox;
        private ComboBox layersComboBox;
        private Button refreshButton;
        private ComboBox plotTypeComboBox;
        private CheckBox disableLockingCheckBox;
        private Button unlockAllButton;
        private Button lockAllButton;
        private CheckBox trainingPlotCheckBox;
        private FormattedSlider pixelSizeSlider;
        private DNNOptimizers optimizer;
        private int? refreshRate;
        private int weightsSnapshotX;
        private int weightsSnapshotY;
        private bool showWeights;
        private bool showWeightsSnapshot;
        private bool? showTrainingPlot;
        private ObservableCollection<DataPoint> pointsTrain;
        private ObservableCollection<DataPoint> pointsTest;
        private string pointsTrainLabel;
        private string pointsTestLabel;
        private PlotType currentPlotType;
        private LegendPosition currentLegendPosition;
        private BitmapSource weightsSnapshot;
        private BitmapSource inputSnapshot;
        private readonly StringBuilder sb;

        public Timer RefreshTimer;
        public TimeSpan EpochDuration { get; set; }
        public event EventHandler Open;
        public event EventHandler Save;
        public event EventHandler<int?> RefreshRateChanged;
       

        public TrainPageViewModel(dnncore.Model model) : base(model)
        {
            sb = new StringBuilder();
            refreshRate = Settings.Default.RefreshInterval;

            if (Model != null)
            {
                Model.NewEpoch += NewEpoch;
                Model.TrainProgress += TrainProgress;
            }

            showProgress = false;
            showSample = false;
            showWeights = false;
            showWeightsSnapshot = false;

            sgdr = Settings.Default.SGDR;
            gotoCycle = Settings.Default.GotoCycle;
            gotoEpoch = Settings.Default.GotoEpoch;
            showTrainingPlot = Settings.Default.ShowTrainingPlot;
            currentPlotType = (PlotType)Settings.Default.PlotType;
            currentLegendPosition = currentPlotType == PlotType.Accuracy ? LegendPosition.BottomRight : LegendPosition.TopRight;
            optimizer = (DNNOptimizers)Settings.Default.Optimizer;

            AddCommandButtons();

            Modelhanged += TrainPageViewModel_ModelChanged;
            RefreshRateChanged += TrainPageViewModel_RefreshRateChanged;

            (UIElementAutomationPeer.CreatePeerForElement(refreshButton).GetPattern(PatternInterface.Invoke) as IInvokeProvider).Invoke();
        }

        private void TrainPageViewModel_RefreshRateChanged(object sender, int? e)
        {
            if (RefreshTimer != null && e.HasValue)
               RefreshTimer.Interval = 1000 * e.Value;
        }

        private void TrainPageViewModel_ModelChanged(object sender, EventArgs e)
        {
            showProgress = false;
            showSample = false;
            showWeights = false;
            showWeightsSnapshot = false;
            gotoCycle = Settings.Default.GotoCycle;
            gotoEpoch = Settings.Default.GotoEpoch;
            showTrainingPlot = Settings.Default.ShowTrainingPlot;
            currentPlotType = (PlotType)Settings.Default.PlotType;
            currentLegendPosition = currentPlotType == PlotType.Accuracy ? LegendPosition.BottomRight : LegendPosition.TopRight;

            Model.NewEpoch += NewEpoch;
            Model.TrainProgress += TrainProgress;

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
            selectedCostIndex = costLayersComboBox.SelectedIndex;
            costLayersComboBox.IsEnabled = Model.CostLayersCount > 1;

            layersComboBox.ItemsSource = Model.Layers;
            layersComboBox.SelectedIndex = 0;
            Model.SelectedIndex = 0;
            Settings.Default.SelectedLayer = 0;
            Settings.Default.Save();
            dataProviderComboBox.SelectedIndex = (int)Dataset;
           

            Application.Current.Dispatcher.Invoke(() => LayersComboBox_SelectionChanged(sender, null), DispatcherPriority.Render);

            Mouse.OverrideCursor = null;
            if (TrainingLog.Count > 0)
                if (Xceed.Wpf.Toolkit.MessageBox.Show("Do you want to clear the training log?", "Clear log?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    TrainingLog.Clear();

            Application.Current.Dispatcher.Invoke(() => RefreshTrainingPlot(), DispatcherPriority.Render);
        }

        private void NewEpoch(UInt Cycle, UInt Epoch, UInt TotalEpochs, UInt Optimizer, Float Beta2, Float Eps, bool HorizontalFlip, bool VerticalFlip, Float Dropout, Float Cutout, Float AutoAugment, Float ColorCast, UInt ColorAngle, Float Distortion, UInt Interpolation, Float Scaling, Float Rotation, Float Rate, UInt64 BatchSize, Float Momentum, Float L2Penalty, Float AvgTrainLoss, Float TrainErrorPercentage, Float TrainAccuracy, UInt TrainErrors, Float AvgTestLoss, Float TestErrorPercentage, Float TestAccuracy, UInt TestErrors)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                TimeSpan span = Model.Duration.Elapsed.Subtract(EpochDuration);
                EpochDuration = Model.Duration.Elapsed;
                for (uint c = 0; c < Model.CostLayersCount; c++)
                {
                    Model.UpdateCostInfo(c);
                    TrainingLog.Add(new DNNTrainingResult(Cycle, Epoch, c, Model.CostLayers[c].GroupIndex, Model.CostLayers[c].Name, (DNNOptimizers)Optimizer, Momentum, Beta2, L2Penalty, Eps, Rate, BatchSize, Dropout, Cutout, AutoAugment, ColorCast, ColorAngle, Distortion, (DNNInterpolation)Interpolation, Scaling, Rotation, HorizontalFlip, VerticalFlip, Model.CostLayers[c].AvgTrainLoss, Model.CostLayers[c].TrainErrors, Model.CostLayers[c].TrainErrorPercentage, Model.CostLayers[c].TrainAccuracy, Model.CostLayers[c].AvgTestLoss, Model.CostLayers[c].TestErrors, Model.CostLayers[c].TestErrorPercentage, Model.CostLayers[c].TestAccuracy, span.Ticks));
                }

                SelectedIndex = TrainingLog.Count - 1;
              
                RefreshTrainingPlot();
            }, DispatcherPriority.Render);
        }

        private void TrainProgress(DNNOptimizers Optim, UInt BatchSize, UInt Cycle, UInt TotalCycles, UInt Epoch, UInt TotalEpochs, bool HorizontalFlip, bool VerticalFlip, Float Dropout, Float Cutout, Float AutoAugment, Float ColorCast, UInt ColorAngle, Float Distortion, DNNInterpolation Interpolation, Float Scaling, Float Rotation, UInt SampleIndex, Float Rate, Float Momentum, Float L2Penalty, Float AvgTrainLoss, Float TrainErrorPercentage, Float TrainAccuracy, UInt TrainErrors, Float AvgTestLoss, Float TestErrorPercentage, Float TestAccuracy, UInt TestErrors, DNNStates State, DNNTaskStates TaskState)
        {
            sb.Length = 0;
            switch (State)
            {
                case DNNStates.Training:
                    {
                        if (Optimizer != Optim)
                        {
                            Optimizer = Optim;
                            Model.Optimizer = Optim;
                        }

                        sb.Append("<Span><Bold>Training</Bold></Span><LineBreak/>");
                        sb.Append("<Span>");
                        switch (Model.Optimizer)
                        {
                            case DNNOptimizers.AdaGrad:
                                sb.AppendFormat(" Sample:\t\t{0:G}\n Cycle:\t\t\t{1}/{2}\n Epoch:\t\t\t{3}/{4}\n Batch Size:\t\t{5:G}\n Rate:\t\t\t{6:0.#######}\n Dropout:\t\t" + (Dropout > 0 ? Dropout.ToString() + "\n" : "No\n") + " Cutout:\t\t" + (Cutout > 0 ? Cutout.ToString() + "\n" : "No\n") + " Auto Augment:\t\t" + (AutoAugment > 0 ? AutoAugment.ToString() + "\n" : "No\n") + (HorizontalFlip ? " Horizontal Flip:\tYes\n" : " Horizontal Flip:\tNo\n") + (VerticalFlip ? " Vertical Flip:\tYes\n" : " Vertical Flip:\tNo\n") + " Color Cast:\t\t" + (ColorCast > 0u ? ColorCast.ToString() + "\n" : "No\n") + " Distortion:\t\t" + (Model.Distortion > 0 ? Distortion.ToString() + "\n" : "No\n") + " Loss:\t\t\t{7:N7}\n Errors:\t\t{8:G}\n Error:\t\t\t{9:N2} %\n Accuracy:\t\t{10:N2} %", SampleIndex, Cycle, TotalCycles, Epoch, TotalEpochs, Model.BatchSize, Rate, AvgTrainLoss, TrainErrors, TrainErrorPercentage, 100 - TrainErrorPercentage);
                                break;

                            case DNNOptimizers.AdaDelta:
                            case DNNOptimizers.Adam:
                            case DNNOptimizers.RMSProp:
                            case DNNOptimizers.Adamax:
                                sb.AppendFormat(" Sample:\t\t{0:G}\n Cycle:\t\t\t{1}/{2}\n Epoch:\t\t\t{3}/{4}\n Batch Size:\t\t{5:G}\n Rate:\t\t\t{6:0.#######}\n Momentum:\t\t{7:0.#######}\n Dropout:\t\t" + (Dropout > 0 ? Dropout.ToString() + "\n" : "No\n") + " Cutout:\t\t" + (Cutout > 0 ? Cutout.ToString() + "\n" : "No\n") + " Auto Augment:\t\t" + (AutoAugment > 0 ? AutoAugment.ToString() + "\n" : "No\n") + (HorizontalFlip ? " Horizontal Flip:\tYes\n" : " Horizontal Flip:\tNo\n") + (VerticalFlip ? " Vertical Flip:\tYes\n" : " Vertical Flip:\tNo\n") + " Color Cast:\t\t" + (ColorCast > 0u ? ColorCast.ToString() + "\n" : "No\n") + " Distortion:\t\t" + (Model.Distortion > 0 ? Distortion.ToString() + "\n" : "No\n") + " Loss:\t\t\t{8:N7}\n Errors:\t\t{9:G}\n Error:\t\t\t{10:N2} %\n Accuracy:\t\t{11:N2} %", SampleIndex, Cycle, TotalCycles, Epoch, TotalEpochs, Model.BatchSize, Rate, Momentum, AvgTrainLoss, TrainErrors, TrainErrorPercentage, 100 - TrainErrorPercentage);
                                break;

                            case DNNOptimizers.SGD:
                                sb.AppendFormat(" Sample:\t\t{0:G}\n Cycle:\t\t\t{1}/{2}\n Epoch:\t\t\t{3}/{4}\n Batch Size:\t\t{5:G}\n Rate:\t\t\t{6:0.#######}\n L2 Penalty:\t\t{7:0.#######}\n Dropout:\t\t" + (Dropout > 0 ? Dropout.ToString() + "\n" : "No\n") + " Cutout:\t\t" + (Cutout > 0 ? Cutout.ToString() + "\n" : "No\n") + " Auto Augment:\t\t" + (AutoAugment > 0 ? AutoAugment.ToString() + "\n" : "No\n") + (HorizontalFlip ? " Horizontal Flip:\tYes\n" : " Horizontal Flip:\tNo\n") + (VerticalFlip ? " Vertical Flip:\tYes\n" : " Vertical Flip:\tNo\n") + " Color Cast:\t\t" + (ColorCast > 0u ? ColorCast.ToString() + "\n" : "No\n") + " Distortion:\t\t" + (Model.Distortion > 0 ? Distortion.ToString() + "\n" : "No\n") + " Loss:\t\t\t{8:N7}\n Errors:\t\t{9:G}\n Error:\t\t\t{10:N2} %\n Accuracy:\t\t{11:N2} %", SampleIndex, Cycle, TotalCycles, Epoch, TotalEpochs, Model.BatchSize, Rate, L2Penalty, AvgTrainLoss, TrainErrors, TrainErrorPercentage, 100 - TrainErrorPercentage);
                                break;

                            case DNNOptimizers.NAG:
                            case DNNOptimizers.SGDMomentum:
                                sb.AppendFormat(" Sample:\t\t{0:G}\n Cycle:\t\t\t{1}/{2}\n Epoch:\t\t\t{3}/{4}\n Batch Size:\t\t{5:G}\n Rate:\t\t\t{6:0.#######}\n Momentum:\t\t{7:0.#######}\n L2 Penalty:\t\t{8:0.#######}\n Dropout:\t\t" + (Dropout > 0 ? Dropout.ToString() + "\n" : "No\n") + " Cutout:\t\t" + (Cutout > 0 ? Cutout.ToString() + "\n" : "No\n") + " Auto Augment:\t\t" + (AutoAugment > 0 ? AutoAugment.ToString() + "\n" : "No\n") + (HorizontalFlip ? " Horizontal Flip:\tYes\n" : " Horizontal Flip:\tNo\n") + (VerticalFlip ? " Vertical Flip:\tYes\n" : " Vertical Flip:\tNo\n") + " Color Cast:\t\t" + (ColorCast > 0u ? ColorCast.ToString() + "\n" : "No\n") + " Distortion:\t\t" + (Model.Distortion > 0 ? Distortion.ToString() + "\n" : "No\n") + " Loss:\t\t\t{9:N7}\n Errors:\t\t{10:G}\n Error:\t\t\t{11:N2} %\n Accuracy:\t\t{12:N2} %", SampleIndex, Cycle, TotalCycles, Epoch, TotalEpochs, Model.BatchSize, Rate, Momentum, L2Penalty, AvgTrainLoss, TrainErrors, TrainErrorPercentage, 100 - TrainErrorPercentage);
                                break;
                        }
                        sb.Append("</Span>");
                    }
                    break;

                case DNNStates.Testing:
                    {
                        sb.Append("<Span><Bold>Testing</Bold></Span><LineBreak/>");
                        sb.Append("<Span>");
                        sb.AppendFormat(" Sample:\t\t{0:G}\n Loss:\t\t\t{1:N7}\n Errors:\t\t{2:G}\n Error:\t\t\t{3:N2} %\n Accuracy:\t\t{4:N2} %", SampleIndex, AvgTestLoss, TestErrors, TestErrorPercentage, (Float)100 - TestErrorPercentage);
                        sb.Append("</Span>");
                    }
                    break;

                case DNNStates.SaveWeights:
                    sb.Append("<Span><Bold>Saving weights</Bold></Span>");
                    break;

                case DNNStates.Completed:
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            RefreshTimer.Stop();
                            RefreshTimer.Elapsed -= new ElapsedEventHandler(RefreshTimer_Elapsed);
                            RefreshTimer.Dispose();
                            Mouse.OverrideCursor = Cursors.Wait;
                            Model.Stop();

                            CommandToolBar[0].ToolTip = "Start Training";
                            CommandToolBar[0].Visibility = Visibility.Visible;
                            CommandToolBar[1].Visibility = Visibility.Collapsed;
                            CommandToolBar[2].Visibility = Visibility.Collapsed;

                            CommandToolBar[5].Visibility = Visibility.Visible;
                            CommandToolBar[6].Visibility = Visibility.Visible;
                            CommandToolBar[7].Visibility = Visibility.Visible;
                            CommandToolBar[13].Visibility = Visibility.Visible;

                            if (Model.Layers[layersComboBox.SelectedIndex].HasWeights || Model.Layers[layersComboBox.SelectedIndex].IsNormalizationLayer)
                            {
                                CommandToolBar[16].Visibility = !Settings.Default.DisableLocking ? Visibility.Visible : Visibility.Collapsed;
                                CommandToolBar[17].Visibility = !Settings.Default.DisableLocking ? Visibility.Visible : Visibility.Collapsed;
                                CommandToolBar[18].Visibility = Visibility.Visible;
                                CommandToolBar[19].Visibility = Visibility.Visible;
                            }
                            else
                            {
                                CommandToolBar[16].Visibility = Visibility.Collapsed;
                                CommandToolBar[17].Visibility = Visibility.Collapsed;
                                CommandToolBar[18].Visibility = Visibility.Collapsed;
                                CommandToolBar[19].Visibility = Visibility.Collapsed;
                            }

                            ShowProgress = false;

                            Mouse.OverrideCursor = null;
                        }, DispatcherPriority.Render);
                    }
                    break;
            }
            ProgressText = sb.ToString();
        }

        private void AddCommandButtons()
        {
            Button startButton = new Button
            {
                Name = "ButtonStart",
                ToolTip = "Start Training",
                Content = new BitmapToImage(Resources.Play),
                ClickMode = ClickMode.Release
            };
            startButton.Click += new RoutedEventHandler(StartButtonClick);

            Button stopButton = new Button
            {
                Name = "ButtonStop",
                ToolTip = "Stop Training",
                Content = new BitmapToImage(Resources.Stop),
                ClickMode = ClickMode.Release
            };
            stopButton.Click += new RoutedEventHandler(StopButtonClick);
            stopButton.Visibility = Visibility.Collapsed;

            Button pauseButton = new Button
            {
                Name = "ButtonPause",
                ToolTip = "Pause Training",
                Content = new BitmapToImage(Resources.Pause),
                ClickMode = ClickMode.Release
            };
            pauseButton.Click += new RoutedEventHandler(PauseButtonClick);
            pauseButton.Visibility = Visibility.Collapsed;

            Button editorButton = new Button
            {
                Name = "ButtonEditor",
                ToolTip = "Training Scheme Editor",
                Content = new BitmapToImage(Resources.Collection),
                ClickMode = ClickMode.Release
            };
            editorButton.Click += new RoutedEventHandler(EditorButtonClick);

            Button openButton = new Button
            {
                Name = "ButtonOpen",
                ToolTip = "Load Model Weights",
                Content = new BitmapToImage(Resources.Open),
                ClickMode = ClickMode.Release
            };
            openButton.Click += new RoutedEventHandler(OpenButtonClick);

            Button saveButton = new Button
            {
                Name = "ButtonSave",
                ToolTip = "Save Model Weights",
                Content = new BitmapToImage(Resources.Save),
                ClickMode = ClickMode.Release
            };
            saveButton.Click += new RoutedEventHandler(SaveButtonClick);

            Button forgetButton = new Button
            {
                Name = "ButtonForgetWeights",
                ToolTip = "Forget Model Weights",
                Content = new BitmapToImage(Resources.Bolt),
                ClickMode = ClickMode.Release
            };
            forgetButton.Click += new RoutedEventHandler(ForgetButtonClick);

            Button clearButton = new Button
            {
                Name = "ButtonClearLog",
                ToolTip = "Clear Log",
                Content = new BitmapToImage(Resources.ClearContents),
                ClickMode = ClickMode.Release
            };
            clearButton.Click += new RoutedEventHandler(ClearButtonClick);

            dataProviderComboBox = new ComboBox
            {
                Name = "ComboBoxDataSet",
                ItemsSource = Enum.GetValues(typeof(DNNDatasets)).Cast<Enum>().ToList(),
                SelectedIndex = (int)Dataset,
                ToolTip = "Dataset",
                IsEnabled = false
            };

            optimizerComboBox = new ComboBox
            {
                Name = "ComboBoxOptimizers",
                ItemsSource = Enum.GetValues(typeof(DNNOptimizers)).Cast<Enum>().ToList(),
                ToolTip = "Optimizer",
                IsEnabled = false
            };
            Binding optBinding = new Binding
            {
                Source = this,
                Path = new PropertyPath("Optimizer"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Converter = new Converters.EnumConverter(),
                ConverterParameter = typeof(DNNOptimizers)
            };
            BindingOperations.SetBinding(optimizerComboBox, ComboBox.SelectedValueProperty, optBinding);

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
            selectedCostIndex = costLayersComboBox.SelectedIndex;
            costLayersComboBox.SelectionChanged += CostLayersComboBox_SelectionChanged;
            costLayersComboBox.IsEnabled = Model.CostLayersCount > 1;

            layersComboBox = new ComboBox { Name = "ComboBoxLayers" };
            layersComboBox.DataContext = Model;
            layersComboBox.ItemsSource = Model.Layers;
            layersComboBox.ItemTemplate = GetLockTemplate();
            layersComboBox.SourceUpdated += LayersComboBox_SourceUpdated;
            layersComboBox.IsSynchronizedWithCurrentItem = true;
            layersComboBox.SelectedIndex = Settings.Default.SelectedLayer;
            layersComboBox.SelectionChanged += new SelectionChangedEventHandler(LayersComboBox_SelectionChanged);
            layersComboBox.ToolTip = "Layer";
            Model.SelectedIndex = Settings.Default.SelectedLayer;

            disableLockingCheckBox = new CheckBox
            {
                Name = "CheckBoxDisableLocking",
                Content = new BitmapToImage(Resources.Key),
                ToolTip = "Disable Locking",
                IsChecked = Settings.Default.DisableLocking
            };
            disableLockingCheckBox.Unchecked += DisableLockingCheckBox_Unchecked;
            disableLockingCheckBox.Checked += DisableLockingCheckBox_Unchecked;

            unlockAllButton = new Button
            {
                Name = "UnlockAllButton",
                Content = new BitmapToImage(Resources.Unlock),
                ToolTip = "Unlock All",
                ClickMode = ClickMode.Release,
                Visibility = !Settings.Default.DisableLocking && Model.Layers[Settings.Default.SelectedLayer].Lockable ? Visibility.Visible : Visibility.Collapsed
            };
            unlockAllButton.Click += UnlockAll_Click;

            lockAllButton = new Button
            {
                Name = "LockAllButton",
                Content = new BitmapToImage(Resources.Lock),
                ToolTip = "Lock All",
                ClickMode = ClickMode.Release,
                Visibility = !Settings.Default.DisableLocking && Model.Layers[Settings.Default.SelectedLayer].Lockable ? Visibility.Visible : Visibility.Collapsed
            };
            lockAllButton.Click += LockAll_Click;

            Button openLayerWeightsButton = new Button
            {
                Name = "ButtonOpenWeightsLayer",
                ToolTip = "Load Weights",
                Content = new BitmapToImage(Resources.Open),
                ClickMode = ClickMode.Release
            };
            openLayerWeightsButton.Click += new RoutedEventHandler(OpenLayerWeightsButtonClick);

            Button saveLayerWeightsButton = new Button
            {
                Name = "ButtonSaveWeightsLayer",
                ToolTip = "Save Weights",
                Content = new BitmapToImage(Resources.Save),
                ClickMode = ClickMode.Release
            };
            saveLayerWeightsButton.Click += new RoutedEventHandler(SaveLayerWeightsButtonClick);

            Button forgetLayerWeightsButton = new Button
            {
                Name = "ButtonForgetWeightsLayer",
                ToolTip = "Forget Weights",
                Content = new BitmapToImage(Resources.LightningBolt),
                ClickMode = ClickMode.Release
            };
            forgetLayerWeightsButton.Click += new RoutedEventHandler(ForgetLayerWeightsButtonClick);

            trainingPlotCheckBox = new CheckBox
            {
                Name = "CheckBoxTrainingPlot",
                Content = new BitmapToImage(Resources.PerformanceLog),
                ToolTip = "Training Plot"
            };
            Binding tpBinding = new Binding
            {
                Source = this,
                Path = new PropertyPath("ShowTrainingPlot"),
                Mode = BindingMode.TwoWay,
            };
            BindingOperations.SetBinding(trainingPlotCheckBox, CheckBox.IsCheckedProperty, tpBinding);
            trainingPlotCheckBox.Unchecked += TrainingPlotCheckBox_Unchecked;
            trainingPlotCheckBox.Checked += TrainingPlotCheckBox_Unchecked;

            plotTypeComboBox = new ComboBox
            {
                Name = "ComboBoxPlotType",
                ItemsSource = Enum.GetValues(typeof(PlotType)).Cast<Enum>().ToList(),
                ToolTip = "Plot Type"
            };
            Binding binding = new Binding
            {
                Source = this,
                Path = new PropertyPath("ShowTrainingPlot"),
                Mode = BindingMode.TwoWay,
                Converter = new Converters.NullableBoolToVisibilityConverter(),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            BindingOperations.SetBinding(plotTypeComboBox, ComboBox.VisibilityProperty, binding);
            plotTypeComboBox.SelectionChanged += new SelectionChangedEventHandler(PlotTypeChanged);
            binding = new Binding
            {
                Source = this,
                Path = new PropertyPath("CurrentPlotType"),
                Mode = BindingMode.TwoWay,
                Converter = new Converters.EnumConverter(),
                ConverterParameter = typeof(PlotType)
            };
            BindingOperations.SetBinding(plotTypeComboBox, ComboBox.SelectedValueProperty, binding);

            pixelSizeSlider = new FormattedSlider
            {
                Name = "PixelSizeSlider",
                Minimum = 1,
                Maximum = 8,
                LargeChange = 1,
                SmallChange = 1,
                Width = 96,
                IsSnapToTickEnabled = true,
                AutoToolTipPlacement = System.Windows.Controls.Primitives.AutoToolTipPlacement.TopLeft,
                Interval = 12,
                AutoToolTipFormat = "Pixels",
                ToolTip = Math.Round(Settings.Default.PixelSize) == 1 ? "1 Pixel" : Math.Round(Settings.Default.PixelSize).ToString() + " Pixels",
                Value = Settings.Default.PixelSize
            };
            binding = new Binding
            {
                Source = this,
                Path = new PropertyPath("ShowTrainingPlot"),
                Mode = BindingMode.TwoWay,
                Converter = new Converters.InverseNullableBoolToVisibilityConverter(),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged

            };
            BindingOperations.SetBinding(pixelSizeSlider, ComboBox.VisibilityProperty, binding);
            pixelSizeSlider.ValueChanged += PixelSizeSlider_ValueChanged;

            refreshButton = new Button
            {
                Name = "ButtonRefresh",
                ToolTip = "Refresh",
                Content = new BitmapToImage(Resources.Refresh),
                ClickMode = ClickMode.Release
            };
            refreshButton.Click += new RoutedEventHandler(RefreshButtonClick);

            Xceed.Wpf.Toolkit.IntegerUpDown refreshRateIntegerUpDown = new Xceed.Wpf.Toolkit.IntegerUpDown
            {
                Name = "RefreshRate",
                ToolTip = "Refresh Rate/s",
                HorizontalContentAlignment = System.Windows.HorizontalAlignment.Right,
                Minimum = 1,
                Maximum = 300,
                Increment = 1,
                ParsingNumberStyle = System.Globalization.NumberStyles.Integer,
                ClipValueToMinMax = true,
                AutoMoveFocus = true,
                Focusable = true
            };
            binding = new Binding
            {
                Source = this,
                Path = new PropertyPath("RefreshRate"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            BindingOperations.SetBinding(refreshRateIntegerUpDown, Xceed.Wpf.Toolkit.IntegerUpDown.ValueProperty, binding);

            CommandToolBar.Add(startButton);                        // 0
            CommandToolBar.Add(stopButton);                         // 1
            CommandToolBar.Add(pauseButton);                        // 2
            CommandToolBar.Add(editorButton);                       // 3
            CommandToolBar.Add(new Separator());                    // 4
            CommandToolBar.Add(openButton);                         // 5
            CommandToolBar.Add(saveButton);                         // 6
            CommandToolBar.Add(forgetButton);                       // 7
            CommandToolBar.Add(clearButton);                        // 8
            CommandToolBar.Add(new Separator());                    // 9
            CommandToolBar.Add(dataProviderComboBox);               // 10
            CommandToolBar.Add(optimizerComboBox);                  // 11
            CommandToolBar.Add(new Separator());                    // 12
            CommandToolBar.Add(costLayersComboBox);                 // 13
            CommandToolBar.Add(layersComboBox);                     // 14
            CommandToolBar.Add(disableLockingCheckBox);             // 15
            CommandToolBar.Add(unlockAllButton);                    // 16
            CommandToolBar.Add(lockAllButton);                      // 17
            CommandToolBar.Add(openLayerWeightsButton);             // 18
            CommandToolBar.Add(saveLayerWeightsButton);             // 19
            CommandToolBar.Add(forgetLayerWeightsButton);           // 20
            CommandToolBar.Add(new Separator());                    // 21
            CommandToolBar.Add(trainingPlotCheckBox);               // 22
            CommandToolBar.Add(plotTypeComboBox);                   // 23
            CommandToolBar.Add(pixelSizeSlider);                    // 24
            CommandToolBar.Add(new Separator());                    // 25
            CommandToolBar.Add(refreshButton);                      // 26
            CommandToolBar.Add(refreshRateIntegerUpDown);           // 27
        }

        public void OnDisableLockingChanged(object sender, RoutedEventArgs e)
        {
            disableLockingCheckBox.IsChecked = Settings.Default.DisableLocking;
        }

        private void DisableLockingCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (disableLockingCheckBox.IsChecked.HasValue)
            {
                Settings.Default.DisableLocking = disableLockingCheckBox.IsChecked.Value;
                Settings.Default.Save();

                Model.SetDisableLocking(Settings.Default.DisableLocking);

                unlockAllButton.Visibility = !Settings.Default.DisableLocking ? Visibility.Visible : Visibility.Collapsed;
                lockAllButton.Visibility = !Settings.Default.DisableLocking ? Visibility.Visible : Visibility.Collapsed;

                layersComboBox.ItemTemplate = GetLockTemplate();

                int index = layersComboBox.SelectedIndex;
                if (index > 0)
                {
                    layersComboBox.SelectedIndex = index - 1;
                    layersComboBox.SelectedIndex = index;
                }
                else
                {
                    if (Model.LayerCount > (ulong)(index + 1))
                    {
                        layersComboBox.SelectedIndex = index + 1;
                        layersComboBox.SelectedIndex = index;
                    }
                }
            }
        }

        private void UnlockAll_Click(object sender, RoutedEventArgs e)
        {
            Model.SetLocked(false);
        }

        private void LockAll_Click(object sender, RoutedEventArgs e)
        {
            Model.SetLocked(true);
        }

        static DataTemplate GetLockTemplate()
        {
            DataTemplate checkBoxLayout = new DataTemplate
            {
                DataType = typeof(LayerInformation)
            };
            //set up the StackPanel
            FrameworkElementFactory panelFactory = new FrameworkElementFactory(typeof(StackPanel))
            {
                Name = "myComboFactory"
            };
            panelFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);

            FrameworkElementFactory contentFactory;

            if (!Settings.Default.DisableLocking)
            {

                //set up the CheckBox
                contentFactory = new FrameworkElementFactory(typeof(CheckBox));
                contentFactory.SetBinding(CheckBox.ContentProperty, new Binding("Name"));
                Binding bindingIsChecked = new Binding("LockUpdate")
                {
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                    NotifyOnSourceUpdated = true
                };
                contentFactory.SetBinding(CheckBox.IsCheckedProperty, bindingIsChecked);
                contentFactory.SetBinding(CheckBox.IsEnabledProperty, new Binding("Lockable"));
            }
            else
            {
                contentFactory = new FrameworkElementFactory(typeof(TextBlock));
                contentFactory.SetBinding(TextBlock.TextProperty, new Binding("Name"));
            }

            Binding bindingFontWeights = new Binding("Lockable");
            Converters.BoolToStringConverter converter = new Converters.BoolToStringConverter
            {
                TrueValue = System.Windows.FontWeights.Bold,
                FalseValue = System.Windows.FontWeights.Normal
            };
            bindingFontWeights.Converter = converter;
            contentFactory.SetBinding(CheckBox.FontWeightProperty, bindingFontWeights);
            panelFactory.AppendChild(contentFactory);
            checkBoxLayout.VisualTree = panelFactory;

            return checkBoxLayout;
        }

        private void LayersComboBox_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            if (e.OriginalSource is CheckBox cb)
            {
                if (cb.IsEnabled)
                {
                    var layer = Model.Layers.FirstOrDefault(i => i.Name == cb.Content as String);

                    Model.SetLayerLocked(layer.LayerIndex, layer.LockUpdate.Value);

                    e.Handled = true;
                }
            }
        }

        private void PixelSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int temp = (int)Math.Round(e.NewValue);
            if (temp == 1)
                pixelSizeSlider.ToolTip = "1 Pixel";
            else
                pixelSizeSlider.ToolTip = temp.ToString() + " Pixels";

            Settings.Default.PixelSize = temp;
            Settings.Default.Save();
            Model.BlockSize = (ulong)temp;

            LayersComboBox_SelectionChanged(this, null);
        }

        private void TrainingPlotCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.Default.ShowTrainingPlot = trainingPlotCheckBox.IsChecked ?? false;
            Settings.Default.Save();
        }

        private void PlotTypeChanged(object sender, SelectionChangedEventArgs e)
        {
            CurrentPlotType = (PlotType)plotTypeComboBox.SelectedIndex;
            Settings.Default.PlotType = (uint)plotTypeComboBox.SelectedIndex;
            Settings.Default.Save();
            RefreshTrainingPlot();
        }

        public void CostLayersComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (costLayersComboBox.SelectedIndex >= 0)
            {
                SelectedCostIndex = costLayersComboBox.SelectedIndex;
                Model.SetCostIndex((uint)SelectedCostIndex);
            }
        }

        public void RefreshTrainingPlot()
        {
            pointsTrain = new ObservableCollection<DataPoint>();
            pointsTest = new ObservableCollection<DataPoint>();

            switch (CurrentPlotType)
            {
                case PlotType.Accuracy:
                    foreach (DNNTrainingResult result in TrainingLog)
                        if ((int)result.CostIndex == SelectedCostIndex)
                            PointsTrain.Add(new DataPoint(result.Epoch, result.TrainAccuracy));

                    foreach (DNNTrainingResult result in TrainingLog)
                        if ((int)result.CostIndex == SelectedCostIndex)
                            PointsTest.Add(new DataPoint(result.Epoch, result.TestAccuracy));

                    PointsTrainLabel = "Train Accuracy %";
                    PointsTestLabel = "Test Accuracy %";
                    break;

                case PlotType.Error:
                    foreach (DNNTrainingResult result in TrainingLog)
                        if ((int)result.CostIndex == SelectedCostIndex)
                            PointsTrain.Add(new DataPoint(result.Epoch, result.TrainErrorPercentage));

                    foreach (DNNTrainingResult result in TrainingLog)
                        if ((int)result.CostIndex == SelectedCostIndex)
                            PointsTest.Add(new DataPoint(result.Epoch, result.TestErrorPercentage));

                    PointsTrainLabel = "Train Error %";
                    PointsTestLabel = "Test Error %";
                    break;

                case PlotType.Loss:
                    foreach (DNNTrainingResult result in TrainingLog)
                        if ((int)result.CostIndex == SelectedCostIndex)
                            PointsTrain.Add(new DataPoint(result.Epoch, result.AvgTrainLoss));

                    foreach (DNNTrainingResult result in TrainingLog)
                        if ((int)result.CostIndex == SelectedCostIndex)
                            PointsTest.Add(new DataPoint(result.Epoch, result.AvgTestLoss));

                    PointsTrainLabel = "Avg Train Loss";
                    PointsTestLabel = "Avg Test Loss";
                    break;
            }

            OnPropertyChanged(nameof(PointsTrain));
            OnPropertyChanged(nameof(PointsTest));
        }

        public bool SGDR
        {
            get { return sgdr; }
            set
            {
                if (sgdr == value)
                    return;

                sgdr = value;

                Settings.Default.SGDR = sgdr;
                Settings.Default.Save();

                OnPropertyChanged(nameof(SGDR));
            }
        }

        public uint GotoCycle
        {
            get { return gotoCycle; }
            set
            {
                if (gotoCycle == value)
                    return;

                gotoCycle = value;

                Settings.Default.GotoCycle = gotoCycle;
                Settings.Default.Save();

                OnPropertyChanged(nameof(GotoCycle));
            }
        }

        public uint GotoEpoch
        {
            get { return gotoEpoch; }
            set
            {
                if (gotoEpoch == value)
                    return;

                gotoEpoch = value;

                Settings.Default.GotoEpoch = gotoEpoch;
                Settings.Default.Save();

                OnPropertyChanged(nameof(GotoEpoch));
            }
        }

        public LegendPosition CurrentLegendPosition
        {
            get { return currentLegendPosition; }
            set
            {
                if (currentLegendPosition == value)
                    return;

                currentLegendPosition = value;
                OnPropertyChanged(nameof(CurrentLegendPosition));
            }
        }

        public ObservableCollection<DataPoint> PointsTrain
        {
            get { return pointsTrain; }
            set
            {
                if (value == pointsTrain)
                    return;

                pointsTrain = value;
                OnPropertyChanged(nameof(PointsTrain));
            }
        }

        public ObservableCollection<DataPoint> PointsTest
        {
            get { return pointsTest; }
            set
            {
                if (value == pointsTest)
                    return;

                pointsTest = value;
                OnPropertyChanged(nameof(PointsTest));
            }
        }

        public string PointsTrainLabel
        {
            get { return pointsTrainLabel; }
            set
            {
                if (pointsTrainLabel == value)
                    return;

                pointsTrainLabel = value;
                OnPropertyChanged(nameof(PointsTrainLabel));
            }
        }

        public string PointsTestLabel
        {
            get { return pointsTestLabel; }
            set
            {
                if (pointsTestLabel == value)
                    return;

                pointsTestLabel = value;
                OnPropertyChanged(nameof(PointsTestLabel));
            }
        }

        public PlotType CurrentPlotType
        {
            get { return currentPlotType; }
            set
            {
                if (currentPlotType == value)
                    return;

                currentPlotType = value;
                OnPropertyChanged(nameof(CurrentPlotType));

                switch (currentPlotType)
                {
                    case PlotType.Accuracy:
                        CurrentLegendPosition = LegendPosition.BottomRight;
                        break;
                    case PlotType.Error:
                    case PlotType.Loss:
                        CurrentLegendPosition = LegendPosition.TopRight;
                        break;
                }
            }
        }

        public string ProgressText
        {
            get { return progressText; }
            set
            {
                if (value == progressText)
                    return;

                progressText = value;
                OnPropertyChanged(nameof(ProgressText));
            }
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

        public string LayerInfo
        {
            get { return layerInfo; }
            set
            {
                if (value == layerInfo)
                    return;

                layerInfo = value;
                OnPropertyChanged(nameof(LayerInfo));
            }
        }

        public string WeightsMinMax
        {
            get { return weightsMinMax; }
            set
            {
                if (value == weightsMinMax)
                    return;

                weightsMinMax = value;
                OnPropertyChanged(nameof(WeightsMinMax));
            }
        }

        public int WeightsSnapshotX
        {
            get { return weightsSnapshotX; }
            set
            {
                if (value == weightsSnapshotX)
                    return;

                weightsSnapshotX = value;
                OnPropertyChanged(nameof(WeightsSnapshotX));
            }
        }

        public int WeightsSnapshotY
        {
            get { return weightsSnapshotY; }
            set
            {
                if (value == weightsSnapshotY)
                    return;

                weightsSnapshotY = value;
                OnPropertyChanged(nameof(WeightsSnapshotY));
            }
        }

        public String Label
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

        public bool ShowWeights
        {
            get { return showWeights; }
            set
            {
                if (value == showWeights)
                    return;

                showWeights = value;
                OnPropertyChanged(nameof(ShowWeights));
            }
        }

        public bool ShowWeightsSnapshot
        {
            get { return showWeightsSnapshot; }
            set
            {
                if (value == showWeightsSnapshot)
                    return;

                showWeightsSnapshot = value;
                OnPropertyChanged(nameof(ShowWeightsSnapshot));
            }
        }

        public bool? ShowTrainingPlot
        {
            get { return showTrainingPlot; }
            set
            {
                if (value == showTrainingPlot)
                    return;

                showTrainingPlot = value;
                OnPropertyChanged(nameof(ShowTrainingPlot));
            }
        }

        public BitmapSource WeightsSnapshot
        {
            get { return weightsSnapshot; }
            set
            {
                if (value == weightsSnapshot)
                    return;

                weightsSnapshot = value;
                OnPropertyChanged(nameof(WeightsSnapshot));
            }
        }

        public BitmapSource InputSnapshot
        {
            get { return inputSnapshot; }
            set
            {
                if (value == inputSnapshot)
                    return;

                inputSnapshot = value;
                OnPropertyChanged(nameof(InputSnapshot));
            }
        }

        public DNNTrainingRate TrainRate
        {
            get
            {
                if (Settings.Default.TrainRate == null)
                    Settings.Default.TrainRate = new DNNTrainingRate(DNNOptimizers.NAG, 0.9f, 0.0005f, 0.999f, 1E-08f, 128, 1, 200, 1, 0.005f, 0.0001f, 1, 1, false, false, 0, 0, 0, 0, 0, 0, DNNInterpolation.Cubic, 10, 12);

                return Settings.Default.TrainRate;
            }
            private set
            {
                if (value == Settings.Default.TrainRate)
                    return;

                Settings.Default.TrainRate = value;
                OnPropertyChanged(nameof(TrainRate));
            }
        }

        public ObservableCollection<DNNTrainingRate> TrainRates
        {
            get { return trainRates; }
            private set
            {
                if (value == trainRates)
                    return;

                trainRates = value;
                OnPropertyChanged(nameof(TrainRates));
            }
        }

        public ObservableCollection<DNNTrainingResult> TrainingLog
        {
            get
            {
                if (Settings.Default.TrainingLog == null)
                    Settings.Default.TrainingLog = new ObservableCollection<DNNTrainingResult>();

                return Settings.Default.TrainingLog;
            }
            set
            {
                if (value == Settings.Default.TrainingLog)
                    return;

                Settings.Default.TrainingLog = value;
                OnPropertyChanged(nameof(TrainingLog));
            }
        }

        public int SelectedCostIndex
        {
            get { return selectedCostIndex; }
            set
            {
                if (value == selectedCostIndex)
                    return;

                selectedCostIndex = value;
                OnPropertyChanged(nameof(SelectedCostIndex));
                RefreshTrainingPlot();
            }
        }

        public int SelectedIndex
        {
            get { return selectedIndex; }
            set
            {
                if (value == selectedIndex)
                    return;

                selectedIndex = value;
                OnPropertyChanged(nameof(SelectedIndex));
            }
        }

        public DNNOptimizers Optimizer
        {
            get { return optimizer; }
            set
            {
                if (value == optimizer)
                    return;

                optimizer = value;
                Settings.Default.Optimizer = (int)optimizer;
                Settings.Default.Save();
                                               
                OnPropertyChanged(nameof(Optimizer));
            }
        }

        public override string DisplayName => "Train";

        public int? RefreshRate
        {
            get { return refreshRate; }
            set
            {
                if (value.HasValue && value.Value == refreshRate.Value)
                    return;

                refreshRate = value;
                OnPropertyChanged(nameof(RefreshRate));
                Settings.Default.RefreshInterval = refreshRate.Value;
                Settings.Default.Save();
                EventHandler<int?> handler = RefreshRateChanged;
                handler.Invoke(this, refreshRate);
            }
        }

        public override void Reset()
        {
            if (TrainingLog != null)
                TrainingLog.Clear();
            SelectedIndex = -1;
            ProgressText = String.Empty;
            Label = String.Empty;
            RefreshTrainingPlot();
        }

        private void RefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() => LayersComboBox_SelectionChanged(sender, null), DispatcherPriority.Render);
        }

        private void StartButtonClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (Model.TaskState == DNNTaskStates.Running)
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("You must stop testing first!", "Information", MessageBoxButton.OK);

                    return;
                }

                if (Model.TaskState == DNNTaskStates.Stopped)
                {
                    TrainParameters dialog = new TrainParameters
                    {
                        Owner = Application.Current.MainWindow,
                        Model = this.Model,
                        Path = DefinitionsDirectory,
                        IsEnabled = true,
                        Rate = TrainRate,
                        tpvm = this
                    };
                  
                    if (dialog.ShowDialog() ?? false)
                    {
                        TrainRate = dialog.Rate;

                        if (SGDR)
                            Model.AddLearningRateSGDR(true, GotoEpoch, TrainRate);
                        else
                            Model.AddLearningRate(true, GotoEpoch, TrainRate);

                        Model.SetOptimizer(TrainRate.Optimizer);
                        Model.Optimizer = TrainRate.Optimizer;
                        Optimizer = TrainRate.Optimizer;

                        /*
                        DNNDataSet.TrainingRatesDataTable table = new DNNDataSet.TrainingRatesDataTable();
                        table.BeginLoadData();
                        foreach (DNNTrainingRate rate in Model.TrainingRates)
                            table.AddTrainingRatesRow(
                                (int)rate.Optimizer,
                                (double)rate.Beta2,
                                (double)rate.Eps,
                                (double)rate.MaximumRate,
                                (int)rate.BatchSize,
                                (int)rate.Cycles,
                                (int)rate.Epochs,
                                (int)rate.EpochMultiplier,
                                (double)rate.MinimumRate,
                                (double)rate.L2Penalty,
                                (double)rate.Momentum,
                                (double)rate.DecayFactor,
                                (int)rate.DecayAfterEpochs,
                                rate.HorizontalFlip,
                                rate.VerticalFlip,
                                (double)rate.Dropout,
                                (double)rate.Cutout,
                                (double)rate.AutoAugment,
                                (double)rate.ColorCast,
                                (int)rate.ColorAngle,
                                (double)rate.Distortion,
                                (int)rate.Interpolation,
                                (double)rate.Scaling,
                                (double)rate.Rotation);
                        table.EndLoadData();
                        table.WriteXml(StorageDirectory + @"rates.scheme-xml", System.Data.XmlWriteMode.WriteSchema);
                        */

                        EpochDuration = TimeSpan.Zero;

                        RefreshTimer = new Timer(1000 * Settings.Default.RefreshInterval.Value);
                        RefreshTimer.Elapsed += new ElapsedEventHandler(RefreshTimer_Elapsed);
                        
                        Model.SetCostIndex((uint)SelectedCostIndex);
                        Model.Start(true);
                        RefreshTimer.Start();
                        CommandToolBar[0].Visibility = Visibility.Collapsed;
                        CommandToolBar[1].Visibility = Visibility.Visible;
                        CommandToolBar[2].Visibility = Visibility.Visible;

                        CommandToolBar[5].Visibility = Visibility.Collapsed;
                        CommandToolBar[6].Visibility = Visibility.Visible;
                        CommandToolBar[7].Visibility = Visibility.Collapsed;
                        CommandToolBar[13].Visibility = Visibility.Collapsed;

                        CommandToolBar[16].Visibility = Visibility.Collapsed;
                        CommandToolBar[17].Visibility = Visibility.Collapsed;
                        CommandToolBar[18].Visibility = Visibility.Collapsed;
                        CommandToolBar[19].Visibility = Visibility.Collapsed;
                        CommandToolBar[20].Visibility = Visibility.Collapsed;

                        if (Model.Layers[layersComboBox.SelectedIndex].HasWeights)
                        {
                            if ((Model.Layers[layersComboBox.SelectedIndex].IsNormalizationLayer && Model.Layers[layersComboBox.SelectedIndex].Scaling) || !Model.Layers[layersComboBox.SelectedIndex].IsNormalizationLayer)
                            {
                                CommandToolBar[16].Visibility = !Settings.Default.DisableLocking ? Visibility.Visible : Visibility.Collapsed;
                                CommandToolBar[17].Visibility = !Settings.Default.DisableLocking ? Visibility.Visible : Visibility.Collapsed;
                                CommandToolBar[19].Visibility = Visibility.Visible;
                            }
                        }

                        ShowProgress = true;
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

                        CommandToolBar[5].Visibility = Visibility.Collapsed;
                        CommandToolBar[6].Visibility = Visibility.Visible;
                        CommandToolBar[7].Visibility = Visibility.Collapsed;
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
                    if (Xceed.Wpf.Toolkit.MessageBox.Show("Do you really want stop?", "Stop Training", MessageBoxButton.YesNo, MessageBoxImage.None, MessageBoxResult.No) == MessageBoxResult.Yes)
                    {
                        RefreshTimer.Stop();
                        RefreshTimer.Elapsed -= new ElapsedEventHandler(RefreshTimer_Elapsed);
                        RefreshTimer.Dispose();
                        Mouse.OverrideCursor = Cursors.Wait;
                        Model.Stop();

                        CommandToolBar[0].ToolTip = "Start Training";
                        CommandToolBar[0].Visibility = Visibility.Visible;
                        CommandToolBar[1].Visibility = Visibility.Collapsed;
                        CommandToolBar[2].Visibility = Visibility.Collapsed;

                        CommandToolBar[5].Visibility = Visibility.Visible;
                        CommandToolBar[6].Visibility = Visibility.Visible;
                        CommandToolBar[7].Visibility = Visibility.Visible;
                        CommandToolBar[13].Visibility = Visibility.Visible;

                        CommandToolBar[16].Visibility = Visibility.Collapsed;
                        CommandToolBar[17].Visibility = Visibility.Collapsed;
                        CommandToolBar[18].Visibility = Visibility.Collapsed;
                        CommandToolBar[19].Visibility = Visibility.Collapsed;
                        CommandToolBar[20].Visibility = Visibility.Collapsed;

                        if (Model.Layers[layersComboBox.SelectedIndex].HasWeights)
                        {
                            if ((Model.Layers[layersComboBox.SelectedIndex].IsNormalizationLayer && Model.Layers[layersComboBox.SelectedIndex].Scaling) || !Model.Layers[layersComboBox.SelectedIndex].IsNormalizationLayer)
                            {
                                CommandToolBar[16].Visibility = !Settings.Default.DisableLocking ? Visibility.Visible : Visibility.Collapsed;
                                CommandToolBar[17].Visibility = !Settings.Default.DisableLocking ? Visibility.Visible : Visibility.Collapsed;
                                CommandToolBar[18].Visibility = Visibility.Visible;
                                CommandToolBar[19].Visibility = Visibility.Visible;
                                CommandToolBar[20].Visibility = Visibility.Visible;
                            }
                        }

                        ShowProgress = false;

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
                    CommandToolBar[0].ToolTip = "Resume Training";
                    CommandToolBar[0].Visibility = Visibility.Visible;
                    CommandToolBar[1].Visibility = Visibility.Visible;
                    CommandToolBar[2].Visibility = Visibility.Collapsed;

                    CommandToolBar[5].Visibility = Visibility.Collapsed;
                    CommandToolBar[6].Visibility = Visibility.Visible;
                    CommandToolBar[7].Visibility = Visibility.Collapsed;
                }
            }, DispatcherPriority.Normal);
        }

        private void OpenButtonClick(object sender, RoutedEventArgs e)
        {
            this.Open?.Invoke(this, EventArgs.Empty);
        }

        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            this.Save?.Invoke(this, EventArgs.Empty);
        }

        private void EditorButtonClick(object sender, RoutedEventArgs e)
        {
            if (Model.TaskState == DNNTaskStates.Stopped)
            {
                TrainRates = new ObservableCollection<DNNTrainingRate> { Settings.Default.TrainRate };

                TrainingSchemeEditor dialog = new TrainingSchemeEditor
                {
                    Path = StorageDirectory
                };
                
                dialog.tpvm = this;
                dialog.DataContext = this;
                dialog.buttonTrain.IsEnabled = true;
                dialog.Owner = Application.Current.MainWindow;
                dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                if (dialog.ShowDialog() ?? false)
                {
                    bool first = true;
                    foreach (DNNTrainingRate rate in TrainRates)
                    {
                        if (SGDR)
                            Model.AddLearningRateSGDR(first, GotoEpoch, rate);
                        else
                            Model.AddLearningRate(first, GotoEpoch, rate);

                        first = false;
                    }

                    EpochDuration = TimeSpan.Zero;

                    RefreshTimer = new Timer(1000 * Settings.Default.RefreshInterval.Value);
                    RefreshTimer.Elapsed += new ElapsedEventHandler(RefreshTimer_Elapsed);

                    Model.SetOptimizer(TrainRates[0].Optimizer);
                    Model.Optimizer = TrainRates[0].Optimizer;
                    Optimizer = TrainRates[0].Optimizer;

                    Model.Start(true);
                    RefreshTimer.Start();
                    CommandToolBar[0].Visibility = Visibility.Collapsed;
                    CommandToolBar[1].Visibility = Visibility.Visible;
                    CommandToolBar[2].Visibility = Visibility.Visible;

                    CommandToolBar[5].Visibility = Visibility.Collapsed;
                    CommandToolBar[6].Visibility = Visibility.Visible;
                    CommandToolBar[7].Visibility = Visibility.Collapsed;
                  
                    if (layersComboBox.SelectedIndex >= 0 && Model.Layers[layersComboBox.SelectedIndex].HasWeights)
                    {
                        LayerInformation info = Model.Layers[layersComboBox.SelectedIndex];
                        if (info.IsNormalizationLayer)
                        {
                            if (info.Scaling)
                            {
                                CommandToolBar[16].Visibility = !Settings.Default.DisableLocking ? Visibility.Visible : Visibility.Collapsed;
                                CommandToolBar[17].Visibility = !Settings.Default.DisableLocking ? Visibility.Visible : Visibility.Collapsed;
                                CommandToolBar[18].Visibility = Visibility.Collapsed;
                                CommandToolBar[19].Visibility = Visibility.Visible;
                                CommandToolBar[20].Visibility = Visibility.Visible;
                            }
                            else
                            {
                                CommandToolBar[16].Visibility = Visibility.Collapsed;
                                CommandToolBar[17].Visibility = Visibility.Collapsed;
                                CommandToolBar[18].Visibility = Visibility.Collapsed;
                                CommandToolBar[19].Visibility = Visibility.Collapsed;
                                CommandToolBar[20].Visibility = Visibility.Visible;
                            }
                        }
                        else
                        {
                            CommandToolBar[16].Visibility = !Settings.Default.DisableLocking ? Visibility.Visible : Visibility.Collapsed;
                            CommandToolBar[17].Visibility = !Settings.Default.DisableLocking ? Visibility.Visible : Visibility.Collapsed;
                            CommandToolBar[18].Visibility = Visibility.Collapsed;
                            CommandToolBar[19].Visibility = Visibility.Visible;
                            CommandToolBar[20].Visibility = Visibility.Collapsed;
                        }
                    }
                    else
                    {
                        CommandToolBar[16].Visibility = Visibility.Collapsed;
                        CommandToolBar[17].Visibility = Visibility.Collapsed;
                        CommandToolBar[18].Visibility = Visibility.Collapsed;
                        CommandToolBar[19].Visibility = Visibility.Collapsed;
                        CommandToolBar[20].Visibility = Visibility.Collapsed;
                    }

                    ShowProgress = true;
                }
            }
            else
            {
                TrainRates = new ObservableCollection<DNNTrainingRate> { TrainRate };
                TrainingSchemeEditor dialog = new TrainingSchemeEditor { Path = StorageDirectory };
                dialog.tpvm = this;
                dialog.DataContext = this;
                dialog.buttonTrain.IsEnabled = false;
                dialog.Owner = Application.Current.MainWindow;
                dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                dialog.ShowDialog();
            }
        }

        private void ForgetButtonClick(object sender, RoutedEventArgs e)
        {
            if (Xceed.Wpf.Toolkit.MessageBox.Show("Do you really want to forget all weights?", "Forget Model Weights", MessageBoxButton.YesNo, MessageBoxImage.None, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                Mouse.OverrideCursor = Cursors.Wait;

                Model.ResetWeights();

                Application.Current.Dispatcher.Invoke(() => LayersComboBox_SelectionChanged(sender, null), DispatcherPriority.Render);

                Mouse.OverrideCursor = null;
            }
        }

        private void ClearButtonClick(object sender, RoutedEventArgs e)
        {
            if (TrainingLog.Count > 0)
            {
                //StringBuilder sb = new StringBuilder();
                //foreach (TrainingResult row in TrainingLog)
                //    sb.AppendLine(row.Epoch.ToString() + "\t" + row.TrainingRate.ToString() + "\t" + row.Dropout.ToString() + row.Cutout.ToString() + "\t" + row.Distortion.ToString() + "\t" + row.HorizontalFlip.ToString() + "\t" + row.VerticalFlip.ToString() + "\t" + row.TrainErrors.ToString() + "\t" + row.TestErrors.ToString() + "\t" + row.AvgTrainLoss.ToString() + "\t" + row.AvgTestLoss.ToString() + "\t" + row.TrainErrors.ToString() + "\t" + row.TestErrors.ToString() + "\t" + row.TestAccuracy.ToString() + "\t" + row.ElapsedTime.ToString());
                //Clipboard.SetText(sb.ToString());

                if (Xceed.Wpf.Toolkit.MessageBox.Show("Do you really want to clear the log?", "Clear Log", MessageBoxButton.YesNo, MessageBoxImage.None, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    TrainingLog.Clear();
                    RefreshTrainingPlot();
                }
            }
        }

        private void OpenLayerWeightsButtonClick(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = false,
                AddExtension = true,
                ValidateNames = true,
                Filter = "Layer Weights|*.layer-weights",
                Title = "Load layer weights",
                DefaultExt = ".layer-weights",
                FilterIndex = 1,
                InitialDirectory = DefinitionsDirectory + Model.Name + "-weights" + @"\"
            };

            bool stop = false;
            while (!stop)
            {
                if (openFileDialog.ShowDialog(Application.Current.MainWindow) == true)
                {

                    string fileName = openFileDialog.FileName;
                    if (fileName.Contains(".layer-weights"))
                    {
                        Mouse.OverrideCursor = Cursors.Wait;

                        if (Model.LoadLayerWeights(fileName, (uint)layersComboBox.SelectedIndex) == 0)
                        {
                            Application.Current.Dispatcher.Invoke(() => LayersComboBox_SelectionChanged(sender, null), DispatcherPriority.Render);
                            Mouse.OverrideCursor = null;
                            Xceed.Wpf.Toolkit.MessageBox.Show("Layer weights are loaded", "Information", MessageBoxButton.OK);
                            stop = true;
                        }
                        else
                        {
                            Mouse.OverrideCursor = null;
                            Xceed.Wpf.Toolkit.MessageBox.Show("Layer weights are incompatible", "Choose a different file", MessageBoxButton.OK);
                        }
                    }
                }
                else
                    stop = true;
            }
            Mouse.OverrideCursor = null;
        }

        private void SaveLayerWeightsButtonClick(object sender, RoutedEventArgs e)
        {
            int layerIndex = layersComboBox.SelectedIndex;

            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = Model.Layers[layerIndex].Name,
                AddExtension = true,
                CreatePrompt = false,
                OverwritePrompt = true,
                ValidateNames = true,
                Filter = "Layer Weights|*.layer-weights",
                Title = "Save layer weights",
                DefaultExt = ".layer-weights",
                FilterIndex = 1,
                InitialDirectory = DefinitionsDirectory + Model.Name + "-weights" + @"\"
            };

            if (saveFileDialog.ShowDialog(Application.Current.MainWindow) == true)
            {
                if (saveFileDialog.FileName.Contains(".layer-weights"))
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    if (Model.SaveLayerWeights(saveFileDialog.FileName, (ulong)layerIndex) == 0)
                    {
                        Mouse.OverrideCursor = null;
                        Xceed.Wpf.Toolkit.MessageBox.Show("Layer weights are saved", "Information", MessageBoxButton.OK);
                    }
                    else
                    {
                        Mouse.OverrideCursor = null;
                        Xceed.Wpf.Toolkit.MessageBox.Show("Layer weights not saved!", "Information", MessageBoxButton.OK);
                    }
                }
            }
        }

        private void ForgetLayerWeightsButtonClick(object sender, RoutedEventArgs e)
        {
            if (Xceed.Wpf.Toolkit.MessageBox.Show("Do you really want to forget layer weights?", "Forget Layer Weights", MessageBoxButton.YesNo, MessageBoxImage.None, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                Mouse.OverrideCursor = Cursors.Wait;
                uint index = (uint)layersComboBox.SelectedIndex;
                Model.ResetLayerWeights((uint)layersComboBox.SelectedIndex);
                Application.Current.Dispatcher.Invoke(() => LayersComboBox_SelectionChanged(sender, null), DispatcherPriority.Render);
                Mouse.OverrideCursor = null;
            }
        }

        public void RefreshButtonClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() => LayersComboBox_SelectionChanged(sender, null), DispatcherPriority.Render);
            Application.Current.Dispatcher.Invoke(() => RefreshTrainingPlot(), DispatcherPriority.Render);
        }

        public void LayersComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Model != null && layersComboBox.SelectedIndex >= 0)
            {
                int index = layersComboBox.SelectedIndex;
                if (index < (int)Model.LayerCount)
                {
                    Settings.Default.SelectedLayer = layersComboBox.SelectedIndex;
                    Settings.Default.Save();
                    Model.SelectedIndex = Settings.Default.SelectedLayer;

                    ShowSample = Model.TaskState == DNNTaskStates.Running;
                    ShowWeights = Model.Layers[index].HasWeights || Settings.Default.Timings;
                    ShowWeightsSnapshot = (Model.Layers[index].IsNormalizationLayer && Model.Layers[index].Scaling) || Model.Layers[index].LayerType == DNNLayerTypes.PartialDepthwiseConvolution || Model.Layers[index].LayerType == DNNLayerTypes.DepthwiseConvolution || Model.Layers[index].LayerType == DNNLayerTypes.ConvolutionTranspose || Model.Layers[index].LayerType == DNNLayerTypes.Convolution || Model.Layers[index].LayerType == DNNLayerTypes.Dense || (Model.Layers[index].LayerType == DNNLayerTypes.Activation && Model.Layers[index].HasWeights);

                    Model.UpdateLayerInfo((ulong)index, ShowWeightsSnapshot);

                    if (ShowSample)
                    {
                        Model.UpdateLayerInfo(0ul, true);
                        InputSnapshot = Model.InputSnapshot;
                        Label = Model.Label;
                    }

                    CommandToolBar[16].Visibility = !Settings.Default.DisableLocking ? Visibility.Visible : Visibility.Collapsed;
                    CommandToolBar[17].Visibility = !Settings.Default.DisableLocking ? Visibility.Visible : Visibility.Collapsed;
                    CommandToolBar[18].Visibility = Model.Layers[index].Lockable && Model.TaskState == DNNTaskStates.Stopped ? Visibility.Visible : Visibility.Collapsed;
                    CommandToolBar[19].Visibility = Model.Layers[index].Lockable ? Visibility.Visible : Visibility.Collapsed;
                    CommandToolBar[20].Visibility = Model.Layers[index].Lockable && Model.TaskState == DNNTaskStates.Stopped ? Visibility.Visible : Visibility.Collapsed;

                    LayerInfo = "<Span><Bold>Layer</Bold></Span><LineBreak/>";
                    LayerInfo += "<Span>" + Model.Layers[index].Description + "</Span><LineBreak/>";

                    StringBuilder sb = new StringBuilder();
                    weightsMinMax = String.Empty;

                    weightsMinMax += "<Span><Bold>Neurons</Bold></Span><LineBreak/>";

                    sb.Length = 0;
                    if (Model.Layers[index].NeuronsStdDev >= 0.0f)
                        sb.AppendFormat(" Std:     {0:N8}", Model.Layers[index].NeuronsStdDev);
                    else
                        sb.AppendFormat(" Std:    {0:N8}", Model.Layers[index].NeuronsStdDev);
                    weightsMinMax += "<Span>" + sb.ToString() + "</Span><LineBreak/>";

                    sb.Length = 0;
                    if (Model.Layers[index].NeuronsMean >= 0.0f)
                        sb.AppendFormat(" Mean:    {0:N8}", Model.Layers[index].NeuronsMean);
                    else
                        sb.AppendFormat(" Mean:   {0:N8}", Model.Layers[index].NeuronsMean);
                    weightsMinMax += "<Span>" + sb.ToString() + "</Span><LineBreak/>";

                    sb.Length = 0;
                    if (Model.Layers[index].NeuronsMin >= 0.0f)
                        sb.AppendFormat(" Min:     {0:N8}", Model.Layers[index].NeuronsMin);
                    else
                        sb.AppendFormat(" Min:    {0:N8}", Model.Layers[index].NeuronsMin);
                    weightsMinMax += "<Span>" + sb.ToString() + "</Span><LineBreak/>";

                    sb.Length = 0;
                    if (Model.Layers[index].NeuronsMax >= 0.0f)
                        sb.AppendFormat(" Max:     {0:N8}", Model.Layers[index].NeuronsMax);
                    else
                        sb.AppendFormat(" Max:    {0:N8}", Model.Layers[index].NeuronsMax);
                    weightsMinMax += "<Span>" + sb.ToString() + "</Span><LineBreak/>";

                    if (ShowWeightsSnapshot)
                    {
                        weightsMinMax += "<Span><Bold>Weights</Bold></Span><LineBreak/>";

                        sb.Length = 0;
                        if (Model.Layers[index].WeightsStdDev >= 0.0f)
                            sb.AppendFormat(" Std:     {0:N8}", Model.Layers[index].WeightsStdDev);
                        else
                            sb.AppendFormat(" Std:    {0:N8}", Model.Layers[index].WeightsStdDev);
                        weightsMinMax += "<Span>" + sb.ToString() + "</Span><LineBreak/>";

                        sb.Length = 0;
                        if (Model.Layers[index].WeightsMean >= 0.0f)
                            sb.AppendFormat(" Mean:    {0:N8}", Model.Layers[index].WeightsMean);
                        else
                            sb.AppendFormat(" Mean:   {0:N8}", Model.Layers[index].WeightsMean);
                        weightsMinMax += "<Span>" + sb.ToString() + "</Span><LineBreak/>";


                        sb.Length = 0;
                        if (Model.Layers[index].WeightsMin >= 0.0f)
                            sb.AppendFormat(" Min:     {0:N8}", Model.Layers[index].WeightsMin);
                        else
                            sb.AppendFormat(" Min:    {0:N8}", Model.Layers[index].WeightsMin);
                        weightsMinMax += "<Span Foreground=\"White\">" + sb.ToString() + "</Span><LineBreak/>";

                        sb.Length = 0;
                        if (Model.Layers[index].WeightsMax >= 0.0f)
                            sb.AppendFormat(" Max:     {0:N8}", Model.Layers[index].WeightsMax);
                        else
                            sb.AppendFormat(" Max:    {0:N8}", Model.Layers[index].WeightsMax);
                        if (ShowWeightsSnapshot)
                            weightsMinMax += "<Span Foreground=\"Black\">" + sb.ToString() + "</Span><LineBreak/>";
                        else
                            weightsMinMax += "<Span Foreground=\"White\">" + sb.ToString() + "</Span><LineBreak/>";

                        if (Model.Layers[index].HasBias)
                        {
                            weightsMinMax += "<Span><Bold>Biases</Bold></Span><LineBreak/>";

                            sb.Length = 0;
                            if (Model.Layers[index].BiasesStdDev >= 0.0f)
                                sb.AppendFormat(" Std:     {0:N8}", Model.Layers[index].BiasesStdDev);
                            else
                                sb.AppendFormat(" Std:    {0:N8}", Model.Layers[index].BiasesStdDev);
                            weightsMinMax += "<Span>" + sb.ToString() + "</Span><LineBreak/>";

                            sb.Length = 0;
                            if (Model.Layers[index].BiasesMean >= 0.0f)
                                sb.AppendFormat(" Mean:    {0:N8}", Model.Layers[index].BiasesMean);
                            else
                                sb.AppendFormat(" Mean:   {0:N8}", Model.Layers[index].BiasesMean);
                            weightsMinMax += "<Span>" + sb.ToString() + "</Span><LineBreak/>";

                            sb.Length = 0;
                            if (Model.Layers[index].BiasesMin >= 0.0f)
                                sb.AppendFormat(" Min:     {0:N8}", Model.Layers[index].BiasesMin);
                            else
                                sb.AppendFormat(" Min:    {0:N8}", Model.Layers[index].BiasesMin);
                            weightsMinMax += "<Span Foreground=\"White\">" + sb.ToString() + "</Span><LineBreak/>";

                            sb.Length = 0;
                            if (Model.Layers[index].BiasesMax >= 0.0f)
                                sb.AppendFormat(" Max:     {0:N8}", Model.Layers[index].BiasesMax);
                            else
                                sb.AppendFormat(" Max:    {0:N8}", Model.Layers[index].BiasesMax);
                            if (ShowWeightsSnapshot)
                                weightsMinMax += "<Span Foreground=\"Black\">" + sb.ToString() + "</Span>";
                            else
                                weightsMinMax += "<Span Foreground=\"White\">" + sb.ToString() + "</Span>";
                        }
                    }
                    OnPropertyChanged(nameof(WeightsMinMax));

                    if (Settings.Default.Timings)
                    {
                        LayerInfo += "<Span><Bold>Timings</Bold></Span><LineBreak/>";

                        if (Model.State == DNNStates.Training)
                        {
                            sb.Length = 0;
                            sb.AppendFormat(" fprop:\t\t{0:D}/{1:D} ms", (int)Model.Layers[index].FPropLayerTime, (int)Model.fpropTime);
                            LayerInfo += "<Span>" + sb.ToString() + "</Span><LineBreak/>";
                            sb.Length = 0;
                            sb.AppendFormat(" bprop:\t\t{0:D}/{1:D} ms", (int)Model.Layers[index].BPropLayerTime, (int)Model.bpropTime);
                            LayerInfo += "<Span>" + sb.ToString() + "</Span><LineBreak/>";
                            if (ShowWeightsSnapshot)
                            {
                                sb.Length = 0;
                                sb.AppendFormat(" update:\t{0:D}/{1:D} ms", (int)Model.Layers[index].UpdateLayerTime, (int)Model.updateTime);
                                LayerInfo += "<Span>" + sb.ToString() + "</Span>";
                            }
                        }
                        else
                        {
                            sb.Length = 0;
                            sb.AppendFormat(" fprop:\t\t{0:D}/{1:D} ms", (int)Model.Layers[index].FPropLayerTime, (int)Model.fpropTime);
                            LayerInfo += "<Span>" + sb.ToString() + "</Span>";
                        }
                    }

                    WeightsSnapshotX = Model.Layers[index].WeightsSnapshotX;
                    WeightsSnapshotY = Model.Layers[index].WeightsSnapshotY;
                    WeightsSnapshot = Model.Layers[index].WeightsSnapshot;

                    layersComboBox.ItemsSource = Model.Layers;
                }
            }
        }
    }
}