using Convnet.Common;
using Convnet.Dialogs;
using Convnet.PageViewModels;
using Convnet.Properties;
using CsvHelper;
using dnncore;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using fp = System.Single;


namespace Convnet
{
    public partial class MainWindow : Window, IDisposable
    {
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

        public static RoutedUICommand AboutCmd = new RoutedUICommand();
        public static RoutedUICommand DisableLockingCmd = new RoutedUICommand();
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
                Copy(ApplicationPath.Replace(@"Convnet\bin\x64\" + Mode + @"\net5.0-windows\", "") + @"ScriptsDialog\", ScriptsDirectory);
            }

            if (!File.Exists(Path.Combine(StateDirectory, Settings.Default.ModelNameActive + ".definition")))
            {
                Directory.CreateDirectory(DefinitionsDirectory + @"resnet-32-4-3-2-6-dropout-channelzeropad-weights\");
                Directory.CreateDirectory(DefinitionsDirectory + Settings.Default.ModelNameActive + @"-weights\");
                File.Copy(ApplicationPath + @"Resources\state\resnet-32-4-3-2-6-dropout-channelzeropad.definition", StateDirectory + "resnet-32-4-3-2-6-dropout-channelzeropad.definition", true);
                File.Copy(ApplicationPath + @"Resources\state\resnet-32-4-3-2-6-dropout-channelzeropad.definition", DefinitionsDirectory + "resnet-32-4-3-2-6-dropout-channelzeropad.definition", true);

                Settings.Default.ModelNameActive = "resnet-32-4-3-2-6-dropout-channelzeropad";
                Settings.Default.Optimizer = (int)DNNOptimizers.NAG;
                Settings.Default.AdaDeltaEps = (fp)1e-08;
                Settings.Default.AdaGradEps = (fp)1e-08;
                Settings.Default.AdamEps = (fp)1e-08;
                Settings.Default.AdamBeta2 = (fp)0.999;
                Settings.Default.AdamaxEps = (fp)1e-08;
                Settings.Default.AdamaxBeta2 = (fp)0.999;
                Settings.Default.RMSpropEps = (fp)1e-08;
                Settings.Default.RAdamEps = (fp)1e-08;
                Settings.Default.RAdamBeta1 = (fp)0.9;
                Settings.Default.RAdamBeta2 = (fp)0.999;
                Settings.Default.Save();
            }

            try
            {
                var model = new Model(Settings.Default.ModelNameActive, Path.Combine(StateDirectory, Settings.Default.ModelNameActive + ".definition"), (DNNOptimizers)Settings.Default.Optimizer);
                if (model != null)
                {
                    PageVM = new PageViewModel(model);
                    if (PageVM != null)
                    {
                        PageVM.Model.SetOptimizer((DNNOptimizers)Settings.Default.Optimizer);
                        PageVM.Model.SetPersistOptimizer(Settings.Default.PersistOptimizer);
                        PageVM.Model.SetDisableLocking(Settings.Default.DisableLocking);
                        PageVM.Model.BlockSize = (ulong)Settings.Default.PixelSize;
                        PageVM.Model.SetOptimizersHyperParameters(Settings.Default.AdaDeltaEps, Settings.Default.AdaGradEps, Settings.Default.AdamEps, Settings.Default.AdamBeta2, Settings.Default.AdamaxEps, Settings.Default.AdamaxBeta2, Settings.Default.RMSpropEps, Settings.Default.RAdamEps, Settings.Default.RAdamBeta1, Settings.Default.RAdamBeta2);

                        if (File.Exists(Path.Combine(StateDirectory, Settings.Default.ModelNameActive + @".weights")))
                        {
                            if (PageVM.Model.LoadWeights(Path.Combine(StateDirectory, Settings.Default.ModelNameActive + @".weights"), Settings.Default.PersistOptimizer) != 0)
                            {
                                if (PageVM.Model.LoadWeights(Path.Combine(StateDirectory, Settings.Default.ModelNameActive + @".weights"), !Settings.Default.PersistOptimizer) == 0)
                                {
                                    Settings.Default.PersistOptimizer = !Settings.Default.PersistOptimizer;
                                    Settings.Default.Save();
                                    PageVM.Model.SetPersistOptimizer(Settings.Default.PersistOptimizer);
                                }
                            }
                        }

                        Settings.Default.DefinitionActive = File.ReadAllText(Path.Combine(StateDirectory, Settings.Default.ModelNameActive + ".definition"));
                        Settings.Default.Save();

                        for (int i = 0; i < Settings.Default.TrainingLog.Count; i++)
                            Settings.Default.TrainingLog[i].ElapsedTime = new TimeSpan(Settings.Default.TrainingLog[i].ElapsedTicks);
                        Settings.Default.Save();

                        Title = PageVM.Model.Name + " - Convnet Explorer";
                        DataContext = PageVM;

                        int priority = (int)Math.Round(Settings.Default.PrioritySetter);
                        switch (priority)
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
                       Xceed.Wpf.Toolkit.MessageBox.Show("Failed to create the viewmodel: " + Settings.Default.ModelNameActive, "Error", MessageBoxButton.OK);
                }
                else
                   Xceed.Wpf.Toolkit.MessageBox.Show("Failed to create the model: " + Settings.Default.ModelNameActive, "Error", MessageBoxButton.OK);
            }
            catch (Exception exception)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message + "\r\n\r\n" + exception.GetBaseException().Message + "\r\n\r\n" + exception.InnerException.Message + "\r\n\r\nAn error occured while loading the model:" + Settings.Default.ModelNameActive, "Information", MessageBoxButton.OK);
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

        void CutCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            (e.Source as TextBox).Cut();
        }

        void CutCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Source.GetType() == typeof(TextBox))
                e.CanExecute = (e.Source as TextBox).SelectionLength > 0;
            else
                e.CanExecute = false;
        }

        void CopyCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            (e.Source as TextBox).Copy();
        }

        void CopyCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Source.GetType() == typeof(TextBox))
                e.CanExecute = (e.Source as TextBox).SelectionLength > 0;
            else
                e.CanExecute = false;
        }

        void PasteCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            (e.Source as TextBox).Paste();
        }

        void PasteCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        void SelectAllCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            (e.Source as TextBox).SelectAll();
        }

        void SelectAllCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Source.GetType() == typeof(TextBox))
                e.CanExecute = true;
            else
                e.CanExecute = false;
        }

        void UndoCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            e.Handled = (e.Source as TextBox).Undo();
        }

        void UndoCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Source.GetType() == typeof(TextBox))
                e.CanExecute = (e.Source as TextBox).CanUndo;
            else
                e.CanExecute = false;
        }

        void RedoCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            e.Handled = (e.Source as TextBox).Redo();
        }

        void RedoCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Source.GetType() == typeof(TextBox))
                e.CanExecute = (e.Source as TextBox).CanRedo;
            else
                e.CanExecute = false;
        }

        void HelpCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            ApplicationHelper.OpenBrowser("https://github.com/zamir1001/convnet.git");
        }

        void HelpCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        void AboutCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            Window aboutDialog = new About
            {
                Owner = this,
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner
            };
            aboutDialog.ShowDialog();
        }

        void AboutCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        void CloseCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        void CloseCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        void OpenCmdExecuted(object target, ExecutedRoutedEventArgs e)
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
                    openFileDialog.Filter = "Definition|*.definition";
                    openFileDialog.Title = "Load Definition";
                    openFileDialog.DefaultExt = ".definition";
                    openFileDialog.FilterIndex = 1;
                    openFileDialog.InitialDirectory = DefinitionsDirectory;
                    ret = openFileDialog.ShowDialog(Application.Current.MainWindow);
                    if (ret.HasValue && ret.Value)
                    {
                        Mouse.OverrideCursor = Cursors.Wait;
                        StreamReader reader = new StreamReader(openFileDialog.FileName, true);
                        if (openFileDialog.FileName.Contains(".definition"))
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
                        openFileDialog.Filter = "Weights|*.weights";
                        openFileDialog.Title = "Load Weights";
                        openFileDialog.DefaultExt = ".weights";
                        openFileDialog.FilterIndex = 1;
                        openFileDialog.InitialDirectory = DefinitionsDirectory + PageVM.Model.Name + @"-weights\";
                        ret = openFileDialog.ShowDialog(Application.Current.MainWindow);
                        if (ret.HasValue && ret.Value)
                        {
                            Mouse.OverrideCursor = Cursors.Wait;
                            if (openFileDialog.FileName.EndsWith(".weights"))
                            {
                                if (PageVM.Model.LoadWeights(openFileDialog.FileName, Settings.Default.PersistOptimizer) == 0)
                                {
                                    (PageVM.Pages[(int)ViewModels.Train] as TrainPageViewModel).RefreshButtonClick(this, null);
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
                        openFileDialog.Filter = "Weights|*.weights|Log|*.csv";
                        openFileDialog.Title = "Load Weights";
                        openFileDialog.DefaultExt = ".weights";
                    }
                    else
                    {
                        openFileDialog.Filter = "Log|*.csv";
                        openFileDialog.Title = "Load Log";
                        openFileDialog.DefaultExt = ".cvs";
                    }

                    openFileDialog.FilterIndex = 1;
                    openFileDialog.InitialDirectory = DefinitionsDirectory + PageVM.Model.Name + @"-weights\";
                    ret = openFileDialog.ShowDialog(Application.Current.MainWindow);
                    if (ret.HasValue && ret.Value)
                    {
                        Mouse.OverrideCursor = Cursors.Wait;
                        if (openFileDialog.FileName.EndsWith(".weights"))
                        {
                            if (PageVM.Model.LoadWeights(openFileDialog.FileName, Settings.Default.PersistOptimizer) == 0)
                            {
                                (PageVM.Pages[(int)ViewModels.Train] as TrainPageViewModel).RefreshButtonClick(this, null);
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
                                CsvHelper.Configuration.CsvConfiguration config = new CsvHelper.Configuration.CsvConfiguration(System.Globalization.CultureInfo.CurrentCulture)
                                {
                                    HasHeaderRecord = true
                                };
                                                              
                                TextReader reader = new StreamReader(openFileDialog.FileName);
                                var csvReader = new CsvReader(reader, config);
                                var log = csvReader.GetRecords<DNNTrainingResult>();

                                if (Settings.Default.TrainingLog.Count > 0)
                                {
                                    Mouse.OverrideCursor = null;
                                    if (Xceed.Wpf.Toolkit.MessageBox.Show("Do you want to clear the existing log?", "Clear Log", MessageBoxButton.YesNo, MessageBoxImage.None, MessageBoxResult.No) == MessageBoxResult.Yes)
                                        Settings.Default.TrainingLog.Clear();
                                }
                                foreach (var result in log)
                                    Settings.Default.TrainingLog.Add(result);
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

        void SaveCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            if (File.Exists(DefinitionsDirectory + PageVM.Model.Name + @"-weights\" + PageVM.Model.Name + ".weights"))
            {
                if (Xceed.Wpf.Toolkit.MessageBox.Show("Do you want to overwrite the existing file?", "File already exists", MessageBoxButton.YesNo, MessageBoxImage.None, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    SaveWeights();
                }
            }
            else
            {
                Mouse.OverrideCursor = Cursors.Wait;
                SaveWeights();
            }
            Mouse.OverrideCursor = null;
        }

        private void SaveWeights()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            PageVM.Model.SaveWeights(DefinitionsDirectory + PageVM.Model.Name + @"-weights\" + PageVM.Model.Name + ".weights", Settings.Default.PersistOptimizer);
            if (File.Exists(DefinitionsDirectory + PageVM.Model.Name + @"-weights\" + PageVM.Model.Name + ".weights"))
            {
                File.Copy(DefinitionsDirectory + PageVM.Model.Name + @"-weights\" + PageVM.Model.Name + ".weights", StateDirectory + PageVM.Model.Name + ".weights", true);
                Mouse.OverrideCursor = null;
                Xceed.Wpf.Toolkit.MessageBox.Show("Weights are saved", "Information", MessageBoxButton.OK);
            }
            else
                Mouse.OverrideCursor = null;
        }

        void SaveAsCmdExecuted(object target, ExecutedRoutedEventArgs e)
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
                    saveFileDialog.FileName = (PageVM.CurrentPage as EditPageViewModel).ModelName;
                    saveFileDialog.Filter = "Definition|*.definition";
                    saveFileDialog.Title = "Save Model";
                    saveFileDialog.DefaultExt = ".definition";
                    saveFileDialog.FilterIndex = 1;
                    saveFileDialog.InitialDirectory = DefinitionsDirectory;
                    ret = saveFileDialog.ShowDialog(Application.Current.MainWindow);
                    if (ret.HasValue && ret.Value)
                    {
                        Mouse.OverrideCursor = Cursors.Wait;
                        TextWriter writer = new StreamWriter(saveFileDialog.FileName, false);
                        if (saveFileDialog.FileName.Contains(".definition"))
                            writer.Write(Settings.Default.DefinitionEditing);
                        writer.Flush();
                        writer.Close();
                        writer.Dispose();
                        Mouse.OverrideCursor = null;
                        Xceed.Wpf.Toolkit.MessageBox.Show(saveFileDialog.SafeFileName + " saved", "Information", MessageBoxButton.OK);
                    }
                    break;

                case ViewModels.Train:
                    saveFileDialog.Filter = "Weights|*.weights|Log|*.csv";
                    saveFileDialog.Title = "Save";
                    saveFileDialog.DefaultExt = ".weights";
                    saveFileDialog.FilterIndex = 1;
                    saveFileDialog.InitialDirectory = DefinitionsDirectory + PageVM.Model.Name + @"-weights\";
                    ret = saveFileDialog.ShowDialog(Application.Current.MainWindow);
                    if (ret.HasValue && ret.Value)
                    {
                        if (saveFileDialog.FileName.EndsWith(".csv"))
                        {
                            if (PageVM.CurrentPage is TrainPageViewModel tpvm)
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
                                            "BatchSize" + delim +
                                            "Rate" + delim +
                                            "Momentum" + delim +
                                            "L2Penalty" + delim +
                                            "Dropout" + delim +
                                            "Cutout" + delim +
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
                                            row.BatchSize.ToString() + delim +
                                            row.Rate.ToString() + delim +
                                            row.Momentum.ToString() + delim +
                                            row.L2Penalty.ToString() + delim +
                                            row.Dropout.ToString() + delim +
                                            row.Cutout.ToString() + delim +
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

                                    var csv = sb.ToString();
                                    //Clipboard.SetText(csv);
                                    File.WriteAllText(saveFileDialog.FileName, csv);
                                    Mouse.OverrideCursor = null;
                                    Xceed.Wpf.Toolkit.MessageBox.Show(saveFileDialog.SafeFileName + " log saved", "Information", MessageBoxButton.OK);
                                }
                                catch (Exception exception)
                                {
                                    Mouse.OverrideCursor = null;
                                    Xceed.Wpf.Toolkit.MessageBox.Show(exception.ToString(), "Exception occured", MessageBoxButton.OK);
                                }
                            }
                            Mouse.OverrideCursor = null;
                        }
                        else
                        {
                            if (saveFileDialog.FileName.EndsWith(".weights"))
                            {
                                PageVM.Model.SaveWeights(saveFileDialog.FileName, Settings.Default.PersistOptimizer);
                                Mouse.OverrideCursor = null;
                                Xceed.Wpf.Toolkit.MessageBox.Show(saveFileDialog.SafeFileName + " weights saved", "Information", MessageBoxButton.OK);
                            }
                        }
                        Mouse.OverrideCursor = null;
                    }
                    break;

                case ViewModels.Test:
                    break;
            }
        }

        void OpenCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
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

        void SaveCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (PageVM != null)
                e.CanExecute = PageVM.CurrentModel != ViewModels.Test && PageVM.CurrentModel != ViewModels.Edit;
            else
                e.CanExecute = false;
        }

        void SaveAsCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (PageVM != null)
                e.CanExecute = PageVM.CurrentModel != ViewModels.Test;
            else
                e.CanExecute = false;
        }

        void LockAllCmdExecuted(object target, ExecutedRoutedEventArgs e)
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

        void LockAllCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
            if (PageVM != null && PageVM.Model != null)
                if (PageVM.CurrentModel == ViewModels.Train)
                {
                    if (PageVM.CurrentPage is TrainPageViewModel tpvm)
                        e.CanExecute = !Settings.Default.DisableLocking;
                }
        }

        void UnlockAllCmdExecuted(object target, ExecutedRoutedEventArgs e)
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

        void UnlockAllCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
            if (PageVM != null && PageVM.Model != null)
                if (PageVM.CurrentModel == ViewModels.Train)
                {
                    if (PageVM.CurrentPage is TrainPageViewModel tpvm)
                        e.CanExecute = !Settings.Default.DisableLocking;
                }
        }

        void PersistOptimizerCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            if (PersistOptimizer != null && PageVM != null && PageVM.Model != null)
            {
                Settings.Default.PersistOptimizer = PersistOptimizer.IsChecked;
                Settings.Default.Save();

                PageVM.Model.SetPersistOptimizer(Settings.Default.PersistOptimizer);
            }
        }

        void PersistOptimizerCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;

            if (PersistOptimizer != null && PageVM != null && PageVM.Model != null)
                e.CanExecute = true;
        }

        void DisableLockingCmdExecuted(object target, ExecutedRoutedEventArgs e)
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

        void DisableLockingCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;

            if (DisableLocking != null && PageVM != null && PageVM.Model != null)
                e.CanExecute = true;
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
                        PrioritySlider.ToolTip = "Low";
                        break;
                    case 2:
                        Settings.Default.Priority = System.Diagnostics.ProcessPriorityClass.BelowNormal;
                        PrioritySlider.ToolTip = "Below Normal";
                        break;
                    case 3:
                        Settings.Default.Priority = System.Diagnostics.ProcessPriorityClass.Normal;
                        PrioritySlider.ToolTip = "Normal";
                        break;
                    case 4:
                        Settings.Default.Priority = System.Diagnostics.ProcessPriorityClass.AboveNormal;
                        PrioritySlider.ToolTip = "Above Normal";
                        break;
                    case 5:
                        Settings.Default.Priority = System.Diagnostics.ProcessPriorityClass.High;
                        PrioritySlider.ToolTip = "High";
                        break;
                    case 6:
                        Settings.Default.Priority = System.Diagnostics.ProcessPriorityClass.RealTime;
                        PrioritySlider.ToolTip = "Realtime";
                        break;
                }

                Process.GetCurrentProcess().PriorityClass = Settings.Default.Priority;
                Settings.Default.Save();
            }
        }
    }
}
