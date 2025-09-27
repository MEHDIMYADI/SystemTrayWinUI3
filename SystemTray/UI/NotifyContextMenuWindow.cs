using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Windows.Input;
using System.Runtime.InteropServices;
using Windows.Foundation;
using SystemTray.Core;
using System;
using System.Collections.Generic;

// -----------------------------------------------------------------------------
// SystemTray for WinUI 3
// A complete system tray (notification area) implementation for WinUI 3 apps.
//
// Repository: https://github.com/MEHDIMYADI
// Author: Mehdi Dimyadi
// License: MIT
// -----------------------------------------------------------------------------

namespace SystemTray.UI
{
    internal class NotifyContextMenuWindow
    {
        private const uint WM_WININICHANGE = 0x001A;
        private const uint SPI_GETWORKAREA = 0x0030;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        private Window window;
        private WindowHelper helper;

        public NotifyContextMenuWindow(params Item[] menuItems)
        {
            window = new Window();
            window.Content = new ItemsControl()
            {
                IsTabStop = false,
                MinWidth = 125,
                Margin = new Thickness(4, 4, 4, 4)
            };

            var hWnd = new HWND(WinRT.Interop.WindowNative.GetWindowHandle(window));

            UseRoundCorners(hWnd);
            SetTopMost(hWnd);

            SetWindowStyle(hWnd, WindowStyle.PopupWindow);
            AddWindowStyleEx(hWnd, WindowStyleEx.Layered | WindowStyleEx.TopMost | WindowStyleEx.AppWindow);

            window.SystemBackdrop = new DesktopAcrylicBackdrop();

            window.Activated += OnWindowActivated;
            window.AppWindow.Closing += OnWindowClosing;
            window.AppWindow.IsShownInSwitchers = false;

            SetItems((window.Content as ItemsControl)!, menuItems);

            helper = new WindowHelper(window);
            helper.Message += ProcessMessage;

            UpdateTheme(ShouldSystemUseDarkMode());
        }

        public void Show(int x, int y)
        {
            var hWnd = new HWND(WinRT.Interop.WindowNative.GetWindowHandle(window));

            var workArea = GetPrimaryWorkArea();

            var scale = GetDpiForWindow(hWnd) / 96f;

            if (window.Content is FrameworkElement root)
            {
                root.UpdateLayout();
                root.Measure(new Size(workArea.Width, workArea.Height));
            }
            else
            {
                return;
            }

            window.AppWindow?.Resize(new Windows.Graphics.SizeInt32(
                (int)(root.DesiredSize.Width * scale),
                (int)(root.DesiredSize.Height * scale)));

            var size = window.AppWindow?.Size;

            window.AppWindow?.Move(new Windows.Graphics.PointInt32
            {
                X = (x + size?.Width < workArea.Width ? x : x - size?.Width) ?? x,
                Y = y - size?.Height ?? y
            });

            SetForegroundWindow(hWnd);
            IsVisible = true;
            ShowWindow(hWnd, SW_SHOW);
        }

        public void UpdateItems(IEnumerable<Item> menuItems)
        {
            if (window.Content is not ItemsControl itemsControl) return;

            itemsControl.Items.Clear();
            SetItems(itemsControl, menuItems);
        }

        public void SetFlowDirection(bool isRtl)
        {
            if (window.Content is FrameworkElement element)
            {
                element.FlowDirection = isRtl ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
            }
        }

        private void ProcessMessage(uint messageId, nuint wParam, nint lParam)
        {
            if (messageId == WM_WININICHANGE && Marshal.PtrToStringAuto(lParam) == "ImmersiveColorSet")
            {
                UpdateTheme(ShouldSystemUseDarkMode());
            }
        }

        private void OnWindowClosing(AppWindow sender, AppWindowClosingEventArgs args)
        {
            args.Cancel = true;
            window.AppWindow.Hide();
            IsVisible = false;
        }

        private void OnWindowActivated(object sender, WindowActivatedEventArgs args)
        {
            if (args.WindowActivationState != WindowActivationState.Deactivated)
            {
                return;
            }

            window.AppWindow.Hide();
        }

        private void SetItems(ItemsControl itemsControl, IEnumerable<Item> menuItems)
        {
            foreach (var menuItem in menuItems)
            {
                itemsControl.Items.Add(CreateMenuItem(menuItem));
            }
        }

        private MenuFlyoutItemBase CreateMenuItem(Item menuItem)
        {
            if (menuItem.Text == "--")
            {
                var separator = new MenuFlyoutSeparator();
                separator.IsTabStop = false;
                return separator;
            }
            else
            {
                var flyoutMenuItem = new MenuFlyoutItem
                {
                    Padding = new Thickness(12, 6, 12, 6),
                    DataContext = menuItem,
                    Text = menuItem.Text,
                    Command = menuItem.Command
                };

                flyoutMenuItem.PreviewKeyDown += OnPreviewKeyDown;
                flyoutMenuItem.Click += OnItemClick;

                return flyoutMenuItem;
            }
        }

        private void OnPreviewKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (window.Content is not ItemsControl itemsControl) return;

            var index = itemsControl.Items.IndexOf(sender);
            if (index < 0) return;

            var direction = e.Key switch
            {
                Windows.System.VirtualKey.Up => -1,
                Windows.System.VirtualKey.Down => +1,
                _ => 0
            };

            if (direction == 0) return;

            while (index + direction >= 0 && index + direction < itemsControl.Items.Count)
            {
                index += direction;
                if (itemsControl.Items[index] is MenuFlyoutItem item && item.Visibility == Visibility.Visible)
                {
                    item.Focus(FocusState.Programmatic);
                    return;
                }
            }
        }

        private void OnItemClick(object sender, RoutedEventArgs e)
        {
            window.AppWindow.Hide();
            IsVisible = false;

            MenuClosed?.Invoke(this, EventArgs.Empty); 

            if (sender is MenuFlyoutItem x && x.DataContext is Item menuItem)
            {
                menuItem.Command?.Execute(null);
            }
        }

        private void SetWindowStyle(HWND hWnd, WindowStyle style)
        {
            SetWindowLongPtr(hWnd, GWL_STYLE, (nint)style);
        }

        private void AddWindowStyleEx(HWND hWnd, WindowStyleEx style)
        {
            var current = GetWindowLongPtr(hWnd, GWL_EXSTYLE);
            SetWindowLongPtr(hWnd, GWL_EXSTYLE, current | (nint)style);
        }

        private static void SetTopMost(HWND hWnd)
        {
            const int HWND_TOPMOST = -1;
            SetWindowPos(hWnd, new HWND(HWND_TOPMOST), 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
        }

        private static unsafe void UseRoundCorners(HWND hWnd)
        {
            const uint DWMWCP_ROUND = 2;
            uint cornerPreference = DWMWCP_ROUND;

            var result = DwmSetWindowAttribute(hWnd, DWMWA_WINDOW_CORNER_PREFERENCE, new nint(&cornerPreference), sizeof(uint));
            if (result != 0) throw Marshal.GetExceptionForHR(result) ?? new ApplicationException("Can't switch dark mode setting");
        }

        private unsafe void UpdateTheme(bool isDarkTheme)
        {
            int isDark = isDarkTheme ? 1 : 0;
            var hwnd = new HWND(WinRT.Interop.WindowNative.GetWindowHandle(window));

            DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, new nint(&isDark), sizeof(int));

            if (window.Content is FrameworkElement element)
            {
                element.RequestedTheme = isDarkTheme ? ElementTheme.Dark : ElementTheme.Light;
            }
        }

        public sealed record Item(string Text, ICommand? Command);

        static unsafe Rect GetPrimaryWorkArea()
        {
            RECT rect = new RECT();
            SystemParametersInfo(SPI_GETWORKAREA, 0, new nint(&rect), 0);
            return new Rect(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
        }

        [DllImport("UxTheme.dll", EntryPoint = "#138", SetLastError = true)]
        static extern bool ShouldSystemUseDarkMode();

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(HWND hWnd, HWND hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        static extern nint SetWindowLongPtr(HWND hWnd, int nIndex, nint dwNewLong);

        [DllImport("user32.dll")]
        static extern nint GetWindowLongPtr(HWND hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern uint GetDpiForWindow(HWND hwnd);

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(HWND hwnd);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(HWND hwnd, int nCmdShow);

        [DllImport("user32.dll")]
        static extern bool SystemParametersInfo(uint uiAction, uint uiParam, nint pvParam, uint fWinIni);

        [DllImport("dwmapi.dll")]
        static extern int DwmSetWindowAttribute(HWND hwnd, int dwAttribute, nint pvAttribute, int cbAttribute);

        private const int SW_SHOW = 5;
        private const int GWL_STYLE = -16;
        private const int GWL_EXSTYLE = -20;

        public bool IsVisible { get; private set; } = false;

        public event EventHandler? MenuClosed;

        [StructLayout(LayoutKind.Sequential)]
        struct RECT { public int left, top, right, bottom; }

        [StructLayout(LayoutKind.Sequential)]
        struct HWND { public nint Value; public HWND(nint value) => Value = value; public static implicit operator nint(HWND h) => h.Value; public static implicit operator HWND(nint h) => new HWND(h); }

        [Flags]
        enum WindowStyle : long { Border = 0x00800000, Caption = 0x00C00000, PopupWindow = 0x80880000, }
        [Flags]
        enum WindowStyleEx : long { Layered = 0x00080000, TopMost = 0x00000008, AppWindow = 0x00040000, }
    }

}