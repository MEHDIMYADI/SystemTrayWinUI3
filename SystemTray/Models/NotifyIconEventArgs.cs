using System;
using Windows.Foundation;

// -----------------------------------------------------------------------------
// SystemTray for WinUI 3
// A complete system tray (notification area) implementation for WinUI 3 apps.
//
// Repository: https://github.com/MEHDIMYADI
// Author: Mehdi Dimyadi
// License: MIT
// -----------------------------------------------------------------------------

namespace SystemTray.Models
{
    public class NotifyIconEventArgs : EventArgs
    {
        public Rect Rect { get; init; }
    }
}