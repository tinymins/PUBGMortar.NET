using System.Windows;

namespace PUBGMortar.Views;

/// <summary>
/// 浮动提示窗口
/// </summary>
public partial class OverlayWindow : Window
{
    public OverlayWindow(string message)
    {
        InitializeComponent();
        MessageText.Text = message;
    }
}
