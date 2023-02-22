using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfPos_Rios.ViewModels.helperResponse
{
    public class InvoicePaymentReportResponse
    {
        public List<ReportDataPayments> dataList { get; set; }

    }
    public class ReportDataPayments
    {
        public string Lote { get; set; }
        public string Descripcion { get; set; }
        public double Monto { get; set; }
    }
}
