using Convnet.Properties;
using Microsoft.Build.Locator;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
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

            try
            {
                System.Diagnostics.Process.GetCurrentProcess().PriorityClass = Settings.Default.Priority;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

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
            try
            {
                base.OnStartup(e);

                mainWindow = new MainWindow();
                mainWindow.Closing += MainWindow_Closing;
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Dispose();
            }
        }
        void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (mainWindow.ShowCloseApplicationDialog)
            {
                if (Xceed.Wpf.Toolkit.MessageBox.Show("Do you really want to exit?", "Exit Application", MessageBoxButton.YesNo, MessageBoxImage.None, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    if (mainWindow.PageVM.Model.TaskState != dnncore.DNNTaskStates.Stopped)
                        mainWindow.PageVM.Model.Stop();

                    if (Xceed.Wpf.Toolkit.MessageBox.Show("Do you want to save the network state?", "Save Network State", MessageBoxButton.YesNo, MessageBoxImage.None, MessageBoxResult.Yes) == MessageBoxResult.Yes)
                    {
                        string pathWeights = Settings.Default.PersistOptimizer ? Path.Combine(global::Convnet.MainWindow.StateDirectory, mainWindow.PageVM.Model.Name + "-" + Settings.Default.Optimizer.ToString().ToLower() + ".bin") : Path.Combine(global::Convnet.MainWindow.StateDirectory, mainWindow.PageVM.Model.Name + ".bin");
                        mainWindow.PageVM.Model.SaveWeights(pathWeights, global::Convnet.Properties.Settings.Default.PersistOptimizer);
                    }

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
