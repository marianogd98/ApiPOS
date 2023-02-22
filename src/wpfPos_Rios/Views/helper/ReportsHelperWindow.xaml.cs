using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace wpfPos_Rios.Views.helper
{
    /// <summary>
    /// Interaction logic for ReportsHelperWindow.xaml
    /// </summary>
    public partial class ReportsHelperWindow : Window
    {
        public int id;
        public ReportsHelperWindow()
        {
            InitializeComponent();
            id = 0;

        }

        private void btnLastVoucher_Click(object sender, RoutedEventArgs e)
        {
            id = 1;
            this.Close();
        }

        private void btnCloseTurnReport_Click(object sender, RoutedEventArgs e)
        {
            id = 2;
            this.Close();

        }

        private void btnCLosePosReport_Click(object sender, RoutedEventArgs e)
        {
            id = 3;
            this.Close();

        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();

        }

        private void btnLastVoucherProcesado_Click(object sender, RoutedEventArgs e)
        {
            id = 4;
            this.Close();
        }
    }
}
