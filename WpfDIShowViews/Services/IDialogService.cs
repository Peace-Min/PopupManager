using System;

namespace WpfDIShowViews.Services
{
    public interface IDialogService
    {
        void Register<TViewModel, TView>() where TViewModel : class where TView : class;
        void Show(object viewModel, bool isMaximized = false);
        void ShowDialog(object viewModel);
        void Close(object viewModel);
        void Hide(object viewModel);
    }
}
