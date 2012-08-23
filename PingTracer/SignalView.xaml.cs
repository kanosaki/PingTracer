using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PingTracer
{
    /// <summary>
    /// SignalView.xaml の相互作用ロジック
    /// </summary>
    public partial class SignalView : UserControl
    {
        public SignalView()
        {
            InitializeComponent();
        }

        public double SignalMargin
        {
            get { return (double)GetValue(SignalMarginProperty); }
            set { SetValue(SignalMarginProperty, value); }
        }
        public static DependencyProperty SignalMarginProperty
            = DependencyProperty.Register("SignalMargin", typeof(double), typeof(SignalView), new PropertyMetadata(5.0));


        public int Level
        {
            get { return (int)GetValue(LevelProperty); }
            set { SetValue(LevelProperty, value); }
        }
        public static DependencyProperty LevelProperty
            = DependencyProperty.Register("Level", typeof(int), typeof(SignalView), new PropertyMetadata(5));

    }

    [ValueConversion(typeof(int), typeof(Visibility))]
    public class LevelConverter : IValueConverter
    {
        public LevelConverter()
        {
            this.Default = Visibility.Visible;
        }
        public Visibility Default { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                var val = (int)value;
                var index = int.Parse((string)parameter);
                return val >= index ? Visibility.Visible : Visibility.Hidden;
            }
            catch (InvalidCastException)
            {
                return this.Default;
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
