using ScriptsDialog.Properties;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;

namespace ScriptsDialog
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, IDisposable
    {
        MainWindow mainWindow;

        static App()
        {
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
            Settings.Default.Save();
            e.Cancel = false;
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
