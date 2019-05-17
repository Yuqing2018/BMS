using BackEndMS.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace BackEndMS
{
    public sealed partial class SensitiveWordsContentDialog : ContentDialog
    {
        public SensitiveWordsContentDialog(ObservableCollection<SensitiveWords> TreeItems)
        {
            this.InitializeComponent();
            this.treeviewControl.TreeItems = TreeItems;
            this.treeviewControl.DataContext = TreeItems;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}
