using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class InvoiceToReturn
    {
        public int idCaja { get; set; }
        public int idTienda { get; set; }
        public int idFactura { get; set; }
        public string numeroControl { get; set; }
        public int idUsuario { get; set; }
        public double montoDevolucion { get; set; }
        public string serialFiscal { get; set; }
        public string formaPago { get; set; }
        public int idTurno { get; set; }
        public string idNotaCredito { get; set; }
        public List<DetalleFactura> detallesList { get; set; }
    }
}
