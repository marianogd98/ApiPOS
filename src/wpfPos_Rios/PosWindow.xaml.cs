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
using TfhkaNet.IF.VE;
using OposScanner_CCO;
using OposScale_CCO;
using wpfPos_Rios.service;
using wpfPos_Rios.service.Security;
using Peripherals.helper;
using wpfPos_Rios.Views.helper;
using System.Text.RegularExpressions;
using wpfPos_Rios.ViewModels;
using Newtonsoft.Json;
using wpfPos_Rios.ViewModels.helperResponse;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Globalization;
using System.Threading;
using wpfPos_Rios.ViewModels.customerDisplay;
using wpfPos_Rios.Views;
using TfhkaNet.IF;
using Models.Vpos;
using System.IO;
using Peripherals.printerHKA80;
using System.Xml;
using KeyPad;

namespace wpfPos_Rios
{
    /// <summary>
    /// Lógica de interacción para PosWindow.xaml
    /// </summary>
    public partial class PosWindow : Window
    {

        // delagado para leer codigos de barra
        private delegate void SetCodeBartDeleg(string text);
        //balanza y scanner magellan 
        OPOSScannerClass _Scanner;
        OPOSScaleClass _Scale;

        //servicios 
        VposService _VposService;
        SecurityService _securityService;
        InvoiceService _InvoiceService;
        CajaService _CajaService;
        PaymentMethodServices _PaymentMethodService;
        ProductService _productService;

        //mantener factura en memoria
        CacheJsonInvoice cacheInvoice;
        readonly PrintVouchers _printVoucher;
        readonly Tfhka _printer;
        Caja _infoCaja;
        readonly UserCajera _cajera;

        //modelos
        VmInvoice _Invoice;
        VmClient _client;
        List<PaymentMethod> _paymentlist;

        //hora 
        DispatcherTimer _clock;

        //list datagrid
        //ObservableCollection<VmProduct> listProductDG;
        //global para saber que producto seleccionar en el datagrid
        VmProduct productsInvoice = null;

        //para modificar las monedas y separadores numericos
        NumberFormatInfo _FormatVen;

        //customer display
        SendCustomerDisplay _CustomerD = new SendCustomerDisplay();

        public PosWindow(Tfhka printer, Caja infoCaja, UserCajera cajera)
        {
            InitializeComponent();
            _printer = printer;
            _infoCaja = infoCaja;
            _cajera = cajera;

            _Invoice = new VmInvoice
            {
                ProductList = new List<VmProduct>()
            };

            _client = new VmClient();
            _paymentlist = new List<PaymentMethod>();
            _printVoucher = new PrintVouchers(printer, ConfigPosStatic.ImpresoraConfig.port, ConfigPosStatic.ImpresoraConfig.flat);

            dgListProduct.ItemsSource = _Invoice.ProductList;

            Init();
            FinishInvoice();
        }
        #region inicializadores 
        public void Init()
        {
            InitView();
            InitInvoice();
            InitServices();
            InitMagellan();
            _CustomerD = new SendCustomerDisplay();
            cacheInvoice = new CacheJsonInvoice();
        }
        public void InitServices()
        {
            _VposService = new VposService();
            _securityService = new SecurityService();
            _InvoiceService = new InvoiceService();
            _CajaService = new CajaService();
            _PaymentMethodService = new PaymentMethodServices();
            _productService = new ProductService();
        }

        public void RecoverInvoiceToJson(VmInvoice invoice, List<PaymentMethod> paymentlist, Caja cajaData, VmClient _clientData)
        {
            _client = _clientData;
            _Invoice = invoice;
            _paymentlist = paymentlist;
            _infoCaja = cajaData;

            lblClientName.Content = _client.Name + " " + _client.LastName;
            if (_client.Saldo > 0)
            {
                lblClientBalance.Content = "Wallet Ref:" + " " + _client.Saldo;
            }
            else
            {
                lblClientBalance.Content = "Wallet Ref:" + " " + "0,00";
            }
            lbTasa.Content = _infoCaja.Tasa.ToString("C", _FormatVen);

            LoadLabelsBSRef();
            //RefresList(_Invoice.productList);
            RefresList();
            UpdateDataCustomerT();

        }

        public void InitView()
        {
            _FormatVen = new CultureInfo("es-VE").NumberFormat;
            _FormatVen.NumberGroupSeparator = ".";
            _FormatVen.NumberDecimalSeparator = ",";

            txtUserName.Text = _cajera.Nombre;
            lbTasa.Content = _infoCaja.Tasa.ToString("C", _FormatVen);

            _clock = new DispatcherTimer();
            _clock.Tick += new EventHandler(ClockTimer_Tick);
            _clock.Interval = new TimeSpan(0, 1, 0);
            _clock.Start();

            lbDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
            lbClock.Text = DateTime.Now.ToString("hh:mm");

            RefresList();
            //listProductDG = new ObservableCollection<VmProduct>();
            //dgListProduct.ItemsSource = listProductDG;
            //dgListProduct.ItemsSource = _Invoice.productList;
        }

        public void InitInvoice()
        {
            txtNumFactura.Text = _infoCaja.NControl;
            LoadLabelsBSRef();
        }

        public void LoadLabelsBSRef()
        {
            //subTotal Descuento Impuesto
            lbSubtotal.Text = (_Invoice.SubTotal).ToString("N", _FormatVen);
            lbDescuento.Text = _Invoice.Discount.ToString("P");
            lbImpuesto.Text = _Invoice.Tax.ToString("N", _FormatVen);

            //Total
            lbTotal.Text = (_Invoice.Total).ToString("N", _FormatVen);
            lbRefTotal.Text = "ref. " + _Invoice.TotalRef(_infoCaja.Tasa).ToString();

            //Cancelado
            lbCancelado.Text = (_Invoice.Cancelled).ToString("N", _FormatVen);
            lbRefCancelado.Text = "ref. " + _Invoice.CancelledRef(_infoCaja.Tasa).ToString();

            //Restante
            lbRestante.Text = (_Invoice.Restante).ToString("N", _FormatVen);
            lbRefRestante.Text = "ref. " + _Invoice.RestanteRef(_infoCaja.Tasa).ToString();
        }

        public void InitMagellan()
        {
            try
            {
                InitScale();
                InitScanner();
            }
            catch (Exception)
            {
                //CustomMessageBox.Show("Error", ex.Message, CustomMessageBox.MessageBoxType.Information);
            }
        }

        private void ClockTimer_Tick(object sender, EventArgs e)
        {
            lbClock.Text = DateTime.Now.ToString("hh:mm");
        }

        public void InitScale()
        {
            if (_infoCaja.AreaId == 1)
            {
                try
                {
                    //inicializar scanner y balanza
                    _Scale = new OPOSScaleClass();
                    ResultCodeH.Check(_Scale.Open("RS232Scale"));
                }
                catch (Exception ex)
                {
                    throw new Exception($"Problemas al conectar con la balanza {_Scale.ResultCode} :\n {ex.Message}.");
                }
            }
        }
        public void InitScanner()
        {
            if (_infoCaja.AreaId == 1)
            {
                try
                {
                    const string interfaceScanner = "RS232Scanner";

                    _Scanner = new OPOSScannerClass();
                    //_Scanner.ErrorEvent += new _IOPOSScannerEvents_ErrorEventEventHandler(s_ErrorEvent);
                    _Scanner.DataEvent += new _IOPOSScannerEvents_DataEventEventHandler(DataEvent);

                    ResultCodeH.Check(_Scanner.Open(interfaceScanner));
                    ResultCodeH.Check(_Scanner.ClaimDevice(500));

                    //MessageBox.Show(this.s.DeviceName.ToString());

                    _Scanner.DeviceEnabled = true;
                    ResultCodeH.Check(_Scanner.ResultCode);

                    _Scanner.AutoDisable = true;
                    ResultCodeH.Check(_Scanner.ResultCode);

                    _Scanner.DataEventEnabled = true;
                    ResultCodeH.Check(_Scanner.ResultCode);
                }
                catch (Exception ex)
                {
                    //throw new Exception($"Problemas al conectar con el scanner {_Scanner.ResultCode} :\n {ex.Message}.");
                }
            }
        }
        #endregion inicializadores

        void DataEvent(int Status)
        {
            string data = _Scanner.ScanData;
            _Scanner.DeviceEnabled = true;
            _Scanner.DataEventEnabled = true;
            this.BeginInvoke(new SetCodeBartDeleg(Si_DataReceived), new object[] { data });
        }
        private void BeginInvoke(SetCodeBartDeleg setTextDeleg, object[] value)
        {
            this.Dispatcher.Invoke(() =>
            {
                setTextDeleg((string)value[0]);
            });
        }

        private void Si_DataReceived(string data)
        {
            if (txtCodigoProd.Focus())
            {
                data = data.Trim();
                string code = Regex.Replace(data, @"[^0-9]", "");
                txtCodigoProd.Text = code;
                SearchProducts(code);
            }
        }

        #region botones pos

        private void BtnPayment_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (!Application.Current.Windows.OfType<PaymentWindow>().Any())
                {
                    if (_Invoice.ProductList.Count > 0)
                    {
                        if ((_client.Id == 0) || (_client == null) || (txtUserName.Text == ""))
                        {
                            CustomMessageBox.Show("Información", "Estimado usuario primero debe ingresar los datos del cliente antes de realizar el pago de la factura.", MessageBoxButton.OK);

                            SelectClient();
                            if (!((_client.Id == 0) || (_client == null) || (txtUserName.Text == "")))
                            {
                                PaymentInvoice();
                            }
                        }
                        else
                        {
                            if (!_client.Rif.Contains(_cajera.Cedula))
                            {
                                PaymentInvoice();
                            }
                            else
                            {
                                CustomMessageBox.Show("Información", "Estimado usuario no puede facturar mientras trabaja.", MessageBoxButton.OK);
                            }
                        }
                    }
                    else
                    {
                        CustomMessageBox.Show("Información", "No tiene productos cargados en la factura.", MessageBoxButton.OK);
                    }
                }
                else
                {
                    //CustomMessageBox.Show("Información", "Ya esta abierta una ventana de cobros", MessageBoxButton.OK);
                    Application.Current.Windows.OfType<PaymentWindow>().FirstOrDefault().Close();
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error pago de factura", ex.Message);
            }
        }

        private void BtnProductData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ConsultarProductos app = new ConsultarProductos(_infoCaja.Tasa , _Scanner);
                //closeScanner();
                app.ShowDialog();
                InitScanner();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error consultar productos", ex.Message);
            }
        }

        private void PaymentInvoice()
        {
            try
            {
                var messageBoxResult = CustomMessageBox.Show("Confirmar", "¿Esta seguro que desea facturar estos productos?", MessageBoxButton.YesNoCancel, CustomMessageBox.MessageBoxImage.Question);

                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    PaymentWindow pay = new PaymentWindow(_Invoice, _client, _paymentlist, _printer, _infoCaja, _cajera);
                    pay.ShowDialog();
                    this.Show();

                    if (_Invoice.Id == -1 && _client.Id == 0 && _paymentlist.Count == 0)
                    {
                        //factura terminada
                        ResetInvoice();
                    }
                    else
                    {
                        //factura no terminada
                        InitInvoice();
                        //RefresList(_Invoice.productList);
                        RefresList();
                    }

                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private void BtnSaveInvoice_Click(object sender, RoutedEventArgs e)
        {
            if (_paymentlist.Exists(pay => pay.Id == 1 || pay.Id == 2 || pay.Id == 4 || pay.Id == 5 || pay.Id == 8))
            {
                CustomMessageBox.Show("Informacion: factura con pagos procesados", "Las facturas solo se pueden guardar con los siguientes metodos de pago:\n Punto Interno\n Zelle\n Punto Externo", MessageBoxButton.OK);
                return;
            }

            if (_client == null || _client.Rif == null)
            {
                CustomMessageBox.Show("Informacion", "No puede poner en espera una factura sin cliente.", MessageBoxButton.OK);
                return;
            }

            if (!(_client.Name.Length > 2))
            {
                CustomMessageBox.Show("Informacion", "No puede poner en espera una factura sin cliente.", MessageBoxButton.OK);
                return;
            }

            if (_Invoice.ProductList.Count < 1)
            {
                CustomMessageBox.Show("Informacion", "No puede poner en espera una factura sin productos.", MessageBoxButton.OK);
                return;
            }

            bool result = ValidateAction(18);

            if (result)
            {
                SaveInvoice();
            }
        }

        private void SaveInvoice()
        {
            //clienteid cajaid cajeraid tiendaid productos 
            Invoice invoice = new Invoice
            {
                CajaId = _infoCaja.Id,
                ClienteId = _client.Id,
                CajeraId = _cajera.Id,
                TiendaId = _infoCaja.TiendaId,
                listaProductos = new List<Product>(),
                listaPagos = new List<PaymentMethod>()
            };

            foreach (var product in _Invoice.ProductList)
            {
                invoice.listaProductos.Add(new Product { id = product.Id, Cantidad = product.Quantity });
            }

            _paymentlist.ForEach(pay => invoice.listaPagos.Add(new PaymentMethod
            {
                Id = pay.Id,
                Lote = pay.Lote,
                CuentaBancariaId = pay.CuentaBancariaId,
                Monto = pay.Monto,
                NumeroTransaccion = pay.NumeroTransaccion,
                CodigoMoneda = pay.CodigoMoneda,
                vpos = pay.vpos,
                tipoTarjeta = pay.tipoTarjeta,
                Nombre = pay.Nombre,
                nroAutorizacion = pay.nroAutorizacion,
                numSeq = pay.numSeq
            }));

            Response response = _InvoiceService.SaveInvoice(invoice);

            if (Convert.ToInt32(response.data) == 1)
            {
                CustomMessageBox.Show("Informacion", response.message, MessageBoxButton.OK);
                ResetInvoice();
            }
            else
            {
                CustomMessageBox.Show("Informacion", response.message, MessageBoxButton.OK);
            }
        }

        private void BtnGetInvoice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_paymentlist.Count > 0)
                {
                    CustomMessageBox.Show("Informacion: factura con pagos procesados", "No puede buscar facturas, termine de realizar la facturacion.", MessageBoxButton.OK);
                    return;
                }

                if (_Invoice.ProductList.Count > 0)
                {
                    CustomMessageBox.Show("Informacion", "Mientras este en proceso de facturacion, no puede buscar acturas en espera.", MessageBoxButton.OK);
                    return;
                }
                else
                {

                    bool result = ValidateAction(19);

                    if (result)
                    {
                        GetInvoice();
                    }

                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error recuperar factura", ex.Message);
            }
        }

        public void GetInvoice()
        {
            try
            {
                //reiniciar data de factura y cliente
                _Invoice.ResetInvoice();
                _client.resetData();
                //abrir ventana
                InvoiceListPending app = new InvoiceListPending(_infoCaja.TiendaId, _infoCaja.Id, _Invoice, _client, _infoCaja, _paymentlist);
                app.ShowDialog();

                CalculateCancelled(_paymentlist);
                //RefresList(_Invoice.productList);
                RefresList();
                LoadLabelsBSRef();

                lblClientName.Content = _client.Name + " " + _client.LastName;
                lblClientBalance.Content = _client.Saldo.ToString();

                CachearInvoice(_Invoice, _paymentlist, _infoCaja, _client);

                if (_infoCaja.AreaId != 2)
                {
                    UpdateDataCustomer(_Invoice);
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error recuperando factura", ex.Message, MessageBoxButton.OK);
            }
        }

        private void BtnCleanInvoice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_paymentlist.Count > 0)
                {
                    CustomMessageBox.Show("Informacion: factura con pagos procesados", "No puede cancelar la factura, termine de realizar la facturacion.", MessageBoxButton.OK);
                    return;
                }


                bool result = ValidateAction(17);

                if (result)
                {
                    MessageBoxResult resultado = CustomMessageBox.Show("informacion", "¿Desea anular la factura ?", MessageBoxButton.YesNo);

                    switch (resultado)
                    {
                        case MessageBoxResult.Yes:
                            ResetInvoice();
                            break;
                    }
                }

            

            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error limpiando factura", ex.Message, MessageBoxButton.OK);
            }
        }

        private void BtnReturnInvouice_Click(object sender, RoutedEventArgs e)
        {
            if (_paymentlist.Count > 0)
            {
                CustomMessageBox.Show("Informacion: factura con pagos procesados", "No puede realizar una devolucion, termine de realizar la facturacion.", MessageBoxButton.OK);
                return;
            }
            if (_Invoice.ProductList.Count > 0)
            {
                CustomMessageBox.Show("Información:", "Estimado usuario debe finalizar la facturacion pendiente antes de realizar una devolución.");
            }
            try
            {

                bool result = ValidateAction(20);

                if (result)
                {
                    DevolverFactura invoiceReturn = new DevolverFactura(_infoCaja, _cajera.Id, _printer, _cajera);
                    ApplyEffect(this);
                    invoiceReturn.ShowDialog();
                    RemoveEffect(this);
                }

            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error devolucion", ex.Message);
            }
        }

        private void BtnReprintInvoice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_Invoice.ProductList.Count == 0)
                {

                    bool result = ValidateAction(16);

                    if (result)
                    {

                        RePrintInvoiceWindow printInvoiceWindow = new RePrintInvoiceWindow(_infoCaja, _printer);
                        ApplyEffect(this);
                        printInvoiceWindow.ShowDialog();
                        RemoveEffect(this);
                    }
                }
                else
                {
                    CustomMessageBox.Show("Información:", "Estimado usuario debe finalizar toda venta pendiente antes de reimprimir una factura.");
                }

            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error reimprimir factura", ex.Message);
            }
        }

        private void BtnDiscountInvoice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_paymentlist.Count > 0)
                {
                    CustomMessageBox.Show("Informacion: factura con pagos procesados", "No puede realizar descuento a la factura, termine de realizar la facturacion.", MessageBoxButton.OK);
                    return;
                }

                if (_Invoice.ProductList.Count > 0)
                {

                    bool result = ValidateAction(16);

                    if (result)
                    {
                        DiscountInvoice discountWindow = new DiscountInvoice(_Invoice);

                        discountWindow.ShowDialog();
                        discountWindow.Activate();
                        CachearInvoice(_Invoice, _paymentlist, _infoCaja, _client);
                        //RefresList(_Invoice.productList);
                        RefresList();
                        LoadLabelsBSRef();
                    }
                }
                else
                {
                    CustomMessageBox.Show("Información", "Debe ingresar al menos un producto para realizar un descuento a la factura.");
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error descuento factura", ex.Message);
            }
        }

        private void BtnOpenGavela_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool result = _printer.SendCmd("0");

                if (!result)
                {
                    PrinterStatus status = _printer.GetPrinterStatus();
                    CustomMessageBox.Show($"Error en la Impresora Fiscal : {status.PrinterErrorCode}", status.PrinterStatusDescription + "\n" + status.PrinterErrorDescription);
                }
            }


            catch (Exception ex)
            {
                CustomMessageBox.Show("Error abrir gabeta", ex.Message);
            }
        }

        private void BtnChangeClient_Click(object sender, RoutedEventArgs e)
        {
            SelectClient();
        }

        private void BtnChangePasswordWallet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((_client.Id == 0) || (_client == null) || (txtUserName.Text == ""))
                {
                    CustomMessageBox.Show("Informacion", "Debe ingresar un cliente para poder realizar esta operacion");
                }
                else
                {
                    bool result = ValidateAction(41);
                    if (result)
                    {
                        ApplyEffect(this);
                        CreateAuthWallet();
                        RemoveEffect(this);
                    }
                    else
                    {
                        //CustomMessageBox.Show("Sin credenciales", "No cuenta con permisos para realizar esta operacion");
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error", ex.Message);
            }
        }

        private void BtnKeyboardFast_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Product product = new Product();
                //aplica desenfoque
                ApplyEffect(this);
                QuickMenuProduct quickMenuProduct = new QuickMenuProduct(product, _infoCaja.Tasa, _infoCaja.AreaId);
                quickMenuProduct.ShowDialog();
                //quita desenfoque

                if (product != null && (product.id != -1 && product.id > -1))
                {

                    if (product.id > -1 && product.Descripcion != string.Empty)
                    {
                        AddProductToInvoice(product);
                    }
                }

                RemoveEffect(this);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error teclado rapido", ex.Message);
                RemoveEffect(this);
            }
        }

        private void BtnSearchProduct_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Cambiar accion por la de Modificar producto pesado(Actualmente esta la de buscar producto)
                bool result = ValidateAction(25);
                if (result)
                {
                    Product product = new Product();
                    SearchProducts _SearchProductWindow = new SearchProducts(product, _infoCaja.Tasa);
                    ApplyEffect(this);
                    _SearchProductWindow.ShowDialog();
                    RemoveEffect(this);

                    if (product.CodigoProducto != null)
                    {
                        if (product.CodigoProducto != "" && product.id > 0)
                        {
                            AddProductToInvoice(product);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error buscar productos", ex.Message);
            }
        }

        private void BtnPrintVoucher_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ReportsHelperWindow reportsHelper = new ReportsHelperWindow();
                ApplyEffect(this);
                reportsHelper.ShowDialog();
                RemoveEffect(this);
                string fecha = DateTime.Now.ToString("yyyy-MM-dd");

                int id = reportsHelper.id;
                switch (id)
                {
                    case 1:
                        //Imprimir el ultimo Voucher
                        try
                        {
                            VposResponse requestLastVoucher = _VposService.GetLastVoucher();
                            if (requestLastVoucher.codRespuesta == "00")
                            {
                                string path = requestLastVoucher.nombreVoucher;
                                bool printVoucher = ImprimirVoucher(path);

                                if (!printVoucher)
                                {
                                    CustomMessageBox.Show("error", "No se imprimio el ultimo voucher del punto");
                                }
                            }
                            else
                            {
                                CustomMessageBox.Show("Error de Respuesta con el VPos");
                            }

                        }
                        catch (Exception ex)
                        {
                            CustomMessageBox.Show("Error al imprimir voucher del punto interno", ex.Message);
                        }
                        break;

                    case 2:

                        //Cerrar turno e imprimir reporte por cierre de turno

                        Response response = _PaymentMethodService.GetPaymentReportsCloseTurn(fecha, _infoCaja.Id, _cajera.idTurno);

                        if (response.success == 1)
                        {
                            List<ReportDataPayments> reportResponse = JsonConvert.DeserializeObject<List<ReportDataPayments>>(response.data.ToString());

                            if (reportResponse != null)
                            {
                                if (reportResponse.Count > 1)
                                {
                                    if (_printer.CheckFPrinter())
                                    {
                                        PrintReportsPayment(reportResponse, "Cierre de Turno");
                                    }
                                    else
                                    {
                                        PrinterStatus status = _printer.GetPrinterStatus();
                                        CustomMessageBox.Show($"Error en la Impresora Fiscal : {status.PrinterErrorCode}", status.PrinterStatusDescription + "\n" + status.PrinterErrorDescription);
                                    }

                                }
                                else
                                {
                                    CustomMessageBox.Show("Información", "Estimado Usuario no tiene formas de pago en este turno reportadas, no se imprimira el reporte", MessageBoxButton.OK);
                                }
                            }
                            else
                            {
                                CustomMessageBox.Show("Error de conexión con el Api B", response.message);
                            }
                        }
                        else
                        {
                            CustomMessageBox.Show("error", response.message);
                        }


                        break;

                    case 3:
                        //  Imprimir Reporte de resumen de pagos cierre de caja
                        Response response1 = _PaymentMethodService.GetPaymentReportsClosePos(fecha, _infoCaja.Id);
                        List<ReportDataPayments> reportResponse1 = JsonConvert.DeserializeObject<List<ReportDataPayments>>(response1.data.ToString());

                        if (reportResponse1 != null)
                        {
                            if (reportResponse1.Count > 1)
                            {
                                if (_printer.CheckFPrinter())
                                {
                                    PrintReportsPayment(reportResponse1, "Cierre de Caja");
                                }
                                else
                                {
                                    PrinterStatus status = _printer.GetPrinterStatus();
                                    CustomMessageBox.Show($"Error en la Impresora Fiscal : {status.PrinterErrorCode}", status.PrinterStatusDescription + "\n" + status.PrinterErrorDescription);
                                }

                            }
                            else
                            {
                                CustomMessageBox.Show("Información", "Estimado Usuario no tiene formas de pago en  reportadas en esta caja, no se imprimira el reporte", MessageBoxButton.OK);
                            }


                        }
                        else
                        {
                            MessageBox.Show("Error de conexión con el Api B", response1.message);
                        }

                        break;

                    case 4:

                        //Imprimir el ultimo Voucher procesado
                        try
                        {
                            VposResponse requestLastVoucher = _VposService.GetLastVoucherProcess();
                            if (requestLastVoucher.codRespuesta == "00")
                            {
                                string path = requestLastVoucher.nombreVoucher;
                                bool printVoucher = ImprimirVoucher(path);

                                if (!printVoucher)
                                {
                                    CustomMessageBox.Show("error", "No se imprimio el ultimo voucher del punto");
                                }
                            }
                            else
                            {
                                CustomMessageBox.Show("Error de Respuesta con el VPos");
                            }

                        }
                        catch (Exception ex)
                        {
                            CustomMessageBox.Show("Error al imprimir voucher del punto interno", ex.Message);
                            return;
                        }

                        break;
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error al imrpimir voucher", ex.Message);
            }
        }

        private void BtnPrintPreClose_Click(object sender, RoutedEventArgs e)
        {
            //Imprimir pre cierre
            try
            {
                VposResponse requestLastVoucher = _VposService.PreClosePoint();
                if (requestLastVoucher.codRespuesta == "00")
                {
                    string path = requestLastVoucher.nombreVoucher;
                    bool printPreclose = ImprimirVoucher(path);

                    if (!printPreclose)
                    {
                        CustomMessageBox.Show("error", "No se imprimio el pre cierre del punto");
                    }
                }
                else
                {
                    CustomMessageBox.Show("Error de Respuesta con el VPos");
                }

            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error precierre", ex.Message);
            }
        }

        private void BtnCheckPeripherals_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool result = ValidateAction(38);
                PrinterStatus statusPrinter;
                if (result)
                {
                    //Verificar el estado de le impresora
                    if (_printer.CheckFPrinter())
                    {
                        statusPrinter = _printer.GetPrinterStatus();
                        if (statusPrinter.PrinterStatusDescription == "Ok")
                        {
                            CustomMessageBox.Show("Información", statusPrinter.PrinterStatusDescription, MessageBoxButton.OK);
                        }
                        else
                        {
                            CustomMessageBox.Show("Información del estado de la impresora", " Estado de la Impresora: " + statusPrinter.PrinterStatusDescription);
                        }
                    }
                    else
                    {
                        statusPrinter = _printer.GetPrinterStatus();

                        //Iniciar la impresora fiscal
                        _printer.CloseFpCtrl();
                        bool PrinterResult = _printer.OpenFpCtrl(ConfigPosStatic.ImpresoraConfig.port);
                        if (PrinterResult)
                        {
                            CustomMessageBox.Show("Información", "Impresora en linea");
                        }
                        else
                        {
                            CustomMessageBox.Show("Error con la impresora", "Error con la impresora fiscal, por favor pongase en contacto con soporte Tecnico.");
                            MessageBoxResult response = CustomMessageBox.Show("Informacion", "Desea continuar con  la aplicación de este modo?", MessageBoxButton.YesNoCancel);
                            switch (response.ToString().ToLower())
                            {
                                case "no":
                                    this.Close();
                                    Environment.Exit(0);
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error verificar perifericos", ex.Message);
            }
        }

        private void BtnPrintX_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_paymentlist.Count > 0)
                {
                    CustomMessageBox.Show("Informacion: factura con pagos procesados", "termine de realizar la facturacion para imprimir el x.", MessageBoxButton.OK);
                    return;
                }
                if (_Invoice.ProductList.Count > 0)
                {
                    CustomMessageBox.Show("Información:", "Estimado usuario debe finalizar toda venta pendiente antes de imprimir la X.");
                }

                //Verificar si esta lista para imprimir
                if (_printer.CheckFPrinter())
                {
                    //imprimir
                    bool result = ValidateAction(30);

                    if (result)
                    {
                        ApplyEffect(this);
                        var messageBoxResult = CustomMessageBox.Show("Confirmar", "¿Esta seguro que desea imprimir la X de esta maquina " + txtUserName.Text + " ?", MessageBoxButton.YesNoCancel, CustomMessageBox.MessageBoxImage.Question);
                        RemoveEffect(this);

                        if (messageBoxResult == MessageBoxResult.Yes)
                        {
                            try
                            {
                                _printer.PrintXReport();
                            }
                            catch (Exception ex)
                            {
                                throw new Exception(ex.Message);
                            }
                        }

                    }
                }
                else
                {
                    PrinterStatus status = _printer.GetPrinterStatus();
                    CustomMessageBox.Show($"Error en la Impresora Fiscal : {status.PrinterErrorCode}", status.PrinterStatusDescription + "\n" + status.PrinterErrorDescription);
                }

            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error imprimir reporte x", ex.Message);
            }
        }

        private void BtnSuspendCaja_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_paymentlist.Count > 0)
                {
                    CustomMessageBox.Show("Informacion: factura con pagos procesados", "No puede suspender la caja, termine de realizar la facturacion.", MessageBoxButton.OK);
                    return;
                }

                Response response = new Response();

                if (_Invoice.ProductList.Count == 0)
                {
                    var messageBoxResult = CustomMessageBox.Show("Confirmar", "¿Esta seguro que desea suspender su turno " + txtUserName.Text + " ?", MessageBoxButton.YesNoCancel, CustomMessageBox.MessageBoxImage.Question);

                    if (messageBoxResult == MessageBoxResult.Yes)
                    {

                        //cerrar turno aqui
                        response = _CajaService.CloseTurn(_cajera.idTurno);

                        CloseApp();
                    }
                }
                else
                {
                    CustomMessageBox.Show("Información:", "Estimado usuario por favor finalice la venta pendiente para poder suspender esta caja.");
                }

            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error supender caja", ex.Message);
            }
        }

        private void BtnCloseTurn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Response response = new Response();

                if (_Invoice.ProductList.Count == 0)
                {
                    ApplyEffect(this);
                    var messageBoxResult = CustomMessageBox.Show("Confirmar", "¿Esta seguro que desea cerrar su turno " + txtUserName.Text + " ?", MessageBoxButton.YesNoCancel, CustomMessageBox.MessageBoxImage.Question);
                    RemoveEffect(this);
                    if (messageBoxResult == MessageBoxResult.Yes)
                    {
                        //Verificar si esta lista para imprimir
                        if (_printer.CheckFPrinter())
                        {
                            //cerrar turno aqui
                            PrintReportPayment();
                            CustomMessageBox.Show("Imprimir reporte x", "Presione ok para imprimir");
                            try
                            {
                                _printer.PrintXReport();
                                response = _CajaService.CloseTurn(_cajera.idTurno, 1);
                                //response.success = 1;
                                if (response.success != 1)
                                {
                                    CustomMessageBox.Show("Información, No se pudo cerrar el turno", response.message);
                                }
                                else
                                {
                                    Thread.Sleep(4000);
                                    CustomMessageBox.Show("Imprimir cierre de punto", "Presione ok para imprimir");
                                    //imrpimir reporte de cierre
                                    ClosePoint();
                                    CloseApp();
                                }
                            }
                            catch (Exception ex)
                            {
                                throw new Exception("no se pudo imprimir el reporte x\n" + ex.Message);
                            }
                        }
                        else
                        {
                            PrinterStatus status = _printer.GetPrinterStatus();
                            CustomMessageBox.Show($"Error en la Impresora Fiscal : {status.PrinterErrorCode}", status.PrinterStatusDescription + "\n" + status.PrinterErrorDescription);
                        }
                    }
                }
                else
                {
                    CustomMessageBox.Show("Información:", "Estimado usuario debe finalizar toda venta pendiente antes de cerrar su turno.");
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error cerrar turno", ex.Message);
            }
        }

        private void BtnClosePos_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_Invoice.ProductList.Count == 0)
                {

                    bool result = ValidateAction(23);

                    if (result)
                    {
                        ApplyEffect(this);
                        var messageBoxResult = CustomMessageBox.Show("Confirmar", "¿Esta seguro que desea imprimir la z de esta maquina " + txtUserName.Text + " ?", MessageBoxButton.YesNoCancel, CustomMessageBox.MessageBoxImage.Question);
                        RemoveEffect(this);

                        if (messageBoxResult == MessageBoxResult.Yes)
                        {


                            ReporteZeta reporte = new ReporteZeta();
                            var zetaReport = _printer.GetZReport();
                            var statusS1 = _printer.GetS1PrinterData();
                            reporte.IdCaja = _infoCaja.Id;
                            //guardar zeta 
                            reporte.Numero = (zetaReport.NumberOfLastZReport + 1).ToString("0000");
                            reporte.UltimaFactura = statusS1.LastInvoiceNumber.ToString("00000000");
                            reporte.Monto = statusS1.TotalDailySales;
                            reporte.CantidadFacturas = statusS1.QuantityOfInvoicesToday;
                            reporte.xml = XmlReportZS1(zetaReport, statusS1).OuterXml;

                            PrintReportPaymentCaja();
                            CustomMessageBox.Show("Imprimir reporte Z", "Presione ok para imprimir");

                            try
                            {
                                Thread.Sleep(2000);
                                _printer.PrintZReport();
                                Thread.Sleep(2000);

                                Response requestZeta = _CajaService.SaveZ(reporte);

                                //if (requestZeta.success == 1)
                                //{
                                //cerrar turno aqui
                                Response request = _CajaService.CloseTurn(_cajera.idTurno, 1);
                                if (request.success == 1)
                                {
                                    CloseApp();
                                }
                                else
                                {
                                    CustomMessageBox.Show("Informacion", request.message + " \nNo se puede cerrar la caja");
                                }
                                //}
                                //else
                                //{
                                //    CustomMessageBox.Show("Informacion", requestZeta.message + " \nNo se puede cerrar la caja");
                                //}

                            }
                            catch (Exception ex)
                            {
                                throw new Exception("Error al imprimir el reporte z\n" + ex.Message);
                            }
                        }
                    }
                }
                else
                {
                    CustomMessageBox.Show("Información:", "Estimado usuario debe finalizar toda venta pendiente antes de cerrar la caja.");
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error cerrar caja", ex.Message);
            }
        }

        private void BtnDonate_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnKeypad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                txtCodigoProd.Text = "";
                bool? keypad = ShowKeypad(txtCodigoProd, this);

                if (keypad == true)
                {
                    if (txtCodigoProd.Text != "")
                    {
                        SearchProducts(txtCodigoProd.Text);
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error teclado numerico", ex.Message);

            }
        }
        private void BtnEditDeleteProduct_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                Button btn = sender as Button;
                VmProduct product = btn.DataContext as VmProduct;
                bool tieneBolsa = _Invoice.ProductList.Exists(p => p.Code == "034981");

                if (_Invoice.ProductList.Count > 2)
                {
                    EditDeleteProduct(product, _infoCaja.Tasa, _cajera);
                }
                else
                {
                    if (_Invoice.ProductList.Count == 2 && !tieneBolsa)
                    {
                        EditDeleteProduct(product, _infoCaja.Tasa, _cajera);
                    }
                    else
                    {
                        //permisos de limpiar factura
                        EditDeleteProduct(product, _infoCaja.Tasa, _cajera, false);
                    }
                }               
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error editando linea de producto", ex.Message);
            }
        }

        private void EditDeleteProduct(VmProduct product , double tasa , UserCajera cajera , bool delete = true)
        {
            EditDeleteProduct editDeleteWindow = new EditDeleteProduct(product, _infoCaja.Tasa, _cajera , delete);
            editDeleteWindow.ShowDialog();

            //elimina producto de la lista si tiene cantidad 0
            _Invoice.ProductList.RemoveAll(p=> p.Quantity == 0);

            if (_Invoice.ProductList.Count == 0)
            {
                _Invoice.ProductList = new List<VmProduct>();
                _Invoice.Discount = 00;
            }
            CachearInvoice(_Invoice, _paymentlist, _infoCaja, _client);
            //RefresList(_Invoice.productList);
            RefresList();
            LoadLabelsBSRef();

            //recargar data customer
            if (_infoCaja.AreaId != 2)
            {
                UpdateDataCustomer(_Invoice);
            }
        }

        private void BtnDiscountProduct_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                VmProduct product = btn.DataContext as VmProduct;

                if (product != null)
                {
                    ApplyEffect(this);

                    bool result = ValidateAction(24);

                    if (result)
                    {
                        DiscountProduct windowsDiscountProduct = new DiscountProduct(product);

                        windowsDiscountProduct.ShowDialog();

                        if (_Invoice.ProductList.Exists(p => p.DiscountPercentage > 0))
                            _Invoice.Discount = 0;

                        CachearInvoice(_Invoice, _paymentlist, _infoCaja, _client);
                        dgListProduct.Columns[4].Visibility = Visibility.Visible;
                        //RefresList(_Invoice.productList);
                        RefresList();
                        UpdateDataCustomer(_Invoice);
                        LoadLabelsBSRef();

                    }

                    RemoveEffect(this);

                }
                else
                {
                    CustomMessageBox.Show("Información", "Debe seleccionar un producto.");
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error descuento producto", ex.Message);
            }
        }
        #endregion botones pos



        private void TxtCodigoProd_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchProducts(txtCodigoProd.Text);
            }
        }

        private void SearchProductWeight(string pCode)
        {
            Product productData;

            try
            {
                if (pCode.Length > 12 && pCode.Length <= 13)
                {
                    string codigo = pCode.Substring(2, 5);
                    string pesoEntero = pCode.Substring(7, 2);
                    string pesoDecimal = pCode.Substring(9, 3);

                    // Busqueda en el modelo de la base de datos
                    Response queryProduct = _productService.GetProductsbyCode(codigo);

                    if (queryProduct.success == 1)
                    {
                        productData = JsonConvert.DeserializeObject<Product>(queryProduct.data.ToString());

                        if (productData.id != -1)
                        {
                            if (productData.PrecioDetal != 0 && productData.PrecioDetal > 0)
                            {
                                productData.Cantidad = Convert.ToDouble(pesoEntero + "," + pesoDecimal);
                                AddProductToInvoice(productData);
                            }
                            else
                            {
                                CustomMessageBox.Show("Informacion", "Este producto no esta disponible para la venta", MessageBoxButton.OK, CustomMessageBox.MessageBoxImage.Information);
                            }
                        }
                        else
                        {
                            CustomMessageBox.Show("Informacion", "No existe el codigo de barra", MessageBoxButton.OK, CustomMessageBox.MessageBoxImage.Information);
                        }
                    }
                }
                else
                {
                    CustomMessageBox.Show("Informacion", "No existe el codigo de barra", MessageBoxButton.OK, CustomMessageBox.MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Advertencia", ex.Message, MessageBoxButton.OK);
            }
        }

        public void SearchProducts(string pCode)
        {

            Product productData;

            if (pCode != "")
            {
                try
                {
                    // Busqueda en el modelo de la base de datos
                    Response queryProduct = _productService.GetProductsbyCode(pCode);

                    if (queryProduct.success == 1)
                    {
                        productData = JsonConvert.DeserializeObject<Product>(queryProduct.data.ToString());
                        if (productData.id != -1)
                        {
                            if (productData.PrecioDetal != 0)
                            {
                                AddProductToInvoice(productData);
                            }
                            else
                            {
                                CustomMessageBox.Show("Informacion", "Este producto no esta disponible para la venta", MessageBoxButton.OK, CustomMessageBox.MessageBoxImage.Information);
                            }
                        }
                        else
                        {
                            SearchProductWeight(pCode);
                        }
                    }

                }
                catch (Exception Ex)
                {
                    CustomMessageBox.Show("Advertencia", Ex.Message, MessageBoxButton.OK);
                }
            }

            txtCodigoProd.Text = "";
        }


        public void AddProductHeavy(Product productData)
        {
            try
            {
                productsInvoice = new VmProduct(_infoCaja.Tasa);

                _Scale.ClaimDevice(1000);
                if (_Scale.Claimed)
                {
                    _Scale.DeviceEnabled = true;
                    ResultCodeH.Check(_Scale.ResultCode);
                    
                    productsInvoice.Id = productData.id;
                    productsInvoice.Descripcion = productData.Descripcion;
                    productsInvoice.Code = productData.CodigoProducto;
                    productsInvoice.CodigoTipo = (productData.CodigoTipo == 1) ? TipoProducto.producto : TipoProducto.combo;
                    productsInvoice.Pesado = productData.Pesado;
                    productsInvoice.DiscountPercentage = productData.Descuento;

                    if (productData.CodigoMoneda == "002")
                    {
                        productsInvoice.UnitPrice = productData.PrecioDetal;
                        productsInvoice.UnitPriceBs = Math.Round(productData.PrecioDetal * _infoCaja.Tasa, 2);
                    }
                    else
                    {
                        productsInvoice.UnitPriceBs = Math.Round(productData.PrecioDetal, 2);
                        productsInvoice.UnitPrice = Round(productData.PrecioDetal / _infoCaja.Tasa);
                    }
                    _Scale.ReadWeight(out int peso , 5000);
                    ResultCodeH.Check(_Scale.ResultCode);

                    productsInvoice.Quantity = (double)peso / 1000;

                    if (!(productsInvoice.Quantity > 0))
                    {
                        CustomMessageBox.Show("Informacion", "coloque el producto en la balanza", MessageBoxButton.OK, CustomMessageBox.MessageBoxImage.Information);
                    }
                    else
                    {
                        _Invoice.ProductList.Add(productsInvoice);
                    }
                }
                else
                {
                    CustomMessageBox.Show("informacion", "Balanza error" + " " + _Scale.ResultCode + ":" + ResultCodeH.Message(_Scale.ResultCode) + Environment.NewLine + "Imposible agregar un producto pesado a esta venta usando la balanza, comuniquese con soporte técnico", CustomMessageBox.MessageBoxType.Information);
                    return;
                }

            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("informacion", "Balanza error" + " " + _Scale.ResultCode + ":" + ResultCodeH.Message(_Scale.ResultCode) + ex.ToString(), CustomMessageBox.MessageBoxType.Information);
            }
        }
        public void AddProductHeavyCodeBard(Product productData)
        {
            try
            {
                productsInvoice = new VmProduct(_infoCaja.Tasa)
                {
                    Id = productData.id,
                    Code = productData.CodigoProducto,
                    DiscountPercentage = productData.Descuento,
                    Descripcion = productData.Descripcion,
                    Quantity = productData.Cantidad,
                    Pesado = productData.Pesado,
                    CodigoTipo = (productData.CodigoTipo == 1) ? TipoProducto.producto : TipoProducto.combo
                };
                //calcular precios
                if (productData.CodigoMoneda == "002")
                {
                    productsInvoice.UnitPrice = productData.PrecioDetal;
                    productsInvoice.UnitPriceBs = Math.Round(productData.PrecioDetal * _infoCaja.Tasa, 2);
                }
                else
                {
                    productsInvoice.UnitPriceBs = Math.Round(productData.PrecioDetal, 2);
                    productsInvoice.UnitPrice = Round(productData.PrecioDetal / _infoCaja.Tasa);
                }
                _Invoice.ProductList.Add(productsInvoice);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error al agregar producto", ex.Message, CustomMessageBox.MessageBoxType.Information);
            }
        }
        public void AddProduct(Product productData)
        {
            try
            {
                productsInvoice = new VmProduct(_infoCaja.Tasa);

                if (_Invoice.ProductList.Exists(p => p.Id == productData.id))
                {
                    productsInvoice = _Invoice.ProductList.Where(p => p.Id == productData.id).First();
                    productsInvoice.Quantity++;
                }
                else
                {
                    productsInvoice.Id = productData.id;
                    productsInvoice.Code = productData.CodigoProducto;
                    productsInvoice.CodigoTipo = (productData.CodigoTipo == 1) ? TipoProducto.producto : TipoProducto.combo;
                    productsInvoice.DiscountPercentage = productData.Descuento;
                    productsInvoice.Descripcion = productData.Descripcion;
                    productsInvoice.Quantity = 1;
                    productsInvoice.Pesado = productData.Pesado;
                    //calcular precios

                    if (productData.CodigoMoneda == "002")
                    {
                        productsInvoice.UnitPrice = productData.PrecioDetal;
                        productsInvoice.UnitPriceBs = Math.Round(productData.PrecioDetal * _infoCaja.Tasa, 2);
                    }
                    else
                    {
                        if (productData.CodigoProducto == "034981")
                        {
                            productsInvoice.UnitPriceBs = productData.PrecioDetal;
                            productsInvoice.UnitPrice = 0;
                        }
                        else
                        {
                            productsInvoice.UnitPriceBs = Math.Round(productData.PrecioDetal, 2);
                            productsInvoice.UnitPrice = Round(productData.PrecioDetal / _infoCaja.Tasa);
                        }
                    }
                    _Invoice.ProductList.Add(productsInvoice);
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error al agregar producto", ex.Message, CustomMessageBox.MessageBoxType.Information);
            }
        }

        public void AddProductToInvoice(Product productData)
        {
            productsInvoice = null;
            productsInvoice = new VmProduct(_infoCaja.Tasa);

            if (productData.Pesado)
            {
                if (productData.Cantidad > 0)
                {
                    AddProductHeavyCodeBard(productData);
                }
                else
                {
                    AddProductHeavy(productData);
                }
                //RefresList(_Invoice.productList);
                RefresList();
            }
            else
            {
                AddProduct(productData);
                //RefresList(_Invoice.productList);
                RefresList();
            }

            //guardar factura en json
            CachearInvoice(_Invoice, _paymentlist, _infoCaja, _client);

            txtCodigoProd.Text = "";
            //enviar data al customer            
            new Thread(SendDataCustomerT).Start();
            SelecItemproductList(productsInvoice);
            LoadLabelsBSRef();
        }

        #region customer Display
        //hilos para cargar data en el customer
        private void SendDataCustomerT()
        {
            SendDataCustomer(productsInvoice, _Invoice);
        }
        private void UpdateDataCustomerT()
        {
            UpdateDataCustomer(_Invoice);
        }

        public void SendDataCustomer(VmProduct product, VmInvoice invoice)
        {
            ProductCollection dataCustomer = new ProductCollection
            {
                Tasa = new ViewModels.customerDisplay.Tasa(),
                Cliente = new Cliente(),
                Totales = new Totales(),
                Productonuevo = new Productonuevo(),
                Productos = new Productos()
            };
            dataCustomer.Productos.Product = new List<VmProduct>();

            try
            {
                dataCustomer.Tasa.Valor = string.Format(_FormatVen, "{0:N2}", _infoCaja.Tasa);
                dataCustomer.Totales.Bs = string.Format(_FormatVen, "{0:N2}", _Invoice.Total);
                dataCustomer.Totales.Ref = string.Format(_FormatVen, "{0:N2}", _Invoice.TotalRef(_infoCaja.Tasa));
                dataCustomer.Cliente.name = _client.Name;
                dataCustomer.Cliente.apellido = _client.LastName;
                //0  es no totalizado
                dataCustomer.Totalizar = 0;

                dataCustomer.Productos.Product = invoice.ProductList;

                dataCustomer.Productonuevo =
                    new Productonuevo
                    {
                        Descripcion = product.Descripcion,
                        Cantidad = product.Quantity.ToString(),
                        Precio = product.UnitPriceBs.ToString(),
                        Precioref = product.UnitPrice.ToString(),
                        Subtotal = product.TotalBs.ToString(),
                        Ref = product.TotalRef.ToString()
                    };


                _CustomerD.sendData(JsonConvert.SerializeObject(dataCustomer));
            }
            catch (Exception )
            {

            }
        }
        public void UpdateDataCustomer(VmInvoice invoice)
        {
            ProductCollection dataCustomer = new ProductCollection
            {
                Tasa = new ViewModels.customerDisplay.Tasa(),
                Cliente = new Cliente(),
                Totales = new Totales(),
                Productos = new Productos()
            };
            dataCustomer.Productos.Product = new List<VmProduct>();

            try
            {
                dataCustomer.Cliente.name = _client.Name;
                dataCustomer.Cliente.apellido = _client.LastName;
                dataCustomer.Tasa.Valor = string.Format(_FormatVen, "{0:N2}", _infoCaja.Tasa);
                dataCustomer.Totales.Bs = string.Format(_FormatVen, "{0:N2}", _Invoice.Total);
                dataCustomer.Totales.Ref = string.Format(_FormatVen, "{0:N2}", _Invoice.TotalRef(_infoCaja.Tasa));

                //0  es no totalizado
                dataCustomer.Totalizar = 0;

                dataCustomer.Productos.Product = invoice.ProductList;

                _CustomerD.sendData(JsonConvert.SerializeObject(dataCustomer));
            }
            catch (Exception)
            {

            }
        }

        #endregion Customer Display


        #region utilidades

        public void FinishInvoice()
        {
            if (_infoCaja.IdFac != -1)
            {
                S1PrinterData dataS1 = _printer.GetS1PrinterData();

                string lastPrintedInvoice = (dataS1.LastInvoiceNumber + 1).ToString("00000000");
                if (lastPrintedInvoice == _infoCaja.NumeroFactura)
                {
                    try
                    {
                        Response request = _InvoiceService.GetInvoiceById(_infoCaja.IdFac);
                        InvoiceResponse invoice = JsonConvert.DeserializeObject<InvoiceResponse>(request.data.ToString());

                        Client clien = new Client();
                        Invoice invoiceRecover = new Invoice
                        {
                            id = invoice.factura.id,
                            NumeroFactura = invoice.factura.numeroFactura,
                            NumeroControl = invoice.factura.numeroControl,
                            MontoBruto = invoice.factura.montoBruto,
                            MontoDescuento = invoice.factura.montoDescuento,
                            MontoIVA = invoice.factura.montoIVA,
                            MontoNeto = invoice.factura.montoNeto,
                            MontoPagado = invoice.factura.montoPagado,
                            Fecha = invoice.factura.fecha,
                            CajaId = invoice.factura.cajaId,

                            listaProductos = new List<Product>()
                        };
                        invoice.detalleFactura.ForEach(
                            product =>
                            {
                                invoiceRecover.listaProductos.Add
                                (
                                    new Product()
                                    {
                                        Descripcion = product.descripcion,
                                        Cantidad = product.cantidad,
                                        PrecioBolivar = product.precioneto,
                                        Total = product.total
                                    });
                            });

                        clien.Nombre = invoice.cliente.nombre;
                        clien.Apellido = invoice.cliente.apellido;
                        clien.RIF = invoice.cliente.rif;
                        clien.Telefono = invoice.cliente.telefono;
                        clien.Direccion = invoice.cliente.direccion;

                        FinishInvoice invoiceFinish = new FinishInvoice(_printer, _infoCaja, invoiceRecover, clien, invoice.factura.cajera);
                        invoiceFinish.ShowDialog();
                        //eliminar json
                        cacheInvoice.DeleteArchive();
                        lbTasa.Content = _infoCaja.Tasa.ToString("C", _FormatVen);
                    }
                    catch (Exception ex)
                    {
                        Response info = JsonConvert.DeserializeObject<Response>(ex.Message);
                        CustomMessageBox.Show("Error", info.message);
                    }
                }
            }
        }

        public void SelectClient()
        {
            double tasa = _infoCaja.Tasa;
            try
            {
                ClientData _clientData = new ClientData(_client, _infoCaja);
                ApplyEffect(this);
                _clientData.ShowDialog();

                lblClientName.Content = (_client.Name != null && _client.Name != "") ? _client.Name + " " + _client.LastName : "";


                if (_Invoice.ProductList.Count > 0)
                {
                    _infoCaja.Tasa = tasa;
                }
                else
                {
                    lbTasa.Content = _infoCaja.Tasa.ToString("C", _FormatVen);
                }

                if (_client.Saldo > 0)
                {
                    lblClientBalance.Content = "Wallet Ref:" + " " + _client.Saldo;
                }
                else
                {
                    lblClientBalance.Content = "Wallet Ref:" + " " + "0,00";
                }
                RemoveEffect(this);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error", ex.Message);
            }
        }

        public void CreateAuthWallet()
        {
            PasswordWallet app = new PasswordWallet(_client.Id, 2);
            try
            {
                SeguridadWalletService _serviceSecurityWallet = new SeguridadWalletService();
                
                Response res = _serviceSecurityWallet.ClientPassword(_client.Id);

                if (Convert.ToInt32(res.data) == 1)
                {
                    app.ShowDialog();
                    bool auth = app.GetAuth();
                    if (auth)
                    {
                        CustomMessageBox.Show("Informacion", "Contraseña recuperada exitosamente...");
                    }
                    app.Close();
                }
                else
                {
                    CustomMessageBox.Show("Informacion", "El cliente no tiene contraseña creada");
                }
            }
            catch (Exception ex)
            {
                app.Close();
                CustomMessageBox.Show("Error recuperando contraseña", ex.Message);
            }
        }

        private bool ValidateAction(int pidAction)
        {
            try
            {
                bool flag = false;
                Response result = _securityService.ValidarAccciones(_cajera.Id, pidAction.ToString());

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
        //private void RefresList(List<VmProduct> list)
        //{
        //    listProductDG.Clear();
        //    list.ForEach(p => listProductDG.Add(p));
        //}
        private void RefresList()
        {
            try
            {
                dgListProduct.ItemsSource = _Invoice.ProductList;
                dgListProduct.Items.Refresh();
            }
            catch
            {
                dgListProduct.ItemsSource = _Invoice.ProductList;
                dgListProduct.Items.Refresh();
            }
        }

        private void CachearInvoice(VmInvoice invoice, List<PaymentMethod> paymentlist, Caja _cajaData, VmClient client)
        {
            try
            {
                cacheInvoice.WriteInvoice(invoice, paymentlist, _cajaData, client);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error guardado de factura", ex.Message);
            }
        }
        public void ResetInvoice()
        {
            try
            {
                RefresList();
                cacheInvoice.DeleteArchive();
                
                //recargar data customer    
                if (_infoCaja.AreaId != 2)
                {
                    UpdateDataCustomer(_Invoice);
                }
                
                
                CloseScale();
                CloseScanner();

                _clock.Stop();
                GetPosConfig(_infoCaja.CodigoCaja);

                PosWindow pos = new PosWindow(_printer, _infoCaja, _cajera);
                pos.Show();
                this.Close();
            }
            catch
            {
                return;
            }
        }

        private void GetPosConfig(string pCodigoCaja)
        {
            try
            {
                var data = new CajaService().GetConfigCaja(pCodigoCaja);

                if (data != null && data.success == 1)
                {
                    _infoCaja= JsonConvert.DeserializeObject<Caja>(data.data.ToString());
                    _infoCaja.CodigoCaja = ConfigPosStatic.CajaConfig.CodigoCaja;
                }
            }

            catch
            {
                return;
            }
        }

        private void PrintReportsPayment(List<ReportDataPayments> vmReports, string pTipo)
        {
            try
            {
                string fmt;

                List<string> listaDatos = new List<string>
                {
                    "",
                    "Sucursal Rio:" + _infoCaja.TiendaId + "  Caja: " + _infoCaja.CodigoCaja + " Cajero:" + _cajera.Nombre,
                    "Codigo de Area: " + _infoCaja.AreaId,
                    "Turno: " + _cajera.idTurno,

                    "Detalles del Resumen",
                    "",
                    ""
                };

                foreach (var item in vmReports)
                {

                    fmt = string.Format(_FormatVen, "{0:N2}", (item.Monto));

                    if (item.Lote != null)
                    {
                        listaDatos.Add("Lote:" + item.Lote);
                    }
                    else
                    {
                        listaDatos.Add("");
                    }
                    listaDatos.Add("Descripción:                             " + item.Descripcion);
                    listaDatos.Add("Monto:                                   " + fmt);
                    listaDatos.Add("----------------------------------------");
                }

                if (_printer.CheckFPrinter())
                {
                    bool print = _printVoucher.PrintReportsPayment(listaDatos, pTipo);
                    if (!print)
                    {
                        CustomMessageBox.Show("Error al imprimir el reporte de pagos", "Comuniquese con soporte, hubo problemas al imprimir el reporte resumen de pagos");
                    }
                }
                else
                {
                    PrinterStatus status = _printer.GetPrinterStatus();
                    CustomMessageBox.Show($"Error en la Impresora Fiscal : {status.PrinterErrorCode}", status.PrinterStatusDescription + "\n" + status.PrinterErrorDescription);
                }

            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error al imprimir resumen de pagos", ex.Message);
            }
        }

        private void PrintReportPaymentCaja()
        {

            try
            {
                if (!_printer.CheckFPrinter())
                {
                    bool PrinterResult = _printer.OpenFpCtrl(ConfigPosStatic.ImpresoraConfig.port);

                    if (!PrinterResult)
                    {
                        PrinterStatus status = _printer.GetPrinterStatus();
                        CustomMessageBox.Show($"Error al imprimir cierre de caja : {status.PrinterErrorCode}", status.PrinterStatusDescription + "\n" + status.PrinterErrorDescription);
                        return;
                    }
                }

                //Imprimir Reporte de resumen de pagos cierre de caja
                string fecha = DateTime.Now.ToString("yyyy-MM-dd");
                Response response = _PaymentMethodService.GetPaymentReportsClosePos(fecha, _infoCaja.Id);
                List<ReportDataPayments> reportResponse = JsonConvert.DeserializeObject<List<ReportDataPayments>>(response.data.ToString());

                if (reportResponse != null)
                {
                    if (reportResponse.Count > 1)
                    {
                        PrintReportsPayment(reportResponse, "Cierre de Caja");
                        Thread.Sleep(reportResponse.Count * 300);
                    }
                }

            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Información", ex.Message);
            }
        }
        
        private void PrintReportPayment()
        {
            try
            {
                string fecha = DateTime.Now.ToString("yyyy-MM-dd");
                Response responseReport = _PaymentMethodService.GetPaymentReportsCloseTurn(fecha, _infoCaja.Id, _cajera.idTurno);

                if (responseReport.success == 1)
                {
                    List<ReportDataPayments> reportResponse = JsonConvert.DeserializeObject<List<ReportDataPayments>>(responseReport.data.ToString());

                    if (reportResponse != null)
                    {
                        if (reportResponse.Count >= 1)
                        {
                            PrintReportsPayment(reportResponse, "Cierre de Turno");
                            Thread.Sleep(reportResponse.Count * 150);
                        }
                    }
                    else
                    {
                        CustomMessageBox.Show("Error de conexión con el servidor", responseReport.message);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Problemas al imprimir  reporte cierre de turno.\n {ex.Message}" );
            }
        }

        private void ClosePoint()
        {
            try
            {
                //cerrar punto interno
                VposResponse requestClosePoint = _VposService.ClosePoint();
                if (requestClosePoint.codRespuesta == "00")
                {
                    string path = requestClosePoint.nombreVoucher;
                    bool closePoint = ImprimirVoucher(path);

                    if (!closePoint)
                    {
                        CustomMessageBox.Show("Error al imprimir cierre de punto", "No se pudo imprimir el cierre del punto de venta, comuniquese con soporte.");
                    }
                }
                else
                {
                    CustomMessageBox.Show("Error al imprimir cierre de punto", requestClosePoint.mensajeRespuesta);
                }
            }
            catch (Exception ex)
            {
                if (ex.Message == "Referencia a objeto no establecida como instancia de un objeto.")
                {
                    CustomMessageBox.Show("VPOS Error", "No se pudo conectar con la vpos.\n Comuniquese con soporte tecnico.");
                }
                else
                {
                    CustomMessageBox.Show("VPOS Error", ex.Message);
                }
            }

        }

        private bool ImprimirVoucher(string path)
        {
            bool flag = false;
            string[] voucher;
            try
            {
                if (File.Exists(path))
                {
                    string contenido = File.ReadAllText(path);
                    voucher = contenido.Split('\n');

                    flag = _printVoucher.PrintVoucherMerchant(voucher);
                }
                else
                {
                    CustomMessageBox.Show("Informacion", "No existe el voucher");
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Imprimir ultimo voucher ERROR", ex.Message);
            }
            return flag;
        }

        public void SelecItemproductList(VmProduct product, int count = 0)
        {
            count++;
            if (product.Id != 0)
            {
                try
                {
                    dgListProduct.SelectedIndex = dgListProduct.Items.IndexOf(product);
                    dgListProduct.ScrollIntoView(dgListProduct.Items[dgListProduct.Items.IndexOf(product)]);
                }
                catch (Exception )
                {
                    if (count > 3)
                        return;

                    dgListProduct.Items.Refresh();
                    SelecItemproductList(product, count);
                }

            }
        }

        public void CalculateCancelled(List<PaymentMethod> pPaymentlist)
        {
            double pagado = 0;

            try
            {
                foreach (var item in pPaymentlist)
                {
                    if (item.CodigoMoneda == "001")
                    {
                        pagado += item.Monto;
                    }
                    else
                    {
                        pagado += item.Monto * _infoCaja.Tasa;
                    }
                    pagado = Math.Round(pagado, 8);
                }

                _Invoice.Cancelled = pagado;

            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error al calcular el monto cancelado", ex.Message);

            }
        }

        public double Round(double value)
        {
            return Math.Round((value * 100) - (Math.Round(value * 100, 0)), 2) == 0 ? value : Math.Ceiling(100 * value) / 100;
        }

        public void SetScale()
        {
            if (this.ActualWidth < 1300)
            {
                dgListProduct.Columns[1].Width = 200;
                dgListProduct.Columns[0].Width = 80;

            }
            else
            {
                dgListProduct.Columns[1].Width = 350;
                dgListProduct.Columns[0].Width = 100;

            }
        }

        private void ApplyEffect(Window pwindow)
        {
            var objBlur = new System.Windows.Media.Effects.BlurEffect
            {
                Radius = 10
            };
            pwindow.Effect = objBlur;
            //App.Current.MainWindow.Effect = objBlur;
        }
        private void RemoveEffect(Window pwindow)
        {
            pwindow.Effect = null;
        }

        private void CloseScale()
        {
            if (_Scale != null)
            {
                try
                {
                    //cerrar balanza
                    _Scale.DeviceEnabled = false;
                    _Scale.ReleaseDevice();
                    _Scale.Close();
                }
                catch (Exception)
                {
                    //CustomMessageBox.Show("Opos Balanza Error", ex.Message);
                }

            }

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

        private void CloseScanner()
        {
            if (_Scanner != null)
            {
                try
                {
                    //cerrar escanner
                    _Scanner.DeviceEnabled = false;
                    _Scanner.ReleaseDevice();
                    _Scanner.Close();
                }
                catch (Exception)
                {
                    //CustomMessageBox.Show("Opos Scanner Error", ex.Message);
                }

            }

        }

        private XmlDocument XmlReportZS1(ReportData reportZ, S1PrinterData statusS1)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement root = doc.CreateElement("Root");
            XmlElement zetaNode = doc.CreateElement("ReporteZ");
            XmlElement s1Node = doc.CreateElement("S1");

            try
            {
                zetaNode.SetAttribute("MontoTotalDeBaseImponibleAdicional", reportZ.AdditionalRate3Sale.ToString());
                zetaNode.SetAttribute("MontotiotalIvaAdicional", reportZ.AdditionalRate3Tax.ToString());
                zetaNode.SetAttribute("AcumuladosEnNotasDeDebitoTasaAdicional", reportZ.AdditionalRateDebit.ToString());
                zetaNode.SetAttribute("MontoTotalBaseImponibleAdicionalEnDevolucion", reportZ.AdditionalRateDevolution.ToString());
                zetaNode.SetAttribute("MontoTotalDeIvaTasaAdicionalEnNotaDeDebito", reportZ.AdditionalRateTaxDebit.ToString());
                zetaNode.SetAttribute("MontoToalDeIvaAdicionalEnDevolucion", reportZ.AdditionalRateTaxDevolution.ToString());
                zetaNode.SetAttribute("MontoTotalExento", reportZ.FreeSalesTax.ToString());
                zetaNode.SetAttribute("AcumuladosEnNotasDeDebitoExentas", reportZ.FreeTaxDebit.ToString());
                zetaNode.SetAttribute("MontoTotalExcentoEnDevolucion", reportZ.FreeTaxDevolution.ToString());
                zetaNode.SetAttribute("MontoTotalDeBaseImponibleGeneral", reportZ.GeneralRate1Sale.ToString());
                zetaNode.SetAttribute("MontoTotalDeIvaGeneral", reportZ.GeneralRate1Tax.ToString());
                zetaNode.SetAttribute("AcumuladosEnNotasDeDEbitoTasaGeneral", reportZ.GeneralRateDebit.ToString());
                zetaNode.SetAttribute("MontoTotalDeBaseImponibleGeneralEnDevolucion", reportZ.GeneralRateDevolution.ToString());
                zetaNode.SetAttribute("MontoTotalDeIvaGeneralEnNotaDeDebito", reportZ.GeneralRateTaxDebit.ToString());
                zetaNode.SetAttribute("MontoTotalDeIvaGeneralEnDevolucion", reportZ.GeneralRateTaxDevolution.ToString());
                zetaNode.SetAttribute("FechaUltimaFactura", reportZ.LastInvoiceDate.ToString());
                zetaNode.SetAttribute("NumeroUltimaNotaDeCredito", reportZ.NumberOfLastCreditNote.ToString());
                zetaNode.SetAttribute("NumeroUltimaNotaDeDebito", reportZ.NumberOfLastDebitNote.ToString());
                zetaNode.SetAttribute("NumeroDeLaUltimaFactura", reportZ.NumberOfLastInvoice.ToString());
                zetaNode.SetAttribute("NumeroDelUltimoDocumentoNoFiscal", reportZ.NumberOfLastNonFiscal.ToString());
                zetaNode.SetAttribute("NumeroUltimoReporteZeta", reportZ.NumberOfLastZReport.ToString());
                zetaNode.SetAttribute("MontoTotalDeBaseImponibleReducida", reportZ.ReducedRate2Sale.ToString());
                zetaNode.SetAttribute("MontoTotalDeIvaReducido", reportZ.ReducedRate2Tax.ToString());
                zetaNode.SetAttribute("AcumuladosEnNotasDeDEbitoTasaReducida", reportZ.ReducedRateDebit.ToString());
                zetaNode.SetAttribute("MontoTalDeBaseImponibleReduciedaEnDevolucion", reportZ.ReducedRateDevolution.ToString());
                zetaNode.SetAttribute("MontoTotalIvaTasaReducidaEnNotaDeDebito", reportZ.ReducedRateTaxDebit.ToString());
                zetaNode.SetAttribute("MontoTotalIvaReducidoEnDevolucion", reportZ.ReducedRateTaxDevolution.ToString());
                zetaNode.SetAttribute("FechaUltimoReporte", reportZ.ZReportDate.ToString());
                root.AppendChild(zetaNode);


                s1Node.SetAttribute("NumeroDeContadorDeReporteDeAuditoria", statusS1.AuditReportsCounter.ToString());
                s1Node.SetAttribute("NumeroDelCajeroActivo", statusS1.CashierNumber.ToString());
                s1Node.SetAttribute("FechaActualDeLaImpresora", statusS1.CurrentPrinterDateTime.ToString());
                s1Node.SetAttribute("NumeroContadorDeCierre", statusS1.DailyClosureCounter.ToString());
                s1Node.SetAttribute("NumeroUltimaNotaDeCreditoEmitida", statusS1.LastCreditNoteNumber.ToString());
                s1Node.SetAttribute("NumeroDeLaUltimaNotaDeDEbitoEmitida", statusS1.LastDebitNoteNumber.ToString());
                s1Node.SetAttribute("NumeroDeLaUltimaFacturaEmitida", statusS1.LastInvoiceNumber.ToString());
                s1Node.SetAttribute("NumeroDelUltimoDocumentoNoFiscalEmitido", statusS1.LastNonFiscalDocNumber.ToString());
                s1Node.SetAttribute("CantidadDeDocumentosNoFiscales", statusS1.QuantityNonFiscalDocuments.ToString());
                s1Node.SetAttribute("CantidadDeNotasDeCreditoEmitidasEnElDia", statusS1.QuantityOfCreditNotesToday.ToString());
                s1Node.SetAttribute("CantidadDeNotasDeDebitoEmitidasEnElDia", statusS1.QuantityOfDebitNotesToday.ToString());
                s1Node.SetAttribute("CantidadDeFacturasEmitidasEnElDia", statusS1.QuantityOfInvoicesToday.ToString());
                s1Node.SetAttribute("NumeroDeSerialDeLaImpresoraFiscal", statusS1.RegisteredMachineNumber.ToString());
                s1Node.SetAttribute("RifDeFiscalizacion", statusS1.RIF.ToString());
                s1Node.SetAttribute("MontoTotalDeLasVentasDiarias", statusS1.TotalDailySales.ToString());
                root.AppendChild(s1Node);
                doc.AppendChild(root);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("error", ex.Message);
            }

            return doc;
        }

        private void CloseApp()
        {
            cacheInvoice.DeleteArchive();
            CloseScale();
            CloseScanner();
            _printer.CloseFpCtrl();

            MainLogin mainLogin = new MainLogin();
            mainLogin.Show();
            this.Close();
        }




        #endregion utilidades

      
    }
}