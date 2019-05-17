//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

using BackEndMS.Helpers;
using BackEndMS.Models;
using Newtonsoft.Json;
using System;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.Data.Json;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace BackEndMS
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class QueryContent : Page
    {
        private JsonObject jsonObject;

        public PageQuery viewModel { get; set; }
        public static int searchNo { get; set; } = 0;
        public QueryContent()
        {
            this.InitializeComponent();
            if(viewModel == null)
                viewModel = new PageQuery();
            if (!String.IsNullOrEmpty(LinqHelper.RequestId))
                viewModel.RequestId = LinqHelper.RequestId;
            var flagList = Enum.GetValues(typeof(QueryEntryFlag)).Cast<QueryEntryFlag>().ToList();
            flagList.Remove(QueryEntryFlag.Domain);
            this.flagCB.ItemsSource = flagList;
            this.flagCB.SelectedItem = QueryEntryFlag.All;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var url = new Uri("https://www.bing.com/");
            if (e.Parameter is string)
                Uri.TryCreate(e.Parameter.ToString(), UriKind.RelativeOrAbsolute, out url);
            //var url = new Uri("https://cn.bing.com/images/");images/search?form=EDNTHT&mkt=zh-cn
            WebViewControl.Navigate(url);
        }
        private async void WebViewControl_LoadCompleted(object sender, NavigationEventArgs e)
        {
           // var result = await GetKeywordsAS.Autosuggest();
            StorageFile file =await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Helpers/WebviewScript.js"));
            if (file != null)
            {
                string js = await FileIO.ReadTextAsync(file);
                //await GetContentFromFile(file);
                //eval函数大家都知道，就是执行一段字符串
                await WebViewControl.InvokeScriptAsync("eval", new string[] { js });
            }
        }
        public void WebViewControl_ScriptNotify(object sender, NotifyEventArgs e)
        {
            string[] result = e.Value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (result.Length == 2 && result[0].StartsWith("/search?"))
            {
                SaveSearchQuery(result[1]);
                return;
            }
            if (result.Length == 2 && result[0].StartsWith("https:"))
            {
                SaveSearchQuery(result[1]);
                WebViewControl.Navigate(new Uri(result[0]));
                return;
            }

            //As 联想词添加
            if (result.Length >= 3 && result[1] == "3")
            {
                var keyword = result[0].Trim();
                SaveSearchQuery(keyword);
                var Flag = (QueryEntryFlag)int.Parse(result[1]);

                var list = result.ToList();
                    list.RemoveRange(0, 2);
                list.ForEach(item => {
                    var tempRS = new PageQueryEntry()
                    {
                        Keywords = keyword,
                        Content = item,
                        Flag = Flag,
                        isFirst = viewModel.Querys.Where(x => x.Keywords == keyword).Count() == 0,
                    };
                    if (!viewModel.Querys.Contains(tempRS))
                        viewModel.Querys.Add(tempRS);
                    UpdateKeywordsList();
                });
            }
           else if (result.Length == 3 && 0 == viewModel.Querys.Count(x=>x.Keywords == result[0] && x.Content == result[1]))
            {
                var flag = (QueryEntryFlag)int.Parse(result[2]);
                var tempRS = new PageQueryEntry()
                {
                    Keywords = result[0],
                    Content = result[1],
                    Flag  = flag,
                    isFirst = viewModel.Querys.Where(x=>x.Keywords == result[0]).Count() ==0,
                };
                if(!viewModel.Querys.Contains(tempRS))
                    viewModel.Querys.Add(tempRS);
                UpdateKeywordsList();
            }
            //测试Web 弹出信息
            //var dialog = new ContentDialog()
            //{
            //    Title = "ScriptNotify",
            //    Content = e.Value,
            //    PrimaryButtonText = "Ok",
            //    SecondaryButtonText = "Cancel",
            //    FullSizeDesired = false,
            //};
            //await dialog.ShowAsync();
        }
        private void deleteBtn_Click(object sender, RoutedEventArgs e)
        {
            viewModel.Querys.Remove((sender as Button).DataContext as PageQueryEntry);
            UpdateKeywordsList();
        }
        private void copyBtn_Click(object sender, RoutedEventArgs e)
        {
            var copyContent = "";
            DataPackage dataPackage = new DataPackage();
            dataPackage.RequestedOperation = DataPackageOperation.Copy;
            bool param = !((QueryEntryFlag)flagCB.SelectedValue == QueryEntryFlag.All);
            var contentList = viewModel.Querys.Where(x => x.Keywords == keywordsCB.SelectedItem.ToString());
            if ((QueryEntryFlag)flagCB.SelectedItem != QueryEntryFlag.All)
                contentList = contentList.Where(x => x.Flag == (QueryEntryFlag)flagCB.SelectedItem);
            var contentArray = contentList.Select(x => x.Content).ToArray();
            copyContent = String.Join(Environment.NewLine, contentArray);
            dataPackage.SetText(copyContent);
            Clipboard.SetContent(dataPackage);
        }
        private void UpdateKeywordsList()
        {
            this.rootSplitview.IsPaneOpen = true;
            this.keywordsCB.ItemsSource = this.viewModel.Querys.Select(x => x.Keywords).Distinct().ToList();
            if (keywordsCB.ItemsSource != null && keywordsCB.Items.Count>0)
                this.keywordsCB.SelectedIndex = 0;
            this.statisticTb.Text = String.Format("共计:{0}条 ", viewModel.Querys.Count());
        }
        private async void submitBtn_Click(object sender, RoutedEventArgs e)
        {
            var length = viewModel.Querys.Count;
            if (length == 0)
                return;
            if (String.IsNullOrEmpty(viewModel.RequestId))
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
                viewModel.Querys.ToList().ForEach(item => {
                    item.RequestId = viewModel.RequestId;
                    item.ValidDays = (int)viewModel.ValidDays;
                });
                //for (int i = 0; i < length; i++)
                //{
                //    var result = JsonHelper.JsonSerializer(viewModel.Querys[i]);
                //    var res = await LinqHelper.SaveData(result);
                //}
                #region 批量上传
                var jsonArray = JsonConvert.SerializeObject(viewModel.Querys);
                #endregion
                var jsonResult = await LinqHelper.SaveBatchData(jsonArray, "PageQuery");
                var result = JsonConvert.DeserializeObject<JsonChangedTemplate>(jsonResult);
                await MainPage.ShowMessage(result.ResultCode);
                viewModel.Querys.Clear();
                UpdateKeywordsList();
            }
        }
        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            viewModel.Querys.Clear();
            UpdateKeywordsList();
        }

        private void RequestIDtb_TextChanged(object sender, TextChangedEventArgs e)
        {
            LinqHelper.RequestId = (sender as TextBox).Text;
        }
        private void toggleBtn_Click(object sender, RoutedEventArgs e)
        {
            rootSplitview.IsPaneOpen = !rootSplitview.IsPaneOpen;
        }
        private void SaveSearchQuery(string Keyword)
        {
            var JsonStr = JsonConvert.SerializeObject(new SearchQuery() { Keywords = Keyword });
            LinqHelper.SaveData(JsonStr, "SearchQuery");
        }

        private void MoreOP_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (sender as Button);
            btn.FlowDirection = FlowDirection.LeftToRight;
            btn.Flyout.ShowAt(btn);
        }

        //private void SymbolIcon_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        //{
        //    if (copyBar.Visibility == Visibility.Visible)
        //        copyBar.Visibility = Visibility.Collapsed;
        //    else
        //        copyBar.Visibility = Visibility.Visible;
        //}
    }
}
