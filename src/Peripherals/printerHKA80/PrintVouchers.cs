using Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TfhkaNet.IF;
using TfhkaNet.IF.VE;
namespace Peripherals.printerHKA80
{
    public class PrintVouchers
    {
        readonly Tfhka tfhkaPrinter;
        readonly string PortCom;
        readonly string FlagPrinter;
        readonly NumberFormatInfo _FormatVenezuela;

        public string Path { get; set; }
        public string Folder { get; set; }
        public string Archive { get; set; }

        public PrintVouchers(Tfhka pPrinter , string port , string flagPrinter)
        {
            _FormatVenezuela = new CultureInfo("es-VE").NumberFormat;
            tfhkaPrinter = pPrinter;
            PortCom = port;
            FlagPrinter = flagPrinter;

            Path = AppDomain.CurrentDomain.BaseDirectory;
            Folder = "txtFacturas";

        }

        public string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;

            value = Regex.Replace(value, "[^0-9A-Za-z\\s]", "");

            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        private string LineProductToPrinter(Product product , string FlagPrinter)
        {
            string fmt1 = "00000000##";
            string fmt2 = "00000###";
            string fmt3 = "00000000000000##";
            string fmt4 = "00000000000000###";
            string line = string.Empty;

            double precio;
            double cantidad;
            string priceQuantity;

            product.Descripcion = Truncate(product.Descripcion.Trim(), 40);
            if (FlagPrinter == "2100")
            {
                if (product.Descuento > 0)
                {
                    precio = Math.Round( (product.PrecioBolivar - (product.PrecioBolivar * product.Descuento)) , 2 );
                    precio *= 100;
                    cantidad = (product.Cantidad * 1000);
                    priceQuantity = $"{precio.ToString(fmt1)}{cantidad.ToString(fmt2)}";
                    line =$" {priceQuantity}{product.Descripcion}";
                }
                else
                {
                    precio = Math.Round((product.PrecioBolivar), 2);
                    precio *= 100;
                    cantidad = (product.Cantidad * 1000);
                    priceQuantity = $"{precio.ToString(fmt1)}{cantidad.ToString(fmt2)}";
                    line = $" {priceQuantity}{product.Descripcion}";
                }
            }
            if (FlagPrinter == "2130")
            {
                if (product.Descuento > 0)
                {
                    precio = Math.Round((product.PrecioBolivar - (product.PrecioBolivar * product.Descuento )) , 2);
                    precio *= 100;
                    cantidad = (product.Cantidad * 1000);
                    priceQuantity = $"{precio.ToString(fmt3)}{cantidad.ToString(fmt4)}";
                    line = $" {priceQuantity}{product.Descripcion}";
                }
                else
                {
                    precio = Math.Round((product.PrecioBolivar), 2);
                    precio *= 100 ;
                    cantidad = (product.Cantidad * 1000);
                    priceQuantity = $"{precio.ToString(fmt3)}{cantidad.ToString(fmt4)}";
                    line = $" {priceQuantity}{product.Descripcion}";
                }
            }
            return line;
        }

        private string CreateTXT(List<string> commands)
        {
            try
            {
                S1PrinterData dataS1 = tfhkaPrinter.GetS1PrinterData();
                string numInvoice = dataS1.LastInvoiceNumber.ToString("00000000");
                numInvoice = (int.Parse(numInvoice) + 1).ToString("00000000");

                string fecha = DateTime.Now.ToString("ddMMyyyy");
                string pathFolder = $"{Path}\\{Folder}\\{fecha}";
                Archive = $"{numInvoice}.txt";
                string pathTxt = $"{pathFolder}\\{Archive}";

                
                if (!Directory.Exists(pathFolder))
                {
                    DirectoryInfo di = Directory.CreateDirectory(pathFolder);
                }

                if (File.Exists(pathTxt))
                {
                    DeteleTxt(pathTxt);
                }

                if (!File.Exists(pathTxt))
                {
                    // Create a file to write to.
                    using (StreamWriter sw = File.CreateText(pathTxt))
                    {
                        commands.ForEach(cmd => {

                            sw.WriteLine(cmd);

                        });
                    }
                }

                if (File.Exists(pathTxt))
                {
                    return pathTxt;
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        private void DeteleTxt(string path)
        {
            try
            {
                string pathTxt = string.Empty;
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public bool CmdImpFactura(string pNumDocu, string pClientName, string pClienteDoc, 
            string pAdressClient, string pCajero,string pCaja, string pPayment, List<Product> pProductos,                                     
            List<PaymentMethod> ppaymentList, string pSaldoActual = "0", string pSaldoAnterior = "0")
        {
            try
            {
                bool resPrinter = false;

                if (!tfhkaPrinter.CheckFPrinter())
                {
                    tfhkaPrinter.OpenFpCtrl(PortCom);
                }

                //Cambiar cuando pase a 14 digitos con el flag 26 (actualmente esta en 10 digitos)
                List<string> invoicePrint = new List<string>();
                string saldoConsumido = string.Empty;
                string saldoAbonado = string.Empty;
                bool flagWallet = false;

                //datos del cliente
                string clientDate = $"i01Cliente: {pClientName.Trim()}";
                string clientDoc = $"i02Documento: {pClienteDoc.Trim()}";
                string ClientAddress = $"i03Direccion: {pAdressClient.Trim()}";
                string NumDoc = $"i04Documento: {pNumDocu.Trim()}";
                string CajaData = $"i05Cajero:{pCajero} Caja: {int.Parse(pCaja)}";
              
                if (ppaymentList != null)
                {
                    for (int i = 0; i < ppaymentList.Count; i++)
                    {
                        if (ppaymentList[i].Id == 4)
                        {
                            flagWallet = true;
                            if (ppaymentList[i].Monto > 0)
                                saldoConsumido = ppaymentList[i].Monto.ToString();
                            if (ppaymentList[i].Monto < 0)
                                saldoAbonado = (-1 * ppaymentList[i].Monto).ToString();
                        }
                    }
                }

                invoicePrint.Add(clientDate);
                invoicePrint.Add(clientDoc);
                invoicePrint.Add(ClientAddress);
                invoicePrint.Add(NumDoc);
                invoicePrint.Add(CajaData);

                pProductos.ForEach(product => {
                    invoicePrint.Add(LineProductToPrinter(product , FlagPrinter) );
                });

                
                if (flagWallet)
                {
                    invoicePrint.Add("i06Saldo Wallet:" + pSaldoAnterior);
                    //invoicePrint.Add("i07Saldo:" + pSaldoAnterior);
                    
                    if (!string.IsNullOrEmpty(saldoConsumido))
                        invoicePrint.Add("i07Saldo Consumido:" + saldoConsumido);
                    if (!string.IsNullOrEmpty(saldoAbonado))
                        invoicePrint.Add("i08Saldo Abonado:" + saldoAbonado);
                    if(pSaldoActual!= "-1")
                        invoicePrint.Add("i09Saldo Actual:" + pSaldoActual);
                }
                //subtortal
                invoicePrint.Add("3");

                //if (pDiscount != "0000")//Imprimir descuento si hay
                //{
                //    invoicePrint.Add("p-" + pDiscount);
                //}
                invoicePrint.Add(pPayment);

                string path = CreateTXT(invoicePrint);

                //invoicePrint.ForEach(line => {
                //    resPrinter = tfhkaPrinter.SendCmd(line);
                //});

                int cmd = tfhkaPrinter.SendFileCmd(path);
                return cmd > 0;
                //DeteleTxt(path);
                //return resPrinter;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        private string LineProducReturntToPrinter(DetalleFactura product, string FlagPrinter)
        {
            string fmt1 = "00000000##";
            string fmt2 = "00000###";
            string fmt3 = "00000000000000##";
            string fmt4 = "00000000000000###";
            string line = string.Empty;

            product.descripcion = Truncate(product.descripcion, 100);
            if (FlagPrinter == "2100")
            {
                line = "d0" + (product.precioneto * 100).ToString(fmt1) + (product.cantidadDevolver * 1000).ToString(fmt2) + product.descripcion;
            }
            if (FlagPrinter == "2130")
            {
                //Validacion de formato para imprimir
                line = "d0" + (product.precio * 100).ToString(fmt3) + (product.cantidadDevolver * 1000).ToString(fmt4) + product.descripcion;
            }
            return line;
        }
        public bool CmdImpNotaCredito(string pNumDocuImp, string pFechaDocu, string pSerialImp, string pControlInterno
                                    , string pClientName, string pClienteDoc, string pCajero, string pCaja, List<DetalleFactura> pProductos)
        {

            try
            {
                bool resPrinter = false;
                List<string> ListCmd = new List<string>
                {
                    "iR*" + pClienteDoc + "",
                    "iS*" + pClientName + "",
                    "iF*" + pNumDocuImp + "",
                    "iD*" + pFechaDocu + "",
                    "iI*" + pSerialImp + "",
                    "i01Documento*" + pControlInterno + "",
                    "i02Cajero:" + pCajero + "       Caja:" + pCaja + ""
                };

                pProductos.ForEach(product => {
                    ListCmd.Add(LineProducReturntToPrinter(product, FlagPrinter));
                });

                ListCmd.Add("3");
                ListCmd.Add("101");

                ListCmd.ForEach(line =>
                {
                    resPrinter = tfhkaPrinter.SendCmd(line);
                });

                return resPrinter;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public bool PrintInvoiceCredit(Invoice pinvoice , string pClientName , string pClientDoc , string pClientAddress , string pCajeraName , string pCodCaja)
        {
            try
            {
                if (!tfhkaPrinter.CheckFPrinter())
                {
                    tfhkaPrinter.OpenFpCtrl(PortCom);
                }

                bool resPrinter = false;
                List<string> LinePrinter = new List<string>
                {
                    "Documento:" + pinvoice.NumeroControl,
                    "Cliente:" + pClientName,
                    "RIF/C.I:" + pClientDoc,
                    "Dir.:" + pClientAddress,
                    "Cajero:" + pCajeraName + "       Caja:" + pCodCaja + "",
                    "FACTURA:" + pinvoice.NumeroFactura,
                    ""
                };

                foreach (var item in pinvoice.listaProductos)
                {
                    LinePrinter.Add(item.Cantidad.ToString() + "x " + item.PrecioBolivar.ToString("C", _FormatVenezuela));
                    LinePrinter.Add(Truncate(item.Descripcion, 35));
                    LinePrinter.Add("                                          " + "" + (item.PrecioBolivar * item.Cantidad).ToString("C", _FormatVenezuela));
                }

                LinePrinter.Add("");
                LinePrinter.Add("SUBTOTAL:" + "                                       " + pinvoice.MontoBruto.ToString("C", _FormatVenezuela));
                LinePrinter.Add("TOTAL:" + "                                          " + pinvoice.MontoNeto.ToString("C", _FormatVenezuela));
                LinePrinter.Add("Gracias por preferirnos!!!");
                LinePrinter.Add("");
                LinePrinter.Add("Siguénos en instagram como: @rio_supermarket");
                LinePrinter.Add("");

                resPrinter = tfhkaPrinter.SendCmd("80$" + "COPIA FACTURA A CRÉDITO");
                resPrinter = tfhkaPrinter.SendCmd("80¡" + "COPIA FACTURA CLIENTE");
                LinePrinter.ForEach(line => {

                    resPrinter = tfhkaPrinter.SendCmd($"800 {line}");
                
                });

                resPrinter = tfhkaPrinter.SendCmd($"810");

                return resPrinter;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }
        }

        public bool PrintVoucher(string[] voucher)
        {
            try
            {
                bool resPrinter = false;

                if (!tfhkaPrinter.CheckFPrinter())
                {
                    tfhkaPrinter.OpenFpCtrl(PortCom);
                }

                List<string> comandos = new List<string>();

                foreach (var line in voucher)
                {
                    if (line.Trim() != "" && line.Trim() != "\n")
                    {
                        comandos.Add(line);
                    }
                }

                resPrinter = tfhkaPrinter.SendCmd($"800 Cierre Punto de Venta");

                comandos.ForEach(cmd =>{
                    resPrinter = tfhkaPrinter.SendCmd($"800 {cmd}");
                });

                resPrinter = tfhkaPrinter.SendCmd("810 Fin del Documento no Fiscal");


                return resPrinter;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }
        public bool PrintVoucherMerchant(string[] voucher)
        {
            try
            {
                bool resPrinter = false;

                if (!tfhkaPrinter.CheckFPrinter())
                {
                    tfhkaPrinter.OpenFpCtrl(PortCom);
                }

                List<string> comandos = new List<string>();

                foreach (var line in voucher)
                {
                    if (line.Trim() != "" && line.Trim() != "\n")
                    {
                        comandos.Add(line);
                    }
                }

                comandos.ForEach(cmd =>{
                    resPrinter = tfhkaPrinter.SendCmd($"800 {cmd}");
                });

                resPrinter = tfhkaPrinter.SendCmd("810 Fin del Documento no Fiscal");


                return resPrinter;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }



        public  bool PrintReportsPayment(List<string> cmd , string tipo)
        {
            try
            {
                bool resPrinter = false;


                if (!tfhkaPrinter.CheckFPrinter())
                {
                    tfhkaPrinter.OpenFpCtrl(PortCom);
                }

                resPrinter = tfhkaPrinter.SendCmd("800" + "Resumen Formas de Pago" + "");
                resPrinter = tfhkaPrinter.SendCmd("800" + tipo + "");


                cmd.ForEach(line => {

                    resPrinter = tfhkaPrinter.SendCmd("800" + line + "");

                });

                resPrinter = tfhkaPrinter.SendCmd("810 Fin del Documento no Fiscal");

                return resPrinter;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }
        public bool CmdImpDNF(string pDescripcionDoc, string pEncabezado1, string pEncabezado2, List<string> pDatos)
        {
            try
            {
                bool resPrinter = false;
                if (!tfhkaPrinter.CheckFPrinter())
                {
                    tfhkaPrinter.OpenFpCtrl(PortCom);
                }

                resPrinter = tfhkaPrinter.SendCmd("80$" + pEncabezado1 + "");
                resPrinter = tfhkaPrinter.SendCmd("80¡" + pDescripcionDoc + "");
                resPrinter = tfhkaPrinter.SendCmd("80!" + pEncabezado2 + "");

                foreach (string item in pDatos)
                {
                    resPrinter = tfhkaPrinter.SendCmd("80*" + item + "");

                }

                resPrinter = tfhkaPrinter.SendCmd("81 Fin del Documento no Fiscal");

             
                return resPrinter;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public bool PrintPromocion(List<string> cmd )
        {
            try
            {
                bool resPrinter = false;


                if (!tfhkaPrinter.CheckFPrinter())
                {
                    tfhkaPrinter.OpenFpCtrl(PortCom);
                }

                resPrinter = tfhkaPrinter.SendCmd("800" + "Ticket dia del niño");


                cmd.ForEach(line => {

                    resPrinter = tfhkaPrinter.SendCmd("800" + line + "");

                });

                resPrinter = tfhkaPrinter.SendCmd("810 Fin del Documento no Fiscal");

                return resPrinter;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

    }
}
