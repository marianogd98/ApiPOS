using System;
using System.Collections.Generic;
using System.Threading;
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
using wpfPos_Rios.Views;
using wpfPos_Rios.Views.helper;
using System.Diagnostics;
using wpfPos_Rios.ViewModels;
using wpfPos_Rios.service;
using Models;
using Newtonsoft.Json;
using wpfPos_Rios.ViewModels.helperResponse;
using System.Text.RegularExpressions;

namespace wpfPos_Rios.Views
{
    /// <summary>
    /// Interaction logic for ClientData.xaml
    /// </summary>
    public partial class ClientData : Window
    {
        VmClient _clientModel = new VmClient();
        ClientService _ClientService = new ClientService();
        Caja _Caja = new Caja();


        //Reglas de expresiones regulares
        Regex reglas = new Regex(@"[0-9a-zA-Z -&]+");
        Regex reglasNum = new Regex(@"^[0-9]+$");

        public ClientData(VmClient clientModel, Caja pCaja)
        {

            InitializeComponent();

            _Caja = pCaja;

            _clientModel = clientModel;

            cmbRif.Items.Add("V");
            cmbRif.Items.Add("J");
            cmbRif.Items.Add("E");
            cmbRif.Items.Add("G");

            if (_clientModel.Rif != "" && _clientModel.Rif != null)
            {
                _clientModel.Id = clientModel.Id;
                txtCedulaClient.Text = _clientModel.Rif.Substring(1, _clientModel.Rif.Length - 1);
                cmbRif.Text = _clientModel.Rif.Substring(0, 1);
                txtNombreClient.Text = _clientModel.Name;
                txtApellidoClient.Text = _clientModel.LastName;
                txtDireClient.Text = _clientModel.Address;
                txtTlfClient.Text = _clientModel.Cellnumber;
            }


        }

        #region //Teclado virtual  metodos
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
        #endregion




        #region//Metodos del Cliente


        /// <summary>
        /// Metodo para registrar o actualizar un cliente
        /// </summary>
        private string InsertUpdateClient()
        {
            string message;
            try
            {
                //Comienzo de estructura en capas para el registro de Clientes
                _clientModel.resetData();
                _clientModel.Name = txtNombreClient.Text;
                _clientModel.LastName = txtApellidoClient.Text;
                _clientModel.Rif = cmbRif.Text + txtCedulaClient.Text;
                _clientModel.Cellnumber = txtTlfClient.Text;
                _clientModel.Address = txtDireClient.Text;
                _clientModel.Email = "";

                Response result = _ClientService.GuardarCambios(new Client { Nombre = _clientModel.Name, Apellido = _clientModel.LastName, RIF = _clientModel.Rif, Telefono = _clientModel.Cellnumber, Direccion = _clientModel.Address, email = _clientModel.Email });

                if (result.success == 1)
                {
                    message = "Cliente ingresado exitosamente...";
                    ClientTasaResponse.ClientTasa data = JsonConvert.DeserializeObject<ClientTasaResponse.ClientTasa>(result.data.ToString());
                    _clientModel.Id = data.cliente.id;
                    _clientModel.Rif = cmbRif.Text + txtCedulaClient.Text;
                    _clientModel.Saldo =Math.Round(data.cliente.saldo,2);
                    _clientModel.Name = txtNombreClient.Text;
                    _clientModel.LastName = txtApellidoClient.Text;
                    _clientModel.Cellnumber = txtTlfClient.Text;
                    _clientModel.Address = txtDireClient.Text;
                }
                else
                {
                    message = result.message;
                }
            }
            catch (Exception ex)
            {
                message = ex.ToString();
            }
            return message;
        }

        /// <summary>
        /// Metodos para Buscar el cliente por el rif o cedula
        /// </summary>
        /// <param name="pRifCliente"></param>
        /// <returns></returns>
        private bool GetClient(string pRifCliente)
        {
            bool flag = false;

            try
            {
                // Busqueda en el modelo de la base de datos

                var response = _ClientService.GetClient(pRifCliente);

                if (response.success == 1)
                {


                    if (response.data == null)
                    {

                                txtNombreClient.Focus();

                                txtNombreClient.Focus();
                                txtDireClient.Text = "";
                                txtTlfClient.Text = "";
                                txtApellidoClient.Text = "";
                                txtNombreClient.Text = "";
                                txtDireClient.IsEnabled = true;
                                txtNombreClient.IsEnabled = true;
                                txtCedulaClient.IsEnabled = true;
                                if ((cmbRif.SelectedIndex==1 || cmbRif.SelectedIndex==3))
                                {
                                    txtApellidoClient.IsEnabled = false;
                                }
                                else
                                {
                                    txtApellidoClient.IsEnabled = true;
                                }
                                txtTlfClient.IsEnabled = true;
                                
                                //Limpiar el modelo
                                _clientModel.Name = "";
                                _clientModel.LastName = "";
                                _clientModel.Rif = "";
                                _clientModel.Cellnumber = "";
                                _clientModel.Id = 0;
                                _clientModel.Saldo = 0;
                    
                    }
                    else
                    {
                        ClientTasaResponse.ClientTasa data = JsonConvert.DeserializeObject<ClientTasaResponse.ClientTasa>(response.data.ToString());

                        // txtCedulaClient.Text = queryClient.Rif;
                        txtNombreClient.Text = data.cliente.nombre; // + " " + queryClient.Apellido;
                        txtTlfClient.Text = data.cliente.telefono;
                        txtDireClient.Text = data.cliente.direccion;
                        txtApellidoClient.Text = data.cliente.apellido;
                        if ((cmbRif.SelectedIndex == 1 || cmbRif.SelectedIndex == 3))
                        {
                            txtApellidoClient.IsEnabled = false;
                        }
                        else
                        {
                            txtApellidoClient.IsEnabled = true;
                        }
                        //queryClient.Email;

                        //Se llena el modelo con los datos traidos de la consulta al Api
                        _clientModel.Id = data.cliente.id;
                        _clientModel.Rif = data.cliente.rif;
                        _clientModel.Name = data.cliente.nombre;
                        _clientModel.LastName = data.cliente.apellido;
                        _clientModel.Address = data.cliente.direccion;
                        _clientModel.Cellnumber = data.cliente.telefono;
                        _clientModel.Email = data.cliente.email;
                        _clientModel.Saldo = Math.Round( data.cliente.saldo , 2);

                        _Caja.Tasa = data.tasa.monto;

                        flag = true;
                    }

                }

            }
            catch (Exception Ex)
            {
                CustomMessageBox.Show("Información", Environment.NewLine + Ex.Message + " Comuniquese con soporte técnico");
                flag = false;
            }

            return flag;

        }
        #endregion

        #region // Eventos del los Textbox por Teclado Fisico


        private void txtCedulaClient_PreviewKeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Enter)
            {
                if (cmbRif.Text != "")
                {
                    if (txtCedulaClient.Text != "" && reglasNum.IsMatch(txtCedulaClient.Text))
                    {
                        string clienteDatos = cmbRif.Text + txtCedulaClient.Text;
                        bool client = GetClient(clienteDatos); 

                        if (client)
                        {
                            txtTlfClient.IsEnabled = true;
                            txtDireClient.IsEnabled = true;
                            txtNombreClient.IsEnabled = true;
                            if (cmbRif.Text.ToLower() == "j" || cmbRif.Text.ToLower() == "g")
                            {
                                txtApellidoClient.IsEnabled = false;
                            }
                            else
                            {
                                txtApellidoClient.IsEnabled = true;
                            }

                        }

                    }
                    else
                    {
                        txtCedulaClient.Focus();
                        CustomMessageBox.Show("informacion", "Debe ingresar el numero  de documento (Solo se aceptan carácteres numéricos)");
                    }
                }
                else
                {
                    cmbRif.Focus();
                    CustomMessageBox.Show("informacion", "Debe seleccionar el tipo de documento");
                }

            }
        }

        #endregion

        #region //Validaciones de los controles
        private bool ValidateData()
        {
            bool flag = true;
            if ((txtCedulaClient.Text == "") || (txtCedulaClient.Text.Length < 7) && !reglasNum.IsMatch(txtCedulaClient.Text))
            {
                CustomMessageBox.Show("Cedula o Rif en formato no valido.", "Estimado Usuario debe ingresar mas de 7 carácteres numéricos");
                flag = false;

            }
            else
            {

                if ((txtNombreClient.Text == "") || (txtNombreClient.Text.Length < 3) || !reglas.IsMatch(txtNombreClient.Text))
                {
                    CustomMessageBox.Show("Formato no valido en el campo Nombre", "Estimado Usuario debe ingresar mas de 3 carácteres al campo Nombre del cliente y no se admiten carácteres especiales");
                    flag = false;
                }
                else
                {
                    if (cmbRif.Text.ToLower() == "e" || cmbRif.Text.ToLower() == "v")
                    {
                        if ((txtApellidoClient.Text == "") || (txtApellidoClient.Text.Length < 3) || !reglas.IsMatch(txtApellidoClient.Text))
                        {
                            CustomMessageBox.Show("Apellido en formato no valido.", "Estimado Usuario debe ingresar mas de 3 carácteres al campo Apellido del cliente y no se admiten carácteres especiales");
                            flag = false;
                        }
                    }

                    if((cmbRif.Text.ToLower() == "j" || cmbRif.Text.ToLower() == "g") && (txtApellidoClient.Text.Length > 0 || txtApellidoClient.Text != ""))
                    {    
                        CustomMessageBox.Show("informacion", "Los entes gubernamentales y/o juridicos no llevan apellido.");
                        flag = false;   
                    }

                    if (txtDireClient.Text == "" && txtDireClient.Text.Length<3)
                    {
                        CustomMessageBox.Show("Dirección en formato no valido.", "Estimado Usuario debe ingresar más de 3 carácteres");
                        flag = false;

                    }
                    else
                    {
                        if ((txtTlfClient.Text == "") || (txtTlfClient.Text.Length < 11) || (txtTlfClient.Text.Length > 12) || !reglasNum.IsMatch(txtTlfClient.Text))
                        {
                            CustomMessageBox.Show("Estimado Usuario esta ingreando un formato de teléfono no válido", "Teléfono de cliente en formato no válido.");
                            flag = false;
                        }
                    }
                }
            }

            return flag;
        }

        private void CleanAll()
        {
            txtCedulaClient.Text = "";
            txtDireClient.Text = "";
            txtTlfClient.Text = "";
            txtApellidoClient.Text = "";
            txtNombreClient.Text = "";

            txtTlfClient.IsEnabled = false;
            txtDireClient.IsEnabled = false;
            txtNombreClient.IsEnabled = false;
            txtApellidoClient.IsEnabled = false;
            txtCedulaClient.IsEnabled = true;

            //Limpiar el modelo
            _clientModel.Name = "";
            _clientModel.LastName = "";
            _clientModel.Rif = "";
            _clientModel.Cellnumber = "";
            _clientModel.Id = 0;
            _clientModel.Saldo = 0;
        }


        private void btnCloseWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Close();

        }

        #endregion

        #region// Botones
        private void btnAcept_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateData())
            {

                //Si ocurre una modificacion en un campo que lo actualice.
                if ((txtNombreClient.Text != _clientModel.Name) || (txtDireClient.Text != _clientModel.Address) || (txtApellidoClient.Text != _clientModel.LastName) || (txtTlfClient.Text != _clientModel.Cellnumber) || ((cmbRif.Text + txtCedulaClient.Text) != _clientModel.Rif))
                {
                    string result = InsertUpdateClient();

                    if (result == "Cliente ingresado exitosamente...")
                    {
                        this.Close();

                    }
                    else
                    {
                        CustomMessageBox.Show("Error al Ingresar o actualizar el cliente", result);
                    }



                }
                else
                {
                    this.Close();

                }

            }

        }

        private void btnClean_Click(object sender, RoutedEventArgs e)
        {
            CleanAll();
        }

        #endregion

        #region Eventos del Tactil

        private void txtCedulaClient_TouchDown(object sender, TouchEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            bool? keypad = showKeypad(textbox, this);


            if (keypad == true)
            {

                if (cmbRif.Text != "")
                {
                    if (txtCedulaClient.Text != "" && reglasNum.IsMatch(txtCedulaClient.Text))
                    {
                        string clienteDatos = cmbRif.Text + txtCedulaClient.Text;
                        bool client = GetClient(clienteDatos);
                        // MessageBox.Show(mensaje);

                        if (client)
                        {
                            txtTlfClient.IsEnabled = true;
                            txtDireClient.IsEnabled = true;
                            txtNombreClient.IsEnabled = true;
                            if (cmbRif.Text.ToLower() == "j" || cmbRif.Text.ToLower() == "g")
                            {
                                txtApellidoClient.IsEnabled = false;
                            }
                            else
                            {
                                txtApellidoClient.IsEnabled = true;
                            }
                        }

                    }
                    else
                    {
                        txtCedulaClient.Focus();
                        CustomMessageBox.Show("informacion", "Debe ingresar el numero  de documento (Solo se aceptan carácteres numéricos)");
                    }
                }
                else
                {
                    cmbRif.Focus();
                    CustomMessageBox.Show("informacion", "Debe seleccionar el tipo de documento");
                }
            }

        }

        private void txtNombreClient_TouchDown(object sender, TouchEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            showKeyboard(textbox, this);
        }

        private void txtApellidoClient_TouchDown(object sender, TouchEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            showKeyboard(textbox, this);
        }

        private void txtTlfClient_TouchDown(object sender, TouchEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            showKeypad(textbox, this);
        }

        private void txtDireClient_TouchDown(object sender, TouchEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            showKeyboard(textbox, this);
        }

        private void btnCloseWindow_TouchDown(object sender, TouchEventArgs e)
        {
            this.Close();

        }


        #endregion

        private void btnClean_TouchDown(object sender, TouchEventArgs e)
        {
            CleanAll();

        }

        private void cmbRif_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

             CleanAll();

      
        }

     
    }
}
