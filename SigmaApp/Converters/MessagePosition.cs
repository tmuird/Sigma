using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SigmaApp.Converters
{
    public class MessagePosition : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
           
            if ((bool)value)
            {
                return new Thickness(250, 0, 0, 0);
            }
            else
            {
                return new Thickness(0, 0, 0, 0);
            }
                    
            
                
           
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
