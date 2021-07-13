using Convnet.Properties;
using Microsoft.Build.Locator;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using dnncore;
using System.Threading;
using System.Reflection;

namespace Convnet
{
    public partial class App : Application, IDisposable
    {
        private static readonly SingleInstanceMutex sim;

        private MainWindow mainWindow;
       
        static App()
        {
            sim = new SingleInstanceMutex();

            try
            {
                MSBuildLocator.RegisterDefaults();

                System.Diagnostics.Process.GetCurrentProcess().PriorityClass = Settings.Default.Priority;

                FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
                FrameworkContentElement.LanguageProperty.OverrideMetadata(typeof(System.Windows.Documents.TextElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        ~App()
        {
            // In case the client forgets to call
            // Dispose , destructor will be invoked for
            Dispose(false);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            if (sim.IsOtherInstanceRunning)
            {
                Current.Shutdown();
                return;
            }

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

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (mainWindow.ShowCloseApplicationDialog)
            {
                if (Xceed.Wpf.Toolkit.MessageBox.Show("Do you really want to exit?", "Exit Application", MessageBoxButton.YesNo, MessageBoxImage.None, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    if (mainWindow.PageVM.Model.TaskState != DNNTaskStates.Stopped)
                        mainWindow.PageVM.Model.Stop();

                    if (Xceed.Wpf.Toolkit.MessageBox.Show("Do you want to save the network state?", "Save Network State", MessageBoxButton.YesNo, MessageBoxImage.None, MessageBoxResult.Yes) == MessageBoxResult.Yes)
                    {
                        string pathWeights = Settings.Default.PersistOptimizer ? Path.Combine(Convnet.MainWindow.StateDirectory, mainWindow.PageVM.Model.Name + "-(" + Settings.Default.Optimizer.ToString().ToLower(CultureInfo.CurrentCulture) + ").bin") : Path.Combine(Convnet.MainWindow.StateDirectory, mainWindow.PageVM.Model.Name + ".bin");
                        mainWindow.PageVM.Model.SaveWeights(pathWeights, Settings.Default.PersistOptimizer);
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

    /// <summary>
    /// Represents a <see cref="SingleInstanceMutex"/> class.
    /// </summary>
    public partial class SingleInstanceMutex : IDisposable
    {
        #region Fields

        /// <summary>
        /// Indicator whether another instance of this application is running or not.
        /// </summary>
        private readonly bool isNoOtherInstanceRunning;

        /// <summary>
        /// The <see cref="Mutex"/> used to ask for other instances of this application.
        /// </summary>
        private Mutex singleInstanceMutex = null;

        /// <summary>
        /// An indicator whether this object is beeing actively disposed or not.
        /// </summary>
        private bool disposed;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleInstanceMutex"/> class.
        /// </summary>
        public SingleInstanceMutex()
        {
            singleInstanceMutex = new Mutex(true, Assembly.GetCallingAssembly().FullName, out isNoOtherInstanceRunning);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets an indicator whether another instance of the application is running or not.
        /// </summary>
        public bool IsOtherInstanceRunning
        {
            get
            {
                return !isNoOtherInstanceRunning;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Closes the <see cref="SingleInstanceMutex"/>.
        /// </summary>
        public void Close()
        {
            ThrowIfDisposed();
            singleInstanceMutex.Close();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                /* Release unmanaged ressources */

                if (disposing)
                {
                    /* Release managed ressources */
                    Close();
                }

                disposed = true;
            }
        }

        /// <summary>
        /// Throws an exception if something is tried to be done with an already disposed object.
        /// </summary>
        /// <remarks>
        /// All public methods of the class must first call this.
        /// </remarks>
        public void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        #endregion
    }
}
