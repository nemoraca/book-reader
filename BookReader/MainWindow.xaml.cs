using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BookReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Border leftPageBottom;
        private Border leftPageTop;
        private Border rightPageBottom;
        private Border rightPageTop;

        private static Thickness nullThickness = new Thickness(0);

        private double animatedAngle;
        private int animatePageTurning;

        public MainWindow()
        {
            App.mainWindow = this;

            InitializeComponent();

            SetF1CommandBinding();

            CompositionTarget.Rendering += AnimatePageTurning;
        }

        private void Window_Loaded(object sender, RoutedEventArgs args)
        {
            availableSpace.Child = App.bookOrientation == BookOrientation.Horizontal ? (Border)FindResource("HorizontalBook") : (Border)FindResource("VerticalBook");
            
            StackPanel stackPanel = (StackPanel)FindResource("Book");
            leftPageBottom = (Border)((Grid)stackPanel.Children[0]).Children[0];
            leftPageTop = (Border)((Grid)stackPanel.Children[0]).Children[1];
            rightPageBottom = (Border)((Grid)stackPanel.Children[2]).Children[0];
            rightPageTop = (Border)((Grid)stackPanel.Children[2]).Children[1];

            ((Image)leftPageBottom.Child).Source = new BitmapImage(new Uri(App.CurrentLeftPagePath, UriKind.RelativeOrAbsolute));
            ((Image)leftPageTop.Child).Source = (BitmapImage)App.Current.Resources["Transparent"];
            ((Image)rightPageBottom.Child).Source = new BitmapImage(new Uri(App.CurrentRightPagePath, UriKind.RelativeOrAbsolute));
            ((Image)rightPageTop.Child).Source = (BitmapImage)App.Current.Resources["Transparent"];

            SetBook(stackPanel);
        }

        private void SetF1CommandBinding()
        {
            CommandBinding helpBinding = new CommandBinding(ApplicationCommands.Help);
            helpBinding.CanExecute += CanHelpExecute;
            helpBinding.Executed += HelpExecuted;
            CommandBindings.Add(helpBinding);
        }

        private void CanHelpExecute(object sender, CanExecuteRoutedEventArgs args)
        {
            args.CanExecute = true;
        }

        private void HelpExecuted(object sender, ExecutedRoutedEventArgs args)
        {
            MessageBox.Show("Desktop version for Windows of the Internet Archive BookReader app.", "About BookReader");
        }

        public void SetBook(StackPanel sp)
        {
            Border border = App.bookOrientation == BookOrientation.Horizontal ? (Border)FindResource("HorizontalBook") : (Border)FindResource("VerticalBook");
            StackPanel stackPanel = sp ?? (StackPanel)FindResource("Book");
            if (stackPanel.Parent != null) ((Border)stackPanel.Parent).Child = null;
            border.Child = (StackPanel)FindResource("Book");
            availableSpace.Child = border;
        }

        private void AnimatePageTurning(object sender, EventArgs args)
        {
            if (animatePageTurning == 0) return;
            animatedAngle += 5;
            progressBar1.Tag = (App.currentPage + animatePageTurning * animatedAngle / 180.0) / (App.numberOfPages - (App.numberOfPages % 2));
            if (animatedAngle > 180) animatedAngle = 180;
            
            if (animatePageTurning > 0)
            {
                rightPageTop.Width = Math.Max(0, rightPageBottom.ActualWidth * Math.Cos(animatedAngle * Math.PI / 180));
                leftPageTop.Width = Math.Max(0, leftPageBottom.ActualWidth * Math.Cos((180 - animatedAngle) * Math.PI / 180));
            }

            if (animatePageTurning < 0)
            {
                leftPageTop.Width = Math.Max(0, leftPageBottom.ActualWidth * Math.Cos(animatedAngle * Math.PI / 180));
                rightPageTop.Width = Math.Max(0, rightPageBottom.ActualWidth * Math.Cos((180 - animatedAngle) * Math.PI / 180));
            }

            if (animatedAngle == 180)
            {
                if (animatePageTurning > 0)
                {
                    ((Image)leftPageBottom.Child).Source = ((Image)leftPageTop.Child).Source;
                    ((Image)leftPageTop.Child).Source = (BitmapImage)App.Current.Resources["Transparent"];
                    ((Image)rightPageTop.Child).Source = (BitmapImage)App.Current.Resources["Transparent"];
                }
                if (animatePageTurning < 0)
                {
                    ((Image)rightPageBottom.Child).Source = ((Image)rightPageTop.Child).Source;
                    ((Image)rightPageTop.Child).Source = (BitmapImage)App.Current.Resources["Transparent"];
                    ((Image)leftPageTop.Child).Source = (BitmapImage)App.Current.Resources["Transparent"];
                }
                rightPageTop.Width = double.NaN; // This is equivalent to rightPageTop.Width = "Auto" in XAML.
                leftPageTop.Width = double.NaN; // This is equivalent to leftPageTop.Width = "Auto" in XAML.
                leftPageTop.BorderThickness = nullThickness;
                rightPageTop.BorderThickness = nullThickness;
                animatedAngle = 0;
                App.currentPage += animatePageTurning;
                ((Grid)leftPageBottom.Parent).ToolTip = App.LeftPageToolTip;
                ((Grid)rightPageBottom.Parent).ToolTip = App.RightPageToolTip;
                animatePageTurning = 0;
            }
        }

        private void RightArrow_Click(object sender, RoutedEventArgs args)
        {
            if (animatePageTurning != 0 || App.currentPage >= App.numberOfPages - (App.numberOfPages % 2)) return;
            FlipPages(2);
        }

        private void LeftArrow_Click(object sender, RoutedEventArgs args)
        {
            if (animatePageTurning != 0 || App.currentPage < 2) return;
            FlipPages(-2);
        }

        private void FlipPages(int delta)
        {
            if (delta == 0) return;
            if (delta > 0)
            {
                leftPageTop.Width = 0;
                leftPageTop.BorderThickness = new Thickness(App.borderThickness, 0, 0, 0);
                rightPageTop.BorderThickness = new Thickness(0, 0, App.borderThickness, 0);
                ((Image)leftPageTop.Child).Source = new BitmapImage(new Uri(App.PagePath(App.currentPage + delta - 1), UriKind.RelativeOrAbsolute));
                ((Image)rightPageTop.Child).Source = new BitmapImage(new Uri(App.PagePath(App.currentPage), UriKind.RelativeOrAbsolute));
                ((Image)rightPageBottom.Child).Source = new BitmapImage(new Uri(App.PagePath(App.currentPage + delta), UriKind.RelativeOrAbsolute));
            }
            else
            {
                rightPageTop.Width = 0;
                leftPageTop.BorderThickness = new Thickness(App.borderThickness, 0, 0, 0);
                rightPageTop.BorderThickness = new Thickness(0, 0, App.borderThickness, 0);
                ((Image)leftPageTop.Child).Source = new BitmapImage(new Uri(App.PagePath(App.currentPage + delta + 1), UriKind.RelativeOrAbsolute));
                ((Image)leftPageBottom.Child).Source = new BitmapImage(new Uri(App.PagePath(App.currentPage + delta - 1), UriKind.RelativeOrAbsolute));
                ((Image)rightPageTop.Child).Source = new BitmapImage(new Uri(App.PagePath(App.currentPage + delta), UriKind.RelativeOrAbsolute));
            }
            animatePageTurning = delta;
        }

        private void Page_MouseMove(object sender, MouseEventArgs args)
        {
            Grid grid = (Grid)sender;
            ToolTip tp = (ToolTip)grid.ToolTip;
            if (tp != null) tp.IsOpen = false;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs args)
        {
            switch (args.Key)
            {
                case Key.Left: LeftArrow_Click(null, null); break;
                case Key.Right: RightArrow_Click(null, null); break;
                default: return;
            }
        }
    }
}
