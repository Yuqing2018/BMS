using BackEndMS.Helpers;
using BackEndMS.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace BackEndMS
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BingWSPage : Page
    {
        public ObservableCollection<BingWSItem> ViewModel { get; set; }
        public ValidDays ValidDays { get; set; }
        public string RequestId { get; set; }
        public string keywords { get; set; }
        public int level { get; set; } = 2;
        public QueryEntryFlag type { get; set; } = QueryEntryFlag.AS;
        public List<QueryEntryFlag> FlagList
        {
            get
            {
                var list = new List<QueryEntryFlag>();
                list.Add(QueryEntryFlag.AS);
                list.Add(QueryEntryFlag.QS);
                list.Add(QueryEntryFlag.PQS);
                list.Add(QueryEntryFlag.PIQS);
                //var list = Enum.GetValues(typeof(QueryEntryFlag)).Cast<QueryEntryFlag>().ToList();
                return list;
            }
        }
        public List<ValidDays> ValidDaysList
        {
            get
            {
                return Enum.GetValues(typeof(ValidDays)).Cast<ValidDays>().ToList();
            }
        }
        public List<int> LevelList = new List<int>() { 1, 2, 3, 4, 5 };
        public BingWSPage()
        {
            this.InitializeComponent();
            ViewModel = new ObservableCollection<BingWSItem>();
            if (!String.IsNullOrEmpty(LinqHelper.RequestId))
                RequestId = LinqHelper.RequestId;
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }
        private async void InitViewModel()
        {
            MainPage.Current.ActiveProgressRing();
            try
            {
                ViewModel.Clear();
                string jsonResult = await LinqHelper.GetBingWS(keywords, level, type.ToString().ToLower());
                JObject jo = JObject.Parse(jsonResult);
                if (!jo["Data"].HasValues)
                    return;
                var result = JsonConvert.DeserializeObject<JsonResultTemplate<BingWSItem>>(jsonResult);
                if (result.ResultCode == (int)ResultCodeType.操作成功)
                {
                    result.Data.ToList().ForEach(item =>
                        {
                            ViewModel.Add(item);
                        });
                }
                // BingWSLv.LayoutUpdated += BingWSLv_LayoutUpdated;
            }
            catch (Exception ex)
            {
                await MainPage.ShowErrorMessage(ex.Message);
            }
            finally {
                MainPage.Current.InActiveProgressRing();
            }
        }

        private void BingWSLv_LayoutUpdated(object sender, object e)
        {
            ViewModel.Where(x => x.IsBlock).ToList().ForEach(item =>
            {
                var lv = BingWSLv.ContainerFromItem(item) as ListViewItem;
                lv.IsEnabled = false;
            });
            //BingWSLv.LayoutUpdated -= BingWSLv_LayoutUpdated;
            //throw new NotImplementedException();
        }

        private async void BingWSLv_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var item = (sender as TextBlock).DataContext;
            if (null != item && item is BingWSItem && (item as BingWSItem).IsBlock)
            {
                var lv = BingWSLv.ContainerFromItem(item) as ListViewItem;
                lv.IsHitTestVisible = false;
                lv.IsEnabled = false;
                
            }
            //ViewModel.Where(x=>x.IsBlock).ToList().ForEach(item =>
            //{
            //    var lv = BingWSLv.ContainerFromItem(item) as ListViewItem;
            //    lv.IsEnabled = false;
            //});
        }
        private async void SubmitBtn_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                if (String.IsNullOrEmpty(RequestId))
                {
                    var dialog = new ContentDialog()
                    {
                        Title = "Warning!",
                        Content = "当前RequestID 为空，提交前请输入RequestID",
                        PrimaryButtonText = "Ok",
                        SecondaryButtonText = "Cancel",
                        FullSizeDesired = false,
                    };
                    await dialog.ShowAsync();
                }
                else
                {
                    List<PageQueryEntry> submitList = new List<PageQueryEntry>();
                    BingWSLv.SelectedItems.ToList().ForEach(item =>
                    {
                        var content = item as BingWSItem;
                        var tempItem = new PageQueryEntry()
                        {
                            Keywords = this.keywords,
                            Content = content.Content,
                            Flag = type,
                            RequestId = this.RequestId,
                            ValidDays = (int)this.ValidDays,
                        };
                        submitList.Add(tempItem);
                    });
                    var jsonArray = JsonConvert.SerializeObject(submitList);
                    var jsonResult = await LinqHelper.SaveBatchData(jsonArray, "PageQuery");
                    var result = JsonConvert.DeserializeObject<JsonChangedTemplate>(jsonResult);
                    if (result.ResultCode == (int)ResultCodeType.操作成功)
                        await MainPage.ShowMessage(result.ResultCode);
                }
            }
            catch (Exception ex)
            {
                await MainPage.ShowErrorMessage(ex.Message);
            }
        }

        private void searchBtn_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            InitViewModel();
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            (sender as GridViewItem).IsSelected = true;
        }

        private void typeRB_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var btn = (sender as RadioButton).DataContext;

            typeGV.SelectedItem = btn;

        }
        private void typeGV_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var dd = typeGV.FindName("typeRB");
             //  ((typeGV.ContainerFromItem(typeGV.SelectedItem) as GridViewItem).ContentTemplateRoot as RadioButton).IsChecked = true;
        }

        private void typeRB_Loaded(object sender, RoutedEventArgs e)
        {
            if ((sender as RadioButton).DataContext == typeGV.SelectedItem)
                (sender as RadioButton).IsChecked = true;
        }

        private void searchTb_KeyUp(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            var currentTextbox = sender as TextBox;
            if (!String.IsNullOrEmpty(currentTextbox.Text))
            {
                keywords = currentTextbox.Text;
                if (e.OriginalKey == VirtualKey.Enter && e.KeyStatus.RepeatCount == 1)
                {
                    InitViewModel();
                }
            }
        }
    }
}
    
