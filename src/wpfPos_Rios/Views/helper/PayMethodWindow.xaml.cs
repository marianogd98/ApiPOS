using KeyPad;
using Models;
using Models.Vpos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using wpfPos_Rios.service;
using wpfPos_Rios.ViewModels;
using wpfPos_Rios.ViewModels.helperResponse;

namespace wpfPos_Rios.Views.helper
{
    /// <summary>
    /// Interaction logic for PayMethodWindow.xaml
    /// </summary>
    public partial class PayMethodWindow : Window
    {
        //para modificar las monedas y separadores numericos
        NumberFormatInfo _FormatVen = new CultureInfo("es-VE").NumberFormat;

        Regex reglas = new Regex(@"^[0-9a-zA-Za]+$");
        Regex reglasNum = new Regex(@"^[0-9]+$");

        VmInvoice _Invoice;
        List<PaymentMethod> _Paymentslist = new List<PaymentMethod>();
        double _Restante = 0;
        double _Tasa = 0;
        int idForma;
        int _BancoId = 0;
        public PayMethodWindow(List<PaymentMethod> pPaymentslist, int pFormaPago, VmInvoice pInvoice, double pTasa, int pBancoId)
        {
            InitializeComponent();
            _FormatVen.NumberDecimalSeparator = ",";
            _FormatVen.NumberGroupSeparator = ".";
            _FormatVen.CurrencyDecimalSeparator = ",";
            _FormatVen.CurrencyGroupSeparator = ".";
            _FormatVen.CurrencySymbol = "";
            _Invoice = pInvoice;
            idForma = pFormaPago;
            _Paymentslist = pPaymentslist;
            _Tasa = pTasa;
            _BancoId = pBancoId;
            _Restante = _Invoice.Restante;

            switch (pFormaPago)
            {
                case 3:
                    lblPayMethod.Content = "Punto Interno";
                    txtLote.Visibility = Visibility.Visible;
                    cbTypeCard.Visibility = Visibility.Visible;
                    txtNroAuth.Visibility = Visibility.Visible;
                    break;
                case 6:
                    lblPayMethod.Content = "Zelle";
                    _Restante = 0;
                    _Restante = _Invoice.RestanteRef(_Tasa);
                    txtAmmount.Text = _Invoice.RestanteRef(_Tasa).ToString();
                    txtNameZelle.Visibility = Visibility.Visible;
                    break;
                case 7:
                    loadBank();
                    lblPayMethod.Content = "Punto de Venta Externo";
                    txtAmmount.Text = _Invoice.Restante.ToString("C", _FormatVen);
                    txtLote.Visibility = Visibility.Visible;
                    cbTypeCard.Visibility = Visibility.Visible;
                    cbBanco.Visibility = Visibility.Visible;
                    break;
                case 9:
                    lblPayMethod.Content = "Pago de Factura a Credito";
                    txtAmmount.Text = _Invoice.Restante.ToString("C", _FormatVen);
                    txtAmmount.IsEnabled = false;
                    break;
            }

            if (_Restante <= 0)
            {
                var response = CustomMessageBox.Show("Informacion", "Ya cancelo el total de la factura", MessageBoxButton.OK);

                if (response == MessageBoxResult.OK)
                {
                    this.Close();
                }

            }

        }


        #region Eventos Teclado tactil


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

        private bool? showKeypadNum(TextBox pTextbox, Window pIndex)
        {
            KeypadNum keypad = new KeypadNum(pTextbox, pIndex);
            bool? flagKeypad = keypad.ShowDialog();
            if (flagKeypad == true)
            {
                pTextbox.Text = keypad.Result;
            }
            return flagKeypad;

        }
        private bool? showKeyboard(TextBox pTextbox, Window pIndex)
        {
            VirtualKeyboard keyboardwindow = new VirtualKeyboard(pTextbox, pIndex);

            bool? flagKeyboard = keyboardwindow.ShowDialog();

            if (flagKeyboard == true)
            {
                pTextbox.Text = keyboardwindow.Result;
            }

            return flagKeyboard;
        }

        #endregion
        
        void loadBank()
        {
            Response res = new BancoService().GetBancos();
            if (res.success == 1)
            {
                var bancos = JsonConvert.DeserializeObject<List<vmBanco>>(res.data.ToString());
                bancos.ForEach(b =>
                {
                    ComboBoxItem item = new ComboBoxItem();
                    item.Content = b.Name;
                    item.Tag = b.Code;
                    cbBanco.Items.Add(item);
                });

            }
            else
            {
                CustomMessageBox.Show("Error", "No se pudieron cargar los bancos de pago.\n vuelva a cargar la ventana\n" + res.message);
                this.Close();
            }
        }

        string AddPayMethod(double pay, string reference, string lote, string Nombre)
        {
            
            string result = string.Empty;
            try
            {

                if (_Restante > 0)
                {
                    switch (idForma)
                    {
                        case 3:

                            if (pay > _Restante)
                            {
                                result = ("El restante por pagar es: Ref " + _Restante);
                            }
                            else
                            {

                                if (cbTypeCard.SelectedIndex != -1)
                                {
                                    if (!string.IsNullOrEmpty(txtNroAuth.Text))
                                    {
                                        _Paymentslist.Add(
                                            new PaymentMethod
                                            {
                                                Id = 3,
                                                Lote = lote,
                                                Monto = pay,
                                                CuentaBancariaId = 134,
                                                NumeroTransaccion = reference,
                                                CodigoMoneda = "001",
                                                tipoTarjeta = cbTypeCard.Text,
                                                nroAutorizacion = txtNroAuth.Text
                                            });
                                        result = "Agregado";
                                    }
                                    else
                                    {
                                        result = "Ingrese el numero de autorizacion";
                                    }
                                }
                                else
                                {
                                    result = "Debe seleccionar el tipo de tarjeta";
                                }
                            }

                            break;
                        //Zelle
                        case 6:

                            if (pay > _Invoice.RestanteRef(_Tasa))
                            {
                                result = ("El restante por pagar es: Ref " + _Restante);
                            }
                            else
                            {

                                _Paymentslist.Add(new PaymentMethod { Id = 6, Lote = "", Monto = pay, NumeroTransaccion = reference, CodigoMoneda = "002", Nombre = Nombre });

                                result = "Agregado";

                                //cleartxtbPayments();
                            }

                            break;
                        //Punto externo
                        case 7:
                            ComboBoxItem bancoSelected = cbBanco.SelectedValue as ComboBoxItem;
                            if (pay > _Restante)
                            {
                                result = ("El restante por pagar es: " + (_Restante).ToString("C", _FormatVen));
                            }
                            else
                            {
                                try
                                {
                                    if (cbTypeCard.SelectedIndex != -1)
                                    {
                                        if (cbBanco.SelectedIndex != -1)
                                        {
                                            _Paymentslist.Add(new PaymentMethod
                                            {
                                                Id = 7,
                                                CuentaBancariaId = Convert.ToInt32(bancoSelected.Tag.ToString()),
                                                Lote = lote,
                                                Monto = pay,
                                                NumeroTransaccion = reference,
                                                tipoTarjeta = cbTypeCard.Text,
                                                CodigoMoneda = "001"
                                            });
                                            result = "Agregado";
                                        }
                                        else
                                        {
                                            result = "Debe seleccionar el banco";
                                        }
                                    }
                                    else
                                    {
                                        result = "Debe seleccionar el tipo de tarjeta";
                                    }
                                }
                                catch (Exception ex)
                                {
                                    CustomMessageBox.Show("error", ex.Message);
                                }
                            }
                            break;
                        case 9:

                            if (pay > _Restante)
                            {
                                result = ("El restante por pagar es: " + (_Restante).ToString("C", _FormatVen));
                            }
                            else
                            {

                                _Paymentslist.Add(new PaymentMethod { Id = 9, Lote = "", Monto = pay, NumeroTransaccion = reference, CodigoMoneda = "001" });


                                result = "Agregado";
                                //cleartxtbPayments();
                            }

                            break;
                    }

                }
                else
                {
                    // txtbWalletUsd.Text = "00";
                    result = ("No puede procesar mas metodos de pago, ya cancelo el total de la factura");
                }

            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error al calcular el total pagado", ex.Message);
                return null;
            }
            return result;
        }

        //Eventos tactil


        private void txtAmmount_TouchDown(object sender, TouchEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            textbox.Text = "";


            if (idForma == 6)
            {
                //zelle
                bool? keypad = showKeypad(textbox, this);
                if (keypad == true)
                {
                    if (txtAmmount.Text != "")
                    {
                        txtAmmount.Text = textbox.Text;
                    }
                }

            }
            if (idForma == 7 || idForma == 3)
            {
                //externo
                bool? keypad = showKeypadNum(textbox, this);
                if (keypad == true)
                {
                    if (txtAmmount.Text != "")
                    {
                        txtAmmount.Text = string.Format(_FormatVen, "{0:C2}", double.Parse(txtAmmount.Text));
                    }
                }
            }
            if (idForma == 9)
            {
                //Credito
                CustomMessageBox.Show("Información", "No se puede modificar el monto a pagar de una factura a crédito (Debe ser el monto total de la factura).");
                txtAmmount.IsEnabled = false;
            }

        }

        private void txtReference_TouchDown(object sender, TouchEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            textbox.Text = "";


            if (idForma == 7 || idForma == 3)
            {
                bool? keypad = showKeypadNum(textbox, this);

                if (keypad == true)
                {

                    if (!reglasNum.IsMatch(txtReference.Text))
                    {
                        CustomMessageBox.Show("Información", "Referencia de la Transacción en un formato no valido, no se admiten carácteres especiales, solo numericos en la referencia del punto externo");
                        txtReference.Text = "";
                    }
                }
                return;
            }

            if (idForma == 6)
            {
                bool? keypad = showKeyboard(textbox, this);

                if (keypad == true)
                {

                    if (!reglas.IsMatch(txtReference.Text))
                    {
                        CustomMessageBox.Show("Información", "Referencia de la Transacción en un formato no valido, no se admiten carácteres especiales, solo numericos en la referencia del punto externo");
                        txtReference.Text = "";
                    }
                }
                return;
            }

            if (idForma == 9)
            {
                bool? keypad = showKeyboard(textbox, this);

                if (keypad == true)
                {

                    if (txtReference.Text.Length < 5 || txtReference.Text.Length > 50)
                    {
                        CustomMessageBox.Show("Información", "Debe dar una descripción de la transsación a realizar y la referencia debe estar  en un formato válido, no se admiten carácteres especiales en la forma de pago a crédito y deben ser mas de 5 carácteres.");
                        txtReference.Text = "";
                    }
                }
                return;
            }

        }
        //Eventos teclado

        #region Eventos de Botones

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private bool validarPagos()
        {
            bool flag = true;

            if (txtAmmount.Text == "" || txtReference.Text == "")
            {
                return false;
            }

            if ((idForma == 7 || idForma == 3) && txtLote.Text == "")
            {
                return false;
            }

            
            if (idForma == 6 && txtNameZelle.Text == "")
            {
                return false;
            }


            return flag;
        }

        private void btnAcept_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                if (validarPagos() == false)
                {
                    CustomMessageBox.Show("Información", "campos vacios, todos los campos son obligatorios");
                    return;
                }

                else
                {
                    MessageBoxResult response = CustomMessageBox.Show("Confirmar", " ¿Esta seguro que desea registrar esta forma de pago?", MessageBoxButton.YesNo);


                    switch (response.ToString().ToLower())
                    {
                        case "yes":


                            //registrar forma de pago
                            string result;
                            double num;

                            bool pay = double.TryParse(txtAmmount.Text.Replace(".", ""), out num);

                            if (pay)
                            {
                                result = AddPayMethod(num, txtReference.Text, txtLote.Text, txtNameZelle.Text.Trim());

                                if (result == "Agregado")
                                {

                                    // CustomMessageBox.Show("Información", "Metodo de pago registrado!");
                                    this.Close();

                                }
                                else
                                {

                                    CustomMessageBox.Show("Información", result);
                                }
                            }

                            break;
                    }

                }


            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error al agregar metodo de pago", ex.Message);

            }
        }
        #endregion



        private void btnClean_Click_(object sender, RoutedEventArgs e)
        {
            MessageBoxResult response = CustomMessageBox.Show("Confirmar", " ¿Esta seguro que desea limpiar los campos de esta forma de pago?", MessageBoxButton.YesNo);


            switch (response.ToString().ToLower())
            {
                case "yes":
                    //Eliminar Datos de la factura

                    txtAmmount.Text = "";
                    txtReference.Text = "";

                    //validar que no cierre la aplicaacion sino ha habido un cierre de caja

                    break;

            }
        }

        private void txtLote_TouchDown(object sender, TouchEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            textbox.Text = "";

            bool? keypad = showKeypadNum(textbox, this);

            if (keypad == true)
            {

                if (!reglasNum.IsMatch(txtLote.Text))
                {
                    CustomMessageBox.Show("Información", "Lote de la Transacción en un formato no valido, no se admiten carácteres especiales, solo numericos en la referencia del punto externo");
                    txtReference.Text = "";
                }
            }
        }

        private void txtNameZelle_TouchDown(object sender, TouchEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            textbox.Text = "";
            bool? keypad = showKeyboard(textbox, this);

            if (keypad == true)
            {
                txtNameZelle.Text = textbox.Text;
            }
        }

        private void txtNroAuth_TouchDown(object sender, TouchEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            textbox.Text = "";
            bool? keypad = showKeyboard(textbox, this);

            if (keypad == true)
            {
                txtNroAuth.Text = textbox.Text;
            }
        }
    }
}
