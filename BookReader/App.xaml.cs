using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace BookReader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    
    public enum BookOrientation
    {
        Undefined,
        Horizontal,
        Vertical
    }

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
            int flags = (int)coefficient;
            coefficient -= flags;
            double l = coefficient * (double)value;
            if (flags == 31) App.borderThickness = l;
            double[] d = new double[4];
            for (int i = 3; i != -1; --i)
            {
                d[i] = (flags % 2) * l;
                flags /= 2;
            }
            return new Thickness(d[0], d[1], d[2], d[3]);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    public class ProgressConverter : IMultiValueConverter
    {
        public const double paddingScaleCoefficient = 0.01;
        public const double innerScaleCoefficient = 0.02;
        public const double innerScaleCoefficient15 = 15 + innerScaleCoefficient;
        public const double innerScaleCoefficientReduced = 0.015;
        public const double innerScaleCoefficientReduced15 = 15 + innerScaleCoefficientReduced;
        public const double outerScaleCoefficient = 3 * innerScaleCoefficient;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double width = (double)values[0];
            double height = (double)values[1];
            App.availableSpaceX = width * (1 - 2 * paddingScaleCoefficient);
            App.availableSpaceY = height * (1 - 2 * paddingScaleCoefficient - outerScaleCoefficient);
            BookOrientation orientation;
            if (App.availableSpaceY / App.availableSpaceX > App.halfAspectRatio)
                orientation = BookOrientation.Horizontal;
            else
                orientation = BookOrientation.Vertical;
            if (App.bookOrientation == BookOrientation.Undefined) App.bookOrientation = orientation;
            if (orientation != App.bookOrientation)
            {
                App.bookOrientation = orientation;
                App.mainWindow.SetBook(null);
            }
            return width - 2 * height * outerScaleCoefficient;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[] { value };
        }
    }

    public class ProductConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double product = 1;
            foreach (object value in values)
                product *= (double)value;
            return product;
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
        public static readonly IMultiValueConverter productConverter = new ProductConverter();

        public static MainWindow mainWindow;
        public static BookOrientation bookOrientation;

        public const double borderCoefficient = 0.002;
        public static readonly double coefficientW;
        public static readonly double coefficientH;
        public static readonly double doubleCoefficientH;
        public static readonly double coefficientW31;
        public static readonly double coefficientH31;
        public static double borderThickness;

        public static int numberOfPages;
        public static int currentPage;

        // supported extensions are: BMP, JPG, JPEG, GIF, PNG, TIF, TIFF and WEBP.
        public static string fileExtension;
        public static string rootDirectoryPath;
        public static string pagesDirectoryPath;

        public static readonly double aspectRatio;
        public static readonly double inverseAspectRatio;
        public static readonly double halfAspectRatio;
        public static readonly double doubleInverseAspectRatio;

        public static double availableSpaceX;
        public static double availableSpaceY;

        public static string CurrentLeftPagePath
        {
            get { return PagePath(currentPage - 1); }
        }
        public static string CurrentRightPagePath
        {
            get { return PagePath(currentPage); }
        }

        public static ToolTip LeftPageToolTip
        {
            get { return currentPage > 0 ? new ToolTip() { Content = string.Format("Page {0}", currentPage - 1) } : null;  }
        }
        public static ToolTip RightPageToolTip
        {
            get { return currentPage < numberOfPages ? new ToolTip() { Content = string.Format("Page {0}", currentPage) } : null; }
        }

        static App()
        {
            string path = Environment.GetCommandLineArgs()[0];
            int num = path.LastIndexOf('\\');
            rootDirectoryPath = path.Substring(0, num + 1);
            pagesDirectoryPath = rootDirectoryPath + "Book\\";
            string[] files = Directory.GetFiles(pagesDirectoryPath, "*", SearchOption.TopDirectoryOnly);
            fileExtension = files[0].Substring(files[0].LastIndexOf(".") + 1);
            numberOfPages = files.Length;
            using (FileStream imageStream = File.OpenRead(pagesDirectoryPath + "Page0." + fileExtension))
            {
                BitmapDecoder decoder = BitmapDecoder.Create(imageStream, BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.Default);
                int height = decoder.Frames[0].PixelHeight;
                int width = decoder.Frames[0].PixelWidth;
                aspectRatio = (double)height / width;
                inverseAspectRatio = (double)width / height;
                // We want border thickness around width x height book page to be: borderCoefficient * Math.Sqrt(width * height)
                coefficientW = 0.25 - 1 / (4 + 8 * borderCoefficient * Math.Sqrt(aspectRatio));
                coefficientW31 = 31 + coefficientW;
                coefficientH = 0.5 - 1 / (2 + 4 * borderCoefficient * Math.Sqrt(inverseAspectRatio));
                doubleCoefficientH = 2 * coefficientH;
                coefficientH31 = 31 + coefficientH;
                halfAspectRatio = coefficientW / coefficientH;
                doubleInverseAspectRatio = coefficientH / coefficientW;
            }
        }

        public static string PagePath(int pageNumber)
        {
            if (pageNumber < 0 || pageNumber >= numberOfPages)
                return "Transparent.png";
            else
                return pagesDirectoryPath + string.Format("Page{0}.{1}", pageNumber, fileExtension);
        }
    }
}
