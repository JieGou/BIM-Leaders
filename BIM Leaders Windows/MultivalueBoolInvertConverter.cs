using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// Returns false if any input is true. Usable for enabling buttons if no errors in binded controls
    /// </summary>
    public class MultivalueBoolInvertConverter : IValueConverter, IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool b = true;
            if (values.LongLength > 0)
            {
                foreach (var value in values)
                {
                    if (value is bool)
                    {
                        if ((bool)value == false)
                        {
                            b = false;
                        }
                    }
                }
            }
            return !b;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool b = true;
            if (value is bool)
            {
                if ((bool)value == false)
                {
                    b = false;
                }
            }
            return !b;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
