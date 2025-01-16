using System.Net;
using System.Security.AccessControl;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Lavie;
using S7.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;

namespace LavieDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private bool _camStatus = false;
        private bool _plcStatus = false;
        private PlcCom plc = new PlcCom();
        private bool _appStatus = false;
        private int _totalCount = 0;
        private int _okCount = 0;
        private int _ngCount = 0;
        private DispatcherTimer _timer;
        private DispatcherTimer _timer2;
        public MainWindow()
        {
            InitializeComponent();
            InitPLC();
            InitCamView();
            InitTimer();
        }
        private void InitTimer()
        {
            // Tạo timer
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMicroseconds(200); // Timer cập nhật counter
            _timer.Tick += Timer_Tick; // Gắn sự kiện Tick
        }
        private void Timer_Tick(object? sender, EventArgs e)
        {
            Console.WriteLine("Timer1 started");
            _totalCount = int.Parse(plc.ReadData("DB1.DBW2")); //Đọc biến total_count trên DB1
            _okCount = int.Parse(plc.ReadData("DB1.DBW4")); //Đọc biến ok_count trên DB1
            _ngCount = _totalCount - _okCount;
            totalCountLabel.Text = _totalCount.ToString(); 
            okCountLabel.Text = _okCount.ToString();
            ngCountLabel.Text = _ngCount.ToString();
        }
        
        private void InitPLC()
        {
            // Khởi tạo PLC (CPU loại S7-1200, địa chỉ IP là 192.168.0.1, Rack 0, Slot 1)
            try
            {
                plc.Connect("192.168.1.11");
                if (plc.Connected)
                {
                    _plcStatus = true;
                    status_plc.Fill = new SolidColorBrush(Colors.LimeGreen); //cập nhật status của plc
                    //Đọc dữ liệu từ PLC
                    _totalCount = int.Parse(plc.ReadData("DB1.DBW2")); //Đọc biến total_count trên DB1
                    _okCount = int.Parse(plc.ReadData("DB1.DBW4")); //Đọc biến ok_count trên DB1
                    _ngCount = _totalCount - _okCount;
                    totalCountLabel.Text = _totalCount.ToString();
                    okCountLabel.Text = _okCount.ToString();
                    ngCountLabel.Text = _ngCount.ToString();
                    //Đọc trạng thái start/stop
                    if (plc.ReadData("DB1.DBX0.1") == "True")
                    {
                        _appStatus = true;
                        status_app.Fill = new SolidColorBrush(Colors.LimeGreen);
                        button_stop.IsEnabled = true;
                        button_start.IsEnabled = false;
                        button_reset.IsEnabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }
        
        private void InitCamView()
        {
            webView2.Source = new Uri("http://192.168.1.36:8087/");
            webView2.NavigationCompleted += WebView_NavigationCompleted;
        }
        private void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                if (plc.Connected)
                {
                    if (plc.ReadData("DB1.DBX0.0") == "True")
                    {
                        _camStatus = true;
                        status_cam.Fill = new SolidColorBrush(Colors.LimeGreen);
                    }
                    else
                    {
                        MessageBox.Show("Cannot read camera status from PLC. Check again!", "Camera Status Reading Error", MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                }
            }
        }

        private void button_exit_Click(object sender, RoutedEventArgs e)
        {
            // Hiển thị hộp thoại xác nhận
            var result = MessageBox.Show("Do you want to quit?", "Exit Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                plc.Disconnect();
                // Đóng ứng dụng
                Application.Current.Shutdown();
            }
        }

        private void button_start_Click(object sender, RoutedEventArgs e)
        {
            if (_plcStatus && _camStatus)
            {
                plc.WriteData("DB1.DBX0.1", true);
                _timer.Start();
                _appStatus = true;
                status_app.Fill = new SolidColorBrush(Colors.LimeGreen);
                button_stop.IsEnabled = true;
                button_start.IsEnabled = false;
                button_reset.IsEnabled = false;
            }
            else
            {
                MessageBox.Show("Camera or PLC is not online. Try restarting the application!", "Communication Error", MessageBoxButton.OK);
            }
        }

        private void button_stop_Click(object sender, RoutedEventArgs e)
        {
            plc.WriteData("DB1.DBX0.1", false);
            _timer.Stop(); 
            _appStatus = false;
            status_app.Fill = new SolidColorBrush(Colors.Red);
            button_start.IsEnabled = true;
            button_stop.IsEnabled = false;
            button_reset.IsEnabled = true;
        }

        private void button_reset_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Do you want to reset data?", "Reset Data Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _totalCount = 0;
                _okCount = 0;
                _ngCount = 0;
                totalCountLabel.Text = _totalCount.ToString();
                okCountLabel.Text = _okCount.ToString();
                ngCountLabel.Text = _ngCount.ToString();
                //Reset bộ đếm
                plc.WriteData("DB1.DBX0.2", true);
                plc.WriteData("DB1.DBX0.2", false);
            }
        }

        private void refreshButton_Click(object sender, RoutedEventArgs e)
        {
            webView2.Reload(); // Làm mới trang hiện tại
        }
    }
}