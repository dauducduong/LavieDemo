﻿using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using Lavie;
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
            //Start timer update data
            _timer.Start();
        }
        private void Timer_Tick(object? sender, EventArgs e)
        {
            //Đọc dữ liệu counter
            _totalCount = int.Parse(plc.ReadData("DB1.DBW2")); //Đọc biến total_count trên DB1
            _okCount = int.Parse(plc.ReadData("DB1.DBW4")); //Đọc biến ok_count trên DB1
            double okCountPercent = Math.Round(((double)(_okCount/_totalCount) * 100),2);
            _ngCount = _totalCount - _okCount;
            totalCountLabel.Text = _totalCount.ToString();
            okCountLabel.Text = _okCount.ToString() + " (" + okCountPercent.ToString() + "%)";
            ngCountLabel.Text = _ngCount.ToString() + " (" + (100-okCountPercent).ToString() + "%)";
            //Đọc trạng thái start/stop
            bool new_app_status = false;
            if (plc.ReadData("DB1.DBX0.1") == "True")
            {
                new_app_status = true;
            }
            if (new_app_status != _appStatus)
            {
                if (_appStatus == true)
                {
                    _appStatus = false;
                    status_app.Fill = new SolidColorBrush(Colors.Red);
                    button_stop.IsEnabled = false;
                    button_start.IsEnabled = true;
                    button_reset.IsEnabled = true;
                }
                else
                {
                    _appStatus = true;
                    status_app.Fill = new SolidColorBrush(Colors.LimeGreen);
                    button_stop.IsEnabled = true;
                    button_start.IsEnabled = false;
                    button_reset.IsEnabled = false;
                }
            }
            //Đọc trạng thái cam
            bool new_cam_status = false;
            if (plc.ReadData("DB1.DBX0.0") == "True")
            {
                new_cam_status = true;
            }
            if (new_cam_status != _camStatus)
            {
                if (_camStatus == true)
                {
                    _camStatus = false;
                    status_cam.Fill = new SolidColorBrush(Colors.Red);
                }
                else
                {
                    _camStatus = true;
                    status_cam.Fill = new SolidColorBrush(Colors.LimeGreen);
                }
            }
        }

        private void InitPLC()
        {
            // Khởi tạo PLC (CPU loại S7-1200, địa chỉ IP là 192.168.1.11, Rack 0, Slot 1)
            try
            {
                plc.Connect("192.168.1.11");
                if (plc.Connected)
                {
                    _plcStatus = true;
                    status_plc.Fill = new SolidColorBrush(Colors.LimeGreen); //cập nhật status của plc

                    
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
                }
                else
                {
                    MessageBox.Show("Cannot read camera status from PLC. Check again!", "Camera Status Reading Error", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private void button_setting_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.ShowDialog();
            if (loginWindow.IsAuthenticated)
            {
                SettingWindow settingWindow = new SettingWindow();
                settingWindow.ShowDialog();
            }
        }

        private void refresh_cam_status_Click(object sender, RoutedEventArgs e)
        {
            if (plc.ReadData("DB1.DBX0.0") == "True")
            {
                _camStatus = true;
                status_cam.Fill = new SolidColorBrush(Colors.LimeGreen);
            }
            else
            {
                _camStatus = false;
                status_cam.Fill = new SolidColorBrush(Colors.Red);
            }
        }
    }
}