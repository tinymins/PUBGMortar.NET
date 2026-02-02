using System;
using System.Windows;
using H.Hooks;

namespace PUBGMortar.Services;

/// <summary>
/// 全局热键服务 - 使用 H.Hooks 库实现全局热键监听
/// </summary>
public class GlobalHotkeyService : IDisposable
{
    private readonly LowLevelKeyboardHook _keyboardHook;
    private readonly LowLevelMouseHook _mouseHook;

    private bool _disposed;

    /// <summary>
    /// 开始测量事件 (Ctrl+Alt+Q) - 完整流程包含比例尺设置
    /// </summary>
    public event EventHandler? StartMeasurement;

    /// <summary>
    /// 快速测量事件 (Alt+Q) - 跳过比例尺，使用上次的比例尺（首次无比例尺时会自动走完整流程）
    /// 如果提示窗口已存在，则关闭提示窗口
    /// </summary>
    public event EventHandler? QuickMeasurement;

    /// <summary>
    /// 点击设置点事件 (Alt+左键)
    /// </summary>
    public event EventHandler<(double X, double Y)>? PointSet;

    public GlobalHotkeyService()
    {
        _keyboardHook = new LowLevelKeyboardHook
        {
            IsExtendedMode = true,
            HandleModifierKeys = true,
        };

        _mouseHook = new LowLevelMouseHook
        {
            AddKeyboardKeys = true,  // 关键：允许鼠标事件中检测键盘按键
            IsExtendedMode = true,
        };

        _keyboardHook.Down += OnKeyDown;
        _keyboardHook.Up += OnKeyUp;
        _mouseHook.Down += OnMouseDown;
    }

    /// <summary>
    /// 启动热键监听
    /// </summary>
    public void Start()
    {
        _keyboardHook.Start();
        _mouseHook.Start();
    }

    /// <summary>
    /// 停止热键监听
    /// </summary>
    public void Stop()
    {
        _keyboardHook.Stop();
        _mouseHook.Stop();
    }

    private void OnKeyDown(object? sender, KeyboardEventArgs e)
    {
        if (!e.Keys.IsAlt) return;

        switch (e.CurrentKey)
        {
            case Key.Q:
                if (e.Keys.IsCtrl)
                {
                    // Ctrl+Alt+Q: 完整测量流程（重置比例尺）
                    System.Diagnostics.Debug.WriteLine("Ctrl+Alt+Q detected - Starting full measurement");
                    Application.Current?.Dispatcher.Invoke(() => StartMeasurement?.Invoke(this, EventArgs.Empty));
                }
                else
                {
                    // Alt+Q: 快速测量（跳过比例尺，首次时自动走完整流程）
                    // 如果提示窗口存在则关闭，不存在则开始测量
                    System.Diagnostics.Debug.WriteLine("Alt+Q detected - Quick measurement or close overlay");
                    Application.Current?.Dispatcher.Invoke(() => QuickMeasurement?.Invoke(this, EventArgs.Empty));
                }
                break;
        }
    }

    private void OnKeyUp(object? sender, KeyboardEventArgs e)
    {
        // 不需要处理
    }

    private void OnMouseDown(object? sender, MouseEventArgs e)
    {
        // 使用 Keys.IsAlt 检测是否按住 Alt 键
        if (e.Keys.IsAlt && e.CurrentKey == Key.MouseLeft)
        {
            var position = ((double)e.Position.X, (double)e.Position.Y);
            System.Diagnostics.Debug.WriteLine($"Alt+Click detected at ({position.Item1}, {position.Item2})");
            Application.Current?.Dispatcher.Invoke(() => PointSet?.Invoke(this, position));
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        _keyboardHook.Down -= OnKeyDown;
        _keyboardHook.Up -= OnKeyUp;
        _mouseHook.Down -= OnMouseDown;

        _keyboardHook.Dispose();
        _mouseHook.Dispose();

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
