using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using System.Runtime.InteropServices;
using WinRT.Interop;

// -----------------------------------------------------------------------------
// SystemTray for WinUI 3
// A complete system tray (notification area) implementation for WinUI 3 apps.
//
// Repository: https://github.com/MEHDIMYADI
// Author: Mehdi Dimyadi
// License: MIT
// -----------------------------------------------------------------------------

namespace SystemTray.Core
{
    public class WindowHelper : IDisposable
    {
        private readonly Window window;
        public Window Window => window;
        private readonly WndProc windowProc, nativeWindowProc;
        public bool IsIconVisible { get; set; }
        public bool CloseButtonMinimizesToTray { get; set; }
        public event Action? CloseButtonPressed;

        public WindowHelper(Window window)
        {
            ArgumentNullException.ThrowIfNull(window);
            this.window = window;

            windowProc = new WndProc(WindowProc);
            var hWnd = new HWND(WindowNative.GetWindowHandle(this.window));

            var proc = SetWindowLongPtr(hWnd, GWL_WNDPROC, Marshal.GetFunctionPointerForDelegate(windowProc));
            nativeWindowProc = Marshal.GetDelegateForFunctionPointer<WndProc>(proc);
        }

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(nint hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(nint hWnd);

        private const int SW_RESTORE = 9;

        public void ShowWindowFromTray()
        {
            var appWindow = this.AppWindow;
            if (appWindow == null) return;

            var hwnd = this.Handle;

            appWindow.Show();
            ShowWindow(hwnd, SW_RESTORE);
            SetForegroundWindow(hwnd);
        }

        public void HideWindowToTray()
        {
            var appWindow = this.AppWindow;
            if (appWindow == null) return;

            appWindow.Hide();
        }

        ~WindowHelper() => Dispose(false);

        public nint Handle => new HWND(WindowNative.GetWindowHandle(window));

        public AppWindow AppWindow
        {
            get
            {
                var hwnd = WindowNative.GetWindowHandle(window);
                var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
                return AppWindow.GetFromWindowId(windowId);
            }
        }

        public bool IsDisposed { get; private set; } = false;

        public event Action<uint, nuint, nint>? Message;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                SetWindowLongPtr(new HWND(WindowNative.GetWindowHandle(window)),
                                 GWL_WNDPROC,
                                 Marshal.GetFunctionPointerForDelegate(nativeWindowProc));
                IsDisposed = true;
            }
        }

        private nint WindowProc(nint hWnd, uint msg, nint wParam, nint lParam)
        {
            if (msg == WM_CLOSE)
            {
                if (CloseButtonMinimizesToTray)
                {
                    AppWindow?.Hide();
                    CloseButtonPressed?.Invoke();
                    return 0;
                }

                CloseButtonPressed?.Invoke();
                return CallWindowProc(nativeWindowProc, hWnd, msg, wParam, lParam);
            }

            Message?.Invoke(msg, (nuint)wParam, lParam);
            return CallWindowProc(nativeWindowProc, hWnd, msg, wParam, lParam);
        }

        // ----------- Interop Section -----------

        private const int GWL_WNDPROC = -4;
        private const uint WM_CLOSE = 0x0010;

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        private delegate nint WndProc(nint hWnd, uint msg, nint wParam, nint lParam);

        [StructLayout(LayoutKind.Sequential)]
        public readonly struct HWND
        {
            public readonly nint Value;
            public HWND(nint value) => Value = value;
            public static implicit operator nint(HWND h) => h.Value;
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
        private static extern nint SetWindowLongPtr(HWND hWnd, int nIndex, nint dwNewLong);

        [DllImport("user32.dll", EntryPoint = "CallWindowProcW", SetLastError = true)]
        private static extern nint CallWindowProc(WndProc lpPrevWndFunc, nint hWnd, uint msg, nint wParam, nint lParam);
    }
}
