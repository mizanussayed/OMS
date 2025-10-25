using System.Globalization;

namespace OMS.Converters;

public class ColorConverter : IValueConverter
{
 public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
 {
  if (value is string colorName)
  {
   return colorName.ToLower() switch
   {
    "blue" => Colors.Blue,
    "red" => Colors.Red,
    "green" => Colors.Green,
    _ => Colors.Gray
   };
  }
  return Colors.Gray;
 }

 public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
 => throw new NotImplementedException();
}