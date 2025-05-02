using QuizApp.ViewModels;
using System.Configuration;
using System.Data;
using System.Windows;

namespace QuizApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // utworzenie obiektu odpowiedzialnego za nawigację
            var navigationService = new QuizApp.Services.NavigationService();

            // utworzenie modelu widoku dla widoku startowego
            var homeViewModel = new SolvingViewModel();
            //navigationService.NavigateTo<HomeViewModel>();
            navigationService.NavigateTo(homeViewModel);

            var mainWindow = new MainWindow
            {
                DataContext = new MainViewModel(navigationService)
            };

            mainWindow.Show();
        }
    }

}
