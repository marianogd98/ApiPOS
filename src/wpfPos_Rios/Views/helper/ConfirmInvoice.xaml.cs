using Models;
using System;
using System.Collections.Generic;
using System.Globalization;
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
using wpfPos_Rios.ViewModels;

namespace wpfPos_Rios.Views.helper
{
    /// <summary>
    /// Lógica de interacción para ConfirmInvoice.xaml
    /// </summary>
    public partial class ConfirmInvoice : Window
    {
        bool flag;
        NumberFormatInfo _FormatVenezuela = new CultureInfo("es-VE").NumberFormat;
        public ConfirmInvoice(Invoice pInvoice, IList<PaymentMethod> pPaymentslist)
        {
            InitializeComponent();
            flag = false;
            _FormatVenezuela.CurrencyDecimalSeparator = ",";
            _FormatVenezuela.CurrencyGroupSeparator = ".";
            txtbTotalBs.Text = pInvoice.MontoNeto.ToString("C",_FormatVenezuela);
            dgPaymentList.ItemsSource = pPaymentslist;

        }
        public bool GetResponse()
        {
            return flag;
        }
        private void header_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender == btnCancel)
                flag = false;
            else if (sender == btnFcturar)
                flag = true;
            this.Hide();
        }
    }
}
