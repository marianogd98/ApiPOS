using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfPos_Rios.ViewModels
{
    public enum TipoProducto
    {
        producto = 1,
        combo = 2
    }
    public class VmProduct : INotifyPropertyChanged
    {
        public Guid guid { get; set; }
        private int id;
        private string descripcion;
        private string code;
        private double unitPrice;
        private double unitPriceBs;
        private double quantity;
        private double totalRef;
        private double totalBs;
        private double discountPercentage;
        private string image;
        private string codeBard;
        public TipoProducto CodigoTipo { get; set; }
        private double iva;
        private string serial;
        private bool pesado;
        
        private double tasa;
        public VmProduct(double pTasa)
        {
            tasa = pTasa;
            guid = Guid.NewGuid();
        }

        public string CodeBard { get => codeBard; set{ codeBard = value; this.NotifyPropertyChanged("CodeBard"); } }
        public string Descripcion { get => descripcion; set { descripcion = value; this.NotifyPropertyChanged("Descripcion"); } }
        public string Code { get => code; set { code = value; this.NotifyPropertyChanged("Code"); } }
        public double Quantity { get => quantity; set { quantity = value; this.NotifyPropertyChanged("Quantity"); } }
        public double UnitPrice { get => unitPrice; set { unitPrice = value; this.NotifyPropertyChanged("UnitPrice"); } }
        public double DiscountPercentage { get => discountPercentage; set { discountPercentage = value; this.NotifyPropertyChanged("DiscountPercentage"); } }
        public string Image { get => image; set { image = value; this.NotifyPropertyChanged("Image"); } }
        public int Id { get => id; set { id = value; this.NotifyPropertyChanged("Id"); } }
        public double Iva { get => iva; set { iva = value; this.NotifyPropertyChanged("Iva"); } }
        public string Serial { get => serial; set { serial = value; this.NotifyPropertyChanged("Serial"); } }
        public bool Pesado { get => pesado; set {pesado = value; this.NotifyPropertyChanged("Pesado"); } }
        public double TotalBs 
        {
            get 
            {
                double amount = 0;
                if (DiscountPercentage > 0)
                {
                    amount = (unitPriceBs * Quantity) - ((unitPriceBs * Quantity) * DiscountPercentage);
                    totalBs = Math.Round(amount, 2);
                    return totalBs;
                }
                else
                {
                    amount = (unitPriceBs * Quantity);
                    totalBs = Math.Round(amount, 2);
                    return totalBs;
                }
            } 
        }
        public double UnitPriceBs { get => Math.Round(unitPriceBs,2); set => unitPriceBs = value; }
        public double TotalRef
        {
            get
            {
                double amount = 0;
                if (DiscountPercentage > 0)
                {
                    amount = (UnitPrice * Quantity) - ((UnitPrice * Quantity) * (DiscountPercentage));
                    totalRef = Round(amount);
                    return totalRef;
                }
                else
                {
                    amount = (UnitPrice * Quantity);
                    totalRef = Round(amount); 
                    return totalRef;
                }
            }
        }

        public double Round(double value)
        {
            return Math.Round((value * 100) - (Math.Round(value * 100, 0)), 2) == 0 ? value : Math.Ceiling(100 * value) / 100;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
      
    }
    
}
