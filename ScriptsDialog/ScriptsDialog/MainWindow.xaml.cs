using ScriptsDialog.Properties;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ScriptsDialog
{
    [Serializable()]
    public class Filler : INotifyPropertyChanged
    {
        [field: NonSerializedAttribute()]
        public event PropertyChangedEventHandler PropertyChanged;

        private Fillers id;
        private string formula;

        public Fillers Id
        {
            get { return id; }
            set
            {
                if (value == id)
                    return;

                id = value;
                OnPropertyChanged("Id");
            }
        }

        public string Formula
        {
            get { return formula; }
            set
            {
                if (value == formula)
                    return;

                formula = value;
                OnPropertyChanged("Formula");
            }
        }

        public string Name { get { return id.ToString(); } }

        public override string ToString()
        {
            return Id.ToString();
        }

        public Filler()
        {
        }

        public Filler(Fillers id, string formula)
        {
            Id = id;
            Formula = formula;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Interaction logic for ModelParameters.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        const string Framework = "netcoreapp3.1";
#if DEBUG
        const string Mode = "Debug";
#else
        const string Mode = "Release";
#endif
        public static string StorageDirectory { get; } = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\convnet\";
        public static string ScriptsDirectory { get; } = StorageDirectory + @"scripts\";
        public static string ScriptPath { get; } = ScriptsDirectory + @"ScriptsDialog\bin\" + Mode + @"\" + Framework + @"\";
      
        public ObservableCollection<Filler> fillersList;

        public MainWindow()
        {
            fillersList = new ObservableCollection<Filler>();
            fillersList.Add(new Filler { Id = Fillers.Constant, Formula = "constant=scale" });
            fillersList.Add(new Filler { Id = Fillers.HeNormal, Formula = "stddev=gain\\cdot\\sqrt{\\frac{2}{FanIn}}" });
            fillersList.Add(new Filler { Id = Fillers.HeUniform, Formula = "limit=gain\\cdot\\sqrt{\\frac{6}{FanIn}}" });
            fillersList.Add(new Filler { Id = Fillers.LeCunNormal, Formula = "stddev=gain\\cdot\\sqrt{\\frac{1}{FanIn}}" });
            fillersList.Add(new Filler { Id = Fillers.LeCunUniform, Formula = "limit=gain\\cdot\\sqrt{\\frac{3}{FanIn}}" });
            fillersList.Add(new Filler { Id = Fillers.Normal, Formula = "stddev=scale" });
            fillersList.Add(new Filler { Id = Fillers.TruncatedNormal, Formula = "stddev=scale" });
            fillersList.Add(new Filler { Id = Fillers.Uniform, Formula = "limit=scale" });
            fillersList.Add(new Filler { Id = Fillers.XavierNormal, Formula = "stddev=gain\\cdot\\sqrt{\\frac{2}{FanIn+FanOut}}" });
            fillersList.Add(new Filler { Id = Fillers.XavierUniform, Formula = "limit=gain\\cdot\\sqrt{\\frac{6}{FanIn+FanOut}}" });

            InitializeComponent();

            if (Settings.Default.Parameters == null)
            {
                Settings.Default.Parameters = new ScriptParameters(Scripts.shufflenetv2, Datasets.cifar10, 32, 32, 4, 4, false, true, Fillers.HeNormal, FillerModes.Auto, 1f, 0.05f, 1f, 1f, false, Fillers.Constant, FillerModes.Auto, 1f, 0f, 1f, 1f, 0.995f, 0.0001f, false, 0f, 0f, 3, 4, 8, 12, false, 0.0f, 0.0f, false, true, Activations.Relu);

                var efficientnetv2 = new ObservableCollection<EfficientNetRecord>();
                efficientnetv2.Add(new EfficientNetRecord(1, 24, 2, 1, false));
                efficientnetv2.Add(new EfficientNetRecord(4, 48, 4, 2, false));
                efficientnetv2.Add(new EfficientNetRecord(4, 64, 4, 2, false));
                efficientnetv2.Add(new EfficientNetRecord(4, 128, 6, 2, true));
                efficientnetv2.Add(new EfficientNetRecord(6, 160, 9, 1, true));
                efficientnetv2.Add(new EfficientNetRecord(6, 256, 15, 2, true));
                Settings.Default.Parameters.EfficientNet = efficientnetv2;

                var shufflenetv2 = new ObservableCollection<ShuffleNetRecord>();
                shufflenetv2.Add(new ShuffleNetRecord(6, 3, 1, 2, false));
                shufflenetv2.Add(new ShuffleNetRecord(7, 3, 1, 2, true));
                shufflenetv2.Add(new ShuffleNetRecord(8, 3, 1, 2, true));
                Settings.Default.Parameters.ShuffleNet = shufflenetv2;

                Settings.Default.Save();
            }

            DataContext = Settings.Default.Parameters;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Free managed objects.
            }
            // Free unmanaged objects
        }

        public void Dispose()
        {
            Dispose(true);
            // Ensure that the destructor is not called
            GC.SuppressFinalize(this);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Settings.Default.Parameters.Compression = Settings.Default.Parameters.Bottleneck ? 0.5f : 1.0f;
           
            Calc();

            comboBoxDataset.Focus();
        }

        bool IsValid(DependencyObject node)
        {
            // Check if dependency object was passed
            if (node != null)
            {
                // Check if dependency object is valid.
                // NOTE: Validation.GetHasError works for controls that have validation rules attached 
                bool isValid = !Validation.GetHasError(node);
                if (!isValid)
                {
                    // If the dependency object is invalid, and it can receive the focus,
                    // set the focus
                    if (node is IInputElement) Keyboard.Focus((IInputElement)node);
                    return false;
                }
            }

            // If this dependency object is valid, check all child dependency objects
            foreach (object subnode in LogicalTreeHelper.GetChildren(node))
            {
                if (subnode is DependencyObject)
                {
                    // If a child dependency object is invalid, return false immediately,
                    // otherwise keep checking
                    if (IsValid((DependencyObject)subnode) == false) return false;
                }
            }

            // All dependency objects are valid
            return true;
        }

        private void ButtonGenerate_Click(object sender, RoutedEventArgs e)
        {
            if (IsValid(this))
            {
                var script = ScriptCatalog.Generate(Settings.Default.Parameters);
                var fileInfo = new FileInfo(ScriptPath + @"script.txt");
                if (!fileInfo.Directory.Exists)
                    fileInfo.Directory.Create();
                var streamWriter = fileInfo.CreateText();
                streamWriter.AutoFlush = true;
                streamWriter.Write(script);
                streamWriter.Close();
                streamWriter.Dispose();

                Close();
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            buttonCancel.Focus();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Calc();
        }

        private void CheckBoxHasBias_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (CheckBoxHasBias.IsChecked.HasValue)
            {
                if (!CheckBoxHasBias.IsChecked.Value)
                {
                    Settings.Default.Parameters.BiasesFiller = Fillers.Constant;
                    Settings.Default.Parameters.BiasesFillerMode = FillerModes.Auto;
                    Settings.Default.Parameters.BiasesGain = 1.0f;
                    Settings.Default.Parameters.BiasesScale = 0.0f;
                    Settings.Default.Parameters.BiasesLRM = 1.0f;
                    Settings.Default.Parameters.BiasesWDM = 1.0f;
                }

                Calc();
            }
        }

        private void CheckBoxBottleneck_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (checkBoxBottleneck.IsChecked.HasValue)
            {
                switch ((Scripts)comboBoxModel.SelectedIndex)
                {
                    case Scripts.densenet:
                        Settings.Default.Parameters.Compression = checkBoxBottleneck.IsChecked.Value ? 0.5f : 1.0f;
                        break;
                }
            }
        }

        private void Calc()
        {
            Grid.RowDefinitions[5].Height = Settings.Default.Parameters.EfficientNetVisible || Settings.Default.Parameters.ShuffleNetVisible ? new GridLength(0, GridUnitType.Pixel) : new GridLength(30, GridUnitType.Pixel);
            Grid.RowDefinitions[6].Height = Settings.Default.Parameters.EfficientNetVisible || Settings.Default.Parameters.ShuffleNetVisible ? new GridLength(0, GridUnitType.Pixel) : new GridLength(30, GridUnitType.Pixel);
            Grid.RowDefinitions[7].Height = Settings.Default.Parameters.EfficientNetVisible || Settings.Default.Parameters.ShuffleNetVisible ? new GridLength(0, GridUnitType.Pixel) : new GridLength(30, GridUnitType.Pixel);
            Grid.RowDefinitions[16].Height = Settings.Default.Parameters.EfficientNetVisible || Settings.Default.Parameters.ShuffleNetVisible ? new GridLength(120, GridUnitType.Pixel) : new GridLength(0, GridUnitType.Pixel);
            textBoxGroups.IsEnabled = !Settings.Default.Parameters.EfficientNetVisible && !Settings.Default.Parameters.ShuffleNetVisible;
            textBoxIterations.IsEnabled = !Settings.Default.Parameters.EfficientNetVisible && !Settings.Default.Parameters.ShuffleNetVisible;

            Grid.RowDefinitions[18].Height = Settings.Default.Parameters.WeightsFillerModeVisible ? new GridLength(30, GridUnitType.Pixel) : new GridLength(0, GridUnitType.Pixel);
            comboBoxWeightsFillerMode.IsEnabled = Settings.Default.Parameters.WeightsFillerModeVisible;
            comboBoxWeightsFillerMode.Visibility = Settings.Default.Parameters.WeightsFillerModeVisible ? Visibility.Visible : Visibility.Collapsed;

            Grid.RowDefinitions[19].Height = Settings.Default.Parameters.WeightsGainVisible ? new GridLength(30, GridUnitType.Pixel) : new GridLength(0, GridUnitType.Pixel);
            textBlockWeightsGain.IsEnabled = Settings.Default.Parameters.WeightsGainVisible;
            textBlockWeightsGain.Visibility = Settings.Default.Parameters.WeightsGainVisible ? Visibility.Visible : Visibility.Collapsed;
            textBoxWeightsGain.IsEnabled = Settings.Default.Parameters.WeightsGainVisible;
            textBoxWeightsGain.Visibility = Settings.Default.Parameters.WeightsGainVisible ? Visibility.Visible : Visibility.Collapsed;

            Grid.RowDefinitions[20].Height = Settings.Default.Parameters.WeightsScaleVisible ? new GridLength(30, GridUnitType.Pixel) : new GridLength(0, GridUnitType.Pixel);
            textBlockWeightsScale.IsEnabled = Settings.Default.Parameters.WeightsScaleVisible;
            textBlockWeightsScale.Visibility = Settings.Default.Parameters.WeightsScaleVisible ? Visibility.Visible : Visibility.Collapsed;

            comboBoxBiasesFiller.IsEnabled = CheckBoxHasBias.IsChecked.Value;

            Grid.RowDefinitions[24].Height = CheckBoxHasBias.IsChecked.Value && Settings.Default.Parameters.BiasesFillerModeVisible ? new GridLength(30, GridUnitType.Pixel) : new GridLength(0, GridUnitType.Pixel);
            comboBoxBiasesFillerMode.IsEnabled = CheckBoxHasBias.IsChecked.Value && Settings.Default.Parameters.BiasesFillerModeVisible;
            comboBoxBiasesFillerMode.Visibility = CheckBoxHasBias.IsChecked.Value && Settings.Default.Parameters.BiasesFillerModeVisible ? Visibility.Visible : Visibility.Collapsed;

            Grid.RowDefinitions[25].Height = CheckBoxHasBias.IsChecked.Value && Settings.Default.Parameters.BiasesGainVisible ? new GridLength(30, GridUnitType.Pixel) : new GridLength(0, GridUnitType.Pixel);
            textBlockBiasesGain.IsEnabled = CheckBoxHasBias.IsChecked.Value && Settings.Default.Parameters.BiasesGainVisible;
            textBlockBiasesGain.Visibility = CheckBoxHasBias.IsChecked.Value && Settings.Default.Parameters.BiasesGainVisible ? Visibility.Visible : Visibility.Collapsed;
            textBoxBiasesGain.IsEnabled = CheckBoxHasBias.IsChecked.Value && Settings.Default.Parameters.BiasesGainVisible;
            textBoxBiasesGain.Visibility = CheckBoxHasBias.IsChecked.Value && Settings.Default.Parameters.BiasesGainVisible ? Visibility.Visible : Visibility.Collapsed;

            Grid.RowDefinitions[26].Height = CheckBoxHasBias.IsChecked.Value && Settings.Default.Parameters.BiasesScaleVisible ? new GridLength(30, GridUnitType.Pixel) : new GridLength(0, GridUnitType.Pixel);
            textBoxBiasesScale.IsEnabled = CheckBoxHasBias.IsChecked.Value && Settings.Default.Parameters.BiasesScaleVisible;
            textBlockBiasesScale.Visibility = CheckBoxHasBias.IsChecked.Value && Settings.Default.Parameters.BiasesScaleVisible ? Visibility.Visible : Visibility.Collapsed;

            Grid.RowDefinitions[27].Height = CheckBoxHasBias.IsChecked.Value ? new GridLength(30, GridUnitType.Pixel) : new GridLength(0, GridUnitType.Pixel);
            textBoxBiasesLRM.IsEnabled = CheckBoxHasBias.IsChecked.Value;
            textBlockBiasesLRM.Visibility = CheckBoxHasBias.IsChecked.Value ? Visibility.Visible : Visibility.Collapsed;

            Grid.RowDefinitions[28].Height = CheckBoxHasBias.IsChecked.Value ? new GridLength(30, GridUnitType.Pixel) : new GridLength(0, GridUnitType.Pixel);
            textBoxBiasesWDM.IsEnabled = CheckBoxHasBias.IsChecked.Value;
            textBlockBiasesWDM.Visibility = CheckBoxHasBias.IsChecked.Value ? Visibility.Visible : Visibility.Collapsed;

            int removeRows = CheckBoxHasBias.IsChecked.Value && Settings.Default.Parameters.BiasesScaleVisible ? 2 : 1;
            removeRows += CheckBoxHasBias.IsChecked.Value ? 0 : 4;
            removeRows += Settings.Default.Parameters.WeightsFillerModeVisible ? 1 : 2;

            switch ((Scripts)comboBoxModel.SelectedIndex)
            {
                case Scripts.densenet:
                case Scripts.resnet:
                    removeRows += 7;
                    break;
                case Scripts.efficientnetv2:
                    removeRows += 10;
                    break;
                case Scripts.mobilenetv3:
                case Scripts.shufflenetv2:
                    removeRows += 9;
                    break;
            }
            Height = 1150 - (removeRows * Grid.RowDefinitions[0].ActualHeight);
        }

        private void ButtonFormulaWeights_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var info = new ScriptsDialog.WeightsInitializers();
            info.ShowDialog();
        }

        private void ButtonFormulaWeights_Click(object sender, RoutedEventArgs e)
        {
            var filler = (Fillers)comboBoxWeightsFiller.SelectedValue;

            foreach (var item in fillersList)
            {
                if (item.Id == filler)
                {
                    FormulaWeightsCtrl.Formula = item.Formula;
                    break;
                }
            }
        }

        private void ButtonFormulaBiases_Click(object sender, RoutedEventArgs e)
        {
            if (CheckBoxHasBias.IsChecked.HasValue && CheckBoxHasBias.IsChecked.Value)
            {
                var filler = (Fillers)comboBoxBiasesFiller.SelectedValue;

                foreach (var item in fillersList)
                {
                    if (item.Id == filler)
                    {
                        FormulaBiasesCtrl.Formula = item.Formula;
                        break;
                    }
                }
            }
            else
                FormulaBiasesCtrl.Formula = "none";
        }
    }
}