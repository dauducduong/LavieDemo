using System.Windows;
using System.Windows.Controls;


namespace LavieDemo
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public bool IsAuthenticated { get; private set; }
        public LoginWindow()
        {
            InitializeComponent();
            UsernameTextBox.Focus();
        }

        private void button_login_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            if (username == "admin" && password == "admin")
            {
                IsAuthenticated = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Username or password is not correct", "Login In Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
