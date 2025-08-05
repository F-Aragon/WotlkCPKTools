using System;

using WotlkCPKTools.Core;

namespace WotlkCPKTools.MVVM.ViewModel
{
    class MainViewModel : ObservableObject
    {




        public RelayCommand AddonsViewCommand { get; set; }
        public RelayCommand BackUpViewCommand { get; set; }
        public RelayCommand ExtrasViewCommand { get; set; }

        public AddonsViewModel AddonsVM { get; set; }
        public BackUpViewModel BackUpVM { get; set; }
        public ExtrasViewModel ExtrasVM { get; set; }

        private object _currentView;

        public object CurrentView
        {
            get { return _currentView; }
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }

        }

        public MainViewModel()
        {
            AddonsVM = new AddonsViewModel();
            BackUpVM = new BackUpViewModel();
            ExtrasVM = new ExtrasViewModel();

            CurrentView = AddonsVM;

            AddonsViewCommand = new RelayCommand(o =>
            {
                CurrentView = AddonsVM;
            });

            BackUpViewCommand = new RelayCommand(o =>
            {
                CurrentView = BackUpVM;
            });

            ExtrasViewCommand = new RelayCommand(o =>
            {
                CurrentView = ExtrasVM;
            });
        }

    }
}
