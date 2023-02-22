using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peripherals.printerHKA80.Models
{
     public class StatusPrinter
    {
        public int PrinterStatusCode { get; set;}
        public string PrinterStatusDescription { get; set; }
        public int PrinterErrorCode { get; set; }
        public string PrinterErrorDescription { get; set; }
        public bool ErrorValidity { get; set; }
    }
}
