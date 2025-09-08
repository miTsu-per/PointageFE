using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace MobileApp.Converters
{
    public class BoolToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return "En attente";
            }

            if (value is bool isApproved)
            {
                return isApproved ? "Approuvé" : "Refusé"; 
            }

            return "En attente";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
