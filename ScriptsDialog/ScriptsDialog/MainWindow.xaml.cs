using ScriptsDialog.Properties;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ScriptsDialog
{
    /// <summary>
    /// Interaction logic for ModelParameters.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
#if DEBUG
        const string Mode = "Debug";
#else
        const string Mode = "Release";
#endif
        const string Framework = @"\netcoreapp3.1\";
        public static string StorageDirectory { get; } = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\convnet\";
        public static string ScriptsDirectory { get; } = StorageDirectory + @"scripts\";
        public static string ScriptPath { get; } = ScriptsDirectory + @"ScriptsDialog\bin\" + Mode + Framework;
      
        public MainWindow()
        {
            InitializeComponent();

            if (Settings.Default.Parameters == null)
            {
                Settings.Default.Parameters = new ScriptParameters(Scripts.shufflenetv2, Datasets.cifar10, 32, 32, 4, 4, false, true, Fillers.HeNormal, 0.05f, 1f, 1f, false, Fillers.Constant, 0f, 1f, 1f, 0.995f, 0.0001f, false, 0f, 0f, 3, 4, 8, 12, false, 0.0f, 0.0f, false, true, true);
                Settings.Default.Save();
            }
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
            DataContext = Settings.Default.Parameters;

            if (Settings.Default.Parameters.Bottleneck)
                Settings.Default.Parameters.Compression = 0.5f;
            else
                Settings.Default.Parameters.Compression = 1.0f;

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
                streamWriter.Flush();
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
                    Settings.Default.Parameters.BiasesScale = 0.0f;
                    Settings.Default.Parameters.BiasesLRM = 1.0f;
                    Settings.Default.Parameters.BiasesWDM = 1.0f;
                }

                Calc();
            }
        }

        private void radioButtonBottleneckYesNo_Checked(object sender, RoutedEventArgs e)
        {
            switch ((Scripts)comboBoxModel.SelectedIndex)
            {
                case Scripts.densenet:
                    if (radioButtonBottleneckYes.IsChecked.HasValue && radioButtonBottleneckYes.IsChecked.Value)
                        Settings.Default.Parameters.Compression = 0.5f;
                    if (radioButtonBottleneckNo.IsChecked.HasValue && radioButtonBottleneckNo.IsChecked.Value)
                        Settings.Default.Parameters.Compression = 1.0f;
                    break;
            }
        }

        private void Calc()
        {
            comboBoxBiasesFiller.IsEnabled = CheckBoxHasBias.IsChecked.Value;

            Grid.RowDefinitions[21].Height = CheckBoxHasBias.IsChecked.Value && Settings.Default.Parameters.BiasesScaleVisible ? new GridLength(30, GridUnitType.Pixel) : new GridLength(0, GridUnitType.Pixel);
            textBoxBiasesScale.IsEnabled = CheckBoxHasBias.IsChecked.Value && Settings.Default.Parameters.BiasesScaleVisible;
            textBlockBiasesScale.Visibility = CheckBoxHasBias.IsChecked.Value && Settings.Default.Parameters.BiasesScaleVisible ? Visibility.Visible : Visibility.Collapsed;

            Grid.RowDefinitions[22].Height = CheckBoxHasBias.IsChecked.Value ? new GridLength(30, GridUnitType.Pixel) : new GridLength(0, GridUnitType.Pixel);
            textBoxBiasesLRM.IsEnabled = CheckBoxHasBias.IsChecked.Value;
            textBlockBiasesLRM.Visibility = CheckBoxHasBias.IsChecked.Value ? Visibility.Visible : Visibility.Collapsed;

            Grid.RowDefinitions[23].Height = CheckBoxHasBias.IsChecked.Value ? new GridLength(30, GridUnitType.Pixel) : new GridLength(0, GridUnitType.Pixel);
            textBoxBiasesWDM.IsEnabled = CheckBoxHasBias.IsChecked.Value;
            textBlockBiasesWDM.Visibility = CheckBoxHasBias.IsChecked.Value ? Visibility.Visible : Visibility.Collapsed;

            int removeRows = Settings.Default.Parameters.WeightsScaleVisible ? 0 : 1;
            removeRows += CheckBoxHasBias.IsChecked.Value && Settings.Default.Parameters.BiasesScaleVisible ? 0 : 1;
            removeRows += CheckBoxHasBias.IsChecked.Value ? 0 : 2;
          
            double rowHeight = Grid.RowDefinitions[0].ActualHeight;
            const double totalHeight = 910;
            switch ((Scripts)comboBoxModel.SelectedIndex)
            {
                case Scripts.densenet:
                case Scripts.resnet:
                    Height = totalHeight - ((3 + removeRows) * rowHeight);
                    break;
                case Scripts.mobilenetv3:
                case Scripts.shufflenetv2:
                    Height = totalHeight - ((5 + removeRows) * rowHeight);
                    break;
            }
        }
    }
}