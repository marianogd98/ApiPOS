using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peripherals.printerHKA80.Models
{
    public class StatusS3
    {
        public int TypeTax1 { get; set; }
        public double Tax1 { get; set; }
        public int TypeTax2 { get; set; }
        public double Tax2 { get; set; }
        public int TypeTax3 { get; set; }
        public double Tax3 { get; set; }
        public int[] AllSystemFlags { get; set; }
    }
}
