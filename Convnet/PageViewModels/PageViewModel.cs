using Convnet.Properties;
using dnncore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

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

        public PageViewModel(Model model) : base(model)
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
            Model.SetOptimizersHyperParameters(Settings.Default.AdaDeltaEps, Settings.Default.AdaGradEps, Settings.Default.AdamEps, Settings.Default.AdamBeta2, Settings.Default.AdamaxEps, Settings.Default.AdamaxBeta2, Settings.Default.RMSpropEps, Settings.Default.RAdamEps, Settings.Default.RAdamBeta1, Settings.Default.RAdamBeta2);
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

        private void TrainProgress(UInt64 BatchSize, UInt64 Cycle, UInt64 TotalCycles, UInt64 Epoch, UInt64 TotalEpochs, bool HorizontalMirror, bool VerticalMirror, float inputDropOut, float inputCutout, float AutoAugment, float ColorCast, UInt64 ColorRadius, float Distortion, DNNInterpolation Interpolation, float Scaling, float Rotation, UInt64 SampleIndex, float Rate, float Momentum, float L2Penalty, float AvgTrainLoss, float TrainErrorPercentage, float TrainAccuracy, UInt64 TrainErrors, float AvgTestLoss, float TestErrorPercentage, float Accuracy, UInt64 TestErrors, DNNStates NetworkState, DNNTaskStates TaskState)
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

        private void TestProgress(UInt64 BatchSize, UInt64 SampleIndex, float AvgTestLoss, float TestErrorPercentage, float Accuracy, UInt64 TestErrors, DNNStates NetworkState, DNNTaskStates TaskState)
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

        public PageViewModelBase CurrentPage
        {
            get { return currentPage; }
            set
            {
                if (value == currentPage)
                    return;

                currentPage = value;
                OnPropertyChanged(nameof(CurrentPage));

                if (currentPage != null)
                {
                    CommandToolBar = currentPage.CommandToolBar;
                    CommandToolBarVisibility = currentPage.CommandToolBarVisibility;
                    Settings.Default.CurrentPage = Pages.IndexOf(currentPage);
                    Settings.Default.Save();
                    OnPageChange();
                }
            }
        }

        public ReadOnlyCollection<PageViewModelBase> Pages { get; }

        public override void Reset()
        {
            foreach (PageViewModelBase page in Pages)
                page.Reset();
        }

        public void OnPageChange()
        {
            PageChange?.Invoke(this, EventArgs.Empty);

            if (Settings.Default.CurrentPage == (int)ViewModels.Edit)
                (Pages[(int)ViewModels.Edit] as EditPageViewModel).CheckButtonClick(this, new System.Windows.RoutedEventArgs());

            if (Settings.Default.CurrentPage == (int)ViewModels.Test && CostLayers.Count > 1)
                (Pages[(int)ViewModels.Test] as TestPageViewModel).CostLayersComboBox_SelectionChanged(this, null);

            if (Settings.Default.CurrentPage == (int)ViewModels.Train && CostLayers.Count > 1)
                (Pages[(int)ViewModels.Train] as TrainPageViewModel).CostLayersComboBox_SelectionChanged(this, null);
        }
    }
}

