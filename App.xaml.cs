using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using System;
using System.Threading;

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
    public partial class App : Application
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Application.Start((p) =>
            {
                var app = new App();
            });
        }

        public App()
        {
            this.InitializeComponent();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            var window = new MainWindow();
            window.Activate();
        }
    }
}

