using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfPos_Rios.ViewModels.helperResponse
{
   public class InvoiceResponse
    {
        public cliente cliente { get; set; }
        public Factura factura { get; set; }
        public List<DetalleFactura> detalleFactura { get; set; }

        public class Cliente
        {
            public int id { get; set; }
            public string nombre { get; set; }
            public double saldo { get; set; }
            public string rif { get; set; }
            public string apellido { get; set; }
            public object telefono { get; set; }
            public object email { get; set; }
            public object direccion { get; set; }
        }

        public class Factura
        {
            public int id { get; set; }
            public object codigoArea { get; set; }
            public int clienteId { get; set; }
            public object codigoAfiliacion { get; set; }
            public int estatus { get; set; }
            public string numeroFactura { get; set; }
            public string numeroControl { get; set; }
            public int cajaId { get; set; }
            public int puntosGenerados { get; set; }
            public object guiaLicor { get; set; }
            public double montoBruto { get; set; }
            public double montoNeto { get; set; }
            public double montoIVA { get; set; }
            public double montoDescuento { get; set; }
            public double montoPagado { get; set; }
            public int cajeraId { get; set; }
            public string cajera { get; set; }
            public object serialFiscal { get; set; }
            public DateTime fecha { get; set; }
            public int codigoTurno { get; set; }
            public double tasa { get; set; }
            public int tiendaId { get; set; }
            public object listaProductos { get; set; }
            public object listaPagos { get; set; }
        }

        public class DetalleFactura
        {
            public int id { get; set; }
            public int tipo { get; set; }
            public string codigoproducto { get; set; }
            public string descripcion { get; set; }
            public double precio { get; set; }
            public double total { get; set; }
            public double precioneto { get; set; }
            public double totalneto { get; set; }
            public double cantidad { get; set; }
            public bool pesado{ get; set; }
        }

    }
}
