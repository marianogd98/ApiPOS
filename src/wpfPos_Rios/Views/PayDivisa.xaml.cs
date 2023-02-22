using KeyPad;
using Models;
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
using wpfPos_Rios.ViewModels;
using wpfPos_Rios.Views.helper;

namespace wpfPos_Rios.Views
{
    /// <summary>
    /// Lógica de interacción para PayDivisa.xaml
    /// </summary>
    public partial class PayDivisa : Window
    {

        List<PaymentMethod> _Paymentslist;
        VmInvoice _Invoice;
        double _Tasa;
        public PayDivisa(List<PaymentMethod> pPaymentslist, VmInvoice pInvoice, double pTasa)
        {
            InitializeComponent();
            _Paymentslist = pPaymentslist;
            _Invoice = pInvoice;
            _Tasa = pTasa;

            double vuelto = _Invoice.RestanteRef(_Tasa);
            txtbMonto.Text = ((int)vuelto).ToString();

        }

        private void rbDivisaEuro_Click(object sender, RoutedEventArgs e)
        {
            rbDivisaDolar.IsChecked = false;
        }

        private void rbDivisaDolar_Click(object sender, RoutedEventArgs e)
        {
            rbDivisaEuro.IsChecked = false;
        }
        private void ShowKeypad_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            showKeypad(textbox, this);
        }
        private bool? showKeypad(TextBox pTextbox, Window pIndex)
        {
            Keypad keypad = new Keypad(pTextbox, pIndex);

            bool? flagKeypad = keypad.ShowDialog();

            if (flagKeypad == true)
            {
                pTextbox.Text = keypad.Result;
            }

            return flagKeypad;

        }

        private void btnDonate_Click(object sender, RoutedEventArgs e)
        {
            if (rbDivisaDolar.IsChecked == false && rbDivisaEuro.IsChecked == false)
            {
                CustomMessageBox.Show("Informacion" , "Debe seleccionar la divisa");
            }
            else
            {
                if (string.IsNullOrEmpty(txtbMonto.Text))
                {
                    CustomMessageBox.Show("Informacion", "Debe ingresar el monto");
                }
                else
                {

                    try
                    {
                        var res = CustomMessageBox.Show("Confirmar" , "Desea Agregar el pago ? ",MessageBoxButton.YesNo);

                        if (MessageBoxResult.Yes == res)
                        {
                            double pay = Convert.ToDouble(txtbMonto.Text);

                            if ((_Invoice.Cancelled + pay) > (_Invoice.Total + (100)))
                            {
                                CustomMessageBox.Show("informacion", "por favor, verifique el monto ingresado\n" + pay);
                            }
                            else
                            {
                                if (rbDivisaDolar.IsChecked == true)
                                {
                                    if (_Paymentslist.Exists(pm => pm.Id == 2))
                                    {
                                        PaymentMethod payment = _Paymentslist.Where(pm => pm.Id == 2).FirstOrDefault();
                                        payment.Monto = pay;

                                    }
                                    else
                                    {
                                        _Paymentslist.Add(new PaymentMethod { Id = 2, Lote = "", Monto = pay, NumeroTransaccion = "", CodigoMoneda = "002" });
                                    }
                                }
                                if (rbDivisaEuro.IsChecked == true)
                                {
                                    if (_Paymentslist.Exists(pm => pm.Id == 12))
                                    {
                                        PaymentMethod payment = _Paymentslist.Where(pm => pm.Id == 12).FirstOrDefault();
                                        payment.Monto = pay;

                                    }
                                    else
                                    {
                                        _Paymentslist.Add(new PaymentMethod { Id = 12, Lote = "", Monto = pay, NumeroTransaccion = "", CodigoMoneda = "002" });
                                    }
                                }
                                this.Close();
                            }
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        CustomMessageBox.Show("Error", ex.Message);
                    }

                   
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

      
    }
}
