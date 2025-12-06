
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WpfDIShowViews.Services;
using WpfDIShowViews.ViewModels;
using WpfDIShowViews.Views;

namespace WpfDIShowViews
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    private IServiceProvider _services = default!;

    private IServiceProvider ConfigurationService()
    {
      IServiceCollection services = new ServiceCollection();

      // Services
      services.AddSingleton<IDialogService, DialogService>();
      services.AddSingleton<IPopupManager, PopupManager>();

      // ViewModels
      services.AddSingleton<MainViewModel>();
      services.AddTransient<WpfDIShowViews.ViewModels.Left.SubViewModel1>();
      services.AddTransient<WpfDIShowViews.ViewModels.Left.SubViewModel2>();
      services.AddTransient<WpfDIShowViews.ViewModels.Right.SubViewModel3>();
      services.AddTransient<WpfDIShowViews.ViewModels.Right.SubViewModel4>();

      return services.BuildServiceProvider();
    }

    public App()
    {
      _services = ConfigurationService();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
      base.OnStartup(e);

      var dialogService = _services.GetRequiredService<IDialogService>();

      // Register Mappings
      dialogService.Register<MainViewModel, MainView>();
      dialogService.Register<WpfDIShowViews.ViewModels.Left.SubViewModel1, WpfDIShowViews.Views.Left.SubView1>();
      dialogService.Register<WpfDIShowViews.ViewModels.Left.SubViewModel2, WpfDIShowViews.Views.Left.SubView2>();
      dialogService.Register<WpfDIShowViews.ViewModels.Right.SubViewModel3, WpfDIShowViews.Views.Right.SubView3>();
      dialogService.Register<WpfDIShowViews.ViewModels.Right.SubViewModel4, WpfDIShowViews.Views.Right.SubView4>();

      var mainViewModel = _services.GetRequiredService<MainViewModel>();
      dialogService.Show(mainViewModel);
    }
  }
}
