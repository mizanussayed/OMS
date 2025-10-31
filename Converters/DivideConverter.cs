using System.Globalization;

namespace OMS.Converters;

public class DivideConverter : IValueConverter
{
 public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
 {
  if (value is double v && parameter is double p && p != 0)
   return (double)(v / p);
  return 0.0;
 }

 public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
 => throw new NotImplementedException();
}