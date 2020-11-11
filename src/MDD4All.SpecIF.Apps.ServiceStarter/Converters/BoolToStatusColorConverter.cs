using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;

namespace MDD4All.SpecIF.Apps.ServiceStarter.Converters
{
    public class BoolToStatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush result = new SolidColorBrush(Colors.Yellow);

            if(value is bool)
            {
                bool boolValue = (bool)value;
                if(boolValue)
                {
                    result = new SolidColorBrush(Colors.Green);
                }
                else
                {
                    result = new SolidColorBrush(Colors.Red);
                }
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
