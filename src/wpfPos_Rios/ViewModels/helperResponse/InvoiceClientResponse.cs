using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfPos_Rios.ViewModels.helperResponse
{
    public class InvoiceClientResponse
    {
        public cliente cliente { get; set; }
        public Tasa tasa { get; set; }
        public Invoice factura { get; set; }
    }


    public class cliente
    {
        public int id { get; set; }
        public string nombre { get; set; }
        public string apellido { get; set; }
        public string rif { get; set; }
        public string telefono { get; set; }
        public string direccion { get; set; }
        public string email { get; set; }
        public double saldo { get; set; }
    }

    public class Tasa
    {
        public double monto { get; set; }
    }


}
