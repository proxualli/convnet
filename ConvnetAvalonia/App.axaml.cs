using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ConvnetAvalonia.Properties;
using System;
using System.Diagnostics;


namespace ConvnetAvalonia
{
    public partial class App : Application, IDisposable
    {
        public static readonly bool SingleInstanceApp = true;
        public event EventHandler<ShutdownRequestedEventArgs>? ShutdownRequested;
        public static ConvnetAvalonia.PageViews.MainWindow? MainWindow = null;
        
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.ShutdownRequested += AppShutdownRequested;
                desktop.MainWindow = new ConvnetAvalonia.PageViews.MainWindow
                {
                    
                };

                if (desktop.MainWindow != null)
                {
                    App.MainWindow = desktop.MainWindow as ConvnetAvalonia.PageViews.MainWindow;
                    if (App.MainWindow != null)
                        App.MainWindow.Closing += MainWindow_Closing;
                }
            }
            
            base.OnFrameworkInitializationCompleted();
        }

        protected virtual void AppShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
        {
            Debug.WriteLine($"App.{nameof(AppShutdownRequested)}");
            OnShutdownRequested(e);
        }

        protected virtual void OnShutdownRequested(ShutdownRequestedEventArgs e)
        {
            ShutdownRequested?.Invoke(this, e);
        }

        public void MainWindow_Closing(object? sender, Avalonia.Controls.WindowClosingEventArgs e)
        {           
            if (MainWindow != null && MainWindow.ShowCloseApplicationDialog)
            {
                //if (Xceed.Wpf.Toolkit.MessageBox.Show("Do you really want to exit?", "Exit Application", MessageBoxButton.YesNo, MessageBoxImage.None, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    //    if (mainWindow.PageVM.Model.TaskState != DNNTaskStates.Stopped)
                    //        mainWindow.PageVM.Model.Stop();

                    //    if (Xceed.Wpf.Toolkit.MessageBox.Show("Do you want to save the network state?", "Save Network State", MessageBoxButton.YesNo, MessageBoxImage.None, MessageBoxResult.Yes) == MessageBoxResult.Yes)
                    //    {
                    //        var dataset = Settings.Default.Dataset.ToString().ToLower(CultureInfo.CurrentCulture);
                    //        var optimizer = Settings.Default.Optimizer.ToString().ToLower(CultureInfo.CurrentCulture);
                    //        var fileName = Convnet.MainWindow.StateDirectory + mainWindow.PageVM.Model.Name + @"-(" + dataset + @")" + (Settings.Default.PersistOptimizer ? (@"(" + optimizer + @").bin") : @".bin");
                    //        mainWindow.PageVM.Model.SaveWeights(fileName, Settings.Default.PersistOptimizer);
                    //    }

                    if (MainWindow.PageVM != null && MainWindow.PageVM.Pages != null)
                    { 
                        //var editPV = MainWindow.PageVM.Pages[(int)PageViewModels.ViewModels.Edit] as EditPageViewModel;
                        //if (editPV != null)
                        //    Settings.Default.Script = editPV.Script;
                        
                        Settings.Default.Save();
                        e.Cancel = false;
                    }
                }
                //else
                  //  e.Cancel = true;
            }
            else
            {
                Settings.Default.Save();
                e.Cancel = false;
            }   
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Free managed objects.
                if (MainWindow != null)
                {
                    Settings.Default.Save();
                    MainWindow.Closing -= MainWindow_Closing;
                    MainWindow.Dispose();
                }
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