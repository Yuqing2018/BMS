using BackEndMS.Helpers;
using BackEndMS.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace BackEndMS.Controls
{
    public sealed partial class OrderContentDialog : ContentDialog
    {
        public Order ViewModel { get; set; }

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
        public List<string> CategoryList { get; set; }
        #endregion

        public OrderContentDialog(List<String> list)
        {
            Width = 700;
            Height = 700;
            this.InitializeComponent();
            ViewModel = new Order();
            if (list == null)
                CategoryList = new List<string>();
            else
                CategoryList = list;
            this.DataContext = ViewModel;
        }
        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            ViewModel.Strategy = StrategyLV.SelectedItems.Select(x=>x.ToString()).ToList();
            var textContent = "";
            textContentEB.Document.GetText(Windows.UI.Text.TextGetOptions.AdjustCrlf, out textContent);
            ViewModel.TextContent = textContent;

            var textDetail = "";
            textDetailEB.Document.GetText(Windows.UI.Text.TextGetOptions.AdjustCrlf, out textDetail);
            ViewModel.TextDetail = textDetail;

            celaListview.Items.ToList().ForEach(item => {
                var tempStr = "";
                var rootContainer = celaListview.ContainerFromItem(item) as ListViewItem;
                (VisualTreeHelper.GetChild(rootContainer.ContentTemplateRoot,2) as RichEditBox).Document.GetText(Windows.UI.Text.TextGetOptions.AdjustCrlf, out tempStr);
                (item as CelaExplain).Item = tempStr;
            });
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
        private void FontIcon_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ViewModel.TempLables.Add(new TempLable());
        }

        private void Cela_FontIcon_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ViewModel.CelaExplains.Add(new CelaExplain() { Item = ""});
        }
        private async void SelectImage(object sender, RoutedEventArgs e)
        {
            var filesPointer = ViewModel.UploadFileContents;
            switch ((sender as Button).DataContext)
            {
                case "上传指令内容图片":
                    filesPointer = ViewModel.UploadFileContents;
                    break;
                case "上传指令详情图片":
                    filesPointer = ViewModel.UploadFileDetails;
                    break;
                case "上传截图":
                    filesPointer = ViewModel.UploadBaiduActions;
                    break;
            }
            FileOpenPicker open = new FileOpenPicker();
            open.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            open.ViewMode = PickerViewMode.Thumbnail;
            // Filter to include a sample subset of file types
            open.FileTypeFilter.Clear();
            open.FileTypeFilter.Add("*");
            //open.FileTypeFilter.Add(".png");
            //open.FileTypeFilter.Add(".jpeg");
            //open.FileTypeFilter.Add(".jpg");

            // Open a stream for the selected file
            IReadOnlyList<StorageFile> files = await open.PickMultipleFilesAsync();
            // Ensure a file was selected
            if (files != null && files.Count>0)
            {
                foreach (var file in files)
                {
                    using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read))
                    {
                        BitmapImage bitmapImage = new BitmapImage();
                        await bitmapImage.SetSourceAsync(fileStream);
                        var tempbitmap = new UploadImage()
                        {
                            ImageSource = bitmapImage,
                            UplodFile = file,
                            ImageTitle = file.Name,
                            contentType = file.ContentType,
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
        
    }
}
