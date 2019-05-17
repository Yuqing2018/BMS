using BackEndMS.Helpers;
using BackEndMS.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace BackEndMS
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static MainPage Current;
        Configerations configs = new Configerations();
        public UserInfo Userinfo { get; set; }
        public Scenario CurrentScenario { get; set; }
        public ObservableCollection<Scenario> NavLinks{get;}
        public MainPage()
        {
            this.InitializeComponent();
          //  SimulateLogin();
            Current = this;
            NavLinks =configs.Scenarios;
            if (Window.Current.Bounds.Width < 640)
            {
                splitView.IsPaneOpen = false;
            }
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Userinfo = (e.Parameter as UserInfo);
            if (!Userinfo.Role.Contains(RoleType.Admin))
                NavLinks.RemoveAt(NavLinks.Count-1);
            base.OnNavigatedTo(e);
        }
        private async void SimulateLogin()
        {
            int timeout = 1000;
            var task = LinqHelper.login("root", "root");
            if (await Task.WhenAny(task, Task.Delay(timeout)) == task)
            {
                return;
            }
            else
            {
                await new ContentDialog()
                {
                    Title = "登录结果",
                    Content = "登录失败",
                    PrimaryButtonText = "Again",
                    SecondaryButtonText = "Close",
                }.ShowAsync();
            }
            //if (flag)
            //{
            //    return;
            //}
            //else
              
        }
        private void toggleBtn_Click(object sender, RoutedEventArgs e)
        {
            splitView.IsPaneOpen = !splitView.IsPaneOpen;
        }

        private void NavLinksList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView scenarioListBox = sender as ListView;
            Scenario s = scenarioListBox.SelectedItem as Scenario;
            if (s != null)
            {
                CurrentScenario = s;
                ScenarioFrame.Navigate(s.ClassType, s.Tabs);
                if (Window.Current.Bounds.Width < 640)
                {
                    splitView.IsPaneOpen = false;
                }
            }
        }

        private void NavLinksList_Loaded(object sender, RoutedEventArgs e)
        {
            if(NavLinksList.Items.Count >0)
            NavLinksList.SelectedIndex = 0;
        }

        private void logoutBtn_Click(object sender, RoutedEventArgs e)
        {
            LinqHelper.token =String.Empty ;
            Frame.Navigate(typeof(Login));
        }

        private void settingBtn_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (sender as Button);
            btn.FlowDirection = FlowDirection.LeftToRight;
            btn.Flyout.ShowAt(btn);
        }

        private void personalCenter_Click(object sender, RoutedEventArgs e)
        {
            ScenarioFrame.Navigate(typeof(PersonalCenter),Userinfo);
        }
        public async static Task ShowMessage(int resultCode)
        {
            ContentDialog content = new ContentDialog()
            {
                Title = "提醒!",
                Content = Enum.GetName(typeof(ResultCodeType), resultCode),
                SecondaryButtonText = "Close",
            };
            await content.ShowAsync();
        }
        public async static Task ShowErrorMessage(String errorMessage)
        {
            ContentDialog content = new ContentDialog()
            {
                Title = "系统错误!",
                Content = errorMessage,
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 0, 0)),
                SecondaryButtonText = "Close",
            };
            await content.ShowAsync();
        }

        private void settingSysBtn_Click(object sender, RoutedEventArgs e)
        {
            ScenarioFrame.Navigate(typeof(SettingsPage));
           // Frame.Navigate(typeof(SettingsPage));
        }
        public void ActiveProgressRing()
        {
            progressRing.IsActive = true;
        }
        public void InActiveProgressRing()
        {
            progressRing.IsActive = false;
        }

        private void personPic_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Image btn = (sender as Image);
            btn.FlowDirection = FlowDirection.LeftToRight;
            btn.ContextFlyout.ShowAt(btn);
        }
    }
}
