using KioskAppServer.Service;
using KioskAppServer.ViewModel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KioskAppServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<string> ComboBoxOptionList { get; } = new List<string>()
        { 
            "카테고리",
            "메뉴"
        };
        
        public MainWindow()
        {
            InitializeComponent();
            GoogleDriveDataService service = new GoogleDriveDataService();
            DataContext = new MainViewModel(service);
            //client.BaseAddress = new Uri("http://localhost:9000");
            //client.DefaultRequestHeaders.Accept.Add(
            //    new MediaTypeWithQualityHeaderValue("application/json")
            //    );
            
            AddOptionComboBox.ItemsSource = ComboBoxOptionList;
        }

    }
}
