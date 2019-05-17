using BackEndMS.Models;
using DocumentFormat.OpenXml.Drawing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace BackEndMS.Controls
{
    public sealed partial class OrderTableControl : UserControl
    {
        public event ItemClickEventHandler EditClicked = delegate { };
        public ObservableCollection<Order> OrderList { get; set; }
        public OrderTableControl()
        {
            this.InitializeComponent();
            OrderList = new ObservableCollection<Order>();
        }
       
        private void selectAll_Checked(object sender, RoutedEventArgs e)
        {
            gridview.Items.ToList().ForEach(item =>
            {
                GridViewItem gi = gridview.ContainerFromItem(item) as GridViewItem;
                var cb = (gi.ContentTemplateRoot as Grid).FindName("ItemSelectCB") as CheckBox;
                cb.IsChecked = true;
            });
        }

        private void selectAll_Unchecked(object sender, RoutedEventArgs e)
        {
            gridview.Items.ToList().ForEach(item =>
            {
                GridViewItem gi = gridview.ContainerFromItem(item) as GridViewItem;
                var cb = (gi.ContentTemplateRoot as Grid).FindName("ItemSelectCB") as CheckBox;
                cb.IsChecked = false;
            });
        }
        private void ItemSelectCB_Click(object sender, RoutedEventArgs e)
        {
            int count = gridview.Items.Where(x => (x as Order).selected).Count();
            if (count == gridview.Items.Count)
                selectAllCB.IsChecked = true;
            else if (count > 0)
                selectAllCB.IsChecked = null;
            else
                selectAllCB.IsChecked = false;
        }
        private void Modify_Click(object sender, RoutedEventArgs e)
        {
            EditClicked?.Invoke(sender, null);
        }
    }
}
