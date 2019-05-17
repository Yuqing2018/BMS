using BackEndMS.Controls;
using BackEndMS.Helpers;
using BackEndMS.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.System;
using Windows.UI.Core;
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
    public sealed partial class BackManage_Query : Page
    {
        #region property
        public SearchEntry SearchModel { get; set; }
        public ObservableCollection<BlockEntry> ResultList { get; set; }
        public ObservableCollection<BlockQueryCategory> CategoryList{get;set;}
        public BlockEntryCreateModel CreateModel { get; set; }
        KeywordControl keywordControl;

        /// <summary>
        /// 处理方式列表
        /// </summary>
        public List<Strategy> StrategyList
        {
            get
            {
                var temp = Enum.GetValues(typeof(Strategy)).Cast<Strategy>().ToList();
                return temp;
            }
        }
        public List<Status> EnableList
        {
            get
            {
                return Enum.GetValues(typeof(Status)).Cast<Status>().ToList();
            }
        }
        /// <summary>
        /// 程度列表
        /// </summary>
        public List<Level> LevelList
        {
            get
            {
                return Enum.GetValues(typeof(Level)).Cast<Level>().ToList();
            }
        }
        /// <summary>
        /// 匹配方式列表
        /// </summary>
        public List<Match> MatchList
        {
            get
            {
                return Enum.GetValues(typeof(Match)).Cast<Match>().ToList();
            }
        }
        #endregion

        public BackManage_Query()
        {
            this.InitializeComponent();
            ResultList = new ObservableCollection<BlockEntry>();
            CategoryList = new ObservableCollection<BlockQueryCategory>();
            if (SearchModel == null)
                SearchModel = new SearchEntry()
                {
                    //StartDate = DateTime.Now.AddMonths(-1),
                    //EndDate = DateTime.Today.AddDays(1).AddSeconds(-1),
                    TotalCount = 0,
                    PageIndex = 0,
                    PageSize = 20,
                };
            pagingControl.searchModel = SearchModel;
            if (!MainPage.Current.Userinfo.Role.Contains(RoleType.Admin))
                DeleteBtn.Visibility = Visibility.Collapsed;
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            init();

        }
        public async void init()
        {
            try
            {
                MainPage.Current.ActiveProgressRing();
                CategoryList.Clear();
                InitCategory();
                ResultList.Clear();
                string jsonResult = await LinqHelper.GetSearchData(SearchModel, "BlockQuery");
                var result = JsonConvert.DeserializeObject<JsonSearchResultTemplate<BlockEntry>>(jsonResult);
                if (result.ResultCode == (int)ResultCodeType.操作成功)
                {
                    SearchModel.TotalCount = result.Data.TotalSize;
                    SearchModel.PageIndex = result.Data.PageIndex;
                    SearchModel.PageSize = result.Data.PageSize;
                    result.Data.Data.ToList().ForEach(item => { ResultList.Add(item); });
                    var length = ResultList.Count > 0 ? ResultList.Max(x => x.Keywords.Length) : 0;
                    BlockEntryControl.KeywordMinWidth = length * BlockEntryControl.FontSize;
                }
                else
                {
                    await MainPage.ShowMessage(result.ResultCode);
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
        public async void InitCategory()
        {
            try
            {
                string jsonResult = await LinqHelper.GetAllQueryData("BlockQueryCategory");
                var result = JsonConvert.DeserializeObject<JsonResultTemplate<BlockQueryCategory>>(jsonResult);
                if (result.ResultCode == (int)ResultCodeType.操作成功)
                {
                    result.Data.ToList().ForEach(item => { CategoryList.Add(item); });
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        private async void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            keywordControl = new KeywordControl();
            if (keywordControl.ViewModel.KeywordsCategory.Count == 0)
                keywordControl.ViewModel.KeywordsCategory.AddRange(await BlockEntryCreateModel.InitCategory());
            keywordControl.Show();
            keywordControl.AddRecordClick += KeywordControl_Refresh;
        }
        //private async void EditBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    var block = ResultList.Where(x => x.selected).FirstOrDefault();
        //    if (block != null)
        //    {
        //        keywordControl = new KeywordControl(block);
        //        if(keywordControl.ViewModel.KeywordsCategory.Count  == 0)
        //            keywordControl.ViewModel.KeywordsCategory.AddRange(await BlockEntryCreateModel.InitCategory());
        //        keywordControl.Show();
        //        keywordControl.AddRecordClick += KeywordControl_Refresh;
        //    }
        //}
        private async void BlockEntryControl_EditClicked(object sender, ItemClickEventArgs e)
        {
            try
            {
                BlockEntry block = (sender as Button).DataContext as BlockEntry;
                if (block != null)
                {
                    keywordControl = new KeywordControl(block);
                    if (keywordControl.ViewModel.KeywordsCategory.Count == 0)
                        keywordControl.ViewModel.KeywordsCategory.AddRange(await BlockEntryCreateModel.InitCategory());
                    keywordControl.Show();
                    keywordControl.AddRecordClick += KeywordControl_Refresh;
                }
            }
            catch (Exception ex)
            {
                MainPage.ShowErrorMessage(ex.Message);
            }
        }
        private async void KeywordControl_Refresh(object sender, ItemClickEventArgs e)
        {
            try
            {
                string jsonResult = string.Empty;
                if (null != keywordControl.ViewModel.BlockEntry.Id)
                {
                    jsonResult = await LinqHelper.UpdateData("BlockQuery", keywordControl.ViewModel.BlockEntry);
                }
                else
                {
                    var JsonStr = JsonConvert.SerializeObject(keywordControl.ViewModel.BlockEntry);
                    jsonResult = await LinqHelper.SaveData(JsonStr, "BlockQuery");
                }
                var result = JsonConvert.DeserializeObject<JsonChangedTemplate>(jsonResult);
                if (result.ResultCode == (int)ResultCodeType.操作成功)
                {
                    init();
                }
                keywordControl.Dispose();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private async void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            try { 
            var blockList = ResultList.Where(x => x.selected).ToList();
            if (null != blockList)
            {
                ContentDialog content = new ContentDialog()
                {
                    Title = "记录删除",
                    Content = new TextBlock
                    {
                        Text = "您确定要永久删除这些记录吗 ?",
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness()
                        {
                            Top = 12
                        }
                    },
                    PrimaryButtonText = "确认",
                    PrimaryButtonCommandParameter = blockList,
                    SecondaryButtonText = "取消",
                };

                content.PrimaryButtonClick += Content_PrimaryButtonClick;
                await content.ShowAsync();
            }
            }catch(Exception ex)
            {
                MainPage.ShowErrorMessage(ex.Message);
            }
        }

        private async void Content_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            try
            {
                List<BlockEntry> blockList = sender.PrimaryButtonCommandParameter as List<BlockEntry>;
                foreach (var item in blockList)
                {
                    string jsonResult = await LinqHelper.DeleteData("BlockQuery", item.Id);
                    var result = JsonConvert.DeserializeObject<JsonChangedTemplate>(jsonResult);
                    if (result.ResultCode == (int)ResultCodeType.操作成功)
                    {
                        ResultList.Remove(item);
                    }
                    else
                    {
                        MainPage.ShowMessage(result.ResultCode);
                    }
                }
            }
            catch (Exception ex)
            {
                throw(ex);
            }
            finally
            {
                init();
            }
        }

        private void commanderBar_Loaded(object sender, RoutedEventArgs e)
        {
            var CommandBar = sender as CommandBar;
            var root = VisualTreeHelper.GetChild(CommandBar, 0) as Grid;
            var MoreButton = root.FindName("MoreButton") as UIElement;
            if (MoreButton != null)
            {
                MoreButton.Visibility = Visibility.Collapsed;
            }

        }

        private void searchBtn_Click(object sender, RoutedEventArgs e)
        {
            SearchModel.PageIndex = 0;
            init();
        }

        private void pagingControl_JumpClicked(object sender, ItemClickEventArgs e)
        {
            init();
        }
        KeywordsCategoryControl KeywordsCategoryControl = null;
        private void EditCategoryBtn_Click(object sender, RoutedEventArgs e)
        {
            KeywordsCategoryControl = new KeywordsCategoryControl(CategoryList);
            KeywordsCategoryControl.Show();
            KeywordsCategoryControl.SubmitClick += KeywordsCategoryControl_SubmitClick;
          //  KeywordsCategoryControl.AddRecordClick += SigninControl_AddRecordClick;
        }

        private async void KeywordsCategoryControl_SubmitClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                KeywordsCategoryControl.ActiveProgressRing();
                #region 删除多余类目
                var length = KeywordsCategoryControl.deleteIds?.Count;
                for (int i = 0; i < length; i++)
                {
                    var DeleteObject = await LinqHelper.DeleteData("BlockQueryCategory", KeywordsCategoryControl.deleteIds[i]);
                    var result = JsonConvert.DeserializeObject<JsonChangedTemplate>(DeleteObject);
                  //  string DeleteResultCode = DeleteObject.GetNamedValue("ResultCode").ToString();
                    if (result.ResultCode != (int)ResultCodeType.操作成功)
                    {
                        throw (new Exception("KeywordsCategory 删除失败"+result.Message));
                    }
                }
                #endregion
                #region Update the CategoryList exist before
                var updateList = KeywordsCategoryControl.CategoryList.Where(x => !String.IsNullOrEmpty(x.Id)).ToList();
                for (int i = 0; i < updateList.Count; i++)
                {
                    var item = updateList[i];
                    if (!String.IsNullOrEmpty(item.Id))
                    {
                        var updateResult = await LinqHelper.UpdateData("BlockQueryCategory", item);
                        var result = JsonConvert.DeserializeObject<JsonChangedTemplate>(updateResult);
                        if (result.ResultCode != (int)ResultCodeType.操作成功)
                        {
                            throw (new Exception("KeywordsCategory 更新"+result.Message));
                        }
                    }
                }
                #endregion
                #region 批量上传 新增类目
                var newlist = KeywordsCategoryControl.CategoryList.Where(x => String.IsNullOrEmpty(x.Id));
                if (newlist != null && newlist.Count() > 0)
                {
                    var jsonArray = JsonConvert.SerializeObject(newlist);
                    var jsonResult = await LinqHelper.SaveBatchData(jsonArray, "BlockQueryCategory");
                    var result = JsonConvert.DeserializeObject<JsonChangedTemplate>(jsonResult);
                    await MainPage.ShowMessage(result.ResultCode);
                }
                #endregion

                CategoryList.Clear();
                InitCategory();
                KeywordsCategoryControl.Dispose();
            }
            catch (Exception ex)
            {
                await MainPage.ShowErrorMessage(ex.Message);
            }
            finally
            {
                KeywordsCategoryControl.InActiveProgressRing();
            }
        }

        private void searchTB_KeyUp(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            var currentTextbox = sender as TextBox;
            if (!String.IsNullOrEmpty(currentTextbox.Text))
                if (e.OriginalKey == VirtualKey.Enter && e.KeyStatus.RepeatCount == 1)
                {
                    switch (currentTextbox.Name)
                    {
                        case "requestIdTB":
                            SearchModel.SearchEntity.RequestId = currentTextbox.Text;
                            break;
                        case "keywordTB":
                            SearchModel.SearchEntity.Keywords = currentTextbox.Text;
                            break;
                        case "opertorTB":
                            SearchModel.SearchEntity.Operator = currentTextbox.Text;
                            break;
                        case "class1TB":
                            SearchModel.SearchEntity.Class1 = currentTextbox.Text;
                            break;
                        case "class2TB":
                            SearchModel.SearchEntity.Class2 = currentTextbox.Text;
                            break;
                        case "class3TB":
                            SearchModel.SearchEntity.Class3 = currentTextbox.Text;
                            break;

                    }
                    SearchModel.PageIndex = 0;
                    init();
                }
        }
    }
    
}
