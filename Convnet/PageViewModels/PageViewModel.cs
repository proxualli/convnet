using Convnet.Properties;
using dnncore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Float = System.Single;
using UInt = System.UInt64;

namespace Convnet.PageViewModels
{
    public enum ViewModels
    {
        Edit = 0,
        Test = 1,
        Train = 2
    }

    public class PageViewModel : PageViewModelBase
    {
        private PageViewModelBase currentPage;
        private double progressBarMinimum;
        private double progressBarMaximum;
        private double progressBarValue;
        private double progressValue;
        private string sampleRate;
        private string duration;

        public event EventHandler PageChange;

        public PageViewModel(DNNModel model) : base(model)
        {
            Settings.Default.PropertyChanged += Default_PropertyChanged;

            progressBarMinimum = 0.0;
            progressBarMaximum = 100.0;

            if (Model != null)
            {
                Model.TrainProgress += TrainProgress;
                Model.TestProgress += TestProgress;
            }

            var EditPageVM = new EditPageViewModel(Model);
            EditPageVM.Open += PageVM_Open;
            EditPageVM.Save += PageVM_SaveAs;
            EditPageVM.Modelhanged += EditPageVM_ModelChanged;

            var TestPageVM = new TestPageViewModel(Model);
            TestPageVM.Open += PageVM_Open;

            var TrainPageVM = new TrainPageViewModel(Model);
            TrainPageVM.Open += PageVM_Open;
            TrainPageVM.Save += PageVM_Save;

            List<PageViewModelBase> listPages = new List<PageViewModelBase> { EditPageVM, TestPageVM, TrainPageVM };

            Pages = new ReadOnlyCollection<PageViewModelBase>(listPages);
            CurrentPage = Pages[Settings.Default.CurrentPage];
        }

        private void Default_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Settings.Default.Save();
        }

        private void PageVM_Open(object sender, EventArgs e)
        {
            if (ApplicationCommands.Open.CanExecute(null, null))
                ApplicationCommands.Open.Execute(null, null);
        }

        private void PageVM_Save(object sender, EventArgs e)
        {
            if (ApplicationCommands.Save.CanExecute(null, null))
                ApplicationCommands.Save.Execute(null, null);
        }

        private void PageVM_SaveAs(object sender, EventArgs e)
        {
            if (ApplicationCommands.SaveAs.CanExecute(null, null))
                ApplicationCommands.SaveAs.Execute(null, null);
        }

        private void TrainProgress(DNNOptimizers Optimizer, UInt BatchSize, UInt Cycle, UInt TotalCycles, UInt Epoch, UInt TotalEpochs, bool HorizontalMirror, bool VerticalMirror, Float InputDropOut, Float Cutout, bool CutMix, Float AutoAugment, Float ColorCast, UInt ColorRadius, Float Distortion, DNNInterpolations Interpolation, Float Scaling, Float Rotation, UInt SampleIndex, Float Rate, Float Momentum, Float Beta2, Float Gamma, Float L2Penalty, Float DropOut, Float AvgTrainLoss, Float TrainErrorPercentage, Float TrainAccuracy, UInt TrainErrors, Float AvgTestLoss, Float TestErrorPercentage, Float Accuracy, UInt64 TestErrors, DNNStates NetworkState, DNNTaskStates TaskState)
        {
            switch (NetworkState)
            {
                case DNNStates.Training:
                    ProgressValue = (double)(SampleIndex + 1ul) / Model.AdjustedTrainingSamplesCount;
                    ProgressBarMaximum = Model.AdjustedTrainingSamplesCount;
                    break;

                case DNNStates.Testing:
                    ProgressValue = (double)(SampleIndex + 1ul) / Model.AdjustedTestingSamplesCount;
                    ProgressBarMaximum = Model.AdjustedTestingSamplesCount;
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

        private void TestProgress(UInt BatchSize, UInt SampleIndex, Float AvgTestLoss, Float TestErrorPercentage, Float Accuracy, UInt TestErrors, DNNStates NetworkState, DNNTaskStates TaskState)
        {
            if (NetworkState == DNNStates.Testing)
            {
                ProgressValue = (double)(SampleIndex + 1ul) / Model.AdjustedTestingSamplesCount;
                ProgressBarMaximum = Model.AdjustedTestingSamplesCount;
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

        private void EditPageVM_ModelChanged(object sender, EventArgs e)
        {
            if (Model != null)
            {
                Model = Pages[(int)ViewModels.Edit].Model;
                Model.TrainProgress += TrainProgress;
                Model.TestProgress += TestProgress;
                Pages[(int)ViewModels.Train].Model = Model;
                Pages[(int)ViewModels.Test].Model = Model;
            }
        }

        public ViewModels CurrentModel => (ViewModels)Pages.IndexOf(CurrentPage);

        public override string DisplayName => "Main";

        public string SampleRate
        {
            get { return sampleRate; }
            set
            {
                if (value == sampleRate)
                    return;

                sampleRate = value;
                OnPropertyChanged(nameof(SampleRate));
            }
        }

        public string Duration
        {
            get { return duration; }
            set
            {
                if (value == duration)
                    return;

                duration = value;
                OnPropertyChanged(nameof(Duration));
            }
        }

        public double ProgressBarMinimum
        {
            get { return progressBarMinimum; }
            set
            {
                if (value == progressBarMinimum)
                    return;

                progressBarMinimum = value;
                OnPropertyChanged(nameof(ProgressBarMinimum));
            }
        }

        public double ProgressBarMaximum
        {
            get { return progressBarMaximum; }
            set
            {
                if (value == progressBarMaximum)
                    return;

                progressBarMaximum = value;
                OnPropertyChanged(nameof(ProgressBarMaximum));
            }
        }

        public double ProgressBarValue
        {
            get { return progressBarValue; }
            set
            {
                if (value == progressBarValue)
                    return;

                progressBarValue = value;
                OnPropertyChanged(nameof(ProgressBarValue));
            }
        }

        public double ProgressValue
        {
            get { return progressValue; }
            set
            {
                if (value == progressValue)
                    return;

                progressValue = value;
                OnPropertyChanged(nameof(ProgressValue));
            }
        }

        public ReadOnlyCollection<PageViewModelBase> Pages { get; }

        public PageViewModelBase CurrentPage
        {
            get { return currentPage; }
            set
            {
                if (value == currentPage || value == null)
                    return;

                currentPage = value;
                OnPropertyChanged(nameof(CurrentPage));

                CommandToolBar = currentPage.CommandToolBar;
                CommandToolBarVisibility = currentPage.CommandToolBarVisibility;
                Settings.Default.CurrentPage = Pages.IndexOf(currentPage);
                Settings.Default.Save();
                
                OnPageChange();
            }
        }

        public void OnPageChange()
        {
            PageChange?.Invoke(this, EventArgs.Empty);

            if (Settings.Default.CurrentPage == (int)ViewModels.Edit)
                (Pages[(int)ViewModels.Edit] as EditPageViewModel).CheckButtonClick(this, new System.Windows.RoutedEventArgs());

            if (Settings.Default.CurrentPage == (int)ViewModels.Test)
            {
                var testPVM = Pages[(int)ViewModels.Test] as TestPageViewModel;

                if (testPVM.Model != null)
                {
                    testPVM.CommandToolBar[0].Visibility = testPVM.Model.TaskState == DNNTaskStates.Stopped ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                    testPVM.CommandToolBar[1].Visibility = System.Windows.Visibility.Collapsed;
                    testPVM.CommandToolBar[2].Visibility = System.Windows.Visibility.Collapsed;
                }

                if (CostLayers.Count > 1)
                    testPVM.CostLayersComboBox_SelectionChanged(this, null);
            }

            if (Settings.Default.CurrentPage == (int)ViewModels.Train && CostLayers.Count > 1)
                (Pages[(int)ViewModels.Train] as TrainPageViewModel).CostLayersComboBox_SelectionChanged(this, null);
        }

        public override void Reset()
        {
            foreach (PageViewModelBase page in Pages)
                page.Reset();
        }
    }
}