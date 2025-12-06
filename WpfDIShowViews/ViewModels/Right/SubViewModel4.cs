using CommunityToolkit.Mvvm.Input;
using System.Windows;
using System.Windows.Input;
using WpfDIShowViews.Models;
using WpfDIShowViews.Services;

namespace WpfDIShowViews.ViewModels.Right
{
    public class SubViewModel4 : ViewModelBase, IParameterReceiver, IGroupableViewModel
    {
        public string GroupName => "Right";
        private readonly IPopupManager _popupManager;

        public SubViewModel4(IPopupManager popupManager)
        {
            _popupManager = popupManager;
        }

        public SubData SubData { get; set; } = default!;

        public void ReceiveParameter(object parameter)
        {
            if (parameter is SubData subData)
            {
                SubData = subData;
            }
        }

        public ICommand CloseCommand => new RelayCommand<object>(CloseCommandAction);

        private void CloseCommandAction(object? obj)
        {
            _popupManager.Remove(this);
        }

        public ICommand MinimizeCommand => new RelayCommand<object>(MinimizeCommandAction);

        private void MinimizeCommandAction(object? obj)
        {
            _popupManager.Add(this);
        }
    }
}
