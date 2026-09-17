using grzyClothTool.Helpers;
using System;
using System.Globalization;
using System.Windows.Data;

namespace grzyClothTool.Converters;

public class DrawableTypeNameConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => DrawableTypeNames.GetLabel(value as string);

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
