using AsyncImageLoader;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using AvaloniaEdit.Document;
using Convnet.Common;
using ConvnetAvalonia.Properties;
using CustomMessageBox.Avalonia;
using Interop;
using ReactiveUI;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;


namespace ConvnetAvalonia.PageViewModels
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
    public class EditPageViewModel : ConvnetAvalonia.ViewModels.ViewModelBase
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
        private TextLocation textLocation = new(1, 1);
        private string filePath;
        private bool wordWrap = false;
        private bool showLineNumbers = true;
        private string script = File.ReadAllText(ScriptsDirectory + @"Scripts\Program.cs");
        private bool dirty = true;
        private static bool initAction = true;
        private readonly DispatcherTimer clickWaitTimer;
       
        public EditPageViewModel(DNNModel model) : base(model)
        {
            initAction = true;
            clickWaitTimer = new DispatcherTimer(new TimeSpan(0, 0, 0, 0, 500), DispatcherPriority.Background, MouseWaitTimer_Tick);
        
            AddCommandButtons();
        }

        private void AddCommandButtons()
        {
            var openButton = new Button
            {
                Name = "ButtonOpen",
                Content = new AdvancedImage(new Uri("../Resources/Open.png")),
                ClickMode = ClickMode.Release
            };
            ToolTip.SetTip(openButton, "Open");
            openButton.Click += OpenButtonClick;
            
            var saveButton = new Button
            {
                Name = "ButtonSave",
                Content = new AdvancedImage(new Uri("../Resources/Save.png")),
                ClickMode = ClickMode.Release
            };
            ToolTip.SetTip(saveButton, "Save");
            saveButton.Click += SaveButtonClick;

            var checkButton = new Button
            {
                Name = "ButtonCheck",
                Content = new AdvancedImage(new Uri("../Resources/SpellCheck.png", UriKind.Relative)),
                ClickMode = ClickMode.Release
            };
            ToolTip.SetTip(checkButton, "Check");
            checkButton.Click += CheckButtonClick;

            var synchronizeButton = new Button
            {
                Name = "ButtonSynchronize",
                Content = new AdvancedImage(new Uri("../Resources/Synchronize.png")),
                ClickMode = ClickMode.Release
            };
            ToolTip.SetTip(synchronizeButton, "Synchronize");
            synchronizeButton.Click += SynchronizeButtonClick;
            //var binding = new Binding("CanSynchronize")
            //{
            //    Converter = new Converters.BooleanToVisibilityConverter(),
            //    Source = this
            //};
            //BindingOperations.Apply(synchronizeButton, Button.IsVisibleProperty, binding);

            var scriptsButton = new Button
            {
                Name = "ButtonScripts",
                Content = new AdvancedImage(new Uri("../Resources/Calculate.png")),
                ClickMode = ClickMode.Release,
            };
            ToolTip.SetTip(scriptsButton, "Run Script");
            scriptsButton.Click += ScriptsButtonClick;
            //scriptsButton.MouseDoubleClick += ScriptsButtonMouseDoubleClick;

            var visualStudioButton = new Button
            {
                Name = "ButtonVisualStudio",
                Content = new AdvancedImage(new Uri("../Resources/VisualStudio.png")),
                ClickMode = ClickMode.Release,
            };
            ToolTip.SetTip(visualStudioButton, "Open in Visual Studio");
            visualStudioButton.Click += VisualStudioButtonClick; 

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
            get => definition; 
            set
            {
                this.RaiseAndSetIfChanged(ref definition, value);
                Settings.Default.DefinitionEditing = definition;
                Settings.Default.Save();

                ModelName = definition.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0].Trim().Replace("[", "").Replace("]", "").Trim();
                DefinitionStatus = false;
            }
        }

        public string FilePath
        {
            get => filePath;
            set => this.RaiseAndSetIfChanged(ref filePath, value);
        }

        public bool WordWrap
        {
            get => wordWrap;
            set => this.RaiseAndSetIfChanged(ref wordWrap, value);
        }

        public bool ShowLineNumbers
        {
            get => showLineNumbers;
            set => this.RaiseAndSetIfChanged(ref showLineNumbers, value);
        }

        public int SelectionStart
        {
            get => selectionStart;
            set => this.RaiseAndSetIfChanged(ref selectionStart, value);
        }

        public int SelectionLength
        {
            get => selectionLength;
            set => this.RaiseAndSetIfChanged(ref selectionLength, value);
        }

        public TextLocation TextLocation
        {
            get => textLocation;
            set => this.RaiseAndSetIfChanged(ref textLocation, value);
        }

        public bool DefinitionStatus
        {
            get => definitionStatus;
            set
            {
                this.RaiseAndSetIfChanged(ref definitionStatus, value);

                string editing = Definition.ToLower();
                string active = Settings.Default.DefinitionActive.ToLower();
                CanSynchronize = definitionStatus && Model != null && Model.TaskState == DNNTaskStates.Stopped && !editing.Equals(active);
            }
        }

        public bool CanSynchronize
        {
            get => canSynchronize;
            set => this.RaiseAndSetIfChanged(ref canSynchronize, value);
        }

        public string ModelName
        {
            get => modelName;
            set
            {
                if (value.Equals(modelName))
                    return;

                if (value.Trim().All(c => char.IsLetterOrDigit(c) || c == '-' || c == '(' || c == ')'))
                   this.RaiseAndSetIfChanged(ref modelName, value.Trim());
            }
        }

        public string Script
        {
            get => script;
            set
            {
                if (value == null)
                    return;

                if (value.Equals(script))
                    return;

                this.RaiseAndSetIfChanged(ref script, value);

                File.WriteAllText(ScriptsDirectory + @"Scripts\Program.cs", Script);
                dirty = true;
            }
        }

        private static readonly string[] separator = ["\r\n"];

        private void OpenButtonClick(object? sender, RoutedEventArgs e)
        {
            Open?.Invoke(this, EventArgs.Empty);
        }

        private void SaveButtonClick(object? sender, RoutedEventArgs e)
        {
            Save?.Invoke(this, EventArgs.Empty);
        }

        public void CheckButtonClick(object? sender, RoutedEventArgs e)
        {
            DefinitionStatus = CheckDefinition();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
        private void SynchronizeButtonClick(object? sender, RoutedEventArgs e)
        {
            //Mouse.OverrideCursor = null;
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
                    //Mouse.OverrideCursor = Cursors.Wait;

                    var result = MessageBox.Show("File already exists", "File already exists! Overwrite?", MessageBoxButtons.YesNo, MessageBoxIcon.None);

                    Task<MessageBoxResult> overwrite;
                    if (File.Exists(Path.Combine(DefinitionsDirectory, modelname + ".txt")))
                    {
                        // Mouse.OverrideCursor = null;
                        overwrite = MessageBox.Show("File already exists! Overwrite?", "File already exists", MessageBoxButtons.YesNo, MessageBoxIcon.None, MessageBoxDefaultButton.Button2);

                        if (overwrite.Result == MessageBoxResult.Yes)
                        {
                            //Mouse.OverrideCursor = Cursors.Wait;

                            File.WriteAllText(pathDefinition, Definition);
                            File.WriteAllText(pathStateDefinition, Definition);

                            if (!Directory.Exists(pathWeightsDirectory))
                                Directory.CreateDirectory(pathWeightsDirectory);

                            //Task<MessageBoxResult> keepWeights;
                            var reloadWeights = false;
                            if (sameDef)
                            {
                                //Mouse.OverrideCursor = null;
                                var keepWeights = MessageBox.Show("Keep model weights?", "Same Model", MessageBoxButtons.YesNo, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);

                                if (keepWeights.Result == MessageBoxResult.Yes)
                                {
                                    Model.SaveWeights(pathWeights, Settings.Default.PersistOptimizer);
                                    reloadWeights = true;
                                }
                            }

                            try
                            {

                                Model.Dispose();
                                Model = new DNNModel(Definition)
                                {
                                    BackgroundColor = Settings.Default.BackgroundColor,
                                    BlockSize = (UInt64)Settings.Default.PixelSize,
                                    TrainingStrategies = Settings.Default.TrainingStrategies
                                };
                                Model.ClearTrainingStrategies();
                                foreach (DNNTrainingStrategy strategy in Settings.Default.TrainingStrategies)
                                    Model.AddTrainingStrategy(strategy);
                                Model.SetFormat(Settings.Default.PlainFormat);
                                Model.SetOptimizer(Settings.Default.Optimizer);
                                Model.SetPersistOptimizer(Settings.Default.PersistOptimizer);
                                Model.SetUseTrainingStrategy(Settings.Default.UseTrainingStrategy);
                                Model.SetDisableLocking(Settings.Default.DisableLocking);
                                Model.SetShuffleCount((ulong)Math.Round(Settings.Default.Shuffle));

                                if (reloadWeights)
                                    Model.LoadWeights(pathWeights, Settings.Default.PersistOptimizer);

                                ModelName = modelname;
                                Settings.Default.ModelNameActive = Model.Name;
                                Settings.Default.DefinitionEditing = Definition;
                                Settings.Default.DefinitionActive = Definition;
                                Settings.Default.Save();

                                App.MainWindow.Title = Model.Name + " - Convnet Explorer";
                                CanSynchronize = false;

                                GC.Collect(GC.MaxGeneration);
                                GC.WaitForFullGCComplete();

                                //Mouse.OverrideCursor = null;
                                MessageBox.Show("Model synchronized", "Information", MessageBoxButtons.OK);
                            }
                            catch (Exception ex)
                            {
                                //Mouse.OverrideCursor = null;
                                MessageBox.Show("An error occured during synchronization:\r\n" + ex.ToString(), "Synchronize Debug Information", MessageBoxButtons.OK);
                            }
                        }
                    }
                }
                else
                {
                    if (modelname != Model.Name || modelname != ModelName)
                    {
                        //Mouse.OverrideCursor = Cursors.Wait;

                        var overwrite = false;
                        if (File.Exists(Path.Combine(DefinitionsDirectory, modelname + ".txt")))
                        {
                            //Mouse.OverrideCursor = null;
                            var dialog = MessageBox.Show("File already exists! Overwrite?", "File already exists", MessageBoxButtons.YesNo, MessageBoxIcon.None, MessageBoxDefaultButton.Button2);
                            overwrite = dialog.Result == MessageBoxResult.Yes;
                        }

                        if (overwrite)
                        {
                            //Mouse.OverrideCursor = Cursors.Wait;

                            File.WriteAllText(pathDefinition, Definition);
                            File.WriteAllText(pathStateDefinition, Definition);

                            if (!Directory.Exists(pathWeightsDirectory))
                                Directory.CreateDirectory(pathWeightsDirectory);

                                Task<MessageBoxResult> keepWeights;// = MessageBoxResult.No;

                            //Mouse.OverrideCursor = null;
                            keepWeights = MessageBox.Show("Keep model weights?", "Same Network", MessageBoxButtons.YesNo, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);

                            if (keepWeights.Result == MessageBoxResult.Yes)
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

                                if (keepWeights.Result == MessageBoxResult.Yes)
                                    Model.LoadWeights(pathWeights, Settings.Default.PersistOptimizer);

                                ModelName = modelname;
                                Settings.Default.ModelNameActive = Model.Name;
                                Settings.Default.DefinitionEditing = Definition;
                                Settings.Default.DefinitionActive = Definition;
                                Settings.Default.Save();

                                App.MainWindow.Title = Model.Name + " - Convnet Explorer";
                                CanSynchronize = false;

                                GC.Collect(GC.MaxGeneration);
                                GC.WaitForFullGCComplete();

                                //Mouse.OverrideCursor = null;
                                MessageBox.Show("Model synchronized", "Information", MessageBoxButtons.OK);
                            }
                            catch (Exception ex)
                            {
                                //Mouse.OverrideCursor = null;
                                MessageBox.Show("An error occured during synchronization:\r\n" + ex.ToString(), "Synchronize Debug Information", MessageBoxButtons.OK);
                            }
                        }
                    }
                }

                Settings.Default.Dataset = Model.Dataset.ToString().ToLower(CultureInfo.CurrentCulture);
                Settings.Default.Save();
            }
            catch (Exception ex)
            {
                //Mouse.OverrideCursor = null;
                MessageBox.Show("An error occured during synchronization:\r\n" + ex.ToString(), "Synchronize Debug Information", MessageBoxButtons.OK);
            }
        }

        private void MouseWaitTimer_Tick(object? sender, EventArgs e)
        {
            clickWaitTimer.Stop();

            if (!initAction)
                ScriptDialog();
        }

        private void ScriptsButtonClick(object? sender, RoutedEventArgs e)
        {
            initAction = false;
            clickWaitTimer.Start();
        }

        //private void ScriptsButtonMouseDoubleClick(object? sender, MouseButtonEventArgs e)
        //{
        //    e.Handled = true;
        //}
        
        private void VisualStudioButtonClick(object? sender, RoutedEventArgs e)
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
                    var ProcStartInfo = new ProcessStartInfo(vspath + version + common + @"devenv.exe", ScriptsDirectory + @"\Scripts\Scripts.csproj")
                    {
                        WorkingDirectory = ScriptsDirectory + @"Scripts",
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
            await ProcessAsyncHelper.RunAsync(new ProcessStartInfo(ScriptPath + @"Scripts.exe"), null);

            var fileName = ScriptPath + @"script.txt";
            var fileInfo = new FileInfo(fileName);

            if (fileInfo.Exists)
            {
                //#pragma warning disable CA1416 // Validate platform compatibility
                var security = new FileSecurity(fileInfo.FullName, AccessControlSections.Owner | AccessControlSections.Group | AccessControlSections.Access);
                //var authorizationRules = security.GetAccessRules(true, true, typeof(NTAccount));
                var owner = security.GetOwner(typeof(NTAccount));
                security.ModifyAccessRule(AccessControlModification.Add, new FileSystemAccessRule(owner, FileSystemRights.Modify, AccessControlType.Allow), out bool modified);
                //#pragma warning restore CA1416 // Validate platform compatibility

                Definition = File.ReadAllText(fileName);
                ModelName = Definition.Split(separator, StringSplitOptions.RemoveEmptyEntries)[0].Replace("[", "").Replace("]", "");

                fileInfo.Delete();
            }
        }

        //private void ScriptDialog()
        //{
        //    if (dirty)
        //    {
        //        Mouse.OverrideCursor = Cursors.Wait;
        //        IsValid = false;

        //        var processInfo = new ProcessStartInfo("dotnet")
        //        {
        //            WorkingDirectory = ScriptsDirectory + @"Scripts\",
        //            UseShellExecute = true,
        //            CreateNoWindow = true,
        //            WindowStyle = ProcessWindowStyle.Hidden,
        //            Verb = "runas"
        //        };

        //        using (var process = Process.Start(processInfo))
        //        {
        //            process.WaitForExit();
        //        }

        //        var projectFilePath = ScriptsDirectory + @"Scripts\Scripts.csproj";

        //        Dictionary<string, string> GlobalProperty = new()
        //        {
        //            { "Configuration", Mode },
        //            { "Platform", "AnyCPU" },
        //        };

        //        int repeat = 0;
        //        FileInfo fileInfo;
        //        BuildResult buildResult;
        //        do
        //        {
        //            var pc = new ProjectCollection(GlobalProperty, null, ToolsetDefinitionLocations.Default);
        //            BuildParameters bp = new(pc)
        //            {
        //                OnlyLogCriticalEvents = true,
        //                DetailedSummary = true,
        //                MaxNodeCount = 1
        //            };

        //            var tempFilePath = Path.GetTempFileName();
        //            fileInfo = new FileInfo(tempFilePath)
        //            {
        //                Attributes = FileAttributes.Temporary
        //            };
        //            bp.Loggers = [new BasicFileLogger() { Parameters = fileInfo.FullName }];
        //            bp.Loggers.FirstOrDefault().Verbosity = LoggerVerbosity.Diagnostic;

        //            var buildRequest = new BuildRequestData(projectFilePath, GlobalProperty, null, ["Build"], null);
        //            buildResult = BuildManager.DefaultBuildManager.Build(bp, buildRequest);
        //            BuildManager.DefaultBuildManager.ResetCaches();
        //            BuildManager.DefaultBuildManager.ShutdownAllNodes();

        //            pc.UnloadAllProjects();
        //            pc.UnregisterAllLoggers();
        //            pc.Dispose();
        //            BuildManager.DefaultBuildManager.Dispose();

        //            repeat++;
        //        }
        //        while (buildResult.OverallResult != BuildResultCode.Success && repeat <= 1);

        //        Mouse.OverrideCursor = null;
        //        IsValid = true;

        //        if (buildResult.OverallResult == BuildResultCode.Success)
        //            dirty = false;
        //        else
        //        {
        //            Xceed.Wpf.Toolkit.MessageBox.Show(File.ReadAllText(fileInfo.FullName), "Compiler Result", MessageBoxButton.OK);
        //            fileInfo.Delete();
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
        //        Xceed.Wpf.Toolkit.MessageBox.Show(exception.Message, "Load Assembly", MessageBoxButton.OK);
        //    }
        //}


        private void ScriptDialog()
        {
            if (dirty)
            {
                //Mouse.OverrideCursor = Cursors.Wait;
                IsValid = false;

                try
                {
                    var processInfo = new ProcessStartInfo("dotnet", @"build Scripts.csproj -p:Platform=AnyCPU -p:nugetinteractive=true -c Release -fl -flp:logfile=msbuild.log;verbosity=quiet")
                    {
                        WorkingDirectory = ScriptsDirectory + @"Scripts\",
                        UseShellExecute = true,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        Verb = "runas"
                    };

                    File.Delete(ScriptsDirectory + @"Scripts\msbuild.log");

                    using (var process = Process.Start(processInfo))
                    {
                        process.WaitForExit();
                    }

                    var log = File.ReadAllText(ScriptsDirectory + @"Scripts\msbuild.log");

                    //Mouse.OverrideCursor = null;
                    IsValid = true;

                    dirty = log.Length > 0;

                    if (dirty)
                        MessageBox.Show(log, "Build error", MessageBoxButtons.OK);
                }
                catch (Exception ex)
                {
                    //Mouse.OverrideCursor = null;
                    IsValid = true;

                    MessageBox.Show(ex.Message, "Build failed", MessageBoxButtons.OK);
                }
            }

            try
            {
                if (!dirty)
                {
                    File.Delete(ScriptPath + @"Scripts.deps.json");
                    var task = ScriptsDialogAsync();
                }
            }
            catch (Exception exception)
            {
                //Mouse.OverrideCursor = null;
                IsValid = true;

                MessageBox.Show(exception.Message, "Load Assembly", MessageBoxButtons.OK);
            }
        }

        private bool CheckDefinition()
        {
            var definition = new StringBuilder(Definition);
            var msg = Model.Check(ref definition);

            Definition = msg.Definition;

            if (msg.Error)
            {
                TextLocation = new TextLocation((int)msg.Row - 1, (int)msg.Column);
                TextLocation = new TextLocation((int)msg.Row, (int)msg.Column);
                MessageBox.Show(msg.Message, "Check Information", MessageBoxButtons.OK);
            }
            
            return !msg.Error;
        }

       
    }
}