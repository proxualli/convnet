using dnncore;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace Convnet.PageViewModels
{
    public abstract class PageViewModelBase : INotifyPropertyChanged
    {
#if DEBUG
        const string Mode = "Debug";
#else
        const string Mode = "Release";
#endif
        const string Framework = @"\netcoreapp3.1\";
        public static string ApplicationPath { get; } = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\";
        public static string StorageDirectory { get; } = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\convnet\";
        public static string StateDirectory { get; } = StorageDirectory + @"state\";
        public static string DefinitionsDirectory { get; } = StorageDirectory + @"definitions\";
        public static string ScriptsDirectory { get; } = StorageDirectory + @"scripts\";
        public static string ScriptPath { get; } = ScriptsDirectory + @"ScriptsDialog\bin\" + Mode + Framework;


        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler Modelhanged;

        private Model model;
        private DNNDatasets dataset;
        private ObservableCollection<Control> commandToolBar;
        private Visibility commandToolBarVisibility;
        private bool isValid = true;
        private ObservableCollection<DNNCostLayer> costLayers;
        private int costIndex;

        protected PageViewModelBase(Model model)
        {
            if (model != null)
            {
                Model = model;
                dataset = Model.Dataset;
                costLayers = new ObservableCollection<DNNCostLayer>(Model.CostLayers);
                costIndex = (int)Model.CostIndex;
            }

            commandToolBarVisibility = Visibility.Hidden;
            commandToolBar = new ObservableCollection<Control>();
            commandToolBar.CollectionChanged += new NotifyCollectionChangedEventHandler(CommandToolBarCollectionChanged);
        }

        void CommandToolBarCollectionChanged(Object sender, NotifyCollectionChangedEventArgs e)
        {
            if (CommandToolBar.Count > 0)
                CommandToolBarVisibility = Visibility.Visible;
            else
                CommandToolBarVisibility = Visibility.Collapsed;
        }

        void OnModelChanged()
        {
            Modelhanged?.Invoke(this, EventArgs.Empty);
        }

        public Model Model
        {
            get { return model; }
            set
            {
                if (value == null)
                    return;

                if (value == model)
                    return;

                model = value;
                Dataset = model.Dataset;

                this.OnPropertyChanged(nameof(Model));
                this.OnModelChanged();
            }
        }

        public DNNDatasets Dataset
        {
            get { return dataset; }
            set
            {
                if (value == dataset)
                    return;

                dataset = value;
                OnPropertyChanged(nameof(Dataset));
            }
        }

        public ObservableCollection<DNNCostLayer> CostLayers
        {
            get { return costLayers; }
            set
            {
                if (value == costLayers)
                    return;

                costLayers = value;
                OnPropertyChanged(nameof(CostLayers));
            }
        }

        public int CostIndex
        {
            get { return costIndex; }
            set
            {
                if (value == costIndex)
                    return;

                costIndex = value;
                OnPropertyChanged(nameof(CostIndex));
            }
        }

        public ObservableCollection<Control> CommandToolBar
        {
            get { return commandToolBar; }
            set
            {
                if (value == commandToolBar)
                    return;

                commandToolBar = value;
                OnPropertyChanged(nameof(CommandToolBar));
            }
        }

        public Visibility CommandToolBarVisibility
        {
            get { return commandToolBarVisibility; }

            set
            {
                if (commandToolBarVisibility == value)
                    return;

                commandToolBarVisibility = value;
                OnPropertyChanged(nameof(CommandToolBarVisibility));
            }
        }

        public abstract string DisplayName { get; }

        public abstract void Reset();

        public bool IsValid
        {
            get { return isValid; }
            set
            {
                if (value == isValid)
                    return;

                isValid = value;
                OnPropertyChanged(nameof(IsValid));
            }
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
