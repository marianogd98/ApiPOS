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
using System.Windows.Navigation;
using System.Windows.Shapes;
using wpfPos_Rios.ViewModels;
using wpfPos_Rios.service;
using wpfPos_Rios.Views.helper;
using Models;
using Newtonsoft.Json;
using KeyPad;
using wpfPos_Rios.ViewModels.helperResponse;
using System.Configuration;

namespace wpfPos_Rios.Views
{
    /// <summary>
    /// Lógica de interacción para QuickMenuProduct.xaml
    /// </summary>
    public partial class QuickMenuProduct : Window
    {
        List<Product> _ListProducts;
        ProductService _ProductService = new ProductService();
        double _Tasa;
        int _Area = 0;
        Product _Product = new Product();
        public QuickMenuProduct(Product pProduct , double pTasa , int pArea)
        {
            InitializeComponent();
            _Tasa = pTasa;
            _Product = pProduct;
            _Area = pArea;
            DisplayProducts();
        }

        public void SetTasa(double pTasa)
        {
            _Tasa = pTasa;
        }

        private void DisplayProducts()
        {
            //pasar area como parametro
            GetProducts(_Area);
            if (_ListProducts != null)
            {
                ListViewProducts.ItemsSource = _ListProducts;
            }

        }

        public string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;

            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        //Metodo que obtiene los productos de una lista dinamica para mostrarlo en la pagina del teclado rapido
        private void GetProducts(int codArea)
        {

            try
            {
                Response response = _ProductService.GetProductsArea(codArea);

                if (response.success == 1 && response.data != null)
                {
                    List<Product> productData = JsonConvert.DeserializeObject<List<Product>>(response.data.ToString());

                    _ListProducts = new List<Product>();

                    foreach (var item in productData)
                    {
                        if (item.Image != "default.png")
                        {
                            Product product = new Product();
                            product.id = item.id;
                            product.CodigoProducto = item.CodigoProducto;
                            product.CodigoTipo = item.CodigoTipo;
                            product.Barra = item.Barra;
                            product.Descripcion = Truncate(item.Descripcion, 25); ;
                            product.CodigoMoneda = item.CodigoMoneda;
                            product.PrecioDetal = item.PrecioDetal;
                            
                            product.PrecioDolar = (item.CodigoMoneda == "002") ? item.PrecioDetal : Round((item.PrecioDetal / _Tasa));
                            product.PrecioBolivar = (item.CodigoMoneda == "002") ? Math.Round( item.PrecioDetal * _Tasa , 2) : item.PrecioDetal;
                            
                            product.Pesado = item.Pesado;
                            product.IVA = item.IVA;
                            product.Image = $"{ConfigPosStatic.TiendaConfig.uriAreaImage}{item.Image}";
                            _ListProducts.Add(product);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error cargar productos", ex.ToString());
            }
        }

        public double Round(double value)
        {
            return Math.Round((value * 100) - (Math.Round(value * 100, 0)), 2) == 0 ? value : Math.Ceiling(100 * value) / 100;
        }

        //Metodo que filtra de la lista los productos por su nombre
        private void TxtSearchProducts_TextChanged(object sender, TextChangedEventArgs e)
        {
            ListViewProducts.ItemsSource = SearchProducts(TxtSearchProducts.Text);
        }


        private List<Product> SearchProducts(string filter)
        {
            return _ListProducts.FindAll(e => e.Descripcion.ToLower().Contains(filter.ToLower()));
        }
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            _Product.id = -1;
            _Product.CodigoProducto = string.Empty;
            _Product.Descripcion = string.Empty;
            _Product.PrecioDetal = 0;
            //_Product.CodigoTipo = 0;
            _Product.Serial = string.Empty;
            _Product.Descuento = 0;
            _Product.Pesado = false;
            _Product.IVA = 0;
            _Product.CodigoMoneda = string.Empty;
            TxtSearchProducts.Text = "";


            this.Close();
            TxtSearchProducts.Text = "";
            SearchProducts("");
        }
        private void BtnImage_Click(object sender, RoutedEventArgs e)
        {
            Button datacontext = sender as Button;
            Product product = datacontext.DataContext as Product;

            var messageBoxResult = CustomMessageBox.Show("Confirmar", "¿Deseas agregar el producto" + " " + product.Descripcion + " " + "?", MessageBoxButton.YesNoCancel);

            switch (messageBoxResult)
            {
                case MessageBoxResult.Yes:
                    _Product.id = product.id;
                    _Product.CodigoProducto = product.CodigoProducto;
                    _Product.Descripcion = product.Descripcion;
                    _Product.PrecioDetal = product.PrecioDetal;
                    _Product.CodigoTipo = product.CodigoTipo;
                    _Product.Serial = product.Serial;
                    _Product.Descuento = product.Descuento;
                    _Product.Pesado = product.Pesado;
                    _Product.IVA = product.IVA;
                    _Product.CodigoMoneda = product.CodigoMoneda;
                    TxtSearchProducts.Text = "";
                    SearchProducts("");
                    this.Hide();
                    break;
            }
        }

        #region Eventos del tactil

        private bool? showKeyboard(TextBox pTextbox, Window pIndex)
        {
            VirtualKeyboard keyboardwindow = new VirtualKeyboard(pTextbox, pIndex);

            bool? flagKeyboard = keyboardwindow.ShowDialog();

            if (flagKeyboard == true)
            {
                pTextbox.Text = keyboardwindow.Result;
            }

            return flagKeyboard;
        }

        private void TxtSearchProducts_TouchDown(object sender, TouchEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            textbox.Text = "";
            bool? keypad = showKeyboard(textbox, this);
            if (keypad == true)
            {
                ListViewProducts.ItemsSource = SearchProducts(TxtSearchProducts.Text);
            }
        }
        #endregion

        private void TxtSearchProducts_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ListViewProducts.ItemsSource = SearchProducts(TxtSearchProducts.Text);

            }
        }

       
    }
}
