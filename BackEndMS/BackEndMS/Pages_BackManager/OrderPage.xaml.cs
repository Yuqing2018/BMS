using BackEndMS.Controls;
using BackEndMS.Helpers;
using BackEndMS.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
namespace BackEndMS
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class OrderPage : Page
    { 
        public Order ViewModel { get; set; }
        public bool IsEditable { get; set; } = true;
        #region List property
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
        private List<string> deleteFileContentIDs= new List<string>();
        private List<string> deleteFileDetailIDs = new List<string>();
        private List<string> deleteBaiduActionIDs = new List<string>();
        // public ObservableCollection<string> CategoryList { get; set; }
        #endregion
        public OrderPage()
        {
            this.InitializeComponent();
            ViewModel = new Order();
          //  CategoryList = new ObservableCollection<string>();
        }
        
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            try
            {
               // InitCategory();//.GetAwaiter().GetResult();
                if (e.Parameter is Order)
                {
                    InitViewModel(e.Parameter as Order);
                    IsEditable = false;
                   // fileContentGV.ItemClick += UploadedFileGV_ItemClick;
                   // fileDetailGV.ItemClick += UploadedFileGV_ItemClick;
                  //  BaiduActionsGV.ItemClick += UploadedFileGV_ItemClick;
                    //fileContentGV.Loaded += UploadedFileGV_Loaded;
                    //fileDetailGV.Loaded += UploadedFileGV_Loaded;
                    //BaiduActionsGV.Loaded += UploadedFileGV_Loaded;
                    //OrderSourceCB.IsEnabled = false;
                    //classCB.IsEnabled = false;
                }
                
                this.DataContext = ViewModel;
            }
            catch (Exception ex)
            {
                MainPage.ShowErrorMessage(ex.Message);
            }
        }

        private void UploadedFileGV_Loaded(object sender, RoutedEventArgs e)
        {
            var gv = (sender as GridView);
            gv.HeaderTemplate = null ;
            gv.Items.ToList().ForEach(item => {
                var gvItem = gv.ContainerFromItem(item) as GridViewItem;
                gvItem.Tapped += tempImage_Tapped;
                var rootPanel = gvItem.ContentTemplateRoot as RelativePanel;
                //(rootPanel.FindName("tempImage") as Image).Tapped += tempImage_Tapped;
                (rootPanel.FindName("deleteFile") as SymbolIcon).Visibility = Visibility.Collapsed;
              //  rootPanel.Tapped += tempImage_Tapped;
            });
            //throw new NotImplementedException();
        }

        private async void InitViewModel(Order v)
        {
            try
            {
                ViewModel = v;
                ViewModel.Labels.Select(x => new TempLable() { Text = x })
                    .ToList().ForEach(item =>
                    {
                        ViewModel.TempLables.Add(item);
                    });
                ViewModel.FileContents.ToList().ForEach(async item =>
                {
                    item.ImageTitle = item.Id;
                    item.ImageSource =await SetImageSource(item);
                });
                textContentEB.Document.SetText(Windows.UI.Text.TextSetOptions.FormatRtf, ViewModel.TextContent);
                textDetailEB.Document.SetText(Windows.UI.Text.TextSetOptions.FormatRtf, ViewModel.TextDetail);
                ViewModel.FileDetails.ToList().ForEach(async item =>
                {
                    item.ImageTitle = item.Id;
                    item.ImageSource = await SetImageSource(item);
                });
                ViewModel.BaiduActions.ToList().ForEach(async item =>
                {
                    item.ImageTitle = item.Id;
                    item.ImageSource = await SetImageSource(item);
                });
             //   ViewModel.CelaExplains.ToList().ForEach(item => { item.IsEditable = false; });
            }
            catch (Exception ex)
            {
                await MainPage.ShowErrorMessage(ex.Message);
            }
        }
        private async Task<BitmapImage> SetImageSource(UploadImage file)
        {
            BitmapImage bitmapImage = new BitmapImage();
            try
            {
                if (!file.ContentType.Contains("image"))
                {
                    var uri = @"ms-appx:///Assets/documentType/blank.png";
                    StorageFile tempFile =await StorageFile.GetFileFromApplicationUriAsync(new Uri(uri));
                    var tempStream = await tempFile.OpenReadAsync();
                    await bitmapImage.SetSourceAsync(tempStream);
                }
                else
                {
                    bitmapImage = new BitmapImage(new Uri(App.RootBaseUri + "File/" + file.Id));
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return bitmapImage;

        }

        public async Task InitCategory()
        {
            string jsonResult = await LinqHelper.GetAllQueryData("BlockQueryCategory");
            var result = JsonConvert.DeserializeObject<JsonResultTemplate<BlockQueryCategory>>(jsonResult);
            if (result.ResultCode == (int)ResultCodeType.操作成功)
            {
                result.Data.Select(x => x.Name).ToList().ForEach(item => { CategoryList.Add(item); });
                if (!String.IsNullOrEmpty(ViewModel.Class1))
                    classCB.SelectedItem = ViewModel.Class1;
            }
        }

        private async void SelectImage(object sender, RoutedEventArgs e)
        {
            FileOpenPicker open = new FileOpenPicker();
            open.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            open.ViewMode = PickerViewMode.Thumbnail;
            // Filter to include a sample subset of file types
            open.FileTypeFilter.Clear();
            string[] ImageTypes = { ".png", ".jpeg", ".jpg", ".bmp", ".gif", ".tiff", ".tif" };
            var filesPointer = ViewModel.FileContents;
            switch ((sender as Button).DataContext)
            {
                case "上传指令内容图片":
                    filesPointer = ViewModel.FileContents;
                    ImageTypes.ToList().ForEach(item => { open.FileTypeFilter.Add(item); });
                    break;
                case "上传指令详情":
                    filesPointer = ViewModel.FileDetails;
                    open.FileTypeFilter.Add("*");
                    break;
                case "上传截图":
                    filesPointer = ViewModel.BaiduActions;
                    ImageTypes.ToList().ForEach(item => { open.FileTypeFilter.Add(item); });
                    break;
            }
            // Open a stream for the selected file
            IReadOnlyList<StorageFile> files = await open.PickMultipleFilesAsync();
            // Ensure a file was selected
            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read))
                    {
                        BitmapImage bitmapImage = new BitmapImage();
                        var fileExtension = file.FileType.ToLower().TrimStart('.');
                        if (!file.ContentType.Contains("image"))
                        {
                            List<string> types = new List<string>() { "doc", "docx", "txt", "xls", "xlsx", "pdf" };
                            string uri = @"ms-appx:///Assets/documentType/blank.png";
                            if (types.Contains(fileExtension))
                                uri = @"ms-appx:///Assets/documentType/" + fileExtension + ".png";
                            StorageFile tempFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri(uri));
                            var tempStream = await tempFile.OpenReadAsync();
                            await bitmapImage.SetSourceAsync(tempStream);
                        }
                        else
                        {
                            await bitmapImage.SetSourceAsync(fileStream);
                        }
                        var tempbitmap = new UploadImage()
                        {
                            ImageSource = bitmapImage,
                            UplodFile = file,
                            ImageTitle = file.Name,
                            ContentType = file.ContentType,
                            FileExtension = fileExtension,
                        };
                        filesPointer.Add(tempbitmap);

                    }
                }
            }
        }
        private void imageTitle_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Flyout.ShowAttachedFlyout(sender as TextBlock);
        }

        private void StrategyLV_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var CurrentLV = sender as ListView;
            (CurrentLV.DataContext as CelaExplain).Strategy = CurrentLV.SelectedItems.Select(x => x.ToString()).ToList();
        }

        private void submitBtn_Click(object sender, RoutedEventArgs e)
        {
            MainPage.Current.ActiveProgressRing();
            try
            {
                ViewModel.Strategy = StrategyLV.SelectedItems.Select(x => x.ToString()).ToList();
                var textContent = "";
                textContentEB.Document.GetText(Windows.UI.Text.TextGetOptions.AdjustCrlf, out textContent);
                ViewModel.TextContent = textContent;

                var textDetail = "";
                textDetailEB.Document.GetText(Windows.UI.Text.TextGetOptions.AdjustCrlf, out textDetail);
                ViewModel.TextDetail = textDetail;

                celaListview.Items.ToList().ForEach(item =>
                {
                    var tempStr = "";
                    var rootContainer = celaListview.ContainerFromItem(item) as ListViewItem;
                    ((rootContainer.ContentTemplateRoot as Grid).FindName("itemEB") as RichEditBox).Document.GetText(Windows.UI.Text.TextGetOptions.AdjustCrlf, out tempStr);
                    (item as CelaExplain).Item = tempStr;
                });
                SaveOrder();
            }
            catch (Exception ex)
            {
                MainPage.ShowErrorMessage(ex.Message);
            }
            finally
            {
                MainPage.Current.InActiveProgressRing();
            }
        }

        //private async void UpdateOrder()
        //{
        //    var resultJOb = JsonConvert.SerializeObject(ViewModel);
        //    string jsonResult = await LinqHelper.UpdateData("Order", ViewModel);
        //    var result = JsonConvert.DeserializeObject<JsonChangedTemplate>(jsonResult);
        //    await MainPage.ShowMessage(result.ResultCode);
        //}

        private async void SaveOrder()
        {
            try
            {
                deleteFileContentIDs.ForEach(async item => { await LinqHelper.DeleteData("File", item); });
                deleteFileDetailIDs.ForEach(async item => { await LinqHelper.DeleteData("File", item); });
                deleteBaiduActionIDs.ForEach(async item => { await LinqHelper.DeleteData("File", item); });
                await SaveUploadFiles(ViewModel.FileContents, "ContentsFile");
                await SaveUploadFiles(ViewModel.FileDetails, "DetailsFile");
                await SaveUploadFiles(ViewModel.BaiduActions, "BaiduActionsFile");
                ViewModel.CelaExplains.ToList().ForEach(item => {
                    if (string.IsNullOrEmpty(item.Item) && (null == item.Strategy || item.Strategy.Count == 0))
                        ViewModel.CelaExplains.Remove(item);
                });
                var resultJOb = JsonConvert.SerializeObject(ViewModel);
                string jsonResult = "";
                if (!String.IsNullOrEmpty(ViewModel.Id))
                {
                    jsonResult = await LinqHelper.UpdateData("Order", ViewModel);
                }
                else
                {
                    jsonResult = await LinqHelper.SaveData(resultJOb, "Order");
                }
                var result = JsonConvert.DeserializeObject<JsonChangedTemplate>(jsonResult);
                await MainPage.ShowMessage(result.ResultCode);
                Frame.Navigate(typeof(OrderStatisticsPage));
            }
            catch (Exception ex)
            {
                MainPage.ShowErrorMessage(ex.Message);
            }
        }
        private async Task SaveUploadFiles(ObservableCollection<UploadImage> UploadImages, string UploadName)
        {
            var SaveList = UploadImages.Where(x => String.IsNullOrEmpty(x.Id)).ToList();
            string jsonResult = await LinqHelper.SaveFileAsync(SaveList, UploadName);
            var result = JsonConvert.DeserializeObject<JsonResultTemplate<UploadImage>>(jsonResult);
            int i = 0;
            while (i < SaveList.Count && i < result.Data.Count())
            {
                SaveList[i].Id = result.Data.ElementAt(i).Id;
                SaveList[i].ContentType = result.Data.ElementAt(i).ContentType;
               // UploadImages[i].FileExtension = result.Data.ElementAt(i).FileExtension;
                i++;
            }
        }
        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(this.GetType());
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as Button).DataContext)
            {
                case "Cela建议":
                    ViewModel.CelaExplains.Add(new CelaExplain());
                    break;
                case "标签":
                    ViewModel.TempLables.Add(new TempLable());
                    break;
            }
        }

        private async void deleteFile_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var item = (sender as SymbolIcon).DataContext as UploadImage;
            string deleteId = item.Id;
            var Filepointer = ViewModel.FileContents;
            var deletePointer = deleteFileContentIDs;
            if (fileContentGV.SelectedItem != null)
            {
                if (ViewModel.FileContents.Contains(item))
                {
                    Filepointer = ViewModel.FileContents;
                    deletePointer = deleteFileContentIDs;
                }
                    //ViewModel.FileContents.Remove(item);
            }
            if (fileDetailGV.SelectedItem != null)
            {
                if (ViewModel.FileDetails.Contains(item))
                {
                    Filepointer = ViewModel.FileDetails;
                    deletePointer = deleteFileDetailIDs;
                }
                //ViewModel.FileDetails.Remove(item);
            }
            if (BaiduActionsGV.SelectedItem != null)
            {
                if (ViewModel.BaiduActions.Contains(item))
                {
                    Filepointer = ViewModel.BaiduActions;
                    deletePointer = deleteBaiduActionIDs;
                }
                //ViewModel.BaiduActions.Remove(item);
            }
            Filepointer.Remove(item);//页面中删掉对应附件
            if (!String.IsNullOrEmpty(item.Id))
            {
                if (!String.IsNullOrEmpty(ViewModel.Id))
                    deletePointer.Add(item.Id);
                else
                    LinqHelper.DeleteData("File", deleteId);
            }
        }

        private void deleteLabel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ViewModel.TempLables.Remove((sender as SymbolIcon).DataContext as TempLable);
        }

        private async void tempImage_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                var item = (sender as Image).DataContext as UploadImage;
                    //.FindName("tempImage") as Image;
              //  var item = (sender as GridViewItem).Content as UploadImage;
                Uri source = new Uri(App.RootBaseUri + "File/" + item.Id);
                if (item.ContentType.Contains("image"))
                {
                    ShowImage((sender as Image).Source);
                }
                else if(item.IsEdit)
                {
                    BackgroundDownloader downloader = new BackgroundDownloader();
                    FolderPicker picker = new FolderPicker { SuggestedStartLocation = PickerLocationId.Downloads };
                    picker.FileTypeFilter.Add("*");
                    StorageFolder folder = await picker.PickSingleFolderAsync();
                    if (folder != null)
                    {
                        StorageFile destinationFile = await folder.CreateFileAsync(item.Id + "." + item.FileExtension, CreationCollisionOption.ReplaceExisting);
                        DownloadOperation download = downloader.CreateDownload(source, destinationFile);
                        await download.StartAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                await MainPage.ShowErrorMessage(ex.Message);
            }
        }
        private void ShowImage(ImageSource source)
        {
            ImageShowControl image = new ImageShowControl(source);
            image.Show();
            //currentImage.Source = new BitmapImage(source);
            //popup.Width = contentGrid.ActualWidth;
            //popup.Height = contentGrid.ActualHeight;
            //popup.IsOpen = true;

        }
        //private void Canvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        //{
        //    popup.IsOpen = false ;
        //}
        private void StrategyLV_Loaded(object sender, RoutedEventArgs e)
        {
            var listview = (sender as ListView);
            List<string> Strategy = null;
                
            if (listview.DataContext is Order)
            {
                Strategy = (listview.DataContext as Order).Strategy;
            }
            else if (listview.DataContext is CelaExplain)
            {
                Strategy = (listview.DataContext as CelaExplain).Strategy;
            }
            if (null != Strategy && Strategy.Count > 0)
            {
                listview.SelectAll();
                listview.SelectedItems.Where(x => !Strategy.Contains(x)).ToList().ForEach(item =>
                {
                    listview.SelectedItems.Remove(item);
                });
            }
        }

        private void itemEB_Loaded(object sender, RoutedEventArgs e)
        {
            var itemEB = (RichEditBox)sender;
            var data = (itemEB.DataContext as CelaExplain);
            if (!String.IsNullOrEmpty(data.Item))
            {
                itemEB.Document.SetText(Windows.UI.Text.TextSetOptions.FormatRtf, data.Item);
            }
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(OrderStatisticsPage));
        }
    }
}

