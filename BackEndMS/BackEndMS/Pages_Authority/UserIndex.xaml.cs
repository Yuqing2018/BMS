using BackEndMS.Controls;
using BackEndMS.Helpers;
using BackEndMS.Models;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System;
using System.Linq;
using Windows.Data.Json;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Collections.Generic;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace BackEndMS
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UserIndex : Page
    {
        ObservableCollection<UserInfo> UserList { get; set; }
        SignInControl signinControl;
        public UserIndex()
        {
            this.InitializeComponent();
            UserList = new ObservableCollection<UserInfo>();
            this.userTableControl.UserList = UserList;
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            init();
            
        }
        public async void init()
        {
            try { 
            MainPage.Current.ActiveProgressRing();
            UserList.Clear();
            string jsonResult = await LinqHelper.GetAllQueryData("User");
            var result = JsonConvert.DeserializeObject<JsonResultTemplate<UserInfo>>(jsonResult);
            if (result.ResultCode == (int)ResultCodeType.操作成功)
            {
                result.Data.ToList().ForEach(item => { UserList.Add(item); });
            }
            else
                await MainPage.ShowMessage(result.ResultCode);
            MainPage.Current.InActiveProgressRing();
            }
            catch (Exception ex)
            {
                await MainPage.ShowErrorMessage(ex.Message);
            }
        }
        private void AddUserBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                signinControl = new SignInControl();
                signinControl.Show();
                signinControl.AddRecordClick += userTableControl_Refresh;
            }
            catch (Exception ex)
            {
                MainPage.ShowErrorMessage(ex.Message);
            }
        }
        private void EditUserBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var user = UserList.Where(x => x.selected).FirstOrDefault();
                if (null != user)
                {
                    signinControl = new SignInControl(user);
                    signinControl.Show();
                    signinControl.AddRecordClick += userTableControl_Refresh;
                }
            }
            catch (Exception ex)
            {
                MainPage.ShowErrorMessage(ex.Message);
            }
        }
        private void userTableControl_EditClicked(object sender, ItemClickEventArgs e)
        {
            UserInfo user = (sender as Button).DataContext as UserInfo;
            if (null != user)
            {
                signinControl = new SignInControl(user);
                signinControl.Show();
                signinControl.AddRecordClick += userTableControl_Refresh;
            }
        }
        private async void userTableControl_Refresh(object sender, ItemClickEventArgs e)
        {
            try
            {
                string jsonResult = string.Empty;
                if (!String.IsNullOrEmpty(signinControl.Userinfo.Id))
                    jsonResult = await LinqHelper.UpdateData("User", signinControl.Userinfo);
                else
                    jsonResult = await LinqHelper.SaveData(JsonConvert.SerializeObject(signinControl.Userinfo), "User");

                var result = JsonConvert.DeserializeObject<JsonChangedTemplate>(jsonResult);
                if (null != result)
                {
                    if (result.ResultCode == (int)ResultCodeType.操作成功)
                    {
                        init();
                    }
                }
                signinControl.Dispose();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private async void DeleteUserBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedList = UserList.Where(x => x.selected).ToList();

                if (null != selectedList)
                {
                    ContentDialog content = new ContentDialog()
                    {
                        Title = "记录删除",
                        Content = new TextBlock
                        {
                            Text = "您确定要永久删除这些用户吗?",
                            TextWrapping = TextWrapping.Wrap,
                            Margin = new Thickness()
                            {
                                Top = 12
                            }
                        },
                        PrimaryButtonText = "确认",
                        PrimaryButtonCommandParameter = selectedList,
                        SecondaryButtonText = "取消",
                    };
                    content.PrimaryButtonClick += Content_PrimaryButtonClick;
                    await content.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                await MainPage.ShowErrorMessage(ex.Message);
            }
            finally
            {
                init();
            }
    }

        private async void Content_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            try
            {
                List<UserInfo> selectedList = sender.PrimaryButtonCommandParameter as List<UserInfo>;
                if (null != selectedList)
                {
                    foreach (var user in selectedList)
                    {
                        string jsonResult = await LinqHelper.DeleteData("User", user.Id);
                        var result = JsonConvert.DeserializeObject<JsonChangedTemplate>(jsonResult);
                        if (result.ResultCode == (int)ResultCodeType.操作成功)
                        {
                            UserList.Remove(user);
                        }
                        else
                        {
                            await MainPage.ShowMessage(result.ResultCode);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private async void EnableUserBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedList = UserList.Where(x => x.selected).ToList();
                foreach (var user in selectedList)
                {
                    if (null != user && !String.IsNullOrEmpty(user.Id))
                    {
                        user.Enable = !user.Enable;
                        string jsonResult = await LinqHelper.UpdateData("User", user);
                        var result = JsonConvert.DeserializeObject<JsonChangedTemplate>(jsonResult);
                        if (null != result)
                        {
                            if (result.ResultCode == (int)ResultCodeType.操作成功)
                            {
                                user.Enable = !user.Enable;
                            }
                            else
                                await MainPage.ShowMessage(result.ResultCode);
                        }
                    }
                    else
                        continue;
                }
            }
            catch (Exception ex)
            {
                await MainPage.ShowErrorMessage(ex.Message);
            }
            finally
            {
                init();
            }
        }
    }
}
