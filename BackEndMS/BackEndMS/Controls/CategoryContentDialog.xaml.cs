using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace BackEndMS.Controls
{
    public sealed partial class CategoryContentDialog : ContentDialog
    {
        public String  TextString{get;set;}
        public CategoryContentDialog(String result,String DialogTitle)
        {
            this.InitializeComponent();
            Title = DialogTitle;
            EditCategoryContent.Document.SetText(Windows.UI.Text.TextSetOptions.ApplyRtfDocumentDefaults,result);
        }
      
        public void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var result = "";
            this.EditCategoryContent.Document.GetText(Windows.UI.Text.TextGetOptions.AdjustCrlf, out result);
            TextString =  result;
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }
    }
}
