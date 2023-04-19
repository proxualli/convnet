using Convnet.Common;
using Convnet.PageViewModels;
using Convnet.Properties;
using CsvHelper;
using dnncore;
using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Cursors = System.Windows.Input.Cursors;

namespace Convnet.Dialogs
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
    public partial class TrainingStrategiesEditor : Window
    {
        public string Path { get; set; }

        public TrainPageViewModel tpvm;

        private readonly Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
        private readonly Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();

        public TrainingStrategiesEditor()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private bool IsValid(DependencyObject node)
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

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            if (IsValid(this))
            {
                bool stochastic = false;

                foreach (DNNTrainingStrategy rate in tpvm.TrainingStrategies)
                {
                    if (rate.BatchSize == 1)
                    {
                        stochastic = true;
                        break;
                    }
                }

                if (tpvm.Model.BatchNormUsed() && stochastic)
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("Your model uses batch normalization.\r\nThe batch size cannot be equal to 1 in this case.", "Warning", MessageBoxButton.OK);
                    return;
                }

                float totalEpochs = 0;
                foreach (DNNTrainingStrategy rate in tpvm.TrainingStrategies)
                    totalEpochs += rate.Epochs;

                if (totalEpochs < 0.9995 || totalEpochs > 1.0005)
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("Sum of Epochs must be exactly 1.", "Warning", MessageBoxButton.OK);
                    return;
                }

                Settings.Default.TrainingStrategies = tpvm.TrainingStrategies;
                Settings.Default.Save();

                DialogResult = true;
                Close();
            }
        }

        private void ButtonInsert_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = DataGridStrategies.SelectedIndex;
            if (selectedIndex != -1)
            {
                var rate = Settings.Default.TraininingRate != null ? Settings.Default.TraininingRate : new DNNTrainingRate();
                var strategy = new DNNTrainingStrategy(1, rate.N, rate.H, rate.W, rate.PadH, rate.PadW, rate.Momentum, rate.Beta2, rate.Gamma, rate.L2Penalty, rate.Dropout, rate.HorizontalFlip, rate.VerticalFlip, rate.InputDropout, rate.Cutout, rate.CutMix, rate.AutoAugment, rate.ColorCast, rate.ColorAngle, rate.Distortion, rate.Interpolation, rate.Scaling, rate.Rotation);
                tpvm.TrainingStrategies.Insert(selectedIndex, strategy);
                DataGridStrategies.SelectedIndex = selectedIndex;
                DataGridStrategies.Focus();
            }
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            saveFileDialog.InitialDirectory = Path;
            saveFileDialog.Filter = "Training Strategy|*.csv";
            saveFileDialog.DefaultExt = ".csv";
            saveFileDialog.AddExtension = true;
            saveFileDialog.CreatePrompt = false;
            saveFileDialog.OverwritePrompt = true;
            saveFileDialog.ValidateNames = true;

            if (saveFileDialog.ShowDialog(this) == true)
            {
                var fileName = saveFileDialog.FileName;

                if (fileName.Contains("csv"))
                {
                    Mouse.OverrideCursor = Cursors.Wait;

                    CsvHelper.Configuration.CsvConfiguration config = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.CurrentCulture)
                    {
                        HasHeaderRecord = true,
                        DetectDelimiter = true,
                        DetectDelimiterValues = new string[] { ";" },
                        Delimiter = ";"
                    };

                    using (var writer = new StreamWriter(fileName, false))
                    using (var csv = new CsvWriter(writer, config))
                    {
                        csv.WriteRecords(tpvm.TrainingStrategies);
                    }

                    Mouse.OverrideCursor = null;
                    Title = "Training Strategy Editor - " + fileName.Replace(".csv", "");
                    Xceed.Wpf.Toolkit.MessageBox.Show("Training strategy is saved", "Information", MessageBoxButton.OK);
                }
            }
        }

        private void ButtonLoad_Click(object sender, RoutedEventArgs e)
        {
            openFileDialog.InitialDirectory = Path;
            openFileDialog.Filter = "Training Strategy|*.csv";
            openFileDialog.Title = "Load Training Strategy";
            openFileDialog.DefaultExt = ".csv";
            openFileDialog.CheckFileExists = true;
            openFileDialog.CheckPathExists = true;
            openFileDialog.Multiselect = false;
            openFileDialog.ValidateNames = true;

            var stop = false;

            while (!stop)
            {
                if (openFileDialog.ShowDialog(this) == true)
                {

                    var fileName = openFileDialog.FileName;

                    if (fileName.Contains(".csv"))
                    {
                        try
                        {
                            Mouse.OverrideCursor = Cursors.Wait;

                            CsvHelper.Configuration.CsvConfiguration config = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.CurrentCulture)
                            {
                                HasHeaderRecord = true,
                                DetectDelimiter = true,
                                DetectDelimiterValues = new string[] { ";" },
                                Delimiter = ";"
                            };

                            using (var reader = new StreamReader(fileName, true))
                            using (var csv = new CsvReader(reader, config))
                            {
                                var records = csv.GetRecords<DNNTrainingStrategy>();

                                if (Settings.Default.TrainingStrategies.Count > 0)
                                {
                                    Mouse.OverrideCursor = null;
                                    if (Xceed.Wpf.Toolkit.MessageBox.Show("Do you want to clear the existing strategy?", "Clear Training Strategy", MessageBoxButton.YesNo, MessageBoxImage.None, MessageBoxResult.No) == MessageBoxResult.Yes)
                                        Settings.Default.TrainingStrategies.Clear();
                                }

                                foreach (var record in records)
                                    Settings.Default.TrainingStrategies.Add(record);

                            }

                            Mouse.OverrideCursor = null;
                            Title = "Training Strategy Editor - " + fileName.Replace(".csv", "");
                            stop = true;
                        }
                        catch (Exception)
                        {
                            Mouse.OverrideCursor = null;
                            Xceed.Wpf.Toolkit.MessageBox.Show("Wrong file format", "Choose a different file", MessageBoxButton.OK);
                        }
                    }
                    else
                        Xceed.Wpf.Toolkit.MessageBox.Show("Wrong file format", "Choose a different file", MessageBoxButton.OK);
                }
                else
                    stop = true;
            }
        }
    }
}