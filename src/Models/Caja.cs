using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Caja
    {
        public int Id { get; set; }
        public int TiendaId { get; set; }
        public string CodigoCaja { get; set; }
        public string NControl { get; set; }
        public string PuertoBalanza { get; set; }
        public string PuertoCodigoBarra { get; set; }
        public string PuertoImpresora { get; set; }
        public string SerialImpresora { get; set; }
        public string VTID { get; set; }
        public int AreaId { get; set; }
        public bool AbrirGaveta { get; set; }
        public bool FacturaAlMayor { get; set; }
        public bool Estatus { get; set; }
        public double Tasa { get; set; }
        public int IdFac { get; set; }
        public string NumeroFactura { get; set; }
        public int BancoId { get; set; }

    }
}
