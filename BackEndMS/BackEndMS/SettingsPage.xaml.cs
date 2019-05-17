﻿//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************
using BackEndMS.Helpers;
using System;
using System.Linq;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace BackEndMS
{
    /// <summary>
    /// A page that displays the app's settings.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        
        public SettingsPage()
        {
            this.InitializeComponent();
            Loaded += OnSettingsPageLoaded;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

           // MainPage.Current..Header = "Settings";
        }

        private void OnSettingsPageLoaded(object sender, RoutedEventArgs e)
        {
            var currentTheme = App.RootTheme.ToString();
            (ThemePanel.Children.Cast<RadioButton>().FirstOrDefault(c => c?.Tag?.ToString() == currentTheme)).IsChecked = true;
            WebAPiPanel.Children.Cast<RadioButton>().FirstOrDefault(x=>x?.Content?.ToString() == App.RootBaseUri).IsChecked = true;
        }

        private void OnThemeRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            var selectedTheme = ((RadioButton)sender)?.Tag?.ToString();

            if (selectedTheme != null)
            {
                App.RootTheme = App.GetEnum<ElementTheme>(selectedTheme);
            }
        }

        private void OnWebAPiRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            var selectBaseUri = ((RadioButton)sender)?.Content?.ToString();
            if(selectBaseUri !=null)
            {
                App.RootBaseUri = selectBaseUri;
            }
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
                Frame.GoBack();
        }
    }
}