using KeyPad;
using Models;
using Newtonsoft.Json;
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
using wpfPos_Rios.ViewModels;
using wpfPos_Rios.ViewModels.helperResponse;
using wpfPos_Rios.Views.helper;
using TfhkaNet.IF;
using TfhkaNet.IF.VE;
using Peripherals.printerHKA80;
using System.Configuration;

namespace wpfPos_Rios.Views
{
    /// <summary>
    /// Interaction logic for RePrintInvoiceWindow.xaml
    /// </summary>
    public partial class RePrintInvoiceWindow : Window
    {
        //Declaracion de Objetos

        readonly Caja _cajaData;

        //Declaracion del servicio del Api

        readonly InvoiceService _InvoiceService;

        VmInvoiceReturn _invoiceReturn;

        //Declaracion de los metodos de la impresora
        readonly Tfhka _printer;
        readonly PrintVouchers _printVoucher;

        //Modelo del estado de la impresora
        readonly NumberFormatInfo _FormatVenezuela = new CultureInfo("es-VE").NumberFormat;


        public RePrintInvoiceWindow(Caja pCajaDatos, Tfhka pPrinter)
        {
            InitializeComponent();

            //Se obtienen los datos de la caja por parametros
            _cajaData = pCajaDatos;
            _printer = pPrinter;
            _printVoucher = new PrintVouchers(_printer, ConfigPosStatic.ImpresoraConfig.port, ConfigPosStatic.ImpresoraConfig.flat);
            //Iniciar pagina
            _InvoiceService = new InvoiceService();

            _invoiceReturn = new VmInvoiceReturn();
            // _IdUser = pIdUsuario;
        }


        #region Metodos Para buscar la factura

        private void SearchInvoice(string pNumerofactura)
        {
            if (pNumerofactura != "")
            {
                try
                {

                    Response result = _InvoiceService.GetInvoice(txtDescrip.Text, _cajaData.TiendaId, _cajaData.Id, _cajaData.SerialImpresora);

                    if (result.success == 1)
                    {
                        if (result.data != null)
                        {

                            InvoiceResponse returnResponse = JsonConvert.DeserializeObject<InvoiceResponse>(result.data.ToString());

                            VmInvoiceReturn vmInvoice = new VmInvoiceReturn
                            {
                                id = returnResponse.factura.id,
                                rif = returnResponse.cliente.rif,
                                nombre = returnResponse.cliente.nombre,
                                apellido = returnResponse.cliente.apellido,
                                fecha = returnResponse.factura.fecha,
                                montoBruto = returnResponse.factura.montoBruto,
                                montoDescuento = returnResponse.factura.montoDescuento,
                                montoIVA = returnResponse.factura.montoIVA,
                                montoNeto = returnResponse.factura.montoNeto,
                                numeroControl = returnResponse.factura.numeroControl,
                                numeroFactura = returnResponse.factura.numeroFactura,
                                idCaja = _cajaData.Id,
                                cajero = returnResponse.factura.cajera,
                                codigoCaja = _cajaData.CodigoCaja,
                                serialFiscal = _cajaData.SerialImpresora,
                                idTienda = _cajaData.TiendaId,


                                detallesList = new List<VmDetalleFacturaReturn>()
                            };
                            foreach (var item in returnResponse.detalleFactura)
                            {
                                VmDetalleFacturaReturn vmDetalleFactura = new VmDetalleFacturaReturn
                                {
                                    id = item.id,
                                    codigoproducto = item.codigoproducto,
                                    descripcion = item.descripcion,
                                    cantidad = item.cantidad,
                                    precio = item.precio,
                                    precioneto = item.precioneto,
                                    totalneto = item.totalneto,
                                    cantidadDevolver = 0,
                                    tipo = item.tipo,
                                    total = item.total
                                };

                                vmInvoice.detallesList.Add(vmDetalleFactura);

                            }

                            dgListInvoice.Items.Add(vmInvoice);

                            _invoiceReturn = vmInvoice;



                            if (vmInvoice.montoDescuento > 0)
                            {
                                dgListInvoice.Columns[5].Visibility = Visibility.Visible;
                            }

                            dgListInvoice.Visibility = Visibility.Visible;

                            lblInvoice.Text = "Numero de Documento:";
                            lbInvoiceNum.Content = _invoiceReturn.numeroControl;
                            lblDate.Visibility = Visibility.Visible;
                            lbInvoiceDate.Content = _invoiceReturn.fecha;

                            btnRePrint.IsEnabled = true;
                            txtDescrip.IsEnabled = false;


                        }
                        else
                        {
                            CustomMessageBox.Show("Información", "El numero de factura buscado no existe, por favor revise los datos e intente de nuevo.");
                        }


                    }
                    else
                    {
                        CustomMessageBox.Show("Problemas de conexión", "No se ha podido establecer conexión con el servidor, por favor pongase en contacto con servicio técnico");
                    }

                }
                catch (Exception ex)
                {
                    Response data = JsonConvert.DeserializeObject<Response>(ex.Message);
                    CustomMessageBox.Show("Error", data.message);
                }
            }
            else
            {
                CustomMessageBox.Show("Campo Vacio", "Ingrese el numero de factura a devolver");
            }

        }

        private void InicializeInvoiceReturn()
        {
            dgListInvoice.Visibility = Visibility.Hidden;

            this.WindowState = WindowState.Normal;
            btnRePrint.IsEnabled = false;
            txtDescrip.IsEnabled = true;
            txtDescrip.Text = "";
            lbInvoiceDate.Content = ""; lblDate.Visibility = Visibility.Hidden;
            lbInvoiceNum.Content = ""; lblInvoice.Text = "Numero de Factura:";
            dgListInvoice.ItemsSource = null;
            dgListInvoice.Items.Clear();


        }

        #endregion

        #region Metodos de Imprimir 

        private void PrintInvoiceCopy()
        {

            //Verificar si esta lista para imprimir
            if (_printer.CheckFPrinter())
            {
                //calcular y llenar el modelo com los datos de la factura
                List<string> listaDatos = new List<string>
                {
                    //listaDatos.Add(infoWallet.ClientNombre+ " " + infoWallet.ClientApellido+ " "+ infoWallet.ClientRif);
                    "Documento:" + _invoiceReturn.numeroControl,
                    "Cliente:" + _invoiceReturn.nombre + " " + _invoiceReturn.apellido,
                    "RIF/C.I:" + _invoiceReturn.rif,
                    "Cajero:" + _invoiceReturn.cajero + "       Caja:" + _invoiceReturn.codigoCaja + "",
                    "FACTURA:" + _invoiceReturn.numeroFactura,
                    "FECHA:" + _invoiceReturn.fecha.ToString(),
                    ""
                };
                foreach (var item in _invoiceReturn.detallesList)
                {
                    listaDatos.Add(item.cantidad.ToString() + "x " + item.precio.ToString("C", _FormatVenezuela));
                    listaDatos.Add(Truncate(item.descripcion, 35));
                    listaDatos.Add("           " + "" + (item.precio * item.cantidad).ToString("C", _FormatVenezuela));
                }
                listaDatos.Add("");
                listaDatos.Add("SUBTOTAL:" + " " + _invoiceReturn.montoBruto.ToString("C", _FormatVenezuela));
                listaDatos.Add("TOTAL:" + "    " + _invoiceReturn.montoNeto.ToString("C", _FormatVenezuela));
                listaDatos.Add("Gracias por preferirnos!!!");
                listaDatos.Add("");
                listaDatos.Add("Siguénos en instagram como: @rio_supermarket");
                listaDatos.Add("");

                //Imprimir copia

                bool resultado = _printVoucher.CmdImpDNF("COPIA FACTURA", "COPIA FACTURA CLIENTE", "", listaDatos);

                if (resultado)
                {
                    this.Close();

                }
                else
                {
                    CustomMessageBox.Show("Informacion", "Error al imprimir copia de la factura, comuniquese con soporte");
                    InicializeInvoiceReturn();
                    return;
                }
            }
            else
            {
                PrinterStatus status = _printer.GetPrinterStatus();
                CustomMessageBox.Show($"Error en la Impresora Fiscal : {status.PrinterErrorCode}", status.PrinterStatusDescription + "\n" + status.PrinterErrorDescription);
            }
        }


        #endregion


        #region Eventos Botonera Principales

        private void BtnBuscarFac_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgListInvoice.Items.Count > 0)
                {
                    CustomMessageBox.Show("Información", "Estimado ya tiene una factura por reimprimir, cambie o cancele si desea imprimir otra.");
                }
                else
                {
                    SearchInvoice(txtDescrip.Text);

                }


            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(ex.Message);
            }
        }

        private void BtnRePrint_Click(object sender, RoutedEventArgs e)
        {
            var messageBoxResult = CustomMessageBox.Show("Confirmar", "¿Esta seguro que desea realizar una reimpresión de la Factura Numero: " + txtDescrip.Text + "?", MessageBoxButton.YesNo, CustomMessageBox.MessageBoxImage.Question);

            if (messageBoxResult == MessageBoxResult.Yes)
            {
                PrintInvoiceCopy();

            }
            else
            {
                return;
            }

        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        #endregion

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            var messageBoxResult = CustomMessageBox.Show("Confirmar", "¿Esta seguro que desea cancelar la reimpresión de la Factura Numero: " + txtDescrip.Text + "?", MessageBoxButton.YesNo, CustomMessageBox.MessageBoxImage.Question);

            if (messageBoxResult == MessageBoxResult.Yes)
            {
                InicializeInvoiceReturn();

            }
            else
            {
                return;
            }
        }



        #region Eventos de botones
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
        private void TxtDescrip_TouchDown(object sender, TouchEventArgs e)
        {
            try
            {

                txtDescrip.Text = "";
                bool? keypad = ShowKeypad(txtDescrip, this);

                if (keypad == true)
                {


                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error", ex.Message);

            }
        }

        private void TxtDescrip_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Enter)
            {
                SearchInvoice(txtDescrip.Text);
            }
        }

        public string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;

            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        #endregion
    }
}
