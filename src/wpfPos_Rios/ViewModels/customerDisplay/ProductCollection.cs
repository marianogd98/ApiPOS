using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfPos_Rios.ViewModels.customerDisplay
{
    public class ProductCollection
    {

        public Tasa Tasa { get; set; }
        public Totales Totales { get; set; }
        public Productos Productos { get; set; }
        public Productonuevo Productonuevo { get; set; }
        public Cliente Cliente { get; set; }
        public Pagado Pagado { get; set; }
        public int Totalizar { get; set; }
    }
    public class Pagado
    {
        //bs
        public string TotalBs { get; set; }
        public string RecibidoBs { get; set; }
        public string RestanteBs { get; set; }
        //ref
        public string TotalRef { get; set; }
        public string RecibidoRef { get; set; }
        public string RestanteRef { get; set; }


    }

    public class Cliente
    {
        public string name;
        public string apellido;
    }
    public class Tasa
    {
        public string Valor { get; set; }
    }

    public class Totales
    {
        public string Bs { get; set; }
        public string Ref { get; set; }
    }

    public class ProductC
    {
        public string Descripcion { get; set; }
        public string Cantidad { get; set; }
        public string Precio { get; set; }
        public string Subtotal { get; set; }
        public string Ref { get; set; }
        public string Id { get; set; }
    }

    public class Productos
    {
        public List<VmProduct> Product { get; set; }
    }

    public class Productonuevo
    {
        public string Descripcion { get; set; }
        public string Cantidad { get; set; }
        public string Precio { get; set; }
        public string Precioref { get; set; }
        public string Subtotal { get; set; }
        public string Ref { get; set; }
    }
}
