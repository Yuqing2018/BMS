using BackEndMS.Helpers;
using BackEndMS.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace BackEndMS
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PersonalCenter : Page
    {
        public UserInfo Personalinfo { get; set; }
        public PersonalCenter()
        {
            this.InitializeComponent();
            if(null == Personalinfo)
                Personalinfo = new UserInfo();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Personalinfo = new UserInfo((e.Parameter as UserInfo));
            base.OnNavigatedTo(e);
        }
        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            var obj = VisualTreeHelper.GetParent((sender as UIElement));
            var editBox = VisualTreeHelper.GetChild(obj, 0) as Control;
            editBox.IsEnabled = true;
            if (SubmitBtn.IsEnabled == false)
                SubmitBtn.IsEnabled = true;
            if (CancelBtn.IsEnabled == false)
                CancelBtn.IsEnabled = true;
        }
        private async void Submit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Windows.UI.Color red = Windows.UI.Color.FromArgb(255, 255, 0, 0);
                bool isValid = true;
                if (String.IsNullOrEmpty(usernameTb.Text))
                {
                    isValid = false;
                    usernameTb.BorderBrush = new SolidColorBrush(red);
                }
                if (String.IsNullOrEmpty(NicknameTb.Text))
                {
                    isValid = false;
                    NicknameTb.BorderBrush = new SolidColorBrush(red);
                }
                if (confirmpasswordpb.Visibility == Visibility.Visible && confirmpasswordpb.Password != passwordpb.Password)
                {
                    isValid = false;
                    confirmpasswordpb.BorderBrush = new SolidColorBrush(red);
                }
                var emailAddress = EmailTb.Text.Trim().ToLower();

                if (string.IsNullOrWhiteSpace(emailAddress) || !Regex.IsMatch(emailAddress, ConstStrings.EmailAddressRegex))
                {
                    isValid = false;
                    confirmpasswordpb.BorderBrush = new SolidColorBrush(red);
                }
                if (isValid)
                {
                    string jsonResult = await LinqHelper.UpdateData("User", Personalinfo);
                    var result = JsonConvert.DeserializeObject<JsonChangedTemplate>(jsonResult);
                    await MainPage.ShowMessage(result.ResultCode);
                    MainPage.Current.Userinfo = Personalinfo;
                    this.Frame.Navigate(typeof(PersonalCenter), Personalinfo);
                }
            }
            catch (Exception ex)
            {
                MainPage.ShowErrorMessage(ex.Message);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Personalinfo = MainPage.Current.Userinfo;
            this.Frame.Navigate(typeof(PersonalCenter),Personalinfo);
        }

        private void GoBackBtn_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(MainPage.Current.CurrentScenario.ClassType, MainPage.Current.CurrentScenario.Tabs);
        }
    }
}
