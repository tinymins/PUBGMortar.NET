using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace PUBGMortar.Views;

/// <summary>
/// 浮动提示窗口 - 支持鼠标穿透
/// </summary>
public partial class OverlayWindow : Window
{
    #region Win32 API for Click-Through

    private const int WS_EX_TRANSPARENT = 0x00000020;
    private const int GWL_EXSTYLE = -20;

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hwnd, int index);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

    #endregion

    public OverlayWindow(string message)
    {
        InitializeComponent();
        MessageText.Text = message;

        // 窗口加载后设置鼠标穿透
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // 获取窗口句柄并设置穿透样式
        var hwnd = new WindowInteropHelper(this).Handle;
        var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
        SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
    }
}
