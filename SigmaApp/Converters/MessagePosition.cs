using System.Globalization;
namespace SigmaApp.Converters
{

    public class MessagePosition : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isMine)
            {
                return isMine ? new Thickness(50, 5, 5, 5) : new Thickness(5, 5, 50, 5);
            }
            throw new InvalidCastException("Expected a boolean value.");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

