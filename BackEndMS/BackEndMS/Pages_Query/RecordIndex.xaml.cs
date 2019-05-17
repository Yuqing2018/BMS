using BackEndMS.Helpers;
using BackEndMS.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace BackEndMS
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RecordIndex : Page
    {
        public SearchEntry SearchEntry { get; set; }
        public ObservableCollection<PageQuery> QueryRecords { get; set; }
        public RecordIndex()
        {
            this.InitializeComponent();
            if (QueryRecords == null)
            {
                QueryRecords = new ObservableCollection<PageQuery>();
            }
            if (SearchEntry == null)
                SearchEntry = new SearchEntry()
                {
                    StartDate = DateTime.Today.AddMonths(-1),
                    EndDate = DateTime.Today.AddDays(1).AddSeconds(-1),
                    TotalCount = 0,
                    PageIndex = 0,
                    PageSize = 20,
                };
            gridviewControl.DataContext = QueryRecords;
            pagingControl.searchModel = SearchEntry;
            //flagCB.ItemsSource = SearchEntry.FlagList;
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
              LoadRecords();
        }
        #region 文件中的保存数据 
        /// <summary>
        /// 加载查询记录
        /// </summary>
        private async void LoadRecords1()
        {
            QueryRecords.Clear();
            SearchEntry.TotalCount = 0;
            StorageFolder loaclFolder = ApplicationData.Current.LocalFolder;
            IReadOnlyList<StorageFile> files = await loaclFolder.GetFilesAsync();
            var txtList = files.Where(x => x.FileType.ToLower() == ".txt").ToList();
            if (!String.IsNullOrEmpty(SearchEntry.SearchEntity.RequestId))
            {
                txtList = txtList.Where(x => x.DisplayName == SearchEntry.SearchEntity.RequestId).ToList();
            }
            Parallel.ForEach(txtList, async item =>
            {
                string result = await FileIO.ReadTextAsync(item, Windows.Storage.Streams.UnicodeEncoding.Utf8);
                var list = JsonConvert.DeserializeObject<ObservableCollection<PageQueryEntry>>(result);
                //string jsonRS = await JsonHelper.GetContentFromFile(item);
                //ObservableCollection<PageQueryEntry> list = JsonHelper.JsonDeserializer<ObservableCollection<PageQueryEntry>>(jsonRS);
                var tempList = list.ToList();
                //if (SearchEntry.SearchEntity.StartDate.HasValue)
                //{
                //    tempList = tempList.Where(x => SearchEntry.SearchEntity.StartDate < x.ModifyDate).ToList();
                //}
                //if (SearchEntry.SearchEntity.EndDate.HasValue)
                //    tempList = tempList.Where(x => SearchEntry.SearchEntity.EndDate > x.ModifyDate).ToList();
                if (!String.IsNullOrEmpty(SearchEntry.SearchEntity.Operator))
                {
                    tempList = tempList.Where(x => x.Operator == SearchEntry.SearchEntity.Operator).ToList();
                }
                list = tempList.ToObservableCollectionAsync();
                list.GroupBy(x => x.Keywords).ToList().ForEach(queryItem =>
                {
                    queryItem.ToList().ForEach(
                        singleItem =>
                        {
                            if (singleItem != queryItem.FirstOrDefault())
                                singleItem.Keywords = "";
                        });
                });

                PageQuery temp = new PageQuery()
                {
                    RequestId = item.DisplayName,
                    Querys = list,
                };
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher
                .RunAsync(CoreDispatcherPriority.Normal, () =>
                 {
                     QueryRecords.Add(temp);
                     SearchEntry.TotalCount += list.Count;
                 });

            });

        }
        #endregion

        private async void LoadRecords()
        {
            try
            {
                MainPage.Current.ActiveProgressRing();
                QueryRecords.Clear();
                var recordList = await GetSearchedData();
                var pageQueryList = recordList.GroupBy(x => x.RequestId)
                         .Select(x => new PageQuery
                         {
                             RequestId = x.Key,
                             Querys = x.ToObservableCollectionAsync(),
                         }).ToList();

                pageQueryList.ForEach(item =>
                {
                    item.Querys.GroupBy(x => x.Keywords).ToList().ForEach(queryItem =>
                     {
                         queryItem.ToList().ForEach(
                             singleItem =>
                             {
                                 singleItem.isFirst = (singleItem == queryItem.FirstOrDefault());
                             });
                     });
                    QueryRecords.Add(item);
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

        private async Task<List<PageQueryEntry>> GetSearchedData()
        {
            List<PageQueryEntry> pageQueryList = new List<PageQueryEntry>();
            try
            {
                var jsonResult = await LinqHelper.GetSearchData(SearchEntry, "PageQuery");
                if (SearchEntry.ExportType.HasValue && SearchEntry.ExportType.Value == ExportType.AllPages)
                {
                    var result = JsonConvert.DeserializeObject<JsonResultTemplate<PageQueryEntry>>(ResetJsonResult(jsonResult,"Data"));
                    if (result.ResultCode == (int)ResultCodeType.操作成功)
                        pageQueryList = result.Data.ToList();
                }
                else
                {
                    var result = JsonConvert.DeserializeObject<JsonSearchResultTemplate<PageQueryEntry>>(ResetJsonResult(jsonResult, "Data.Data"));
                    SearchEntry.TotalCount = result.Data.TotalSize;
                    SearchEntry.PageIndex = result.Data.PageIndex;
                    SearchEntry.PageSize = result.Data.PageSize;
                    pageQueryList = result.Data.Data.ToList();
                }
               
            }
            catch (Exception ex)
            {
                await MainPage.ShowErrorMessage(ex.Message);
            }
            return pageQueryList;
        }
        private string ResetJsonResult(string jsonResult,string rootPath)
        {
            JObject jo = JObject.Parse(jsonResult);
            foreach (var token in jo.SelectTokens(rootPath)
                .Children())
            {
                var flagToken = token.SelectToken("Flag").Parent;
                if (null != flagToken)
                {
                    var prop = flagToken as JProperty;
                    int target = (int)prop.Value;
                    prop.Value = target;
                }
                var statusToken = token.SelectToken("Status").Parent;
                if (null != statusToken)
                {
                    var prop = statusToken as JProperty;
                    int target = (int)prop.Value;
                    prop.Value = target;
                }
            }
            return jo.ToString();
        }
        private void searchBtn_Click(object sender, RoutedEventArgs e)
        {
            SearchEntry.PageIndex = 0;
            LoadRecords();
        }
        private void searchTb_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            var currentTextbox = sender as TextBox;
            if (!String.IsNullOrEmpty(currentTextbox.Text))
                if (e.OriginalKey == VirtualKey.Enter && e.KeyStatus.RepeatCount == 1)
                {
                    switch(currentTextbox.Name)
                    {
                        case "requestIDtb":
                            SearchEntry.SearchEntity.RequestId = currentTextbox.Text;
                            break;
                        case "keywordsTb":
                            SearchEntry.SearchEntity.Keywords = currentTextbox.Text;
                            break;
                        case "userNametb":
                            SearchEntry.SearchEntity.Operator = currentTextbox.Text;
                            break;

                    }
                    SearchEntry.PageIndex = 0;
                    LoadRecords();
                }
        }
        private void pagingControl_JumpClicked(object sender, ItemClickEventArgs e)
        {
            LoadRecords();
            // string jsonParam = JsonHelper.JsonSerializer<BaseSearch>(pagingControl.searchModel);
        }

        private void flagCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SearchEntry.Flag = (QueryEntryFlag)flagCB.SelectedValue;
        }

        private void flagCB_Loaded(object sender, RoutedEventArgs e)
        {
            if (flagCB.ItemsSource != null)
                flagCB.SelectedIndex = 0;
            // LoadRecords();
        }
        private async void exportBtn_Click(object sender, RoutedEventArgs e)
        {
            MainPage.Current.ActiveProgressRing();
            try
            {
                List<PageQueryEntry> recordList = new List<PageQueryEntry>();
                var exportBtnName = (sender as Button).Name;
                if (exportBtnName == "exportAllBtn")
                {
                    SearchEntry.ExportType = ExportType.AllPages;
                    recordList = await GetSearchedData();
                }
                else
                    QueryRecords.Select(x => x.Querys).ToList().ForEach(item => { recordList.AddRange(item); });
                // SearchEntry.ExportType = ExportType.CurrentPage;
                //  var recordList = await GetSearchedData();
                if (recordList.Count > 0)
                    ExportHelper.GetInstance().GenerateTempOrderExcel(recordList);
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

        private async void tableControl_ChangeValidCliked(object sender, ItemClickEventArgs e)
        {
            try
            {
                var item = (sender as MenuFlyoutItem).DataContext as PageQueryEntry;
                switch ((sender as MenuFlyoutItem).Text.Trim())
                {
                    case "UnBlock":
                        item.Method = "UnBlock";
                        break;
                    case "永久":
                        item.Method = "Block";
                        item.Status = SubmitStatus.未提交;
                        item.ValidDays = (int)ValidDays.永久;
                        item.IsExpire = false;
                        break;
                    case "一个月":
                        item.Method = "Block";
                        item.Status = SubmitStatus.未提交;
                        item.ValidDays = (int)ValidDays.一个月;
                        item.IsExpire = false;
                        break;
                    case "三个月":
                        item.Method = "Block";
                        item.Status = SubmitStatus.未提交;
                        item.ValidDays = (int)ValidDays.三个月;
                        item.IsExpire = false;
                        break;
                }
                string jsonResult = string.Empty;
                if (!String.IsNullOrEmpty(item.Id))
                    jsonResult = await LinqHelper.UpdateData("PageQuery", item);
                var result = JsonConvert.DeserializeObject<JsonChangedTemplate>(jsonResult);
                if (result.ResultCode == (int)ResultCodeType.操作成功)
                {
                    MainPage.ShowMessage(result.ResultCode);
                    LoadRecords();
                }
            }
            catch (Exception ex)
            {
                MainPage.ShowErrorMessage(ex.Message).GetAwaiter().GetResult();
            }
    }

        private void ExpireCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //0-全部，1-有效，2-已过期
            var item = (sender as ComboBox).SelectedIndex;
            if (item == 1)
                SearchEntry.IsExpire = false;
            else if (item == 2)
                SearchEntry.IsExpire = true;
        }
    }
}
