using QuizApp.ViewModels;
using System.Windows.Input;

public class AnswerViewModel : ViewModelBase
{
    private string _text = "";
    private bool _isCorrect;
    private bool _isSelected;
    private bool _isSelectable = true; // Dodajemy właściwość IsSelectable

    public AnswerViewModel(string text, bool isCorrect)
    {
        _text = text;
        _isCorrect = isCorrect;
    }

    public AnswerViewModel() { }
    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            OnPropertyChanged(nameof(Text));
            CommandManager.InvalidateRequerySuggested(); // Odśwież stan komend
        }
    }

    public bool IsCorrect
    {
        get => _isCorrect;
        set
        {
            _isCorrect = value;
            OnPropertyChanged(nameof(IsCorrect));
        }
    }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            OnPropertyChanged(nameof(IsSelected));
        }
    }

    public bool IsSelectable // Dodajemy flagę, która będzie kontrolować możliwość zaznaczania odpowiedzi
    {
        get => _isSelectable;
        set
        {
            _isSelectable = value;
            OnPropertyChanged(nameof(IsSelectable));
        }
    }

    // Metoda, która wywoła się po zakończeniu quizu i zablokuje możliwość zmiany odpowiedzi
    public void LockAnswer()
    {
        IsSelectable = false;
    }
}
