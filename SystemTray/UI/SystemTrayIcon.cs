using System;
using System.Runtime.InteropServices;
using SystemTray.Core;
using SystemTray.Interfaces;
using SystemTray.Models;
using Windows.Foundation;

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
    internal class SystemTrayIcon : IDisposable
    {
        private const int WM_LBUTTONUP = 0x0202;
        private const int WM_RBUTTONUP = 0x0205;
        private const int WM_CONTEXTMENU = 0x007B;

        private const uint MESSAGE_ID = 5800;
        private static readonly uint WM_TASKBARCREATED = RegisterWindowMessage("TaskbarCreated");

        private readonly WindowHelper windowHelper;

        private string? text;
        private IIconFile? icon;
        private bool keepIconAlive;

        public SystemTrayIcon(WindowHelper windowHelper, bool keepIconAlive = false)
        {
            ArgumentNullException.ThrowIfNull(windowHelper);

            this.windowHelper = windowHelper;
            this.keepIconAlive = keepIconAlive;

            ContextMenu = new SystemTrayContextMenu();

            this.windowHelper.Message += ProcessMessage;
        }

        ~SystemTrayIcon() => Dispose(false);

        public bool IsVisible { get; private set; }

        public Guid Id { get; init; }

        public string Text
        {
            get => text ?? string.Empty;
            set
            {
                if (text == value) return;
                text = value;
                Update();
            }
        }

        public IIconFile? Icon
        {
            get => icon;
            set
            {
                if (icon != null && !keepIconAlive)
                    icon.Dispose();

                icon = value;
                Update();
            }
        }

        public SystemTrayContextMenu ContextMenu { get; }

        public event EventHandler<SystemTrayEventArgs>? LeftClick;
        public event EventHandler<SystemTrayEventArgs>? RightClick;

        public void Show()
        {
            var data = GetData();
            if (Shell_NotifyIcon(NIM_ADD, ref data))
                IsVisible = true;
        }

        public void Hide()
        {
            var data = GetData();
            if (Shell_NotifyIcon(NIM_DELETE, ref data))
                IsVisible = false;
        }

        private void Update()
        {
            if (IsVisible)
            {
                var data = GetData();
                Shell_NotifyIcon(NIM_MODIFY, ref data);
            }
        }

        private unsafe NOTIFYICONDATAW GetData()
        {
            var data = new NOTIFYICONDATAW
            {
                cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATAW>(),
                hWnd = windowHelper.Handle,
                uID = 0,
                uFlags = NIF_TIP | NIF_MESSAGE | NIF_GUID | NIF_ICON,
                uCallbackMessage = MESSAGE_ID,
                hIcon = Icon?.Handle ?? nint.Zero,
                guidItem = Id,
                szTip = Text
            };
            data.uVersion = 5;
            return data;
        }

        private void OnLeftClick(SystemTrayEventArgs e) => LeftClick?.Invoke(this, e);
        private void OnRightClick(SystemTrayEventArgs e) => RightClick?.Invoke(this, e);

        private void Dispose(bool disposing)
        {
            Hide();
            if (icon != null && !keepIconAlive)
                icon.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void ProcessMessage(uint messageId, nuint wParam, nint lParam)
        {
            if (messageId == WM_TASKBARCREATED)
            {
                Show();
                return;
            }

            if (messageId != MESSAGE_ID)
                return;

            switch ((int)lParam)
            {
                case WM_LBUTTONUP:
                    OnLeftClick(new SystemTrayEventArgs { Rect = GetIconRectangle() });
                    break;

                case WM_CONTEXTMENU:
                case WM_RBUTTONUP:
                    if (RightClick == null)
                        ShowContextMenu();
                    else
                        OnRightClick(new SystemTrayEventArgs { Rect = GetIconRectangle() });
                    break;
            }
        }

        private void ShowContextMenu()
        {
            var rect = GetIconRectangle();
            ContextMenu.Show(
                windowHelper.Handle,
                (int)(rect.Left + rect.Right) / 2,
                (int)(rect.Top + rect.Bottom) / 2);
        }

        private Rect GetIconRectangle()
        {
            var systemTray = new NOTIFYICONIDENTIFIER
            {
                cbSize = (uint)Marshal.SizeOf<NOTIFYICONIDENTIFIER>(),
                hWnd = windowHelper.Handle,
                guidItem = Id
            };

            return Shell_NotifyIconGetRect(ref systemTray, out RECT rect) != 0
                ? Rect.Empty
                : new Rect(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
        }

        // ============ Interop section ============

        private const int NIM_ADD = 0x00000000;
        private const int NIM_MODIFY = 0x00000001;
        private const int NIM_DELETE = 0x00000002;

        private const int NIF_MESSAGE = 0x00000001;
        private const int NIF_ICON = 0x00000002;
        private const int NIF_TIP = 0x00000004;
        private const int NIF_GUID = 0x00000020;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern uint RegisterWindowMessage(string lpString);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool Shell_NotifyIcon(int dwMessage, ref NOTIFYICONDATAW lpData);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int Shell_NotifyIconGetRect(ref NOTIFYICONIDENTIFIER identifier, out RECT iconLocation);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct NOTIFYICONDATAW
        {
            public uint cbSize;
            public nint hWnd;
            public uint uID;
            public uint uFlags;
            public uint uCallbackMessage;
            public nint hIcon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szTip;
            public uint dwState;
            public uint dwStateMask;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string szInfo;
            public uint uTimeoutOrVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string szInfoTitle;
            public uint dwInfoFlags;
            public Guid guidItem;
            public nint hBalloonIcon;

            // helper field
            public uint uVersion;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NOTIFYICONIDENTIFIER
        {
            public uint cbSize;
            public nint hWnd;
            public uint uID;
            public Guid guidItem;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }
    }
}
