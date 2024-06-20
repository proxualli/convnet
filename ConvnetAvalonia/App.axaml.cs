using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ConvnetAvalonia.PageViewModels;
using ConvnetAvalonia.Properties;
using CustomMessageBox.Avalonia;
using Interop;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Threading;


namespace ConvnetAvalonia
{
    public partial class App : Application, IDisposable
    {
        public static readonly bool SingleInstanceApp = true;
        private static readonly SingleInstanceMutex sim = new SingleInstanceMutex();
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
                if (SingleInstanceApp)
                {
                    if (sim.IsOtherInstanceRunning)
                    {
                        return;
                    }
                }

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
                MessageBoxResult exit = MessageBoxResult.Yes;
                //exit = Dispatcher.UIThread.Invoke(() => MessageBox.Show(MainWindow, "Do you really want to exit?", "Exit Application", MessageBoxButtons.YesNo, MessageBoxIcon.None, MessageBoxDefaultButton.Button2)).Result;
               
                if (exit == MessageBoxResult.Yes)
                {
                    if (MainWindow?.PageVM?.Model?.TaskState != DNNTaskStates.Stopped)
                        MainWindow?.PageVM?.Model?.Stop();

                    MessageBoxResult save = MessageBoxResult.Yes;
                    //save = Dispatcher.UIThread.Invoke(() => MessageBox.Show("Do you want to save the network state?", "Save Network State", MessageBoxButtons.YesNo, MessageBoxIcon.None, MessageBoxDefaultButton.Button1)).Result;
                    
                    if (save == MessageBoxResult.Yes)
                    {
                        var dataset = Settings.Default.Dataset.ToString().ToLower(CultureInfo.CurrentCulture);
                        var optimizer = Settings.Default.Optimizer.ToString().ToLower(CultureInfo.CurrentCulture);
                        var fileName = PageViews.MainWindow.StateDirectory + MainWindow?.PageVM?.Model?.Name + @"-(" + dataset + @")" + (Settings.Default.PersistOptimizer ? (@"(" + optimizer + @").bin") : @".bin");

                        MainWindow?.PageVM?.Model?.SaveWeights(fileName, Settings.Default.PersistOptimizer);
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
}