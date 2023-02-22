using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Models
{
    public class ReporteZeta
    {
        public int IdCaja { get; set; }
        public string Numero { get; set; }
        public double Monto { get; set; }
        public string UltimaFactura { get; set; }
        public int CantidadFacturas { get; set; }
        public string xml { get; set; }



    }
}
