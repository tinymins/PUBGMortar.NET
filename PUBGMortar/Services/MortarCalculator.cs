using System;

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
    /// 最大仰角 (度)
    /// </summary>
    public const double MAX_DEGREE = 26.19;

    /// <summary>
    /// 屏幕中心Y坐标 (2560x1440分辨率)
    /// </summary>
    public double CenterPixelY { get; set; } = 719;

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
        // 0度是屏幕中心，MAX_DEGREE是屏幕顶部
        double deltaY = CenterPixelY - point.Y;
        ElevationAngle = deltaY * MAX_DEGREE / CenterPixelY;
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
