using System.Windows.Input;
using QuizApp.Services;
using QuizApp.ViewModels.BaseClass;

namespace QuizApp.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly NavigationService _navigationService;

        private ViewModelBase _currentViewModel;
        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set { _currentViewModel = value; OnPropertyChanged("CurrentViewModel"); }
        }

        // polecenia zmiany modelu widoku
        public ICommand NavigateMakerCommand { get; }
        public ICommand NavigateSolvingCommand { get; }
        public MainViewModel(NavigationService navigationService)
        {
            _navigationService = navigationService;
            _navigationService.SetNavigator(vm => CurrentViewModel = vm);

            // tworzymy obiekty modelu widoku
            ViewModelBase mv = new MakerViewModel();
            ViewModelBase sv = new SolvingViewModel();

            // utworzenie obiektów typu RelayCommand,
            // polecenia maj¹ za zadanie zmieniæ aktualny model widoku
            // polecenia zawsze mo¿na wykonaæ
            // jeœli parametr nie jest wykorzystywany w metodzie, wówczas w funkcji
            // lambda mo¿na wpisaæ _ zamiast nazwy parametru
            NavigateMakerCommand = new RelayCommand(_ => _navigationService.NavigateTo(mv), _ => true);
            NavigateSolvingCommand = new RelayCommand(_ => _navigationService.NavigateTo(sv), _ => true);

            CurrentViewModel = mv;
        }
    }
}
