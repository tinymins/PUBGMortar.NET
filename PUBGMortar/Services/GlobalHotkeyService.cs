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
    /// 开始测量事件 (Alt+Q)
    /// </summary>
    public event EventHandler? StartMeasurement;

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
        // 检测 Alt+Q 组合
        if (e.Keys.IsAlt && e.CurrentKey == Key.Q)
        {
            System.Diagnostics.Debug.WriteLine("Alt+Q detected - Starting measurement");
            Application.Current?.Dispatcher.Invoke(() => StartMeasurement?.Invoke(this, EventArgs.Empty));
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
