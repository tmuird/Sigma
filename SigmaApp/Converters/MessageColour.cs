using System.Globalization;
namespace SigmaApp.Converters
{
    public class MessageColour : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isMine)
            {
                return isMine ? Color.FromHex("#DCF8C6") : Color.FromHex("#FFFFFF");
            }
            throw new InvalidCastException("Expected a boolean value.");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

