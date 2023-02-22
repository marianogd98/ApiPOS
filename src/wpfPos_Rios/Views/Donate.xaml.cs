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
using wpfPos_Rios.service;
using Newtonsoft.Json;
using wpfPos_Rios.ViewModels;
using wpfPos_Rios.ViewModels.helperResponse;
using wpfPos_Rios.Views.helper;
using Models;
using Models.Vpos;
using System.Globalization;
using KeyPad;
using TfhkaNet.IF;
using TfhkaNet.IF.VE;
using Peripherals.printerHKA80;
using System.Configuration;

namespace wpfPos_Rios.Views
{
    /// <summary>
    /// Lógica de interacción para Donate.xaml
    /// </summary>
    public partial class Donate : Window
    {
        readonly Tfhka _Printer;
        readonly PrintVouchers _printVoucher;
        readonly PaymentMethodService _paymentMethod;
        List<VmPaymentMethod> ListPaymentMethod;
        List<vmDonationReason> ListDonationReason;
        readonly VposService _VposService;
        readonly DonationService _donationService;
        readonly VmClient _CLient;
        readonly Donation _Donation;
        readonly NumberFormatInfo _FormatVenezuela = new CultureInfo("es-VE", false).NumberFormat;
        readonly Caja _Caja;
        readonly UserCajera _Cajera;
        public Donate(VmClient client , Caja caja ,UserCajera cajera, Tfhka pPrinterOperatations)
        {
            InitializeComponent();
            _paymentMethod = new PaymentMethodService();
            ListPaymentMethod = new List<VmPaymentMethod>();
            ListDonationReason = new List<vmDonationReason>();
            _VposService = new VposService();
            _donationService = new DonationService();
            _Donation = new Donation();
            _Printer = pPrinterOperatations;
            _printVoucher = new PrintVouchers(_Printer, ConfigPosStatic.ImpresoraConfig.port, ConfigPosStatic.ImpresoraConfig.flat);
            LoadPaymentMethod();
            LoadReasonDonation();
            _Caja = caja;
            _Cajera = cajera;
            _CLient = client;
            _Donation.ClientId = _CLient.Id;
            _Donation.CajaId = caja.Id;
            _Donation.CajeroId = _Cajera.Id;
            _Donation.TiendaId = caja.TiendaId;
            _Donation.TurnoId = _Cajera.idTurno;
            _Donation.Tasa = caja.Tasa;
            _Donation.Estatus = 1;
            txtbNombre.Text = _CLient.Name + " " + _CLient.LastName;
        }

        private void LoadPaymentMethod()
        {
            ListPaymentMethod = new List<VmPaymentMethod>();
            Response res = _paymentMethod.GetPaymentMethodDonate();
            if (res.success == 1)
            {
                ListPaymentMethod = JsonConvert.DeserializeObject<List<VmPaymentMethod>>(res.data.ToString());
                ListPaymentMethod.ForEach(pay =>
                {
                    ComboBoxItem item = new ComboBoxItem
                    {
                        Content = pay.descripcion,
                        Tag = pay.id
                    };
                    cmbFPago.Items.Add(item);
                });

            }
            else
            {
                CustomMessageBox.Show("Error", "No se pudieron cargar los metodos de pago.\n" + res.message);
            }

        }
        private void LoadReasonDonation()
        {
            ListDonationReason = new List<vmDonationReason>();
            Response res = _donationService.GetReasonDonations();
            if (res.success == 1)
            {
                ListDonationReason = JsonConvert.DeserializeObject<List<vmDonationReason>>(res.data.ToString());
                ListDonationReason.ForEach(reason =>
                {
                    ComboBoxItem item = new ComboBoxItem
                    {
                        Content = reason.Nombre + " - " + reason.Motivo,
                        Tag = reason.Id
                    };
                    cmbCausa.Items.Add(item);
                });

            }
            else
            {
                CustomMessageBox.Show("Error", "No se pudieron cargar los metodos de pago.\n" + res.message);
            }

        }



        private void CmbRif_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MessageBox.Show("Recuarda valdiar al cambiar el documento");
        }

        private void CmbFPago_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem itemPaymentMethod = cmbFPago.SelectedValue as ComboBoxItem;
            if (ListPaymentMethod != null)
            {
                var paymentMethod = ListPaymentMethod.Where(pay => pay.id.ToString() == itemPaymentMethod.Tag.ToString()).First();
                CleanAll();
                ShowHideInputs(paymentMethod.id);
            }
        }
        private void CleanAll()
        {
            txtbMonto.Clear();
            txtbRef.Clear();
            txtbLote.Clear();
            txtbCedula.Clear();

            cmbTypeCard.SelectedItem = 0;
            cmbFPago.SelectedItem = 0;
            cmbCausa.SelectedItem = 0;
        }


        private void PMethodZelle()
        {
            lbRef.Visibility = Visibility.Visible;
            txtbRef.Visibility = Visibility.Visible;

            lbLote.Visibility = Visibility.Collapsed;
            txtbLote.Visibility = Visibility.Collapsed;

            lbTypeCard.Visibility = Visibility.Collapsed;
            cmbTypeCard.Visibility = Visibility.Collapsed;

            lbCedula.Visibility = Visibility.Collapsed;
            txtbCedula.Visibility = Visibility.Collapsed;
        }
        private void PMethodPointEx()
        {
            lbRef.Visibility = Visibility.Visible;
            txtbRef.Visibility = Visibility.Visible;

            lbLote.Visibility = Visibility.Visible;
            txtbLote.Visibility = Visibility.Visible;

            lbTypeCard.Visibility = Visibility.Visible;
            cmbTypeCard.Visibility = Visibility.Visible;

            lbCedula.Visibility = Visibility.Collapsed;
            txtbCedula.Visibility = Visibility.Collapsed;
        }
        private void PMethod()
        {
            lbRef.Visibility = Visibility.Collapsed;
            txtbRef.Visibility = Visibility.Collapsed;

            lbLote.Visibility = Visibility.Collapsed;
            txtbLote.Visibility = Visibility.Collapsed;

            lbTypeCard.Visibility = Visibility.Collapsed;
            cmbTypeCard.Visibility = Visibility.Collapsed;

            lbCedula.Visibility = Visibility.Collapsed;
            txtbCedula.Visibility = Visibility.Collapsed;

        }
        private void PMethodPI()
        {
            lbCedula.Visibility = Visibility.Visible;
            txtbCedula.Visibility = Visibility.Visible;
        }


        private void ShowHideInputs(int idPaymentMethod)
        {
            switch (idPaymentMethod)
            {
                //zelle
                case 6:
                    PMethodZelle();
                    break;
                //punto externo
                case 7:
                    PMethodPointEx();
                    break;
                // efectivo(bs , $) y punto interno
                default:
                    PMethod();
                    if (idPaymentMethod == 3)
                        PMethodPI();
                    break;
            }
        }
        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            var res = CustomMessageBox.Show("Salir", "Desea cancelar la donacion?", MessageBoxButton.YesNo);

            switch (res)
            {
                case MessageBoxResult.Yes:
                    this.Close();
                    break;
            }

        }

        private void BtnDonate_Click(object sender, RoutedEventArgs e)
        {
            ComboBoxItem itemPaymentMethod = cmbFPago.SelectedValue as ComboBoxItem;
            
            ProcessDonate(Convert.ToInt32(itemPaymentMethod.Tag.ToString()));
            
        }
        private void ProcessDonate(int paymentMethod)
        {
            if (cmbCausa.SelectedIndex == 0)
            {
                CustomMessageBox.Show("Informacion", "No puede realizar una donacion sin una razon");
            }
            else
            {
                if (string.IsNullOrEmpty(txtbMonto.Text))
                {
                    CustomMessageBox.Show("Informacion", "Debe ingresar el monto");
                }
                else
                {
                    if (Convert.ToDouble(txtbMonto.Text) <=0)
                    {
                        CustomMessageBox.Show("Informacion", "El monto debe ser mayor a cero");
                    }
                    else
                    {
                        switch (paymentMethod)
                        {
                            //zelle
                            case 6:
                                DonateZelle(paymentMethod);
                                break;
                            //punto externo
                            case 7:
                                DonatePointEx( paymentMethod);
                                break;
                            // efectivo(bs , $) y punto interno
                            default:

                                DonatecashPI(paymentMethod);

                                break;

                        }
                    }
                }
            }
        }
        //efectivo o punto interno
        private void DonatecashPI(int paymentMethod)
        {
            PaymentMethod pay;
            Response res;
            ComboBoxItem itemMotivo = cmbCausa.SelectedValue as ComboBoxItem;

            switch (paymentMethod)
                {
                    //punto interno
                    case 3:

                        if (string.IsNullOrEmpty(txtbCedula.Text))
                        {
                            CustomMessageBox.Show("Informacion", "Debe ingresar la cedula");
                        }
                        else
                        {
                            try
                            {
                                VposResponse request = _VposService.BuyWithCard(txtbMonto.Text.ToString(_FormatVenezuela), txtbCedula.Text);
                                if (request.codRespuesta == "00")
                                {
                                    //colocar la coma antes de los dos decimales
                                    string monto = request.montoTransaccion;
                                    string parteEntera = monto.Substring(0, monto.Length - 2);
                                    string parteDecimal = monto.Substring(monto.Length - 2);
                                    pay = new PaymentMethod
                                    {
                                        Id = paymentMethod,
                                        CuentaBancariaId = Convert.ToInt32(request.codigoAdquiriente),
                                        Lote = request.numeroLote,
                                        Monto = Convert.ToDouble(parteEntera + "," + parteDecimal, _FormatVenezuela),
                                        NumeroTransaccion = request.numeroReferencia,
                                        CodigoMoneda = "001",
                                        numSeq = request.numSeq.ToString(),
                                        tipoTarjeta = request.tipoTarjeta,
                                        nroAutorizacion = request.numeroAutorizacion,
                                        vpos = request
                                    };
                                    CustomMessageBox.Show("Informacion", request.mensajeRespuesta, MessageBoxButton.OK);

                                    //CustomMessageBox.Show("Informacion", request.mensajeRespuesta + "\nOkey para imprimir.", MessageBoxButton.OK);
                                    //path = request.nombreVoucher;
                                    //pathCopia = path.Insert((path.Length - 4), "a");

                                    //bool original = imprimirVoucher(path);
                                    //Thread.Sleep(200);
                                    //bool copia = imprimirVoucher(pathCopia,"");

                                    _Donation.OrganizacionId = Convert.ToInt32(itemMotivo.Tag.ToString());
                                    _Donation.PaymnetMethod = pay;

                                    res = _donationService.PostDonate(_Donation);
                                    PrintDonation(_CLient.Rif, _CLient.Name + " " + _CLient.LastName, pay.Monto.ToString() , cmbCausa.Text, _Caja.CodigoCaja, _Cajera.Nombre);
                                    CustomMessageBox.Show("Informacion", res.message);

                                    this.Close();
                                }
                                else
                                {
                                    CustomMessageBox.Show("Error", request.mensajeRespuesta);
                                }
                            }
                            catch (Exception ex)
                            {
                                CustomMessageBox.Show("Error", ex.Message);
                            }
                        }

                        break;
                    //dolar
                    case 2:
                        try
                        {
                            pay = new PaymentMethod()
                            {
                                Id = paymentMethod,
                                Monto = Convert.ToDouble(txtbMonto.Text),
                                CodigoMoneda = "002"
                            };

                            _Donation.OrganizacionId = Convert.ToInt32(itemMotivo.Tag.ToString());
                            _Donation.PaymnetMethod = pay;

                            res = _donationService.PostDonate(_Donation);
                            PrintDonation(_CLient.Rif, _CLient.Name + " " + _CLient.LastName, (pay.Monto * _Donation.Tasa).ToString() , cmbCausa.Text, _Caja.CodigoCaja, _Cajera.Nombre);
                            CustomMessageBox.Show("Informacion", res.message);
                            this.Close();
                        }
                        catch (Exception ex)
                        {
                            CustomMessageBox.Show("Error" , ex.Message);
                        }

                        break;
                    //bs
                    default:

                        try
                        {

                            pay = new PaymentMethod()
                            {
                                Id = paymentMethod,
                                Monto = Convert.ToDouble(txtbMonto.Text),
                                CodigoMoneda = "001"
                            };

                            _Donation.OrganizacionId = Convert.ToInt32(itemMotivo.Tag.ToString());
                            _Donation.PaymnetMethod = pay;
                            res = _donationService.PostDonate(_Donation);

                            PrintDonation(_CLient.Rif, _CLient.Name + " " + _CLient.LastName , pay.Monto.ToString() , cmbCausa.Text , _Caja.CodigoCaja , _Cajera.Nombre);

                            CustomMessageBox.Show("Informacion", res.message);
                            this.Close();
                        }
                        catch(Exception ex)
                        {
                            CustomMessageBox.Show("Error", ex.Message);
                        }
                        break;
                }
            
        }
        private void DonateZelle(int paymentMethod)
        {
            Response res;
            PaymentMethod pay;
            ComboBoxItem itemMotivo = cmbCausa.SelectedValue as ComboBoxItem;
            if (cmbCausa.SelectedIndex == 0)
            {
                CustomMessageBox.Show("Informacion", "No puede realizar una donacion sin una razon");
            }
            else
            {
                if ( string.IsNullOrEmpty(txtbRef.Text))
                {
                    CustomMessageBox.Show("Informacion", "Debe agregar la referencia del Zelle");
                }
                else
                {
                    if (string.IsNullOrEmpty(txtbMonto.Text))
                    {
                        CustomMessageBox.Show("Informacion", "Debe ingresar el monto");
                    }
                    else
                    {
                        pay = new PaymentMethod()
                        {
                            Id = paymentMethod,
                            Monto = Convert.ToDouble(txtbMonto.Text),
                            Nombre = _CLient.Name + " " + _CLient.LastName,
                            NumeroTransaccion = txtbRef.Text,
                            CodigoMoneda = "002"
                        };

                        _Donation.OrganizacionId = Convert.ToInt32(itemMotivo.Tag.ToString());
                        _Donation.PaymnetMethod = pay;
                        res = _donationService.PostDonate(_Donation);
                        PrintDonation(_CLient.Rif, _CLient.Name + " " + _CLient.LastName, (pay.Monto * _Donation.Tasa ).ToString(), cmbCausa.Text, _Caja.CodigoCaja, _Cajera.Nombre);

                        CustomMessageBox.Show("Informacion", res.message);
                        this.Close();
                    }
                }

            }
        }
        private void DonatePointEx(int paymentMethod)
        {
            Response res;
            PaymentMethod pay;
            ComboBoxItem itemMotivo = cmbCausa.SelectedValue as ComboBoxItem;
            if (cmbCausa.SelectedIndex == 0)
            {
                CustomMessageBox.Show("Informacion", "No puede realizar una donacion sin una razon");
            }
            else
            {
                if ( string.IsNullOrEmpty(txtbRef.Text))
                {
                    CustomMessageBox.Show("Informacion", "Debe agregar la referencia");
                }
                else
                {
                    if (string.IsNullOrEmpty(txtbLote.Text))
                    {
                        CustomMessageBox.Show("Informacion", "Debe ingresar el lote de la transsacion");
                    }
                    else
                    {
                        if (cmbTypeCard.SelectedIndex == -1)
                        {
                            CustomMessageBox.Show("Informacion", "Debe seleccionar el tipo de tarjeta");
                        }
                        else
                        {
                           
                            pay = new PaymentMethod()
                            {
                                Id = paymentMethod,
                                Monto = Convert.ToDouble(txtbMonto.Text),
                                tipoTarjeta = cmbTypeCard.Text,
                                Lote = txtbLote.Text,
                                NumeroTransaccion = txtbRef.Text,
                                CodigoMoneda = "001"
                            };

                            _Donation.OrganizacionId = Convert.ToInt32(itemMotivo.Tag.ToString());
                            _Donation.PaymnetMethod = pay;
                            res = _donationService.PostDonate(_Donation);
                            PrintDonation(_CLient.Rif, _CLient.Name + " " + _CLient.LastName, pay.Monto.ToString(), cmbCausa.Text, _Caja.CodigoCaja, _Cajera.Nombre);

                            CustomMessageBox.Show("Informacion", res.message);
                            this.Close();
                        }
                    }
                }
            }
        }

        private void PrintDonation(string Ci , string nombre , string monto , string motivo  , string caja , string cajera)
        {
            try
            {
                if (_Printer.CheckFPrinter())
                {
                    string Head = "**** DONACION ****";

                    string[] voucher = { Head, "Cajera: " + cajera, "Caja: " + caja, nombre + " " + Ci, "Usted ha donado a : ", motivo, "Monto: " + monto };
                    bool printVoucher = _printVoucher.PrintVoucher(voucher);
                }
                else
                {
                    PrinterStatus status = _Printer.GetPrinterStatus();
                    CustomMessageBox.Show($"Error en la Impresora Fiscal : {status.PrinterErrorCode}", status.PrinterStatusDescription + "\n" + status.PrinterErrorDescription);
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error" , ex.Message);
            }
            
        }

        private void ShowKeypad_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            ShowKeypad(textbox , this);
        }
        private bool? ShowKeypad(TextBox pTextbox, Window pIndex)
        {
            KeypadNum keypad = new KeypadNum(pTextbox, pIndex);

            bool? flagKeypad = keypad.ShowDialog();

            if (flagKeypad == true)
            {
                pTextbox.Text = keypad.Result;
            }

            return flagKeypad;

        }

        private void TxtbRef_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            ShowKeyboard(textbox, this);
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

    }
}
