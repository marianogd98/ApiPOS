using KeyPad;
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
using wpfPos_Rios.ViewModels;

namespace wpfPos_Rios.Views.helper
{
    /// <summary>
    /// Lógica de interacción para cantidadDevParcial.xaml
    /// </summary>
    public partial class cantidadDevParcial : Window
    {
        VmDetalleFacturaReturn _product;
        public cantidadDevParcial(VmDetalleFacturaReturn product)
        {
            InitializeComponent();
            _product = product;
            txtProduct.Text = $"{ Truncate(_product.descripcion , 20) } Cantidad: {_product.cantidad}";
        }
        public string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;

            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
        private void btnReturnAll_Click(object sender, RoutedEventArgs e)
        {
           
            _product.cantidadDevolver = _product.cantidad;
            this.Close();
            
        }
        private void btnReturn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtbCantidadDevolver.Text))
                {
                    CustomMessageBox.Show("informacion" , "Debe ingresar la cantidad a devolver");
                    return;
                }
                double cantidadDevolver = Convert.ToDouble(txtbCantidadDevolver.Text);
                if (cantidadDevolver > _product.cantidad)
                {
                    CustomMessageBox.Show("informacion", $"No puede devolver mas de {_product.cantidad} unidades del producto {_product.descripcion}");
                    return;
                }

                
                _product.cantidadDevolver = cantidadDevolver;
                this.Close();
                
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("error", ex.Message);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var messageBoxResult = CustomMessageBox.Show("Confirmar", "desea cancelar la operacion?", MessageBoxButton.YesNo, CustomMessageBox.MessageBoxImage.Question);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("error" , ex.Message);
            }
        }

        private void txtbCantidadDevolver_TouchDown(object sender, TouchEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            showKeypad(textbox, this);
        }

        private void showKeypad(TextBox pTextbox, Window pIndex)
        {
            KeypadNum keypad = new KeypadNum(pTextbox, pIndex);
            if (keypad.ShowDialog() == true)
                pTextbox.Text = keypad.Result;
        }

    }
}
