using Models.Vpos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
  public  class PaymentMethod
    {
        private int id;
        private double monto;
        private int? cuentaBancariaId;
        private string lote;
        private string numeroTransaccion;
        private string codigoMoneda;
        private string nombre;
        public VposResponse vpos { get; set; }
        public string tipoTarjeta { get; set; }
        public string nroAutorizacion { get; set; }
        public string numSeq { get; set; }
        public int Id { get => id; set => id = value; }
        public double Monto { get => monto; set => monto = value; }
        public int? CuentaBancariaId { get => cuentaBancariaId; set => cuentaBancariaId = value; }
        public string Lote { get => lote; set => lote = value; }
        public string NumeroTransaccion { get => numeroTransaccion; set => numeroTransaccion = value; }
        public string CodigoMoneda { get => codigoMoneda; set => codigoMoneda = value; }
        public string Nombre { get => nombre; set => nombre = value; }
    }
}
