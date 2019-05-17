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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace BackEndMS.Controls
{
    public sealed partial class ImageShowControl : UserControl
    {
        public ImageShowControl(ImageSource source)
        {
            this.InitializeComponent();
            currentImage.Source = source;
        }
        Popup popup;
        public async void Show()
        {
            var appView = ApplicationView.GetForCurrentView();

            popup = new Popup();
            popup.Child = this;
            this.Height = appView.VisibleBounds.Height;
            this.Width = appView.VisibleBounds.Width;
            currentImage.MaxHeight = this.Height * 0.8;
            currentImage.MaxWidth = this.Width * 0.8;
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
                        this.Height = appView.VisibleBounds.Height;
                        this.Width = appView.VisibleBounds.Width;
                        currentImage.MaxHeight = this.Height * 0.8;
                        currentImage.MaxWidth = this.Width * 0.8;
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

        private void Canvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Dispose();
        }

        private void currentImage_Loaded(object sender, RoutedEventArgs e)
        {
            var image = (sender as Image);
            if(image.ActualHeight > image.MaxHeight || image.ActualWidth > image.MaxWidth)
            {
                image.Stretch = Stretch.UniformToFill;
            }

        }
    }
}
