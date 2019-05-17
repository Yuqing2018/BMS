using BackEndMS.Models;
using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace BackEndMS
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class QueryRecordIndex : Page
    {
        public PageQuery viewModel { get; set; }
        public QueryRecordIndex()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if(e.Parameter!=null && e.Parameter is PageQuery)
            {
                this.viewModel = e.Parameter as PageQuery;
                this.rsTableControl.ViewModel = this.viewModel;
                this.keywordsCB.ItemsSource = this.viewModel.Querys.Select(x => x.Keywords).Distinct().ToList();
                this.keywordsCB.SelectedIndex =0;
                this.flagCB.ItemsSource = Enum.GetValues(typeof(QueryEntryFlag)).Cast<QueryEntryFlag>().ToList();
                this.flagCB.SelectedIndex = 0;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
                Frame.GoBack();
            //else if (this.Frame.CanGoForward)
            //    this.Frame.GoForward();
        }

        private void copyBtn_Click(object sender, RoutedEventArgs e)
        {
            var copyContent = "";
            DataPackage dataPackage = new DataPackage();
            dataPackage.RequestedOperation = DataPackageOperation.Copy;
            var contentArray = viewModel.Querys.Where(x => x.Keywords == keywordsCB.SelectedItem.ToString() && x.Flag == (QueryEntryFlag)flagCB.SelectedValue).Select(x => x.Content).ToArray();
            copyContent = String.Join(Environment.NewLine, contentArray);
            dataPackage.SetText(copyContent);
            Clipboard.SetContent(dataPackage);
        }
    }
}
