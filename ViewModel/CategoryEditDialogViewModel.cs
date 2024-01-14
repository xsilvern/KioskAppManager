using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KioskAppServer.ViewModel
{
    public class CategoryEditDialogViewModel:ViewModelBase
    {
        public string? CategoryName { get; set; }
        public event Action? CloseRequested;
        public ICommand? ConfirmCommand { get; }
        public CategoryEditDialogViewModel() {
            ConfirmCommand = new RelayCommand(ConfirmSelection);
        }
        private void ConfirmSelection()
        {
            CloseRequested?.Invoke();
        }
    }
}
