using ConvnetAvalonia.Properties;
using Interop;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Float = System.Single;
using UInt = System.UInt64;

namespace ConvnetAvalonia.PageViewModels
{
    public enum ViewModels
    {
        Edit = 0,
        Test = 1,
        Train = 2
    }

    public class PageViewModel : PageViewModelBase
    {
        public event EventHandler? PageChange;

         public void Cut()
        {
           
            //if (PageVM != null && PageVM.Pages != null)
            //{
            //    //var epvm = MainView.PageViews.Items[(int)PageViewModels.ViewModels.Edit] as EditPageViewModel;

            //    var epvm = PageVM.Pages[(int)PageViewModels.ViewModels.Edit] as EditPageViewModel;
            //    if (epvm != null)
            //    {
            //        var topLevel = TopLevel.GetTopLevel(this);
            //        if (FocusManager != null)
            //        {
            //            var elem = FocusManager.GetFocusedElement();

            //        }
            //    }
            //}
        }

        public bool CanCut()
        {
            //if (PageVM != null && PageVM.Pages != null)
            //{
            //    //var epvm = MainView.PageViews.Items[(int)PageViewModels.ViewModels.Edit] as EditPageViewModel;

            //    var epvm = PageVM.Pages[(int)PageViewModels.ViewModels.Edit] as EditPageViewModel;
            //    if (epvm != null)
            //    {
            //        var topLevel = TopLevel.GetTopLevel(this);
            //        if (FocusManager != null)
            //        {
            //            var elem = FocusManager.GetFocusedElement();
            //            return true;
            //        }
            //    }
            //}

            return true;
        }
        public PageViewModel(DNNModel model) : base(model)
        {
            Settings.Default.PropertyChanged += Default_PropertyChanged;

            progressBarMinimum = 0.0;
            progressBarMaximum = 100.0;

            if (Model != null)
            {
                Model.TrainProgress += TrainProgress;
                Model.TestProgress += TestProgress;

                var EditPageVM = new EditPageViewModel(Model);
                EditPageVM.Open += PageVM_Open;
                EditPageVM.Save += PageVM_SaveAs;
                EditPageVM.Modelhanged += EditPageVM_ModelChanged;

                //var TestPageVM = new TestPageViewModel(Model);
                //TestPageVM.Open += PageVM_Open;

                var TrainPageVM = new TrainPageViewModel(Model);
                TrainPageVM.Open += PageVM_Open;
                TrainPageVM.Save += PageVM_Save;

                //var listPages = new List<ConvnetAvalonia.ViewModels.ViewModelBase> { EditPageVM, TestPageVM, TrainPageVM };
                var listPages = new List<PageViewModelBase> { EditPageVM, TrainPageVM };
                Pages = new ReadOnlyCollection<PageViewModelBase>(listPages);
                CurrentPage = Pages[Settings.Default.CurrentPage];
            }
        }

        private void Default_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Settings.Default.Save();
        }

        private void PageVM_Open(object? sender, EventArgs e)
        {
            //if (ApplicationCommands.Open.CanExecute(null, null))
            //    ApplicationCommands.Open.Execute(null, null);
        }

        private void PageVM_Save(object? sender, EventArgs e)
        {
            //if (ApplicationCommands.Save.CanExecute(null, null))
            //    ApplicationCommands.Save.Execute(null, null);
        }

        private void PageVM_SaveAs(object? sender, EventArgs e)
        {
            //if (ApplicationCommands.SaveAs.CanExecute(null, null))
            //    ApplicationCommands.SaveAs.Execute(null, null);
        }

        private void TrainProgress(DNNOptimizers Optimizer, UInt BatchSize, UInt Cycle, UInt TotalCycles, UInt Epoch, UInt TotalEpochs, bool HorizontalMirror, bool VerticalMirror, Float InputDropOut, Float Cutout, bool CutMix, Float AutoAugment, Float ColorCast, UInt ColorRadius, Float Distortion, DNNInterpolations Interpolation, Float Scaling, Float Rotation, UInt SampleIndex, Float Rate, Float Momentum, Float Beta2, Float Gamma, Float L2Penalty, Float DropOut, Float AvgTrainLoss, Float TrainErrorPercentage, Float TrainAccuracy, UInt TrainErrors, Float AvgTestLoss, Float TestErrorPercentage, Float Accuracy, UInt64 TestErrors, DNNStates NetworkState, DNNTaskStates TaskState)
        {
            if (Model != null)
            {
                switch (NetworkState)
                {
                    case DNNStates.Training:
                        ProgressValue = (double)(SampleIndex + 1ul) / Model.AdjustedTrainSamplesCount;
                        ProgressBarMaximum = Model.AdjustedTrainSamplesCount;
                        break;

                    case DNNStates.Testing:
                        ProgressValue = (double)(SampleIndex + 1ul) / Model.AdjustedTestSamplesCount;
                        ProgressBarMaximum = Model.AdjustedTestSamplesCount;
                        break;

                    default:
                        ProgressBarValue = 0.0;
                        SampleIndex = 0ul;
                        break;
                }

                ProgressBarValue = SampleIndex;
                Duration = Model.DurationString;
                SampleRate = Model.SampleRate.ToString("N1");
            }
        }

        private void TestProgress(UInt BatchSize, UInt SampleIndex, Float AvgTestLoss, Float TestErrorPercentage, Float Accuracy, UInt TestErrors, DNNStates NetworkState, DNNTaskStates TaskState)
        {
            if (Model != null)
            {
                if (NetworkState == DNNStates.Testing)
                {
                    ProgressValue = (double)(SampleIndex + 1ul) / Model.AdjustedTestSamplesCount;
                    ProgressBarMaximum = Model.AdjustedTestSamplesCount;
                }
                else
                {
                    ProgressValue = 0.0;
                    SampleIndex = 0ul;
                }

                ProgressBarValue = SampleIndex;
                Duration = Model.DurationString;
                SampleRate = Model.SampleRate.ToString("N1");
            }
        }

        private void EditPageVM_ModelChanged(object? sender, EventArgs e)
        {
            if (Pages != null && Pages.Count > 0)
            {
                Model = Pages[(int)ViewModels.Edit].Model;
                if (Model != null && Pages.Count == 3)
                {
                    Model.TrainProgress += TrainProgress;
                    Model.TestProgress += TestProgress;
                    Pages[(int)ViewModels.Train].Model = Model;
                    Pages[(int)ViewModels.Test].Model = Model;
                }
            }
        }

        public ViewModels CurrentModel => (ViewModels)Pages.IndexOf(CurrentPage);

        public override string DisplayName => "Main";

        
        private string? sampleRate;
        public string? SampleRate
        {
            get => sampleRate;
            set => this.RaiseAndSetIfChanged(ref sampleRate, value);
        }

        private string? duration;
        public string? Duration
        {
            get => duration;
            set => this.RaiseAndSetIfChanged(ref duration, value);
        }

        private double progressBarMinimum = 0;
        public double ProgressBarMinimum
        {
            get => progressBarMinimum;
            set => this.RaiseAndSetIfChanged(ref progressBarMinimum, value);
        }

        private double progressBarMaximum = 100;
        public double ProgressBarMaximum
        {
            get => progressBarMaximum;
            set => this.RaiseAndSetIfChanged(ref progressBarMaximum, value);
        }

        private double progressBarValue = 0;
        public double ProgressBarValue
        {
            get => progressBarValue;
            set => this.RaiseAndSetIfChanged(ref progressBarValue, value);
        }

        private double progressValue = 0;
        public double ProgressValue
        {
            get => progressValue;
            set => this.RaiseAndSetIfChanged(ref progressValue, value);
        }

        public ReadOnlyCollection<PageViewModelBase>? Pages { get; }

        private PageViewModelBase? currentPage;
        public PageViewModelBase? CurrentPage
        {
            get => currentPage; 
            set
            {
                if (value == currentPage || value == null)
                    return;

                this.RaiseAndSetIfChanged(ref currentPage, value);
                if (currentPage != null && Pages != null)
                {
                    CommandToolBar = currentPage.CommandToolBar;
                    CommandToolBarVisibility = currentPage.CommandToolBarVisibility;
                    Settings.Default.CurrentPage = Pages.IndexOf(currentPage);
                    Settings.Default.Save();

                    OnPageChange();
                }
            }
        }

        public void OnPageChange()
        {
            PageChange?.Invoke(this, EventArgs.Empty);

            if (Settings.Default.CurrentPage == (int)ViewModels.Edit && Pages != null)
            {
                var vm = Pages[(int)ViewModels.Edit] as EditPageViewModel;
                vm?.CheckButtonClick(this, new Avalonia.Interactivity.RoutedEventArgs());
            }

            //if (Settings.Default.CurrentPage == (int)ViewModels.Test)
            //{
            //    var testPVM = Pages[(int)ViewModels.Test] as TestPageViewModel;

            //    if (testPVM.Model != null)
            //    {
            //        testPVM.CommandToolBar[0].Visibility = testPVM.Model.TaskState == DNNTaskStates.Stopped ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            //        testPVM.CommandToolBar[1].Visibility = System.Windows.Visibility.Collapsed;
            //        testPVM.CommandToolBar[2].Visibility = System.Windows.Visibility.Collapsed;
            //    }

            //    if (CostLayers.Count > 1)
            //        testPVM.CostLayersComboBox_SelectionChanged(this, null);
            //}

            if (Settings.Default.CurrentPage == (int)ViewModels.Train && CostLayers?.Count > 1)
                (Pages[(int)ViewModels.Train] as TrainPageViewModel)?.CostLayersComboBox_SelectionChanged(this, null);
        }

        public override void Reset()
        {
            if (Pages != null)
                foreach (PageViewModelBase page in Pages)
                    page.Reset();
        }
    }
}