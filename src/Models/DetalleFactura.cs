using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class DetalleFactura
    {
        public int id { get; set; }
        public int tipo { get; set; }
        public string codigoproducto { get; set; }
        public string descripcion { get; set; }
        public double precio { get; set; }
        public double total { get; set; }
        public double precioneto { get; set; }
        public double totalneto { get; set; }
        public double cantidad { get; set; }
        public double cantidadDevolver { get; set; }
    }
}
