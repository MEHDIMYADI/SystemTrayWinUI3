// -----------------------------------------------------------------------------
// SystemTray for WinUI 3
// A complete system tray (notification area) implementation for WinUI 3 apps.
//
// Repository: https://github.com/MEHDIMYADI
// Author: Mehdi Dimyadi
// License: MIT
// -----------------------------------------------------------------------------

using System;

namespace SystemTray.Interfaces
{
    public interface IIconFile : IDisposable
    {
        nint Handle { get; }
    }
}