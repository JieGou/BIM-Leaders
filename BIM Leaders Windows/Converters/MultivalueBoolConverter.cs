using System;
using System.Windows.Data;
using System.Globalization;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// Returns first error if any input has error. Usable for error message if any error in binded controls
    /// </summary>
    public class MultivalueErrorConverter : IValueConverter, IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string b = "";
            if (values.LongLength > 0)
            {
                foreach (var value in values)
                {
                    if (value is string)
                        if (value.ToString() != "")
                        {
                            b = value.ToString();
                            return b;
                        }
                }
            }
            return b;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string b = "";

            if (value is string)
                if (value.ToString() != "")
                    b = value.ToString();

            return b;
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
