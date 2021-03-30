using Convnet.Properties;
using dnncore;
using Microsoft.Build.Locator;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;

namespace Convnet
{
    public partial class App : Application, IDisposable
    {
        public MainWindow mainWindow;

        static App()
        {
            MSBuildLocator.RegisterDefaults();

            Process.GetCurrentProcess().PriorityClass = Settings.Default.Priority;
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
            FrameworkContentElement.LanguageProperty.OverrideMetadata(typeof(System.Windows.Documents.TextElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
        }

        ~App()
        {
            // In case the client forgets to call
            // Dispose , destructor will be invoked for
            Dispose(false);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                mainWindow = new MainWindow();
                mainWindow.Closing += MainWindow_Closing;
                mainWindow.Show();
            }
            catch (Exception)
            {
            }
        }
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (mainWindow.ShowCloseApplicationDialog)
            {
                if (Xceed.Wpf.Toolkit.MessageBox.Show("Do you really want to exit?", "Exit Application", MessageBoxButton.YesNo, MessageBoxImage.None, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    if (Xceed.Wpf.Toolkit.MessageBox.Show("Do you want to save the network state?", "Save Network State", MessageBoxButton.YesNo, MessageBoxImage.None, MessageBoxResult.Yes) == MessageBoxResult.Yes)
                        mainWindow.PageVM.Model.SaveWeights(global::Convnet.MainWindow.StateDirectory + mainWindow.PageVM.Model.Name + ".weights", (bool)global::Convnet.Properties.Settings.Default.PersistOptimizer);
                    
                    if (mainWindow.PageVM.Model.TaskState != DNNTaskStates.Stopped)
                        mainWindow.PageVM.Model.Stop();
                        
                    Settings.Default.Save();

                    e.Cancel = false;
                }
                else
                    e.Cancel = true;
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
                mainWindow.Closing -= MainWindow_Closing;
                mainWindow.Dispose();
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
