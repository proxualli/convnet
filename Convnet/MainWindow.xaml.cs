using Convnet.Common;
using Convnet.Dialogs;
using Convnet.PageViewModels;
using Convnet.Properties;
using CsvHelper;
using dnncore;
using OxyPlot;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;


namespace Convnet
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
    public partial class MainWindow : Window, IDisposable
    {
        const string Framework = "netcoreapp3.1";
        //const string Framework = "net7.0-windows";

#if DEBUG
        const string Mode = "Debug";
#else
        const string Mode = "Release";
#endif
        public static string ApplicationPath { get; } = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\";
        public static string StorageDirectory { get; } = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\convnet\";
        public static string StateDirectory { get; } = StorageDirectory + @"state\";
        public static string DefinitionsDirectory { get; } = StorageDirectory + @"definitions\";
        public static string ScriptsDirectory { get; } = StorageDirectory + @"scripts\";

        public static RoutedUICommand ResetCmd = new RoutedUICommand();
        public static RoutedUICommand AboutCmd = new RoutedUICommand();
        public static RoutedUICommand DisableLockingCmd = new RoutedUICommand();
        public static RoutedUICommand PlainFormatCmd = new RoutedUICommand();
        public static RoutedUICommand LockAllCmd = new RoutedUICommand();
        public static RoutedUICommand UnlockAllCmd = new RoutedUICommand();
        public static RoutedUICommand PersistOptimizerCmd = new RoutedUICommand();

        public bool ShowCloseApplicationDialog = true;
        public PageViewModel PageVM;

        public static void Copy(string sourceDirectory, string targetDirectory)
        {
            var diSource = new DirectoryInfo(sourceDirectory);
            var diTarget = new DirectoryInfo(targetDirectory);

            CopyAll(diSource, diTarget);
        }

        public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (var fi in source.GetFiles())
            {
                //Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (var diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            Directory.CreateDirectory(StorageDirectory);
            Directory.CreateDirectory(DefinitionsDirectory);
            Directory.CreateDirectory(StateDirectory);

            if (!Directory.Exists(ScriptsDirectory))
            {
                Directory.CreateDirectory(ScriptsDirectory);
                Copy(ApplicationPath.Replace(@"Convnet\bin\x64\" + Mode + @"\" + Framework + @"\", "") + @"ScriptsDialog\", ScriptsDirectory);
            }

            var fileName = Path.Combine(StateDirectory, Settings.Default.ModelNameActive + ".txt");
            var backupModelName = "resnet-3-2-6-channelzeropad-relu";
                        
            if (!File.Exists(Path.Combine(StateDirectory, backupModelName + ".txt")))
                File.Copy(ApplicationPath + @"Resources\state\" + backupModelName + ".txt", StateDirectory + backupModelName + ".txt", true);
            
            if (!File.Exists(fileName) || !File.ReadLines(Path.Combine(StateDirectory, backupModelName + ".txt")).SequenceEqual(File.ReadLines(ApplicationPath + @"Resources\state\" + backupModelName + ".txt")))
            {
                Directory.CreateDirectory(DefinitionsDirectory + backupModelName + @"\");
                File.Copy(ApplicationPath + @"Resources\state\" + backupModelName + ".txt", DefinitionsDirectory + backupModelName + ".txt", true);
                
                fileName = Path.Combine(StateDirectory, backupModelName + ".txt");
                Settings.Default.ModelNameActive = backupModelName;
                Settings.Default.DefinitionActive = File.ReadAllText(fileName);
                Settings.Default.Optimizer = DNNOptimizers.NAG;
                Settings.Default.Save();
            }

            try
            {
                var model = new DNNModel(Settings.Default.DefinitionActive);

                if (model != null)
                {
                    PageVM = new PageViewModel(model);

                    if (PageVM != null)
                    {
                        model.BackgroundColor = Settings.Default.BackgroundColor;
                        model.BlockSize = (ulong)Settings.Default.PixelSize;
                        model.TrainingStrategies = Settings.Default.TrainingStrategies;
                        model.ClearTrainingStrategies();
                        foreach (DNNTrainingStrategy strategy in Settings.Default.TrainingStrategies)
                            model.AddTrainingStrategy(strategy);
                        model.SetFormat(Settings.Default.PlainFormat);
                        model.SetOptimizer(Settings.Default.Optimizer);
                        model.SetPersistOptimizer(Settings.Default.PersistOptimizer);
                        model.SetUseTrainingStrategy(Settings.Default.UseTrainingStrategy);
                        model.SetDisableLocking(Settings.Default.DisableLocking);
                        model.SetShuffleCount((ulong)Math.Round(Settings.Default.Shuffle));

                        try
                        {
                            const string delim = ";";
                            var sb = new StringBuilder();
                            sb.AppendLine(
                                    "Cycle" + delim +
                                    "Epoch" + delim +
                                    "GroupIndex" + delim +
                                    "CostIndex" + delim +
                                    "CostName" + delim +
                                    "Optimizer" + delim +
                                    "Momentum" + delim +
                                    "Beta2" + delim +
                                    "Gamma" + delim +
                                    "L2Penalty" + delim +
                                    "Dropout" + delim +
                                    "Eps" + delim +
                                    "Rate" + delim +
                                    "N" + delim +
                                    "D" + delim +
                                    "H" + delim +
                                    "W" + delim +
                                    "PadD" + delim +
                                    "PadH" + delim +
                                    "PadW" + delim +
                                    "InputDropout" + delim +
                                    "Cutout" + delim +
                                    "CutMix" + delim +
                                    "AutoAugment" + delim +
                                    "HorizontalFlip" + delim +
                                    "VerticalFlip" + delim +
                                    "ColorCast" + delim +
                                    "ColorAngle" + delim +
                                    "Distortion" + delim +
                                    "Interpolation" + delim +
                                    "Scaling" + delim +
                                    "Rotation" + delim +
                                    "AvgTrainLoss" + delim +
                                    "TrainErrors" + delim +
                                    "TrainErrorPercentage" + delim +
                                    "TrainAccuracy" + delim +
                                    "AvgTestLoss" + delim +
                                    "TestErrors" + delim +
                                    "TestErrorPercentage" + delim +
                                    "TestAccuracy" + delim +
                                    "ElapsedTicks" + delim +
                                    "ElapsedTime");

                            foreach (var row in Settings.Default.TrainingLog)
                                sb.AppendLine(
                                    row.Cycle.ToString() + delim +
                                    row.Epoch.ToString() + delim +
                                    row.GroupIndex.ToString() + delim +
                                    row.CostIndex.ToString() + delim +
                                    row.CostName.ToString() + delim +
                                    row.Optimizer.ToString() + delim +
                                    row.Momentum.ToString() + delim +
                                    row.Beta2.ToString() + delim +
                                    row.Gamma.ToString() + delim +
                                    row.L2Penalty.ToString() + delim +
                                    row.Dropout.ToString() + delim +
                                    row.Eps.ToString() + delim +
                                    row.Rate.ToString() + delim +
                                    row.N.ToString() + delim +
                                    row.D.ToString() + delim +
                                    row.H.ToString() + delim +
                                    row.W.ToString() + delim +
                                    row.PadD.ToString() + delim +
                                    row.PadH.ToString() + delim +
                                    row.PadW.ToString() + delim +
                                    row.InputDropout.ToString() + delim +
                                    row.Cutout.ToString() + delim +
                                    row.CutMix.ToString() + delim +
                                    row.AutoAugment.ToString() + delim +
                                    row.HorizontalFlip.ToString() + delim +
                                    row.VerticalFlip.ToString() + delim +
                                    row.ColorCast.ToString() + delim +
                                    row.ColorAngle.ToString() + delim +
                                    row.Distortion.ToString() + delim +
                                    row.Interpolation.ToString() + delim +
                                    row.Scaling.ToString() + delim +
                                    row.Rotation.ToString() + delim +
                                    row.AvgTrainLoss.ToString() + delim +
                                    row.TrainErrors.ToString() + delim +
                                    row.TrainErrorPercentage.ToString() + delim +
                                    row.TrainAccuracy.ToString() + delim +
                                    row.AvgTestLoss.ToString() + delim +
                                    row.TestErrors.ToString() + delim +
                                    row.TestErrorPercentage.ToString() + delim +
                                    row.TestAccuracy.ToString() + delim +
                                    row.ElapsedTicks.ToString() + delim +
                                    row.ElapsedTime.ToString());

                            //Clipboard.SetText(sb.ToString());
                            string tmpFileName = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".csv";
                            File.WriteAllText(tmpFileName, sb.ToString());
                            PageVM.Model.LoadLog(tmpFileName);
                            File.Delete(tmpFileName);
                        }
                        catch (Exception ex)
                        {
                            Mouse.OverrideCursor = null;
                            Xceed.Wpf.Toolkit.MessageBox.Show(ex.ToString(), "Exception occured", MessageBoxButton.OK);
                        }


                        var dataset = PageVM.Model.Dataset.ToString().ToLower();
                        var optimizer = PageVM.Model.Optimizer.ToString().ToLower();

                        var fileNamePersistOptimizer = Path.Combine(StateDirectory, Settings.Default.ModelNameActive + "-(" + dataset + ")(" + optimizer + @").bin");
                        var fileNameNoOptimizer = Path.Combine(StateDirectory, Settings.Default.ModelNameActive + "-(" + dataset + ").bin");

                        var fileNameOptimizer = Settings.Default.PersistOptimizer ? fileNamePersistOptimizer : fileNameNoOptimizer;
                        var fileNameOptimizerInverse = Settings.Default.PersistOptimizer ? fileNameNoOptimizer : fileNamePersistOptimizer;

                        if (PageVM.Model.LoadWeights(fileNameOptimizer, Settings.Default.PersistOptimizer) != 0)
                            if (PageVM.Model.LoadWeights(fileNameOptimizerInverse, !Settings.Default.PersistOptimizer) == 0)
                            {
                                Settings.Default.PersistOptimizer = !Settings.Default.PersistOptimizer;
                                Settings.Default.Save();
                                PageVM.Model.SetPersistOptimizer(Settings.Default.PersistOptimizer);
                            }

                        //for (int i = 0; i < Settings.Default.TrainingLog.Count; i++)
                        //    Settings.Default.TrainingLog[i].ElapsedTime = TimeSpan.FromMilliseconds((double)Settings.Default.TrainingLog[i].ElapsedTicks);
                        //Settings.Default.Save();

                        Title = PageVM.Model.Name + " - Convnet Explorer";
                        DataContext = PageVM;

                        switch ((int)Math.Round(Settings.Default.PrioritySetter))
                        {
                            case 1:
                                PrioritySlider.ToolTip = "Low";
                                break;
                            case 2:
                                PrioritySlider.ToolTip = "Below Normal";
                                break;
                            case 3:
                                PrioritySlider.ToolTip = "Normal";
                                break;
                            case 4:
                                PrioritySlider.ToolTip = "Above Normal";
                                break;
                            case 5:
                                PrioritySlider.ToolTip = "High";
                                break;
                            case 6:
                                PrioritySlider.ToolTip = "Realtime";
                                break;
                        }
                    }
                    else
                        Xceed.Wpf.Toolkit.MessageBox.Show("Failed to create the PageViewModel: " + Settings.Default.ModelNameActive, "Error", MessageBoxButton.OK);
                }
                else
                {
                    // try backup model
                    File.Copy(ApplicationPath + @"Resources\state\" + backupModelName + ".txt", StateDirectory + backupModelName + ".txt", true);
                    fileName = Path.Combine(StateDirectory, backupModelName + ".txt");
                    Settings.Default.ModelNameActive = backupModelName;
                    Settings.Default.DefinitionActive = File.ReadAllText(fileName);
                    Settings.Default.Optimizer = DNNOptimizers.NAG;
                    Settings.Default.Save();

                    model = new DNNModel(Settings.Default.DefinitionActive);

                    if (model == null)
                    {
                        Xceed.Wpf.Toolkit.MessageBox.Show("Failed to create the Model: " + Settings.Default.ModelNameActive, "Error", MessageBoxButton.OK);
                        return;
                    }

                    PageVM = new PageViewModel(model);

                    if (PageVM != null)
                    {
                        model.BackgroundColor = Settings.Default.BackgroundColor;
                        model.BlockSize = (ulong)Settings.Default.PixelSize;
                        model.TrainingStrategies = Settings.Default.TrainingStrategies;
                        model.ClearTrainingStrategies();
                        foreach (DNNTrainingStrategy strategy in Settings.Default.TrainingStrategies)
                            model.AddTrainingStrategy(strategy);
                        model.SetFormat(Settings.Default.PlainFormat);
                        model.SetOptimizer(Settings.Default.Optimizer);
                        model.SetPersistOptimizer(Settings.Default.PersistOptimizer);
                        model.SetUseTrainingStrategy(Settings.Default.UseTrainingStrategy);
                        model.SetDisableLocking(Settings.Default.DisableLocking);
                        model.SetShuffleCount((ulong)Math.Round(Settings.Default.Shuffle));

                        try
                        {
                            const string delim = ";";
                            var sb = new StringBuilder();
                            sb.AppendLine(
                                    "Cycle" + delim +
                                    "Epoch" + delim +
                                    "GroupIndex" + delim +
                                    "CostIndex" + delim +
                                    "CostName" + delim +
                                    "Optimizer" + delim +
                                    "Momentum" + delim +
                                    "Beta2" + delim +
                                    "Gamma" + delim +
                                    "L2Penalty" + delim +
                                    "Dropout" + delim +
                                    "Eps" + delim +
                                    "Rate" + delim +
                                    "N" + delim +
                                    "D" + delim +
                                    "H" + delim +
                                    "W" + delim +
                                    "PadD" + delim +
                                    "PadH" + delim +
                                    "PadW" + delim +
                                    "InputDropout" + delim +
                                    "Cutout" + delim +
                                    "CutMix" + delim +
                                    "AutoAugment" + delim +
                                    "HorizontalFlip" + delim +
                                    "VerticalFlip" + delim +
                                    "ColorCast" + delim +
                                    "ColorAngle" + delim +
                                    "Distortion" + delim +
                                    "Interpolation" + delim +
                                    "Scaling" + delim +
                                    "Rotation" + delim +
                                    "AvgTrainLoss" + delim +
                                    "TrainErrors" + delim +
                                    "TrainErrorPercentage" + delim +
                                    "TrainAccuracy" + delim +
                                    "AvgTestLoss" + delim +
                                    "TestErrors" + delim +
                                    "TestErrorPercentage" + delim +
                                    "TestAccuracy" + delim +
                                    "ElapsedTicks" + delim +
                                    "ElapsedTime");

                            foreach (var row in Settings.Default.TrainingLog)
                                sb.AppendLine(
                                    row.Cycle.ToString() + delim +
                                    row.Epoch.ToString() + delim +
                                    row.GroupIndex.ToString() + delim +
                                    row.CostIndex.ToString() + delim +
                                    row.CostName.ToString() + delim +
                                    row.Optimizer.ToString() + delim +
                                    row.Momentum.ToString() + delim +
                                    row.Beta2.ToString() + delim +
                                    row.Gamma.ToString() + delim +
                                    row.L2Penalty.ToString() + delim +
                                    row.Dropout.ToString() + delim +
                                    row.Eps.ToString() + delim +
                                    row.Rate.ToString() + delim +
                                    row.N.ToString() + delim +
                                    row.D.ToString() + delim +
                                    row.H.ToString() + delim +
                                    row.W.ToString() + delim +
                                    row.PadD.ToString() + delim +
                                    row.PadH.ToString() + delim +
                                    row.PadW.ToString() + delim +
                                    row.InputDropout.ToString() + delim +
                                    row.Cutout.ToString() + delim +
                                    row.CutMix.ToString() + delim +
                                    row.AutoAugment.ToString() + delim +
                                    row.HorizontalFlip.ToString() + delim +
                                    row.VerticalFlip.ToString() + delim +
                                    row.ColorCast.ToString() + delim +
                                    row.ColorAngle.ToString() + delim +
                                    row.Distortion.ToString() + delim +
                                    row.Interpolation.ToString() + delim +
                                    row.Scaling.ToString() + delim +
                                    row.Rotation.ToString() + delim +
                                    row.AvgTrainLoss.ToString() + delim +
                                    row.TrainErrors.ToString() + delim +
                                    row.TrainErrorPercentage.ToString() + delim +
                                    row.TrainAccuracy.ToString() + delim +
                                    row.AvgTestLoss.ToString() + delim +
                                    row.TestErrors.ToString() + delim +
                                    row.TestErrorPercentage.ToString() + delim +
                                    row.TestAccuracy.ToString() + delim +
                                    row.ElapsedTicks.ToString() + delim +
                                    row.ElapsedTime.ToString());

                            //Clipboard.SetText(sb.ToString());
                            string tmpFileName = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".csv";
                            File.WriteAllText(tmpFileName, sb.ToString());
                            PageVM.Model.LoadLog(tmpFileName);
                            File.Delete(tmpFileName);
                        }
                        catch (Exception ex)
                        {
                            Mouse.OverrideCursor = null;
                            Xceed.Wpf.Toolkit.MessageBox.Show(ex.ToString(), "Exception occured", MessageBoxButton.OK);
                        }

                        var dataset = PageVM.Model.Dataset.ToString().ToLower();
                        var optimizer = PageVM.Model.Optimizer.ToString().ToLower();

                        var fileNamePersistOptimizer = Path.Combine(StateDirectory, Settings.Default.ModelNameActive + "-(" + dataset + ")(" + optimizer + @").bin");
                        var fileNameNoOptimizer = Path.Combine(StateDirectory, Settings.Default.ModelNameActive + "-(" + dataset + ").bin");

                        var fileNameOptimizer = Settings.Default.PersistOptimizer ? fileNamePersistOptimizer : fileNameNoOptimizer;
                        var fileNameOptimizerInverse = Settings.Default.PersistOptimizer ? fileNameNoOptimizer : fileNamePersistOptimizer;

                        if (PageVM.Model.LoadWeights(fileNameOptimizer, Settings.Default.PersistOptimizer) != 0)
                            if (PageVM.Model.LoadWeights(fileNameOptimizerInverse, !Settings.Default.PersistOptimizer) == 0)
                            {
                                Settings.Default.PersistOptimizer = !Settings.Default.PersistOptimizer;
                                Settings.Default.Save();
                                PageVM.Model.SetPersistOptimizer(Settings.Default.PersistOptimizer);
                            }

                        //for (int i = 0; i < Settings.Default.TrainingLog.Count; i++)
                        //    Settings.Default.TrainingLog[i].ElapsedTime = TimeSpan.FromMilliseconds((double)Settings.Default.TrainingLog[i].ElapsedTicks);
                        //Settings.Default.Save();

                        Title = PageVM.Model.Name + " - Convnet Explorer";
                        DataContext = PageVM;

                        switch ((int)Math.Round(Settings.Default.PrioritySetter))
                        {
                            case 1:
                                PrioritySlider.ToolTip = "Low";
                                break;
                            case 2:
                                PrioritySlider.ToolTip = "Below Normal";
                                break;
                            case 3:
                                PrioritySlider.ToolTip = "Normal";
                                break;
                            case 4:
                                PrioritySlider.ToolTip = "Above Normal";
                                break;
                            case 5:
                                PrioritySlider.ToolTip = "High";
                                break;
                            case 6:
                                PrioritySlider.ToolTip = "Realtime";
                                break;
                        }
                    }
                    else
                        Xceed.Wpf.Toolkit.MessageBox.Show("Failed to create the PageViewModel: " + Settings.Default.ModelNameActive, "Error", MessageBoxButton.OK);
                }
            }
            catch (Exception exception)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message + "\r\n\r\n" + exception.GetBaseException().Message + "\r\n\r\n" + exception.InnerException.Message + "\r\n\r\nAn error occured while loading the Model:" + Settings.Default.ModelNameActive, "Information", MessageBoxButton.OK);
            }
        }

        ~MainWindow()
        {
            // In case the client forgets to call
            // Dispose , destructor will be invoked for
            Dispose(false);
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

        private void CutCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            (e.Source as TextBox).Cut();
        }

        private void CutCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Source.GetType() == typeof(TextBox))
                e.CanExecute = (e.Source as TextBox).SelectionLength > 0;
            else
                e.CanExecute = false;
        }

        private void CopyCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            (e.Source as TextBox).Copy();
        }

        private void CopyCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Source.GetType() == typeof(TextBox))
                e.CanExecute = (e.Source as TextBox).SelectionLength > 0;
            else
                e.CanExecute = false;
        }

        private void PasteCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            (e.Source as TextBox).Paste();
        }

        private void PasteCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        private void SelectAllCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            (e.Source as TextBox).SelectAll();
        }

        private void SelectAllCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Source.GetType() == typeof(TextBox))
                e.CanExecute = true;
            else
                e.CanExecute = false;
        }

        private void UndoCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            e.Handled = (e.Source as TextBox).Undo();
        }

        private void UndoCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Source.GetType() == typeof(TextBox))
                e.CanExecute = (e.Source as TextBox).CanUndo;
            else
                e.CanExecute = false;
        }

        private void RedoCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            e.Handled = (e.Source as TextBox).Redo();
        }

        private void RedoCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Source.GetType() == typeof(TextBox))
                e.CanExecute = (e.Source as TextBox).CanRedo;
            else
                e.CanExecute = false;
        }

        private void HelpCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            ApplicationHelper.OpenBrowser("https://github.com/zamir1002/convnet.git");
        }

        private void HelpCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void AboutCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            Window aboutDialog = new About
            {
                Owner = this,
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner
            };
            aboutDialog.ShowDialog();
        }

        private void AboutCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CloseCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        private void CloseCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void OpenCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = false,
                AddExtension = true,
                ValidateNames = true,
                FilterIndex = 1,
                InitialDirectory = DefinitionsDirectory
            };

            bool? ret;

            switch (PageVM.CurrentModel)
            {
                case ViewModels.Edit:
                    openFileDialog.Filter = "Definition|*.txt";
                    openFileDialog.Title = "Load Definition";
                    openFileDialog.DefaultExt = ".txt";
                    openFileDialog.FilterIndex = 1;
                    openFileDialog.InitialDirectory = DefinitionsDirectory;
                    ret = openFileDialog.ShowDialog(Application.Current.MainWindow);
                    if (ret.HasValue && ret.Value)
                    {
                        Mouse.OverrideCursor = Cursors.Wait;
                        StreamReader reader = new StreamReader(openFileDialog.FileName, true);
                        if (openFileDialog.FileName.Contains(".txt"))
                        {
                            string definition = reader.ReadToEnd().Trim();
                            (PageVM.CurrentPage as EditPageViewModel).Definition = definition;
                            Settings.Default.DefinitionEditing = definition.Trim();
                            Settings.Default.Save();
                        }
                        reader.Close();
                        reader.Dispose();

                        Mouse.OverrideCursor = null;
                        Xceed.Wpf.Toolkit.MessageBox.Show(openFileDialog.SafeFileName + " is loaded", "Information", MessageBoxButton.OK);
                    }
                    break;

                case ViewModels.Test:
                    {
                        openFileDialog.Filter = "Weights|*.bin";
                        openFileDialog.Title = "Load Weights";
                        openFileDialog.DefaultExt = ".bin";
                        openFileDialog.FilterIndex = 1;
                        openFileDialog.InitialDirectory = DefinitionsDirectory + PageVM.Model.Name + @"\";
                        ret = openFileDialog.ShowDialog(Application.Current.MainWindow);
                        if (ret.HasValue && ret.Value)
                        {
                            Mouse.OverrideCursor = Cursors.Wait;
                            if (openFileDialog.FileName.EndsWith(".bin"))
                            {
                                if (PageVM.Model.LoadWeights(openFileDialog.FileName, Settings.Default.PersistOptimizer) == 0)
                                {
                                    var tpvm = PageVM.Pages[(int)ViewModels.Train] as TrainPageViewModel;
                                    tpvm.Optimizer = tpvm.Model.Optimizer;
                                    tpvm.RefreshButtonClick(this, null);

                                    Mouse.OverrideCursor = null;
                                    Xceed.Wpf.Toolkit.MessageBox.Show(openFileDialog.SafeFileName + " is loaded", "Information", MessageBoxButton.OK);
                                }
                                else
                                {
                                    Mouse.OverrideCursor = null;
                                    Xceed.Wpf.Toolkit.MessageBox.Show(openFileDialog.SafeFileName + " is incompatible", "Information", MessageBoxButton.OK);
                                }
                            }
                            Mouse.OverrideCursor = null;
                        }
                    }
                    break;

                case ViewModels.Train:
                    if (PageVM.CurrentPage.Model.TaskState == DNNTaskStates.Stopped)
                    {
                        openFileDialog.Filter = "Weights|*.bin|Log|*.csv";
                        openFileDialog.Title = "Load Weights";
                        openFileDialog.DefaultExt = ".bin";
                    }
                    else
                    {
                        openFileDialog.Filter = "Log|*.csv";
                        openFileDialog.Title = "Load Log";
                        openFileDialog.DefaultExt = ".cvs";
                    }

                    openFileDialog.FilterIndex = 1;
                    openFileDialog.InitialDirectory = DefinitionsDirectory + PageVM.Model.Name + @"\";
                    ret = openFileDialog.ShowDialog(Application.Current.MainWindow);
                    if (ret.HasValue && ret.Value)
                    {
                        Mouse.OverrideCursor = Cursors.Wait;
                        if (openFileDialog.FileName.EndsWith(".bin"))
                        {
                            if (PageVM.Model.LoadWeights(openFileDialog.FileName, Settings.Default.PersistOptimizer) == 0)
                            {
                                var tpvm = PageVM.Pages[(int)ViewModels.Train] as TrainPageViewModel;
                                tpvm.Optimizer = tpvm.Model.Optimizer;
                                tpvm.RefreshButtonClick(this, null);
                                                                
                                Mouse.OverrideCursor = null;
                                Xceed.Wpf.Toolkit.MessageBox.Show(openFileDialog.SafeFileName + " is loaded", "Information", MessageBoxButton.OK);
                            }
                            else
                            {
                                Mouse.OverrideCursor = null;
                                Xceed.Wpf.Toolkit.MessageBox.Show(openFileDialog.SafeFileName + " is incompatible", "Information", MessageBoxButton.OK);
                            }
                        }
                        else if (openFileDialog.FileName.EndsWith(".csv"))
                        {
                            if (PageVM.CurrentPage is TrainPageViewModel tpvm)
                            {
                                ObservableCollection<DNNTrainingResult> backup = new ObservableCollection<DNNTrainingResult>(Settings.Default.TrainingLog);
                               
                                try
                                {
                                    CsvHelper.Configuration.CsvConfiguration config = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.CurrentCulture)
                                    {
                                        HasHeaderRecord = true,
                                        DetectDelimiter = true,
                                        DetectDelimiterValues = new string[] { ";" },
                                        Delimiter = ";"
                                    };

                                    using (var reader = new StreamReader(openFileDialog.FileName, true))
                                    using (var csv = new CsvReader(reader, config))
                                    {
                                        var records = csv.GetRecords<DNNTrainingResult>();

                                        if (Settings.Default.TrainingLog.Count > 0)
                                        {
                                            Mouse.OverrideCursor = null;
                                            if (Xceed.Wpf.Toolkit.MessageBox.Show("Do you want to clear the existing log?", "Clear Log", MessageBoxButton.YesNo, MessageBoxImage.None, MessageBoxResult.No) == MessageBoxResult.Yes)
                                            {
                                                Settings.Default.TrainingLog.Clear();
                                                PageVM.Model.ClearLog();
                                            }
                                        }

                                        foreach (var record in records)
                                            Settings.Default.TrainingLog.Add(record);

                                        PageVM.Model.LoadLog(openFileDialog.FileName);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Settings.Default.TrainingLog = backup;
                                    Mouse.OverrideCursor = null;
                                    Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message);
                                }
                               
                                Settings.Default.Save();

                                tpvm.RefreshTrainingPlot();

                                Mouse.OverrideCursor = null;
                                Xceed.Wpf.Toolkit.MessageBox.Show(openFileDialog.SafeFileName + " is loaded", "Information", MessageBoxButton.OK);
                            }
                        }
                        Mouse.OverrideCursor = null;
                    }
                    break;
            }
        }

        private void SaveCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            var path = DefinitionsDirectory + PageVM.Model.Name + @"\";
            var fileName = PageVM.Model.Name + @"-(" + PageVM.Model.Dataset.ToString().ToLower() + @")" + (Settings.Default.PersistOptimizer ? @"(" + PageVM.Model.Optimizer.ToString().ToLower() + @").bin" : @".bin");
            
            var result = MessageBoxResult.Yes;
            if (File.Exists(path + fileName))
                result = Xceed.Wpf.Toolkit.MessageBox.Show("Do you want to overwrite the existing file?", "File already exists", MessageBoxButton.YesNo, MessageBoxImage.None, MessageBoxResult.No);

            if (result == MessageBoxResult.Yes)
            {
                Mouse.OverrideCursor = Cursors.Wait;
                if (PageVM.Model.SaveWeights(StateDirectory + fileName, Settings.Default.PersistOptimizer) == 0)
                {
                    File.Copy(StateDirectory + fileName, path + @"(" + PageVM.Model.Dataset.ToString().ToLower() + @")" + (Settings.Default.PersistOptimizer ? @"(" + PageVM.Model.Optimizer.ToString().ToLower() + @").bin" : @".bin"), true);
                    Mouse.OverrideCursor = null;
                    Xceed.Wpf.Toolkit.MessageBox.Show("Weights are saved", "Information", MessageBoxButton.OK);
                }
                Mouse.OverrideCursor = null;
            }
        }

        private void SaveAsCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = PageVM.Model.Name,
                AddExtension = true,
                CreatePrompt = false,
                OverwritePrompt = true,
                ValidateNames = true,
                Title = "Save Model",
                InitialDirectory = DefinitionsDirectory
            };

            bool? ret;

            switch (PageVM.CurrentModel)
            {
                case ViewModels.Edit:
                    {
                        saveFileDialog.FileName = (PageVM.CurrentPage as EditPageViewModel).ModelName;
                        saveFileDialog.Filter = "Definition|*.txt";
                        saveFileDialog.Title = "Save Model";
                        saveFileDialog.DefaultExt = ".txt";
                        saveFileDialog.FilterIndex = 1;
                        saveFileDialog.InitialDirectory = DefinitionsDirectory;
                        ret = saveFileDialog.ShowDialog(Application.Current.MainWindow);
                        var ok = ret.HasValue && ret.Value;
                        var fileName = saveFileDialog.FileName;
                        var saveFileName = saveFileDialog.SafeFileName;
                        
                        if (ok)
                        {
                            Mouse.OverrideCursor = Cursors.Wait;
                            TextWriter writer = new StreamWriter(fileName, false);
                            if (saveFileDialog.FileName.Contains(".txt"))
                                writer.Write(Settings.Default.DefinitionEditing);
                            writer.Flush();
                            writer.Close();
                            writer.Dispose();
                            Mouse.OverrideCursor = null;
                            Xceed.Wpf.Toolkit.MessageBox.Show(saveFileName + " saved", "Information", MessageBoxButton.OK);
                        }
                    }
                    break;

                case ViewModels.Train:
                    {
                        var tpvm = PageVM.CurrentPage as TrainPageViewModel;

                        saveFileDialog.FileName = @"(" + PageVM.Model.Dataset.ToString().ToLower() + @")" + (Settings.Default.PersistOptimizer ? @"(" + PageVM.Model.Optimizer.ToString().ToLower() + @")" : "");
                        saveFileDialog.Filter = "Weights|*.bin|Log|*.csv";
                        saveFileDialog.Title = "Save";
                        saveFileDialog.DefaultExt = ".bin";
                        saveFileDialog.FilterIndex = 1;
                        saveFileDialog.InitialDirectory = DefinitionsDirectory + PageVM.Model.Name + @"\";
                        ret = saveFileDialog.ShowDialog(Application.Current.MainWindow);
                                                
                        if (ret.HasValue && ret.Value)
                        {
                            if (saveFileDialog.FileName.EndsWith(".csv"))
                            {
                                try
                                {
                                    const string delim = ";";
                                    var sb = new StringBuilder();
                                    sb.AppendLine(
                                            "Cycle" + delim +
                                            "Epoch" + delim +
                                            "GroupIndex" + delim +
                                            "CostIndex" + delim +
                                            "CostName" + delim +
                                            "Optimizer" + delim +
                                            "Momentum" + delim +
                                            "Beta2" + delim +
                                            "Gamma" + delim +
                                            "L2Penalty" + delim +
                                            "Dropout" + delim +
                                            "Eps" + delim +
                                            "Rate" + delim +
                                            "N" + delim +
                                            "D" + delim +
                                            "H" + delim +
                                            "W" + delim +
                                            "PadD" + delim +
                                            "PadH" + delim +
                                            "PadW" + delim +
                                            "InputDropout" + delim +
                                            "Cutout" + delim +
                                            "CutMix" + delim +
                                            "AutoAugment" + delim +
                                            "HorizontalFlip" + delim +
                                            "VerticalFlip" + delim +
                                            "ColorCast" + delim +
                                            "ColorAngle" + delim +
                                            "Distortion" + delim +
                                            "Interpolation" + delim +
                                            "Scaling" + delim +
                                            "Rotation" + delim +
                                            "AvgTrainLoss" + delim +
                                            "TrainErrors" + delim +
                                            "TrainErrorPercentage" + delim +
                                            "TrainAccuracy" + delim +
                                            "AvgTestLoss" + delim +
                                            "TestErrors" + delim +
                                            "TestErrorPercentage" + delim +
                                            "TestAccuracy" + delim +
                                            "ElapsedTicks" + delim +
                                            "ElapsedTime");

                                    foreach (var row in tpvm.TrainingLog)
                                        sb.AppendLine(
                                            row.Cycle.ToString() + delim +
                                            row.Epoch.ToString() + delim +
                                            row.GroupIndex.ToString() + delim +
                                            row.CostIndex.ToString() + delim +
                                            row.CostName.ToString() + delim +
                                            row.Optimizer.ToString() + delim +
                                            row.Momentum.ToString() + delim +
                                            row.Beta2.ToString() + delim +
                                            row.Gamma.ToString() + delim +
                                            row.L2Penalty.ToString() + delim +
                                            row.Dropout.ToString() + delim +
                                            row.Eps.ToString() + delim +
                                            row.Rate.ToString() + delim +
                                            row.N.ToString() + delim +
                                            row.D.ToString() + delim +
                                            row.H.ToString() + delim +
                                            row.W.ToString() + delim +
                                            row.PadD.ToString() + delim +
                                            row.PadH.ToString() + delim +
                                            row.PadW.ToString() + delim +
                                            row.InputDropout.ToString() + delim +
                                            row.Cutout.ToString() + delim +
                                            row.CutMix.ToString() + delim +
                                            row.AutoAugment.ToString() + delim +
                                            row.HorizontalFlip.ToString() + delim +
                                            row.VerticalFlip.ToString() + delim +
                                            row.ColorCast.ToString() + delim +
                                            row.ColorAngle.ToString() + delim +
                                            row.Distortion.ToString() + delim +
                                            row.Interpolation.ToString() + delim +
                                            row.Scaling.ToString() + delim +
                                            row.Rotation.ToString() + delim +
                                            row.AvgTrainLoss.ToString() + delim +
                                            row.TrainErrors.ToString() + delim +
                                            row.TrainErrorPercentage.ToString() + delim +
                                            row.TrainAccuracy.ToString() + delim +
                                            row.AvgTestLoss.ToString() + delim +
                                            row.TestErrors.ToString() + delim +
                                            row.TestErrorPercentage.ToString() + delim +
                                            row.TestAccuracy.ToString() + delim +
                                            row.ElapsedTicks.ToString() + delim +
                                            row.ElapsedTime.ToString());

                                    //Clipboard.SetText(sb.ToString());

                                    File.WriteAllText(saveFileDialog.FileName, sb.ToString());
                                    PageVM.Model.LoadLog(saveFileDialog.FileName);
                                }
                                catch (Exception ex)
                                {
                                    Mouse.OverrideCursor = null;
                                    Xceed.Wpf.Toolkit.MessageBox.Show(ex.ToString(), "Exception occured", MessageBoxButton.OK);
                                }

                                Mouse.OverrideCursor = null;
                                Xceed.Wpf.Toolkit.MessageBox.Show(saveFileDialog.SafeFileName + " log saved", "Information", MessageBoxButton.OK);
                            }
                            else
                            {
                                if (saveFileDialog.FileName.EndsWith(".bin"))
                                {
                                    PageVM.Model.SaveWeights(saveFileDialog.FileName, Settings.Default.PersistOptimizer);
                                    Mouse.OverrideCursor = null;
                                    Xceed.Wpf.Toolkit.MessageBox.Show(saveFileDialog.SafeFileName + " weights saved", "Information", MessageBoxButton.OK);
                                }
                            }
                            Mouse.OverrideCursor = null;
                        }
                    }
                    break;

                case ViewModels.Test:
                    break;
            }
        }

        private void OpenCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (PageVM != null)
            {
                switch (PageVM.CurrentModel)
                {
                    case ViewModels.Edit:
                        e.CanExecute = true;
                        break;

                    case ViewModels.Test:
                        e.CanExecute = (PageVM.CurrentPage.Model.TaskState == DNNTaskStates.Stopped);
                        break;

                    case ViewModels.Train:
                        e.CanExecute = true;
                        break;
                }
            }
            else
                e.CanExecute = false;
        }

        private void SaveCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (PageVM != null)
                e.CanExecute = PageVM.CurrentModel != ViewModels.Test && PageVM.CurrentModel != ViewModels.Edit;
            else
                e.CanExecute = false;
        }

        private void SaveAsCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (PageVM != null)
                e.CanExecute = PageVM.CurrentModel != ViewModels.Test;
            else
                e.CanExecute = false;
        }

        private void LockAllCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            if (PageVM != null && PageVM.Model != null)
            {
                if (PageVM.CurrentModel == ViewModels.Train)
                {
                    if (PageVM.CurrentPage is TrainPageViewModel tpvm)
                    {
                        tpvm.Model.SetLocked(true);
                        Application.Current.Dispatcher.Invoke(() => tpvm.LayersComboBox_SelectionChanged(this, null), DispatcherPriority.Render);

                    }
                }
            }
        }

        private void LockAllCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
            if (PageVM != null && PageVM.Model != null)
                if (PageVM.CurrentModel == ViewModels.Train)
                {
                    if (PageVM.CurrentPage is TrainPageViewModel tpvm)
                        e.CanExecute = !Settings.Default.DisableLocking;
                }
        }

        private void UnlockAllCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            if (PageVM != null && PageVM.Model != null)
            {
                if (PageVM.CurrentModel == ViewModels.Train)
                {
                    if (PageVM.CurrentPage is TrainPageViewModel tpvm)
                    {
                        tpvm.Model.SetLocked(false);
                        Application.Current.Dispatcher.Invoke(() => tpvm.LayersComboBox_SelectionChanged(this, null), DispatcherPriority.Render);
                    }
                }
            }
        }

        private void UnlockAllCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
            if (PageVM != null && PageVM.Model != null)
                if (PageVM.CurrentModel == ViewModels.Train)
                {
                    if (PageVM.CurrentPage is TrainPageViewModel tpvm)
                        e.CanExecute = !Settings.Default.DisableLocking;
                }
        }

        private void PersistOptimizerCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            if (PersistOptimizer != null && PageVM != null && PageVM.Model != null)
            {
                Settings.Default.PersistOptimizer = PersistOptimizer.IsChecked;
                Settings.Default.Save();

                PageVM.Model.SetPersistOptimizer(Settings.Default.PersistOptimizer);
            }
        }

        private void PersistOptimizerCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;

            if (PersistOptimizer != null && PageVM != null && PageVM.Model != null)
                e.CanExecute = true;
        }

        private void DisableLockingCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            if (DisableLocking != null)
            {
                if (PageVM == null || PageVM.Model == null)
                    return;

                if (PageVM.Pages[2] is TrainPageViewModel tpvm)
                {
                    Settings.Default.DisableLocking = DisableLocking.IsChecked;
                    Settings.Default.Save();

                    tpvm.OnDisableLockingChanged(target, e);
                }
            }
        }

        private void DisableLockingCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;

            if (DisableLocking != null && PageVM != null && PageVM.Model != null)
                e.CanExecute = true;
        }

        private void PlainFormatCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            if (Plain != null && PageVM != null && PageVM.Model != null)
            {
                if (PageVM.Pages[2] is TrainPageViewModel tpvm)
                {
                    if (tpvm.Model.TaskState == DNNTaskStates.Stopped)
                    {
                        if (PageVM.Model.SetFormat(Plain.IsChecked))
                        {
                            Settings.Default.PlainFormat = Plain.IsChecked;
                            Settings.Default.Save();
                        }
                    }
                }
            }
        }

        private void PlainFormatCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;

            if (Plain != null && PageVM != null && PageVM.Model != null)
                e.CanExecute = PageVM.Model.TaskState == DNNTaskStates.Stopped;
        }

        private void ShuffleSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (PageVM != null && PageVM.Model != null)
            {
                var temp = (ulong)Math.Round(Settings.Default.Shuffle);
                if (PageVM.Model.SetShuffleCount(temp))
                    Settings.Default.Save();
            }
        }

        private void PrioritySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (PageVM != null && PageVM.Model != null)
            {
                var temp = (int)Math.Round(Settings.Default.PrioritySetter);

                switch (temp)
                {
                    case 1:
                        Settings.Default.Priority = System.Diagnostics.ProcessPriorityClass.Idle;
                        break;
                    case 2:
                        Settings.Default.Priority = System.Diagnostics.ProcessPriorityClass.BelowNormal;
                        break;
                    case 3:
                        Settings.Default.Priority = System.Diagnostics.ProcessPriorityClass.Normal;
                        break;
                    case 4:
                        Settings.Default.Priority = System.Diagnostics.ProcessPriorityClass.AboveNormal;
                        break;
                    case 5:
                        Settings.Default.Priority = System.Diagnostics.ProcessPriorityClass.High;
                        break;
                    case 6:
                        Settings.Default.Priority = System.Diagnostics.ProcessPriorityClass.RealTime;
                        break;
                }
                PrioritySlider.ToolTip = Settings.Default.Priority.ToString();
                Process.GetCurrentProcess().PriorityClass = Settings.Default.Priority;
                Settings.Default.Save();
            }
        }

        private void ResetCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            if (Xceed.Wpf.Toolkit.MessageBox.Show("Do you really want to reset the Scripts folder?", "Reset Application", MessageBoxButton.YesNo, MessageBoxImage.None, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                Directory.Delete(ScriptsDirectory, true);
                Directory.CreateDirectory(ScriptsDirectory);
                Copy(ApplicationPath.Replace(@"Convnet\bin\x64\" + Mode + @"\" + Framework + @"\", "") + @"ScriptsDialog\", ScriptsDirectory);
            }
        }

        private void ResetCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
    }
}