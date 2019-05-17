using BackEndMS.Helpers;
using BackEndMS.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace BackEndMS.Controls
{
    public sealed partial class SignInControl : UserControl, INotifyPropertyChanged
    {
        public event ItemClickEventHandler AddRecordClick = delegate { };
        public UserInfo Userinfo { get; set; }
        private List<UserInfo> userList;
      //  public bool ShowErrorMessage { get; set; } = false;
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        private bool _ShowErrorMessage = false;
        private string _ErrorMessage;
        public bool ShowErrorMessage
        {
            get
            {
                return _ShowErrorMessage;
            }
            set
            {
                if (null != PropertyChanged && _ShowErrorMessage != value)
                {
                    _ShowErrorMessage = value;
                    NotifyPropertyChange("ShowErrorMessage");
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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public List<RoleType> RoleList
        {
            get
            {
                return Enum.GetValues(typeof(RoleType)).Cast<RoleType>().ToList();
            }
        }
        public SignInControl()
        {
            InitializeComponent();
            if (null == Userinfo)
                Userinfo = new UserInfo() { Role = new List<RoleType>() { RoleType.User} };
            validateSignInInfo();
        }
        public SignInControl(UserInfo user,string caption="")
        {
            InitializeComponent();
            if (null == user)
                user = new UserInfo() { Role = new List<RoleType>() { RoleType.User } };
            else
            {
                Userinfo = user;
                if (Userinfo.Role == null || 0 == Userinfo.Role.Count)
                    Userinfo.Role.Add(RoleType.User);
                if (!String.IsNullOrEmpty(caption))
                    captionTb.Text = caption;
                else if (!String.IsNullOrEmpty(user.Id))
                {
                    captionTb.Text = "用户信息修改";
                    confirmpasswordpb.Password = user.Password;
                    roleGridview.IsEnabled = true;
                }
            }
        }
        Popup popup;
        public async void Show()
        {
            var appView = ApplicationView.GetForCurrentView();

            popup = new Popup();
            //popup.HorizontalOffset = appView.VisibleBounds.Width;
            //popup.VerticalOffset = appView.VisibleBounds.Height;
            popup.Child = this;
            this.Height = appView.VisibleBounds.Height;
            this.Width = appView.VisibleBounds.Width;
            this.contentGrid.Width = this.Width / 2;
            this.contentGrid.Height = this.Height / 2;
            EventHandler<Windows.UI.Core.BackRequestedEventArgs> PageNavHelper_BeforeBackRequest = (s, e) =>
            {
                if (popup.IsOpen)
                {
                    e.Handled = true;
                    popup.IsOpen = false;
                }
            };

            TypedEventHandler<ApplicationView, object> handler = (s, e) =>
            {
                try
                {
                    if (popup.IsOpen)
                    {
                        //popup.HorizontalOffset = appView.VisibleBounds.Width;
                        //popup.VerticalOffset = appView.VisibleBounds.Height;
                        this.Height = appView.VisibleBounds.Height;
                        this.Width = appView.VisibleBounds.Width;
                        this.contentGrid.Width = this.Width / 2;
                        this.contentGrid.Height = this.Height / 2;
                    }
                }
                catch (Exception ex)
                {
                    MainPage.ShowErrorMessage(ex.Message);
                    return;
                }
            };

            //TappedEventHandler tapped = (s, e) =>
            //{
            //    if (popup.IsOpen)
            //    {
            //            //popup.IsOpen = false;
            //    }
            //};
            popup.Opened += (s, e) =>
            {
                this.Visibility = Visibility.Visible;
                appView.VisibleBoundsChanged += handler;
            };

            popup.Closed += (s, e) =>
            {
                this.Visibility = Visibility.Collapsed;
                appView.VisibleBoundsChanged -= handler;
            };

            popup.IsOpen = true;
        }
        public void Dispose()
        {
            if (popup.IsOpen)
                popup.IsOpen = false;
        }
        private void closeBtn_Click(object sender, RoutedEventArgs e)
        {
            popup.IsOpen = false;
        }

        private async  void submitBtn_Click(object sender, RoutedEventArgs e)
        {
            Windows.UI.Color red = Windows.UI.Color.FromArgb(255,255, 0, 0);
            bool isValid = true;
            if (String.IsNullOrEmpty(usernameTb.Text))
            {
                isValid = false;
                ShowErrorMessage = true;
                ErrorMessage = "Username is required";
                // usernameTb.BorderBrush = new SolidColorBrush(red);
            }
           else if (String.IsNullOrEmpty(nicknameTb.Text))
            {
                isValid = false;
                ShowErrorMessage = true;
                ErrorMessage = "Nickname is required";
                // nicknameTb.BorderBrush = new SolidColorBrush(red);
            }
            else if (null != userList)
            {

                if (userList.Select(x => x.Username).Contains(Userinfo.Username))
                {
                    isValid = false;
                    ShowErrorMessage = true;
                    ErrorMessage = "Username is Uniqued";
                }
                else if (userList.Select(x => x.Nickname).Contains(Userinfo.Nickname))
                {
                    isValid = false;
                    ShowErrorMessage = true;
                    ErrorMessage = "Nickname is Uniqued";
                }
            }
            else if (String.IsNullOrEmpty(passwordpb.Password))
            {
                isValid = false;
                ShowErrorMessage = true;
                ErrorMessage = "Password is required";
                //passwordpb.BorderBrush = new SolidColorBrush(red);
            }
            else if (confirmpasswordpb.Password != passwordpb.Password)
            {
                isValid = false;
                ShowErrorMessage = true;
                ErrorMessage = "confirmPassword is incorrect";
                // confirmpasswordpb.BorderBrush =new SolidColorBrush(red);
            }
            else if (String.IsNullOrEmpty(STC_UserNameTB.Text))
            {
                isValid = false;
                ShowErrorMessage = true;
                ErrorMessage = "STC_Username is required";
                //passwordpb.BorderBrush = new SolidColorBrush(red);
            }
            else if (String.IsNullOrEmpty(STC_PasswordPB.Password))
            {
                isValid = false;
                ShowErrorMessage = true;
                ErrorMessage = "STC_Password is required";
                // confirmpasswordpb.BorderBrush =new SolidColorBrush(red);
            }
            else
            {
                var emailAddress = EmailTb.Text.Trim().ToLower();

                if (string.IsNullOrWhiteSpace(emailAddress))
                {
                    isValid = false;
                    ShowErrorMessage = true;
                    ErrorMessage = "EmailAddress is required";
                    //EmailTb.BorderBrush = new SolidColorBrush(red);
                }else if(!Regex.IsMatch(emailAddress, ConstStrings.EmailAddressRegex))
                {
                    isValid = false;
                    ShowErrorMessage = true;
                    ErrorMessage = "EmailAddress format is incorrect";
                }
            }
            if (isValid)
            {
                ShowErrorMessage = false;
                if (Userinfo.Role.Count == 0)
                    Userinfo.Role.Add(RoleType.User);
                AddRecordClick?.Invoke(sender, null);
            }
        }

        private async void validateSignInInfo()
        {
            var jsonResult = await LinqHelper.GetAllQueryData("User");
            var result =JsonConvert.DeserializeObject<JsonResultTemplate<UserInfo>>(jsonResult);
            userList = result.Data.ToList();
           // string resultCode = jsonObject.GetNamedValue("ResultCode").ToString();
            //if (result.ResultCode == (int)ResultCodeType.操作成功)
            //{
            //    JsonArray jsonArray = jsonObject.GetNamedArray("Data");
            //    userList = jsonArray.Select(x => JsonConvert.DeserializeObject<UserInfo>(x.ToString())).ToList();
            //}
        }
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var item = (sender as CheckBox).Content;
            var ob = (RoleType)Enum.Parse(typeof(RoleType), item.ToString());
            if (Userinfo.Role !=null && !Userinfo.Role.Contains(ob))
                Userinfo.Role.Add(ob);
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            var item = (sender as CheckBox).Content;
            var ob = (RoleType)Enum.Parse(typeof(RoleType), item.ToString());
            if (Userinfo.Role != null && Userinfo.Role.Contains(ob))
                Userinfo.Role.Remove(ob);
        }

        private void CheckBox_Loaded(object sender, RoutedEventArgs e)
        {
            var item = (sender as CheckBox).Content;
            var ob = (RoleType)Enum.Parse(typeof(RoleType), item.ToString());
            if (Userinfo.Role != null && Userinfo.Role.Contains(ob))
                (sender as CheckBox).IsChecked = true;
        }
    }
}
