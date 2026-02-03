using System;
using System.Windows;

namespace PUBGMortar.Services;

/// <summary>
/// 迫击炮计算器 - 根据仰角和水平距离计算迫击炮设置值
/// </summary>
public class MortarCalculator
{
    /// <summary>
    /// 比例尺因子 (像素到米的转换)
    /// </summary>
    public double ScaleFactor { get; private set; }

    /// <summary>
    /// 水平距离 (米)
    /// </summary>
    public double HorizontalDistance { get; private set; }

    /// <summary>
    /// 仰角 (度)
    /// </summary>
    public double ElevationAngle { get; private set; }

    /// <summary>
    /// 迫击炮最大射程 (米)
    /// </summary>
    public const double MAX_DISTANCE = 700.0;

    /// <summary>
    /// PUBG默认水平FOV (度)
    /// </summary>
    public const double DEFAULT_HORIZONTAL_FOV = 80.0;

    /// <summary>
    /// 最大仰角 (度) - 根据屏幕宽高比自动计算
    /// </summary>
    public double MaxDegree { get; private set; }

    /// <summary>
    /// 屏幕中心Y坐标 - 根据屏幕分辨率自动计算
    /// </summary>
    public double CenterPixelY { get; private set; }

    public MortarCalculator()
    {
        UpdateScreenParameters();
    }

    /// <summary>
    /// 根据当前屏幕分辨率更新参数
    /// </summary>
    public void UpdateScreenParameters()
    {
        // 获取主屏幕物理分辨率（不受 DPI 缩放影响）
        // 使用 Win32 API 获取真实分辨率
        double screenWidth = GetSystemMetrics(SM_CXSCREEN);
        double screenHeight = GetSystemMetrics(SM_CYSCREEN);

        // 屏幕中心Y坐标 (从0开始计数，所以是 height/2 - 0.5，取整后约等于 height/2 - 1)
        CenterPixelY = screenHeight / 2.0 - 1;

        // 根据屏幕宽高比计算垂直FOV
        // PUBG使用Hor+系统：垂直FOV = 2 * arctan(tan(水平FOV/2) * 高度/宽度)
        double horizontalFovRad = DEFAULT_HORIZONTAL_FOV * Math.PI / 180.0;
        double verticalFovRad = 2.0 * Math.Atan(Math.Tan(horizontalFovRad / 2.0) * screenHeight / screenWidth);

        // 最大仰角是垂直FOV的一半
        MaxDegree = verticalFovRad * 180.0 / Math.PI / 2.0;
    }

    // Win32 API 常量
    private const int SM_CXSCREEN = 0;
    private const int SM_CYSCREEN = 1;

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);

    /// <summary>
    /// 根据两点设置100米比例尺
    /// </summary>
    /// <param name="point1">第一点</param>
    /// <param name="point2">第二点</param>
    public void SetScaleFactor((double X, double Y) point1, (double X, double Y) point2)
    {
        double deltaX = point2.X - point1.X;
        double deltaY = point2.Y - point1.Y;
        double distanceInPixels = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

        // 100米对应的像素距离
        ScaleFactor = 100.0 / distanceInPixels;
    }

    /// <summary>
    /// 计算水平距离
    /// </summary>
    /// <param name="point1">第一点</param>
    /// <param name="point2">第二点</param>
    /// <returns>水平距离 (米)</returns>
    public double GetHorizontalDistance((double X, double Y) point1, (double X, double Y) point2)
    {
        double deltaX = point2.X - point1.X;
        double deltaY = point2.Y - point1.Y;
        double distanceInPixels = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

        HorizontalDistance = distanceInPixels * ScaleFactor;
        return HorizontalDistance;
    }

    /// <summary>
    /// 根据屏幕上的点计算仰角
    /// </summary>
    /// <param name="point">屏幕上的点</param>
    /// <returns>仰角 (度)</returns>
    public double GetElevationAngle((double X, double Y) point)
    {
        // 使用正确的角度计算：通过反正切函数将像素偏移转换为角度
        // deltaY > 0 表示目标在屏幕中心上方（仰角为正）
        double deltaY = CenterPixelY - point.Y;

        // 计算每像素对应的tan值：屏幕顶部对应 tan(MaxDegree)
        // tanPerPixel = tan(MaxDegree) / CenterPixelY
        double maxDegreeRad = MaxDegree * Math.PI / 180.0;
        double tanPerPixel = Math.Tan(maxDegreeRad) / CenterPixelY;

        // 通过反正切计算实际角度
        ElevationAngle = Math.Atan(deltaY * tanPerPixel) * 180.0 / Math.PI;

        return ElevationAngle;
    }

    /// <summary>
    /// 计算迫击炮应设置的距离值
    /// 公式: R = (L + tan(β) * (M - √(M² - 2LM·tan(β) - L²))) / (tan²(β) + 1)
    /// </summary>
    /// <param name="beta">仰角 (度)</param>
    /// <param name="L">水平距离 (米)</param>
    /// <returns>迫击炮设置距离 (米)，如无解返回-1</returns>
    public double Solve(double beta, double L)
    {
        // 同一水平面
        if (Math.Abs(beta) < 0.001)
        {
            return L;
        }

        double tanBeta = Math.Tan(beta * Math.PI / 180.0);
        double M = MAX_DISTANCE;

        double delta = M * M - 2 * L * M * tanBeta - L * L;

        if (delta < 0)
        {
            // 无解
            return -1;
        }

        double intermediate = M - Math.Sqrt(delta);
        double result = (L + tanBeta * intermediate) / (tanBeta * tanBeta + 1);

        return result;
    }

    /// <summary>
    /// 使用当前存储的仰角和水平距离计算结果
    /// </summary>
    /// <returns>迫击炮设置距离 (米)</returns>
    public double Solve()
    {
        return Solve(ElevationAngle, HorizontalDistance);
    }

    /// <summary>
    /// 重置计算器状态
    /// </summary>
    public void Reset()
    {
        ScaleFactor = 0;
        HorizontalDistance = 0;
        ElevationAngle = 0;
    }
}
