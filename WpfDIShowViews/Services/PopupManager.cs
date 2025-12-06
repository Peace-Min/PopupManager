using System.Collections.ObjectModel;
using System.Windows;
using WpfDIShowViews.ViewModels;

namespace WpfDIShowViews.Services
{
    public class PopupManager : IPopupManager
    {
        private readonly IDialogService _dialogService;

        public PopupManager(IDialogService dialogService)
        {
            _dialogService = dialogService;
        }

        public ObservableCollection<ViewModelBase> MinimizedPopups { get; } = new ObservableCollection<ViewModelBase>();

        public void Add(ViewModelBase viewModel)
        {
            if (!MinimizedPopups.Contains(viewModel))
            {
                MinimizedPopups.Add(viewModel);
                _dialogService.Hide(viewModel);
            }
        }

        public void Remove(ViewModelBase viewModel)
        {
            if (MinimizedPopups.Contains(viewModel))
            {
                MinimizedPopups.Remove(viewModel);
            }
            _dialogService.Close(viewModel);
        }

        public void Restore(ViewModelBase viewModel)
        {
            if (MinimizedPopups.Contains(viewModel))
            {
                MinimizedPopups.Remove(viewModel);
                _dialogService.Show(viewModel);
            }
        }
    }
}
