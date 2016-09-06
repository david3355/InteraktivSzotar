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
using System.Windows.Shapes;
using System.Data;

namespace IntelligensSzotar
{
    /// <summary>
    /// Interaction logic for Statistics.xaml
    /// </summary>
    public partial class Statistics : Window
    {
        public Statistics(List<Question> Result)
        {
            InitializeComponent();
            red = new SolidColorBrush(Color.FromArgb(255, 219, 79, 79)); // FFDB4F4F
            green = new SolidColorBrush(Color.FromArgb(255, 156, 239, 156)); // FF9CEF9C
            lblue = new SolidColorBrush(Colors.AliceBlue);
            dgrid_statistics.ItemsSource = Result;
        }

        private SolidColorBrush red, green, lblue;

        private void dgrid_statistics_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            Question item = e.Row.Item as Question;
            if (item != null)
            {
                switch (item.Result)
                {
                    case Result.Helyes: e.Row.Background = green; break;
                    case Result.Hibás: e.Row.Background = red; break;
                    case Result.NincsVálasz: e.Row.Background = lblue; break;
                }
            }
        }
    }
}
