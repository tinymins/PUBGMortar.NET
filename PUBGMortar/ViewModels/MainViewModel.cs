using System;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PUBGMortar.Services;
using PUBGMortar.Views;

namespace PUBGMortar.ViewModels;

/// <summary>
/// 主窗口视图模型
/// </summary>
public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly MortarCalculator _calculator;
    private readonly GlobalHotkeyService _hotkeyService;

    private MeasurementStep _currentStep = MeasurementStep.Idle;
    private (double X, double Y)? _tempPoint;
    private OverlayWindow? _overlayWindow;
    private bool _hasValidScale;  // 是否已有有效比例尺

    [ObservableProperty]
    private string _statusText = "就绪";

    [ObservableProperty]
    private string _resultText = "--";

    [ObservableProperty]
    private string _horizontalDistanceText = "--";

    [ObservableProperty]
    private string _elevationAngleText = "--";

    [ObservableProperty]
    private bool _isListening = true;

    public MainViewModel()
    {
        _calculator = new MortarCalculator();
        _hotkeyService = new GlobalHotkeyService();

        _hotkeyService.StartMeasurement += OnStartMeasurement;
        _hotkeyService.QuickMeasurement += OnQuickMeasurement;
        _hotkeyService.PointSet += OnPointSet;

        _hotkeyService.Start();
    }

    [RelayCommand]
    private void ToggleListening()
    {
        IsListening = !IsListening;
        if (IsListening)
        {
            _hotkeyService.Start();
            StatusText = "就绪";
        }
        else
        {
            _hotkeyService.Stop();
            StatusText = "已暂停";
        }
    }

    [RelayCommand]
    private void ResetMeasurement()
    {
        _calculator.Reset();
        _currentStep = MeasurementStep.Idle;
        _tempPoint = null;

        ResultText = "--";
        HorizontalDistanceText = "--";
        ElevationAngleText = "--";
        StatusText = "就绪";

        CloseOverlay();
    }

    private void OnStartMeasurement(object? sender, EventArgs e)
    {
        if (!IsListening) return;

        _calculator.Reset();
        _currentStep = MeasurementStep.ScalePoint1;
        _tempPoint = null;

        ShowOverlay("设置100米比例尺: 第一点");
    }

    /// <summary>
    /// 快速测量 - 跳过比例尺设置，使用上一次的比例尺
    /// 如果提示窗口存在则关闭，不存在则开始测量
    /// </summary>
    private void OnQuickMeasurement(object? sender, EventArgs e)
    {
        if (!IsListening) return;

        // 如果提示窗口存在，则关闭并取消当前测量
        if (_overlayWindow != null)
        {
            CloseOverlay();
            _currentStep = MeasurementStep.Idle;
            _tempPoint = null;
            StatusText = "已取消";
            return;
        }

        if (!_hasValidScale)
        {
            // 如果没有有效的比例尺，回退到完整流程
            OnStartMeasurement(sender, e);
            return;
        }

        _currentStep = MeasurementStep.DistancePoint1;
        _tempPoint = null;

        ShowOverlay("快速测量: 第一点 (你的位置)");
    }

    private void OnPointSet(object? sender, (double X, double Y) point)
    {
        if (!IsListening) return;

        switch (_currentStep)
        {
            case MeasurementStep.ScalePoint1:
                _tempPoint = point;
                _currentStep = MeasurementStep.ScalePoint2;
                ShowOverlay("设置100米比例尺: 第二点");
                break;

            case MeasurementStep.ScalePoint2:
                if (_tempPoint.HasValue)
                {
                    _calculator.SetScaleFactor(_tempPoint.Value, point);
                    _hasValidScale = true;  // 标记已有有效比例尺
                }
                _currentStep = MeasurementStep.DistancePoint1;
                _tempPoint = null;
                ShowOverlay("测量距离: 第一点 (你的位置)");
                break;

            case MeasurementStep.DistancePoint1:
                _tempPoint = point;
                _currentStep = MeasurementStep.DistancePoint2;
                ShowOverlay("测量距离: 第二点 (目标位置)");
                break;

            case MeasurementStep.DistancePoint2:
                if (_tempPoint.HasValue)
                {
                    var distance = _calculator.GetHorizontalDistance(_tempPoint.Value, point);
                    HorizontalDistanceText = $"{distance:F1} m";
                }
                _currentStep = MeasurementStep.ElevationPoint;
                _tempPoint = null;
                ShowOverlay($"水平距离: {_calculator.HorizontalDistance:F1}m\n设置仰角: 瞄准目标后点击");
                break;

            case MeasurementStep.ElevationPoint:
                var angle = _calculator.GetElevationAngle(point);
                ElevationAngleText = $"{angle:F2}°";

                var result = _calculator.Solve();
                if (result < 0)
                {
                    ResultText = "无解";
                    ShowOverlay($"距离: {_calculator.HorizontalDistance:F0}m | 仰角: {angle:F1}°\n⚠ 目标超出射程");  // 持续显示，不自动关闭
                }
                else
                {
                    ResultText = $"{result:F0} m";
                    // 显示完整信息：原始距离、仰角、迫击炮设置
                    ShowOverlay($"距离: {_calculator.HorizontalDistance:F0}m | 仰角: {angle:F1}°\n🎯 迫击炮: {result:F0} m");  // 持续显示，不自动关闭
                }

                _currentStep = MeasurementStep.Idle;
                StatusText = "测量完成 (Alt+Q关闭提示)";
                break;
        }
    }

    private void ShowOverlay(string message, int? autoCloseMs = null)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            CloseOverlay();

            _overlayWindow = new OverlayWindow(message);
            _overlayWindow.Show();

            if (autoCloseMs.HasValue)
            {
                var timer = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(autoCloseMs.Value)
                };
                timer.Tick += (s, e) =>
                {
                    timer.Stop();
                    CloseOverlay();
                };
                timer.Start();
            }
        });

        StatusText = message.Split('\n')[0];
    }

    private void CloseOverlay()
    {
        _overlayWindow?.Close();
        _overlayWindow = null;
    }

    public void Dispose()
    {
        _hotkeyService.StartMeasurement -= OnStartMeasurement;
        _hotkeyService.QuickMeasurement -= OnQuickMeasurement;
        _hotkeyService.PointSet -= OnPointSet;
        _hotkeyService.Dispose();
        CloseOverlay();
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// 测量步骤枚举
/// </summary>
public enum MeasurementStep
{
    Idle,
    ScalePoint1,
    ScalePoint2,
    DistancePoint1,
    DistancePoint2,
    ElevationPoint
}
