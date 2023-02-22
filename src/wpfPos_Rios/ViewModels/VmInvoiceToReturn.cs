using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfPos_Rios.ViewModels
{
   public class VmInvoiceToReturn
   {
            public int idCaja { get; set; }
            public int idTienda { get; set; }
            public int idFactura { get; set; }
            public string numeroControl { get; set; }

            public int idUsuario { get; set; }
            public double montoDevolucion { get; set; }
            public string serialFiscal { get; set; }
            public string formaPago { get; set; }
            public List<VmDetalleFacturaReturn> detallesList { get; set; }


    }
    
}
