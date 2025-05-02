using QuizApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizApp.Services
{
    public class NavigationService
    {
        // Przechowuje sposób w jaki ma zostać wykonana nawigacja,
        // np. jak aplikacja ma zareagować na zmianę widoku
        // ViewModelBase jest klasą bazową (implementuje INotifyPropertyChanged)
        // dla klas modelów widoku
        private Action<ViewModelBase> _navigate;

        // Ta metoda pozwala zarejestrować metodę, która faktycznie
        // obsługuje przełączanie widoków
        public void SetNavigator(Action<ViewModelBase> navigator)
        {
            _navigate = navigator;
        }

        // wywołuje metodę _navigate, która została wcześniej zarejestrowana
        // metodzie przekazywany jest model widoku do ustawienia
        public void NavigateTo(ViewModelBase viewModel)
        {
            _navigate?.Invoke(viewModel);
        }
    }
}
