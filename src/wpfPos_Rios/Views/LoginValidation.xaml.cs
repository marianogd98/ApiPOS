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
using wpfPos_Rios.service.Security;
using wpfPos_Rios.Views.helper;
using wpfPos_Rios.ViewModels.helperResponse;
using wpfPos_Rios.ViewModels;
using KeyPad;
using Models;
using Newtonsoft.Json;

namespace wpfPos_Rios.Views
{
    /// <summary>
    /// Interaction logic for LoginValidation.xaml
    /// </summary>
    public partial class LoginValidation : Window
    {

        private SecurityService _UserServiceApi = new SecurityService();
        private string _Action;

        public UserActionResponse userActions;

        public LoginValidation(string pAction)
        {
            InitializeComponent();
            _Action = pAction;
        }

        #region Teclado
        private void showKeyboard(TextBox pTextbox, Window pIndex)
        {
            VirtualKeyboard keyboardwindow = new VirtualKeyboard(pTextbox, pIndex);
            if (keyboardwindow.ShowDialog() == true)
                pTextbox.Text = keyboardwindow.Result;
        }

        #endregion


        private void txtPasscode_TouchDown(object sender, TouchEventArgs e)
        {
            try
            {

                TextBox textbox = new TextBox();
                PasswordBox box = new PasswordBox();

                textbox.Text = box.Password;

                VirtualKeyboardPass keyboardwindow = new VirtualKeyboardPass(textbox, this);
                if (keyboardwindow.ShowDialog() == true)
                    txtPasscode.Password = keyboardwindow.Result;

            }
            catch (Exception ex)

            {
                CustomMessageBox.Show("Error", ex.Message);
            }

        }

        #region Eventos Tactil

        private void txtUsername_TouchDown(object sender, TouchEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            showKeyboard(textbox, this);
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            ConfirmationLogin();
        }
        #endregion

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            var messageBoxResult = CustomMessageBox.Show("Cancelar Operación", "¿Desea cancelar la operación y regresar?", MessageBoxButton.YesNo, CustomMessageBox.MessageBoxImage.Question);

            switch (messageBoxResult)
            {
                case MessageBoxResult.Yes:
                    this.Close();
                    break;
            }
        }

    

        #region Metodos Del login

        private void ConfirmationLogin()
        {
            try
            {
                if ((txtUsername.Text != "") || (txtPasscode.Password != ""))
                {
                    Response loginUser = new Response();
                    
                    loginUser = _UserServiceApi.UserLoginAction(txtUsername.Text, txtPasscode.Password, _Action);

                    if (loginUser.success == 1)
                    {
                        if (loginUser.data != null)
                        {
                            userActions = JsonConvert.DeserializeObject<UserActionResponse>(loginUser.data.ToString());
                            this.Close();
                        }
                        else
                        {
                            CustomMessageBox.Show("Informacion", loginUser.message, MessageBoxButton.OK);
                        }
                    }
                    else
                    {
                        CustomMessageBox.Show("Informacion",loginUser.message,MessageBoxButton.OK);
                    }

                }
                else
                {
                    CustomMessageBox.Show("Campos vacios", "Estimado usuario por favor ingrese sus credenciales para acceder al sistema", MessageBoxButton.OK);

                    return;
                }

            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error", ex.Message);

            }

        }






        #endregion

     
    

    }
}
