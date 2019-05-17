using BackEndMS.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace BackEndMS.Controls
{
    public sealed partial class LoginControl : UserControl, INotifyPropertyChanged
    {

        public UserInfo Userinfo { get; set; }
        private bool _ShowErrorMessage = false;
        private string _ErrorMessage;
        private bool _isRememberMe = false;
        public event ItemClickEventHandler LoginClicked = delegate { };
        public event ItemClickEventHandler SigninClicked = delegate { };
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public bool ShowErrorMessage {
            get {
                return _ShowErrorMessage;
            } set {
                if(null !=PropertyChanged && _ShowErrorMessage != value)
                {
                    _ShowErrorMessage = value;
                    NotifyPropertyChange("ShowErrorMessage");
                }
            }
        }
        public bool IsRememberMe {
            get
            {
                return _isRememberMe;
            }
            set
            {
                _isRememberMe = value;
                if (null != PropertyChanged)
                {
                    NotifyPropertyChange("IsRememberMe");
                }
            }
        }
        public string ErrorMessage
        {
            get { return _ErrorMessage; }
            set
            {
                if (null != PropertyChanged && _ErrorMessage != value)
                {
                    _ErrorMessage = value;
                    NotifyPropertyChange("ErrorMessage");
                }
            }
        }
        private void NotifyPropertyChange([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public LoginControl()
        {
            this.InitializeComponent();
            Userinfo = new UserInfo();
            if (!string.IsNullOrEmpty(App.RootUserName) && !string.IsNullOrEmpty(App.RootPassword))
            {
                Userinfo.Username = App.RootUserName;
                Userinfo.Password = App.RootPassword;
                IsRememberMe = true;
              //  this.RememberMe.IsChecked = true;
            }
        }

        private void logonBtn_Click(object sender, RoutedEventArgs e)
        {
            if (IsRememberMe)
            {
                App.RootUserName = Userinfo.Username;
                App.RootPassword = Userinfo.Password;
            }
            else
            {
                App.RootUserName = null;
                App.RootPassword = null;
            }
            LoginClicked?.Invoke(sender,null);
        }

        private void usernameTb_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Userinfo.Username) && !string.IsNullOrEmpty(Userinfo.Password))
                ShowErrorMessage = false;
            if (e.OriginalKey == VirtualKey.Enter && e.KeyStatus.RepeatCount == 1)
            {
                if (IsRememberMe)
                {
                    App.RootUserName = Userinfo.Username;
                    App.RootPassword = Userinfo.Password;
                }
                else
                {
                    App.RootUserName = null;
                    App.RootPassword = null;
                }
                LoginClicked?.Invoke(sender, null);
            }
        }

        private void signinBtn_Click(object sender, RoutedEventArgs e)
        {
            SigninClicked?.Invoke(sender,null);
        }
    }
}
