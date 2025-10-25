using System.Globalization;

namespace OMS.Converters;

public class BoolToColorConverter : IValueConverter
{
 public Color TrueColor { get; set; } = Colors.Black;
 public Color FalseColor { get; set; } = Colors.Transparent;

 public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
 {
 var isSelected = value is bool b && b;
 return isSelected ? TrueColor : FalseColor;
 }

 public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
 => throw new NotImplementedException();
}
