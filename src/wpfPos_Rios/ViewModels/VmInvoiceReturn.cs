using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfPos_Rios.ViewModels
{
  public  class VmInvoiceReturn
    {

        public int id { get; set; }
        public string nombre { get; set; }
        public string numeroControl { get; set; }
        public string numeroFactura { get; set; }
        public string codigoCaja { get; set; }
        public string rif { get; set; }
        public string cajero { get; set; }
        public string apellido { get; set; }
        public double montoBruto { get; set; }
        public double montoNeto { get; set; }
        public double montoIVA { get; set; }
        public double montoDescuento { get; set; }
        public DateTime fecha { get; set; }
        public int idUsuario { get; set; }
        public int idCaja { get; set; }
        public int idTienda { get; set; }
        public string serialFiscal { get; set; }

        public IList<VmDetalleFacturaReturn> detallesList { get; set; }


    }


}
