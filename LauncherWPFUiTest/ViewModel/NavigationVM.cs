using LauncherWPFUiTest.Utilities;
using LauncherWPFUiTest.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LauncherWPFUiTest.ViewModel
{
    class NavigationVM : Utilities.ViewModelBase
    {


        private object _currentView;
        public object CurrentView
        {
            get { return _currentView; }
            set
            {
                _currentView =  value;
                OnPropertyChanged();
            }
        }

        public ICommand HomeCommand { get; set; }
        private void Home(object obj) => CurrentView = new HomeVM();
        public NavigationVM()
        {
            HomeCommand = new RelayCommand(Home);
            //start Page
            CurrentView = new HomeVM();
        }
    }
}
