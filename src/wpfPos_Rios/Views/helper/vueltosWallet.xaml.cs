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

namespace wpfPos_Rios.Views.helper
{
    /// <summary>
    /// Lógica de interacción para vueltosWallet.xaml
    /// </summary>
    public partial class vueltosWallet : Window
    {

        List<PaymentMethod> _Paymentslist;
        VmInvoice _Invoice;
        double _Tasa;

        public vueltosWallet(List<PaymentMethod> pPaymentslist, VmInvoice pInvoice, double pTasa)
        {
            InitializeComponent();
            _Paymentslist = new List<PaymentMethod>();
            _Invoice = pInvoice;
            _Tasa = pTasa;
            _Paymentslist = pPaymentslist;


            double vuelto = _Invoice.RetornoRef(pTasa);

            lbVuelto.Content = vuelto.ToString();
            txtbVueltoWallet.Text = vuelto.ToString();
        }
       
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double vuelto = _Invoice.RetornoRef(_Tasa);
                double wallet = Convert.ToDouble(txtbVueltoWallet.Text);

                if (wallet > vuelto)
                {
                    CustomMessageBox.Show("Advertencia", "El vuelto es " + vuelto + " " + "No puede guardar en el wallet un monto mayor.");
                    txtbVueltoWallet.Text = (vuelto).ToString();
                }
                else
                {
                    MessageBoxResult response = CustomMessageBox.Show("Confirmar", "Va a Guardar en el Wallet:\n" + "\n Wallet $: " + wallet, MessageBoxButton.YesNo);
                    switch (response.ToString().ToLower())
                    {
                        case "yes":
                            wallet *= -1;

                            //vuelto Wallet $.
                            if (_Paymentslist.Exists(pm => pm.Id == 4 && pm.Monto < 0))
                            {
                                PaymentMethod payment = _Paymentslist.Where(pm => pm.Id == 4 && pm.Monto < 0).FirstOrDefault();
                                payment.Monto = wallet;
                            }
                            else
                            {
                                _Paymentslist.Add(new PaymentMethod { Id = 4, Lote = "", Monto = wallet, NumeroTransaccion = "", CodigoMoneda = "002" });
                            }

                            this.Close();
                            break;
                    }
                }
            }catch(Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }

        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public void SetVueltoWallet(double amount)
        {
            double wallet = 0;
            double vuelto = _Invoice.RetornoRef(_Tasa);
            if (amount <= vuelto)
            {
                wallet = amount;
                txtbVueltoWallet.Text = wallet.ToString();
            }
            else
            {
                CustomMessageBox.Show("Advertencia", "El vuelto es " + vuelto + " " + "No puede guardar en el wallet un monto mayor.");
                txtbVueltoWallet.Text = "00";
            }
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

        private void txtbVueltoWallet_KeyDown(object sender, KeyEventArgs e)
        {

            try
            {
                if (Key.Enter == e.Key)
                {
                    if (txtbVueltoWallet.Text != "")
                    {
                        double wallet = Convert.ToDouble(txtbVueltoWallet.Text);

                        SetVueltoWallet(wallet);
                    }
                }
            }
            catch { }
        }

        private void txtbVueltoWallet_TouchDown(object sender, TouchEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            textbox.Text = "";
            bool? keypad = showKeypad(textbox, this);
            if (keypad == true)
            {
                if (textbox.Text != "")
                {
                    double wallet = Convert.ToDouble(txtbVueltoWallet.Text);

                    SetVueltoWallet(wallet);
                }

            }
        }
    }
}
