using KeyPad;
using Models;
using Newtonsoft.Json;
using OposScanner_CCO;
using Peripherals.helper;
using System;
using System.Collections.Generic;
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
using wpfPos_Rios.Views.helper;

namespace wpfPos_Rios.Views
{
    /// <summary>
    /// Lógica de interacción para ConsultarProductos.xaml
    /// </summary>
    public partial class ConsultarProductos : Window
    {

        // delagado para leer codigos de barra
        private delegate void SetCodeBartDeleg(string text);

        //balanza y scanner magellan 
        readonly OPOSScannerClass _scanner;
        readonly List<VmProduct> _products;
        private readonly double _tasa;

        public ConsultarProductos(double tasa , OPOSScannerClass scanner)
        {
            InitializeComponent();
            _scanner = scanner;
            InitScanner();
            _products = new List<VmProduct>();
            dgListProduct.ItemsSource = _products;
            _tasa = tasa;

        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
        
        public double Round(double value)
        {
            return Math.Round((value * 100) - (Math.Round(value * 100, 0)), 2) == 0 ? value : Math.Ceiling(100 * value) / 100;
        }

        public void SearchProducts(string pCode)
        {

            Product productData;

            if (pCode != "")
            {
                try
                {
                    // Busqueda en el modelo de la base de datos
                    ProductService _productService = new ProductService();
                    Response queryProduct = _productService.GetProductsbyCode(pCode);

                    if (queryProduct.success == 1)
                    {
                        productData = JsonConvert.DeserializeObject<Product>(queryProduct.data.ToString());
                        
                        if( productData.id != -1)
                        {
                            _products.Add(new VmProduct(_tasa)
                            {
                                Id = productData.id,
                                Code = productData.CodigoProducto,
                                Descripcion = productData.Descripcion,
                                UnitPrice = (productData.CodigoMoneda == "002") ? productData.PrecioDetal : Round(productData.PrecioDetal / _tasa),
                                UnitPriceBs = (productData.CodigoMoneda == "002") ? Math.Round(productData.PrecioDetal * _tasa, 2) : Math.Round(productData.PrecioDetal, 2)
                            });
                            dgListProduct.Items.Refresh();
                        }
                    }

                }
                catch (Exception Ex)
                {
                    CustomMessageBox.Show("Ha ocurrido un Inconveniente buscando producto", Ex.Message, MessageBoxButton.OK);
                }
            }

            txtCodigoProd.Text = "";
        }

        private void TxtCodigoProd_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchProducts(txtCodigoProd.Text);
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            CloseScanner();
            this.Close();
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

        private bool? ShowKeypad(TextBox pTextbox, Window window)
        {
            KeypadNum keypad = new KeypadNum(pTextbox, window);

            bool? flagKeypad = keypad.ShowDialog();

            if (flagKeypad == true)
            {
                pTextbox.Text = keypad.Result;
            }

            return flagKeypad;
        }

        public void InitScanner()
        {
         
            try
            {
                //crear evento para scannear
                _scanner.DataEvent += new _IOPOSScannerEvents_DataEventEventHandler(DataEvent);

                _scanner.DeviceEnabled = true;
                ResultCodeH.Check(_scanner.ResultCode);

                _scanner.AutoDisable = true;
                ResultCodeH.Check(_scanner.ResultCode);

                _scanner.DataEventEnabled = true;
                ResultCodeH.Check(_scanner.ResultCode);
            }
            catch (Exception ex)
            { 
                //MessageBox.Show(ex.Message);
                //throw new Exception($"Problemas al conectar con el scanner {_Scanner.ResultCode} :\n {ex.Message}.");
            }
            
        }
        void DataEvent(int Status)
        {
            string data = _scanner.ScanData;
            _scanner.DeviceEnabled = true;
            _scanner.DataEventEnabled = true;
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
                txtCodigoProd.Text = Regex.Replace(data, @"[^0-9]", "");
                SearchProducts(Regex.Replace(data, @"[^0-9]", ""));
            }
        }


        private void CloseScanner()
        {
            if (_scanner != null)
            {
                try
                {
                    //cerrar escanner
                    _scanner.DeviceEnabled = false;
                    _scanner.ReleaseDevice();
                    _scanner.Close();
                }
                catch (Exception)
                {
                    //CustomMessageBox.Show("Opos Scanner Error", ex.Message);
                }

            }
        }

    }
}
