using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peripherals.printerHKA80.Models
{
    public class StatusS2
    {
        public double SubTotalBases { get; set; }
        public double SubTotalTax { get; set; }
        public string DataDummy { get; set; }
        public double AmountPayable { get; set; }
        public int NumberPaymentsMade { get; set; }
        public int QuantityArticles { get; set; }
        public int TypeDocument { get; set; }
        public int Condition { get; set; }
    }
}
