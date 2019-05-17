using BackEndMS.Models;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace BackEndMS.Controls
{
    public sealed partial class UserTableControl : UserControl
    {
        public event ItemClickEventHandler EditClicked = delegate { };
        public ObservableCollection<UserInfo> UserList { get; set; }
        public UserTableControl()
        {
            this.InitializeComponent();
            UserList = new ObservableCollection<UserInfo>();
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
            int count = gridview.Items.Where(x => (x as UserInfo).selected).Count();
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
