using Avalonia.Controls;
using Avalonia.Input;
using ReactiveUI;
using System.Reactive;


namespace ConvnetAvalonia.PageViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        public ReactiveCommand<Unit, Unit> CutCommand { get; }

        private bool canCut = true;

        public bool CanCut
        {
            get => canCut;
            set => this.RaiseAndSetIfChanged(ref canCut, value);
        }

        public MainWindowViewModel()
        {
            CutCommand = ReactiveCommand.Create(Cut, this.WhenAnyValue(x => x.canCut));
        }


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


    }
}
