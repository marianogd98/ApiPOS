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
using wpfPos_Rios.Views.helper;
using System.Globalization;
using wpfPos_Rios.ViewModels;
using System.ComponentModel;
using KeyPad;
using wpfPos_Rios.ViewModels.helperResponse;
using wpfPos_Rios.service.Security;
using static wpfPos_Rios.ViewModels.helperResponse.UserDataResponse;
using Newtonsoft.Json;
using Models;

namespace wpfPos_Rios.Views
{
    /// <summary>
    /// Lógica de interacción para EditDeleteProduct.xaml
    /// </summary>
    public partial class EditDeleteProduct : Window
    {
        VmProduct _Product;
        NumberFormatInfo _FormatVen = new CultureInfo("es-VE").NumberFormat;
        //modelos de seguridad
        private readonly bool _delete;



        public EditDeleteProduct(VmProduct pProduct, double pTasa , UserCajera pCajera, bool delete)
        {
            InitializeComponent();
            setFormatMoneyNumeric();
            _delete = delete;
            _Product = pProduct;
            SetValueLabels(_Product);
            btnModify.Visibility = Visibility.Hidden;


            if (pProduct.Pesado)
            {
                removeUnit.Visibility = Visibility.Collapsed;
                addUnit.Visibility = Visibility.Collapsed;
                txtbQuantity.IsEnabled = false;
                btnModify.IsEnabled = true;
                btnModify.Visibility = Visibility.Visible;
            }

        }

        public void setFormatMoneyNumeric()
        {
            _FormatVen.NumberGroupSeparator = ".";
            _FormatVen.NumberDecimalSeparator = ",";
        }
        public void SetValueLabels(VmProduct product)
        {

          //  lbCodeProduct.Content = product.Code;
            lbNameProduct.Content = product.Descripcion;
            lbUnitPrice.Content = product.UnitPriceBs.ToString("N2", _FormatVen);
            txtbQuantity.Text = product.Quantity.ToString();
            lbTotal.Content = (product.Quantity * product.UnitPriceBs).ToString("N2", _FormatVen);
        }

        private void applyEffect(Window pwindow)
        {
            var objBlur = new System.Windows.Media.Effects.BlurEffect();
            objBlur.Radius = 5;
            pwindow.Effect = objBlur;
            //App.Current.MainWindow.Effect = objBlur;
        }
        private void removeEffect(Window pwindow)
        {
            pwindow.Effect = null;

            // App.Current.MainWindow.Effect = null;
        }

        private void txtbQuantity_TextChanged(object sender, TextChangedEventArgs e)
        {
            btnConfirm.IsEnabled = true;
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (txtbQuantity.Text != _Product.Quantity.ToString())
            {
                if (txtbQuantity.Text == "")
                {
                    CustomMessageBox.Show("Información, campo vacio", "Estimado usuario debe ingresar la cantidad del producto o eliminarlo antes de confirmar la modificación");

                }
                else
                {
                    double cant = Convert.ToDouble(txtbQuantity.Text);
                    if (cant > 0)
                    {
                        _Product.Quantity = cant;
                        CustomMessageBox.Show("Modificación completada...");
                        this.Close();
                    }
                    else
                    {
                        CustomMessageBox.Show("Informacion","La cantidas debe ser mayor a cero");
                    }
                }

            }
            else
            {
                CustomMessageBox.Show("No has modificado nada...");
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_delete)
            {
                DeleteProduct();
            }
            else
            {
                //permisos de limpiar factura
                bool result = false;
                result = ValidateAction(17);
                if (result)
                {
                    DeleteProduct();
                }
                else
                {
                    this.Close();
                }

            }
        }
        private bool ValidateAction(int pidAction)
        {
            try
            {
                return RequestCredentials(pidAction);
            }
            catch (Exception ex)
            {
                return false;
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

        public void DeleteProduct()
        {
            MessageBoxResult result = CustomMessageBox.Show("Eliminar Producto", "¿Desea eliminar el producto de la factura ?", MessageBoxButton.YesNoCancel);


            switch (result)
            {
                case MessageBoxResult.Yes:
                    _Product.Quantity = 0;
                    this.Close();
                    break;
            }
        }


        #region Eventos Tactil

        private bool? showKeypad(TextBox pTextbox, Window pIndex)
        {
            KeypadNum keypad = new KeypadNum(pTextbox, pIndex);

            bool? flagKeypad = keypad.ShowDialog();

            if (flagKeypad == true)
            {
                pTextbox.Text = keypad.Result;
            }

            return flagKeypad;

        }

        private void addUnit_TouchDown(object sender, TouchEventArgs e)
        {
            int cant = Convert.ToInt32(txtbQuantity.Text);

            int Total = 0;


            Total = cant + 1;

            txtbQuantity.Text = Total.ToString();
            lbTotal.Content = (_Product.UnitPriceBs * Total).ToString("N2", _FormatVen);
        }

        private void removeUnit_TouchDown(object sender, TouchEventArgs e)
        {
            int cant = Convert.ToInt32(txtbQuantity.Text);

            int Total = 0;

            if (cant > 0)
            {
                Total = cant - 1;

                txtbQuantity.Text = Total.ToString();
                lbTotal.Content = (_Product.UnitPriceBs * Total).ToString("N2", _FormatVen);
            }
        }
        #endregion


        private void btnDelete_TouchDown(object sender, TouchEventArgs e)
        {
            MessageBoxResult result = CustomMessageBox.Show("Eliminar Producto", "¿Desea eliminar el producto de la factura ?", MessageBoxButton.YesNoCancel);


            switch (result.ToString().ToLower())
            {
                case "yes":
                    _Product.Quantity = 0;
                    this.Close();
                    break;
            }
        }



        private void txtbQuantity_TouchDown(object sender, TouchEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            bool? keypad = showKeypad(textbox, this);


            if (keypad == true)
            {
                if (textbox.Text != "")
                {
                    try 
                    {
                        txtbQuantity.Text = textbox.Text;
                        int cant = Convert.ToInt32(txtbQuantity.Text);
                        int Total = 0;
                        Total = cant;

                        txtbQuantity.Text = Total.ToString();
                        lbTotal.Content = (_Product.UnitPriceBs * Total).ToString("N2", _FormatVen);
                    }
                    catch (Exception ex)
                    {
                        CustomMessageBox.Show("Error",ex.Message);
                    }

                  
                }
            }
        }

        private void btnModify_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                applyEffect(this);
                bool result = ValidateAction(25);

                if (result)
                {
                    txtbQuantity.IsEnabled = true;
                }
                else
                {
                 //   CustomMessageBox.Show("Información", "Necesita permiso de una supervisora");
                }

            }
            catch(Exception ex)
            {
                CustomMessageBox.Show("Error",ex.ToString());
            }
            removeEffect(this);

        }


        #region Metodos de Seguridad

     
        #endregion
    }
}
