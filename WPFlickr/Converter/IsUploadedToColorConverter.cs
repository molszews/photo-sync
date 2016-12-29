using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace WPFlickr.Converter
{
    [ValueConversion(typeof(bool), typeof(Brush))]
    public class IsUploadedToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(Brush))
                throw new InvalidOperationException("Invalid target type");

            var isUploaded = (bool) value;

            return isUploaded
                ? Brushes.LightGreen
                : Brushes.PapayaWhip;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
