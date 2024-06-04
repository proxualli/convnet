using Avalonia.Controls;
using ConvnetAvalonia.PageViewModels;
using ConvnetAvalonia.Properties;
using CustomMessageBox.Avalonia;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using Interop;
using System.Linq;
using Avalonia.Markup.Xaml;
using AvaloniaEdit;

namespace ConvnetAvalonia
{
    public partial class MainWindow : Window
    {
        public bool ShowCloseApplicationDialog = true;

        const string Framework = "net8.0";
#if DEBUG
        const string Mode = "Debug";
#else
        const string Mode = "Release";
#endif
        public static string ApplicationPath { get; } = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\";
        public static string StorageDirectory { get; } = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\convnet\";
        public static string StateDirectory { get; } = StorageDirectory + @"state\";
        public static string DefinitionsDirectory { get; } = StorageDirectory + @"definitions\";
        public static string ScriptsDirectory { get; } = StorageDirectory + @"scripts\";
        
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
                fi.CopyTo(System.IO.Path.Combine(target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (var diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }

        private bool PersistLog(string path)
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
                        "N" + delim +
                        "D" + delim +
                        "H" + delim +
                        "W" + delim +
                        "PadD" + delim +
                        "PadH" + delim +
                        "PadW" + delim +
                        "Optimizer" + delim +
                        "Rate" + delim +
                        "Eps" + delim +
                        "Momentum" + delim +
                        "Beta2" + delim +
                        "Gamma" + delim +
                        "L2Penalty" + delim +
                        "Dropout" + delim +
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
                        "ElapsedMilliSeconds" + delim +
                        "ElapsedTime");

                if (Settings.Default.TrainingLog != null)
                foreach (var row in Settings.Default.TrainingLog)
                    sb.AppendLine(
                        row.Cycle.ToString() + delim +
                        row.Epoch.ToString() + delim +
                        row.GroupIndex.ToString() + delim +
                        row.CostIndex.ToString() + delim +
                        row.CostName.ToString() + delim +
                        row.N.ToString() + delim +
                        row.D.ToString() + delim +
                        row.H.ToString() + delim +
                        row.W.ToString() + delim +
                        row.PadD.ToString() + delim +
                        row.PadH.ToString() + delim +
                        row.PadW.ToString() + delim +
                        row.Optimizer.ToString() + delim +
                        row.Rate.ToString() + delim +
                        row.Eps.ToString() + delim +
                        row.Momentum.ToString() + delim +
                        row.Beta2.ToString() + delim +
                        row.Gamma.ToString() + delim +
                        row.L2Penalty.ToString() + delim +
                        row.Dropout.ToString() + delim +
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
                        row.ElapsedMilliSeconds.ToString() + delim +
                        row.ElapsedTime.ToString());

                File.WriteAllText(path, sb.ToString());
                
                if (PageVM != null && PageVM.Model != null)
                    PageVM.Model.LoadLog(path);

                return true;
            }
            catch (Exception ex)
            {
                //Mouse.OverrideCursor = null;
                MessageBox.Show(ex.ToString(), "Exception occured", MessageBoxButtons.OK);
            }

            return false;
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
                Copy(ApplicationPath.Replace(@"Convnet\bin\x64\" + Mode + @"\" + Framework + @"\", "") + @"Scripts\", ScriptsDirectory);
            }
                     
            var fileName = System.IO.Path.Combine(StateDirectory, Settings.Default.ModelNameActive + ".txt");
            var backupModelName = "resnet-3-2-6-channelzeropad-relu";

            if (!File.Exists(System.IO.Path.Combine(StateDirectory, backupModelName + ".txt")))
                File.Copy(ApplicationPath + @"Resources\state\" + backupModelName + ".txt", StateDirectory + backupModelName + ".txt", true);

            if (!File.Exists(fileName) || !File.ReadLines(System.IO.Path.Combine(StateDirectory, backupModelName + ".txt")).SequenceEqual(File.ReadLines(ApplicationPath + @"Resources\state\" + backupModelName + ".txt")))
            {
                Directory.CreateDirectory(DefinitionsDirectory + backupModelName + @"\");
                File.Copy(ApplicationPath + @"Resources\state\" + backupModelName + ".txt", DefinitionsDirectory + backupModelName + ".txt", true);

                fileName = System.IO.Path.Combine(StateDirectory, backupModelName + ".txt");
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

                    if (PageVM != null && PageVM.Model != null)
                    {
                        model.BackgroundColor = Settings.Default.BackgroundColor;
                        model.BlockSize = (ulong)Settings.Default.PixelSize;
                        model.TrainingStrategies = Settings.Default.TrainingStrategies;
                        model.ClearTrainingStrategies();
                        if (Settings.Default.TrainingStrategies != null)
                            foreach (DNNTrainingStrategy strategy in Settings.Default.TrainingStrategies)
                                model.AddTrainingStrategy(strategy);
                        model.SetFormat(Settings.Default.PlainFormat);
                        model.SetOptimizer(Settings.Default.Optimizer);
                        model.SetPersistOptimizer(Settings.Default.PersistOptimizer);
                        model.SetUseTrainingStrategy(Settings.Default.UseTrainingStrategy);
                        model.SetDisableLocking(Settings.Default.DisableLocking);
                        model.SetShuffleCount((ulong)Math.Round(Settings.Default.Shuffle));

                        string path = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".csv";
                        if (PersistLog(path))
                            File.Delete(path);

                        var dataset = PageVM.Model.Dataset.ToString().ToLower();
                        var optimizer = PageVM.Model.Optimizer.ToString().ToLower();

                        var fileNamePersistOptimizer = System.IO.Path.Combine(StateDirectory, Settings.Default.ModelNameActive + "-(" + dataset + ")(" + optimizer + @").bin");
                        var fileNameNoOptimizer = System.IO.Path.Combine(StateDirectory, Settings.Default.ModelNameActive + "-(" + dataset + ").bin");

                        var fileNameOptimizer = Settings.Default.PersistOptimizer ? fileNamePersistOptimizer : fileNameNoOptimizer;
                        var fileNameOptimizerInverse = Settings.Default.PersistOptimizer ? fileNameNoOptimizer : fileNamePersistOptimizer;

                        if (PageVM.Model.LoadWeights(fileNameOptimizer, Settings.Default.PersistOptimizer) != 0)
                            if (PageVM.Model.LoadWeights(fileNameOptimizerInverse, !Settings.Default.PersistOptimizer) == 0)
                            {
                                Settings.Default.PersistOptimizer = !Settings.Default.PersistOptimizer;
                                Settings.Default.Save();
                                PageVM.Model.SetPersistOptimizer(Settings.Default.PersistOptimizer);
                            }

                        Title = PageVM.Model.Name + " - Convnet Explorer";
                        
                        DataContext = PageVM;

                        //switch ((int)Math.Round(Settings.Default.PrioritySetter))
                        //{
                        //    case 1:
                        //        PrioritySlider.ToolTip = "Low";
                        //        break;
                        //    case 2:
                        //        PrioritySlider.ToolTip = "Below Normal";
                        //        break;
                        //    case 3:
                        //        PrioritySlider.ToolTip = "Normal";
                        //        break;
                        //    case 4:
                        //        PrioritySlider.ToolTip = "Above Normal";
                        //        break;
                        //    case 5:
                        //        PrioritySlider.ToolTip = "High";
                        //        break;
                        //    case 6:
                        //        PrioritySlider.ToolTip = "Realtime";
                        //        break;
                        //}
                    }
                    else
                        MessageBox.Show("Failed to create the PageViewModel: " + Settings.Default.ModelNameActive, "Error", MessageBoxButtons.OK);
                }
                else
                {
                    // try backup model
                    File.Copy(ApplicationPath + @"Resources\state\" + backupModelName + ".txt", StateDirectory + backupModelName + ".txt", true);
                    fileName = System.IO.Path.Combine(StateDirectory, backupModelName + ".txt");
                    Settings.Default.ModelNameActive = backupModelName;
                    Settings.Default.DefinitionActive = File.ReadAllText(fileName);
                    Settings.Default.Optimizer = DNNOptimizers.NAG;
                    Settings.Default.Save();

                    model = new DNNModel(Settings.Default.DefinitionActive);

                    if (model == null)
                    {
                        MessageBox.Show("Failed to create the Model: " + Settings.Default.ModelNameActive, "Error", MessageBoxButtons.OK);
                        return;
                    }

                    PageVM = new PageViewModel(model);

                    if (PageVM != null && PageVM.Model != null)
                    {
                        model.BackgroundColor = Settings.Default.BackgroundColor;
                        model.BlockSize = (ulong)Settings.Default.PixelSize;
                        model.TrainingStrategies = Settings.Default.TrainingStrategies;
                        model.ClearTrainingStrategies();
                        if (Settings.Default.TrainingStrategies != null)
                            foreach (DNNTrainingStrategy strategy in Settings.Default.TrainingStrategies)
                                model.AddTrainingStrategy(strategy);
                        model.SetFormat(Settings.Default.PlainFormat);
                        model.SetOptimizer(Settings.Default.Optimizer);
                        model.SetPersistOptimizer(Settings.Default.PersistOptimizer);
                        model.SetUseTrainingStrategy(Settings.Default.UseTrainingStrategy);
                        model.SetDisableLocking(Settings.Default.DisableLocking);
                        model.SetShuffleCount((ulong)Math.Round(Settings.Default.Shuffle));

                        string path = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".csv";
                        if (PersistLog(path))
                            File.Delete(path);

                        var dataset = PageVM.Model.Dataset.ToString().ToLower();
                        var optimizer = PageVM.Model.Optimizer.ToString().ToLower();

                        var fileNamePersistOptimizer = System.IO.Path.Combine(StateDirectory, Settings.Default.ModelNameActive + "-(" + dataset + ")(" + optimizer + @").bin");
                        var fileNameNoOptimizer = System.IO.Path.Combine(StateDirectory, Settings.Default.ModelNameActive + "-(" + dataset + ").bin");

                        var fileNameOptimizer = Settings.Default.PersistOptimizer ? fileNamePersistOptimizer : fileNameNoOptimizer;
                        var fileNameOptimizerInverse = Settings.Default.PersistOptimizer ? fileNameNoOptimizer : fileNamePersistOptimizer;

                        if (PageVM.Model.LoadWeights(fileNameOptimizer, Settings.Default.PersistOptimizer) != 0)
                            if (PageVM.Model.LoadWeights(fileNameOptimizerInverse, !Settings.Default.PersistOptimizer) == 0)
                            {
                                Settings.Default.PersistOptimizer = !Settings.Default.PersistOptimizer;
                                Settings.Default.Save();
                                PageVM.Model.SetPersistOptimizer(Settings.Default.PersistOptimizer);
                            }

                        Title = PageVM.Model.Name + " - Convnet Explorer";
                        DataContext = PageVM;

                        //switch ((int)Math.Round(Settings.Default.PrioritySetter))
                        //{
                        //    case 1:
                        //        PrioritySlider.ToolTip = "Low";
                        //        break;
                        //    case 2:
                        //        PrioritySlider.ToolTip = "Below Normal";
                        //        break;
                        //    case 3:
                        //        PrioritySlider.ToolTip = "Normal";
                        //        break;
                        //    case 4:
                        //        PrioritySlider.ToolTip = "Above Normal";
                        //        break;
                        //    case 5:
                        //        PrioritySlider.ToolTip = "High";
                        //        break;
                        //    case 6:
                        //        PrioritySlider.ToolTip = "Realtime";
                        //        break;
                        //}
                    }
                    else
                        MessageBox.Show("Failed to create the PageViewModel: " + Settings.Default.ModelNameActive, "Error", MessageBoxButtons.OK);
                }
            }
            catch (Exception exception)
            {
               MessageBox.Show(exception.Message + "\r\n\r\n" + exception.GetBaseException().Message + "\r\n\r\n" + exception.InnerException.Message + "\r\n\r\nAn error occured while loading the Model:" + Settings.Default.ModelNameActive, "Information", MessageBoxButtons.OK);
            }
        }

        //public void CutCmdExecuted(object? target, ExecutedRoutedEventArgs e)
        //{
        //    ((TextBox?)e.Source)?.Cut();
        //}

        //public void CutCmdCanExecute(object? sender, CanExecuteRoutedEventArgs e)
        //{
        //    if (e != null && e.Source != null)
        //    {
        //        if (e.Source.GetType() == typeof(TextBox))
        //            e.CanExecute = ((TextBox)e.Source).SelectionEnd > 0;
        //        else
        //            e.CanExecute = false;
        //    }
        //    else
        //        e.CanExecute = false;
        //}

        //private void CopyCmdExecuted(object target, ExecutedRoutedEventArgs e)
        //{
        //    (e.Source as TextBox).Copy();
        //}

        //private void CopyCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        //{
        //    if (e.Source.GetType() == typeof(TextBox))
        //        e.CanExecute = (e.Source as TextBox).SelectionLength > 0;
        //    else
        //        e.CanExecute = false;
        //}

        //private void PasteCmdExecuted(object target, ExecutedRoutedEventArgs e)
        //{
        //    (e.Source as TextBox).Paste();
        //}

        //private void PasteCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        //{
        //    e.CanExecute = false;
        //}

        //private void SelectAllCmdExecuted(object target, ExecutedRoutedEventArgs e)
        //{
        //    (e.Source as TextBox).SelectAll();
        //}

        //private void SelectAllCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        //{
        //    if (e.Source.GetType() == typeof(TextBox))
        //        e.CanExecute = true;
        //    else
        //        e.CanExecute = false;
        //}

        //private void UndoCmdExecuted(object target, ExecutedRoutedEventArgs e)
        //{
        //    e.Handled = (e.Source as TextBox).Undo();
        //}

        //private void UndoCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        //{
        //    if (e.Source.GetType() == typeof(TextBox))
        //        e.CanExecute = (e.Source as TextBox).CanUndo;
        //    else
        //        e.CanExecute = false;
        //}

        //private void RedoCmdExecuted(object target, ExecutedRoutedEventArgs e)
        //{
        //    e.Handled = (e.Source as TextBox).Redo();
        //}

        //private void RedoCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        //{
        //    if (e.Source.GetType() == typeof(TextBox))
        //        e.CanExecute = (e.Source as TextBox).CanRedo;
        //    else
        //        e.CanExecute = false;
        //}

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
    }
}