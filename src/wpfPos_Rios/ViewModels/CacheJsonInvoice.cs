using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfPos_Rios.Views;
using System.IO;
using Newtonsoft.Json;
//eliminar
using System.Windows;
using System.Security.AccessControl;
using System.Security.Principal;

namespace wpfPos_Rios.ViewModels
{
    class CacheJsonInvoice
    {
        public string PathCache { get; set; }
        public string Folder { get; set; }
        public string Archive { get; set; }

        public CacheJsonInvoice ()
        {
            PathCache = AppDomain.CurrentDomain.BaseDirectory;
            Folder = "factura";
            Archive = "factura.json";
        }

        public void WriteInvoice(VmInvoice invoice, List<PaymentMethod> paymentlist, Caja _cajaData , VmClient _clientData)
        {
            //si no existe el directorio crealo
            try
            {
                if (!Directory.Exists(PathCache + Folder))
                {
                    DirectoryInfo di = Directory.CreateDirectory(PathCache + Folder);
                }

                string dataJson = JsonConvert.SerializeObject(new { Client = _clientData, Caja = _cajaData, Invoice = invoice, Pagos = paymentlist });
                File.WriteAllText(PathCache + Folder + "\\" + Archive, dataJson);


                DirectoryInfo dInfo = new DirectoryInfo(PathCache + Folder + "\\" + Archive);
                DirectorySecurity dSecurity = dInfo.GetAccessControl();
                dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
                dInfo.SetAccessControl(dSecurity);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void DeleteArchive()
        {
            try
            {
                if (File.Exists(PathCache + Folder + "\\"+ Archive))
                {
                    File.Delete(PathCache + Folder + "\\" + Archive);
                }   
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }

    public class JsonData
    {
        public VmClient Client { get; set; }
        public Caja Caja { get; set; }
        public InvoiceJson Invoice { get; set; }
        public List<PaymentMethod> Pagos { get; set; }
    }

    public class ProductList
    {
        public int CodigoTipo { get; set; }
        public string Descripcion { get; set; }
        public string Code { get; set; }
        public double Quantity { get; set; }
        public double UnitPrice { get; set; }
        public double DiscountPercentage { get; set; }
        public object Image { get; set; }
        public int Id { get; set; }
        public double Iva { get; set; }
        public string Serial { get; set; }
        public bool Pesado { get; set; }
        public double TotalBs { get; set; }
        public double UnitPriceBs { get; set; }
        public double TotalRef { get; set; }
    }

    public class InvoiceJson
    {
        public List<ProductList> ProductList { get; set; }
        public double Total { get; set; }
        public double SubTotal { get; set; }
        public double Discount { get; set; }
        public double Tax { get; set; }
        public double Cancelled { get; set; }
        public double Retorno { get; set; }
        public double Restante { get; set; }
    }

}
