using KioskAppServer.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace KioskAppServer
{
    /// <summary>
    /// CategoryEditDialog.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CategoryEditDialog : Window
    {
        public CategoryEditDialog()
        {
            InitializeComponent();
            Loaded += (obj, handler) =>
            {
                if (DataContext is CategoryEditDialogViewModel viewModel)
                {
                    viewModel.CloseRequested += () => { DialogResult = true; Close(); };
                }
            };
        }
    }
}
