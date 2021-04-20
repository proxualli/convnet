using Convnet.PageViewModels;
using Convnet.Properties;
using dnncore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Convnet.Dialogs
{
    public partial class TrainingSchemeEditor : Window
    {
        public static IEnumerable<DNNOptimizers> GetOptimizers => Enum.GetValues(typeof(DNNOptimizers)).Cast<DNNOptimizers>();
        public static IEnumerable<DNNInterpolation> GetInterpolations => Enum.GetValues(typeof(DNNInterpolation)).Cast<DNNInterpolation>();
        public string Path { get; set; }
        
        public TrainPageViewModel trainingPageViewModel;
       

        private readonly Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
        private readonly Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();

        public TrainingSchemeEditor()
        {
            InitializeComponent();
        }

        void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DataGridRates.ItemsSource = (this.DataContext as TrainPageViewModel).TrainRates;
        }
        /*
        void DataGridRates_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.Column.Header.ToString() == "OptimizerList")
                e.Cancel = true;
            if (e.Column.Header.ToString() == "InterpolationList")
                e.Cancel = true;

            if (e.Column.Header.ToString() == "Optimizer")
                e.Column.DisplayIndex = 0;
            if (e.Column.Header.ToString() == "Momentum")
                e.Column.DisplayIndex = 0;
            if (e.Column.Header.ToString() == "Beta2")
                e.Column.DisplayIndex = 0;
            if (e.Column.Header.ToString() == "L2Penalty")
                e.Column.DisplayIndex = 0;
            if (e.Column.Header.ToString() == "Eps")
                e.Column.DisplayIndex = 0;
            if (e.Column.Header.ToString() == "BatchSize")
                e.Column.DisplayIndex = 0;
            if (e.Column.Header.ToString() == "Cycles")
                e.Column.DisplayIndex = 0;
            if (e.Column.Header.ToString() == "Epochs")
                e.Column.DisplayIndex = 0;
            if (e.Column.Header.ToString() == "EpochMultiplier")
                e.Column.DisplayIndex = 0;
            if (e.Column.Header.ToString() == "MaximumRate")
                e.Column.DisplayIndex = 0;
            if (e.Column.Header.ToString() == "MinimumRate")
                e.Column.DisplayIndex = 0;
            if (e.Column.Header.ToString() == "DecayAfterEpochs")
                e.Column.DisplayIndex = 0;
            if (e.Column.Header.ToString() == "DecayFactor")
                e.Column.DisplayIndex = 0;
            if (e.Column.Header.ToString() == "HorizontalFlip")
                e.Column.DisplayIndex = 0;
            if (e.Column.Header.ToString() == "VerticalFlip")
                e.Column.DisplayIndex = 0;
            if (e.Column.Header.ToString() == "Dropout")
                e.Column.DisplayIndex = 0;
            if (e.Column.Header.ToString() == "Cutout")
                e.Column.DisplayIndex = 0;
            if (e.Column.Header.ToString() == "AutoAugment")
                e.Column.DisplayIndex = 0;
            if (e.Column.Header.ToString() == "ColorCast")
                e.Column.DisplayIndex = 0;
            if (e.Column.Header.ToString() == "ColorAngle")
                e.Column.DisplayIndex = 0;
            if (e.Column.Header.ToString() == "Distortion")
                e.Column.DisplayIndex = 0;
            if (e.Column.Header.ToString() == "Interpolation")
                e.Column.DisplayIndex = 0;
            if (e.Column.Header.ToString() == "Scaling")
                e.Column.DisplayIndex = 0;
            if (e.Column.Header.ToString() == "Rotation")
                e.Column.DisplayIndex = 0;
        }
        */
        void ButtonInsert_Click(object sender, RoutedEventArgs e)
        {
            DataGridRates.CommitEdit();
            
            int selectedIndex = DataGridRates.SelectedIndex;
            if (selectedIndex != -1)
            {
                trainingPageViewModel.TrainRates.Insert(selectedIndex, Settings.Default.TrainRate);
                DataGridRates.SelectedIndex = selectedIndex;
                DataGridRates.Focus();
            }
        }

        void ButtonTrain_Click(object sender, RoutedEventArgs e)
        {
            bool stochastic = false;

            foreach (DNNTrainingRate rate in trainingPageViewModel.TrainRates)
            {
                if (rate.BatchSize == 1)
                {
                    stochastic = true;
                    break;
                }
            }

            if (trainingPageViewModel.Model.BatchNormalizationUsed() && stochastic)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Your model uses batch normalization.\r\nThe batch size cannot be equal to 1 in this case.", "Warning", MessageBoxButton.OK);

                return;
            }


            DialogResult = true;
            this.Close();
        }

        void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            saveFileDialog.InitialDirectory = Path;
            saveFileDialog.Filter = "Xml Training Scheme|*.scheme-xml";
            saveFileDialog.DefaultExt = ".scheme-xml";
            saveFileDialog.AddExtension = true;
            saveFileDialog.CreatePrompt = false;
            saveFileDialog.OverwritePrompt = true;
            saveFileDialog.ValidateNames = true;

            if (saveFileDialog.ShowDialog(this) == true)
            {
                string fileName = saveFileDialog.FileName;

                Mouse.OverrideCursor = Cursors.Wait;
                if (fileName.Contains("scheme-xml"))
                {
                    using (DNNDataSet.TrainingRatesDataTable table = new DNNDataSet.TrainingRatesDataTable())
                    {
                        table.BeginLoadData();
                        foreach (DNNTrainingRate rate in trainingPageViewModel.TrainRates)
                            table.AddTrainingRatesRow((int)rate.Optimizer, (double)rate.Beta2, (double)rate.Eps, (double)rate.MaximumRate, (int)rate.BatchSize, (int)rate.Cycles, (int)rate.Epochs, (int)rate.EpochMultiplier, (double)rate.MinimumRate, (double)rate.L2Penalty, (double)rate.Momentum, (double)rate.DecayFactor, (int)rate.DecayAfterEpochs, rate.HorizontalFlip, rate.VerticalFlip, (double)rate.Dropout, (double)rate.Cutout, (double)rate.AutoAugment, (double)rate.ColorCast, (int)rate.ColorAngle, (double)rate.Distortion, (int)rate.Interpolation, (double)rate.Scaling, (double)rate.Rotation);
                        table.EndLoadData();
                        table.WriteXml(fileName, System.Data.XmlWriteMode.WriteSchema);
                    }
                    Mouse.OverrideCursor = null;
                    Title = "Training Scheme Editor - " + saveFileDialog.SafeFileName.Replace(".scheme-xml", "");
                    Xceed.Wpf.Toolkit.MessageBox.Show("Training scheme is saved", "Information", MessageBoxButton.OK);
                }
            }
            Mouse.OverrideCursor = null;
        }

        void ButtonLoad_Click(object sender, RoutedEventArgs e)
        {
            openFileDialog.InitialDirectory = Path;
            openFileDialog.Filter = "Xml Training Scheme|*.scheme-xml";
            openFileDialog.Title = "Load Training Scheme";
            openFileDialog.DefaultExt = ".scheme-xml";
            openFileDialog.CheckFileExists = true;
            openFileDialog.CheckPathExists = true;
            openFileDialog.Multiselect = false;
            openFileDialog.ValidateNames = true;

            bool stop = false;
            while (!stop)
            {
                if (openFileDialog.ShowDialog(this) == true)
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    string fileName = openFileDialog.FileName;

                    if (fileName.Contains(".scheme-xml"))
                    {
                        try
                        {
                            using (DNNDataSet.TrainingRatesDataTable table = new DNNDataSet.TrainingRatesDataTable())
                            {
                                table.ReadXml(fileName);

                                trainingPageViewModel.TrainRates.Clear();

                                foreach (DNNDataSet.TrainingRatesRow row in table)
                                    trainingPageViewModel.TrainRates.Add(new DNNTrainingRate((DNNOptimizers)row.Optimizer, (float)row.Momentum, (float)row.L2Penalty, (float)row.Beta2, (float)row.Eps, (uint)row.BatchSize, (uint)row.Cycles, (uint)row.Epochs, (uint)row.EpochMultiplier, (float)row.MaximumRate, (float)row.MinimumRate, (float)row.DecayFactor, (uint)row.DecayAfterEpochs, row.HorizontalFlip, row.VerticalFlip, (float)row.Dropout, (float)row.Cutout, (float)row.AutoAugment, (float)row.ColorCast, (uint)row.ColorAngle, (float)row.Distortion, (DNNInterpolation)row.Interpolation, (float)row.MaxScaling, (float)row.MaxRotation));
                            }

                            Mouse.OverrideCursor = null;
                            this.Title = "Training Scheme Editor - " + openFileDialog.SafeFileName.Replace(".scheme-xml", "");
                            stop = true;
                        }
                        catch (Exception)
                        {
                            Mouse.OverrideCursor = null;
                            Xceed.Wpf.Toolkit.MessageBox.Show("Wrong file format", "Choose a different file", MessageBoxButton.OK);
                        }
                    }
                    else
                    {
                        Mouse.OverrideCursor = null;
                        Xceed.Wpf.Toolkit.MessageBox.Show("Wrong file format", "Choose a different file", MessageBoxButton.OK);
                    }
                }
                else
                    stop = true;
            }
            Mouse.OverrideCursor = null;
        }
    }
}