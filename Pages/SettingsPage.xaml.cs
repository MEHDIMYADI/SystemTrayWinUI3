using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SystemTray.Core;

// -----------------------------------------------------------------------------
// SystemTray for WinUI 3
// A complete system tray (notification area) implementation for WinUI 3 apps.
//
// Repository: https://github.com/MEHDIMYADI
// Author: Mehdi Dimyadi
// License: MIT
// -----------------------------------------------------------------------------

namespace SystemTrayWinUI3.Pages
{
    public sealed partial class SettingsPage : Page
    {
        private SystemTrayManager? systemTrayManager;

        public SettingsPage()
        {
            this.InitializeComponent();
        }

        public void SetManager(SystemTrayManager manager)
        {
            systemTrayManager = manager;
        }

        private void TrayIconToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (systemTrayManager != null)
            {
                systemTrayManager.IsIconVisible = TrayIconToggle.IsOn;
                if (TrayIconToggle.IsOn == false)
                {
                    systemTrayManager.CloseButtonMinimizesToTray = false;
                    CloseButtonTrayToggle.IsOn = false;
                }
                MinimizeToTrayToggle.IsEnabled = TrayIconToggle.IsOn;
                CloseButtonTrayToggle.IsEnabled = TrayIconToggle.IsOn;
            }
        }

        private void MinimizeToTrayToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (systemTrayManager != null)
            {
                systemTrayManager.MinimizeToTray = MinimizeToTrayToggle.IsOn;
            }
        }

        private void CloseToMinimizesInTrayToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (systemTrayManager != null)
            {
                systemTrayManager.CloseButtonMinimizesToTray = CloseButtonTrayToggle.IsOn;
            }
        }

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LanguageComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                var selectedCode = selectedItem.Tag?.ToString();
                if (!string.IsNullOrEmpty(selectedCode))
                {
                    if (systemTrayManager != null)
                    {
                        systemTrayManager.LanguageCode = selectedCode;
                        systemTrayManager.RefreshContextMenu();
                    }
                }
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is SystemTrayManager manager)
            {
                systemTrayManager = manager;

                TrayIconToggle.IsOn = systemTrayManager.IsIconVisible;
                MinimizeToTrayToggle.IsOn = systemTrayManager.MinimizeToTray;
                CloseButtonTrayToggle.IsOn = systemTrayManager.CloseButtonMinimizesToTray;
            }
        }
    }
}