using BackEndMS.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
    public sealed partial class KeywordsManagerTableControl : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public event ItemClickEventHandler EditClicked = delegate { };
        private double _KeywordMinWidth;
        public double KeywordMinWidth {
            get {
                return _KeywordMinWidth;
            }
            set {
                _KeywordMinWidth = value;
                if(null != PropertyChanged)
                NotifyPropertyChange("KeywordMinWidth");
            } }
        public ObservableCollection<BlockEntry> viewModel
        {
            get { return (ObservableCollection<BlockEntry>)GetValue(viewModelProperty); }
            set {
                SetValue(viewModelProperty, value);
                if (null != PropertyChanged)
                    NotifyPropertyChange("viewModel");
            }
        }

        // Using a DependencyProperty as the backing store for viewModel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty viewModelProperty =
            DependencyProperty.Register("viewModel", typeof(ObservableCollection<BlockEntry>), typeof(ObservableCollection<BlockEntry>), new PropertyMetadata(null));

        private void NotifyPropertyChange([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public KeywordsManagerTableControl()
        {
            this.InitializeComponent();
        }

        private void Modify_Click(object sender, RoutedEventArgs e)
        {
            // BlockEntry block = (sender as Button).DataContext as BlockEntry;
            EditClicked?.Invoke(sender, null);
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
            int count = gridview.Items.Where(x => (x as BlockEntry).selected).Count();
            if (count == gridview.Items.Count)
                selectAllCB.IsChecked = true;
            else if (count > 0)
                selectAllCB.IsChecked = null;
            else
                selectAllCB.IsChecked = false;
        }
    }
}
