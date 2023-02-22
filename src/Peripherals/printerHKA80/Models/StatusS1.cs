using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peripherals.printerHKA80.Models
{
   public class StatusS1
    {
        public int CashierNumber { get; set; }
        public double TotalDailySales { get; set; }
        public int LastInvoiceNumber { get; set; }
        public int QuantityOfInvoicesToday { get; set; }
        public int LastDebitNoteNumber { get; set; }
        public int QuantityOfDebitNotesToday { get; set; }
        public int LastNonFiscalDocNumber { get; set; }
        public int QuantityNonFiscalDocuments { get; set; }
        public int DailyClosureCounter { get; set; }
        public int AuditReportsCounter { get; set; }
        public string RIF { get; set; }
        public string RegisteredMachineNumber { get; set; }
        public DateTime CurrentPrinterDateTime { get; set; }
        public int LastCreditNoteNumber { get; set; }
        public int QuantityOfCreditNotesToday { get; set; }


        public StatusS1()
        {

        }
    }

}
