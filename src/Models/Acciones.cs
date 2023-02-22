using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Acciones
    {
        public string nombre { get; set; }
        public List<Accion> acciones { get; set; }
        public string rolUser { get; set; }
        public int idTienda { get; set; }
        public bool estatus { get; set; }
    }

    public class Accion
    {
        public int idAccion { get; set; }
        public string nombre { get; set; }
        public bool permitido { get; set; }
    }

}
