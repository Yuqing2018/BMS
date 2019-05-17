using BackEndMS.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace BackEndMS
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Query_MainPage : Page
    {
        public static BaseSearchModel BaseSearch { get; set; }

        public ObservableCollection<ModelPivotItem> TabList { get; set; }
        public Query_MainPage()
        {
            this.InitializeComponent();
            if(BaseSearch == null)
                BaseSearch = new BaseSearchModel();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if(e.Parameter is List<ModelPivotItem>)
            {
                TabList = (e.Parameter as List<ModelPivotItem>).ToObservableCollectionAsync();
            }
        }

        private void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (ModelPivotItem item in TabList)
            {
                if (e.AddedItems.Contains(item))
                    item.isSelected = true;
                else
                    item.isSelected = false;
            }
            var root = (pivot.ContainerFromIndex(pivot.SelectedIndex) as PivotItem).ContentTemplateRoot as Frame;
            if (root != null && root.CurrentSourcePageType != typeof(QueryContent))
                root.Navigate(root.CurrentSourcePageType);
        }

        private void pivotContentFrame_Loaded(object sender, RoutedEventArgs e)
        {
            Frame currentFrame = sender as Frame;
            Type type = ((ModelPivotItem)pivot.SelectedItem).CalssType;
            currentFrame.Navigate(type);
        }
    }
}


