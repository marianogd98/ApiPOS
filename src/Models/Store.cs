using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Store
    {

        public int id { get; set; }
        public string CodigoTienda { get; set; }
        public string CodigoArea { get; set; }
        public string Nombre { get; set; }
        public bool Activo { get; set; }
        public bool AplicaIVA { get; set; }
        public string URLAPI { get; set; }
        public double tasa { get; set; }

    }
}
