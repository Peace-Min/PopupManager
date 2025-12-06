using System.Collections.ObjectModel;
using WpfDIShowViews.ViewModels;

namespace WpfDIShowViews.Services
{
    public interface IPopupManager
    {
        ObservableCollection<ViewModelBase> MinimizedPopups { get; }
        void Add(ViewModelBase viewModel);
        void Remove(ViewModelBase viewModel);
        void Restore(ViewModelBase viewModel);
    }
}
