﻿using Avalonia.Controls;
using DynamicData;
using Interop;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentAvalonia;
using Avalonia.Collections;
using FluentAvalonia.UI.Controls;

namespace ConvnetAvalonia.PageViewModels
{
    public abstract class PageViewModelBase : ReactiveObject
    {
        const string Framework = @"net8.0";
#if DEBUG
        const string Mode = "Debug";
#else
        const string Mode = @"Release";
#endif

        public static string ApplicationPath { get; } = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar;
        public static string StorageDirectory { get; } = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + Path.DirectorySeparatorChar + "convnet" + Path.DirectorySeparatorChar;
        public static string StateDirectory { get; } = StorageDirectory + "state" + Path.DirectorySeparatorChar;
        public static string DefinitionsDirectory { get; } = StorageDirectory + "definitions" + Path.DirectorySeparatorChar;
        public static string ScriptsDirectory { get; } = StorageDirectory + "scripts" + Path.DirectorySeparatorChar;
        public static string ScriptPath { get; } = ScriptsDirectory + "Scripts" + Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar + Mode + Path.DirectorySeparatorChar + Framework + Path.DirectorySeparatorChar;
        public static IEnumerable<DNNOptimizers> GetOptimizers => Enum.GetValues(typeof(DNNOptimizers)).Cast<DNNOptimizers>();
        public static IEnumerable<DNNInterpolations> GetInterpolations => Enum.GetValues(typeof(DNNInterpolations)).Cast<DNNInterpolations>();

        public event EventHandler? Modelhanged;


        public abstract string DisplayName { get; }

        public abstract void Reset();


        private ObservableCollection<Control> commandToolBar;
        public ObservableCollection<Control> CommandToolBar
        {
            get => commandToolBar;
            set => this.RaiseAndSetIfChanged(ref commandToolBar, value);
        }

        private bool commandToolBarVisibility;
        public bool CommandToolBarVisibility
        {
            get => commandToolBarVisibility;
            set => this.RaiseAndSetIfChanged(ref commandToolBarVisibility, value);
        }

        private ObservableCollection<DNNCostLayer>? costLayers;
        public ObservableCollection<DNNCostLayer>? CostLayers
        {
            get => costLayers;
            set => this.RaiseAndSetIfChanged(ref costLayers, value);
        }

        private int costIndex;
        public int CostIndex
        {
            get => costIndex;
            set => this.RaiseAndSetIfChanged(ref costIndex, value);
        }

        private bool isValid = true;
        public bool IsValid
        {
            get => isValid;
            set => this.RaiseAndSetIfChanged(ref isValid, value);
        }

        private DNNDatasets dataset;
        public DNNDatasets Dataset
        {
            get => dataset;
            set => this.RaiseAndSetIfChanged(ref dataset, value);
        }

        private void OnModelChanged()
        {
            Modelhanged?.Invoke(this, EventArgs.Empty);
        }

        private DNNModel? model;
        public DNNModel? Model
        {
            get => model;
            set
            {
                this.RaiseAndSetIfChanged(ref model, value);
                if (model != null)
                {
                    Dataset = model.Dataset;
                    OnModelChanged();
                }
            }
        }

        private void CommandToolBarCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (CommandToolBar.Count > 0)
                CommandToolBarVisibility = true;
            else
                CommandToolBarVisibility = false;
        }

        protected PageViewModelBase(DNNModel? model)
        {
            if (model != null)
            {
                Model = model;
                dataset = Model.Dataset;
                costLayers = new ObservableCollection<DNNCostLayer>(Model.CostLayers);
                costIndex = (int)Model.CostIndex;
            }

            commandToolBarVisibility = false;
            commandToolBar = new ObservableCollection<Control>();
            commandToolBar.CollectionChanged += new NotifyCollectionChangedEventHandler(CommandToolBarCollectionChanged);
        }
    }
}
