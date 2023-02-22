using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class ConfigPos
    {
        public CajaConfig CajaConfig { get; set; }
        public ImpresoraConfig ImpresoraConfig { get; set; }
        public TiendaConfig TiendaConfig { get; set; }
        public VposConfig vposConfig { get; set; }
    }

    public class CajaConfig
    {
        public string CodigoCaja { get; set; }
    }

    public class ImpresoraConfig
    {
        public string port { get; set; }
        public string flat { get; set; }
    }

    public class TiendaConfig
    {
        public string ipServerPos { get; set; }
        public string ipServerSec { get; set; }
        public string uriAreaImage { get; set; }
        public string sucursal { get; set; }
    }

    public class VposConfig
    {
        public string uri { get; set; }
    }
}
