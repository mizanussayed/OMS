using OMS.Models;
using System.Globalization;

namespace OMS.Converters;

public class StatusColorConverter : IValueConverter
{
 public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
 {
  if (value is DressOrderStatus status)
  {
   return status switch
   {
    DressOrderStatus.Pending => Colors.Yellow,
    DressOrderStatus.Completed => Colors.Blue,
    DressOrderStatus.Delivered => Colors.Green,
    _ => Colors.Gray
   };
  }
  return Colors.Gray;
 }

 public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
 => throw new NotImplementedException();
}