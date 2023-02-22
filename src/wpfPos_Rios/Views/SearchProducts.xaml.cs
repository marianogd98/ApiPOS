using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
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
using KeyPad;
using Models;
using Newtonsoft.Json;
using wpfPos_Rios.service;
using wpfPos_Rios.ViewModels;
using wpfPos_Rios.ViewModels.helperResponse;
using wpfPos_Rios.Views.helper;

namespace wpfPos_Rios.Views
{
    /// <summary>
    /// Lógica de interacción para SearchProducts.xaml
    /// </summary>
    public partial class SearchProducts : Window
    {

        List<Product> _ProductsList;
        ProductService _ProductModel;

        Product _Product = new Product();
        double _Tasa;
        public SearchProducts(Product pProduct, double pTasa)
        {
            InitializeComponent();
            _Product = pProduct;
            _Tasa = pTasa;
            _ProductsList = new List<Product>();
            _ProductModel = new ProductService();

        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            txtDescrip.Text = "";
            dataGridProd.ItemsSource = null;
            this.Close();
        }

        private void BtnInsert_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Product Data = (Product)dataGridProd.SelectedItem;

                if (Data == null)
                {
                    return;

                }
                else
                {
                    var messageBoxResult = CustomMessageBox.Show("Confirmar", "Desea agregar " + Data.Descripcion + "  a la lista de productos a facturar", MessageBoxButton.YesNo);

                    switch (messageBoxResult)
                    {
                        case MessageBoxResult.Yes:

                            _Product.id = Data.id;
                            _Product.CodigoProducto = Data.CodigoProducto;
                            _Product.Descripcion = Data.Descripcion;
                            _Product.PrecioDetal = Data.PrecioDetal;
                            //_Product.PrecioBolivar = Data.PrecioBolivar;
                            //_Product.PrecioDolar = Data.PrecioDolar;
                            _Product.CodigoTipo = Data.CodigoTipo;
                            _Product.Serial = Data.Serial;
                            _Product.Descuento = Data.Descuento;
                            _Product.Pesado = Data.Pesado;
                            _Product.IVA = Data.IVA;
                            _Product.CodigoMoneda = Data.CodigoMoneda;

                            this.Close();
                            break;
                    }
                }
            }

            catch (Exception ex)
            {
                CustomMessageBox.Show("Error", ex.Message);

            }
        }


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


        /// <summary>
        /// Metodo para buscar productos en la DB por Descripcion 
        /// </summary>
        /// <param name="pDescrip"></param>
        /// <param name="pCode"></param>      
        /// 
        /// //Falta Probar
        /// <returns></returns>
        private void SearchProductsDB(string pDescrip, string pCode)
        {
            //se usa par bucar si en la lista de productos se encuentra un producto
            try
            {
                // Busqueda en el modelo de la base de datos
                // var queryProduct = db.Producto.Where(p => p.Descripcion.Contains(pDescrip)).ToList();
                dataGridProd.ItemsSource = null;
                _ProductsList.Clear();

                Response response = _ProductModel.GetProductsbyParameter(pDescrip);
                List<Product> productData = JsonConvert.DeserializeObject<List<Product>>(response.data.ToString());

                if (productData != null)
                {

                    foreach (var item in productData)
                    {
                        if (item.PrecioDetal != 0)
                        {
                            Product product = new Product();
                            product.id = item.id;
                            product.CodigoProducto = item.CodigoProducto;
                            product.Barra = item.Barra;
                            product.Descripcion = item.Descripcion;
                            product.CodigoMoneda = item.CodigoMoneda;
                            product.CodigoTipo = item.CodigoTipo;
                            product.PrecioDetal = item.PrecioDetal;
                            product.PrecioDolar = (item.CodigoMoneda == "002") ? item.PrecioDetal : Math.Round((item.PrecioDetal / _Tasa),4);
                            product.PrecioBolivar = (item.CodigoMoneda == "002") ? item.PrecioDetal * _Tasa : item.PrecioDetal;
                            product.Pesado = item.Pesado;
                            product.IVA = item.IVA;
                            _ProductsList.Add(product);
                        }
                    }
                }

                dataGridProd.ItemsSource = _ProductsList;

                if (_ProductsList.Count == 0)
                {
                    CustomMessageBox.Show("No se han encontrado nigun resultado, pruebe utilizando una descripción diferente.");
                }

            }
            catch (Exception Ex)
            {
                CustomMessageBox.Show("Error de conexión",Ex.Message+Environment.NewLine+"Comuníquese con soporte técnico");
            }
        }

        #region Eventos de tactil





        private void txtDescrip_TouchDown(object sender, TouchEventArgs e)
        {

            TextBox textbox = sender as TextBox;
            textbox.Text = "";
            bool? keypad = showKeyboard(textbox, this);
            if (keypad == true)

            {
                if (txtDescrip.Text != "")
                {
                    SearchProductsDB(textbox.Text, "");
                }

            }
        }
        #endregion

        private void txtDescrip_KeyDown(object sender, KeyEventArgs e)
        {
            if (txtDescrip.Text != "")
            {
                if (e.Key == Key.Enter)
                {

                    SearchProductsDB(txtDescrip.Text, "");

                }
            }
        }

        private void dataGridProd_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //try
            //{
            //    var Data = (VmProduct)dataGridProd.SelectedItem;

            //    if (Data == null)
            //    {
            //        return;

            //    }
            //    else
            //    {
            //        var messageBoxResult = CustomMessageBox.Show("Confirmar", "Desea agregar " + Data.Descripcion + "  a la lista de productos a facturar", MessageBoxButton.YesNo);

            //        switch (messageBoxResult)
            //        {
            //            case MessageBoxResult.Yes:

            //                break;

            //            case MessageBoxResult.No:



            //                break;
            //        }
            //    }
            //}

            //catch (Exception ex)
            //{
            //    CustomMessageBox.Show("Error", ex.Message);

            //}

        }

       
    }
}
