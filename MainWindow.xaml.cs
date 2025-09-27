using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SystemTray.Core;
using SystemTrayWinUI3.Pages;

// -----------------------------------------------------------------------------
// SystemTray for WinUI 3
// A complete system tray (notification area) implementation for WinUI 3 apps.
//
// Repository: https://github.com/MEHDIMYADI
// Author: Mehdi Dimyadi
// License: MIT
// -----------------------------------------------------------------------------

namespace SystemTrayWinUI3
{
    public partial class MainWindow : Window
    {
        private SystemTrayManager systemTrayManager;

        public MainWindow()
        {
            this.InitializeComponent();

            var helper = new WindowHelper(this);
            systemTrayManager = new SystemTrayManager(helper)
            {
                OpenSettingsAction = () => NavigateToSettings(),
                IsIconVisible = true,
                IconToolTip = "SystemTrayWinUI3",
                CloseButtonMinimizesToTray = false,
                LanguageCode = "en-US"
            };

            Closed += (_, _) => systemTrayManager?.Dispose();

            RootFrame.Navigate(typeof(HomePage));
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                NavigateToSettings();
                return;
            }

            if (args.SelectedItem is NavigationViewItem selectedItem)
            {
                switch (selectedItem.Tag)
                {
                    case "home":
                        RootFrame.Navigate(typeof(HomePage));
                        break;
                }
            }
        }

        public void NavigateToSettings()
        {
            if (systemTrayManager != null)
            {
                RootFrame.Navigate(typeof(SettingsPage), systemTrayManager);
            }
        }
    }
}
