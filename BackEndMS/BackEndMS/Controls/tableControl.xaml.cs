using BackEndMS.Helpers;
using BackEndMS.Models;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace BackEndMS.Controls
{
    public sealed partial class tableControl : UserControl
    {
        public event ItemClickEventHandler ChangeValidCliked = delegate { };
        public PageQuery ViewModel
        {
            get { return (PageQuery)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ViewModel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(PageQuery), typeof(tableControl), new PropertyMetadata(null));
        
        public tableControl()
        {
            this.InitializeComponent();
            if (ViewModel == null)
            {
                ViewModel = new PageQuery();
            }
            (this.Content as FrameworkElement).DataContext = this;
        }
        private async void Init()
        {
            var list = await InitResultList(ViewModel.RequestId);
        }
        private async Task<ObservableCollection<SearchUrlItem>> InitResultList(string requestID)
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFile file = await storageFolder.GetFileAsync(requestID+".txt");
            string result = await FileIO.ReadTextAsync(file, Windows.Storage.Streams.UnicodeEncoding.Utf8);
            var list = JsonConvert.DeserializeObject<ObservableCollection<SearchUrlItem>>(result);
            //var jsonRS = await JsonHelper.GetContentTextFromFile(file);
            // var list = JsonHelper.JsonDeserializer<ObservableCollection<SearchUrlItem>>(jsonRS);
            return list;
        }

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            ChangeValidCliked?.Invoke(sender, null);
        }
    }
}
