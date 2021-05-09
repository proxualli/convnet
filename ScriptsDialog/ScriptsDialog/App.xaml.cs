using ScriptsDialog.Properties;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Markup;

namespace ScriptsDialog
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, IDisposable
    {
        static readonly SingleInstanceMutex sim;
        public MainWindow mainWindow;

        static App()
        {
            sim = new SingleInstanceMutex();

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
            if (sim.IsOtherInstanceRunning)
            {
                Application.Current.Shutdown();
                return;
            }

            base.OnStartup(e);

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

    /// <summary>
    /// Represents a <see cref="SingleInstanceMutex"/> class.
    /// </summary>
    public partial class SingleInstanceMutex : IDisposable
    {
        #region Fields

        /// <summary>
        /// Indicator whether another instance of this application is running or not.
        /// </summary>
        private bool isNoOtherInstanceRunning;

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
            this.singleInstanceMutex = new Mutex(true, Assembly.GetCallingAssembly().FullName, out this.isNoOtherInstanceRunning);
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
                return !this.isNoOtherInstanceRunning;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Closes the <see cref="SingleInstanceMutex"/>.
        /// </summary>
        public void Close()
        {
            this.ThrowIfDisposed();
            this.singleInstanceMutex.Close();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                /* Release unmanaged ressources */

                if (disposing)
                {
                    /* Release managed ressources */
                    this.Close();
                }

                this.disposed = true;
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
            if (this.disposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }

        #endregion
    }
}
