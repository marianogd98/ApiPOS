using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfPos_Rios.ViewModels
{
    public class vmDonationReason
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Motivo { get; set; }
        public string Address { get; set; }
        public string Rif { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public double Tasa { get; set; }

    }
}
