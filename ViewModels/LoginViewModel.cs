using CommunityToolkit.Mvvm.Input;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TrioDocs.Core;
using TrioDocs.Data;

namespace TrioDocs.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private string _login;
        public string Login { get => _login; set => SetProperty(ref _login, value); }

        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand<Window>(LoginExecute);
            RegisterCommand = new RelayCommand<Window>(RegisterExecute);
        }

        private string GetPasswordFromWindow(Window window)
        {
            return (window.FindName("PasswordBox") as PasswordBox)?.Password;
        }

        private void LoginExecute(Window window)
        {
            var password = GetPasswordFromWindow(window);
            if (string.IsNullOrEmpty(Login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Пожалуйста, введите логин и пароль.");
                return;
            }

            using (var db = new TrioDocsDBEntities())
            {
                var user = db.Users.FirstOrDefault(u => u.Login == Login);
                if (user != null && PasswordHelper.VerifyPassword(password, user.PasswordHash))
                {
                    window.DialogResult = true;
                    window.Close();
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль.");
                }
            }
        }

        private void RegisterExecute(Window window)
        {
            var password = GetPasswordFromWindow(window);
            if (string.IsNullOrEmpty(Login) || string.IsNullOrEmpty(password) || password.Length < 6)
            {
                MessageBox.Show("Логин не может быть пустым, а пароль должен содержать не менее 6 символов.");
                return;
            }

            using (var db = new TrioDocsDBEntities())
            {
                if (db.Users.Any(u => u.Login == Login))
                {
                    MessageBox.Show("Пользователь с таким логином уже существует!");
                    return;
                }

                var hashedPassword = PasswordHelper.HashPassword(password);
                var newUser = new User
                {
                    Login = this.Login,
                    Email = $"{this.Login}@example.com",
                    PasswordHash = hashedPassword,
                    RegistrationDate = System.DateTime.Now
                };
                db.Users.Add(newUser);
                db.SaveChanges();
                MessageBox.Show("Регистрация прошла успешно! Теперь вы можете войти.");
            }
        }
    }
}