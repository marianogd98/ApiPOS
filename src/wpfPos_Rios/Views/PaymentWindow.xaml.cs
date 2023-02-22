using KeyPad;
using Models;
using Newtonsoft.Json;
using System.Threading;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using wpfPos_Rios.service;
using wpfPos_Rios.ViewModels;
using wpfPos_Rios.ViewModels.helperResponse;
using wpfPos_Rios.Views.helper;
using System.Configuration;
using wpfPos_Rios.service.Security;
using wpfPos_Rios.ViewModels.customerDisplay;
using System.IO;
using System.Diagnostics;
using wpfPos_Rios.helper;
using TfhkaNet.IF;
using TfhkaNet.IF.VE;
using Peripherals.printerHKA80;

namespace wpfPos_Rios.Views
{
    /// <summary>
    /// Lógica de interacción para PaymentWindow.xaml
    /// </summary>
    public partial class PaymentWindow : Window
    {
        readonly Log log;

        //servicios y respuesta
        readonly InvoiceService _serviceInvoice;

        //modelos de seguridad
        readonly SecurityService _securityService;

        //servicio seguridad wallet
        readonly SeguridadWalletService _serviceSecurityWallet;

        //viewmodels
        readonly VmInvoice _Invoice = new VmInvoice();

        //VmCajera _Cajera = new VmCajera();
        readonly VmClient _Client = new VmClient();

        //modelo

        readonly List<PaymentMethod> _paymentslist;
        readonly Caja _cajaData;
        readonly UserCajera _Cajera;
        readonly NumberFormatInfo _FormatVenezuela = new CultureInfo("es-VE").NumberFormat;

        //PrinterOperations _PrinterOperatations;
        readonly Tfhka _printer;
        readonly PrintVouchers _printVoucher;

        //Modelo del estado de la impresora

        //cache de factura
        readonly CacheJsonInvoice cacheInvoice;

        //customer display
        readonly SendCustomerDisplay _CustomerD = new SendCustomerDisplay();

        
        public PaymentWindow(VmInvoice pinvoice, VmClient pClient, List<PaymentMethod> ppayments, Tfhka pprinter, Caja pcaja , UserCajera pCajera)
        {
            InitializeComponent();
            SetFormatMoneyNumeric();
            //Inicializar la seguridad
            _securityService = new SecurityService();
            _serviceInvoice = new InvoiceService();
            _serviceSecurityWallet = new SeguridadWalletService();

            _printer = pprinter;
            _printVoucher = new PrintVouchers(_printer , ConfigPosStatic.ImpresoraConfig.port, ConfigPosStatic.ImpresoraConfig.flat);
            _cajaData = pcaja;
            _Cajera = pCajera;
            btnVueltoRef.IsEnabled = false;
            btnPrint.IsEnabled = false;
            btnWallet.IsEnabled = false;
            log = new Log();
            cacheInvoice = new CacheJsonInvoice();

            _Invoice = pinvoice;
            _Client = pClient;
            _paymentslist = ppayments;

            lbBalanceWallet.Content = "Ref. " + pClient.Saldo;
            lblClientName.Content = _Client.Name + " " + _Client.LastName;

            lbTasa.Content = _cajaData.Tasa.ToString("C", _FormatVenezuela);
            CalculateCancelled();
            InitializePay();
            //canPayWallet();
        }



        #region Metodos Helper

        public string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;

            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        private void CachearInvoice(VmInvoice invoice, List<PaymentMethod> paymentlist, Caja _cajaData, VmClient client)
        {
            try
            {
                cacheInvoice.WriteInvoice(invoice, paymentlist, _cajaData, client);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error al guardar cache de factura", ex.Message);
            }
        }
        public void SetFormatMoneyNumeric()
        {
            //venezuela
            _FormatVenezuela.CurrencyGroupSeparator = ".";
            _FormatVenezuela.CurrencyDecimalSeparator = ",";
            _FormatVenezuela.CurrencySymbol = "";
            _FormatVenezuela.NumberGroupSeparator = ".";
            _FormatVenezuela.NumberDecimalSeparator = ",";

        }

        #endregion

        #region Metodos de Iniciar y actualizar la vista

        private void InitializePay()
        {
            double vueltoRef = (_Invoice.Cancelled > _Invoice.Total) ? _Invoice.RetornoRef(_cajaData.Tasa) : 0;

            TxtTotal.Text = (_Invoice.Total).ToString("C", _FormatVenezuela);
            TxtCancelado.Text = (_Invoice.Cancelled).ToString("C", _FormatVenezuela);
            TxtRestante.Text = (_Invoice.Total - _Invoice.Cancelled).ToString("C", _FormatVenezuela);

            TxtRefTotal.Text = "ref." + _Invoice.TotalRef(_cajaData.Tasa).ToString();
            TxtRefCancelado.Text = "ref." + _Invoice.CancelledRef(_cajaData.Tasa).ToString();
            TxtRefRestante.Text = "ref." + (vueltoRef).ToString();

            LoadListView();

        }

        public void LoadListView()
        {
            _paymentslist.RemoveAll(pay => pay.Monto == 0);
            UpdateLbPayments();
            dgPaymentList.ItemsSource = _paymentslist;
            dgPaymentList.Items.Refresh();
        }

        public void UpdateLbPayments()
        {
            double vueltoRef = (_Invoice.Cancelled > _Invoice.Total) ? _Invoice.RetornoRef(_cajaData.Tasa) : 0;

            TxtRestante.Text = _Invoice.Restante.ToString("N", _FormatVenezuela);
            TxtCancelado.Text = _Invoice.Cancelled.ToString("C", _FormatVenezuela);
            TxtVuelto.Text = _Invoice.Retorno.ToString("C", _FormatVenezuela);

            TxtRefCancelado.Text = "ref." + _Invoice.CancelledRef(_cajaData.Tasa).ToString();
            TxtRefRestante.Text = "ref." + _Invoice.RestanteRef(_cajaData.Tasa).ToString();
            TxtRefVuelto.Text = "ref." + vueltoRef.ToString();
        }

        #endregion


        #region Eventos de Botones Principales

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {

            if (!(_paymentslist.Count > 1))
            {
                _paymentslist.RemoveAll(pay => pay.Id == 4);
            }
            else
            {
                _paymentslist.RemoveAll(pay => pay.Id != 1 && pay.Id != 2 && pay.Id != 3 && pay.Id != 7 && pay.Id != 6);
            }


            CalculateCancelled();

            this.Hide();
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult response = MessageBox.Show("Deseas Limpiar los datos del pago de la factura" + " " + _cajaData.NControl + " " + "?", "Confirmar", MessageBoxButton.YesNoCancel);

            switch (response.ToString().ToLower())
            {
                case "yes":
                    //Eliminar Datos de la factura

                    InitializePay();

                    break;
            }
        }

        private void BtnWallet_Click(object sender, RoutedEventArgs e)
        {
            double vuelto = 0;
            double pagado = 0;

            // ver si hay metodo de pago 
            if (_paymentslist.Count > 0)
            {
                if (_paymentslist.Exists(pay => pay.Id == 2 || pay.Id == 12))
                {

                    foreach (var pay in _paymentslist)
                    {
                        if (pay.CodigoMoneda == "002")
                        {
                            pagado += pay.Monto * _cajaData.Tasa;
                        }
                        else
                        {
                            pagado += pay.Monto;
                        }
                    }

                    vuelto = _Invoice.Cancelled - _Invoice.Total;

                    if (vuelto > 0 && _paymentslist.Exists(pm => pm.Id == 2 || pm.Id == 12))
                    {
                        SetWallet(_paymentslist, _Invoice);
                    }
                }
                else
                {
                    CustomMessageBox.Show("Informacion", "No se puede guardar en wallet. El cliente no cancelo con divisas", MessageBoxButton.OK);
                }
            }
        }

        private void VueltosWallet()
        {
            double vuelto = 0;
            double pagado = 0;

            // ver si hay metodo de pago 
            if (_paymentslist.Count > 0)
            {
                if (_paymentslist.Exists(pay => pay.Id == 2 || pay.Id == 12))
                {

                    _paymentslist.ForEach(pay =>
                    {
                        if (pay.CodigoMoneda == "002")
                        {
                            pagado += pay.Monto * _cajaData.Tasa;
                        }
                        else
                        {
                            pagado += pay.Monto;
                        }
                    });

                    vuelto = _Invoice.Cancelled - _Invoice.Total;

                    if (vuelto > 0 && _paymentslist.Exists(pm => pm.Id == 2 || pm.Id == 12))
                    {
                        SetWallet(_paymentslist, _Invoice);
                    }
                }
                else
                {
                    CustomMessageBox.Show("Informacion", "No se puede almacenar en el wallet $. El cliente no cancelo con efectivo $.", MessageBoxButton.OK);
                }
            }
        } 

        private void BtnVueltoRef_Click(object sender, RoutedEventArgs e)
        {

            CalculateCancelled();
            double vuelto = _Invoice.RetornoRef(_cajaData.Tasa);
            // ver si hay metodo de pago 
            if (_paymentslist.Count > 0)
            {
                if (_paymentslist.Exists(pay => pay.Id == 2 || pay.Id == 12))
                {
                    if (vuelto > 0 && _paymentslist.Exists(pm => pm.Id == 2 || pm.Id == 12))
                    {
                        VueltoDolares vueltosEfectivoRef = new VueltoDolares(_paymentslist, _Invoice, _cajaData.Tasa);
                        vueltosEfectivoRef.ShowDialog();
                        CalculateCancelled();
                        LoadListView();
                    }
                }
                else
                {
                    CustomMessageBox.Show("Informacion", "No se puede dar vuelto en efectivo $. El cliente no cancelo con $.", MessageBoxButton.OK);
                }
            }
        }

        private void VueltosRef()
        {
            CalculateCancelled();
            double vuelto = _Invoice.RetornoRef(_cajaData.Tasa);
            // ver si hay metodo de pago 
            if (_paymentslist.Count > 0)
            {
                if (_paymentslist.Exists(pay => pay.Id == 2))
                {
                    if (vuelto > 0 && _paymentslist.Exists(pm => pm.Id == 2))
                    {
                        VueltoDolares vueltosEfectivoRef = new VueltoDolares(_paymentslist, _Invoice, _cajaData.Tasa);
                        vueltosEfectivoRef.ShowDialog();

                        CalculateCancelled();
                        LoadListView();
                    }
                }
                else
                {
                    CustomMessageBox.Show("Informacion", "No se puede dar vuelto en efectivo $. El cliente no cancelo con $.", MessageBoxButton.OK);
                }
            }
        }


        private void ClosePayment()
        {
            _paymentslist.Clear();
            _Invoice.ResetInvoice();
            _Client.resetData();
            this.Close();
        }

        private void PrintUpdateInvoice(Invoice pInvoice , VmClient pClient , UserCajera pCajera, int idFactura , string saldoActual = "-1")
        {
            try
            {
                bool print;
                print = _printVoucher.CmdImpFactura(pInvoice.NumeroControl,$"{_Client.Name} {_Client.LastName}", _Client.Rif
                                   , _Client.Address, pCajera.Nombre, _cajaData.CodigoCaja, "101"
                                   , pInvoice.listaProductos, pInvoice.listaPagos, saldoActual, _Client.Saldo.ToString());

                if (print && pInvoice.NumeroFactura == _printer.GetS1PrinterData().LastInvoiceNumber.ToString("00000000"))
                {
                    int espera = pInvoice.listaProductos.Count() / 2;
                    Thread.Sleep(espera * 150);
                    //update invoice en bd
                    UpdateInvoice(idFactura);
                    CustomMessageBox.Show("Confirmacion", "Termino la impresion de la factura ?", MessageBoxButton.OK);
                    Thread.Sleep(1000);
                    PrintCopyClient(pInvoice);

                    
                    string numInvoice = _printer.GetS1PrinterData().LastInvoiceNumber.ToString("00000000");
                    PrintPromo($"{pClient.Name} {pClient.LastName}", pClient.Rif, $"{pClient.Cellnumber}", numInvoice, $"{_Cajera.Nombre}", _cajaData.CodigoCaja , pInvoice);

                    
                    ClosePayment();
                }
                else
                {
                    PrinterStatus status = _printer.GetPrinterStatus();
                    CustomMessageBox.Show($"Error en la Impresora Fiscal : {status.PrinterErrorCode}", status.PrinterStatusDescription + "\n" + status.PrinterErrorDescription);
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        
        private void PrintPromo(string cliente, string pRif ,string telefono , string factura , string cajera , string numeroCaja , Invoice pInvoice)
        {
            try
            {
                Response promoData = _serviceInvoice.GetPromocion(pInvoice);

                if(promoData.success == 1)
                {
                    int cantPrint = Convert.ToInt32(promoData.data);

                    List<string> promocion = new List<string>
                    {
                        $"",
                        $"Cajera: {cajera} caja: {numeroCaja} ",
                        $"Factura Nro: {factura}",
                        $"Cliente: {cliente}",
                        $"Documento nro: {pRif}",
                        $"tlfn: {telefono}",
                        $"",
                        $"Usted esta participando PROMO DIA DEL NIÑO",
                        $"",
                        $"Gracias por preferirnos",
                        $"@rio_supermarket"
                    };

                    for (int i = 0; i < cantPrint; i++)
                    {
                        _printVoucher.PrintPromocion(promocion);
                        Thread.Sleep(1200);
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error al imprimir promocion", ex.Message);
            }
        }
        private void CheckIn(Invoice _InvoiceModels , string numInvoice)
        {
            try
            {
                //guardar imprimir actualizar
                Response resInsertInvoice = _serviceInvoice.InsertInvoice(_InvoiceModels);

                if (resInsertInvoice.success == 1)
                {
                    cacheInvoice.DeleteArchive();
                    FacturaIdNControlResponse dataInsertResponse = JsonConvert.DeserializeObject<FacturaIdNControlResponse>(resInsertInvoice.data.ToString());
                    //pasar id de factura e numero factura al modelo
                    _Invoice.Id = dataInsertResponse.facturaId;
                    _Invoice.NumeroFactura = numInvoice;
                    string saldoActual = dataInsertResponse.saldoActual.ToString();
                    _cajaData.NControl = dataInsertResponse.numeroControl;

                    PrintUpdateInvoice(_InvoiceModels, _Client, _Cajera, _Invoice.Id, saldoActual);

                }
                else
                {
                    CustomMessageBox.Show("Error al insertar la factura", $"{resInsertInvoice.message}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            
            string numInvoice = string.Empty;
            string serialFicalPrinter = string.Empty;
            #region validaciones de vuelto
            if (_paymentslist.Exists(p => p.Id ==2 || p.Id == 12))
            {
                if (_Invoice.RetornoRef(_cajaData.Tasa) > 1)
                {
                    var returnRef = CustomMessageBox.Show("Advertencia", "Tiene un vuelto pendiente por definir de Ref" + "\"" + _Invoice.RetornoRef(_cajaData.Tasa) + "\"" + " " + "Lo devolvera en efectivo $?.", MessageBoxButton.YesNo);
                    if (returnRef == MessageBoxResult.Yes)
                    {
                        VueltosRef();
                        return;   
                    }
                    else
                    {
                        returnRef = CustomMessageBox.Show("Advertencia", "Tiene un vuelto pendiente por definir de Ref" + "\"" + _Invoice.RetornoRef(_cajaData.Tasa) + "\"" + " " + "Lo agregara al wallet del cliente ?", MessageBoxButton.YesNo);
                        if (returnRef == MessageBoxResult.Yes)
                        {
                            VueltosWallet();
                            return;
                        }
                    }
                }
                else
                {
                    if (_Invoice.RetornoRef(_cajaData.Tasa) > 0 && _Invoice.RestanteRef(_cajaData.Tasa) < 1 )
                    {
                        var returnRef = CustomMessageBox.Show("Advertencia", "Tiene un vuelto pendiente por definir de " + "\"" + _Invoice.RetornoRef(_cajaData.Tasa) + "\"" + " " + "Lo agregara al wallet del cliente ?", MessageBoxButton.YesNo);
                        if (returnRef == MessageBoxResult.Yes)
                        {
                            VueltosWallet();
                            return;
                        }
                    }
                }
            }

            if(_Invoice.Retorno > 0)
            {
                var confirmRetorno = CustomMessageBox.Show("Advertencia", "Hay un vuelto en bs de " + "\"" + _Invoice.Retorno.ToString("N" , _FormatVenezuela) + "\"" + " " + "Desea continuar con la facturacion ?", MessageBoxButton.YesNo);
                if (confirmRetorno == MessageBoxResult.No)
                {
                    return;
                }
            }
            #endregion
            if (_Invoice.Cancelled < _Invoice.Total)
            {
                CustomMessageBox.Show("informacion", "No se ha cancelado toda la factura");
            }
            else
            {
                if (!_printer.CheckFPrinter())
                {
                    bool PrinterResult = _printer.OpenFpCtrl(ConfigPosStatic.ImpresoraConfig.port);

                    if (!PrinterResult)
                    {
                        btnPrint.IsEnabled = true;
                        PrinterStatus status = _printer.GetPrinterStatus();
                        CustomMessageBox.Show($"Error en la Impresora Fiscal : {status.PrinterErrorCode}", status.PrinterStatusDescription + "\n" + status.PrinterErrorDescription);
                        return;
                    }
                }
                btnPrint.IsEnabled = false;
                try
                {
                   
                    S1PrinterData dataS1 = _printer.GetS1PrinterData();
                    numInvoice = dataS1.LastInvoiceNumber.ToString("00000000");
                    numInvoice = (int.Parse(numInvoice) + 1).ToString("00000000");
                    serialFicalPrinter = dataS1.RegisteredMachineNumber;


                    //validar que la imrpesora pertenezca a la caja
                    if (dataS1.RegisteredMachineNumber == _cajaData.SerialImpresora)
                    {
                        //llenar data en el modelo
                        Invoice InvoiceModels = FillDataClientModel(numInvoice, serialFicalPrinter);

                        if (InvoiceModels != null)
                        {
                            ConfirmInvoice confirInvoice = new ConfirmInvoice(InvoiceModels, InvoiceModels.listaPagos);
                            confirInvoice.ShowDialog();
                            if (confirInvoice.GetResponse())
                            {
                                confirInvoice.Close();

                                if (_Invoice.Id == -1)
                                {
                                    CheckIn(InvoiceModels , numInvoice);
                                }
                                else
                                {
                                    Response request = new InvoiceService().GetInvoiceById(_Invoice.Id);
                                    if (request.success == 1)
                                    {
                                        InvoiceResponse invoiceRequest = JsonConvert.DeserializeObject<InvoiceResponse>(request.data.ToString());

                                        //imprimir y actualizar
                                        if(invoiceRequest.detalleFactura.Count > 0)
                                            PrintUpdateInvoice(InvoiceModels, _Client, _Cajera, _Invoice.Id);
                                    }
                                    else
                                    {
                                        CheckIn(InvoiceModels, numInvoice);
                                    }
                                }
                            }
                            else
                            {
                                btnPrint.IsEnabled = true;
                                confirInvoice.Close();
                            }

                        }
                        else
                        {
                            btnPrint.IsEnabled = true;
                            CustomMessageBox.Show("Error al cargar la data de la factura", "Comuniquese con soporte");
                        }
                    }
                    else
                    {
                        btnPrint.IsEnabled = true;
                        CustomMessageBox.Show("Advertencia", "La impresora asignada no es la de esta caja.\n Comuniquese con sistemas");
                    }
                    
                }
                catch (Exception ex)
                {
                    Response data = JsonConvert.DeserializeObject<Response>(ex.Message);
                    log.Add(data.message + "\n" +ex.Message + "\n"+ex.StackTrace.ToString());
                    CustomMessageBox.Show("Error al realizar la facturacion", data.message +"\n"+ex.Message);
                }

            }

        }

        private void BtnDeletePaymentMethod_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            PaymentMethod payment = btn.DataContext as PaymentMethod;

            if (payment.Id != 3)
            {
                MessageBoxResult messageBoxResult = CustomMessageBox.Show("Confirmar", "¿Desea quitar el metodo de pago ?", MessageBoxButton.YesNo);

                switch (messageBoxResult)
                {
                    case MessageBoxResult.Yes:
                        _paymentslist.Remove(payment);
                        _paymentslist.RemoveAll(pay => pay.Id == 4);
                        _paymentslist.RemoveAll(pay => pay.Id == 5);
                        CalculateCancelled();
                        CachearInvoice(_Invoice, _paymentslist, _cajaData, _Client);
                        LoadListView();
                        break;
                    default:
                        break;
                }
            }
            else
            {
                MessageBoxResult confirm = CustomMessageBox.Show("Confirmar", "Si quitara este método de pago, anote toda la información que necesita\n Desea continuar?", MessageBoxButton.YesNo);

                if (confirm == MessageBoxResult.Yes)
                {
                    MessageBoxResult messageBoxResult = CustomMessageBox.Show("Confirmar", "¿Desea quitar el metodo de pago ?", MessageBoxButton.YesNo);

                    switch (messageBoxResult)
                    {
                        case MessageBoxResult.Yes:
                            _paymentslist.Remove(payment);
                            _paymentslist.RemoveAll(pay => pay.Id == 4);
                            _paymentslist.RemoveAll(pay => pay.Id == 5);
                            CalculateCancelled();
                            CachearInvoice(_Invoice, _paymentslist, _cajaData, _Client);
                            LoadListView();
                            break;
                        default:
                            break;
                    }
                }
            }

        }


        #endregion

        #region Metodos de Calculos


        private void CalculatePayWallet()
        {
            if (_Invoice.Cancelled < _Invoice.Total)
            {
                if (!(_paymentslist.Exists(pay => pay.Id == 4 && pay.Monto > 0)))
                {
                    //cuando el restante es menor que el saldo del cliente
                    if (_Client.Saldo > (_Invoice.RestanteRef(_cajaData.Tasa)))
                    {
                        _paymentslist.Add(new PaymentMethod
                        {
                            Id = 4,
                            CuentaBancariaId = _cajaData.BancoId,
                            Lote = "",
                            Monto = _Client.Saldo - (_Client.Saldo - (_Invoice.RestanteRef(_cajaData.Tasa))),
                            NumeroTransaccion = "",
                            CodigoMoneda = "002"
                        });
                    }
                    //cuando el restante es mayor al saldo del cliente
                    else
                    {
                        _paymentslist.Add(new PaymentMethod
                        {
                            Id = 4,
                            CuentaBancariaId = _cajaData.BancoId,
                            Lote = "",
                            Monto = _Client.Saldo,
                            NumeroTransaccion = "",
                            CodigoMoneda = "002"
                        });
                    }
                }
            }
            LoadListView();
            CalculateCancelled();
        }

        private void CanPayWallet()
        {

            if (_Client.Saldo > 0)
            {
                if (_Client.Saldo <= 5)
                {
                    CalculatePayWallet();
                }
                else
                {
                    double cant = _Invoice.TotalRef(_cajaData.Tasa);

                    if (cant > 5)
                    {

                        CustomMessageBox.Show("Esimado Usuario Requiere de permiso  de su supervisor para agregar el saldo wallet del cliente: " + _Client.Name + " " + _Client.LastName + Environment.NewLine +
                            "supera el limite maximo permitido a su nivel de usuario ( 5 ref.)");

                        bool result = ValidateAction(40);

                        if (result)
                        {
                            CalculatePayWallet();
                        }
                    }
                    else
                    {
                        CalculatePayWallet();
                    }

                }



            }
        }

        public void CalculateCancelled()
        {
            double pagado = 0;

            try
            {
                foreach (var item in _paymentslist)
                {
                    if (item.CodigoMoneda == "001")
                    {
                        pagado += item.Monto;
                    }
                    else
                    {
                        pagado += item.Monto * _cajaData.Tasa;
                    }
                }
               
                _Invoice.Cancelled = pagado;

                if (_Invoice.CancelledRef(_cajaData.Tasa) - _Invoice.TotalRef(_cajaData.Tasa) == 0)
                {
                    _Invoice.Cancelled = _Invoice.Total;
                }

                if (_Invoice.Cancelled <= _Invoice.Total)
                {
                    btnVueltoRef.IsEnabled = false;
                    btnVueltoRef.Visibility = Visibility.Hidden;
                    btnWallet.IsEnabled = false;
                    btnWallet.Visibility = Visibility.Hidden;

                    btnPrint.IsEnabled = false;
                }
                if (_Invoice.Cancelled >= _Invoice.Total)
                {
                    // PrintFactura();
                    //Mostrar Boton de imprimir factura
                    btnPrint.IsEnabled = true;
                }
                if (_Invoice.Retorno >= 0)
                {
                    if (_paymentslist.Exists(pay => pay.Id == 2 || pay.Id == 12) && _Invoice.Retorno > 0)
                    {
                        btnWallet.IsEnabled = true;
                        btnVueltoRef.IsEnabled = true;
                        btnWallet.Visibility = Visibility.Visible;
                        btnVueltoRef.Visibility = Visibility.Visible;
                    }
                }

                UpdateLbPayments();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error al calcular el monto cancelado", ex.Message);

            }
        }
        
        public bool CalculateAmounts()
        {
            double pagado = 0;

            try
            {
                if (_paymentslist.Count > 0)
                {
                    foreach (var pay in _paymentslist)
                    {

                        if (pay.CodigoMoneda == "001")
                        {
                            pagado += pay.Monto;
                        }
                        else
                        {
                            pagado += pay.Monto * _cajaData.Tasa;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error al calcular el total pagado", ex.Message);

            }

            return pagado < _Invoice.Total;
        }

        #endregion

        #region Metodos de la Impresora

        private bool ReconectPrinter()
        {
            if (_printer.CheckFPrinter())
            {
                return true;
            }
            else
            {
                PrinterStatus status = _printer.GetPrinterStatus();
                CustomMessageBox.Show($"Error en la Impresora Fiscal : {status.PrinterErrorCode}", status.PrinterStatusDescription + "\n" + status.PrinterErrorDescription);
                //Iniciar la impresora fiscal
                _printer.CloseFpCtrl();
                bool resPrintConnect = _printer.OpenFpCtrl(ConfigPosStatic.ImpresoraConfig.port);

                if (resPrintConnect)
                {
                    CustomMessageBox.Show("Información", "Puerto de la impresora Abierto y lista para imprimir");
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        #endregion

        #region Metodos Para actualizar los modelos
        //las otras variables usadas son globales
        public Invoice FillDataClientModel(string numInvoice, string serialFicalPrinter)
        {
            try
            {
                Invoice invoice;

                invoice = new Invoice()
                {
                    ClienteId = _Client.Id,
                    NumeroControl = _cajaData.NControl,
                    CajaId = _cajaData.Id,
                    MontoBruto = Math.Round(_Invoice.SubTotal, 2),
                    MontoNeto = Math.Round(_Invoice.Total, 2),
                    //por definir
                    MontoIVA = 0,
                    MontoDescuento = Math.Round(_Invoice.SubTotal * _Invoice.Discount, 2),
                    
                    CajeraId = _Cajera.Id,
                    //por definir
                    CodigoTurno = _Cajera.idTurno.ToString(),
                    tasa = _cajaData.Tasa,
                    TiendaId = _cajaData.TiendaId,
                    NumeroFactura = numInvoice,
                    SerialFiscal = serialFicalPrinter
                };

                invoice.listaProductos = new List<Product>();
                invoice.listaPagos = new List<PaymentMethod>();


                //foreach (VmProduct item in _Invoice.productList)
                //{
                //    Product productAdd = new Product();
                //    productAdd.id = item.Id;
                //    productAdd.CodigoProducto = item.Code;
                //    productAdd.Cantidad = item.Quantity;
                //    if (_Invoice.Discount > 0)
                //    {
                //        productAdd.Descuento = _Invoice.Discount;
                //    }
                //    else
                //    {
                //        productAdd.Descuento = item.DiscountPercentage;
                //    }
                //    productAdd.PrecioDolar = item.UnitPrice;
                //    productAdd.PrecioBolivar = Math.Round(item.UnitPriceBs,2);
                //    productAdd.IVA = item.Iva;
                //    productAdd.Serial = item.Serial;
                //    productAdd.Pesado = item.Pesado;
                //    productAdd.Descripcion = item.Descripcion;
                //    productAdd.Total = (_Invoice.Discount > 0) ? (item.TotalBs - (item.TotalBs * _Invoice.Discount)) : item.TotalBs;
                //    productAdd.CodigoTipo = (item.CodigoTipo == TipoProducto.combo) ? 2 : 1;
                //    _InvoiceModels.listaProductos.Add(productAdd);
                //}

                _Invoice.ProductList.ForEach(
                    p => invoice.listaProductos.Add(
                        new Product()
                        {
                            id = p.Id,
                            CodigoProducto = p.Code,
                            Cantidad = p.Quantity,
                            Descuento = (_Invoice.Discount > 0 ) ? _Invoice.Discount : p.DiscountPercentage,
                            PrecioDolar = p.UnitPrice,
                            PrecioBolivar = Math.Round(p.UnitPriceBs, 2),
                            IVA = p.Iva,
                            Serial = p.Serial,
                            Pesado = p.Pesado,
                            Descripcion = p.Descripcion,
                            Total = p.TotalBs,
                            CodigoTipo = (p.CodigoTipo == TipoProducto.combo) ? 2 : 1   
                        }
                    ));

                _paymentslist.ForEach(p =>{
                    invoice.listaPagos.Add(
                        new PaymentMethod()
                        {
                            Id = p.Id,
                            CuentaBancariaId = p.CuentaBancariaId,
                            Monto = p.Monto,
                            CodigoMoneda = p.CodigoMoneda,
                            vpos = p.vpos,
                            tipoTarjeta = p.tipoTarjeta,
                            NumeroTransaccion = (!string.IsNullOrEmpty(p.NumeroTransaccion)) ? p.NumeroTransaccion : "",
                            Lote = (!string.IsNullOrEmpty(p.Lote)) ? p.Lote : "",
                            numSeq = (!string.IsNullOrEmpty(p.numSeq)) ? p.numSeq : "",
                            Nombre = (!string.IsNullOrEmpty(p.Nombre)) ? p.Nombre : "",
                            nroAutorizacion = (!string.IsNullOrEmpty(p.nroAutorizacion)) ? p.nroAutorizacion : ""
                        });
                });

              

                //Se agrega el vuelto en Efectivo Bs
                if (_Invoice.Total < _Invoice.Cancelled)
                {
                    invoice.listaPagos.Add(
                        new PaymentMethod
                        {
                            Id = 8,
                            NumeroTransaccion = "",
                            Lote = "",
                            Monto = (_Invoice.Total - _Invoice.Cancelled),
                            CodigoMoneda = "001"
                        });
                }

                //calcular monto pagado
                double init = 0;
                invoice.MontoPagado = invoice.listaPagos.Aggregate(init, (accum, current) => {

                    if (current.CodigoMoneda == "002")
                    {
                        accum += (current.Monto * _cajaData.Tasa);
                    }
                    else
                    {
                        accum += current.Monto;
                    }
                    return accum;
                });
                
                
                return invoice;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al llenar los datos de la factura" +"\n"+ ex.Message + "\n"+ex.StackTrace.ToString());
            }

        }

        /// <summary>
        ///  Metodo para cargar el wallet
        /// </summary>
        /// <param name="pPaymentslist"></param>
        /// <param name="pInvoice"></param>
        private void SetWallet(List<PaymentMethod> pPaymentslist, VmInvoice pInvoice)
        {

            Response res = _serviceSecurityWallet.ClientPassword(_Client.Id);

            if (res.success == 0)
            {
                CustomMessageBox.Show("Informacion", $"{res.message}");
                CreateAuthWallet();

                res = _serviceSecurityWallet.ClientPassword(_Client.Id);

                if (Convert.ToInt32(res.data) == 1)
                {
                    vueltosWallet vuelto = new vueltosWallet(pPaymentslist, pInvoice, _cajaData.Tasa);
                    vuelto.ShowDialog();
                    LoadListView();
                    CalculateCancelled();
                }


            }
            else
            {
                vueltosWallet vuelto = new vueltosWallet(pPaymentslist, pInvoice, _cajaData.Tasa);
                vuelto.ShowDialog();
                LoadListView();
                CalculateCancelled();
            }
        }

        public void UpdateInvoice(int idFactura)
        {
            try
            {
                Response request = new Response();
                request = _serviceInvoice.UpdateInvoice(idFactura);

                if (request.success == 1 && request.data != null)
                {
                    //actualizar tasa
                    _cajaData.Tasa = Convert.ToDouble(request.data);
                }
                else
                {
                    CustomMessageBox.Show("informacion", request.message);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "\n" + ex.StackTrace.ToString());
            }
          
        }

        #endregion


        #region Metodos para imprimir

        public void PrintCopyClient(Invoice pinvoice)
        {
            if (_paymentslist.Exists(p => p.Id == 9))
            {
                //Verificar si esta lista para imprimir
                if (_printer.CheckFPrinter())
                {
                    //Imprimir copia

                    string clientName = _Client.Name + " " + _Client.LastName;
                    string clientDoc = _Client.Rif;
                    string clientAddress = _Client.Address;
                    string cajeraName = _Cajera.Nombre;
                    string codCaja = _cajaData.CodigoCaja;

                    bool printCopyInvoiceCredit = _printVoucher.PrintInvoiceCredit(pinvoice, clientName, clientDoc , clientAddress , cajeraName , codCaja);

                    if (!printCopyInvoiceCredit)
                    {
                        CustomMessageBox.Show("Error al imprimir copia de factura a credito", "Problemas al imprimir la copia de la factura a credito");
                    }
                }
                else
                {
                    PrinterStatus status = _printer.GetPrinterStatus();
                    CustomMessageBox.Show($"Error en la Impresora Fiscal : {status.PrinterErrorCode}", status.PrinterStatusDescription + "\n" + status.PrinterErrorDescription);
                }
            }
        }

        #endregion


        #region // Eventos Botones Formas de Pago


        private void BtnCashBs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool flag = false;
                //si flag es true, falta dinero por pagar
                flag = CalculateAmounts();
                if (flag)
                {
                    CalculateCancelled();

                    TextBox textbox = new TextBox
                    {
                        Text = ""
                    };
                    bool? keypad = ShowKeypadNum(textbox, this);

                    if (keypad == true)
                    {
                        if (textbox.Text != "")
                        {
                            // MessageBox.Show(textbox.Text);
                            double pay = Convert.ToDouble(textbox.Text);

                            if ((_Invoice.Cancelled + pay) > (_Invoice.Total + (100 * _cajaData.Tasa)))
                            {
                                CustomMessageBox.Show("informacion", "por favor, verifique el monto ingresado\n" + pay);
                            }
                            else
                            {
                                if (_paymentslist.Exists(pm => pm.Id == 1))
                                {
                                    PaymentMethod payment = _paymentslist.Where(pm => pm.Id == 1).FirstOrDefault();
                                    payment.Monto += pay;
                                    CalculateCancelled();

                                    new Thread(SendDataCustomerT).Start();

                                }
                                else
                                {
                                    _paymentslist.Add(new PaymentMethod { Id = 1, Lote = "", Monto = pay, NumeroTransaccion = "", CodigoMoneda = "001" });
                                    CalculateCancelled();
                                    new Thread(SendDataCustomerT).Start();
                                }
                                LoadListView();
                                CachearInvoice(_Invoice, _paymentslist, _cajaData, _Client);

                            }



                        }

                        else
                        {
                            MessageBox.Show("Estimado Usuario debe ingresar una cantidad");
                        }
                    }
                }
                else
                {
                    // txtbCash.Text = "00";
                    CustomMessageBox.Show("Informacion", "No puede procesar mas metodos de pago, ya cancelo el total de la factura");
                }



            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error al ingresar forma de pago Efectivo Bs.", ex.Message);

            }


        }

        private void SendDataCustomerT()
        {
            SendDataCustomer(_Invoice);
        }

        //Evento click del Mercahnte
        private void BtnMerchant_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool flag;
                flag = CalculateAmounts();

                if (flag)
                {
                    CalculateCancelled();

                    ApplyEffect(this, 2);
                    PaymentWithCard methodWindow = new PaymentWithCard(_paymentslist, _Invoice.Restante, _cajaData.Tasa, _printer, _Client.Rif.Substring(1));
                    methodWindow.Activate();
                    methodWindow.ShowDialog();
                    CalculateCancelled();
                    RemoveEffect(this);
                    LoadListView();
                    CachearInvoice(_Invoice, _paymentslist, _cajaData, _Client);
                    //hilo para cargar informacion en el customer
                    new Thread(SendDataCustomerT).Start();
                }
                else
                {
                    // txtbCashUsd.Text = "00";
                    CustomMessageBox.Show("Informacion", "No puede procesar mas metodos de pago, ya cancelo el total de la factura");
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error al ingresar forma de pago Merchant", ex.Message);
            }
        }

        private void BtnMerchantManual_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                bool flag;

                flag = CalculateAmounts();

                if (flag)
                {
                    CalculateCancelled();

                    ApplyEffect(this, 2);
                    PayMethodWindow methodWindow = new PayMethodWindow(_paymentslist, 3, _Invoice, _cajaData.Tasa, _cajaData.BancoId);
                    methodWindow.ShowDialog();
                    CalculateCancelled();
                    RemoveEffect(this);
                    LoadListView();
                    CachearInvoice(_Invoice, _paymentslist, _cajaData, _Client);


                    //hilo para cargar informacion en el customer
                    new Thread(SendDataCustomerT).Start();
                }
                else
                {
                    // txtbCashUsd.Text = "00";
                    CustomMessageBox.Show("Informacion", "No puede procesar mas metodos de pago, ya cancelo el total de la factura");
                }


            }
            catch (Exception ex)
            {
                log.Add(ex.Message + "\n" + ex.StackTrace.ToString());
                CustomMessageBox.Show("Error al ingresar forma de pago Merchant", ex.Message);
            }
        }

        private void BtnCashUsd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool flag;
                flag = CalculateAmounts();
                if (flag)
                {
                    CalculateCancelled();

                    PayDivisa payCash = new PayDivisa(_paymentslist , _Invoice , _cajaData.Tasa);
                    payCash.Activate();
                    payCash.ShowDialog();
                    

                    CalculateCancelled();
                    //hilo para cargar informacion en el customer
                    new Thread(SendDataCustomerT).Start();
                    CachearInvoice(_Invoice, _paymentslist, _cajaData, _Client);
                    LoadListView();
                }
                else
                {
                    // txtbCashUsd.Text = "00";
                    CustomMessageBox.Show("Informacion", "No puede procesar mas metodos de pago, ya cancelo el total de la factura");
                }
            }

            catch (Exception ex)
            {
                CustomMessageBox.Show("Error al ingresar forma de pago Efectivo $", ex.Message);
            }

        }

        public void CreateAuthWallet()
        {
            try
            {
                PasswordWallet app = new PasswordWallet(_Client.Id);
                app.ShowDialog();
                bool auth = app.GetAuth();
                if (auth)
                {
                    CustomMessageBox.Show("Informacion" , "Contraseña creada exitosamente...");
                }
                app.Close();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error  creando contraseña del wallet" , ex.Message);
            }
        }

        private bool AuthWallet()
        {
            PasswordWallet app = new PasswordWallet(_Client.Id, 1);
            try
            {
                app.ShowDialog();
                bool auth = app.GetAuth();
                app.Close();
                return auth;
            }
            catch (Exception ex)
            {
                app.Close();
                throw new Exception("Error autenticando wallet \n" + ex.Message);
            }
        }

        private bool SecurityWallet(int IdClient)
        {
            bool auth = false;
            try
            {
                Response res = _serviceSecurityWallet.ClientPassword(IdClient);

                if (res.success == 0)
                {
                    CreateAuthWallet();
                    res = _serviceSecurityWallet.ClientPassword(IdClient);
                    if (Convert.ToInt32(res.data) == 0)
                    {
                        return auth;
                    }
                }

                auth = AuthWallet();

                return auth;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private void BtnpayWallet_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (!(_Client.Saldo > 0))
                {
                    CustomMessageBox.Show("Informacion", "El cliente no tiene saldo en el wallet");
                    return;
                }

                bool flag;
                flag = CalculateAmounts();
                if (flag)
                {
                    //seguridad del wallet
                    bool auth = SecurityWallet(_Client.Id);
                    //bool auth = ValidateAction(40);
                    if (auth)
                    {
                        CalculateCancelled();
                        TextBox textbox = new TextBox
                        {
                            Text = ""
                        };
                        bool? keypad = ShowKeypad(textbox, this);
                        if (keypad == true)
                        {

                            double pay = Convert.ToDouble(textbox.Text);
                            if (pay > _Client.Saldo)
                            {
                                CustomMessageBox.Show("informacion", "No puede ingresar una cantidad mayor al saldo del cliente.\n Saldo del cliente :" + " " + _Client.Saldo, MessageBoxButton.OK);
                            }
                            else
                            {
                                if (pay > _Invoice.RestanteRef(_cajaData.Tasa))
                                {
                                    CustomMessageBox.Show("informacion", "El restante por pagar es: Ref " + (_Invoice.RestanteRef(_cajaData.Tasa)).ToString(), MessageBoxButton.OK);
                                }
                                else
                                {

                                    if ((_Invoice.CancelledRef(_cajaData.Tasa) + pay) > (_Invoice.TotalRef(_cajaData.Tasa) + 100))
                                    {
                                        CustomMessageBox.Show("informacion", "por favor, verifique el monto ingresado\n" + pay);
                                    }
                                    else
                                    {
                                        if (_paymentslist.Exists(pm => pm.Id == 4 && pm.Monto > 0))
                                        {
                                            PaymentMethod payment = _paymentslist.Where(pm => pm.Id == 4 && pm.Monto > 0).FirstOrDefault();
                                            payment.Monto = pay;
                                        }
                                        else
                                        {
                                            _paymentslist.Add(new PaymentMethod { Id = 4, Lote = "", Monto = pay, NumeroTransaccion = "", CodigoMoneda = "002" });
                                        }
                                        CalculateCancelled();
                                        LoadListView();
                                        CachearInvoice(_Invoice, _paymentslist, _cajaData, _Client);
                                    }
                                }
                            }

                        }
                    }
                    else
                    {
                        //CustomMessageBox.Show("Credenciales incorrectas" , "No puede usar el wallet");
                    }
                }
                else
                {
                    // txtbCashUsd.Text = "00";
                    CustomMessageBox.Show("Informacion", "No puede procesar mas metodos de pago, ya cancelo el total de la factura");
                }
            }

            catch (Exception ex)
            {
                CustomMessageBox.Show("Error al ingresar forma de pago Wallet", ex.Message);
            }

        }

        private void BtnExterno_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool flag;

                flag = CalculateAmounts();

                if (flag)
                {
                    CalculateCancelled();
                    ApplyEffect(this, 2);
                    PayMethodWindow methodWindow = new PayMethodWindow(_paymentslist, 7, _Invoice, _cajaData.Tasa, _cajaData.BancoId);
                    methodWindow.ShowDialog();
                    CalculateCancelled();
                    RemoveEffect(this);
                    LoadListView();
                    CachearInvoice(_Invoice, _paymentslist, _cajaData, _Client);
                    //hilo para cargar informacion en el customer
                    new Thread(SendDataCustomerT).Start();
                }
                else
                {
                    // txtbCashUsd.Text = "00";
                    CustomMessageBox.Show("Informacion", "No puede procesar mas metodos de pago, ya cancelo el total de la factura");
                }


            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error al ingresar forma de pago punto externo", ex.Message);

            }
        }

        private void BtnZelle_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                bool flag;

                flag = CalculateAmounts();

                if (flag)
                {
                    CalculateCancelled();

                    ApplyEffect(this, 2);
                    PayMethodWindow methodWindow = new PayMethodWindow(_paymentslist, 6, _Invoice, _cajaData.Tasa, _cajaData.BancoId);
                    methodWindow.ShowDialog();
                    RemoveEffect(this);
                    CalculateCancelled();
                    LoadListView();
                    CachearInvoice(_Invoice, _paymentslist, _cajaData, _Client);
                    //hilo para cargar informacion en el customer
                    new Thread(SendDataCustomerT).Start();
                }
                else
                {
                    // txtbCashUsd.Text = "00";
                    CustomMessageBox.Show("Informacion", "No puede procesar mas metodos de pago, ya cancelo el total de la factura");
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error al ingresar forma de pago Zelle", ex.Message);

            }
        }

        private void BtnCupon_Click(object sender, RoutedEventArgs e)
        {

        }


        private void BtnCreditPay_Click(object sender, RoutedEventArgs e)
        {
            bool flag;

            flag = CalculateAmounts();

            if (flag)
            {
                    bool result = ValidateAction(37);
                    if (result)
                    {
                        CalculateCancelled();

                        ApplyEffect(this, 2);
                        PayMethodWindow methodWindow = new PayMethodWindow(_paymentslist, 9, _Invoice, _cajaData.Tasa, _cajaData.BancoId);
                        methodWindow.ShowDialog();
                        CalculateCancelled();
                        RemoveEffect(this);
                        LoadListView();
                        CachearInvoice(_Invoice, _paymentslist, _cajaData, _Client);
                    }
            }
            else
            {
                CustomMessageBox.Show("Información", "No puede procesar mas metodos de pago, ya cancelo el total de la factura");

            }

        }


        #endregion


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

        #endregion


        #region Effectos de Pantalla
        private void ApplyEffect(Window pwindow, int pEffect)
        {
            var objBlur = new System.Windows.Media.Effects.BlurEffect
            {
                Radius = pEffect
            };
            pwindow.Effect = objBlur;
            //App.Current.MainWindow.Effect = objBlur;
        }
        private void RemoveEffect(Window pwindow)
        {
            pwindow.Effect = null;

            // App.Current.MainWindow.Effect = null;
        }
        #endregion


        #region Metodos de Seguridad para realizar facturas a credito

        private bool ValidateAction(int pidAction)
        {
            try
            {
                bool flag = false;
                Response result = _securityService.ValidarAccciones(_Cajera.Id, pidAction.ToString());

                if (result.success == 1)
                {
                    if (result.data != null)
                    {
                        List<UserDataResponse.AccionPermitida> accion = JsonConvert.DeserializeObject<List<UserDataResponse.AccionPermitida>>(result.data.ToString());
                        if (accion.Exists(action => action.idAccion == pidAction && action.permitido))
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    return RequestCredentials(pidAction);
                }

                return flag;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "\n" + "Error al Validar la accion del usuario" + "\n" + ex.Message);
            }
        }


        #endregion


        //enviar data customer
        public void SendDataCustomer(VmInvoice invoice)
        {
            NumberFormatInfo formato = new CultureInfo("en-US", false).NumberFormat;
            formato.NumberDecimalSeparator = ",";
            formato.NumberGroupSeparator = ".";
            ProductCollection dataCustomer = new ProductCollection
            {
                Pagado = new Pagado(),
                Tasa = new ViewModels.customerDisplay.Tasa(),
                Cliente = new Cliente()
            };

            dataCustomer.Tasa.Valor = string.Format(formato, "{0:N2}", _cajaData.Tasa);
            dataCustomer.Cliente.name = _Client.Name;
            dataCustomer.Cliente.apellido = _Client.LastName;

            dataCustomer.Pagado.TotalBs = invoice.Total.ToString(formato);
            dataCustomer.Pagado.TotalRef = invoice.TotalRef(_cajaData.Tasa).ToString();

            dataCustomer.Pagado.RecibidoBs = invoice.Cancelled.ToString(formato);
            dataCustomer.Pagado.RecibidoRef = invoice.CancelledRef(_cajaData.Tasa).ToString();

            dataCustomer.Pagado.RestanteBs = invoice.Restante.ToString(formato);
            dataCustomer.Pagado.RestanteRef = invoice.RestanteRef(_cajaData.Tasa).ToString();

            //0  es no totalizado
            dataCustomer.Totalizar = 1;

            _CustomerD.sendData(JsonConvert.SerializeObject(dataCustomer));
        }


        private void CleanPayment()
        {
            //  Total = _Invoice.Total;
            // Restante = Total;

            //  TxtRefRestante.Text = "ref." + (TruncateDecimal((Total / TasaDolar), 2)).ToString();
            TxtCancelado.Text = 0.ToString("C", _FormatVenezuela);
            TxtRefCancelado.Text = "ref." + 0.ToString();

        }

        private void BtnLimpiarInvoice_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult response = CustomMessageBox.Show("Limpiar Campos", "Deseas Limpiar los datos del pago de la factura" + " " + _cajaData.NControl + " " + "?", MessageBoxButton.YesNoCancel);


            switch (response.ToString().ToLower())
            {
                case "yes":
                    //Eliminar Datos de la factura

                    CleanPayment();

                    break;
            }

        }

        

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string path = @"C:\Users\soporte\Desktop\voucher\voucher\vouchers\01220.txt";
            MessageBox.Show((File.Exists(path) == true ? "yes" : "No"));
            string[] lineas = File.ReadAllText(path).Split('\n');
            if (File.Exists(path))
            {

                _printVoucher.PrintVoucher(lineas);
            }
            else
            {
                MessageBox.Show("Informacion", "No existe el voucher en el equipo");
            }
        }
       
        private void BtnCalculadora_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process[] calc;
                calc = Process.GetProcessesByName("calculator");
                if (calc.Length > 0)
                {
                    calc[0].Kill();
                    Process program = new Process();
                    program.StartInfo.FileName = "calc.exe";
                    program.Start();
                }
                else
                {
                    Process calculator = new Process();
                    calculator.StartInfo.FileName = "calc.exe";
                    calculator.Start();
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error al abrir la calculadora", ex.Message);
            }
        }

     
        private bool RequestCredentials(int pidAction)
        {
            try
            {
                bool flag = false;

                CustomMessageBox.Show("Información", "Se necesitan más privilegios para realizar esta acción" + "\nDebre ingresar creedenciales");
                LoginValidation validation = new LoginValidation(pidAction.ToString());
                validation.ShowDialog();
                if (validation.userActions != null)
                {
                    if (validation.userActions.AccionPermitida != null)
                    {
                        if (validation.userActions.AccionPermitida.Exists(action => action.idAccion == pidAction && action.permitido))
                        {
                            flag = true;
                        }
                    }
                }

                if (!flag)
                    CustomMessageBox.Show("Usuario sin permiso", "No tiene privilegios para realizar esta operación");

                return flag;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


    }
}
