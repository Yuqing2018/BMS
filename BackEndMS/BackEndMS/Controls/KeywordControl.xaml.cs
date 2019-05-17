using BackEndMS.Helpers;
using BackEndMS.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.UI.Text;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace BackEndMS.Controls
{
    public sealed partial class KeywordControl : UserControl
    {
        public event ItemClickEventHandler AddRecordClick = delegate { };
        public BlockEntryCreateModel ViewModel { get; set; }
        public KeywordControl()
        {
            this.InitializeComponent();
            ViewModel = new BlockEntryCreateModel();
            ViewModel.BlockEntry.Operator = MainPage.Current.Userinfo.Username;
        }
        
        public KeywordControl(BlockEntry block)
        {
            this.InitializeComponent();
            ViewModel = new BlockEntryCreateModel(block);
            if(String.IsNullOrEmpty(ViewModel.BlockEntry.Operator))
                ViewModel.BlockEntry.Operator = MainPage.Current.Userinfo.Username;
            if (!string.IsNullOrEmpty(ViewModel.BlockEntry.Keywords))
                Keywords.Document.SetText(TextSetOptions.ApplyRtfDocumentDefaults, ViewModel.BlockEntry.Keywords);
        }
        Popup popup;
        public async void Show()
        {
            var appView = ApplicationView.GetForCurrentView();

            popup = new Popup();
            //popup.HorizontalOffset = appView.VisibleBounds.Width;
            //popup.VerticalOffset = appView.VisibleBounds.Height;
            popup.Child = this;
            this.Height = appView.VisibleBounds.Height;
            this.Width = appView.VisibleBounds.Width;
            //this.contentGrid.Width = this.Width / 2;
            //this.contentGrid.Height = this.Height / 2;
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
                        //this.contentGrid.Width = this.Width / 2;
                        //this.contentGrid.Height = this.Height / 2;
                    }
                }
                catch (Exception ex)
                {
                    MainPage.ShowErrorMessage(ex.Message);
                    return;
                }
            };
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

        private async void submitBtn_Click(object sender, RoutedEventArgs e)
        {
            string tempKeywords = "";
            Keywords.Document.GetText(TextGetOptions.AllowFinalEop, out tempKeywords);
            ViewModel.BlockEntry.Keywords = tempKeywords;
            AddRecordClick?.Invoke(sender, null);
        }

        private void Category_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is ComboBox)
            {
                var CategoryCombox = (sender as ComboBox);
                string value = CategoryCombox.SelectedValue?.ToString();
                CategoryCombox.SelectedItem = String.IsNullOrEmpty(value) ? CategoryCombox.Items.FirstOrDefault() :
                   CategoryCombox.Items.Where(x => (x as BlockQueryCategory).Name == value).FirstOrDefault();
            }
        }
        
    }
}
