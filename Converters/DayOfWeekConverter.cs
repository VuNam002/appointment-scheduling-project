using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace ProjectMaui.Converters
{
    public class DayOfWeekConverter : IValueConverter
    {
        private readonly string[] _days = { "Chủ Nhật", "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7" };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int index && index >= 0 && index < _days.Length)
            {
                return _days[index];
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}