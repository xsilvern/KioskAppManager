using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KioskAppServer.ViewModel
{
    class MenuSelectionViewModel:ViewModelBase
    {
        public ObservableCollection<Menu> MenuList { get; set; }

        private Menu _selectedMenu;
        public Menu SelectedMenu
        {
            get { return _selectedMenu; }
            set { _selectedMenu = value; }
        }
        public event Action CloseRequested;
        public ICommand ConfirmCommand { get; }
        public MenuSelectionViewModel(ObservableCollection<Menu> menus)
        {
            MenuList = menus;
            ConfirmCommand = new RelayCommand(ConfirmSelection);
        }

        // 확인 버튼 클릭시 수행
        private void ConfirmSelection()
        {
            CloseRequested?.Invoke();
        }
    }
}
