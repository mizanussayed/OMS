using OMS.Models;
using System.Globalization;

namespace OMS.Converters;

public class StatusVisibilityConverter : IValueConverter
{
 public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
 {
  if (value is DressOrderStatus status && parameter is string param)
  {
   return status.ToString() == param;
  }
  return false;
 }

 public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
 => throw new NotImplementedException();
}