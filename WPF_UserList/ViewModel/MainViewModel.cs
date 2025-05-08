using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using WPF_UserList.Model;
using WPF_UserList.Service;

namespace WPF_UserList.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly Services _userService = new Services();
        private ObservableCollection<User> _users;
        private ICollectionView _usersView;
        private string _selectedCity;
        private ObservableCollection<string> _cities;

        public ICommand ClearCommand { get; }

        public ObservableCollection<User> Users
        {
            get => _users;
            set
            {
                _users = value;
                OnPropertyChanged();
            }
        }

        public ICollectionView UsersView
        {
            get => _usersView;
            set
            {
                _usersView = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> Cities
        {
            get => _cities;
            set
            {
                _cities = value;
                OnPropertyChanged();
            }
        }

        public string SelectedCity
        {
            get => _selectedCity;
            set
            {
                _selectedCity = value;
                OnPropertyChanged();
                UsersView.Refresh();
            }
        }

        public MainViewModel()
        {
            ClearCommand = new RelayCommand(Clear);
            LoadData();
        }

        private void Clear(object obj)
        {
            SelectedCity = null;
        }

        private async void LoadData()
        {
            var usersList = await _userService.GetUsersAsync();
            Users = new ObservableCollection<User>(usersList);
            Cities = new ObservableCollection<string>(usersList.Select(u => u.address.city).Distinct());

            UsersView = CollectionViewSource.GetDefaultView(Users);
            UsersView.Filter = FilterByCity;
        }

        private bool FilterByCity(object obj)
        {
            if (obj is User user)
            {
                return string.IsNullOrEmpty(SelectedCity) || user.address.city == SelectedCity;
            }
            return false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public class RelayCommand : ICommand
        {
            private readonly Action<object> _execute;
            private readonly Predicate<object> _canExecute;

            public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
            {
                _execute = execute ?? throw new ArgumentNullException(nameof(execute));
                _canExecute = canExecute;
            }

            public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);

            public void Execute(object parameter) => _execute(parameter);

            public event EventHandler CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }
        }
    }
}
