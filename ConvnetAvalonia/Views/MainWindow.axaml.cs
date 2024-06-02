using Avalonia.Controls;
using System;

namespace ConvnetAvalonia
{
    public partial class MainWindow : Window
    {
        public bool ShowCloseApplicationDialog = true;

        public MainWindow()
        {
            InitializeComponent();
        }

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