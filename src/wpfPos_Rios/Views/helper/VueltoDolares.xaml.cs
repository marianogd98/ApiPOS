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
    /// Lógica de interacción para VueltoDolares.xaml
    /// </summary>
    public partial class VueltoDolares : Window
    {
        List<PaymentMethod> _Paymentslist;
        VmInvoice _Invoice;
        double _Tasa;

        public VueltoDolares(List<PaymentMethod> pPaymentslist, VmInvoice pInvoice, double pTasa)
        {
            InitializeComponent();
            _Paymentslist = new List<PaymentMethod>();
            _Invoice = pInvoice;
            _Tasa = pTasa;
            _Paymentslist = pPaymentslist;

            double vuelto = _Invoice.RetornoRef(pTasa);
            int vueltoEntero = (int)vuelto;

            lbVuelto.Content = vueltoEntero.ToString();
            txtbVueltoRef.Text = vueltoEntero.ToString();

        }

        private void txtbVueltoRef_TouchDown(object sender, TouchEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            textbox.Text = "";
            bool? keypad = showKeypad(textbox, this);
            if (keypad == true)
            {
                if (textbox.Text != "")
                {
                    try
                    {
                        double vuelto;
                        bool flag = double.TryParse(txtbVueltoRef.Text , out vuelto);

                        SetVuelto(vuelto);
                    }
                    catch (Exception ex)
                    {
                        CustomMessageBox.Show("Error" , ex.Message);
                    }
                   
                }

            }
        }
        public void SetVuelto(double amount)
        {
            double vuelto = _Invoice.RetornoRef(_Tasa);
            if (amount <= vuelto)
            {
                txtbVueltoRef.Text = amount.ToString();
            }
            else
            {
                CustomMessageBox.Show("Advertencia", "El vuelto es " + vuelto + " " + "No puede dar un vuelto mayor.");
                txtbVueltoRef.Text = "00";
            }
        }
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            var option = CustomMessageBox.Show("Confirmar", "Cerrar ventana?" , MessageBoxButton.YesNo);

            switch (option)
            {
                case MessageBoxResult.Yes:
                    this.Close();
                    break;
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double vuelto = _Invoice.RetornoRef(_Tasa);
                double amountVuelto;
                bool flag = double.TryParse(txtbVueltoRef.Text , out amountVuelto);

                if (amountVuelto > vuelto)
                {
                    CustomMessageBox.Show("Advertencia", "El vuelto es " + vuelto + " " + "No puede devolver un monto mayor en efectivo $.");
                    txtbVueltoRef.Text = Convert.ToInt32(vuelto).ToString();
                }
                else
                {
                    MessageBoxResult response = CustomMessageBox.Show("Confirmar", "Entrega de vuelto:\n" + "\n Efectivo $: " + amountVuelto, MessageBoxButton.YesNo);
                    switch (response.ToString().ToLower())
                    {
                        case "yes":
                            amountVuelto *= -1;

                            //vuelto Wallet $.
                            if (_Paymentslist.Exists(pm => pm.Id == 5 && pm.Monto < 0))
                            {
                                PaymentMethod payment = _Paymentslist.Where(pm => pm.Id == 5).FirstOrDefault();
                                payment.Monto = amountVuelto;
                            }
                            else
                            {
                                _Paymentslist.Add(new PaymentMethod { Id = 5, Lote = "", Monto = amountVuelto, NumeroTransaccion = "", CodigoMoneda = "002" });
                            }

                            this.Close();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
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

    }
}
