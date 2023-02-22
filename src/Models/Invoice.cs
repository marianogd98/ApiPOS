using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Models
{
    public class Invoice
    {

        public int id { get; set; }
        public string CodigoArea { get; set; }
        public int ClienteId { get; set; }
        public string CodigoAfiliacion { get; set; }
        public int Estatus { get; set; }
        public string NumeroFactura { get; set; }
        public string NumeroControl { get; set; }
        public int CajaId { get; set; }
        public int PuntosGenerados { get; set; }
        public string GuiaLicor { get; set; }
        public double MontoBruto { get; set; }
        public double MontoNeto { get; set; }
        public double MontoIVA { get; set; }
        public double MontoDescuento { get; set; }
        public double MontoPagado { get; set; }
        public int CajeraId { get; set; }
        public string SerialFiscal { get; set; }
        public System.DateTime Fecha { get; set; }
        public string CodigoTurno { get; set; }
        public double tasa { get; set; }
        public int TiendaId { get; set; }
        public  List<Product> listaProductos { get; set; }
        public  List<PaymentMethod> listaPagos { get; set; }

        //retorna porcentaje de descuento formateado para la maquina fiscal
        public double PorcentajeDescuento
        {
            get => (MontoDescuento == 0)? 0 : Math.Round((MontoDescuento / MontoBruto) * 10000 , 2);
        }
      
    }
}
