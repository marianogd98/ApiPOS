using Models;
using Newtonsoft.Json;
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
using wpfPos_Rios.ViewModels;
using wpfPos_Rios.ViewModels.helperResponse;
using wpfPos_Rios.Views.helper;
using TfhkaNet.IF;
using TfhkaNet.IF.VE;
using Peripherals.printerHKA80;
using System.Configuration;
using KeyPad;

namespace wpfPos_Rios.Views
{
    /// <summary>
    /// Lógica de interacción para DevolverFactura.xaml
    /// </summary>
    public partial class DevolverFactura : Window
    {
        readonly Caja _caja;
        readonly int _idUsuario;
        readonly Tfhka _printer;
        readonly PrintVouchers _printVoucher;
        readonly UserCajera _cajera;
        readonly InvoiceService _InvoiceService;
        VmInvoiceReturn _invoiceReturn;
        public DevolverFactura(Caja pCajaDatos, int pIdUsuario, Tfhka pPrinter, UserCajera pCajera)
        {
            InitializeComponent();
            _InvoiceService = new InvoiceService();
            _caja = pCajaDatos;
            _idUsuario = pIdUsuario;
            _printer = pPrinter;
            _printVoucher = new PrintVouchers(_printer, ConfigPosStatic.ImpresoraConfig.port, ConfigPosStatic.ImpresoraConfig.flat);
            _cajera = pCajera;
            _invoiceReturn = new VmInvoiceReturn();
            btnDParcial.Visibility = Visibility.Hidden;
            btnDTotal.Visibility = Visibility.Hidden;
        }

        private void TxtSerial_TouchDown(object sender, TouchEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            ShowKeyboard(textbox, this);
        }

        private void TxtDescrip_TouchDown(object sender, TouchEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            ShowKeypad(textbox, this);
        }
        private void ShowKeypad(TextBox pTextbox, Window pIndex)
        {
            KeypadNum keypad = new KeypadNum(pTextbox, pIndex);
            if (keypad.ShowDialog() == true)
                pTextbox.Text = keypad.Result;
        }
        private void ShowKeyboard(TextBox pTextbox, Window pIndex)
        {
            VirtualKeyboard keypad = new VirtualKeyboard(pTextbox, pIndex);
            if (keypad.ShowDialog() == true)
                pTextbox.Text = keypad.Result;
        }

        private void BtnSearchInvoice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnDParcial.Visibility = Visibility.Hidden;
                btnDTotal.Visibility = Visibility.Hidden;

                if (string.IsNullOrEmpty(txtNroFac.Text))
                {
                    CustomMessageBox.Show("Informacion", "Debe ingresar el numero de la factura");
                    return;
                }
                if (string.IsNullOrEmpty(txtSerial.Text))
                {
                    CustomMessageBox.Show("Informacion", "Debe ingresar el Serial de la impresora fiscal");
                    return;
                }
                if (txtSerial.Text != _caja.SerialImpresora)
                {
                    CustomMessageBox.Show("Información", "Estimado usuario esta no es la caja que corresponde a esa factura, por favor realice la devolución en la caja correspondiente" + Environment.NewLine + "para más información solicite ayuda a un personal capacitado.");
                    return;
                }

                Response resInvoice = _InvoiceService.GetInvoice(txtNroFac.Text, _caja.TiendaId, _caja.Id, _caja.SerialImpresora);

                if (resInvoice.success == 1)
                {
                    InvoiceResponse returnResponse = JsonConvert.DeserializeObject<InvoiceResponse>(resInvoice.data.ToString());

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
                        numeroFactura = returnResponse.factura.numeroFactura,
                        numeroControl = returnResponse.factura.numeroControl,
                        idCaja = _caja.Id,
                        codigoCaja = _caja.CodigoCaja,
                        serialFiscal = _caja.SerialImpresora,
                        idTienda = _caja.TiendaId,
                        idUsuario = _idUsuario,

                        detallesList = new List<VmDetalleFacturaReturn>()
                    };
                    returnResponse.detalleFactura.ForEach( product => {

                        vmInvoice.detallesList.Add(new VmDetalleFacturaReturn()
                        {
                            id = product.id,
                            tipo = product.tipo,
                            codigoproducto = product.codigoproducto,
                            descripcion = product.descripcion,
                            precio = product.precio,
                            total = product.total,
                            precioneto = product.precioneto,
                            cantidad = product.cantidad,
                            totalneto = product.totalneto,
                            pesado = product.pesado
                        });
                    });
                    dgListInvoice.Items.Clear();
                    dgListInvoice.Items.Add(vmInvoice);

                    if (vmInvoice.montoDescuento > 0)
                    {
                        dgListInvoice.Columns[5].Visibility = Visibility.Visible;
                    }

                    dgListProduct.ItemsSource = vmInvoice.detallesList;
                    dgListProduct.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                    //asignar data a la variable global
                    _invoiceReturn = vmInvoice;

                    btnDParcial.Visibility = Visibility.Visible;
                    btnDTotal.Visibility = Visibility.Visible;

                }
                else
                {
                    CustomMessageBox.Show("Iformacion", resInvoice.message);
                }

            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error" , ex.Message);
            }
        }

        private void BtnDTotal_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var messageBoxResult = CustomMessageBox.Show("Confirmar", "¿Esta seguro que desea realizar una devolucion Total de la Factura Numero: " + _invoiceReturn.numeroFactura + "?", MessageBoxButton.YesNo, CustomMessageBox.MessageBoxImage.Question);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    InvoiceToReturn invoiceToReturn = new InvoiceToReturn
                    {
                        detallesList = new List<DetalleFactura>()
                    };

                    for (int i = 0; i < _invoiceReturn.detallesList.Count; i++)
                    {
                        invoiceToReturn.detallesList.Add(new DetalleFactura() {
                            id = _invoiceReturn.detallesList[i].id,
                            tipo = _invoiceReturn.detallesList[i].tipo,
                            codigoproducto = _invoiceReturn.detallesList[i].codigoproducto,
                            descripcion = _invoiceReturn.detallesList[i].descripcion,
                            precio = _invoiceReturn.detallesList[i].precio,
                            total = _invoiceReturn.detallesList[i].total,
                            precioneto = _invoiceReturn.detallesList[i].precioneto,
                            cantidad = _invoiceReturn.detallesList[i].cantidad,
                            cantidadDevolver = _invoiceReturn.detallesList[i].cantidad,
                            totalneto = _invoiceReturn.detallesList[i].totalneto
                        });
                    }

                    invoiceToReturn.idTurno = _cajera.idTurno;
                    invoiceToReturn.idFactura = _invoiceReturn.id;
                    invoiceToReturn.idCaja = _invoiceReturn.idCaja;
                    invoiceToReturn.idTienda = _invoiceReturn.idTienda;
                    invoiceToReturn.idUsuario = _invoiceReturn.idUsuario;
                    invoiceToReturn.numeroControl = _invoiceReturn.numeroControl;
                    invoiceToReturn.serialFiscal = _invoiceReturn.serialFiscal;
                    invoiceToReturn.formaPago = "";

                    if (_printer.CheckFPrinter())
                    {
                        S1PrinterData dataS1 = _printer.GetS1PrinterData();
                        //recuperar ultima nota de credito
                        string notaCredito = dataS1.LastCreditNoteNumber.ToString("00000000");
                        //sumar 1  ese numero de la nota de credito
                        string numInvoice = (int.Parse(notaCredito) + 1).ToString("00000000");
                        invoiceToReturn.idNotaCredito = numInvoice;

                        //Guardar la devolucion
                        Response request = _InvoiceService.ReturnTotalInvoice(invoiceToReturn);

                        if (request.success == 1)
                        {

                            //imprimir

                            bool result = _printVoucher.CmdImpNotaCredito(_invoiceReturn.numeroFactura, _invoiceReturn.fecha.ToShortDateString(), 
                                _invoiceReturn.serialFiscal, _invoiceReturn.numeroControl, _invoiceReturn.nombre + " " + _invoiceReturn.apellido, 
                                _invoiceReturn.rif, "Supervisor", _invoiceReturn.codigoCaja, invoiceToReturn.detallesList);

                            if (result)
                            {
                                CustomMessageBox.Show("Información", request.message);
                                this.Close();
                            }
                            else
                            {
                                CustomMessageBox.Show("Error al imprimir la devolucion", "Comuniquese con soporte tecnico, error al imprimir la devolucion");
                            }

                        }
                        else
                        {
                            CustomMessageBox.Show("Información", request.message);
                        }


                    }
                    else
                    {
                        PrinterStatus status = _printer.GetPrinterStatus();
                        CustomMessageBox.Show($"Error en la Impresora Fiscal : {status.PrinterErrorCode}", status.PrinterStatusDescription + "\n" + status.PrinterErrorDescription);
                    }

                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error", ex.Message);
            }
        }

        private void BtnDParcial_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DevolucionParcial returnProducts = new DevolucionParcial(_invoiceReturn , _printer , _cajera);
                returnProducts.Activate();
                returnProducts.ShowDialog();

                if (returnProducts.GetDevolucion())
                {
                    returnProducts.Close();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error", ex.Message);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        
    }
}
