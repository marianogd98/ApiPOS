using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public  class Client
    {
        public int Id { get; set; }
        public int CodigoTipoCliente { get; set; }
        public bool Estatus { get; set; }
        public System.DateTime FechaRegistro { get; set; }
        public string Nombre { get; set; }
        public string RazonComercial { get; set; }
        public string RIF { get; set; }
        public string NIT { get; set; }
        public string Apellido { get; set; }
        public string Telefono { get; set; }
        public string email { get; set; }
        public string Direccion { get; set; }
        public System.DateTime created_at { get; set; }
        public System.DateTime updated_at { get; set; }
    }
}
