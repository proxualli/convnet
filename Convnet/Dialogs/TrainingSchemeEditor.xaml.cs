using Convnet.Common;
using Convnet.PageViewModels;
using dnncore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using Cursors = System.Windows.Input.Cursors;

namespace Convnet.Dialogs
{
    public partial class TrainingSchemeEditor : Window
    {
        public static IEnumerable<DNNOptimizers> GetOptimizers => Enum.GetValues(typeof(DNNOptimizers)).Cast<DNNOptimizers>();
        public static IEnumerable<DNNInterpolation> GetInterpolations => Enum.GetValues(typeof(DNNInterpolation)).Cast<DNNInterpolation>();
        public string Path { get; set; }

        public TrainPageViewModel tpvm;
       
        private readonly Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
        private readonly Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();

        public TrainingSchemeEditor()
        {
            InitializeComponent();
        }

        public static void PostitionWindowOnScreen(Window window, double horizontalShift = 0, double verticalShift = 0)
        {
            Screen screen = Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(window).Handle);
            window.Left = screen.Bounds.X + ((screen.Bounds.Width - window.ActualWidth) / 2) + horizontalShift;
            window.Top = screen.Bounds.Y + ((screen.Bounds.Height - window.ActualHeight) / 2) + verticalShift;
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

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        void ButtonTrain_Click(object sender, RoutedEventArgs e)
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

                if (tpvm.Model.BatchNormalizationUsed() && stochastic)
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("Your model uses batch normalization.\r\nThe batch size cannot be equal to 1 in this case.", "Warning", MessageBoxButton.OK);
                    return;
                }

                ulong totalEpochs = 0;
                foreach (DNNTrainingRate rate in tpvm.TrainRates)
                    totalEpochs += rate.Epochs;
  
                if (uint.TryParse(textBoxGotoEpoch.Text, out uint gotoEpoch))
                {

                    if (gotoEpoch > totalEpochs)
                    {
                        Xceed.Wpf.Toolkit.MessageBox.Show("Value Goto Epoch is to large", "Warning", MessageBoxButton.OK);
                        return;
                    }

                    Properties.Settings.Default.GotoEpoch = gotoEpoch;
                    Properties.Settings.Default.Save();
                    
                    DialogResult = true;
                    this.Close();
                }
            }
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
                        foreach (DNNTrainingRate rate in tpvm.TrainRates)
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

                                tpvm.TrainRates.Clear();

                                foreach (DNNDataSet.TrainingRatesRow row in table)
                                    tpvm.TrainRates.Add(new DNNTrainingRate((DNNOptimizers)row.Optimizer, (float)row.Momentum, (float)row.Beta2, (float)row.L2Penalty,  (float)row.Eps, (uint)row.BatchSize, (uint)row.Cycles, (uint)row.Epochs, (uint)row.EpochMultiplier, (float)row.MaximumRate, (float)row.MinimumRate, (float)row.DecayFactor, (uint)row.DecayAfterEpochs, row.HorizontalFlip, row.VerticalFlip, (float)row.Dropout, (float)row.Cutout, (float)row.AutoAugment, (float)row.ColorCast, (uint)row.ColorAngle, (float)row.Distortion, (DNNInterpolation)row.Interpolation, (float)row.MaxScaling, (float)row.MaxRotation));
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ChangeSGDR();
        }

        private void ChangeSGDR()
        {
            DataGridRates.Columns[6].Visibility = tpvm.SGDR ? Visibility.Visible : Visibility.Collapsed;
            DataGridRates.Columns[8].Visibility = tpvm.SGDR ? Visibility.Visible : Visibility.Collapsed;
            
            //var width = tpvm.SGDR ? DataGridRates.Columns[6].ActualWidth + DataGridRates.Columns[8].ActualWidth : 0;
            //PostitionWindowOnScreen(this, -width, 0);

            textBoxGotoCycle.Visibility = tpvm.SGDR ? Visibility.Visible : Visibility.Collapsed;
            LabelGotoCycle.Visibility = tpvm.SGDR ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}