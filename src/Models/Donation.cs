using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Donation
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public int CajaId { get; set; }
        public int TurnoId { get; set; }
        public int CajeroId { get; set; }
        public int OrganizacionId { get; set; }
        public int TiendaId { get; set; }
        public int Estatus { get; set; }
        public DateTime Fecha { get; set; }
        public double Tasa { get; set; }
        public PaymentMethod PaymnetMethod { get; set; }
    }
}
