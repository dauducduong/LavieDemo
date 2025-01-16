using Microsoft.Win32;
using System.Windows;

namespace LavieDemo
{
    /// <summary>
    /// Interaction logic for SettingWindow.xaml
    /// </summary>
    public partial class SettingWindow : Window
    {
        public SettingWindow()
        {
            InitializeComponent();
        }

        private void ChooseFolderButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ThemeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }

        private void save_folder_picker_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void apply_button_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Confirm to apply these setting?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                // Apply...
                this.Close();
            }
        }

        private void cancel_button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
