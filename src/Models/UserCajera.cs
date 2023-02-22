using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class UserCajera
    {
        public string token { get; set; }
        public int Id { get; set; }
        public int Estatus { get; set; }
        public string Nombre { get; set; }
        public string UserName { get; set; }
        public string Cedula { get; set; }
        public int idTurno { get; set; }
    }
}
