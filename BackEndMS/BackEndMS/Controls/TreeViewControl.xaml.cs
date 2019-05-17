using BackEndMS.Helpers;
using BackEndMS.Models;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Data.Json;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace BackEndMS.Controls
{
    public sealed partial class TreeViewControl : UserControl
    {
        public ObservableCollection<SensitiveWords> TreeItems { get; set; }
        public TreeViewControl()
        {
            this.InitializeComponent();
            TreeItems = new ObservableCollection<SensitiveWords>();
        }

        private void tvDataBound_SelectedItemChanged(object sender, WinRTXamlToolkit.Controls.RoutedPropertyChangedEventArgs<object> e)
        {
            //var selectCB =  tvDataBound.SelectedContainer.ItemsPanelRoot.FindName("selectCB") as CheckBox;
            //selectCB.IsChecked = !selectCB.IsChecked;
        }
    }
}
