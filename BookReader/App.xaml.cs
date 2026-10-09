using System;
using System.Windows;
using System.Windows.Data;
using System.Globalization;

namespace BookReader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public class ScaleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double coefficient = double.Parse(parameter.ToString());
            return coefficient * (double)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    public class ThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double coefficient = double.Parse(parameter.ToString());
            return new Thickness(coefficient * (double)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    public class ProgressConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)values[0] - 2 * (double)values[1] * 0.06;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[] { value };
        }
    }

    public partial class App : Application
    {
        public static readonly IValueConverter scaleConverter = new ScaleConverter();
        public static readonly IValueConverter thicknessConverter = new ThicknessConverter();
        public static readonly IMultiValueConverter progressConverter = new ProgressConverter();
    }
}
