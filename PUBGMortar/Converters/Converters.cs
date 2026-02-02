using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PUBGMortar.Converters;

/// <summary>
/// 将布尔值转换为 Brush (用于状态指示)
/// </summary>
public class BoolToAppearanceConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isListening)
        {
            return isListening ? "Success" : "Caution";
        }
        return "Secondary";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// 将布尔值转换为 Brush 颜色
/// </summary>
public class BoolToBrushConverter : IValueConverter
{
    private static readonly SolidColorBrush SuccessBrush = new(Color.FromRgb(0x10, 0x7C, 0x10));
    private static readonly SolidColorBrush CautionBrush = new(Color.FromRgb(0xD4, 0xA0, 0x00));

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isListening)
        {
            return isListening ? SuccessBrush : CautionBrush;
        }
        return CautionBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// 将布尔值转换为监听按钮文本
/// </summary>
public class BoolToListenTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isListening)
        {
            return isListening ? "暂停监听" : "开始监听";
        }
        return "监听";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// 将布尔值转换为监听按钮图标 (这里返回文本符号)
/// </summary>
public class BoolToListenIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isListening)
        {
            return isListening ? "⏸" : "▶";
        }
        return "▶";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
