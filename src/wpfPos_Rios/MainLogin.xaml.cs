using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using wpfPos_Rios.service.Security;
using wpfPos_Rios.Views.helper;
using KeyPad;
using Models;
using System.Collections.Specialized;
using Newtonsoft.Json;
using wpfPos_Rios.service;
using wpfPos_Rios.ViewModels;
using System.Reflection;
using wpfPos_Rios.ViewModels.helperResponse;
using System.IO;
using TfhkaNet.IF.VE;
namespace wpfPos_Rios
{
    /// <summary>
    /// Interaction logic for MainLogin.xaml
    /// </summary>
    public partial class MainLogin : Window
    {
        //servicios y respuesta
        InvoiceService _serviceInvoice;
        CajaService _posServiceApi = new CajaService();
        UserService _serviceUser = new UserService();
        //Impresora
        //PrinterOperations _PrinterOperatations;
        Tfhka _printer;
        //Datos de la caja
        Caja _cajaData;

        //ConfigPos _configPos;
        const string pathConfig = @"C:\posRio\configCaja.json";
        //cache de factura
        CacheJsonInvoice cacheInvoice;


        public MainLogin()
        {
            InitializeComponent();
            //// mostrar versionamiento de app
            Assembly assem = typeof(MainLogin).Assembly;
            AssemblyName assemName = assem.GetName();
            Version ver = assemName.Version;
            lbVersion.Content = $"Application Version {ver}";

            GetConfigCaja();

            _serviceInvoice = new InvoiceService();
            _printer = new Tfhka();
            _cajaData = new Caja();
            cacheInvoice = new CacheJsonInvoice();
            txtbCaja.Text = $"{ConfigPosStatic.TiendaConfig.sucursal} Caja {ConfigPosStatic.CajaConfig.CodigoCaja}";
            
            GetPosConfig(ConfigPosStatic.CajaConfig.CodigoCaja);
            ConectPrinter(ConfigPosStatic.ImpresoraConfig.port);
            S1PrinterData dataS1 = _printer.GetS1PrinterData();
            CheckPrinterCaja(dataS1);
            
        }

        private void GetConfigCaja()
        {
            if (File.Exists(pathConfig))
            {
                string json = File.ReadAllText(pathConfig);
                //configruarion de la caja
                var config = JsonConvert.DeserializeObject<ConfigPos>(json);
                
                ConfigPosStatic.CajaConfig = config.CajaConfig;
                ConfigPosStatic.ImpresoraConfig = config.ImpresoraConfig;
                ConfigPosStatic.TiendaConfig = config.TiendaConfig;
                ConfigPosStatic.vposConfig = config.vposConfig;
            }
            else
            {
                CustomMessageBox.Show("Error obtener la configuracion de la caja", "Falta la configuracion de la caja en el equipo.\n Comuniquese con soporte.");
                Environment.Exit(0);
            }
            
        }

        private void ConectPrinter(string portCom) 
        {
            try
            {
                bool res = _printer.OpenFpCtrl(portCom);

                if (res)
                {
                    res = _printer.CheckFPrinter();
                    if (!res)
                    {
                        throw new Exception("La impresora esta desconectada");
                    }
                }
                else
                {
                    throw new Exception("No se puedo establecer conexión con la impresora, comuníquese con soporte técnico");
                }

            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Error al conectar con la impresora" , ex.Message);
                Environment.Exit(0);
            }
        
        }
        public void CheckPrinterCaja(S1PrinterData dataS1)
        {
            
            if (_cajaData.SerialImpresora == dataS1.RegisteredMachineNumber)
            {
                if (_cajaData.IdFac != -1)
                {
                    string lastPrintedInvoice = dataS1.LastInvoiceNumber.ToString("00000000");
                    if (lastPrintedInvoice == _cajaData.NumeroFactura)
                    {
                        //si la ultima factura se imprimio y quedo estatus 0 las actualiza a impresa 1
                        cacheInvoice.DeleteArchive();
                        UpdatePrintedInvoice(_cajaData.IdFac);
                    }

                }
            }
            else
            {
                CustomMessageBox.Show("Error con la impresora", "La Impresora fiscal no corrresponde a esta caja,  comuniquese con sistemas.\nLa aplicacion se cerrara");
                Environment.Exit(0);
            }
            
        }
        #region Teclado
        private void showKeyboard(TextBox pTextbox, Window pIndex)
        {
            VirtualKeyboard keyboardwindow = new VirtualKeyboard(pTextbox, pIndex);
            if (keyboardwindow.ShowDialog() == true)
                pTextbox.Text = keyboardwindow.Result;
        }
     

        #endregion


        #region Metodos del Login

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            if ((txtUsername.Text != "") || (txtPasscode.Password != ""))
            {
                try
                {
                    S1PrinterData dataS1 = _printer.GetS1PrinterData();
                    Response loginUser = _serviceUser.Login(txtUsername.Text, txtPasscode.Password, _cajaData.Id);

                    if (loginUser.success == 1)
                    {
                        UserCajera cajera = new UserCajera();
                        cajera = JsonConvert.DeserializeObject<UserCajera>(loginUser.data.ToString());


                        if (cajera.Estatus == 1)
                        {
                            //Configurar los puertos  y actualizar y abrir
                            //Pasar los datos del usuario a un modelo protegido y abstracto 
                            Response ResultProfile = _serviceUser.GetUserProfile(cajera.Id); ;

                            if (ResultProfile.success == 1)
                            {
                                if (ResultProfile.data != null)
                                {
                                    Acciones ListActions = JsonConvert.DeserializeObject<Acciones>(ResultProfile.data.ToString());

                                    if (ListActions.estatus)
                                    {
                                        VMUserProfile.ActionsList = ListActions.acciones;
                                        VMUserProfile.rolUser = ListActions.rolUser;
                                        VMUserProfile.name = ListActions.nombre;
                                        VMUserProfile.idTienda = ListActions.idTienda;

                                        string lastPrintedInvoice = (dataS1.LastInvoiceNumber + 1).ToString("00000000");
                                        //Index app = new Index(_printer, _cajaData, cajera);
                                        PosWindow app = new PosWindow(_printer, _cajaData, cajera);

                                        if (_cajaData.IdFac == -1 && File.Exists(cacheInvoice.PathCache + cacheInvoice.Folder + "\\" + cacheInvoice.Archive))
                                        {
                                            string json = File.ReadAllText(cacheInvoice.PathCache + cacheInvoice.Folder + "\\" + cacheInvoice.Archive);
                                            if (!string.IsNullOrEmpty(json))
                                            {
                                               
                                                JsonData jsonData = JsonConvert.DeserializeObject<JsonData>(json);

                                                VmInvoice invoice = new VmInvoice();
                                                invoice.Total = jsonData.Invoice.Total;
                                                invoice.SubTotal = jsonData.Invoice.SubTotal;
                                                invoice.Discount = jsonData.Invoice.Discount;
                                                invoice.Tax = jsonData.Invoice.Tax;
                                                invoice.Cancelled = jsonData.Invoice.Cancelled;
                                                invoice.Retorno = jsonData.Invoice.Retorno;
                                                invoice.ProductList = new List<VmProduct>();
                                                jsonData.Invoice.ProductList.ForEach(p => {
                                                    if (p.Descripcion.Length > 0)
                                                    {
                                                        invoice.ProductList.Add(new VmProduct(jsonData.Caja.Tasa)
                                                        {
                                                            Id = p.Id,
                                                            Descripcion = p.Descripcion,
                                                            Code = p.Code,
                                                            CodigoTipo = (p.CodigoTipo == 1) ? TipoProducto.producto : TipoProducto.combo, 
                                                            UnitPriceBs = p.UnitPriceBs,
                                                            Quantity = p.Quantity,
                                                            UnitPrice = p.UnitPrice,
                                                            DiscountPercentage = p.DiscountPercentage,
                                                            Iva = p.Iva,
                                                            Serial = p.Serial,
                                                            Pesado = p.Pesado
                                                        });
                                                    }
                                                });

                                                app.RecoverInvoiceToJson(invoice, jsonData.Pagos, jsonData.Caja, jsonData.Client);
                                                
                                            }
                                            else
                                            {
                                                cacheInvoice.DeleteArchive();
                                            }
                                        }

                                        app.Show();
                                        app.SetScale();
                                        if (!((_cajaData.IdFac == -1 || lastPrintedInvoice != _cajaData.NumeroFactura) && File.Exists(cacheInvoice.PathCache + cacheInvoice.Folder + "\\" + cacheInvoice.Archive)))
                                        {
                                            app.SelectClient();
                                        }
                                        this.Close();
                                    }
                                    else
                                    {
                                        CustomMessageBox.Show("Advertencia" , "Usuario sin permisos");
                                    }

                                }
                            }

                        }
                    }
                    else
                    {
                        CustomMessageBox.Show("Advertencia" , loginUser.message);
                    }
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show("Error iniciar turno",ex.Message , MessageBoxButton.OK);
                }
            }
            else
            {
                CustomMessageBox.Show("Campos vacios", "Estimado usuario por favor ingrese sus credenciales para acceder al sistema", MessageBoxButton.OK);
            }

        }


        #endregion


        #region Inicio de configuracion de perifericos
     
        public void UpdatePrintedInvoice(int invoiceID)
        {
            Response request = new Response();
            request = _serviceInvoice.UpdateInvoice(invoiceID);

            if (request.success == 1)
            {
                
            }
            else
            {
                CustomMessageBox.Show("Error al actualizar factura creada", request.message);
            }
        }
        private void GetPosConfig(string pCodigoCaja)
        {
            try
            {
                var data = _posServiceApi.GetConfigCaja(pCodigoCaja);

                if (data != null && data.success == 1)
                {
                    _cajaData = JsonConvert.DeserializeObject<Caja>(data.data.ToString());
                    _cajaData.CodigoCaja = ConfigPosStatic.CajaConfig.CodigoCaja;
                }
                else
                {
                    CustomMessageBox.Show("No se obtuvo la configuracion de la caja", data.message , MessageBoxButton.OK);
                }

            }

            catch (Exception ex)
            {
                CustomMessageBox.Show("Error en la configuración de la caja", "Estimado usuario ha habido un problema al momento de obtener la configuración de esta caja, No se ha podido establecer conexión con el servidor."+"\n"+ex.Message+Environment.NewLine+"Comuniquese con soporte técnico", MessageBoxButton.OK);
                Environment.Exit(0);
            }
        }


        #endregion


        #region Cerrar aplicacion

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {

            MessageBoxResult response = CustomMessageBox.Show("Cerrar la aplicación", " Esta seguro que deseas cerrar la aplicación?", MessageBoxButton.YesNoCancel);

            switch (response.ToString().ToLower())
            {
                case "yes":
                    //Eliminar Datos de la factura
                    _printer.CloseFpCtrl();
                    //validar que no cierre la aplicaacion sino ha habido un cierre de caja
                    Environment.Exit(0);
                    break;
            }


        }

        #endregion

        #region Eventos Del Tactil
        private void txtUsername_TouchDown(object sender, TouchEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            showKeyboard(textbox, this);
        }

        private void txtPasscode_TouchDown(object sender, TouchEventArgs e)
        {
            TextBox textbox = new TextBox();
            PasswordBox box = new PasswordBox();

            textbox.Text = box.Password;

            VirtualKeyboardPass keyboardwindow = new VirtualKeyboardPass(textbox, this);
            if (keyboardwindow.ShowDialog() == true)
                txtPasscode.Password = keyboardwindow.Result;

        }
        #endregion
    }
}

