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
    public partial class TrainingSchemeEditor : Window
    {
        public string Path { get; set; }

        public TrainPageViewModel tpvm;
       
        private readonly Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
        private readonly Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();

        public TrainingSchemeEditor()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ChangeSGDR();
        }

        private void ButtonSGDRHelp_Click(object sender, RoutedEventArgs e)
        {
            ApplicationHelper.OpenBrowser("https://arxiv.org/abs/1608.03983");
        }

        private void CheckBoxSGDR_Checked(object sender, RoutedEventArgs e)
        {
            ChangeSGDR();
        }

        private void CheckBoxSGDR_Unchecked(object sender, RoutedEventArgs e)
        {
            ChangeSGDR();
        }

        private void ChangeSGDR()
        {
            DataGridRates.Columns[11].Visibility = tpvm.SGDR ? Visibility.Visible : Visibility.Collapsed;
            DataGridRates.Columns[13].Visibility = tpvm.SGDR ? Visibility.Visible : Visibility.Collapsed;

            DataGridRates.Columns[18].Visibility = tpvm.SGDR ? Visibility.Collapsed : Visibility.Visible;
            //DataGridRates.Columns[19].Visibility = tpvm.SGDR ? Visibility.Collapsed: Visibility.Visible;
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

        private void ButtonInsert_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = DataGridRates.SelectedIndex;
            if (selectedIndex != -1)
            {
                tpvm.TrainRates.Insert(selectedIndex, Settings.Default.TraininingRate);
                DataGridRates.SelectedIndex = selectedIndex;
                DataGridRates.Focus();
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ButtonTrain_Click(object sender, RoutedEventArgs e)
        {
            if (IsValid(this))
            {
                bool stochastic = false;

                foreach (DNNTrainingRate rate in tpvm.TrainRates)
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

                ulong totalEpochs = 0;
                foreach (DNNTrainingRate rate in tpvm.TrainRates)
                    totalEpochs += tpvm.SGDR ? rate.Epochs * rate.Cycles * rate.EpochMultiplier : rate.Epochs;
  
                if (uint.TryParse(textBoxGotoEpoch.Text, out uint gotoEpoch))
                {
                    if (gotoEpoch > totalEpochs)
                    {
                        Xceed.Wpf.Toolkit.MessageBox.Show("Goto epoch is to large", "Warning", MessageBoxButton.OK);
                        return;
                    }

                    Settings.Default.TrainingRates = tpvm.TrainRates;
                    Settings.Default.GotoEpoch = gotoEpoch;
                    Settings.Default.Save();
                    
                    DialogResult = true;
                    this.Close();
                }
            }
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            saveFileDialog.InitialDirectory = Path;
            saveFileDialog.Filter = "Training Scheme|*.csv";
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
                        csv.WriteRecords(tpvm.TrainRates);
                    }

                    Mouse.OverrideCursor = null;
                    Title = "Training Scheme Editor - " + fileName.Replace(".csv", "");
                    Xceed.Wpf.Toolkit.MessageBox.Show("Training scheme is saved", "Information", MessageBoxButton.OK);
                }
            }
        }

        private void ButtonLoad_Click(object sender, RoutedEventArgs e)
        {
            openFileDialog.InitialDirectory = Path;
            openFileDialog.Filter = "Training Scheme|*.csv";
            openFileDialog.Title = "Load Training Scheme";
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
                                var records = csv.GetRecords<DNNTrainingRate>();

                                if (Settings.Default.TrainingRates.Count > 0)
                                {
                                    Mouse.OverrideCursor = null;
                                    if (Xceed.Wpf.Toolkit.MessageBox.Show("Do you want to clear the existing sheme?", "Clear Training Scheme", MessageBoxButton.YesNo, MessageBoxImage.None, MessageBoxResult.No) == MessageBoxResult.Yes)
                                        Settings.Default.TrainingRates.Clear();
                                }

                                foreach (var record in records)
                                    Settings.Default.TrainingRates.Add(record);

                            }

                            Mouse.OverrideCursor = null;
                            Title = "Training Scheme Editor - " + fileName.Replace(".csv", "");
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