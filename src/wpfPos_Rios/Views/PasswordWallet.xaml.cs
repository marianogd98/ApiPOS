using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
using wpfPos_Rios.ViewModels.helperResponse;
using wpfPos_Rios.Views.helper;

namespace wpfPos_Rios.Views
{
    /// <summary>
    /// Lógica de interacción para PasswordWallet.xaml
    /// </summary>
    public partial class PasswordWallet : Window
    {
        /// <summary>
        /// accion 0 crear clave a cliente
        /// accion 1 solicitar clave
        /// accion 2 actualizar clave
        /// </summary>
        readonly int _Accion;
        bool _auth;
        readonly int _idClient;
        readonly SeguridadWalletService _serviceSecurityWallet;
        public PasswordWallet(int idClient , int accion = 0)
        {
            InitializeComponent();

            _serviceSecurityWallet = new SeguridadWalletService();
            _Accion = accion;
            _auth = false;
            _idClient = idClient;
            if (accion == 1)
            {
                txtPasswordConfirm.Visibility = Visibility.Collapsed;
            }

        }

        public bool GetAuth()
        {
            return _auth;
        }

        private void CreatePaswordClient(BodySeguridadWallet data)
        {
            try
            {
                Response res = _serviceSecurityWallet.CreatePassword(data);
                authSucces.Text = res.message;
                authSucces.Visibility = Visibility.Visible;
                _auth = true;
                this.Hide();
            }
            catch (Exception ex)
            {
                authFail.Text = ex.Message;
                authFail.Visibility = Visibility.Visible;
                //CustomMessageBox.Show("Error creando contraseña del usuario" , ex.Message);
            }
        }

        private void AuthenticationClient(BodySeguridadWallet data)
        {
            try
            {
                Response res = _serviceSecurityWallet.authenticationClient(data);
                if ( Convert.ToInt32(res.data) == 1)
                {
                    _auth = true;
                    authSucces.Text = "Autenticacion correcta";
                    authSucces.Visibility = Visibility.Visible;
                    this.Hide();
                }
                else
                {
                    authFail.Text = "Contraseña incorrecta";
                    authFail.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                authFail.Text = ex.Message;
                authFail.Visibility = Visibility.Visible;
                //CustomMessageBox.Show("Error en autenticacion", ex.Message);
            }
        }

        private void UpdatePassword(BodySeguridadWallet data)
        {
            try
            {
                Response res = _serviceSecurityWallet.UpdatePassword(data);
                if (Convert.ToInt32(res.data) == 1)
                {
                    _auth = true;
                    authSucces.Text = res.message;
                    authSucces.Visibility = Visibility.Visible;
                    this.Hide();
                }
            }
            catch (Exception ex)
            {
                //CustomMessageBox.Show("Error en autenticacion", ex.Message);
                authFail.Text = ex.Message;
                authFail.Visibility = Visibility.Visible;
            }
        }

        private void InsertUpdateLoging()
        {
            switch (_Accion)
            {
                case 0:
                    if (string.IsNullOrEmpty(txtPassword.Password))
                    {
                        authFail.Text = "El cliente debe ingresar la contraseña";
                        authFail.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(txtPasswordConfirm.Password))
                        {
                             authFail.Text = "El cliente debe repetir la contraseña, para confirmar!";
                             authFail.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            if (txtPasswordConfirm.Password != txtPassword.Password)
                            {
                                authFail.Text = "Las contraseñas no coninciden!";
                                authFail.Visibility = Visibility.Visible;
                                
                                txtPassword.Password = "";
                                txtPasswordConfirm.Password = "";
                            }
                            else
                            {
                                if ( (txtPassword.Password.Length < 4) || (txtPassword.Password.Length > 6))
                                {
                                    authFail.Text = "La contraseña debe tener como minimo 4 digitos y 6 como maximo!";
                                    authFail.Visibility = Visibility.Visible;

                                    txtPassword.Password = "";
                                    txtPasswordConfirm.Password = "";
                                }
                                else
                                {
                                    if (!Regex.IsMatch(txtPassword.Password, "([0-9])+"))
                                    {
                                        authFail.Text = "La contraseña debe ser numerica!!";
                                        authFail.Visibility = Visibility.Visible;

                                        txtPassword.Password = "";
                                        txtPasswordConfirm.Password = "";
                                    }
                                    else {

                                        BodySeguridadWallet data = new BodySeguridadWallet() { IdClient = _idClient, Password = txtPassword.Password };
                                        CreatePaswordClient(data);
                                       
                                    }
                                }
                            }
                        }
                    }
                    break;
                
                case 1:
                    if (string.IsNullOrEmpty(txtPassword.Password))
                    {
                        authFail.Text = "El cliente debe ingresar la contraseña";
                        authFail.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        BodySeguridadWallet data = new BodySeguridadWallet() { IdClient = _idClient, Password = txtPassword.Password };
                        AuthenticationClient(data);
                    }
                    break;

                case 2:
                    if (string.IsNullOrEmpty(txtPassword.Password))
                    {
                        authFail.Text = "El cliente debe ingresar la contraseña";
                        authFail.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(txtPasswordConfirm.Password))
                        {
                            authFail.Text = "El cliente debe repetir la contraseña, para confirmar!";
                            authFail.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            if (txtPasswordConfirm.Password != txtPassword.Password)
                            {
                                authFail.Text = "Las contraseñas no coninciden!";
                                authFail.Visibility = Visibility.Visible;

                                txtPassword.Password = "";
                                txtPasswordConfirm.Password = "";
                            }
                            else
                            {
                                if ((txtPassword.Password.Length < 4) || (txtPassword.Password.Length > 6))
                                {
                                    authFail.Text = "La contraseña debe tener como minimo 4 digitos y 6 como maximo!";
                                    authFail.Visibility = Visibility.Visible;

                                    txtPassword.Password = "";
                                    txtPasswordConfirm.Password = "";
                                }
                                else
                                {
                                    if (!Regex.IsMatch(txtPassword.Password, "([0-9])+"))
                                    {
                                        authFail.Text = "La contraseña debe ser numerica!!";
                                        authFail.Visibility = Visibility.Visible;

                                        txtPassword.Password = "";
                                        txtPasswordConfirm.Password = "";
                                    }
                                    else
                                    {
                                        //ejecutar sp para crear password
                                        BodySeguridadWallet data = new BodySeguridadWallet() { IdClient = _idClient, Password = txtPassword.Password };
                                        UpdatePassword(data);
                                    }
                                }
                            }
                        }
                    }
                    break;
            }
        }

        

        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            if (sender == BtnCancel)
            {
                _auth = false;
                this.Hide();
            }
            else if (sender == BtnSucces)
                InsertUpdateLoging();
        }

    }
}
