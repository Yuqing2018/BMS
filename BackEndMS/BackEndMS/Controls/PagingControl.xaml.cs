using BackEndMS.Helpers;
using System;
using System.Collections.Generic;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace BackEndMS.Controls
{
    public sealed partial class PagingControl : UserControl
    {
        public event ItemClickEventHandler JumpClicked = delegate { };
        //public event ItemClickEventHandler ExportClicked;
        public List<int> pageSizeList = new List<int>()
        {
            20,40,60,80,100,200
        };
        public BaseSearch searchModel { get; set; }
        public PagingControl()
        {
            this.InitializeComponent();
            searchModel = new BaseSearch();
        }

        private void firstPage_Click(object sender, RoutedEventArgs e)
        {
            searchModel.PageIndex = 0;
            JumpClicked?.Invoke(sender, null);
        }

        private void nextPage_Click(object sender, RoutedEventArgs e)
        {
            //NextClicked?.Invoke(sender,null);
            searchModel.PageIndex++;
            JumpClicked?.Invoke(sender, null);
        }

        private void previousPage_Click(object sender, RoutedEventArgs e)
        {
            searchModel.PageIndex--;
            JumpClicked?.Invoke(sender, null);
        }

        private void lastPage_Click(object sender, RoutedEventArgs e)
        {
            searchModel.PageIndex = searchModel.PageCount-1;
            JumpClicked?.Invoke(sender, null);
        }

        private void jump_Click(object sender, RoutedEventArgs e)
        {
            JumpClicked?.Invoke(sender, null);
        }

        //private void export_Click(object sender, RoutedEventArgs e)
        //{
        //    searchModel.ExportType = ExportType.CurrentPage;
        //    ExportClicked?.Invoke(sender, new ItemClickEventArgs());
        //}

        //private void exportAll_Click(object sender, RoutedEventArgs e)
        //{
        //    searchModel.ExportType = ExportType.AllPages;
        //    ExportClicked?.Invoke(sender, new ItemClickEventArgs());
        //}
        
        private void pageIndexTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textbox = (TextBox)sender;
            if (!System.Text.RegularExpressions.Regex.IsMatch(textbox.Text, "^\\d*\\.?\\d*$") && textbox.Text != "")
            {
                int pos = textbox.SelectionStart - 1;
                textbox.Text = textbox.Text.Remove(pos, 1);
                textbox.SelectionStart = pos;
            }
            //if (String.IsNullOrEmpty(pageIndexTb.Text))
            //    pageIndexTb.Text = "0";
            //searchModel.PageIndex = int.Parse(pageIndexTb.Text);
            //JumpClicked?.Invoke(sender, null);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (searchModel.PageIndex >= searchModel.PageCount - 1)
            {
                pageIndexTb.Text = "0";
                searchModel.PageIndex = int.Parse(pageIndexTb.Text);
            }
           // JumpClicked?.Invoke(sender, null);
        }

        private void pageIndexTb_KeyUp(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            var textbox = (TextBox)sender;
            if (!String.IsNullOrEmpty(textbox.Text))
            {
                if (e.OriginalKey == VirtualKey.Enter && e.KeyStatus.RepeatCount == 1)
                {
                    searchModel.PageIndex = int.Parse(pageIndexTb.Text);
                    JumpClicked?.Invoke(sender, null);
                }
                else if (!System.Text.RegularExpressions.Regex.IsMatch(textbox.Text, "^\\d*\\.?\\d*$"))
                {
                    int pos = textbox.SelectionStart - 1;
                    textbox.Text = textbox.Text.Remove(pos, 1);
                    textbox.SelectionStart = pos;
                }
            }
            else
            {
                textbox.Text = "0";
            }
        }
    }
}