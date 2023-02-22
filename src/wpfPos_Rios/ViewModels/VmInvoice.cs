using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfPos_Rios.ViewModels
{
    public class VmInvoice
    {
        public List<VmProduct> ProductList { get; set; }
        

        public int Id { get; set; }
        public string NumeroFactura { get; set; }
        private double subTotal;
        private double total;
        private double discount;
        private double tax;
        private double cancelled;
        private double retorno;
        public VmInvoice()
        {
            Id = -1;
            NumeroFactura = string.Empty;
            total = 0;
            discount = 0;
            subTotal = 0;
            retorno = 0;
            tax = 0;
            cancelled = 0;
            ProductList = new List<VmProduct>();
        }

        public void ResetInvoice()
        {
            Id = -1;
            NumeroFactura = string.Empty;
            total = 0;
            discount = 0;
            subTotal = 0;
            retorno = 0;
            tax = 0;
            cancelled = 0;
            if (ProductList.Count > 0)
                ProductList.Clear();
            ProductList = new List<VmProduct>();
        }

        //Metodo que suma los totales de los montos dentro de la lista productos al subtotal de la factura
        public double Total
        {
            set
            {
                total = value;
            }

            get
            {
                double amount = 0;
                if (ProductList.Count > 0)
                {
                    total = 0;
                    foreach (var item in ProductList)
                    {
                        amount += item.TotalBs;
                    }
                }
                else
                {
                    amount = 0;
                }

                if (discount > 0)
                {
                    amount = (amount) - (amount * discount);
                }
                total = amount;
                return total;
            }
        }
        public double TotalRef(double pTasa)
        {
            //primero redondea el total de bs a dos decimales
            double amount = Math.Round(Total / pTasa , 4);
            return Round(amount);
        }
     
        public double Round(double value)
        {
            return Math.Round( (value * 100) - ( Math.Round(value * 100, 0)), 2) == 0 ? value : Math.Ceiling(100 * value) / 100;
        }
        public double SubTotal
        {
            set
            {
                subTotal = value;
            }
            get
            {

                double amount = 0;
                if (ProductList.Count > 0)
                {
                    subTotal = 0;
                    foreach (var item in ProductList)
                    {
                        amount += item.Quantity * item.UnitPriceBs;
                    }
                }
                else
                {
                    amount = 0;
                }
                subTotal = amount;
                return subTotal;
            }
        }

        public double Discount
        {
            get
            {
                return discount;
            }
            set
            {
                discount = value;
            }
        }

        public double Tax
        {
            get
            {
                return tax;
            }
            set
            {
                tax = value;
            }
        }
        public double Cancelled
        {
            set
            {
                cancelled = value;
            }
            get
            {
                return cancelled;
            }
        }

        public double CancelledRef(double pTasa)
        {
            //primero redondea el total de bs a dos decimales
            double amount = Math.Round(Cancelled / pTasa, 4);
            return Round(amount);
        }

        public double Retorno
        {
            set
            {
                retorno = value;
            }
            get
            {
                return (cancelled > total) ? Round(cancelled - total) : 0;
            }
        }

        public double RetornoRef(double pTasa)
        {
            return (cancelled > total) ? Math.Round(( CancelledRef(pTasa) - TotalRef(pTasa)) , 2) : 0;
        }

        public double Restante
        {
            get
            {
                return (cancelled >= total) ? 0 : Math.Round(total - cancelled , 2);
            }
        }

        public double RestanteRef(double pTasa)
        {
            return (cancelled >= total) ? 0 : Math.Round( TotalRef(pTasa) - CancelledRef(pTasa), 2 ); 
        }
       

    }
}
