using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfPos_Rios.ViewModels
{
    public class VmPaymentMethod
    {
        public int id { get; set; }
        public string codigoMoneda { get; set; }
        public string descripcion { get; set; }
        public string telefono { get; set; }
        public string correoZelle { get; set; }
        public int orden { get; set; }

    }
}
