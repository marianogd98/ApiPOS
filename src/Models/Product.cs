using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Product
    {
        public int id { get; set; }
        public string CodigoProducto { get; set; }
        public string CodigoSubgrupo { get; set; }
        public int CodigoTipo { get; set; }
        public string Barra { get; set; }
        public string CodigoBalanza { get; set; }
        public bool Pesado { get; set; }
        public bool ManejaSerial { get; set; }
        public bool Regulado { get; set; }
        public string Descripcion { get; set; }
        public bool Estatus { get; set; }
        public double Costo { get; set; }
        public double Cantidad { get; set; }
        public double PrecioDetal { get; set; }
        public double PrecioMayor { get; set; }
        public double PrecioOferta { get; set; }
        public bool ActivoVenta { get; set; }
        public Nullable<int> CantidadAplicaDescuento { get; set; }
        public Nullable<System.DateTime> FechaOfertaIni { get; set; }
        public double Descuento { get; set; }
        public Nullable<System.DateTime> FechaOfertaFin { get; set; }
        public int Punto { get; set; }
        public string CodigoMoneda { get; set; }
        public double PrecioDolar { get; set; }
        public double PrecioBolivar { get; set; }
        public double IVA { get; set; }
        public string Serial { get; set; }
        public string Image { get; set; }
        public double Total { get; set; }

    }
}
