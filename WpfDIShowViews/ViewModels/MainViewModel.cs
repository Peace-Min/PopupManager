using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WpfDIShowViews.Services;


namespace WpfDIShowViews.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly IPopupManager _popupManager;
        private readonly IServiceProvider _serviceProvider;

        public System.Collections.ObjectModel.ObservableCollection<ViewModelBase> MinimizedPopups => _popupManager.MinimizedPopups;

        public IEnumerable<ViewModelBase> LeftMinimizedPopups => MinimizedPopups.Where(vm => (vm as Models.IGroupableViewModel)?.GroupName == "Left");
        public IEnumerable<ViewModelBase> RightMinimizedPopups => MinimizedPopups.Where(vm => (vm as Models.IGroupableViewModel)?.GroupName == "Right");

        public int LeftCount => LeftMinimizedPopups.Count();
        public int RightCount => RightMinimizedPopups.Count();

        private void ShowSubView(object? obj)
        {
            if (obj == null) return;
            string type = (string)obj;

            ViewModelBase? viewModel = null;
            Models.SubData subData = new Models.SubData { StringData = "Data for " + type, IntData = int.Parse(type) };

            switch (type)
            {
                case "1":
                    viewModel = _serviceProvider.GetRequiredService<WpfDIShowViews.ViewModels.Left.SubViewModel1>();
                    break;
                case "2":
                    viewModel = _serviceProvider.GetRequiredService<WpfDIShowViews.ViewModels.Left.SubViewModel2>();
                    break;
                case "3":
                    viewModel = _serviceProvider.GetRequiredService<WpfDIShowViews.ViewModels.Right.SubViewModel3>();
                    break;
                case "4":
                    viewModel = _serviceProvider.GetRequiredService<WpfDIShowViews.ViewModels.Right.SubViewModel4>();
                    break;
            }

            if (viewModel != null)
            {
                var minimized = _popupManager.MinimizedPopups.FirstOrDefault(vm => vm.GetType() == viewModel.GetType());
                if (minimized != null)
                {
                    if (minimized is IParameterReceiver receiver)
                    {
                        receiver.ReceiveParameter(subData);
                    }
                    _popupManager.Restore(minimized);
                }
                else
                {
                    if (viewModel is IParameterReceiver receiver)
                    {
                        receiver.ReceiveParameter(subData);
                    }
                    _dialogService.Show(viewModel);
                }
            }
        }



        public MainViewModel(IDialogService dialogService, IPopupManager popupManager, IServiceProvider serviceProvider)
        {
            _dialogService = dialogService;
            _popupManager = popupManager;
            _serviceProvider = serviceProvider;

            _popupManager.MinimizedPopups.CollectionChanged += (s, args) =>
            {
                RaisePropertyChanged(nameof(LeftMinimizedPopups));
                RaisePropertyChanged(nameof(RightMinimizedPopups));
                RaisePropertyChanged(nameof(LeftCount));
                RaisePropertyChanged(nameof(RightCount));
            };
        }

        public ICommand ShowSubViewCommand => new RelayCommand<object>(ShowSubView);

        public ICommand RestorePopupCommand => new RelayCommand<ViewModelBase>(RestorePopup);
        private void RestorePopup(ViewModelBase? vm)
        {
            if (vm != null) _popupManager.Restore(vm);
        }

        public ICommand ClosePopupCommand => new RelayCommand<ViewModelBase>(ClosePopup);
        private void ClosePopup(ViewModelBase? vm)
        {
            if (vm != null) _popupManager.Remove(vm);
        }
    }
}
