using BackEndMS.Helpers;
using BackEndMS.Models;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Data.Json;
using Windows.System;
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
    public sealed partial class SearchQueryPage : Page
    {
        public ObservableCollection<SearchQuery> QueryList { get; set; }
        public SearchEntry SearchModel { get; set; }
        public SearchQueryPage()
        {
            this.InitializeComponent();
            QueryList = new ObservableCollection<SearchQuery>();
            if (SearchModel == null)
                SearchModel = new SearchEntry()
                {
                    StartDate = DateTime.Now.AddMonths(-1),
                    EndDate = DateTime.Today.AddDays(1).AddSeconds(-1),
                    TotalCount = 0,
                    PageIndex = 0,
                    PageSize = 20,
                };
            pagingControl.searchModel = SearchModel;
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Init();
        }
        public async void Init()
        {
            MainPage.Current.ActiveProgressRing();
            try
            {
                QueryList.Clear();
                string jsonResult = await LinqHelper.GetSearchData(SearchModel, "SearchQuery");
                if (SearchModel.ExportType.HasValue && SearchModel.ExportType.Value == ExportType.AllPages)
                {
                    var result = JsonConvert.DeserializeObject<JsonResultTemplate<SearchQuery>>(jsonResult);
                    if (result.ResultCode == (int)ResultCodeType.操作成功)
                        result.Data.ToList().ForEach(item=> { QueryList.Add(item); });
                }
                else
                {
                    var result = JsonConvert.DeserializeObject<JsonSearchResultTemplate<SearchQuery>>(jsonResult);
                    SearchModel.TotalCount = result.Data.TotalSize;
                    SearchModel.PageIndex = result.Data.PageIndex;
                    SearchModel.PageSize = result.Data.PageSize;
                    result.Data.Data.ToList().ForEach(item => { QueryList.Add(item); });
                }
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

        private void searchTB_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            var currentTextbox = sender as TextBox;
            if (!String.IsNullOrEmpty(currentTextbox.Text))
                if (e.OriginalKey == VirtualKey.Enter && e.KeyStatus.RepeatCount == 1)
                {
                    switch (currentTextbox.Name)
                    {
                        case "keywordTB":
                            SearchModel.SearchEntity.Keywords = currentTextbox.Text;
                            break;
                        case "opertorTB":
                            SearchModel.SearchEntity.Operator = currentTextbox.Text;
                            break;

                    }
                    SearchModel.PageIndex = 0;
                    Init();
                }
        }
        private void searchBtn_Click(object sender, RoutedEventArgs e)
        {
            SearchModel.PageIndex = 0;
            Init();
        }

        private void pagingControl_JumpClicked(object sender, ItemClickEventArgs e)
        {
            Init();
        }
    }
}
