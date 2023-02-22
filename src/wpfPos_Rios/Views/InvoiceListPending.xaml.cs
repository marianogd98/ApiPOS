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
using Newtonsoft.Json;
using wpfPos_Rios.ViewModels;
using wpfPos_Rios.Views.helper;
using wpfPos_Rios.ViewModels.helperResponse;

namespace wpfPos_Rios.Views
{
    /// <summary>
    /// Lógica de interacción para InvoiceListPending.xaml
    /// </summary>
    public partial class InvoiceListPending : Window
    {
        //services
        InvoiceService _InvoiceService = new InvoiceService();
        List<vmInvoicePending> _ListPendingInvoice;
        VmInvoice _Invoice;
        VmClient _Client;
        Caja _Caja;
        List<PaymentMethod> _Paymentlist;
        public InvoiceListPending(int idTienda, int idCaja, VmInvoice pInvoice, VmClient pClient, Caja pCaja, List<PaymentMethod> pPaymentlist)
        {
            InitializeComponent();
            _Invoice = pInvoice;
            _Client = pClient;
            _Caja = pCaja;
            _Paymentlist = pPaymentlist;
            Response response = _InvoiceService.GetInvoicesPending(idTienda, idCaja);

            if (response.data != null)
            {
                _ListPendingInvoice = new List<vmInvoicePending>();
                _ListPendingInvoice = JsonConvert.DeserializeObject<List<vmInvoicePending>>(response.data.ToString());
                dgListInvoice.ItemsSource = _ListPendingInvoice;
            }     
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        

        private void BtnInsert_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                vmInvoicePending data = (vmInvoicePending)dgListInvoice.SelectedItem;

                if (data == null)
                {
                    return;
                }
                else
                {
                    var messageBoxResult = CustomMessageBox.Show("Confirmar", "Confirmar recuperacion de la factura", MessageBoxButton.YesNoCancel);
                    Response result = _InvoiceService.GetInvoicePending(data.id);
                    
                    switch (messageBoxResult)
                    {
                        case MessageBoxResult.Yes:


                            if (result.success == 1)
                            {
                                InvoiceClientResponse invoiceTasaClient = JsonConvert.DeserializeObject<InvoiceClientResponse>(result.data.ToString());
                                //set tasa
                                _Caja.Tasa = invoiceTasaClient.tasa.monto;
                                //set cliente
                                _Client.Id = invoiceTasaClient.cliente.id;
                                _Client.Rif = invoiceTasaClient.cliente.rif;
                                _Client.Name = invoiceTasaClient.cliente.nombre;
                                _Client.LastName = invoiceTasaClient.cliente.apellido;
                                _Client.Cellnumber = invoiceTasaClient.cliente.telefono;
                                _Client.Email = invoiceTasaClient.cliente.email;
                                _Client.Address = invoiceTasaClient.cliente.direccion;
                                _Client.Saldo = Math.Round(invoiceTasaClient.cliente.saldo, 4);

                                foreach (var product in invoiceTasaClient.factura.listaProductos)
                                {
                                    VmProduct productAgg = new VmProduct(_Caja.Tasa);
                                    productAgg.Id = product.id;
                                    productAgg.Code = product.CodigoProducto;
                                    productAgg.Descripcion = product.Descripcion;
                                    productAgg.Quantity = product.Cantidad;
                                    productAgg.Pesado = product.Pesado;
                                    productAgg.DiscountPercentage = product.Descuento;
                                    //calcular precios
                                    if (product.CodigoMoneda == "002")
                                    {
                                        productAgg.UnitPrice = Math.Round(product.PrecioDetal, 2);
                                        productAgg.UnitPriceBs = product.PrecioDetal * _Caja.Tasa;
                                    }
                                    else
                                    {
                                        productAgg.UnitPriceBs = Math.Round(product.PrecioDetal, 2);
                                        productAgg.UnitPrice = Round(product.PrecioDetal / _Caja.Tasa);
                                    }
                                    _Invoice.ProductList.Add(productAgg);
                                }

                                if (invoiceTasaClient.factura.listaPagos != null)
                                {
                                    if (invoiceTasaClient.factura.listaPagos.Count > 0)
                                    {
                                        foreach (var pay in invoiceTasaClient.factura.listaPagos)
                                        {

                                            PaymentMethod payM = new PaymentMethod();
                                            payM.Id = pay.Id;
                                            payM.Lote = pay.Lote;
                                            payM.Monto = pay.Monto;
                                            payM.NumeroTransaccion = pay.NumeroTransaccion;
                                            payM.CodigoMoneda = (pay.Id == 6) ? "002" : "001";
                                            if (pay.CuentaBancariaId != null)
                                            {
                                                payM.CuentaBancariaId = pay.CuentaBancariaId;
                                            }
                                            payM.Nombre = pay.Nombre;
                                            payM.tipoTarjeta = pay.tipoTarjeta;
                                            payM.nroAutorizacion = pay.nroAutorizacion;
                                            if (pay.vpos != null)
                                            {
                                                payM.vpos = pay.vpos;
                                            }

                                            _Paymentlist.Add(payM);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                CustomMessageBox.Show("informacion", result.message);
                            }
                            this.Close();
                            break;

                        case MessageBoxResult.No:
                            break;
                    }
                }
            }

            catch (Exception ex)
            {

                CustomMessageBox.Show("Error", ex.Message + "\n" + ex.StackTrace.ToString());

            }
        }
        public double Round(double value)
        {
            return Math.Round((value * 100) - (Math.Round(value * 100, 0)), 2) == 0 ? value : Math.Ceiling(100 * value) / 100;
        }
    }
}

