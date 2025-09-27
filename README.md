# SystemTray for WinUI 3

A comprehensive system tray (notification area) implementation for WinUI 3 applications. Since WinUI 3 doesn't have built-in system tray support, this library provides a complete solution with context menus, icons, and window management.
![SystemTrayWinUI3](https://github.com/user-attachments/assets/d337e2e5-9dac-40f7-9c29-63568a88d126)

## ⚠️ Important Note

**WinUI 3 does NOT have native system tray support.** This library implements system tray functionality using Win32 interop and provides a WinUI 3-friendly API.

## Features

- 🖱️ System tray icon with left/right click support
- 📋 Context menu with RTL language support
- 🌐 Multi-language support (English, Persian, Arabic, Urdu, Hebrew)
- 🪟 Window management (minimize to tray, close to tray)
- 🎨 Dark/light theme support
- ⌨️ Keyboard navigation support
- 🔄 Automatic taskbar recreation handling

## Installation

1. Add the source files to your WinUI 3 project
2. Ensure you have the required dependencies:
   - Microsoft.UI.Xaml
   - Microsoft.WindowsAppSDK

## Quick Start

### 1. Basic Setup

```csharp
public sealed partial class MainWindow : Window
{
    private SystemTrayManager? systemTrayManager;
    private WindowHelper windowHelper;

    public MainWindow()
    {
        this.InitializeComponent();
        
        // Initialize window helper
        windowHelper = new WindowHelper(this);
        
        // Initialize system tray manager
        systemTrayManager = new SystemTrayManager(windowHelper)
        {
            IconToolTip = "My WinUI 3 App",
            MinimizeToTray = true,
            CloseButtonMinimizesToTray = true
        };

        // Set up settings action
        systemTrayManager.OpenSettingsAction = () => 
        {
            // Navigate to your settings page
            MyFrame.Navigate(typeof(SettingsPage), systemTrayManager);
        };
    }
}
```
### 2. Add Icon File
Place your icon file in the Assets folder (e.g., Assets/Windows.ico) and ensure it's included in your project with "Build Action" set to "Content".

#### Custom Icons
```cs
// From .ico file
SystemTrayIcon.Icon = new IcoIcon("Assets/CustomIcon.ico");

// From system DLL (like shell32.dll)
SystemTrayIcon.Icon = new LibIcon("shell32.dll", 130);
```

### 3. Settings Page Integration
```xaml
<Page x:Class="YourApp.SettingsPage"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    
    <StackPanel Padding="20" Spacing="10">
        <ToggleSwitch x:Name="TrayIconToggle" 
                     Header="Show Tray Icon"
                     OffContent="Hidden" 
                     OnContent="Visible"
                     Toggled="TrayIconToggle_Toggled"/>
        
        <ToggleSwitch x:Name="MinimizeToTrayToggle" 
                     Header="Minimize to Tray"
                     OffContent="Minimize to Taskbar" 
                     OnContent="Minimize to Tray"
                     Toggled="MinimizeToTrayToggle_Toggled"/>
        
        <ToggleSwitch x:Name="CloseButtonTrayToggle" 
                     Header="Close Button Behavior"
                     OffContent="Exit App" 
                     OnContent="Minimize to Tray"
                     Toggled="CloseToMinimizesInTrayToggle_Toggled"/>
        
        <ComboBox x:Name="LanguageComboBox" 
                 Header="Language"
                 SelectionChanged="LanguageComboBox_SelectionChanged">
            <ComboBoxItem Tag="en">English</ComboBoxItem>
            <ComboBoxItem Tag="fa">فارسی</ComboBoxItem>
            <ComboBoxItem Tag="ar">العربية</ComboBoxItem>
        </ComboBox>
    </StackPanel>
</Page>
```
### 4. API Reference
SystemTrayManager

### Properties  

| Property                  | Type   | Description                                 |
|----------------------------|--------|---------------------------------------------|
| `IsIconVisible`            | bool   | Gets or sets tray icon visibility           |
| `LanguageCode`             | string | Sets context menu language (e.g., "en", "fa", "ar") |
| `IconToolTip`              | string | Sets the tooltip text for the tray icon     |
| `MinimizeToTray`           | bool   | Enable/disable minimize to tray behavior    |
| `CloseButtonMinimizesToTray` | bool | Change close button behavior                |

### Methods  

| Method                    | Description                                  |
|----------------------------|----------------------------------------------|
| `ToggleWindowVisibility()` | Show/hide the main window                   |
| `RefreshContextMenu()`     | Update context menu with current settings   |

### Events
- LeftClick: Fired when tray icon is left-clicked

- RightClick: Fired when tray icon is right-clicked

### Supported Languages
- LTR
   - English (default)
- RTL
   - Persian/Farsi (fa, fa-IR)
   - Arabic (ar, ar-SA)
   - Urdu (ur, ur-PK)
   - Hebrew (he, he-IL)
   - Customization

### Custom Context Menu Items
```cs
// Override the default menu items by modifying the BuildMenuItems method
private void BuildMenuItems()
{
    var texts = MenuTranslations.TryGetValue(languageCode, out string[]? value)
        ? value : MenuTranslations["default"];

    menuItems =
    [
        new NotifyContextMenuWindow.Item("Custom Item", new Command(CustomAction)),
        new NotifyContextMenuWindow.Item("--", null), // Separator
        new NotifyContextMenuWindow.Item(texts[0], new Command(OpenSettings)),
        new NotifyContextMenuWindow.Item("--", null),
        new NotifyContextMenuWindow.Item(texts[1], new Command(() => Application.Current.Exit()))
    ];
}

private void CustomAction()
{
    // Your custom action here
}
```

### Dispose Pattern
Always dispose the SystemTrayManager when your application exits:

```cs
protected override void OnClosed(EventArgs e)
{
    systemTrayManager?.Dispose();
    windowHelper?.Dispose();
    base.OnClosed(e);
}
```

### Troubleshooting
#### Icon Not Showing
- Ensure the icon file exists in the correct path
- Check that the icon file is included in the project as "Content"
- Verify that IsIconVisible is set to true

#### Context Menu Not Appearing
- Check language code formatting (should be like "en", "fa-IR")
- Verify that RTL languages are properly handled if using them

#### Window Management Issues
- Ensure WindowHelper is properly initialized with your main window
- Check that window events are properly hooked up

#### Sample Usage
See the included SettingsPage class for a complete example of how to integrate the system tray manager with your application's settings UI.

#### License
This code is provided as-is. Feel free to modify and use in your projects.

#### Contributing
Contributions are welcome! Please feel free to submit pull requests or open issues for bugs and feature requests.

🔗 [GitHub - MEHDIMYADI](https://github.com/MEHDIMYADI)
