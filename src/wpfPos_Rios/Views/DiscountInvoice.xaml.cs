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
using KeyPad;
using wpfPos_Rios.ViewModels;
using wpfPos_Rios.Views.helper;
using wpfPos_Rios.service.Security;
using System.Globalization;

namespace wpfPos_Rios.Views
{
    /// <summary>
    /// Lógica de interacción para DiscountInvoice.xaml
    /// </summary>
    public partial class DiscountInvoice : Window
    {
        //Acceder a los controles de la Ventana Principal(index)
        //Index _Form = Application.Current.Windows[0] as Index;

        VmInvoice _Invoice = new VmInvoice();
        NumberFormatInfo _FormatVen = new CultureInfo("es-VE").NumberFormat;
        public DiscountInvoice(VmInvoice pInvoice)
        {
            InitializeComponent();
            _FormatVen.NumberDecimalSeparator = ",";
            _FormatVen.NumberGroupSeparator = ".";

            _Invoice = pInvoice;
            lbMountInvoice.Content = _Invoice.SubTotal.ToString("N2" , _FormatVen);
        }
      
        private void chkbDiscount_Click(object sender, RoutedEventArgs e)
        {
            string check = ((CheckBox)sender).Name;

            switch (check)
            {
                case "chkbPercentageDiscount":

                    chkbDiscoutMount.IsChecked = false;
                    chkbDeleteDiscount.IsChecked = false;
                    disabledCheckBoxMountDiscount();
                    switch (chkbPercentageDiscount.IsChecked)
                    {
                        case true:
                            enabledCheckBoxPorcentageDiscount();
                            break;
                        case false:
                            disabledCheckBoxPorcentageDiscount();
                            break;
                    }
                    break;
                case "chkbDiscoutMount":

                    chkbPercentageDiscount.IsChecked = false;
                    chkbDeleteDiscount.IsChecked = false;
                    disabledCheckBoxPorcentageDiscount();

                    switch (chkbDiscoutMount.IsChecked)
                    {
                        case true:
                            enabledCheckBoxMountDiscount();
                            break;
                        case false:
                            disabledCheckBoxMountDiscount();
                            break;

                    }
                    break;
                case "chkbDeleteDiscount":
                    
                    disabledCheckBoxMountDiscount();
                    disabledCheckBoxPorcentageDiscount();
                    chkbPercentageDiscount.IsChecked = false;
                    chkbDiscoutMount.IsChecked = false;
                    break;

            }

        }

        public void disabledCheckBoxPorcentageDiscount()
        {

            rbFiveDiscount.IsEnabled = false;
            rbTenDiscount.IsEnabled = false;
            rbTwentyDiscount.IsEnabled = false;
            rbOtherDiscount.IsEnabled = false;
            tbOtherDiscount.IsEnabled = false;

            rbFiveDiscount.IsChecked = false;
            rbTenDiscount.IsChecked = false;
            rbTwentyDiscount.IsChecked = false;
            rbOtherDiscount.IsChecked = false;
            tbOtherDiscount.Text = "";
        }
        public void enabledCheckBoxPorcentageDiscount()
        {

            rbFiveDiscount.IsEnabled = true;
            rbTenDiscount.IsEnabled = true;
            rbTwentyDiscount.IsEnabled = true;
            rbOtherDiscount.IsEnabled = true;
        }
        public void disabledCheckBoxMountDiscount()
        {

            txtbMountDiscount.IsEnabled = false;
            cbMountDiscount.IsEnabled = false;

            txtbMountDiscount.Text = string.Empty;
            cbMountDiscount.SelectedIndex = 0;

        }
        public void enabledCheckBoxMountDiscount()
        {


            txtbMountDiscount.IsEnabled = true;
            cbMountDiscount.IsEnabled = true;

        }

        private void rbOtherDiscount_Click(object sender, RoutedEventArgs e)
        {
            if (rbOtherDiscount.IsChecked == true)
            {
                tbOtherDiscount.IsEnabled = true;
            }
        }

        private void rbDiscount_Click(object sender, RoutedEventArgs e)
        {
            if (tbOtherDiscount.IsEnabled == true)
            {
                tbOtherDiscount.IsEnabled = false;
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {

            if (chkbPercentageDiscount.IsChecked == true || chkbDiscoutMount.IsChecked == true)
            {
                var messageBoxResult = CustomMessageBox.Show("Salir", "¿Desea regresar sin aplicar los cambios?", MessageBoxButton.YesNoCancel, CustomMessageBox.MessageBoxImage.Question);

                switch (messageBoxResult)
                {
                    case MessageBoxResult.Yes:
                        this.Close();
                        break;
                }
            }
            else
            {
                this.Close();
            }


        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _Invoice.Discount = 0;
            double discount = 0;
            if (chkbPercentageDiscount.IsChecked == true)
            {
                if (rbFiveDiscount.IsChecked == true)
                {
                    discount = 0.05;
                }

                if (rbTenDiscount.IsChecked == true)
                {
                    discount = 0.10;
                }

                if (rbTwentyDiscount.IsChecked == true)
                {
                    discount = 0.20;
                }
               
                if (rbOtherDiscount.IsChecked == true  )
                {
                    try
                    {
                        if(tbOtherDiscount.Text != "")
                        {
                            discount = Convert.ToDouble(tbOtherDiscount.Text) / 100;
                            discount = Math.Round(discount, 4);
                        }
                        else
                        {
                            CustomMessageBox.Show("Informacion", "Debe ingresar el procentaje de descuento", MessageBoxButton.OK ,CustomMessageBox.MessageBoxImage.Information);
                        }
                    }
                    catch (Exception)
                    {
                            CustomMessageBox.Show("Informacion", "Debe asignar un formato de descuento correcto", MessageBoxButton.OK ,CustomMessageBox.MessageBoxImage.Information);
                    }
                }

                //comprobar descuento maximo
                if (discount < 0 || discount > 1)
                {
                    CustomMessageBox.Show("Informacion", "No puede dar un descuento menor a 0 o mayor al 100 %" , MessageBoxButton.YesNoCancel, CustomMessageBox.MessageBoxImage.Question);
                }
                else
                {
                    if (discount > 0)
                    {
                        var messageBoxResult = CustomMessageBox.Show("Confirmar Descuento", "¿Seguro desea  aplicar un descuento de" + " " + (discount * 100).ToString() + "% " + "a la factura?", MessageBoxButton.YesNoCancel, CustomMessageBox.MessageBoxImage.Question);
                        switch (messageBoxResult)
                        {
                            case MessageBoxResult.Yes:

                                SetDiscount(discount);

                                break;
                        }
                    }
                }
            }
            else
            {
                if (chkbDiscoutMount.IsChecked == true)
                {
                    try
                    {
                        if (txtbMountDiscount.Text != "")
                        {
                            double amount = Math.Round( _Invoice.Total - Convert.ToDouble(txtbMountDiscount.Text) , 2);

                            discount = 1 - (amount / _Invoice.Total);
                            discount = Math.Round(discount, 4);

                            if (discount < 0)
                            {
                                CustomMessageBox.Show("Informacion", "No asigno descuento a la factura", MessageBoxButton.OK, CustomMessageBox.MessageBoxImage.Question);
                            }
                            else
                            {
                                if (discount == 1)
                                {
                                    CustomMessageBox.Show("Informacion", "El monto a pagar no puede ser igual a cero(0) , no puede aplicar el 100% de descuento.", MessageBoxButton.OK, CustomMessageBox.MessageBoxImage.Question);
                                }
                                else
                                {
                                   
                                    var messageBoxResult = CustomMessageBox.Show("Confirmar Descuento", "¿Seguro desea  aplicar un descuento de" + " " + (discount * 100).ToString() + "% " + "a la factura?", MessageBoxButton.YesNoCancel, CustomMessageBox.MessageBoxImage.Question);
                                    switch (messageBoxResult)
                                    {
                                        case MessageBoxResult.Yes:

                                            SetDiscount(discount);

                                            break;
                                    }
                                    
                                }
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        CustomMessageBox.Show("error asignando descuento", ex.Message, MessageBoxButton.OK, CustomMessageBox.MessageBoxImage.Question);
                    }
                }else
                {

                    if (chkbDeleteDiscount.IsChecked == true)
                    {
                        discount = 0;

                        var messageBoxResult = CustomMessageBox.Show("Confirmar Descuento", "¿Seguro desea  quitar el descuento?", MessageBoxButton.YesNoCancel, CustomMessageBox.MessageBoxImage.Question);

                        switch (messageBoxResult)
                        {
                            case MessageBoxResult.Yes:
                                
                                SetDiscount(discount);
                                
                                break;
                        }
                    }
                    else
                    {
                        CustomMessageBox.Show("Informacion", "No selecciono ningun tipo de descuento", MessageBoxButton.OK, CustomMessageBox.MessageBoxImage.Information);
                    }


                }
                
            }

        }

        private void SetDiscount(double discount)
        {
            _Invoice.Discount = discount;

            //quitar descuento a todos los productos
            _Invoice.ProductList.ForEach(p=> p.DiscountPercentage = 0);

            this.Close();

        }

        private void showKeyboard(TextBox pTextbox, Window pIndex)
        {
            VirtualKeyboard keyboardwindow = new VirtualKeyboard(pTextbox, pIndex);
            if (keyboardwindow.ShowDialog() == true)
                pTextbox.Text = keyboardwindow.Result;
        }
        private void showKeypad(TextBox pTextbox, Window pIndex)
        {
            KeypadNum keypad = new KeypadNum(pTextbox, pIndex);
            if (keypad.ShowDialog() == true)
                pTextbox.Text = keypad.Result;
        }

 
        private void txtbMountDiscount_TouchDown(object sender, TouchEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            showKeypad(textbox, this);
        }

        private void tbOtherDiscount_TouchDown(object sender, TouchEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            showKeypad(textbox, this);
        }

     
    }
}
