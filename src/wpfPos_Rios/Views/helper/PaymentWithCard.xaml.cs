using KeyPad;
using Models;
using Models.Vpos;
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
using wpfPos_Rios.service;
using Newtonsoft.Json;
using System.IO;
using System.Threading;
using wpfPos_Rios.helper;
using TfhkaNet.IF;
using TfhkaNet.IF.VE;
using Peripherals.printerHKA80;
using System.Configuration;
using Path = System.IO.Path;

namespace wpfPos_Rios.Views.helper
{
    /// <summary>
    /// Lógica de interacción para PaymentWithCard.xaml
    /// </summary>
    public partial class PaymentWithCard : Window
    {
        readonly Log log;
        readonly VposService _VposService;
        readonly NumberFormatInfo _FormatVenezuela = new CultureInfo("es-VE", false).NumberFormat;

        //para modificar las monedas y separadores numericos
        readonly List<PaymentMethod> _Paymentslist = new List<PaymentMethod>();
        readonly Tfhka _Printer;
        readonly PrintVouchers _printVoucher;
        double _Restante = 0;
        double _Tasa = 0;
        public PaymentWithCard(List<PaymentMethod> pPaymentslist, double pRestante, double pTasa, Tfhka pPrinter, string pCedula)
        {
            InitializeComponent();
            _FormatVenezuela.NumberDecimalSeparator = ",";
            _FormatVenezuela.NumberGroupSeparator = ".";
            _FormatVenezuela.CurrencyDecimalSeparator = ",";
            _FormatVenezuela.CurrencyGroupSeparator = ".";

            _Printer = pPrinter;
            _printVoucher = new PrintVouchers(_Printer, ConfigPosStatic.ImpresoraConfig.port, ConfigPosStatic.ImpresoraConfig.flat);
            log = new Log();
            _VposService = new VposService();
            _Paymentslist = pPaymentslist;
            _Tasa = pTasa;
            _Restante = pRestante;

            txtCedula.Text = pCedula;

            txtAmmount.Text = string.Format(_FormatVenezuela, "{0:N2}", _Restante);
        }

        #region Eventos Teclado tactil


        private bool? ShowKeypad(TextBox pTextbox, Window pIndex)
        {
            Keypad keypad = new Keypad(pTextbox, pIndex);
            bool? flagKeypad = keypad.ShowDialog();
            if (flagKeypad == true)
            {
                pTextbox.Text = keypad.Result;
            }
            return flagKeypad;

        }

        private bool? ShowKeypadNum(TextBox pTextbox, Window pIndex)
        {
            KeypadNum keypad = new KeypadNum(pTextbox, pIndex);
            bool? flagKeypad = keypad.ShowDialog();
            if (flagKeypad == true)
            {
                pTextbox.Text = keypad.Result;
            }
            return flagKeypad;

        }
        private bool? ShowKeyboard(TextBox pTextbox, Window pIndex)
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

        private void TxtAmmount_TouchDown(object sender, TouchEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            textbox.Text = "";

            bool? keypad = ShowKeypad(textbox, this);
            if (keypad == true)
            {
                if (txtAmmount.Text != "")
                {
                    bool pay = double.TryParse(string.Format(_FormatVenezuela, "{0:n}", Convert.ToDouble(txtAmmount.Text)), out double num);
                    if (pay)
                    {
                        if (num <= _Restante)
                        {
                            txtAmmount.Text = string.Format(_FormatVenezuela, "{0:N2}", num);
                        }
                        else
                        {
                            CustomMessageBox.Show("informacion", "El monto a pagar es de " + _Restante);
                            txtAmmount.Text = string.Format(_FormatVenezuela, "{0:N2}", _Restante);
                        }
                    }
                    else
                    {
                        CustomMessageBox.Show("Error", "Formato invalido en el monto");
                    }

                }
            }
        }

        private void TxtAmmount_KeyDown(object sender, KeyEventArgs e)
        {

            if (Key.Enter == e.Key)
            {
                if (txtAmmount.Text != "")
                {
                    try
                    {
                        bool pay = double.TryParse(string.Format(_FormatVenezuela, "{0:n}", Convert.ToDouble(txtAmmount.Text)), out double num);

                        if (pay)
                        {
                            if (num <= _Restante)
                            {
                                txtAmmount.Text = string.Format(_FormatVenezuela, "{0:N2}", num);
                            }
                            else
                            {
                                CustomMessageBox.Show("informacion", "El monto a pagar es de " + _Restante);
                                txtAmmount.Text = string.Format(_FormatVenezuela, "{0:N2}", _Restante);
                            }
                        }
                        else
                        {
                            CustomMessageBox.Show("Error", "Formato invalido en el monto");
                        }
                    }
                    catch (Exception ex)
                    {
                        CustomMessageBox.Show("Error con el monto", ex.Message);
                    }



                }

            }



        }

        private void TxtCedula_TouchDown(object sender, TouchEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            textbox.Text = "";

            bool? keypad = ShowKeypadNum(textbox, this);

            if (keypad == true)
            {
                if (txtCedula.Text != "")
                {
                    txtCedula.Text = textbox.Text;
                }
            }
        }

        private bool ImprimirVoucher(string path, string pDescripcion)
        {
            bool flag = false;
            string contenido;
            string[] voucher;
            if (File.Exists(path))
            {
                contenido = File.ReadAllText(path);
                voucher = contenido.Split('\n');

                if (_Printer.CheckFPrinter())
                {
                    bool printVoucher = _printVoucher.PrintVoucherMerchant(voucher);
                    flag = printVoucher;
                }
                else
                {
                    PrinterStatus status = _Printer.GetPrinterStatus();
                    CustomMessageBox.Show($"Error en la Impresora Fiscal : {status.PrinterErrorCode}", status.PrinterStatusDescription + "\n" + status.PrinterErrorDescription);
                }

            }
            else
            {
                CustomMessageBox.Show("Informacion", "No existe el voucher en el equipo");
            }
            return flag;
        }

        private void BtnAcept_Click(object sender, RoutedEventArgs e)
        {

            string pathCopia;
            string path;

            if (txtAmmount.Text != "" && txtCedula.Text != "")
            {
                MessageBoxResult response = CustomMessageBox.Show("Confirmar", " ¿Esta seguro que desea registrar esta forma de pago?", MessageBoxButton.YesNo);

                switch (response.ToString().ToLower())
                {
                    case "yes":
                        //registrar forma de pago

                        try
                        {
                            if (_Restante >= Convert.ToDouble(txtAmmount.Text))
                            {
                                VposResponse request =  _VposService.BuyWithCard(txtAmmount.Text.ToString(_FormatVenezuela), txtCedula.Text);
                                if (request.codRespuesta == "00")
                                {
                                    //colocar la coma antes de los dos decimales
                                    string monto = request.montoTransaccion;
                                    string parteEntera = monto.Substring(0, monto.Length - 2);
                                    string parteDecimal = monto.Substring(monto.Length - 2);
                                    
                                    path = Path.GetFullPath(request.nombreVoucher);
                                    pathCopia = path.Insert((path.Length - 4), "a");
                                    request.nombreVoucher = path;
                                    _Paymentslist.Add(
                                        new PaymentMethod
                                        {
                                            Id = 3,
                                            CuentaBancariaId = Convert.ToInt32(request.codigoAdquiriente),
                                            Lote = request.numeroLote,
                                            Monto = Convert.ToDouble(parteEntera + "," + parteDecimal, _FormatVenezuela),
                                            NumeroTransaccion = request.numeroReferencia,
                                            CodigoMoneda = "001",
                                            numSeq = request.numSeq.ToString(),
                                            tipoTarjeta = request.tipoTarjeta,
                                            nroAutorizacion = request.numeroAutorizacion,
                                            vpos = request
                                        });
                                    //CustomMessageBox.Show("Informacion", request.mensajeRespuesta, MessageBoxButton.OK);
                                    CustomMessageBox.Show("Informacion", request.mensajeRespuesta + "\nOkey para imprimir.", MessageBoxButton.OK);
                                    path = request.nombreVoucher;
                                    pathCopia = path.Insert((path.Length - 4), "a");

                                    bool original = ImprimirVoucher(path, "");
                                    Thread.Sleep(200);
                                    //bool copia = imprimirVoucher(pathCopia, "");
                                    //Thread.Sleep(200);
                                    this.Close();
                                }
                                else
                                {
                                    if (request.tipoTarjeta == "C")
                                    {
                                        ImprimirVoucher(request.nombreVoucher , "");
                                    }
                                    CustomMessageBox.Show("Error", $"{request.mensajeRespuesta}");
                                    
                                }
                            }
                            else
                            {
                                CustomMessageBox.Show("Advertencia", "El monto a pagar es :" + _Restante.ToString(_FormatVenezuela) + "\nEspecifique los decimales con coma(,)");
                            }

                        }
                        catch (Exception ex)
                        {
                            log.Add(ex.Message + "\n" + ex.StackTrace.ToString());
                            CustomMessageBox.Show("Informacion", ex.Message);
                        }

                        break;
                }

            }
            else
            {
                CustomMessageBox.Show("Información", " Estimado usuario debe Ingresar los datos de la forma de pago");
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnClean_Click_(object sender, RoutedEventArgs e)
        {
            MessageBoxResult response = CustomMessageBox.Show("Confirmar", " ¿Esta seguro que desea limpiar los campos de esta forma de pago?", MessageBoxButton.YesNo);

            switch (response.ToString().ToLower())
            {
                case "yes":
                    //Eliminar Datos de la factura
                    txtAmmount.Text = "";
                    txtCedula.Text = "";
                    //validar que no cierre la aplicaacion sino ha habido un cierre de caja
                    break;

            }
        }


    }
}
