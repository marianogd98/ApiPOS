using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peripherals.printerHKA80.Models
{
   public class StatusS5
    {
        public string RIF { get; set; }
        public string RegisteredMachineNumber { get; set; }
        public int AuditMemoryNumber { get; set; }
        public double AuditMemoryTotalCapacity { get; set; }
        public double AuditMemoryFreeCapacity { get; set; }
        public int NumberRegisteredDocuments { get; set; }
    }
}
