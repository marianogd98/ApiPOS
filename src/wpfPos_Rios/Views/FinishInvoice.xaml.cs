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
using TfhkaNet.IF.VE;
using System.Threading;
using System.Globalization;
using Peripherals.printerHKA80;
using System.Configuration;
using TfhkaNet.IF;

namespace wpfPos_Rios.Views
{
    /// <summary>
    /// Lógica de interacción para FinishInvoice.xaml
    /// </summary>
    public partial class FinishInvoice : Window
    {
        //servicios y respuesta
        readonly InvoiceService _serviceInvoice;
        readonly Tfhka _Printer;
        readonly Caja _cajaData;
        readonly Invoice _Invoice;
        readonly Client _Client;
        readonly string _cajera;
        readonly PrintVouchers _printVoucher;
        readonly NumberFormatInfo _FormatVenezuela = new CultureInfo("es-VE").NumberFormat;
        public FinishInvoice(Tfhka pPrinter, Caja pCajaDatos, Invoice pInvoice,  Client pClientModel , string cajera)
        {
            InitializeComponent();
            _FormatVenezuela.NumberGroupSeparator = ".";
            _FormatVenezuela.NumberDecimalSeparator = ",";

            _serviceInvoice = new InvoiceService();
            _Printer = pPrinter;
            _cajaData = pCajaDatos;
            _Invoice = pInvoice;
            _Client = pClientModel;
            _cajera = cajera;
            _printVoucher = new PrintVouchers(_Printer, ConfigPosStatic.ImpresoraConfig.port, ConfigPosStatic.ImpresoraConfig.flat);
            SetFrom();
            dgListProduct.ItemsSource = _Invoice.listaProductos;
        }

        public void SetFrom()
        {
            txtbRifClient.Text = _Client.RIF;
            txtbNameClient.Text = _Client.Nombre + " " + _Client.Apellido;
            txtbNrofactura.Text = _Invoice.NumeroFactura;
            txtbNroCaja.Text = _cajaData.CodigoCaja;
            txtbTotalBs.Text = _Invoice.MontoNeto.ToString("N2");
            txtbCajero.Text = _cajera;
            
        }

        public void UpdateInvoice(int id)
        {
            Response request = _serviceInvoice.UpdateInvoice(id);

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
         public void CancelInvoice(int id)
        {
            try
            {
                Response request = new Response();
                request = _serviceInvoice.CancelInvoice(id);
                CustomMessageBox.Show("informacion" , request.message);
                this.Close();
            }
            catch (Exception ex)
            {
                Response data = JsonConvert.DeserializeObject<Response>(ex.Message);
                CustomMessageBox.Show("Error", data.message);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var answer = CustomMessageBox.Show("Confirmar", "Confirmar facturacion ?", MessageBoxButton.YesNo);

            switch (answer)
            {
                case MessageBoxResult.Yes:

                    string name = _Client.Nombre + " " + _Client.Apellido;
                    double descuento = Math.Round((_Invoice.MontoDescuento / _Invoice.MontoBruto) * 10000, 2);
                    int wait = _Invoice.listaProductos.Count() / 2;
                    
                    try
                    {
                        bool print = _printVoucher.CmdImpFactura(_Invoice.NumeroControl, name, _Client.RIF, _Client.Direccion, _cajera,
                       _cajaData.CodigoCaja, "101", _Invoice.listaProductos , _Invoice.listaPagos);
                        
                        if (print && _Invoice.NumeroFactura == _Printer.GetS1PrinterData().LastInvoiceNumber.ToString("00000000"))
                        {
                            UpdateInvoice(_Invoice.id);
                            Thread.Sleep(wait * 100);
                            this.Close();
                        }
                        else
                        {
                            PrinterStatus status = _Printer.GetPrinterStatus();
                            CustomMessageBox.Show($"Error en la Impresora Fiscal : {status.PrinterErrorCode}", status.PrinterStatusDescription + "\n" + status.PrinterErrorDescription);
                        }

                    }
                    catch (Exception ex)
                    {
                        CustomMessageBox.Show("Advertencia", ex.Message);
                    }
                    break;
            }
            
           
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var answer = CustomMessageBox.Show("Confirmar" , "Esta seguro de anular esta factura ?" , MessageBoxButton.YesNo);

            switch (answer)
            {
                case MessageBoxResult.Yes:
                    CancelInvoice(_Invoice.id);
                    break;
            }
            
        }

        private void Header_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
