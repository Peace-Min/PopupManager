using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WpfDIShowViews.ViewModels;

namespace WpfDIShowViews.Services
{
    public class DialogService : IDialogService
    {
        private readonly Dictionary<Type, Type> _mappingviewModelToView = new Dictionary<Type, Type>();
        private readonly Dictionary<Type, Window> _openWindows = new Dictionary<Type, Window>();

        public void Register<T1, T2>()
           where T1 : class
           where T2 : class
        {
            if (!this._mappingviewModelToView.ContainsKey(typeof(T1)))
            {
                this._mappingviewModelToView.Add(typeof(T1), typeof(T2));
            }
        }

        public void Show(object viewModel, bool isMaximized = false)
        {
            Type viewType = GetViewTypeForViewMode(viewModel.GetType());

            // 기존 Window가 열려있다면 닫기.
            if (_openWindows.TryGetValue(viewType, out Window existingWindow))
            {
                existingWindow.Close();
                existingWindow.DataContext = null;
                this._openWindows.Remove(viewType);

            }

            var viewInstance = Activator.CreateInstance(viewType);

            if (viewInstance is Window window)
            {
                if (!this._openWindows.ContainsKey(viewType))
                {
                    this._openWindows.Add(viewType, window);
                }
                window.DataContext = viewModel;
                if (isMaximized)
                {
                    window.WindowState = WindowState.Maximized;
                }
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                window.Show();
            }
            else
            {
                MessageBox.Show("View Instance is not a Window");
            }
        }


        public void ShowDialog(object viewModel)
        {
            Type viewType = GetViewTypeForViewMode(viewModel.GetType());
            var viewInstance = Activator.CreateInstance(viewType);

            if (viewInstance is Window window)
            {
                if (!this._openWindows.ContainsKey(viewType))
                {
                    this._openWindows.Add(viewType, window);
                }

                window.DataContext = viewModel;
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                window.ShowDialog();
            }
            else
            {
                MessageBox.Show("View Instance is not a Window");
            }
        }

        public void Close(object viewModel)
        {
            Type viewType = GetViewTypeForViewMode(viewModel.GetType());
            var viewInstance = this._openWindows[viewType];

            if (viewInstance is Window window)
            {
                window.Close();
                window.DataContext = null;
                this._openWindows.Remove(viewType);
            }
            else
            {
                MessageBox.Show("View Instance is not a Window");
            }
        }

        public void Hide(object viewModel)
        {
            Type viewType = GetViewTypeForViewMode(viewModel.GetType());
            if (this._openWindows.TryGetValue(viewType, out Window window))
            {
                window.Hide();
            }
            else
            {
                MessageBox.Show("Window not found for ViewModel");
            }
        }

        private Type GetViewTypeForViewMode(Type viewModel)
        {
            if (this._mappingviewModelToView.TryGetValue(viewModel, out Type viewType))
            {
                return viewType;
            }
            MessageBox.Show("No view found for " + viewModel.FullName);
            return null;
        }
    }
}
