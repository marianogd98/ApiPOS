using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfPos_Rios.ViewModels
{
    public class vmInvoicePending
    {
        public int id { get; set; }
        public int clienteId { get; set; }
        public string rif { get; set; }
        public string nombre { get; set; }
        public string apellido { get; set; }
        public DateTime fecha { get; set; }
    }
}
