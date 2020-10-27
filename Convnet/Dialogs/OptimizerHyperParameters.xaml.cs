using Convnet.Properties;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using fp = System.Single;

namespace Convnet.Dialogs
{
    public partial class OptimizerHyperParameters : Window, INotifyPropertyChanged
    {
        private fp adaDeltaEps;
        private fp adaGradEps;
        private fp adamEps;
        private fp adamBeta2;
        private fp adamaxEps;
        private fp adamaxBeta2;
        private fp rmsPropEps;
        private fp radamEps;
        private fp radamBeta1;
        private fp radamBeta2;

        public event PropertyChangedEventHandler PropertyChanged;

        public fp AdaDeltaEps
        {
            get { return adaDeltaEps; }
            set
            {
                if (value == adaDeltaEps)
                    return;

                adaDeltaEps = value;
                OnPropertyChanged(nameof(AdaDeltaEps));
            }
        }

        public fp AdaGradEps
        {
            get { return adaGradEps; }
            set
            {
                if (value == adaGradEps)
                    return;

                adaGradEps = value;
                OnPropertyChanged(nameof(AdaGradEps));
            }
        }

        public fp AdamEps
        {
            get { return adamEps; }
            set
            {
                if (value == adamEps)
                    return;

                adamEps = value;
                OnPropertyChanged(nameof(AdamEps));
            }
        }

        public fp AdamBeta2
        {
            get { return adamBeta2; }
            set
            {
                if (value == adamBeta2)
                    return;

                adamBeta2 = value;
                OnPropertyChanged(nameof(AdamBeta2));
            }
        }

        public fp AdamaxEps
        {
            get { return adamaxEps; }
            set
            {
                if (value == adamaxEps)
                    return;

                adamaxEps = value;
                OnPropertyChanged(nameof(AdamaxEps));
            }
        }

        public fp AdamaxBeta2
        {
            get { return adamaxBeta2; }
            set
            {
                if (value == adamaxBeta2)
                    return;

                adamaxBeta2 = value;
                OnPropertyChanged(nameof(AdamaxBeta2));
            }
        }

        public fp RMSPropEps
        {
            get { return rmsPropEps; }
            set
            {
                if (value == rmsPropEps)
                    return;

                rmsPropEps = value;
                OnPropertyChanged(nameof(RMSPropEps));
            }
        }

        public fp RAdamEps
        {
            get { return radamEps; }
            set
            {
                if (value == radamEps)
                    return;

                radamEps = value;
                OnPropertyChanged(nameof(RAdamEps));
            }
        }

        public fp RAdamBeta1
        {
            get { return radamBeta1; }
            set
            {
                if (value == radamBeta1)
                    return;

                radamBeta1 = value;
                OnPropertyChanged(nameof(RAdamBeta1));
            }
        }
        public fp RAdamBeta2
        {
            get { return radamBeta2; }
            set
            {
                if (value == radamBeta2)
                    return;

                radamBeta2 = value;
                OnPropertyChanged(nameof(RAdamBeta2));
            }
        }

        public OptimizerHyperParameters()
        {
            InitializeComponent();

            AdaDeltaEps = Settings.Default.AdaDeltaEps;
            AdaGradEps = Settings.Default.AdaGradEps;
            AdamEps = Settings.Default.AdamEps;
            AdamBeta2 = Settings.Default.AdamBeta2;
            AdamaxEps = Settings.Default.AdamaxEps;
            AdamaxBeta2 = Settings.Default.AdamaxBeta2;
            RMSPropEps = Settings.Default.RMSpropEps;
            RAdamEps = Settings.Default.RAdamEps;
            RAdamBeta1 = Settings.Default.RAdamBeta1;
            RAdamBeta2 = Settings.Default.RAdamBeta2;

            DataContext = this;
        }

        private void ButtonReset_Click(object sender, RoutedEventArgs e)
        {
            AdaDeltaEps = (fp)1e-08;
            AdaGradEps = (fp)1e-08;
            AdamEps = (fp)1e-08;
            AdamBeta2 = (fp)0.999;
            AdamaxEps = (fp)1e-08;
            AdamaxBeta2 = (fp)0.999;
            RMSPropEps = (fp)1e-08;
            RAdamEps = (fp)1e-08;
            RAdamBeta1 = (fp)0.9;
            RAdamBeta2 = (fp)0.999;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            if (IsValid(this))
            {
                Settings.Default.AdaDeltaEps = AdaDeltaEps;
                Settings.Default.AdaGradEps = AdaGradEps;
                Settings.Default.AdamEps = AdamEps;
                Settings.Default.AdamBeta2 = AdamBeta2;
                Settings.Default.AdamaxEps = AdamaxEps;
                Settings.Default.AdamaxBeta2 = AdamaxBeta2;
                Settings.Default.RMSpropEps = RMSPropEps;
                Settings.Default.RAdamEps = RAdamEps;
                Settings.Default.RAdamBeta1 = RAdamBeta1;
                Settings.Default.RAdamBeta2 = RAdamBeta2;

                Settings.Default.Save();

                DialogResult = true;
                Close();
            }
        }

        bool IsValid(DependencyObject node)
        {
            // Check if dependency object was passed
            if (node != null)
            {
                // Check if dependency object is valid.
                // NOTE: Validation.GetHasError works for controls that have validation rules attached 
                bool isValid = !Validation.GetHasError(node);
                if (!isValid)
                {
                    // If the dependency object is invalid, and it can receive the focus,
                    // set the focus
                    if (node is IInputElement) Keyboard.Focus((IInputElement)node);
                    return false;
                }
            }

            // If this dependency object is valid, check all child dependency objects
            foreach (object subnode in LogicalTreeHelper.GetChildren(node))
            {
                if (subnode is DependencyObject)
                {
                    // If a child dependency object is invalid, return false immediately,
                    // otherwise keep checking
                    if (IsValid((DependencyObject)subnode) == false) return false;
                }

            }

            // All dependency objects are valid
            return true;
        }


        #region INotifyPropertyChanged Members

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.VerifyPropertyName(propertyName);

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Debugging Aides

        /// <summary>
        /// Warns the developer if this object does not have
        /// a public property with the specified name. This 
        /// method does not exist in a Release build.
        /// </summary>
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName)
        {
            // If you raise PropertyChanged and do not specify a property name,
            // all properties on the object are considered to be changed by the binding system.
            if (String.IsNullOrEmpty(propertyName))
                return;

            // Verify that the property name matches a real,  
            // public, instance property on this object.
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                string msg = "Invalid property name: " + propertyName;

                if (this.ThrowOnInvalidPropertyName)
                    throw new ArgumentException(msg);
                else
                    Debug.Fail(msg);
            }
        }

        /// <summary>
        /// Returns whether an exception is thrown, or if a Debug.Fail() is used
        /// when an invalid property name is passed to the VerifyPropertyName method.
        /// The default value is false, but subclasses used by unit tests might 
        /// override this property's getter to return true.
        /// </summary>
        protected virtual bool ThrowOnInvalidPropertyName { get; private set; }

        #endregion // Debugging Aides

        #endregion // INotifyPropertyChanged Members
    }
}
