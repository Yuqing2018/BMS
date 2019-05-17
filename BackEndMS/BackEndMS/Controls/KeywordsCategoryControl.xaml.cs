using BackEndMS.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace BackEndMS.Controls
{
    public sealed partial class KeywordsCategoryControl : UserControl
    {
        public List<string> deleteIds { get; set; }
        public event ItemClickEventHandler SubmitClick = delegate { };
        public ObservableCollection<BlockQueryCategory> CategoryList { get; set; }
        public ObservableCollection<BlockQueryCategory> Class1List { get; set; }
        public ObservableCollection<BlockQueryCategory> Class2List { get; set; }
        public ObservableCollection<BlockQueryCategory> Class3List { get; set; }
        public KeywordsCategoryControl()
        {
            this.InitializeComponent();
            CategoryList = new ObservableCollection<BlockQueryCategory>();
            Class1List = new ObservableCollection<BlockQueryCategory>();
            Class2List = new ObservableCollection<BlockQueryCategory>();
            Class3List = new ObservableCollection<BlockQueryCategory>();
        }
        public KeywordsCategoryControl(ObservableCollection<BlockQueryCategory> list)
        {
            this.InitializeComponent();
            CategoryList = list;
            Class1List = new ObservableCollection<BlockQueryCategory>();
            Class2List = new ObservableCollection<BlockQueryCategory>();
            Class3List = new ObservableCollection<BlockQueryCategory>();
            CategoryList.ToList().ForEach(item => { Class1List.Add(item); });
        }
        public async void EditClass1Btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(null == deleteIds)
                    deleteIds = new List<string>();
                ActiveProgressRing();
                var text = String.Join(Environment.NewLine, firstCategory.Items.Select(x => (x as BlockQueryCategory).Name));
                CategoryContentDialog dialog = new CategoryContentDialog(text, "一级分类");
                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    var currentExists = CategoryList.Select(x => x.Name).ToList();
                    var keywordsList = dialog.TextString.Split(new char[] { '\r' }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToList();
                    keywordsList.ForEach(item =>
                    {
                        if (!currentExists.Contains(item))
                            CategoryList.Add(new BlockQueryCategory() { Name = item, Children = new List<BlockQueryCategory>() });
                    });
                    var deleteList = currentExists.Except(keywordsList);
                    var removeList = CategoryList.Where(x => deleteList.Contains(x.Name)).ToList();
                    for (int i = 0; i < removeList.Count; i++)
                    {
                        var isDelete = await WarningTip(removeList[i]);
                        if (isDelete)
                        {
                            deleteIds.Add(removeList[i].Id);
                            if (removeList[i].Children != null && removeList[i].Children.Count > 0)
                            {
                                Class2List.Clear();
                                Class3List.Clear();
                            }
                            CategoryList.Remove(removeList[i]);
                        }
                    }
                    Class1List.Clear();
                    CategoryList.ToList().ForEach(item => { Class1List.Add(item); });
                   // firstCategory.ItemsSource = CategoryList;
                    //Parallel.ForEach(removeList, async item =>
                    //{
                    //    var isDelete = await WarningTip(item);
                    //    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher
                    //    .RunAsync(CoreDispatcherPriority.Normal, () =>
                    //    {
                    //        if (isDelete)
                    //        {
                    //            deleteIds.Add(item._id);
                    //            CategoryList.Remove(item);
                    //        }
                    //    });
                    //});
                    // Thread.Sleep(1000 * CategoryList.Count);
                    // firstCategory.SelectedItem = firstCategory.Items?.FirstOrDefault();
                }

            }
            catch (Exception ex)
            {
                await MainPage.ShowErrorMessage(ex.Message);
            }
            finally
            {
                InActiveProgressRing();
            }
        }
        private async Task<bool> WarningTip(BlockQueryCategory item)
        {
            var flag = false;
            if (item.Children != null && item.Children.Count > 0)
            {
                var dialog = new ContentDialog()
                {
                    Title = "Warning!",
                    Content = new TextBlock()
                    {
                        Text = item.Name + "下级条目，确定要移除该一级分类吗？",
                        TextWrapping = TextWrapping.Wrap,
                    },
                    PrimaryButtonText = "删除",
                    SecondaryButtonText = "保留",
                };

                dialog.PrimaryButtonClick += delegate
                {
                    flag = true;
                };
                await dialog.ShowAsync();
            }
            else
            {
                flag = true;
            }
            return flag;
        }
        
        private async void EditClass2Btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ActiveProgressRing();
                if (null == firstCategory.SelectedValue)
                    return;
                var secondCategoryList = CategoryList.Where(x => x.Name == firstCategory.SelectedValue.ToString()).FirstOrDefault().Children;
                var currentExists = secondCategoryList.Select(x => x.Name).ToList();
                var text = String.Join(Environment.NewLine, secondCategory.Items.Select(x => (x as BlockQueryCategory).Name));
                CategoryContentDialog dialog = new CategoryContentDialog(text, "二级分类");
                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    var keywordsList = dialog.TextString.Split(new char[] { '\r' }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToList();
                    keywordsList.ForEach(item =>
                    {
                        if (!currentExists.Contains(item))
                            secondCategoryList.Add(new BlockQueryCategory() { Name = item, Children = new List<BlockQueryCategory>() });
                    });
                    var deleteList = currentExists.Except(keywordsList);
                    var removeList = secondCategoryList.Where(x => deleteList.Contains(x.Name)).ToList();
                    for (int i = 0; i < removeList.Count; i++)
                    {
                        var isDelete = await WarningTip(removeList[i]);
                        if (isDelete)
                        {
                            secondCategoryList.Remove(removeList[i]);
                        }
                    }
                    secondCategory.ItemsSource = null;
                    Class2List.Clear();
                    secondCategoryList.ToList().ForEach(item => { Class2List.Add(item); });
                    secondCategory.ItemsSource = Class2List;
                    Class3List.Clear();
                    // secondCategory.ItemsSource = secondCategoryList;
                    //Parallel.ForEach(removeList, async item =>
                    //{
                    //    var isDelete = await WarningTip(item);
                    //    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher
                    //    .RunAsync(CoreDispatcherPriority.Normal, () =>
                    //    {

                    //        if (isDelete)
                    //        {
                    //            deleteIds.Add(item._id);
                    //            secondCategoryList.Remove(item);
                    //        }
                    //    });
                    //});
                    // Thread.Sleep(1000 * secondCategoryList.Count);
                    //removeList.ForEach(item =>
                    //{
                    //    var delete = WarningTip(secondCategoryList.ToList(), item);
                    //});
                    //secondCategory.SelectedItem = secondCategory.Items?.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                await MainPage.ShowErrorMessage(ex.Message);
            }
            finally
            {
                InActiveProgressRing();
            }
        }
        private async void EditClass3Btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ActiveProgressRing();
                if (null == firstCategory.SelectedValue || null == secondCategory.SelectedValue)
                    return;
                var secondCategoryList = CategoryList.Where(x => x.Name == firstCategory.SelectedValue.ToString()).FirstOrDefault().Children;
                var thirdCategoryList = secondCategoryList.Where(x => x.Name == secondCategory.SelectedValue.ToString()).FirstOrDefault().Children;
                var currentExists = thirdCategoryList.Select(x => x.Name).ToList();
                var text = String.Join(Environment.NewLine, thirdCategory.Items.Select(x => (x as BlockQueryCategory).Name));
                CategoryContentDialog dialog = new CategoryContentDialog(text, "三级分类");
                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    var keywordsList = dialog.TextString.Trim('\r').Split(new char[] { '\r' }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToList();
                    keywordsList.ForEach(item =>
                    {
                        if (!currentExists.Contains(item))
                            thirdCategoryList.Add(new BlockQueryCategory() { Name = item, Children = null });
                    });
                    var deleteList = currentExists.Except(keywordsList);
                    var removeList = thirdCategoryList.Where(x => deleteList.Contains(x.Name)).ToList();
                    removeList.ForEach(item =>
                    {
                        thirdCategoryList.Remove(item);
                    });
                    thirdCategory.ItemsSource = null;
                    Class3List.Clear();
                    thirdCategoryList.ToList().ForEach(item => { Class3List.Add(item); });
                    thirdCategory.ItemsSource = Class3List;
                    // Thread.Sleep(1000* thirdCategoryList.Count);
                    // thirdCategory.SelectedItem = thirdCategory.Items?.FirstOrDefault();
                    //thirdCategory.ItemsSource = thirdCategoryList;
                }
            }
            catch (Exception ex)
            {
                await MainPage.ShowErrorMessage(ex.Message);
            }
            finally
            {
                InActiveProgressRing();
            }

        }
        private void submitBtn_Click(object sender, RoutedEventArgs e)
        {
            SubmitClick?.Invoke(sender, null);
        }

        Popup popup;
        public void Show()
        {
            var appView = ApplicationView.GetForCurrentView();

            popup = new Popup();
            //popup.HorizontalOffset = appView.VisibleBounds.Width;
            //popup.VerticalOffset = appView.VisibleBounds.Height;
            popup.Child = this;
            this.Height = appView.VisibleBounds.Height;
            this.Width = appView.VisibleBounds.Width;
            this.contentGrid.Width = this.Width / 3;
            this.contentGrid.Height = this.Height / 3;
            EventHandler<Windows.UI.Core.BackRequestedEventArgs> PageNavHelper_BeforeBackRequest = (s, e) =>
            {
                if (popup.IsOpen)
                {
                    e.Handled = true;
                    popup.IsOpen = false;
                }
            };

            TypedEventHandler<ApplicationView, object> handler = (s, e) =>
            {
                try
                {
                    if (popup.IsOpen)
                    {
                        //popup.HorizontalOffset = appView.VisibleBounds.Width;
                        //popup.VerticalOffset = appView.VisibleBounds.Height;
                        this.Height = appView.VisibleBounds.Height;
                        this.Width = appView.VisibleBounds.Width;
                        this.contentGrid.Width = this.Width / 3;
                        this.contentGrid.Height = this.Height / 3;
                    }
                }
                catch (Exception ex)
                {
                    MainPage.ShowErrorMessage(ex.Message);
                    return;
                }
            };

            //TappedEventHandler tapped = (s, e) =>
            //{
            //    if (popup.IsOpen)
            //    {
            //            //popup.IsOpen = false;
            //    }
            //};
            popup.Opened += (s, e) =>
            {
                this.Visibility = Visibility.Visible;
                appView.VisibleBoundsChanged += handler;
            };

            popup.Closed += (s, e) =>
            {
                this.Visibility = Visibility.Collapsed;
                appView.VisibleBoundsChanged -= handler;
            };

            popup.IsOpen = true;
        }
        public void Dispose()
        {
            if (popup.IsOpen)
                popup.IsOpen = false;
        }
        private void closeBtn_Click(object sender, RoutedEventArgs e)
        {
            popup.IsOpen = false;
        }
        public void ActiveProgressRing()
        {
            progressRing.IsActive = true;
        }
        public void InActiveProgressRing()
        {
            progressRing.IsActive = false;
        }

        private void Category_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var combobox = (sender as ComboBox);
            switch(combobox.Name)
            {
                case "firstCategory":
                    if(firstCategory.SelectedItem!=null)
                        secondCategory.ItemsSource = (firstCategory.SelectedItem as BlockQueryCategory).Children;
                    break;
                case "secondCategory":
                    if (secondCategory.SelectedItem != null)
                        thirdCategory.ItemsSource = (secondCategory.SelectedItem as BlockQueryCategory).Children;
                    break;
            }
        }
    }
}
