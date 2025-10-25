using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using OMS.Models;

namespace OMS.ViewModels;

public class LoginViewModel : INotifyPropertyChanged
{
    private bool _isAdminSelected = true;
    private bool _isMakerSelected;
    private string _selectedMaker = "Rajesh Kumar";

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<string> Makers { get; } = new()
    {
        "Rajesh Kumar",
        "Amit Patel",
        "Suresh Singh"
    };

    public bool IsAdminSelected
    {
        get => _isAdminSelected;
        set
        {
            if (_isAdminSelected != value)
            {
                _isAdminSelected = value;
                OnPropertyChanged();
                if (value)
                {
                    IsMakerSelected = false;
                }
            }
        }
    }

    public bool IsMakerSelected
    {
        get => _isMakerSelected;
        set
        {
            if (_isMakerSelected != value)
            {
                _isMakerSelected = value;
                OnPropertyChanged();
                if (value)
                {
                    IsAdminSelected = false;
                }
            }
        }
    }

    public string SelectedMaker
    {
        get => _selectedMaker;
        set
        {
            if (_selectedMaker != value)
            {
                _selectedMaker = value;
                OnPropertyChanged();
            }
        }
    }

    public ICommand SelectRoleCommand => new Command<string>(SelectRole);
    public ICommand LoginCommand => new Command(async () => await Login());

    private void SelectRole(string role)
    {
        if (role == "admin")
        {
            IsAdminSelected = true;
        }
        else if (role == "maker")
        {
            IsMakerSelected = true;
        }
    }

    private async Task Login()
    {
        var user = new User(
            Id: IsAdminSelected ? "admin-1" : GetMakerId(SelectedMaker),
            Name: IsAdminSelected ? "Admin User" : SelectedMaker,
            Role: IsAdminSelected ? UserRole.Admin : UserRole.Maker
        );

        // Store current user
        App.CurrentUser = user;

        // Navigate to main shell
        await Shell.Current.GoToAsync("//Dashboard");
    }

    private string GetMakerId(string makerName)
    {
        return makerName switch
        {
            "Rajesh Kumar" => "maker-1",
            "Amit Patel" => "maker-2",
            "Suresh Singh" => "maker-3",
            _ => "maker-1"
        };
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
