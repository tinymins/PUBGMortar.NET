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
                    ShowOverlay("计算无解 - 目标超出射程", 3000);
                }
                else
                {
                    ResultText = $"{result:F0} m";
                    ShowOverlay($"迫击炮距离: {result:F0} m", 3000);
                }

                _currentStep = MeasurementStep.Idle;
                StatusText = "测量完成";
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
