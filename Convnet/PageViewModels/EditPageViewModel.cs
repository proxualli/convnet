using Convnet.Common;
using Convnet.Properties;
using dnncore;
using ICSharpCode.AvalonEdit.Document;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace Convnet.PageViewModels
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
    public class EditPageViewModel : PageViewModelBase
    {
#if DEBUG
        const string Mode = "Debug";
#else
        const string Mode = "Release";
#endif

        public event EventHandler Open;
        public event EventHandler Save;

        private string modelName = Settings.Default.ModelNameActive;
        private string definition = Settings.Default.DefinitionEditing;
        private bool definitionStatus = false;
        private bool canSynchronize = false;
        private int selectionStart = 0;
        private int selectionLength = 0;
        private TextLocation textLocation = new TextLocation(1, 1);
        private string filePath;
        private bool wordWrap = false;
        private bool showLineNumbers = true;
        private string script = File.ReadAllText(ScriptsDirectory + @"ScriptsDialog\ScriptCatalog.cs");
        private string parameters = File.ReadAllText(ScriptsDirectory + @"ScriptsDialog\ScriptParameters.cs");
        private bool dirty = true;
        private static bool initAction = true;
        private DispatcherTimer clickWaitTimer;
       
        public EditPageViewModel(DNNModel model) : base(model)
        {
            initAction = true;
            clickWaitTimer = new DispatcherTimer(new TimeSpan(0, 0, 0, 0, 500), DispatcherPriority.Background, mouseWaitTimer_Tick, Dispatcher.CurrentDispatcher);
        
            AddCommandButtons();
        }

        private void AddCommandButtons()
        {
            Button openButton = new Button
            {
                Name = "ButtonOpen",
                ToolTip = "Open",
                Content = new BitmapToImage(Resources.Open),
                ClickMode = ClickMode.Release
            };
            openButton.Click += new RoutedEventHandler(OpenButtonClick);

            Button saveButton = new Button
            {
                Name = "ButtonSave",
                ToolTip = "Save",
                Content = new BitmapToImage(Resources.Save),
                ClickMode = ClickMode.Release
            };
            saveButton.Click += new RoutedEventHandler(SaveButtonClick);

            Button checkButton = new Button
            {
                Name = "ButtonCheck",
                ToolTip = "Check",
                Content = new BitmapToImage(Resources.SpellCheck),
                ClickMode = ClickMode.Release
            };
            checkButton.Click += new RoutedEventHandler(CheckButtonClick);

            var synchronizeButton = new Button
            {
                Name = "ButtonSynchronize",
                ToolTip = "Synchronize",
                Content = new BitmapToImage(Resources.Synchronize),
                ClickMode = ClickMode.Release
            };
            synchronizeButton.Click += new RoutedEventHandler(SynchronizeButtonClick);
            Binding binding = new Binding("CanSynchronize")
            {
                Converter = new Converters.BooleanToVisibilityConverter(),
                Source = this
            };
            BindingOperations.SetBinding(synchronizeButton, Button.VisibilityProperty, binding);

            var scriptsButton = new Button
            {
                Name = "ButtonScripts",
                ToolTip = "Run Script",
                Content = new BitmapToImage(Resources.Calculate),
                ClickMode = ClickMode.Release,
            };
            scriptsButton.Click += new RoutedEventHandler(ScriptsButtonClick);
            scriptsButton.MouseDoubleClick += ScriptsButtonMouseDoubleClick;

            var visualStudioButton = new Button
            {
                Name = "ButtonVisualStudio",
                ToolTip = "Open in Visual Studio",
                Content = new BitmapToImage(Resources.VisualStudio),
                ClickMode = ClickMode.Release,
            };
            visualStudioButton.Click += new RoutedEventHandler(VisualStudioButtonClick); 

            CommandToolBar.Add(openButton);
            CommandToolBar.Add(saveButton);
            CommandToolBar.Add(checkButton);
            CommandToolBar.Add(synchronizeButton);
            CommandToolBar.Add(scriptsButton);
            CommandToolBar.Add(visualStudioButton);
        }

        public override string DisplayName => "Edit";

        public override void Reset()
        {
            DefinitionStatus = false;
        }

        public string Definition
        {
            get { return definition; }
            set
            {
                if (definition == value)
                    return;

                definition = value;
                OnPropertyChanged(nameof(Definition));
                Settings.Default.DefinitionEditing = definition;
                Settings.Default.Save();

                ModelName = definition.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0].Trim().Replace("[", "").Replace("]", "").Trim();
                DefinitionStatus = false;
            }
        }

        public string FilePath
        {
            get { return filePath; }
            set
            {
                if (filePath == value)
                    return;

                filePath = value;
                OnPropertyChanged(nameof(FilePath));
            }
        }

        public bool WordWrap
        {
            get { return wordWrap; }
            set
            {
                if (wordWrap == value)
                    return;

                wordWrap = value;
                OnPropertyChanged(nameof(WordWrap));
            }
        }

        public bool ShowLineNumbers
        {
            get { return showLineNumbers; }
            set
            {
                if (showLineNumbers == value)
                    return;

                showLineNumbers = value;
                OnPropertyChanged(nameof(ShowLineNumbers));
            }
        }

        public int SelectionStart
        {
            get { return selectionStart; }
            set
            {
                selectionStart = value;
                OnPropertyChanged(nameof(SelectionStart));
            }
        }

        public int SelectionLength
        {
            get { return selectionLength; }
            set
            {
                selectionLength = value;
                OnPropertyChanged(nameof(SelectionLength));
            }
        }

        public TextLocation TextLocation
        {
            get { return textLocation; }
            set
            {
                if (textLocation == value)
                    return;

                textLocation = value;
                OnPropertyChanged(nameof(TextLocation));
            }
        }

        public bool DefinitionStatus
        {
            get { return definitionStatus; }

            set
            {
                if (value == definitionStatus)
                    return;

                definitionStatus = value;
                OnPropertyChanged(nameof(DefinitionStatus));

                string editing = Definition.ToLower();
                string active = Settings.Default.DefinitionActive.ToLower();
                CanSynchronize = definitionStatus && Model != null && Model.TaskState == DNNTaskStates.Stopped && !editing.Equals(active);
            }
        }

        public bool CanSynchronize
        {
            get { return canSynchronize; }

            set
            {
                if (value == canSynchronize)
                    return;

                canSynchronize = value;
                OnPropertyChanged(nameof(CanSynchronize));
            }
        }

        public string ModelName
        {
            get { return modelName; }
            set
            {
                if (value.Equals(modelName))
                    return;

                if (value.Trim().All(c => char.IsLetterOrDigit(c) || c == '-' || c == '(' || c == ')'))
                {
                    modelName = value.Trim();
                    OnPropertyChanged(nameof(ModelName));
                }
            }
        }

        public string Script
        {
            get { return script; }
            set
            {
                if (value == null)
                    return;

                if (value.Equals(script))
                    return;

                script = value;
                File.WriteAllText(ScriptsDirectory + @"ScriptsDialog\ScriptCatalog.cs", Script);
                dirty = true;
                OnPropertyChanged(nameof(Script));
            }
        }

        public string Parameters
        {
            get { return parameters; }
            set
            {
                if (value == null)
                    return;

                if (value.Equals(parameters))
                    return;

                parameters = value;
                File.WriteAllText(ScriptsDirectory + @"ScriptsDialog\ScriptParameters.cs", Parameters);
                dirty = true;
                OnPropertyChanged(nameof(Parameters));
            }
        }

        private void OpenButtonClick(object sender, RoutedEventArgs e)
        {
            Open?.Invoke(this, EventArgs.Empty);
        }

        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            Save?.Invoke(this, EventArgs.Empty);
        }

        public void CheckButtonClick(object sender, RoutedEventArgs e)
        {
            DefinitionStatus = CheckDefinition();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
        private void SynchronizeButtonClick(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = null;
            try
            {
                var sameDef = Definition.ToLower(CultureInfo.CurrentCulture).Equals(Settings.Default.DefinitionActive.ToLower(CultureInfo.CurrentCulture));
                var modelname = Definition.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0].Trim().Replace("[", "").Replace("]", "").Trim();
                var pathDefinition = Path.Combine(DefinitionsDirectory, modelname + ".txt");
                var pathStateDefinition = Path.Combine(StateDirectory, modelname + ".txt");
                var pathWeightsDirectory = DefinitionsDirectory + modelname + @"\";
                
                var pathWeights = Settings.Default.PersistOptimizer ? Path.Combine(pathWeightsDirectory, Dataset.ToString().ToLower(CultureInfo.CurrentCulture) + "-" + Settings.Default.Optimizer.ToString().ToLower(CultureInfo.CurrentCulture) + @".bin") : Path.Combine(pathWeightsDirectory, Dataset.ToString().ToLower(CultureInfo.CurrentCulture) + ".bin");
                
                if (!sameDef || modelname != Model.Name || modelname != ModelName)
                {
                    Mouse.OverrideCursor = Cursors.Wait;

                    MessageBoxResult overwrite = MessageBoxResult.Yes;
                    if (File.Exists(Path.Combine(DefinitionsDirectory, modelname + ".txt")))
                    {
                        Mouse.OverrideCursor = null;
                        overwrite = Xceed.Wpf.Toolkit.MessageBox.Show("File already exists! Overwrite?", "File already exists", MessageBoxButton.YesNo, MessageBoxImage.None, MessageBoxResult.No);
                    }

                    if (overwrite == MessageBoxResult.Yes)
                    {
                        Mouse.OverrideCursor = Cursors.Wait;

                        File.WriteAllText(pathDefinition, Definition);
                        File.WriteAllText(pathStateDefinition, Definition);

                        if (!Directory.Exists(pathWeightsDirectory))
                            Directory.CreateDirectory(pathWeightsDirectory);

                        MessageBoxResult keepWeights = MessageBoxResult.No;
                        if (sameDef)
                        {
                            Mouse.OverrideCursor = null;
                            keepWeights = Xceed.Wpf.Toolkit.MessageBox.Show("Keep model weights?", "Same Model", MessageBoxButton.YesNo, MessageBoxImage.None, MessageBoxResult.Yes);

                            if (keepWeights == MessageBoxResult.Yes)
                                Model.SaveWeights(pathWeights, Settings.Default.PersistOptimizer);
                        }

                        try
                        {
                            Model.Dispose();
                            Model = new DNNModel(Definition);

                            Model.BackgroundColor = Settings.Default.BackgroundColor;
                            Model.BlockSize = (UInt64)Settings.Default.PixelSize;
                            Model.TrainingStrategies = Settings.Default.TrainingStrategies;
                            Model.ClearTrainingStrategies();
                            foreach (DNNTrainingStrategy strategy in Settings.Default.TrainingStrategies)
                                Model.AddTrainingStrategy(strategy);
                            Model.SetFormat(Settings.Default.PlainFormat);
                            Model.SetOptimizer(Settings.Default.Optimizer);
                            Model.SetPersistOptimizer(Settings.Default.PersistOptimizer);
                            Model.SetUseTrainingStrategy(Settings.Default.UseTrainingStrategy);
                            Model.SetDisableLocking(Settings.Default.DisableLocking);
                            Model.SetShuffleCount((ulong)Math.Round(Settings.Default.Shuffle));

                            if (keepWeights == MessageBoxResult.Yes)
                                Model.LoadWeights(pathWeights, Settings.Default.PersistOptimizer);

                            ModelName = modelname;
                            Settings.Default.ModelNameActive = Model.Name;
                            Settings.Default.DefinitionEditing = Definition;
                            Settings.Default.DefinitionActive = Definition;
                            Settings.Default.Save();
                           
                            Application.Current.MainWindow.Title = Model.Name + " - Convnet Explorer";
                            CanSynchronize = false;

                            GC.Collect(GC.MaxGeneration);
                            GC.WaitForFullGCComplete();

                            Mouse.OverrideCursor = null;
                            Xceed.Wpf.Toolkit.MessageBox.Show("Model synchronized", "Information", MessageBoxButton.OK);
                        }
                        catch (Exception ex)
                        {
                            Mouse.OverrideCursor = null;
                            Xceed.Wpf.Toolkit.MessageBox.Show("An error occured during synchronization:\r\n" + ex.ToString(), "Synchronize Debug Information", MessageBoxButton.OK);
                        }
                    }
                }
                else
                {
                    if (modelname != Model.Name || modelname != ModelName)
                    {
                        Mouse.OverrideCursor = Cursors.Wait;

                        MessageBoxResult overwrite = MessageBoxResult.Yes;
                        if (File.Exists(Path.Combine(DefinitionsDirectory, modelname + ".txt")))
                        {
                            Mouse.OverrideCursor = null;
                            overwrite = Xceed.Wpf.Toolkit.MessageBox.Show("File already exists! Overwrite?", "File already exists", MessageBoxButton.YesNo, MessageBoxImage.None, MessageBoxResult.No);
                        }

                        if (overwrite == MessageBoxResult.Yes)
                        {
                            Mouse.OverrideCursor = Cursors.Wait;

                            File.WriteAllText(pathDefinition, Definition);
                            File.WriteAllText(pathStateDefinition, Definition);

                            if (!Directory.Exists(pathWeightsDirectory))
                                Directory.CreateDirectory(pathWeightsDirectory);

                            MessageBoxResult keepWeights = MessageBoxResult.No;

                            Mouse.OverrideCursor = null;
                            keepWeights = Xceed.Wpf.Toolkit.MessageBox.Show("Keep model weights?", "Same Network", MessageBoxButton.YesNo, MessageBoxImage.None, MessageBoxResult.Yes);

                            if (keepWeights == MessageBoxResult.Yes)
                                Model.SaveWeights(pathWeights, Settings.Default.PersistOptimizer);

                            try
                            {
                                Model.Dispose();
                                Model = new DNNModel(Definition);
                                Model.SetFormat(Settings.Default.PlainFormat);
                                Model.SetOptimizer(Settings.Default.Optimizer);
                                Model.SetPersistOptimizer(Settings.Default.PersistOptimizer);
                                Model.SetDisableLocking(Settings.Default.DisableLocking);
                                Model.SetShuffleCount((ulong)Math.Round(Settings.Default.Shuffle));
                                Settings.Default.Save();
                                Model.BlockSize = (UInt64)Settings.Default.PixelSize;

                                if (keepWeights == MessageBoxResult.Yes)
                                    Model.LoadWeights(pathWeights, Settings.Default.PersistOptimizer);

                                ModelName = modelname;
                                Settings.Default.ModelNameActive = Model.Name;
                                Settings.Default.DefinitionEditing = Definition;
                                Settings.Default.DefinitionActive = Definition;
                                Settings.Default.Save();

                                Application.Current.MainWindow.Title = Model.Name + " - Convnet Explorer";
                                CanSynchronize = false;

                                GC.Collect(GC.MaxGeneration);
                                GC.WaitForFullGCComplete();

                                Mouse.OverrideCursor = null;
                                Xceed.Wpf.Toolkit.MessageBox.Show("Model synchronized", "Information", MessageBoxButton.OK);
                            }
                            catch (Exception ex)
                            {
                                Mouse.OverrideCursor = null;
                                Xceed.Wpf.Toolkit.MessageBox.Show("An error occured during synchronization:\r\n" + ex.ToString(), "Synchronize Debug Information", MessageBoxButton.OK);
                            }
                        }
                    }
                }

                Settings.Default.Dataset = Model.Dataset.ToString().ToLower(CultureInfo.CurrentCulture);
                Settings.Default.Save();
            }
            catch (Exception ex)
            {
                Mouse.OverrideCursor = null;
                Xceed.Wpf.Toolkit.MessageBox.Show("An error occured during synchronization:\r\n" + ex.ToString(), "Synchronize Debug Information", MessageBoxButton.OK);
            }
        }

        private void mouseWaitTimer_Tick(object sender, EventArgs e)
        {
            clickWaitTimer.Stop();

            if (!initAction)
                ScriptDialog();
        }

        private void ScriptsButtonClick(object sender, RoutedEventArgs e)
        {
            initAction = false;
            clickWaitTimer.Start();
        }

        private void ScriptsButtonMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
        
        private void VisualStudioButtonClick(object sender, RoutedEventArgs e)
        {
            var vspath = @"C:\Program Files\Microsoft Visual Studio\2022\";
            var version = @"Community";
            const string common = @"\Common7\IDE\";

            if (!Directory.Exists(vspath))
                vspath = @"C:\Program Files (x86)\Microsoft Visual Studio\2019\";
            if (Directory.Exists(vspath + @"Community" + common))
                version = "Community";
            else if (Directory.Exists(vspath + @"Professional" + common))
                version = "Professional";
            else if (Directory.Exists(vspath + @"Enterprise" + common))
                version = "Enterprise";

            if (version.Length > 1)
            {
                try
                {
                    var ProcStartInfo = new ProcessStartInfo(vspath + version + common + @"devenv.exe", ScriptsDirectory + @"ScriptsDialog.sln")
                    {
                        WorkingDirectory = ScriptsDirectory + @"ScriptsDialog",
                        Verb = "runas",
                        UseShellExecute = true,
                        CreateNoWindow = true,
                        RedirectStandardError = false,
                        RedirectStandardOutput = false
                    };
                    
                    Process.Start(ProcStartInfo);
                }
                catch (Exception)
                {
                }
            }
        }

        async Task ScriptsDialogAsync()
        {
            await ProcessAsyncHelper.RunAsync(new ProcessStartInfo(ScriptPath + @"ScriptsDialog.exe"), null);

            var fileName = ScriptPath + @"script.txt";
            var fileInfo = new FileInfo(fileName);

            if (fileInfo.Exists)
            {
                #pragma warning disable CA1416 // Validate platform compatibility
                var security = new FileSecurity(fileInfo.FullName, AccessControlSections.Owner | AccessControlSections.Group | AccessControlSections.Access);
                //var authorizationRules = security.GetAccessRules(true, true, typeof(NTAccount));
                var owner = security.GetOwner(typeof(NTAccount));
                security.ModifyAccessRule(AccessControlModification.Add, new FileSystemAccessRule(owner, FileSystemRights.Modify, AccessControlType.Allow), out bool modified);
                #pragma warning restore CA1416 // Validate platform compatibility

                Definition = File.ReadAllText(fileName);
                ModelName = Definition.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)[0].Replace("[", "").Replace("]", "");

                fileInfo.Delete();
            }
        }

        private void ScriptDialog()
        {
            if (dirty)
            {
                Mouse.OverrideCursor = Cursors.Wait;
                IsValid = false;

                var processInfo = new ProcessStartInfo("dotnet", @"add package WpfMath --version 0.13.1")
                {
                    WorkingDirectory = ScriptsDirectory + @"ScriptsDialog\",
                    UseShellExecute = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    Verb = "runas"
                };

                using (var process = Process.Start(processInfo))
                {
                    process.WaitForExit();
                }

                var projectFilePath = ScriptsDirectory + @"ScriptsDialog\ScriptsDialog.csproj";

                Dictionary<string, string> GlobalProperty = new()
                {
                    { "Configuration", Mode },
                    { "Platform", "AnyCPU" },
                };

                int repeat = 0;
                FileInfo fileInfo;
                BuildResult buildResult;
                do
                {
                    ProjectCollection pc = new ProjectCollection(GlobalProperty, null, ToolsetDefinitionLocations.Default);
                    BuildParameters bp = new(pc)
                    {
                        OnlyLogCriticalEvents = true,
                        DetailedSummary = true,
                        MaxNodeCount = 1
                    };

                    var tempFilePath = Path.GetTempFileName();
                    fileInfo = new FileInfo(tempFilePath)
                    {
                        Attributes = FileAttributes.Temporary
                    };
                    bp.Loggers = new List<ILogger>() { new BasicFileLogger() { Parameters = fileInfo.FullName } };
                    bp.Loggers.FirstOrDefault().Verbosity = LoggerVerbosity.Diagnostic;

                    BuildRequestData buildRequest = new BuildRequestData(projectFilePath, GlobalProperty, null, new string[] { "Build" }, null);
                    buildResult = BuildManager.DefaultBuildManager.Build(bp, buildRequest);
                    BuildManager.DefaultBuildManager.ResetCaches();
                    BuildManager.DefaultBuildManager.ShutdownAllNodes();

                    pc.UnloadAllProjects();
                    pc.UnregisterAllLoggers();
                    pc.Dispose();
                    BuildManager.DefaultBuildManager.Dispose();

                    repeat++;
                }
                while (buildResult.OverallResult != BuildResultCode.Success && repeat <= 1);

                Mouse.OverrideCursor = null;
                IsValid = true;

                if (buildResult.OverallResult == BuildResultCode.Success)
                    dirty = false;
                else
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show(File.ReadAllText(fileInfo.FullName), "Compiler Result", MessageBoxButton.OK);
                    fileInfo.Delete();
                }
            }

            try
            {
                if (!dirty)
                {
                    File.Delete(ScriptPath + @"ScriptsDialog.deps.json");
                    var task = ScriptsDialogAsync();
                }
            }
            catch (Exception exception)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message, "Load Assembly", MessageBoxButton.OK);
            }
        }


        //private void ScriptDialog()
        //{
        //    if (dirty)
        //    {
        //        Mouse.OverrideCursor = Cursors.Wait;
        //        IsValid = false;

        //        try
        //        {
        //            var processInfo = new ProcessStartInfo("dotnet", @"build ScriptsDialog.csproj -p:Platform=AnyCPU -p:nugetinteractive=true -c Release -fl -flp:logfile=msbuild.log;verbosity=quiet")
        //            {
        //                WorkingDirectory = ScriptsDirectory + @"ScriptsDialog\",
        //                UseShellExecute = true,
        //                CreateNoWindow = true,
        //                WindowStyle = ProcessWindowStyle.Hidden,
        //                Verb = "runas"
        //            };

        //            File.Delete(ScriptsDirectory + @"ScriptsDialog\msbuild.log");

        //            using (var process = Process.Start(processInfo))
        //            {
        //                process.WaitForExit();
        //            }

        //            var log = File.ReadAllText(ScriptsDirectory + @"ScriptsDialog\msbuild.log");

        //            Mouse.OverrideCursor = null;
        //            IsValid = true;

        //            dirty = log.Length > 0;

        //            if (dirty)
        //                Xceed.Wpf.Toolkit.MessageBox.Show(log, "Build error", MessageBoxButton.OK);
        //        }
        //        catch (Exception ex)
        //        {
        //            Mouse.OverrideCursor = null;
        //            IsValid = true;

        //            Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, "Build failed", MessageBoxButton.OK);
        //        }
        //    }

        //    try
        //    {
        //        if (!dirty)
        //        {
        //            File.Delete(ScriptPath + @"ScriptsDialog.deps.json");
        //            var task = ScriptsDialogAsync();
        //        }
        //    }
        //    catch (Exception exception)
        //    {
        //        Mouse.OverrideCursor = null;
        //        IsValid = true;

        //        Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message, "Load Assembly", MessageBoxButton.OK);
        //    }
        //}

        private bool CheckDefinition()
        {
            var msg = Model.Check(Definition);

            Definition = msg.Definition;

            if (msg.Error)
            {
                TextLocation = new TextLocation((int)msg.Row - 1, (int)msg.Column);
                TextLocation = new TextLocation((int)msg.Row, (int)msg.Column);
                Xceed.Wpf.Toolkit.MessageBox.Show(msg.Message, "Check Information", MessageBoxButton.OK);
            }

            return !msg.Error;
        }
    }
}