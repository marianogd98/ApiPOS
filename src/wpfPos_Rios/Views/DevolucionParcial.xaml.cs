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
using wpfPos_Rios.service;
using wpfPos_Rios.ViewModels;
using Newtonsoft.Json;
using wpfPos_Rios.Views.helper;
using wpfPos_Rios.ViewModels.helperResponse;
using TfhkaNet.IF;
using TfhkaNet.IF.VE;
using Peripherals.printerHKA80;
using System.Configuration;

namespace wpfPos_Rios.Views
{
    /// <summary>
    /// Lógica de interacción para DevolucionParcial.xaml
    /// </summary>
    public partial class DevolucionParcial : Window
    {
        readonly PrintVouchers _printVoucher;
        readonly Tfhka _printer;

        //Declaracion del servicio del Api
        readonly InvoiceService _InvoiceService;
        readonly UserCajera _Cajera;
        readonly VmInvoiceReturn _invoiceReturn;
        readonly List<VmDetalleFacturaReturn> listProductReturn;
        bool flagDevolucion;
        public DevolucionParcial(VmInvoiceReturn pinvoiceReturn, Tfhka pPrinter, UserCajera pCajera)
        {
            InitializeComponent();
            flagDevolucion = false;
            listProductReturn = new List<VmDetalleFacturaReturn>();
            _printer = pPrinter;
            _printVoucher = new PrintVouchers(_printer, ConfigPosStatic.ImpresoraConfig.port, ConfigPosStatic.ImpresoraConfig.flat);
            _invoiceReturn = pinvoiceReturn;
            _InvoiceService = new InvoiceService();
            _Cajera = pCajera;
            dgListProduct.ItemsSource = _invoiceReturn.detallesList;
            dgListReturn.ItemsSource = listProductReturn;
            btnReturn.IsEnabled = false;
        }

        public bool GetDevolucion()
        {
            return flagDevolucion;
        }
      



        public void HabilitarDevolucion()
        {
            btnReturn.IsEnabled = true;
        }
        public void DeshabilitarDevolucion()
        {
            btnReturn.IsEnabled = true;
        }

        private void BtnAddReturn_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            VmDetalleFacturaReturn product = btn.DataContext as VmDetalleFacturaReturn;
            try
            {
                if (!listProductReturn.Exists(p => p == product))
                {
                    VmDetalleFacturaReturn data = _invoiceReturn.detallesList.Where(p => p == product).First();
                    if (data.pesado)
                    {
                        data.cantidadDevolver = data.cantidad;
                    }
                    else
                    {
                        if (data.cantidad > 1)
                        {
                            cantidadDevParcial cantProduct = new cantidadDevParcial(data);
                            cantProduct.Activate();
                            cantProduct.ShowDialog();
                        }
                        else
                        {
                            data.cantidadDevolver = data.cantidad;
                        }
                    }
                    if (data.cantidadDevolver > 0)
                    {
                        listProductReturn.Add(data);
                    }
                    dgListReturn.Items.Refresh();

                    if (listProductReturn.Count > 0 && btnReturn.IsEnabled == false)
                    {
                        HabilitarDevolucion();
                    }
                }
                else
                {
                    CustomMessageBox.Show("Inforamcion", "Este producto ya fue agregado a la devolucion");
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error", ex.Message);
            }
        }

        private void BtnRemoveReturn_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            VmDetalleFacturaReturn product = btn.DataContext as VmDetalleFacturaReturn;

            if (listProductReturn.Exists(p => p == product))
            {
                listProductReturn.Remove(product);
                if (listProductReturn.Count == 0)
                {
                    DeshabilitarDevolucion();
                }
                dgListReturn.Items.Refresh();
            }
            else
            {
                CustomMessageBox.Show("Inforamcion", "El producto no se encuentra");
            }
        }

        private void PrintReturn(InvoiceToReturn invoice , VmInvoiceReturn _invoiceReturn)
        {
            try
            {
                //Verificar si esta lista para imprimir
                if (_printer.CheckFPrinter())
                {
                    bool result = _printVoucher.CmdImpNotaCredito(_invoiceReturn.numeroFactura, _invoiceReturn.fecha.ToShortDateString(), invoice.serialFiscal, invoice.numeroControl,
                       _invoiceReturn.nombre + " " + _invoiceReturn.apellido, _invoiceReturn.rif, "Supervisor", _invoiceReturn.codigoCaja, invoice.detallesList);

                    if (result)
                    {
                        CustomMessageBox.Show("Información", "Devolucion realizada con exito");
                        flagDevolucion = true;
                        this.Hide();
                    }
                    else
                    {
                        CustomMessageBox.Show("Error al imprimir la devolucion", "Comuniquese con soporte tecnico, error al imprimir la devolucion");
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
                CustomMessageBox.Show("Error al imprimir devolucion", ex.Message);
            }
        }

        private void BtnReturn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                InvoiceToReturn invoiceToReturn = new InvoiceToReturn
                {
                    idFactura = _invoiceReturn.id,
                    idCaja = _invoiceReturn.idCaja,
                    idTienda = _invoiceReturn.idTienda,
                    idUsuario = _invoiceReturn.idUsuario,
                    numeroControl = _invoiceReturn.numeroControl,
                    serialFiscal = _invoiceReturn.serialFiscal,
                    idTurno = _Cajera.idTurno,
                    formaPago = "",
                    detallesList = new List<DetalleFactura>()
                };

                listProductReturn.ForEach(p =>
                {
                    invoiceToReturn.detallesList.Add(new DetalleFactura()
                    {
                        id = p.id,
                        cantidad = p.cantidad,
                        cantidadDevolver = p.cantidadDevolver,
                        codigoproducto = p.codigoproducto,
                        descripcion = p.descripcion,
                        precio = p.precio,
                        precioneto = p.precioneto,
                        tipo = p.tipo,
                        total = p.cantidadDevolver * p.precio,
                        totalneto = p.cantidadDevolver * p.precio
                    });
                });
                double init = 0;
                invoiceToReturn.montoDevolucion = invoiceToReturn.detallesList.Aggregate(init , (accum , current) => current.totalneto + accum);

                //imprimir
                S1PrinterData dataS1 = _printer.GetS1PrinterData();
                string notaCredito = dataS1.LastCreditNoteNumber.ToString("00000000");
                //sumar 1  ese numero de la nota de credito
                string numInvoice = (int.Parse(notaCredito) + 1).ToString("00000000");

                invoiceToReturn.idNotaCredito = numInvoice;

                Response request = _InvoiceService.ReturnParcialInvoice(invoiceToReturn);

                if (request.success == 1 )
                {
                    PrintReturn(invoiceToReturn, _invoiceReturn);
                }
                else
                {
                    CustomMessageBox.Show("Error en devolucion", request.message);
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
