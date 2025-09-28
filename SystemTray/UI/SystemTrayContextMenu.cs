using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using SystemTray.Models;

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
    class SystemTrayContextMenu : IDisposable
    {
        private SafeMenuHandle handle;

        private readonly List<MenuItem> items = new();

        public SystemTrayContextMenu()
        {
            handle = new SafeMenuHandle(CreatePopupMenu(), true);
        }

        public SystemTrayContextMenuItem this[int index] => items[index];

        public int Count => items.Count;

        public void Show(HWND hWnd, int x, int y)
        {
            var activeWindow = GetForegroundWindow();
            SetForegroundWindow(hWnd);
            try
            {
                var command = TrackPopupMenuEx(
                    handle,
                    TPM_RETURNCMD | TPM_NONOTIFY,
                    x,
                    y,
                    hWnd,
                    nint.Zero);

                if (command == 0) return;

                var item = items.FirstOrDefault(i => i.Id == (uint)command);
                item?.PerformClick();
            }
            finally
            {
                SetForegroundWindow(activeWindow);
            }
        }

        public SystemTrayContextMenuItem AddMenuItem(string text)
        {
            var item = new MenuItem(this)
            {
                Text = text
            };

            var flags = MF_STRING;
            if (text == "--")
                flags |= MF_SEPARATOR;

            AppendMenu(handle, flags, item.Id, text);

            items.Add(item);

            return item;
        }

        public void Dispose()
        {
            handle?.Dispose();
        }

        private void UpdateMenuItem(MenuItem item)
        {
            var flags = MF_STRING;
            if (item.Text == "--") flags |= MF_SEPARATOR;
            if (!item.IsEnabled) flags |= MF_DISABLED;

            ModifyMenu(handle, item.Id, flags, item.Id, item.Text);
        }

        sealed class MenuItem : SystemTrayContextMenuItem
        {
            private static uint idCount = 100;
            private SystemTrayContextMenu menu;
            private string text = string.Empty;
            private bool isEnabled = true;

            public MenuItem(SystemTrayContextMenu menu)
            {
                ArgumentNullException.ThrowIfNull(menu);
                this.menu = menu;
                Id = ++idCount;
            }

            public uint Id { get; }

            public override string Text
            {
                get => text;
                set
                {
                    if (text == value) return;
                    text = value ?? string.Empty;
                    menu.UpdateMenuItem(this);
                }
            }

            public override bool IsEnabled
            {
                get => isEnabled;
                set
                {
                    if (isEnabled == value) return;
                    isEnabled = value;
                    menu.UpdateMenuItem(this);
                }
            }

            public void PerformClick()
            {
                Click?.Invoke(this, EventArgs.Empty);
            }
        }

        #region Win32 Interop

        [StructLayout(LayoutKind.Sequential)]
        public struct HWND { public nint Value; public HWND(nint value) => Value = value; public static implicit operator nint(HWND h) => h.Value; public static implicit operator HWND(nint h) => new HWND(h); }

        private sealed class SafeMenuHandle : SafeHandle
        {
            public SafeMenuHandle(nint handle, bool ownsHandle) : base(nint.Zero, ownsHandle) => SetHandle(handle);
            public override bool IsInvalid => handle == nint.Zero;
            protected override bool ReleaseHandle() => DestroyMenu(handle);
        }

        private const uint MF_STRING = 0x00000000;
        private const uint MF_SEPARATOR = 0x00000800;
        private const uint MF_DISABLED = 0x00000002;
        private const uint TPM_RETURNCMD = 0x0100;
        private const uint TPM_NONOTIFY = 0x0080;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern nint CreatePopupMenu();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool AppendMenu(SafeMenuHandle hMenu, uint uFlags, uint uIDNewItem, string lpNewItem);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool ModifyMenu(SafeMenuHandle hMenu, uint uPosition, uint uFlags, uint uIDNewItem, string lpNewItem);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyMenu(nint hMenu);

        [DllImport("user32.dll")]
        private static extern int TrackPopupMenuEx(SafeMenuHandle hMenu, uint uFlags, int x, int y, HWND hwnd, nint lptpm);

        [DllImport("user32.dll")]
        private static extern HWND GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(HWND hWnd);

        #endregion
    }
}
