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
using System.Windows.Threading;

namespace KioskAppServer
{
    /// <summary>
    /// LoadingWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LoadingWindow : Window
    {
        private DispatcherTimer loadingTimer;
        private int dotCount = 0;

        public LoadingWindow()
        {
            InitializeComponent();

            loadingTimer = new DispatcherTimer();
            loadingTimer.Interval = TimeSpan.FromSeconds(0.5); // 반복 간격 설정
            loadingTimer.Tick += LoadingTimer_Tick;
            loadingTimer.Start();
        }

        private void LoadingTimer_Tick(object? sender, EventArgs e)
        {
            dotCount++;
            if (dotCount > 3) dotCount = 1;

            LoadingText.Text = "Loading" + new string('.', dotCount);
        }
    }
}
