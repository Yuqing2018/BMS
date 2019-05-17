using BackEndMS.Controls;
using BackEndMS.Helpers;
using BackEndMS.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace BackEndMS
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class OrderStatisticsPage : Page
    {
        public OrderSearch SearchEntry { get; set; }
        public  ObservableCollection<Order> OrderList { get; set; }

        /// <summary>
        /// 指令来源列表
        /// </summary>
        public List<string> OrderSourceList
        {
            get
            {
                return Enum.GetNames(typeof(OrderSourceType)).ToList();
            }
        }
        /// <summary>
        /// 处理方式列表
        /// </summary>
        public List<String> OrderStrategyList
        {
            get
            {
                return Enum.GetNames(typeof(OrderStrategyType)).ToList();
            }
        }

        public List<string> CategoryList
        {
            get
            {
                return Enum.GetNames(typeof(OrderCategoryType)).ToList();
            }
        }
        public OrderStatisticsPage()
        {
            this.InitializeComponent();
            if (OrderList == null)
            {
                OrderList = new ObservableCollection<Order>();
            }
            if (SearchEntry == null)
                SearchEntry = new OrderSearch()
                {
                    StartDate = DateTime.Today.AddMonths(-1),
                    EndDate = DateTime.Today.AddDays(1).AddSeconds(-1),
                    TotalCount = 0,
                    PageIndex = 0,
                    PageSize = 20,
                };
            orderTableControl.DataContext = OrderList;
            this.DataContext = this;
            pagingControl.searchModel = SearchEntry;
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            UpdateOrderList();

        }
        public async Task<List<string>> InitCategory()
        {
            string jsonResult = await LinqHelper.GetAllQueryData("BlockQueryCategory");
            var result = JsonConvert.DeserializeObject<JsonResultTemplate<BlockQueryCategory>>(jsonResult);
            if (result.ResultCode == (int)ResultCodeType.操作成功)
            {
                return result.Data.Select(x=>x.Name).ToList();
            }
            else
                return null;
        }
        private async void UpdateOrderList()
        {
            try
            {
                MainPage.Current.ActiveProgressRing();
                OrderList.Clear();
                (await GetSearchedData()).ForEach(item=> 
                {
                    OrderList.Add(item);
                });
            }
            catch (Exception ex)
            {
                await MainPage.ShowErrorMessage(ex.Message);
            }
            finally
            {
                MainPage.Current.InActiveProgressRing();
            }
        }
        private async void AddOrderBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Frame.Navigate(typeof(OrderPage));
            }
            catch (Exception ex)
            {
                await MainPage.ShowErrorMessage(ex.Message);
            }

        }
        private async void DeleteOrderBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedList = OrderList.Where(x => x.selected).ToList();

                if (null != selectedList)
                {
                    ContentDialog content = new ContentDialog()
                    {
                        Title = "记录删除",
                        Content = new TextBlock
                        {
                            Text = "您确定要永久删除这些指令吗?",
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
                UpdateOrderList();
            }
        }
        private async void Content_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            List<Order> selectedList = sender.PrimaryButtonCommandParameter as List<Order>;
            if (null != selectedList)
            {
                foreach (var order in selectedList)
                {
                    order.FileContents.Select(x => x.Id).Distinct().ToList().ForEach(async item =>
                    {
                        if (!String.IsNullOrEmpty(item))
                        {
                            await LinqHelper.DeleteData("File", item);
                        }
                    });
                    order.FileDetails.Select(x => x.Id).Distinct().ToList().ForEach(async item =>
                    {
                        if (!String.IsNullOrEmpty(item))
                        {
                            await LinqHelper.DeleteData("File", item);
                        }
                    });
                    order.BaiduActions.Select(x => x.Id).Distinct().ToList().ForEach(async item =>
                    {
                        if (!String.IsNullOrEmpty(item))
                        {
                            await LinqHelper.DeleteData("File", item);
                        }
                    });
                    string jsonResult = await LinqHelper.DeleteData("Order", order.Id);
                    var result = JsonConvert.DeserializeObject<JsonChangedTemplate>(jsonResult);
                    if (result.ResultCode == (int)ResultCodeType.操作成功)
                    {
                        OrderList.Remove(order);
                    }
                    else
                    {
                        await MainPage.ShowMessage(result.ResultCode);
                    }
                }
            }
        }
        private async void orderTableControl_EditClicked(object sender, ItemClickEventArgs e)
        {
            try
            {
                Frame.Navigate(typeof(OrderPage), (sender as Button).DataContext);
            }
            catch (Exception ex)
            {
                await MainPage.ShowErrorMessage(ex.Message);
            }
        }

        private void pagingControl_JumpClicked(object sender, ItemClickEventArgs e)
        {
            UpdateOrderList();
        }

        private void searchBtn_Click(object sender, RoutedEventArgs e)
        {
            UpdateOrderList();
        }

        private async void exportBtn_Click(object sender, RoutedEventArgs e)
        {
            MainPage.Current.ActiveProgressRing();
            try
            {
                List<Order> recordList = new List<Order>();
                var exportBtnName = (sender as Button).Name;
                if (exportBtnName == "exportAllBtn")
                {
                    SearchEntry.ExportType = ExportType.AllPages;
                }
                recordList = await GetSearchedData();
                if (recordList.Count > 0)
                    ExportHelper.GetInstance().GenerateTempOrderExcel(recordList); ;
                SearchEntry.ExportType = null;
            }
            catch (Exception ex)
            {
                await MainPage.ShowErrorMessage(ex.Message);
            }
            finally
            {
                MainPage.Current.InActiveProgressRing();
            }
        }
        private async Task<List<Order>> GetSearchedData()
        {
            var ResultList = new List<Order>(); 
            try
            {
                if (SearchEntry.ExportType.HasValue && SearchEntry.ExportType.Value == ExportType.AllPages)
                {
                    var jsonResult = await LinqHelper.GetAllQueryData("Order", GenerateSearchParam());
                    var result = JsonConvert.DeserializeObject<JsonResultTemplate<Order>>(jsonResult);
                    if (result.ResultCode == (int)ResultCodeType.操作成功)
                        ResultList = result.Data.ToList();
                }
                else
                {
                    var jsonResult = await LinqHelper.GetData(GenerateSearchParam(), "Order");
                    var result = JsonConvert.DeserializeObject<JsonSearchResultTemplate<Order>>(jsonResult);
                    SearchEntry.TotalCount = result.Data.TotalSize;
                    SearchEntry.PageIndex = result.Data.PageIndex;
                    SearchEntry.PageSize = result.Data.PageSize;
                    ResultList = result.Data.Data.ToList();
                }

            }
            catch (Exception ex)
            {
                await MainPage.ShowErrorMessage(ex.Message);
            }
            return ResultList;
        }

        private string GenerateSearchParam()
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            if(SearchEntry.StartDate.HasValue)
                result.Add("ModifyDate_start", LinqHelper.GetUnixTimeStamp(SearchEntry.StartDate));
            if (SearchEntry.EndDate.HasValue)
                result.Add("ModifyDate_end", LinqHelper.GetUnixTimeStamp(SearchEntry.EndDate));
            result.Add("Operator", SearchEntry.Operator);
            result.Add("PageIndex", SearchEntry.PageIndex);
            result.Add("PageSize", SearchEntry.PageSize);
            result.Add("Source", SearchEntry.Source);
            result.Add("OrderId", SearchEntry.OrderId);
            result.Add("Strategy", SearchEntry.Strategy);
            result.Add("Class1", SearchEntry.Class1);
            result.Add("Label", SearchEntry.Label);
            result.Add("TextContent", SearchEntry.TextContent);
            result.Add("TextDetail", SearchEntry.TextDetail);
            return String.Join("&", result.Select(x => String.Format("{0}={1}", x.Key, x.Value)).ToList());
        }
        private void TextBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            var currentTextbox = sender as TextBox;
            if (!String.IsNullOrEmpty(currentTextbox.Text))
                if (e.OriginalKey == VirtualKey.Enter && e.KeyStatus.RepeatCount == 1)
                {
                    switch (currentTextbox.Name)
                    {
                        case "orderIDTB":
                            SearchEntry.OrderId = currentTextbox.Text;
                            break;
                        case "labelTB":
                            SearchEntry.Label = currentTextbox.Text;
                            break;
                        case "textcontentTB":
                            SearchEntry.TextContent = currentTextbox.Text;
                            break;
                        case "textdetailTB":
                            SearchEntry.TextDetail = currentTextbox.Text;
                            break;
                        case "operatorTB":
                            SearchEntry.Operator = currentTextbox.Text;
                            break;

                    }
                    SearchEntry.PageIndex = 0;
                    UpdateOrderList();
                }
        }
    }
}
