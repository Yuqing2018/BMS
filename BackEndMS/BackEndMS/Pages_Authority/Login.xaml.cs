using BackEndMS.Controls;
using BackEndMS.Helpers;
using BackEndMS.Models;
using Newtonsoft.Json;
using System;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace BackEndMS
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Login : Page
    {
        public Login()
        {
            this.InitializeComponent();
        }
        
        private async void LoginControl_LoginClicked(object sender, ItemClickEventArgs e)
        {
            ActiveProgressRing();
            try
            {
                string username = this.LoginControl.Userinfo.Username;
                string password = this.LoginControl.Userinfo.Password;
                if (!String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(password))
                {
                    string jsonResult = await LinqHelper.login(username, password);
                    var result = JsonConvert.DeserializeObject<JsonLoginTemplate>(jsonResult);
                    if (!String.IsNullOrEmpty(result.Message))
                    {
                        this.LoginControl.ShowErrorMessage = true;
                        this.LoginControl.ErrorMessage = "UserName or Password is invalid, Please try again.";
                    }
                    else
                    {
                        LinqHelper.token = result.Data.Token;
                        UserInfo tempUser = result.Data.Claims;
                        InActiveProgressRing();
                        Frame.Navigate(typeof(MainPage), tempUser);
                    }
                }
                else
                {
                    this.LoginControl.ShowErrorMessage = true;
                    this.LoginControl.ErrorMessage = "UserName or Password is Required.";
                }
            }
            catch (Exception ex)
            {
                await MainPage.ShowErrorMessage(ex.Message);
            }
            finally
            {
                InActiveProgressRing();
            }
        }
        private void ActiveProgressRing()
        {
            progressRing.IsActive = true;
            this.LoginControl.IsEnabled = false;
        }
        private void InActiveProgressRing()
        {
            progressRing.IsActive = false;
            this.LoginControl.IsEnabled = true;
        }
        SignInControl signinControl;
        private void LoginControl_SigninClicked(object sender, ItemClickEventArgs e)
        {
            try
            {
                signinControl = new SignInControl(this.LoginControl.Userinfo, "用户注册");
                signinControl.Show();
                signinControl.AddRecordClick += SigninControl_AddRecordClick;
            }
            catch (Exception ex)
            {
                MainPage.ShowErrorMessage(ex.Message);
            }
        }

        private async void SigninControl_AddRecordClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                string jsonResult = await LinqHelper.Signin(JsonConvert.SerializeObject(signinControl.Userinfo));
                var result = JsonConvert.DeserializeObject<JsonChangedTemplate>(jsonResult);
                if (result != null)
                {
                    await MainPage.ShowMessage(result.ResultCode);
                }
                else
                {
                    this.LoginControl.Userinfo = new UserInfo();
                }
                signinControl.Dispose();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private void settingSysBtn_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage));
        }
    }
}
