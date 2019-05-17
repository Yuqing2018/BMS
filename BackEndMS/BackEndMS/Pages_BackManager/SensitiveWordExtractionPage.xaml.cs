using BackEndMS.Helpers;
using BackEndMS.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace BackEndMS
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SensitiveWordExtractionPage : Page
    {
        public ObservableCollection<SensitiveWords> TreeItems { get; set; }
        public ObservableCollection<AssociativeWord> AssociativeWords { get; set; }
        public SensitiveWordExtractionPage()
        {
            this.InitializeComponent();
            TreeItems = new ObservableCollection<SensitiveWords>();
            AssociativeWords = new ObservableCollection<AssociativeWord>();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }
        private void AssociativeGridview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectWord = (sender as GridView).SelectedItem as AssociativeWord;
           // InitCategory();
        }
        //public async void InitCategory()
        //{
        //    TreeItems.Clear();
        //    JsonObject jsonObject = await LinqHelper.GetAllQueryData("BlockQueryCategory");
        //    string resultCode = jsonObject.GetNamedValue("ResultCode").ToString();
        //    if (int.Parse(resultCode) == (int)ResultCodeType.操作成功)
        //    {
        //        JsonArray jsonArray = jsonObject.GetNamedArray("Data");
        //        jsonArray.Select(x => JsonConvert.DeserializeObject<BlockQueryCategory>(x.ToString())).ToList().ForEach(item => { TreeItems.Add(item); });

        //    }
        //}
        private void SearchBox_QuerySubmitted(SearchBox sender, SearchBoxQuerySubmittedEventArgs args)
        {
            AssociativeWords.Clear();
            for (int i = 1; i <= 20; i++)
            {
                AssociativeWords.Add(new AssociativeWord() {
                    Text = sender.QueryText + "_敏感词" + i,
                    LastDatetime = LinqHelper.GetUnixTimeStamp(DateTime.Now),
                    Status = SensitiveStatus.未处理,
                });
            }
        }

        private async void extractBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var watch = new Stopwatch();
                watch.Start();
                var worditem = (sender as Button).DataContext as AssociativeWord;
                switch (worditem.Status)
                {
                    case SensitiveStatus.未处理:
                        {
                            worditem.Status = SensitiveStatus.获取AS树;
                            Debug.WriteLine("----------------获取ASTree Start");
                            var resultStr = await LinqHelper.GetBingWS("娇蛮之吻",3,QueryEntryFlag.AS.ToString().ToLower());

                            //GenerateCategory(worditem.Text + worditem.cycleNum, worditem.cycleNum++).ContinueWith(_ =>
                            //{
                            //    if (_.Result == null)
                            //        worditem.Status = SensitiveStatus.完结;
                            //    else
                            //    {
                            //        worditem.ResultData = JsonConvert.SerializeObject(_.Result);
                            //        worditem.Status = SensitiveStatus.已获取;

                            //        watch.Stop();
                            //        Debug.WriteLine(watch.ElapsedMilliseconds);
                            //        Debug.WriteLine("----------------获取ASTree Stop");
                            //    }
                            //}, TaskScheduler.FromCurrentSynchronizationContext());

                            break;
                        }
                    case SensitiveStatus.已获取:
                        {
                            var list = JsonConvert.DeserializeObject<List<SensitiveWords>>(worditem.ResultData);
                            TreeItems.Clear();
                            list.ForEach(item => { TreeItems.Add(item); });
                            SensitiveWordsContentDialog dialog = new SensitiveWordsContentDialog(TreeItems);
                            var result = await dialog.ShowAsync();
                            if (result == ContentDialogResult.Primary)
                            {
                                worditem.Status = SensitiveStatus.处理中;
                                Debug.WriteLine("----------------AStree submit处理中 Start");
                                TreeItems.Where(x => x.isSelected).ToList();
                                //Thread.Sleep(5000);
                                //worditem.Status = SensitiveStatus.已处理;
                                //watch.Stop();
                                //Debug.WriteLine(watch.ElapsedMilliseconds);
                                //Debug.WriteLine("----------------AStree submit处理 结束 Stop");
                                Task.Delay(1000 * 2).ContinueWith(x=> {
                                    worditem.Status = SensitiveStatus.已处理;
                                    watch.Stop();
                                    Debug.WriteLine(watch.ElapsedMilliseconds);
                                    Debug.WriteLine("----------------AStree submit处理 结束 Stop");
                                });

                            }
                            break;
                        }
                    case SensitiveStatus.已处理:
                        {
                            worditem.Status = SensitiveStatus.未处理;
                            break;
                        }
                    case SensitiveStatus.完结:
                        {
                            AssociativeWords.Remove(worditem);
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                MainPage.ShowErrorMessage(ex.Message);
            }
        }
        private async Task<List<SensitiveWords>> GenerateCategory(string text,int cycleNum)
        {
            if (text.Split('_').Length == 7 || cycleNum == 10)
                return null;
            else
            {
                var result = new List<SensitiveWords>();
                for (int i = 1; i <= 8; i++)
                {
                    var content = text + "_" + i;
                    Debug.WriteLine(content);
                    await GenerateCategory(content, cycleNum).ContinueWith(_ =>
                    result.Add(new SensitiveWords()
                    {
                        Text = content,
                        Children = _.Result
                    }), TaskScheduler.FromCurrentSynchronizationContext());
                    //result.Add(new SensitiveWords()
                    //{
                    //    Text = content,
                    //    Children = await GenerateCategory(content, cycleNum),
                    //});

                    //this.dispatcherThread = new Thread(() =>
                    //{
                    //    // This is here just to force the dispatcher infrastructure to be setup on this thread
                    //    Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
                    //    {
                    //        Trace.WriteLine("Dispatcher worker thread started.");
                    //    }));

                    //    // Run the dispatcher so it starts processing the message loop
                    //    Dispatcher.Run();
                    //});

                    //this.dispatcherThread.SetApartmentState(ApartmentState.STA);
                    //this.dispatcherThread.IsBackground = true;
                    //this.dispatcherThread.Start();
                }
                return result;
            }
        }
    }

   
}
